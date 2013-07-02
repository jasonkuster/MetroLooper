using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioComponent;
using MetroLooper.AudioDoNotUse;

namespace MetroLooper
{
    class AudioManager : ICallback
    {
        private Recorder _recorder;
        public AudioEngine _engine;
        public bool isPlaying;
        public bool isRecording;

        /// <summary>
        /// Default constructor, does all initialization
        /// </summary>
        public AudioManager()
        {
            _recorder = new Recorder();

            _engine = new AudioEngine();
            _recorder.engine = _engine;

            isPlaying = false;

            _engine.SetCallback(this);
        }

        /// <summary>
        /// Start Recording Track
        /// </summary>
        public void RecordStart()
        {
            _recorder.StartRecording();
            isRecording = true;
        }

        /// <summary>
        /// Stop Recording track
        /// </summary>
        /// <param name="bank">Bank to submit to</param>
        /// <param name="track">Track to submit to</param>
        public void RecordStopAndSubmit(int bank, int track)
        {
            _recorder.StopRecording(bank, track);
            isRecording = false;
        }

        /// <summary>
        /// Play all banks/tracks
        /// </summary>
        public void PlayAll()
        {
            _engine.PlaySound();
            isPlaying = true;
        }

        /// <summary>
        /// Stop all sounds
        /// </summary>
        public void StopAll()
        {
            _engine.StopSound();
            isPlaying = false;
        }

        /// <summary>
        /// Play track
        /// </summary>
        /// <param name="bank">bank to play</param>
        /// <param name="track">track to play</param>
        public void PlayTrack(int bank, int track)
        {
            int returnedSize = _engine.PlayTrack(bank, track);
            if (returnedSize == -1)
            {
                //Engine not initialized
                throw new Exception("Audio Engine not yet initialized, could not play track");
            }
            else if (returnedSize == 0)
            {
                //track not set yet
                throw new Exception("Track has no data yet");
            }

            isPlaying = true;
        }


        public void Exec(int bufferContext)
        {
            System.Diagnostics.Debug.WriteLine("Callback called:" + (bufferContext).ToString());
        }
    }
}
