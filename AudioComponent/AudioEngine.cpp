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
	void __stdcall VoiceCallback::OnStreamEnd() { SetEvent( hBufferEndEvent ); OutputDebugString(L"Stream end\n"); }

	//Unused methods are stubs
	void __stdcall OnVoiceProcessingPassEnd() { }
	void __stdcall OnVoiceProcessingPassStart(UINT32 SamplesRequired) {  }
	void __stdcall OnBufferEnd(void * pBufferContext)
	{
		OutputDebugString(L"Buffer end\n"); 
		AudioEngine::BufferFinished((int)pBufferContext);
	}

	void __stdcall OnBufferStart(void * pBufferContext) 
	{ 
		OutputDebugString(L"Buffer start\n");  
		AudioEngine::BufferStarted((int)pBufferContext);  
	}
	void __stdcall OnLoopEnd(void * pBufferContext) {  OutputDebugString(L"Loop end\n");  }
	void __stdcall OnVoiceError(void * pBufferContext, HRESULT Error) { }
};

AudioEngine::AudioEngine()
{
	Initialize();
}

ICallback ^CSCallback = nullptr;
int AudioEngine::numBuffersPlaying = 0;

void AudioEngine::SetCallback( ICallback ^Callback)
{
	CSCallback = Callback;
}

void AudioEngine::BufferFinished(int bufferContext)
{
	CSCallback->BufferFinished(bufferContext);
	numBuffersPlaying--;
}
void AudioEngine::BufferStarted(int bufferContext)
{
	if (numBuffersPlaying == 0 || bufferContext == 0)
	{
		CSCallback->PlaybackStarted();
	}
	else if (bufferContext != 0)
	{
		numBuffersPlaying++;
	}
}
void AudioEngine::PrintValue(int value)
{
	CSCallback->PrintValue(value);
}

void AudioEngine::ReadPerformanceData()
{
	XAUDIO2_PERFORMANCE_DATA data;
	pXAudio2->GetPerformanceData(&data);

	int latencyInSamples = data.CurrentLatencyInSamples;
	CSCallback->PrintValue(latencyInSamples);
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

		for (int i = 0; i < MAX_BANKS; i++)
		{
			for (int j = 0; j < MAX_TRACKS; j++)
			{
				ThrowIfFailed(pXAudio2->CreateSourceVoice(&(voices[i][j]), &waveformat, 0, XAUDIO2_MAX_FREQ_RATIO, callback));
				voices[i][j]->SetFrequencyRatio(1);
				voices[i][j]->SetVolume(1.0);
				buffer_sizes[i][j] = 0;
			}
		}

		ThrowIfFailed(pXAudio2->CreateSourceVoice(&clickVoice, &waveformat, 0, XAUDIO2_MAX_FREQ_RATIO));
		clickVoice->SetFrequencyRatio(1.0);
		clickVoice->SetVolume(1.0);

		WaitForSingleObjectEx( callback->hBufferEndEvent, INFINITE, TRUE );

		ZeroMemory(clickData, SAMPLE_RATE*sizeof(short));
		clickData[0] = SHRT_MAX;

		initialized = true;
		isClickPlaying = false;

		numBuffersPlaying = 0;
	}
}

void AudioEngine::PushData(const Platform::Array<short>^ data, int size, int bank, int track)
{
	//Should remove latency from all tracks. Need to test this.
	int latencyInSamples = 0;

	XAUDIO2_PERFORMANCE_DATA perfData;
	pXAudio2->GetPerformanceData(&perfData);
	latencyInSamples = perfData.CurrentLatencyInSamples;

	for (int sample = latencyInSamples; sample < BUFFER_LENGTH && sample < size; sample++)
	{
		short value = data->get(sample);
		audioData[bank][track][sample-latencyInSamples] = value;
	}
	buffer_sizes[bank][track] = size-latencyInSamples;
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

int AudioEngine::PlayTrack(int bank, int track)
{
	if (!initialized)
		return -1;

	int size = buffer_sizes[bank][track];
	if (size == 0)
	{
		return 0;
	}

	buffer2.AudioBytes = 2 * BUFFER_LENGTH;
	audioData[bank][track];
	buffer2.pAudioData = (byte *)audioData[bank][track];
	buffer2.PlayBegin = 0;
	buffer2.PlayLength = BUFFER_LENGTH;
	buffer2.pContext = (void *)42;

	if (size < BUFFER_LENGTH)
	{
		buffer2.PlayLength = size;
		buffer2.AudioBytes = 2*size;
	}

	ThrowIfFailed(voices[bank][track]->SubmitSourceBuffer(&buffer2));

	return size;
}

void AudioEngine::PlaySound()
{
	if (!initialized)
		return;

	int voiceCount = 0;

	for (int bank = 0; bank < MAX_BANKS; bank++)
	{
		for (int track = 0; track < MAX_TRACKS; track++)
		{
			int size = buffer_sizes[bank][track];
			if (size == 0)
			{
				continue;
			}
			voiceCount++;
			buffer2.AudioBytes = 2 * BUFFER_LENGTH;
			buffer2.pAudioData = (byte *)audioData[bank][track];
			buffer2.PlayBegin = 0;
			buffer2.PlayLength = BUFFER_LENGTH;
			buffer2.pContext = (void *)42;

			if (size < BUFFER_LENGTH)
			{
				buffer2.PlayLength = size;
				buffer2.AudioBytes = 2*size;
			}

			ThrowIfFailed(voices[bank][track]->SubmitSourceBuffer(&buffer2));
		}
	}

	for (int i = 0; i < MAX_BANKS; i++)
	{
		for (int j = 0; j < MAX_TRACKS; j++)
		{
			ThrowIfFailed(voices[i][j]->Start());
		}
	}

	if (voiceCount == 0)
	{
		BufferStarted(0);
	}
}

void AudioEngine::PlayClickTrack()
{
	if (!initialized)
		return;

	ThrowIfFailed(clickVoice->Start());
	isClickPlaying = true;
}

void AudioEngine::StopClickTrack()
{
	if (!initialized)
		return;

	ThrowIfFailed(clickVoice->Stop());
	SetBPM(beatsPerMinute); //Refresh click voice buffers.

	isClickPlaying = false;
}

void AudioEngine::StopSound()
{
	if (!initialized)
		return;

	for (int i = 0; i < MAX_BANKS; i++)
	{
		for (int j = 0; j < MAX_TRACKS; j++)
		{
			ThrowIfFailed(voices[i][j]->Stop());
		}
	}

	numBuffersPlaying = 0;
}

void AudioEngine::SetBPM(int bpm)
{
	int samples = (SAMPLE_RATE/(bpm/60));
	beatsPerMinute = bpm;

	clickVoice->FlushSourceBuffers();

	buffer2.AudioBytes = 2 * SAMPLE_RATE;
	buffer2.pAudioData = (byte *)clickData;
	buffer2.PlayBegin = 0;
	buffer2.PlayLength = samples;
	buffer2.LoopBegin = 0;
	buffer2.LoopLength = samples;
	buffer2.LoopCount = XAUDIO2_LOOP_INFINITE;

	//PrintValue(beatsPerMinute);

	clickVoice->SubmitSourceBuffer(&buffer2);
}

inline void AudioEngine::ThrowIfFailed(HRESULT hr)
{
	if (FAILED(hr))
	{
		// Set a breakpoint on this line to catch DX API errors.
		throw Platform::Exception::CreateException(hr); 
	}
}
