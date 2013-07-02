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
		virtual void Exec(int bufferContext);
	};

	public ref class AudioEngine sealed
	{
	private:
		static const int RECORDING_SECONDS = 10;
		static const int BUFFER_LENGTH = SAMPLE_RATE  * RECORDING_SECONDS;
		interface IXAudio2*  pXAudio2;
		IXAudio2MasteringVoice * pMasteringVoice;
		IXAudio2SourceVoice * pVoice;
		IXAudio2SourceVoice * pVoice2;

		IXAudio2SourceVoice *voices[MAX_BANKS][MAX_TRACKS];
		int buffer_sizes[MAX_BANKS][MAX_TRACKS];

		short soundData[BUFFER_LENGTH];
		short audioData[MAX_BANKS][MAX_TRACKS][BUFFER_LENGTH];
		short d[BUFFER_LENGTH];
		XAUDIO2_BUFFER buffer;
		XAUDIO2_BUFFER buffer2;
		bool initialized;

		void Initialize();
		void ThrowIfFailed(HRESULT);

		struct ImplData;

	public:
		AudioEngine();

		static void BufferFinished(int bufferContext);

		void Suspend();
		void Resume();

		void PlaySound();
		void StopSound();
		int PlayTrack(int bank, int track);

		void SetCallback( ICallback ^Callback);

		void PushData(const Platform::Array<short>^ data, int size, int bank, int track);

		void BufferEnded();
	};
}
