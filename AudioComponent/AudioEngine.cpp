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
	CSCallback->PrintLatencyValue(GetLatency());
}

void AudioEngine::SetVolumeDB(int bank, int track, double volume_db)
{
	voices[bank][track]->SetVolume(pow(10,(volume_db/20.0)));
}

void AudioEngine::SetBankVolumeDB(int bank, double volume_db)
{
	bankVoices[bank]->SetVolume(pow(10,(volume_db/20.0)));
}

void AudioEngine::SetBankPitch(int bank, double pitch)
{
	float ratio = pow(2.0, pitch / 12.0);
	bankVoices[bank]->SetFrequencyRatio(ratio);
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

			bankFinalized[i] = false;
			ThrowIfFailed(pXAudio2->CreateSourceVoice(&(bankVoices[i]), &waveformat, 0, XAUDIO2_MAX_FREQ_RATIO, callback));
			bankVoices[i]->SetFrequencyRatio(1);
			bankVoices[i]->SetVolume(1.0);
			bank_sizes[i] = 0;
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
		ZeroMemory(bank_offsets, sizeof(int)*MAX_BANKS);

		ZeroMemory(audioData, sizeof(short)*MAX_BANKS*MAX_TRACKS*BUFFER_LENGTH);
		ZeroMemory(bankAudioData, sizeof(short)*MAX_BANKS*BUFFER_LENGTH);

		pulledData = ref new Platform::Array<short>(BUFFER_LENGTH);

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
	for (int sample = MAX_OFFSET; sample < BUFFER_LENGTH && sample < size + MAX_OFFSET; sample++)
	{
		short value = data->get(sample - MAX_OFFSET);
		audioData[bank][track][sample] = value;
	}

	buffer_sizes[bank][track] = size;
	latency_offsets[bank][track] = currentLatency;
	CSCallback->PrintLatencyValue(currentLatency);
	//CSCallback->PrintLatencyValue(microphoneLatency);
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

	if (!bankFinalized[bank])
	{
		for (int track = 0; track < MAX_TRACKS; track++)
		{
			int size = buffer_sizes[bank][track];
			if (size > 0)
			{
				PlayTrack(bank, track);
			}
		}
	}
	else
	{
		PlayFullBank(bank);
	}

	currentLatency = GetLatency();
}

