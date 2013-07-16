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
            _engine.SetCallback(this); //Do this before anything else!

            isPlaying = false;
            this.recordOnPlaybackCallback = false;

            _engine.SetBPM(120);
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
            _engine.PlayClickTrack();
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
            _engine.StopClickTrack();
            _engine.StopSound();

            _recorder.StopRecording(out data, out size);
            _engine.PushData(data, size, bank, track);

            isRecording = false;
            isPlaying = false;
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

        /// <summary>
        /// Gets and prints out Performance Data (latency)
        /// </summary>
        public void GetPerf()
        {
            _engine.ReadPerformanceData();
            //_engine.PlayClickTrack();
            //_engine.PlayTrack(0, 2);
        }

        /// <summary>
        /// Start playing click track
        /// </summary>
        public void PlayClick()
        {
            if (!_engine.IsClickPlaying())
            {
                _engine.PlayClickTrack();
            }
        }

        /// <summary>
        /// Stop click track
        /// </summary>
        public void StopClick()
        {
            if (_engine.IsClickPlaying())
            {
                _engine.StopClickTrack();
            }
        }

        /// <summary>
        /// Sets engine BPM if click track is not playing
        /// </summary>
        /// <param name="bpm">Beats Per Minute</param>
        public void SetBPM(int bpm)
        {
            if (!_engine.IsClickPlaying())
            {
                if (bpm > 180) bpm = 180;
                else if (bpm < 60) bpm = 60;
                _engine.SetBPM(bpm);
            }
        }

        /// <summary>
        /// Gets engine BPM
        /// </summary>
        /// <returns>Current BPM of click track</returns>
        public int GetBPM()
        {
            return _engine.GetBPM();
        }

        /// <summary>
        /// Play track
        /// </summary>
        /// <param name="bank">bank to play</param>
        /// <param name="track">track to play</param>
        public void PlayTrack(int bank, int track)
        {
            _engine.PlayTrack(bank, track);

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
            if (this.recordOnPlaybackCallback && !_recorder.isRecording)
            {
                _recorder.StartRecording();
                System.Diagnostics.Debug.WriteLine("Playback started, recording now");
                this.isRecording = true;
            }
        }

        /// <summary>
        /// Print Value
        /// </summary>
        /// <param name="value">Value to print</param>
        public void PrintValue(int value)
        {
            System.Diagnostics.Debug.WriteLine("Value reported:" + value);
        }

        /// <summary>
        /// Print Latency Value
        /// </summary>
        /// <param name="value">Latency in Samples</param>
        public void PrintLatencyValue(int value)
        {
            double millis = (value / 16000.0) * 1000;
            System.Diagnostics.Debug.WriteLine("Latency in samples:" + value + ", Milliseconds:" + millis);
        }

        /// <summary>
        /// Retrieves Audio Data from engine
        /// </summary>
        /// <param name="bank">Bank number</param>
        /// <param name="track">Track number</param>
        /// <param name="audioData">array of audioData to be filled</param>
        /// <returns>Number of samples returned</returns>
        public int GetAudioData(int bank, int track, out short[] audioData)
        {
            audioData = _engine.GetAudioData(bank, track);
            return _engine.GetAudioDataSize(bank, track);
        }

        /// <summary>
        /// Mix down bank
        /// </summary>
        /// <param name="bank">bank number</param>
        public void MixDownBank(int bank)
        {
            _engine.MixDownBank(bank);
        }
    }
}
