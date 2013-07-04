using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioComponent;
using MetroLooper.AudioDoNotUse;
using Microsoft.Xna.Framework.Audio;

namespace MetroLooper
{
    class AudioManager : ICallback
    {
        private Recorder _recorder;
        private AudioEngine _engine;

        private bool recordOnPlaybackCallback;

        public bool isPlaying;
        public bool isRecording;

        /// <summary>
        /// Default constructor, does all initialization
        /// </summary>
        public AudioManager()
        {
            _recorder = new Recorder();
            _engine = new AudioEngine();

            isPlaying = false;
            this.recordOnPlaybackCallback = false;

            _engine.SetCallback(this);
        }

        /// <summary>
        /// Start Recording Track
        /// </summary>
        public void RecordStart()
        {
            this._recorder.StartRecording();
            this._engine.PlaySound();
            this.isRecording = true;
        }

        public void RecordAndPlay()
        {
            this.recordOnPlaybackCallback = true;
            _engine.PlaySound();
            this.isPlaying = true;
            this.isRecording = true;
        }

        /// <summary>
        /// Stop Recording track
        /// </summary>
        /// <param name="bank">Bank to submit to</param>
        /// <param name="track">Track to submit to</param>
        public void RecordStopAndSubmit(int bank, int track)
        {
            short[] data;
            int size;

            _recorder.StopRecording(out data, out size);
            _engine.PushData(data, size, bank, track);

            isRecording = false;
            this.recordOnPlaybackCallback = false;
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

        public void GetPerf()
        {
            _engine.ReadPerformanceData();
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
                throw new Exception("Audio Engine not yet initialized, could not play track.");
            }
            else if (returnedSize == 0)
            {
                //track not set yet
                throw new Exception("Track:" + track + " in bank:" + bank + " has no data yet.");
            }

            isPlaying = true;
        }

        /// <summary>
        /// Callback method when a buffer (track) finishes
        /// </summary>
        /// <param name="bufferContext">Buffer context</param>
        public void BufferFinished(int bufferContext)
        {
            System.Diagnostics.Debug.WriteLine("Callback called:" + (bufferContext).ToString());
        }

        /// <summary>
        /// Callback method for playback starting
        /// </summary>
        public void PlaybackStarted()
        {
            if (this.recordOnPlaybackCallback)
            {
                _recorder.StartRecording();
                System.Diagnostics.Debug.WriteLine("Playback started, recording now");
                this.isRecording = true;
            }
        }

        public void PrintValue(int value)
        {
            double millis = (value / 16000.0) * 1000;
            System.Diagnostics.Debug.WriteLine("Latency in samples:" + value + ", Milliseconds:"+millis);
        }
    }
}
