using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.IsolatedStorage;
using Microsoft.Live;
using MetroLooper.ViewModels;

namespace MetroLooper
{
    class FileHelper
    {
        /// <summary>
        /// Write WAV File (to isolated storage)
        /// </summary>
        /// <param name="data">Audio data in Byte array</param>
        /// <param name="sampleRate">Sample rate</param>
        /// <param name="fileName">File name</param>
        public static void WriteWAVFile(Byte[] data, int sampleRate, string fileName)
        {
            MemoryStream outputStream = new MemoryStream();
            outputStream.SetLength(0);

            WriteWAVHeader(outputStream, sampleRate);
            outputStream.Write(data, 0, data.Length);
            UpdateWAVHeader(outputStream);

            WriteToIsolatedStorage(outputStream, fileName);
        }

        public static async void UploadToSkydrive(string fileName, MainViewModel viewModel)
        {
            try
            {
                LiveOperationResult clientResult = await viewModel.Client.GetAsync("me/skydrive");
                dynamic res = clientResult.Result;
                string path = res.id;
                {
                    await viewModel.Client.BackgroundUploadAsync(path, new Uri("/shared/transfers/" + fileName, UriKind.RelativeOrAbsolute), OverwriteOption.Overwrite);
                }
            }
            catch (System.Threading.Tasks.TaskCanceledException tce)
            {
            }
            catch (LiveConnectException ce)
            {
            }
            catch (Exception e)
            {
            }
        }

        /// <summary>
        /// Read WAV file from Isolated Storage
        /// </summary>
        /// <param name="fileName">Filename</param>
        /// <returns>Short array with audio data</returns>
        public static short[] ReadWAVFileFromIsolatedStorage(string fileName)
        {
            MemoryStream stream = ReadFromIsolatedStorage(fileName);
            return ReadWAVFile(stream);
        }

        /// <summary>
        /// Extract WAV data from Memory Stream (assumes mono, 16-bit signed PCM for now)
        /// </summary>
        /// <param name="stream">MemoryStream</param>
        /// <returns>Short array with all data</returns>
        public static short[] ReadWAVFile(MemoryStream stream)
        {
            if (!stream.CanSeek) throw new Exception("Can't seek stream to read wav header");

            stream.Seek(40, SeekOrigin.Begin);
            Byte[] lengthData = new Byte[4];
            int numBytesRead = stream.Read(lengthData, 0, 4);
            if (numBytesRead != 4) throw new Exception("Stream ended while reading length value");
            int lengthBytes = BitConverter.ToInt32(lengthData, 0);

            System.Diagnostics.Debug.WriteLine("Bytes read: " + lengthBytes);

            int dataStartByte = 44;
            stream.Seek(dataStartByte, SeekOrigin.Begin);
            Byte[] audioData = new Byte[lengthBytes];
            numBytesRead = stream.Read(audioData, 0, lengthBytes);

            if (numBytesRead != lengthBytes) throw new Exception("Could not read " + lengthBytes + " bytes, only read " + numBytesRead + " bytes.");

            short[] shortData = new short[lengthBytes / 2];
            for (int i = 0; i < lengthBytes; i += 2)
            {
                shortData[i / 2] = BitConverter.ToInt16(audioData, i);
            }

            return shortData;
        }

        /// <summary>
        /// Write to Isloated Storage
        /// </summary>
        /// <param name="stream">Memory Stream</param>
        /// <param name="fileName">File name</param>
        private static void WriteToIsolatedStorage(MemoryStream stream, string fileName)
        {
            // first, we grab the current apps isolated storage handle
            IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication();

            // we give our file a filename
            string strSaveName = fileName;

            // if that file exists... 
            if (isf.FileExists(strSaveName))
            {
                // then delete it
                isf.DeleteFile(strSaveName);
            }

            // now we set up an isolated storage stream to point to store our data
            IsolatedStorageFileStream isfStream =
                     new IsolatedStorageFileStream(strSaveName,
                     FileMode.Create, IsolatedStorageFile.GetUserStoreForApplication());

            isfStream.Write(stream.ToArray(), 0, stream.ToArray().Length);

            // ok, done with isolated storage... so close it
            isfStream.Close();
        }

