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

namespace MetroLooper
{
    public partial class WAVConfigPage : PhoneApplicationPage
    {
        private int startTimeInMilliseconds = 0;
        private int sampleRate = 16000;
        private double secondsPerMeasure = 4;
        private short[] shortData;
        private byte[] byteData;
        SoundEffect effect;

        public WAVConfigPage()
        {
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
            //Submit to AudioManager, need bank and track
        }
    }
}