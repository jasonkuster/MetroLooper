using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Audio;
using AudioComponent;
using System.IO;

namespace MetroLooper
{
    public class Recorder
    {
        private Microphone _microphone;
        private byte[] _buffer;
        private TimeSpan _duration;
        private int numBytes;
        private int totalNumBytes;
        private MemoryStream stream;

        AudioEngine engine { get; set; }

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
            _buffer = new byte[numBytes];
            _microphone.BufferReady += new EventHandler<EventArgs>(MicrophoneBufferReady);

            stream = new MemoryStream();
        }

        public void Record()
        {
            if (_microphone.State != MicrophoneState.Started)
            {
                _microphone.Start();
            }
            else
            {
                _microphone.Stop();
                engine.PushData(stream.ToArray(), numBytes);
            }
        }

        private void MicrophoneBufferReady(object sender, EventArgs e)
        {
            _microphone.GetData(_buffer);
            stream.Write(_buffer, 0, numBytes);
            totalNumBytes += numBytes;
            //System.Diagnostics.Debug.WriteLine("buffer ready");
            //Make voice here
        }
    }
}
