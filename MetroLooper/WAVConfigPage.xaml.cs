using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework.Audio;
using MetroLooper.Audio;
using MetroLooper.ViewModels;
using System.IO;
using MetroLooper.Model;

namespace MetroLooper
{
    public partial class WAVConfigPage : PhoneApplicationPage
    {
        private MainViewModel viewModel;
        private int startTimeInMilliseconds = 0;
        private int sampleRate = 16000;
        private double secondsPerMeasure = 4;
        private short[] shortData;
        private byte[] byteData;
        private MemoryStream stream;
        SoundEffect effect;

        public WAVConfigPage()
        {
            stream = new MemoryStream();
            viewModel = MainViewModel.Instance;
            byteData = stream.ToArray();

            shortData = new short[(int)(secondsPerMeasure*sampleRate)];
            for (int i = 0; i < sampleRate; i++)
            {
                shortData[i] = (short)(Math.Cos(440 * 2 * 3.14 * i / sampleRate)*short.MaxValue);
            }
            byteData = Helper.ConvertShortArrayToByteArray(shortData);


            InitializeComponent();
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            int startSample = startTimeInMilliseconds * (sampleRate / 1000);
            int startByte = startSample * sizeof(short);

            int numSamples = (int)(secondsPerMeasure * sampleRate);
            int numBytes = numSamples * sizeof(short);

            effect = new SoundEffect(byteData, startByte, numBytes, sampleRate, AudioChannels.Mono, 0, 0);
            effect.Play();
        }

        private void LeftButton_Click(object sender, RoutedEventArgs e)
        {
            startTimeInMilliseconds -= 100;
            if (startTimeInMilliseconds < 0)
            {
                startTimeInMilliseconds = 0;
            }
            Time.Text = startTimeInMilliseconds.ToString();
        }

        private void RightButton_Click(object sender, RoutedEventArgs e)
        {
            startTimeInMilliseconds += 100;
        }

        private void DoneButton_Click(object sender, RoutedEventArgs e)
        {
            int trackNum = viewModel.SelectedBank.tracks.Count;
            viewModel.AudioMan.AddTrackFromWAVStream(stream, startTimeInMilliseconds, viewModel.SelectedBank.bankID, trackNum);
            viewModel.SelectedBank.tracks.Add(new Track() { trackID = trackNum } );
            //Submit to AudioManager, need bank and track
        }
    }
}