void AudioEngine::PlayFullBank(int bank)
{
	if (!initialized || !bankFinalized[bank])
	{
		return;
	}

	int size = bank_sizes[bank];
	if (size == 0)
	{
		return;
	}

	buffer2.AudioBytes = 2*BUFFER_LENGTH;
	buffer2.pAudioData = (byte *)bankAudioData[bank];

	int begin = MAX_OFFSET;
	begin += bank_offsets[bank];
	begin += LATENCY;

	buffer2.PlayBegin = begin;
	buffer2.PlayLength = BUFFER_LENGTH;

	buffer2.LoopBegin = XAUDIO2_NO_LOOP_REGION;
	buffer2.LoopLength = 0;
	buffer2.LoopCount = 0;

	if (size < BUFFER_LENGTH)
	{
		buffer2.PlayLength = size;
		buffer2.AudioBytes = 2*(size+(2*MAX_OFFSET));
	}

	ThrowIfFailed(bankVoices[bank]->FlushSourceBuffers());
	ThrowIfFailed(bankVoices[bank]->SubmitSourceBuffer(&buffer2));
	ThrowIfFailed(bankVoices[bank]->Start());
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
	begin += latency_offsets[bank][track];
	begin += LATENCY;

	buffer2.PlayBegin = begin; //if no offset given, will start after the 200ms delay inserted at the beginning, and then skip latency
	buffer2.PlayLength = BUFFER_LENGTH;
	buffer2.pContext = (void *)42;

	buffer2.LoopBegin = XAUDIO2_NO_LOOP_REGION;
	buffer2.LoopLength = 0;
	buffer2.LoopCount = 0;

	if (size < BUFFER_LENGTH)
	{
		buffer2.PlayLength = size;
		buffer2.AudioBytes = (2*size)+(2*buffer2.PlayBegin);
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
	for (int i = 0; i < buffer_sizes[bank][track]; i++)
	{
		pulledData[i] = audioData[bank][track][i+MAX_OFFSET];
	}
	return pulledData;
}
Platform::Array<short>^ AudioEngine::GetBankAudioData(int bank)
{
	for (int i = 0; i < bank_sizes[bank]; i++)
	{
		pulledData[i] = bankAudioData[bank][i+MAX_OFFSET];
	}
	return pulledData;
}

int AudioEngine::GetAudioDataSize(int bank, int track)
{
	return buffer_sizes[bank][track];
}

void AudioEngine::MixDownBank(int bank)
{
	if (bankFinalized[bank] || !initialized)
	{
		return;
	}

	int numTracks = 0;
	int biggestSize = 0;
	double trackVolumes[MAX_TRACKS];

	for (int track = 0; track < MAX_TRACKS; track++)
	{
		int size = buffer_sizes[bank][track];
		if (size != 0)
		{
			numTracks++;
			if (size > biggestSize)
			{
				biggestSize = size;
			}
		}
		float gain;
		voices[bank][track]->GetVolume(&gain);
		trackVolumes[track] = gain;
	}

	for (int i = 0; i < MAX_OFFSET; i++)
	{
		bankAudioData[bank][i] = -1;
	}
	for (int sample = 0; sample < BUFFER_LENGTH; sample++)
	{
		for (int track = 0; track < numTracks; track++)
		{
			short sampleValue = audioData[bank][track][sample+MAX_OFFSET+offsets[bank][track]+latency_offsets[bank][track]+LATENCY];
			bankAudioData[bank][sample+MAX_OFFSET] += trackVolumes[track]*(sampleValue/numTracks);
		}
	}

	bank_sizes[bank] = biggestSize;
	bankFinalized[bank] = true;
}

inline void AudioEngine::ThrowIfFailed(HRESULT hr)
{
	if (FAILED(hr))
	{
		// Set a breakpoint on this line to catch DX API errors.
		throw Platform::Exception::CreateException(hr); 
	}
}

void AudioEngine::LoadTrack(int bank, int track, const Platform::Array<short>^ data, int size, int offset_ms, int latency_samples, double volume_db)
{
	buffer_sizes[bank][track] = size;
	SetOffset(offset_ms, bank, track);
	latency_offsets[bank][track] = latency_samples;

	for (int sample = 0; sample < size; sample++)
	{
		short value = data->get(sample);
		audioData[bank][track][sample+MAX_OFFSET] = value;
	}

	SetVolumeDB(bank, track, volume_db);
}
void AudioEngine::LoadBank(int bank, const Platform::Array<short>^ data, int size, int offset_ms, double volume_db, double pitch)
{
	bank_sizes[bank] = size;
	SetBankOffset(offset_ms, bank);

	for (int sample = 0; sample < size; sample++)
	{
		short value = data->get(sample);
		bankAudioData[bank][sample+MAX_OFFSET] = value;
	}

	SetBankVolumeDB(bank, volume_db);
	SetBankPitch(bank, pitch);
	bankFinalized[bank] = true;
}

void AudioEngine::deleteTrack(int bank, int track)
{
	buffer_sizes[bank][track] = 0;
	SetOffset(0, bank, track);
	latency_offsets[bank][track] = 0;

	ZeroMemory(audioData[bank][track], sizeof(short)*BUFFER_LENGTH);
	SetVolumeDB(bank, track, 0.0);
}

void AudioEngine::deleteFinalizedBank(int bank)
{
	bank_sizes[bank] = 0;
	SetBankOffset(0, bank);

	ZeroMemory(bankAudioData[bank], sizeof(short)*BUFFER_LENGTH);

	SetBankVolumeDB(bank, 0.0);
	SetBankPitch(bank, 0);
	bankFinalized[bank] = false;
}

void AudioEngine::LoadClickOneSecond(const Platform::Array<short>^ data)
{
	for (int sample = 0; sample < SAMPLE_RATE; sample++)
	{
		short value = data->get(sample);
		//value *= 5;
		clickData[sample] = value;
	}
}
