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

            this.DataContext = viewModel;
            InitializeComponent();

            OffsetText.Text = viewModel.AudioMan.GetOffsetMS(this.BankNumber, this.TrackNumber).ToString();
            VolumeSlider.Value = viewModel.SelectedTrack.Volume;
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double value = e.NewValue;
            if (value < -35.0)
            {
                value = -120;
            }
            viewModel.AudioMan.SetVolumeDB(this.BankNumber, this.TrackNumber, value);
            viewModel.SelectedTrack.Volume = value;
        }

        private void PlayBank(object sender, RoutedEventArgs e)
        {
            viewModel.AudioMan.StopAll();
            viewModel.AudioMan.PlayBank(this.BankNumber);
        }

        private void OffsetDecreaseButton_Click(object sender, RoutedEventArgs e)
        {
            double value = Convert.ToDouble(OffsetText.Text);
            if (value > -400)
            {
                value -= 20.0;
                OffsetText.Text = value.ToString();
                viewModel.AudioMan.SetOffsetMS(this.BankNumber, this.TrackNumber, value);
                viewModel.SelectedTrack.Offset = (int)value;
            }
        }

        private void OffsetIncreaseButton_Click(object sender, RoutedEventArgs e)
        {
            double value = Convert.ToDouble(OffsetText.Text);
            if (value < 400)
            {
                value += 20.0;
                OffsetText.Text = value.ToString();
                viewModel.AudioMan.SetOffsetMS(this.BankNumber, this.TrackNumber, value);
                viewModel.SelectedTrack.Offset = (int)value;
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.AudioMan.DeleteTrack(this.BankNumber, this.TrackNumber);

            viewModel.SelectedBank.tracks.Remove(viewModel.SelectedTrack);

            NavigationService.GoBack();
        }
    }
}