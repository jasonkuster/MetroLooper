using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Audio;
using System.IO;

namespace MetroLooper.AudioDoNotUse
{
    public class Recorder
    {
        private Microphone _microphone;
        private byte[] _buffer;
        private TimeSpan _duration;
        private int numBytes;
        private int totalNumBytes;
        private MemoryStream stream;
        private short[] _shortBuffer;

        /// <summary>
        /// True if Microphone is currently started/recording
        /// </summary>
        public bool isRecording
        {
            get
            {
                return (_microphone.State == MicrophoneState.Started);
            }
        }

        public Recorder()
        {
            //Microphone config
            _microphone = Microphone.Default;
            _microphone.BufferDuration = TimeSpan.FromMilliseconds(100);
            _duration = _microphone.BufferDuration;
            numBytes = _microphone.GetSampleSizeInBytes(_microphone.BufferDuration);
            TimeSpan sample = TimeSpan.FromSeconds(1.0 / _microphone.SampleRate);
            int numBytesPerSample = _microphone.GetSampleSizeInBytes(sample);
            _buffer = new byte[numBytes];
            _microphone.BufferReady += new EventHandler<EventArgs>(MicrophoneBufferReady);

            stream = new MemoryStream();

            totalNumBytes = 0;
        }

        public void StartRecording()
        {
            if (_microphone.State != MicrophoneState.Started)
            {
                _microphone.Start();
            }
        }
        //public void StopRecording(int bank, int track)
        public void StopRecording(out short[] outData, out int size)
        {
            _microphone.Stop();

            byte[] data = stream.ToArray();
            _shortBuffer = new short[totalNumBytes / 2];

            for (int i = 0; i < totalNumBytes; i += 2)
            {
                _shortBuffer[i / 2] = BitConverter.ToInt16(data, i);
            }

            System.Diagnostics.Debug.WriteLine("Recorded Time:" + (totalNumBytes / 2.0) / _microphone.SampleRate);

            outData = _shortBuffer;
            size = totalNumBytes / 2;

            stream = new MemoryStream();
            totalNumBytes = 0;
        }

        private void MicrophoneBufferReady(object sender, EventArgs e)
        {
            _microphone.GetData(_buffer);
            stream.Write(_buffer, 0, numBytes);
            totalNumBytes += numBytes;
        }
    }
}
