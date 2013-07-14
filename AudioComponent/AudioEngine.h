#pragma once

namespace AudioComponent
{
#define SAMPLE_RATE (16000)
#define MAX_TRACKS 10
#define MAX_BANKS 6
#define MAX_OFFSET 200*(SAMPLE_RATE/1000)

	[Windows::Foundation::Metadata::WebHostHidden]
	public interface class ICallback
	{
	public:
		virtual void BufferFinished(int bufferContext);
		virtual void PlaybackStarted();
		virtual void PrintValue(int value);
	};

	public ref class AudioEngine sealed
	{
	private:
		static const int RECORDING_SECONDS = 10;
		static const int BUFFER_LENGTH = SAMPLE_RATE  * RECORDING_SECONDS;
		interface IXAudio2*  pXAudio2;
		IXAudio2MasteringVoice * pMasteringVoice;
		IXAudio2SourceVoice *voices[MAX_BANKS][MAX_TRACKS];
		IXAudio2SourceVoice *clickVoice;

		int buffer_sizes[MAX_BANKS][MAX_TRACKS];
		int offsets[MAX_BANKS][MAX_TRACKS];
		int beatsPerMinute;

		short audioData[MAX_BANKS][MAX_TRACKS][BUFFER_LENGTH];
		short clickData[SAMPLE_RATE];

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

	public:
		AudioEngine();

		static void BufferFinished(int bufferContext);
		static void BufferStarted(int bufferContext);
		static void PrintValue(int value);

		Platform::Array<short>^ GetAudioData(int bank, int track);
		int GetAudioDataSize(int bank, int track);

		void Suspend();
		void Resume();

		void PlaySound();
		void PlayTrack(int bank, int track);
		void StopSound();
		void ReadPerformanceData();

		void PlayClickTrack();
		void StopClickTrack();
		bool IsClickPlaying() {return isClickPlaying;}
		int GetBPM() {return beatsPerMinute;}

		void SetCallback( ICallback ^Callback);

		void PushData(const Platform::Array<short>^ data, int size, int bank, int track);
		void SetBPM(int bpm);

		void SetOffset(int offset_ms, int bank, int track)
		{
			int offset_samples = offset_ms*(SAMPLE_RATE/1000);
			offsets[bank][track] = offset_samples;
		}
		int GetOffsetMS(int bank, int track) { return offsets[bank][track]/(SAMPLE_RATE/1000); }
	};
}
