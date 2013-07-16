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
            InitializeComponent();
            viewModel = MainViewModel.Instance;
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            viewModel.AudioMan.SetVolume(this.BankNumber, this.TrackNumber, e.NewValue);
        }

        private void PlayBank(object sender, RoutedEventArgs e)
        {
            viewModel.AudioMan.PlayBank(this.BankNumber);
        }

        private void PitchRatioSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            viewModel.AudioMan.SetPitchSemitones(this.BankNumber, this.TrackNumber, e.NewValue);
        }
    }
}