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
    public class AudioManager : ICallback
    {
        private Recorder _recorder;
        private AudioEngine _engine;

        /// <summary>
        /// Is Playing
        /// </summary>
        public bool isPlaying;

        /// <summary>
        /// Is Recording
        /// </summary>
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

            _engine.SetBPM(120);
        }

        /// <summary>
        /// Start Recording Track
        /// </summary>
        public void RecordStart()
        {
            this._recorder.StartRecording();
            this.isRecording = true;
        }

        /// <summary>
        /// Record while Playing bank
        /// </summary>
        /// <param name="bank">Bank number</param>
        public void RecordAndPlay(int bank)
        {
            _engine.PlayBank(bank);
            _recorder.StartRecording();
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
            _engine.StopSound();

            _recorder.StopRecording(out data, out size);
            _engine.setMicrophoneLatencyMS(_recorder.latency_ms);
            _engine.PushData(data, size, bank, track);

            isRecording = false;
            isPlaying = false;
        }

        /// <summary>
        /// Record Stop, no publishing. Use in bad cases.
        /// </summary>
        public void RecordStop()
        {
            short[] dataDump;
            int sizeDump;

            _engine.StopSound();
            _recorder.StopRecording(out dataDump, out sizeDump);
            isRecording = false;
            isPlaying = false;
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
        }

        /// <summary>
        /// Set Track Volume
        /// </summary>
        /// <param name="bank">Bank Number</param>
        /// <param name="track">Track Number</param>
        /// <param name="volume_db">Volume in dB (20log10(gain))</param>
        public void SetVolume(int bank, int track, double volume_db)
        {
            _engine.SetVolumeDB(bank, track, volume_db);
        }

        /// <summary>
        /// Set Bank Volume
        /// </summary>
        /// <param name="bank">Bank</param>
        /// <param name="volume_db">Volume in dB</param>
        public void SetBankVolume(int bank, double volume_db)
        {
            _engine.SetBankVolumeDB(bank, volume_db);
        }

        /// <summary>
        /// Set bank Pitch in Semitones
        /// </summary>
        /// <param name="bank">Bank</param>
        /// <param name="pitchRatio">Pitch in Semitones</param>
        public void SetPitchSemitones(int bank, double pitchSemitones)
        {
            _engine.SetBankPitch(bank, pitchSemitones);
        }

        /// <summary>
        /// Set Offset in milliseconds
        /// </summary>
        /// <param name="bank">bank</param>
        /// <param name="track">track</param>
        /// <param name="offset_ms">offset in ms</param>
        public void SetOffsetMS(int bank, int track, double offset_ms)
        {
            _engine.SetOffset((int)offset_ms, bank, track);
        }

        /// <summary>
        /// Get Track Offset in milliseconds
        /// </summary>
        /// <param name="bank">Bank</param>
        /// <param name="track">Track</param>
        /// <returns>Offset in milliseconds</returns>
        public int GetOffsetMS(int bank, int track)
        {
            return _engine.GetOffsetMS(bank, track);
        }

        /// <summary>
        /// Set Bank Offset in milliseconds
        /// </summary>
        /// <param name="bank">bank</param>
        /// <param name="offset_ms">offset in ms</param>
        public void SetBankOffsetMS(int bank, double offset_ms)
        {
            _engine.SetBankOffset((int)offset_ms, bank);
        }

        /// <summary>
        /// Get Bank Offset in milliseconds
        /// </summary>
        /// <param name="bank">Bank</param>
        /// <returns>Offset in ms</returns>
        public int GetBankOffsetMS(int bank)
        {
            return _engine.GetBankOffsetMS(bank);
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
        /// Play audio bank 
        /// </summary>
        /// <param name="bank">Bank number</param>
        public void PlayBank(int bank)
        {
            _engine.PlayBank(bank);
            isPlaying = true;
        }

        /// <summary>
        /// Callback method when a buffer (track) finishes
        /// </summary>
        /// <param name="bufferContext">Buffer context</param>
        public void BufferFinished(int bufferContext)
        {
            //System.Diagnostics.Debug.WriteLine("Callback called:" + (bufferContext).ToString());
        }

        /// <summary>
        /// Print Value
        /// </summary>
        /// <param name="value">Value to print</param>
        public void PrintValue(double value)
        {
            System.Diagnostics.Debug.WriteLine("Value reported:" + value);
        }

        /// <summary>
        /// Print Bank and Track Numbers
        /// </summary>
        /// <param name="bank">Bank</param>
        /// <param name="track">Track</param>
        public void PrintBankTrack(int bank, int track)
        {
            System.Diagnostics.Debug.WriteLine("Bank:" + bank + ", Track:" + track);
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
        public int GetAudioData(int bank, int track, out byte[] audioDataInBytes)
        {
            short[] shortAudioData = _engine.GetAudioData(bank, track);
            audioDataInBytes = new byte[shortAudioData.Length * sizeof(short)];
            Buffer.BlockCopy(shortAudioData, 0, audioDataInBytes, 0, audioDataInBytes.Length);

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

        /// <summary>
        /// Get Click Volume
        /// </summary>
        /// <returns>Volume (0 to 1)</returns>
        public float GetClickVolume()
        {
            return _engine.GetClickVolume();
        }

        /// <summary>
        /// Set Click Volume
        /// </summary>
        /// <param name="gain">Volume (0 to 1)</param>
        public void SetClickVolume(float gain)
        {
            _engine.SetClickVolume(gain);
        }

        /// <summary>
        /// Load Track Data
        /// </summary>
        /// <param name="bank">Bank</param>
        /// <param name="track">Track</param>
        /// <param name="data">Byte array of data</param>
        /// <param name="size">Size in samples</param>
        /// <param name="offset_ms">Offset in ms</param>
        /// <param name="latency_samples">Latency in samples</param>
        /// <param name="volume">Volume in DB</param>
        public void LoadTrack(int bank, int track, byte[] data, int size, int offset_ms, int latency_samples, double volumeDB)
        {
            short[] audioData = new short[size];
            for (int i = 0; i < size*2; i += 2)
            {
                audioData[i / 2] = BitConverter.ToInt16(data, i);
            }

            _engine.LoadTrack(bank, track, audioData, size, offset_ms, latency_samples, volumeDB);
        }

        /// <summary>
        /// Load Bank Data
        /// </summary>
        /// <param name="bank">Bank</param>
        /// <param name="data">Byte array of data</param>
        /// <param name="size">Size ins amples</param>
        /// <param name="offset_ms">Offset in ms</param>
        /// <param name="volumeDB">Volume in DB</param>
        /// <param name="pitch">Pitch Value (Semitones)</param>
        public void LoadBank(int bank, byte[] data, int size, int offset_ms, double volumeDB, double pitch)
        {
            short[] audioData = new short[size];
            for (int i = 0; i < size * 2; i += 2)
            {
                audioData[i / 2] = BitConverter.ToInt16(data, i);
            }

            _engine.LoadBank(bank, audioData, size, offset_ms, volumeDB, pitch);
        }
    }
}