        /// <summary>
        /// Read Isolated Storage file to memory
        /// </summary>
        /// <param name="fileName">filename</param>
        /// <returns>MemoryStream</returns>
        private static MemoryStream ReadFromIsolatedStorage(string fileName)
        {
            MemoryStream stream = new MemoryStream();

            IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication();

            if (isoStore.FileExists(fileName))
            {
                Console.WriteLine(fileName + " file read success.");
                using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream(fileName, FileMode.Open, isoStore))
                {
                    isoStream.CopyTo(stream);
                }
            }
            else
            {
                throw new Exception("Could not read from file");
            }

            return stream;
        }

        private static void UpdateWAVHeader(Stream stream)
        {
            // From: http://damianblog.com/2011/02/07/storing-wp7-recorded-audio-as-wav-format-streams/
            if (!stream.CanSeek) throw new Exception("Can't seek stream to update wav header");

            var oldPos = stream.Position;

            // ChunkSize 36 + SubChunk2Size
            stream.Seek(4, SeekOrigin.Begin);
            stream.Write(BitConverter.GetBytes((int)stream.Length - 8), 0, 4);

            // Subchunk2Size == NumSamples * NumChannels * BitsPerSample/8 This is the number of bytes in the data.
            stream.Seek(40, SeekOrigin.Begin);
            stream.Write(BitConverter.GetBytes((int)stream.Length - 44), 0, 4);

            stream.Seek(oldPos, SeekOrigin.Begin);
        }

        private static void WriteWAVHeader(Stream stream, int sampleRate)
        {
            // From: http://damianblog.com/2011/02/07/storing-wp7-recorded-audio-as-wav-format-streams/
            const int bitsPerSample = 16;
            const int bytesPerSample = bitsPerSample / 8;
            var encoding = System.Text.Encoding.UTF8;

            // ChunkID Contains the letters "RIFF" in ASCII form (0x52494646 big-endian form).
            stream.Write(encoding.GetBytes("RIFF"), 0, 4);

            // NOTE this will be filled in later
            stream.Write(BitConverter.GetBytes(0), 0, 4);

            // Format Contains the letters "WAVE"(0x57415645 big-endian form).
            stream.Write(encoding.GetBytes("WAVE"), 0, 4);

            // Subchunk1ID Contains the letters "fmt " (0x666d7420 big-endian form).
            stream.Write(encoding.GetBytes("fmt "), 0, 4);

            // Subchunk1Size 16 for PCM. This is the size of therest of the Subchunk which follows this number.
            stream.Write(BitConverter.GetBytes(16), 0, 4);

            // AudioFormat PCM = 1 (i.e. Linear quantization) Values other than 1 indicate some form of compression.
            stream.Write(BitConverter.GetBytes((short)1), 0, 2);

            // NumChannels Mono = 1, Stereo = 2, etc.
            stream.Write(BitConverter.GetBytes((short)1), 0, 2);

            // SampleRate 8000, 44100, etc.
            stream.Write(BitConverter.GetBytes(sampleRate), 0, 4);

            // ByteRate = SampleRate * NumChannels * BitsPerSample/8
            stream.Write(BitConverter.GetBytes(sampleRate * bytesPerSample), 0, 4);

            // BlockAlign NumChannels * BitsPerSample/8 The number of bytes for one sample including all channels.
            stream.Write(BitConverter.GetBytes((short)(bytesPerSample)), 0, 2);

            // BitsPerSample 8 bits = 8, 16 bits = 16, etc.
            stream.Write(BitConverter.GetBytes((short)(bitsPerSample)), 0, 2);

            // Subchunk2ID Contains the letters "data" (0x64617461 big-endian form).
            stream.Write(encoding.GetBytes("data"), 0, 4);

            // NOTE to be filled in later
            stream.Write(BitConverter.GetBytes(0), 0, 4);
        }

        /// <summary>
        /// Load a project file into memory
        /// </summary>
        /// <param name="relativeFilePath">Relative file path</param>
        /// <returns>MemoryStream with file data</returns>
        public static MemoryStream ReadProjectFile(string relativeFilePath)
        {
            FileStream fileStream = File.OpenRead(relativeFilePath);
            MemoryStream stream = new MemoryStream();
            fileStream.CopyTo(stream);

            fileStream.Close();
            return stream;
        }
    }
}