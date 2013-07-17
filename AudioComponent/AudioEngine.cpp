// AudioEngine.cpp
#include "pch.h"
#include "AudioEngine.h"
#include <windows.h>
#include <iostream>

using namespace AudioComponent;
using namespace Platform;
using namespace std;

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
		//AudioEngine::PrintValue((int)pBufferContext);
		AudioEngine::BufferStarted((int)pBufferContext);  
	}
	void __stdcall OnLoopEnd(void * pBufferContext) {  /*OutputDebugString(L"Loop end\n");*/  }
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
		//CSCallback->PlaybackStarted();
	}
	else if (bufferContext != 0)
	{
		numBuffersPlaying++;
	}
}
void AudioEngine::PrintValue(double value)
{
	CSCallback->PrintValue(value);
}

void AudioEngine::ReadPerformanceData()
{
	//CSCallback->PrintLatencyValue(GetLatency());
	float vol;
	voices[0][0]->GetVolume(&vol);
	CSCallback->PrintValue(vol);
}

void AudioEngine::SetVolume(int bank, int track, double volume_db)
{
	voices[bank][track]->SetVolume(pow(10,(volume_db/20.0)));
}

void AudioEngine::SetPitch(int bank, int track, double pitch)
{
	float ratio = pow(2.0, pitch / 12.0);
	voices[bank][track]->SetFrequencyRatio(ratio);
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

		ThrowIfFailed(pXAudio2->CreateSourceVoice(&clickVoice, &waveformat, 0, XAUDIO2_MAX_FREQ_RATIO, callback));
		clickVoice->SetFrequencyRatio(1.0);
		clickVoice->SetVolume(1.0);

		WaitForSingleObjectEx( callback->hBufferEndEvent, INFINITE, TRUE );

		ZeroMemory(clickData, SAMPLE_RATE*sizeof(short));

		for (int i = 0; i < SAMPLE_RATE/20; i++)
		{
			clickData[i] = sin(2*3.141*440*i/SAMPLE_RATE)*SHRT_MAX;
		}

		ZeroMemory(offsets, sizeof(int)*MAX_BANKS*MAX_TRACKS);

		initialized = true;
		isClickPlaying = false;

		numBuffersPlaying = 0;
	}
}

void AudioEngine::PushData(const Platform::Array<short>^ data, int size, int bank, int track)
{
	for (int index = 0; index < MAX_OFFSET; index++)
	{
		audioData[bank][track][index] = -1;
	}
	for (int sample = MAX_OFFSET; sample < BUFFER_LENGTH && sample < size; sample++)
	{
		short value = data->get(sample);
		audioData[bank][track][sample] = value;
	}

	buffer_sizes[bank][track] = size;
	latency_offsets[bank][track] = currentLatency + microphoneLatency + LATENCY;
	CSCallback->PrintLatencyValue(currentLatency);
	CSCallback->PrintLatencyValue(microphoneLatency);
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

void AudioEngine::PlayBank(int bank)
{
	if (!initialized)
	{
		return;
	}

	for (int track = 0; track < MAX_TRACKS; track++)
	{
		int size = buffer_sizes[bank][track];
		if (size > 0)
		{
			PlayTrack(bank, track);
		}
	}

	currentLatency = GetLatency();
	ReadPerformanceData();
}

void AudioEngine::PlayTrack(int bank, int track)
{
	if (!initialized)
	{
		return;
	}

	int size = buffer_sizes[bank][track];
	if (size == 0)
	{
		return;
	}

	buffer2.AudioBytes = 2 * BUFFER_LENGTH;
	buffer2.pAudioData = (byte *)audioData[bank][track];

	int begin = MAX_OFFSET+offsets[bank][track];
	begin -= latency_offsets[bank][track];

	buffer2.PlayBegin = begin; //if no offset given, will start after the 200ms delay inserted at the beginning, and then skip latency
	buffer2.PlayLength = BUFFER_LENGTH;
	buffer2.pContext = (void *)42;

	buffer2.LoopBegin = XAUDIO2_NO_LOOP_REGION;
	buffer2.LoopLength = 0;
	buffer2.LoopCount = 0;

	if (size < BUFFER_LENGTH)
	{
		buffer2.PlayLength = size;
		buffer2.AudioBytes = 2*(size+(2*MAX_OFFSET));
	}

	ThrowIfFailed(voices[bank][track]->FlushSourceBuffers());
	ThrowIfFailed(voices[bank][track]->SubmitSourceBuffer(&buffer2));
	ThrowIfFailed(voices[bank][track]->Start());
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
			voiceCount++;
			PlayTrack(bank, track);
		}
	}

	currentLatency = GetLatency();
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
	int samples = (int)(SAMPLE_RATE/(bpm/60.0));
	beatsPerMinute = bpm;

	clickVoice->FlushSourceBuffers();

	buffer2.AudioBytes = 2 * SAMPLE_RATE;
	buffer2.pAudioData = (byte *)clickData;
	buffer2.PlayBegin = 0;
	buffer2.PlayLength = samples;
	buffer2.LoopBegin = 0;
	buffer2.LoopLength = samples;
	buffer2.LoopCount = XAUDIO2_LOOP_INFINITE;

	clickVoice->SubmitSourceBuffer(&buffer2);
}

Platform::Array<short>^ AudioEngine::GetAudioData(int bank, int track)
{
	Platform::Array<short>^ data;
	for (int i = 0; i < buffer_sizes[bank][track]; i++)
	{
		data[i] = audioData[bank][track][i];
	}
	return data;
}

int AudioEngine::GetAudioDataSize(int bank, int track)
{
	return buffer_sizes[bank][track];
}

void AudioEngine::MixDownBank(int bank)
{
	int numTracks = 0;
	for (int track = 0; track < MAX_TRACKS; track++)
	{
		int size = buffer_sizes[bank][track];
		if (size != 0)
		{
			numTracks++;
		}
	}
	for (int sample = 0; sample < BUFFER_LENGTH-(2*MAX_OFFSET); sample++)
	{
		for (int track = 0; track < numTracks; track++)
		{
			short sampleValue = audioData[bank][track][sample+MAX_OFFSET+offsets[bank][track]];
			bankAudioData[bank][sample] += (sampleValue/numTracks);
		}
	}
}

inline void AudioEngine::ThrowIfFailed(HRESULT hr)
{
	if (FAILED(hr))
	{
		// Set a breakpoint on this line to catch DX API errors.
		throw Platform::Exception::CreateException(hr); 
	}
}
