#pragma once

namespace AudioComponent
{
#define SAMPLE_RATE (44100)

	public ref class AudioEngine sealed
	{
	private:
		static const int BASE_FREQ = 20;
		static const int BUFFER_LENGTH = SAMPLE_RATE  / BASE_FREQ;
		interface IXAudio2*  pXAudio2;
		IXAudio2MasteringVoice * pMasteringVoice;
		IXAudio2SourceVoice * pVoice;
		short soundData[BUFFER_LENGTH];
		XAUDIO2_BUFFER buffer;
		bool initialized;

		void Initialize();
		void ThrowIfFailed(HRESULT);

		struct ImplData;
	public:
		AudioEngine();


		void Suspend();
		void Resume();

		void PlaySound();
		void StopSound();

		void PushData(const Platform::Array<byte>^ data, int size);

		void BufferEnded();
	};
}
