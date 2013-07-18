#pragma once

namespace AudioComponent
{
#define SAMPLE_RATE (16000)
#define MAX_TRACKS 10
#define MAX_BANKS 6
#define MAX_OFFSET 1000*(SAMPLE_RATE/1000)
#define LATENCY 0*(SAMPLE_RATE/1000)

	[Windows::Foundation::Metadata::WebHostHidden]
	public interface class ICallback
	{
	public:
		virtual void BufferFinished(int bufferContext);
		virtual void PrintValue(double value);
		virtual void PrintLatencyValue(int value);
		virtual void PrintBankTrack(int bank, int track);
	};

	public ref class AudioEngine sealed
	{
	private:
		static const int RECORDING_SECONDS = 20;
		static const int BUFFER_LENGTH = SAMPLE_RATE  * RECORDING_SECONDS;
		interface IXAudio2*  pXAudio2;
		IXAudio2MasteringVoice * pMasteringVoice;
		IXAudio2SourceVoice *voices[MAX_BANKS][MAX_TRACKS];
		IXAudio2SourceVoice *bankVoices[MAX_BANKS];
		IXAudio2SourceVoice *clickVoice;

		int buffer_sizes[MAX_BANKS][MAX_TRACKS];
		int bank_sizes[MAX_BANKS];
		int offsets[MAX_BANKS][MAX_TRACKS];
		int bank_offsets[MAX_BANKS];
		int latency_offsets[MAX_BANKS][MAX_TRACKS];
		int beatsPerMinute;
		int currentLatency;

		short audioData[MAX_BANKS][MAX_TRACKS][BUFFER_LENGTH];
		short bankAudioData[MAX_BANKS][BUFFER_LENGTH];
		bool bankFinalized[MAX_BANKS];
		short clickData[SAMPLE_RATE];

		Platform::Array<short>^ pulledData;

		XAUDIO2_BUFFER buffer2;
		bool initialized;
		bool isClickPlaying;

		static int numBuffersPlaying;

		void Initialize();
		void ThrowIfFailed(HRESULT);

		int GetLatency()
		{
			XAUDIO2_PERFORMANCE_DATA perfData;
			pXAudio2->GetPerformanceData(&perfData);
			return perfData.CurrentLatencyInSamples;
		}

		struct ImplData;
		int microphoneLatency;

	public:
		AudioEngine();

		static void BufferFinished(int bufferContext);
		static void BufferStarted(int bufferContext);
		static void PrintValue(double value);

		Platform::Array<short>^ GetAudioData(int bank, int track);
		Platform::Array<short>^ GetBankAudioData(int bank);
		int GetAudioDataSize(int bank, int track);
		int GetBankSize(int bank)
		{
			return bank_sizes[bank];
		}

		void setMicrophoneLatencyMS(double value)
		{
			microphoneLatency = (int)(value*(SAMPLE_RATE/1000));
		}

		void Suspend();
		void Resume();

		void PlaySound();
		void PlayFullBank(int bank);
		void PlayTrack(int bank, int track);
		void PlayBank(int bank);
		void StopSound();
		void ReadPerformanceData();

		void SetVolume(int bank, int track, double volume_db);
		void SetPitch(int bank, int track, double pitch);
			
		void PlayClickTrack();
		void StopClickTrack();
		bool IsClickPlaying() {return isClickPlaying;}
		int GetBPM() {return beatsPerMinute;}

		void MixDownBank(int bank);

		void SetCallback( ICallback ^Callback);

		void PushData(const Platform::Array<short>^ data, int size, int bank, int track);
		void SetBPM(int bpm);

		void SetOffset(int offset_ms, int bank, int track)
		{
			if (offset_ms > MAX_OFFSET)
				offset_ms = MAX_OFFSET;
			else if (offset_ms < -MAX_OFFSET)
				offset_ms = -MAX_OFFSET;

			int offset_samples = offset_ms*(SAMPLE_RATE/1000);
			offsets[bank][track] = offset_samples;
		}
		int GetOffsetMS(int bank, int track) { return offsets[bank][track]/(SAMPLE_RATE/1000); }

		void SetBankOffset(int offset_ms, int bank)
		{
			if (offset_ms > MAX_OFFSET)
				offset_ms = MAX_OFFSET;
			else if (offset_ms < -MAX_OFFSET)
				offset_ms = -MAX_OFFSET;

			int offset_samples = offset_ms*(SAMPLE_RATE/1000);
			bank_offsets[bank] = offset_samples;
		}
		int GetBankOffsetMS(int bank) { return bank_offsets[bank]/(SAMPLE_RATE/1000); }
	};
}
