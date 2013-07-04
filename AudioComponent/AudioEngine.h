#pragma once

namespace AudioComponent
{
#define SAMPLE_RATE (16000)
#define MAX_TRACKS 10
#define MAX_BANKS 6

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
		int buffer_sizes[MAX_BANKS][MAX_TRACKS];

		short audioData[MAX_BANKS][MAX_TRACKS][BUFFER_LENGTH];
		XAUDIO2_BUFFER buffer;
		XAUDIO2_BUFFER buffer2;
		bool initialized;

		static int numBuffersPlaying;

		void Initialize();
		void ThrowIfFailed(HRESULT);

		struct ImplData;

	public:
		AudioEngine();

		static void BufferFinished(int bufferContext);
		static void BufferStarted(int bufferContext);

		void Suspend();
		void Resume();

		void PlaySound();
		void StopSound();
		void ReadPerformanceData();

		int PlayTrack(int bank, int track);

		void SetCallback( ICallback ^Callback);

		void PushData(const Platform::Array<short>^ data, int size, int bank, int track);
	};
}
