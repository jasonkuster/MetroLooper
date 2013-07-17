using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using MetroLooper.ViewModels;

namespace MetroLooper
{
    public partial class TrackPage : PhoneApplicationPage
    {
        private MainViewModel viewModel;

        public int BankNumber = 0;
        public int TrackNumber = 0;

        public TrackPage()
        {
            viewModel = MainViewModel.Instance;
            this.BankNumber = viewModel.SelectedBank.bankID;
            this.TrackNumber = viewModel.SelectedTrack.trackID;

            InitializeComponent();

            OffsetText.Text = viewModel.AudioMan.GetOffsetMS(this.BankNumber, this.TrackNumber).ToString();
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double value = e.NewValue;
            if (value < -35.0)
            {
                value = -120;
            }
            viewModel.AudioMan.SetVolume(this.BankNumber, this.TrackNumber, value);
        }

        private void PlayBank(object sender, RoutedEventArgs e)
        {
            viewModel.AudioMan.StopAll();
            viewModel.AudioMan.PlayBank(this.BankNumber);
        }

        private void PitchRatioSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            viewModel.AudioMan.SetPitchSemitones(this.BankNumber, this.TrackNumber, e.NewValue);
        }

        private void OffsetDecreaseButton_Click(object sender, RoutedEventArgs e)
        {
            double value = Convert.ToDouble(OffsetText.Text);
            if (value > -200)
            {
                value -= 20.0;
                OffsetText.Text = value.ToString();
                viewModel.AudioMan.SetOffsetMS(this.BankNumber, this.TrackNumber, value);
            }
        }

        private void OffsetIncreaseButton_Click(object sender, RoutedEventArgs e)
        {
            double value = Convert.ToDouble(OffsetText.Text);
            if (value < 200)
            {
                value += 20.0;
                OffsetText.Text = value.ToString();
                viewModel.AudioMan.SetOffsetMS(this.BankNumber, this.TrackNumber, value);
            }
        }
    }
}