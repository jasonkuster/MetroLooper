// AudioEngine.cpp
#include "pch.h"
#include "AudioEngine.h"
#include <windows.h>

using namespace AudioComponent;
using namespace Platform;

class VoiceCallback : public IXAudio2VoiceCallback
{
public:
	HANDLE hBufferEndEvent;
	VoiceCallback(): hBufferEndEvent( CreateEventEx( NULL, FALSE, FALSE, NULL ) ){}
	~VoiceCallback(){ CloseHandle( hBufferEndEvent ); }

	//Called when the voice has just finished playing a contiguous audio stream.
	void __stdcall VoiceCallback::OnStreamEnd() { SetEvent( hBufferEndEvent ); OutputDebugString(L"stream end"); }

	//Unused methods are stubs
	void __stdcall OnVoiceProcessingPassEnd() { }
	void __stdcall OnVoiceProcessingPassStart(UINT32 SamplesRequired) {    }
	void __stdcall OnBufferEnd(void * pBufferContext)    { OutputDebugString(L"buffer end"); }
	void __stdcall OnBufferStart(void * pBufferContext) {    }
	void __stdcall OnLoopEnd(void * pBufferContext) {    }
	void __stdcall OnVoiceError(void * pBufferContext, HRESULT Error) { }
};

AudioEngine::AudioEngine()
{
	initialized = false;
}

void AudioEngine::BufferEnded()
{
	OutputDebugString(L"Buffer Ended");
}
void AudioEngine::Initialize()
{
	if (!initialized)
	{
		// Create an IXAudio2 object
		ThrowIfFailed(XAudio2Create(&pXAudio2));

		// Create a mastering voice
		ThrowIfFailed(pXAudio2->CreateMasteringVoice(&pMasteringVoice));

		// Create a source voice 
		WAVEFORMATEX waveformat;
		waveformat.wFormatTag = WAVE_FORMAT_PCM;
		waveformat.nChannels = 1;
		waveformat.nSamplesPerSec = SAMPLE_RATE;
		waveformat.nAvgBytesPerSec = SAMPLE_RATE * 2;
		waveformat.nBlockAlign = 2;
		waveformat.wBitsPerSample = 16;
		waveformat.cbSize = 0;

		VoiceCallback *callback = new VoiceCallback();
		ThrowIfFailed(pXAudio2->CreateSourceVoice(&pVoice, &waveformat, 0, XAUDIO2_MAX_FREQ_RATIO, callback));

		// Submit the array
		buffer.AudioBytes = 2 * BUFFER_LENGTH;
		buffer.pAudioData = (byte *)soundData;
		//buffer.Flags = XAUDIO2_END_OF_STREAM;
		buffer.PlayBegin = 0;
		buffer.PlayLength = BUFFER_LENGTH;
		buffer.LoopBegin = 0;
		buffer.LoopLength = BUFFER_LENGTH;
		buffer.LoopCount = XAUDIO2_LOOP_INFINITE;

		pVoice->SetFrequencyRatio(1);
		pVoice->SetVolume(0);
		ThrowIfFailed(pVoice->SubmitSourceBuffer(&buffer));

		// The sound will play in a continuous loop, but the volume is currently set at 0. 
		// Playing the sound in this samples refers to increasing the volume so that the sound is audible.
		ThrowIfFailed(pVoice->Start());

		WaitForSingleObjectEx( callback->hBufferEndEvent, INFINITE, TRUE );

		initialized = true;
	}
}

void AudioEngine::PushData(const Platform::Array<byte>^ data, int size)
{
	XAUDIO2_BUFFER buffer = {0};
	buffer.AudioBytes = size;  //buffer containing audio data

	buffer.pAudioData = data->Data;
	pVoice->SubmitSourceBuffer(&buffer);
}

void AudioEngine::Suspend()
{
	if (!initialized)
		return;

	// Prevent battery drain by stopping the audio engine
	pXAudio2->StopEngine();
}


void AudioEngine::Resume()
{
	if (!initialized)
		return;

	ThrowIfFailed(
		pXAudio2->StartEngine()
		);
}


void AudioEngine::PlaySound()
{
	Initialize();
	ThrowIfFailed(pVoice->SetVolume(1));
}

void AudioEngine::StopSound()
{
	if (!initialized)
		return;

	ThrowIfFailed(pVoice->SetVolume(0));
}

inline void AudioEngine::ThrowIfFailed(HRESULT hr)
{
	if (FAILED(hr))
	{
		// Set a breakpoint on this line to catch DX API errors.
		throw Platform::Exception::CreateException(hr); 
	}
}
