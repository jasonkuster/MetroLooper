using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Windows.Storage;
using System.Threading.Tasks;
using System.IO;
using System.IO.IsolatedStorage;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using System.Windows.Threading;
using System.Threading;
using MetroLooper.ViewModels;
using MetroLooper.Model;
using System.Windows.Data;
using System.Windows.Media;

namespace MetroLooper
{
    public partial class BankPage : PhoneApplicationPage
    {
        public enum LOCK_STATE { RECORDING, STOPPED, PLAYING, FINALIZING };
        IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;

        #region Constructors

        public BankPage()
        {
            InitializeComponent();
            viewModel = MainViewModel.Instance;
            this.DataContext = viewModel;
            timerRunning = false;

            viewModel.AudioMan.GetPerf(); //DO NOT REMOVE
        }

        #endregion

        #region Public and Protected Members

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ((MainViewModel)DataContext).lockUI(MainViewModel.LOCK_STATE.NONE); //TODO: CHANGE THIS TO STATES
            if (!viewModel.SelectedBank.Finalized)
            {
                if (viewModel.SelectedBank.tracks.Count > 0)
                {
                    timer = new Timer(Progress_Go, new object(), 0, 4000);
                    timerRunning = true;
                }
                else
                {
                    this.timer = new Timer(Progress_Go, new object(), System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                    timerRunning = false;
                }
                this.recTimer = new Timer(CompleteRecord, new object(), System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                this.micTimer = new Timer(StartMic, new object(), System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                VisualStateManager.GoToState(this, "Opened", true);
            }
            else
            {
                VolumeSlider.Value = viewModel.SelectedBank.Volume;
                //PitchRatioSlider.Value = viewModel.SelectedBank.Pitch; //TODO: ADD THIS
                OffsetTextBlock.Text = viewModel.SelectedBank.Offset.ToString();
                VisualStateManager.GoToState(this, "Finalized", true);
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (!viewModel.SelectedBank.Finalized)
            {
                timer.Dispose();
                recTimer.Dispose();
                micTimer.Dispose();
                viewModel.AudioMan.StopClick();
                if (recording && !starting)
                {
                    viewModel.AudioMan.RecordStop();
                }
            }
            viewModel.AudioMan.StopAll();
            base.OnNavigatingFrom(e);
        }

        #endregion

        #region Private Members

        #region Global Variables

        private Timer timer;
        private Timer recTimer;
        private Timer micTimer;
        private bool ticking = false;
        private bool startTicking = false;
        private bool recording = false;
        private bool stop = true;
        private bool starting = false;
        private bool timerRunning;
        private MainViewModel viewModel;

        #endregion

        #region Start/Stop Recording

        private void StartMic(object state)
        {
            viewModel.AudioMan.StopClick();
            System.Diagnostics.Debug.WriteLine("StartMic ticked, recording is " + recording + ", starting is " + starting + ", and stop is " + stop + ".");
            if (recording)
            {
                if (starting)
                {
                    starting = false;
                    recording = true;
                    recTimer.Change(4100, System.Threading.Timeout.Infinite);
                    Dispatcher.BeginInvoke(delegate
                    {
                        foreach (Track t in viewModel.SelectedBank.tracks)
                        {
                            t.Finalized = true;
                        }
                        viewModel.AudioMan.RecordStart();
                        viewModel.lockUI(MainViewModel.LOCK_STATE.RECORDING);
                    });
                }
            }
        }

        private void CompleteRecord(object state)
        {
            System.Diagnostics.Debug.WriteLine("CompleteRecord ticked, recording is " + recording + ", starting is " + starting + ", and stop is " + stop + ".");
            if (recording)
            {
                if (!starting)
                {
                    Dispatcher.BeginInvoke(delegate
                    {
                        int trackNum = viewModel.SelectedBank.tracks.Count;
                        Track newTrack = new Track() { trackID = trackNum };
                        viewModel.SelectedBank.tracks.Add(newTrack);
                        viewModel.AudioMan.RecordStopAndSubmit(viewModel.SelectedBank.bankID, trackNum);
                        byte[] trackData;
                        newTrack.Size = viewModel.AudioMan.GetAudioData(viewModel.SelectedBank.bankID, trackNum, out trackData);
                        newTrack.trackData = trackData;
                        //TODO: Mix down bank here?
                        ((MainViewModel)DataContext).lockUI(MainViewModel.LOCK_STATE.NONE); //TODO: Fix this
                    });
                    recording = false;
                }
            }
        }

        #endregion

        private void Progress_Go(object state)
        {
            System.Diagnostics.Debug.WriteLine("Progress_go ticked, there are  " + viewModel.SelectedBank.tracks.Count + " tracks in bank " + viewModel.SelectedBank.bankID + ".");
            Dispatcher.BeginInvoke(delegate
            {
                foreach (Track t in viewModel.SelectedBank.tracks)
                {
                    t.Finalized = true;
                }
                PlayAnimation.Stop();
                progressBar.Value = 0;
                PlayAnimation.Begin();
                viewModel.AudioMan.StopAll();
                viewModel.AudioMan.PlayBank(viewModel.SelectedBank.bankID); //TODO: This should be PlayMixedDownBank?
                viewModel.AudioMan.SetClickVolume(MetronomeSlider.IsChecked == true ? 1 : 0);
            });

            micTimer.Change(3950, System.Threading.Timeout.Infinite);

            viewModel.AudioMan.PlayClick();
        }

        #region Button Events

        #region recPanel Buttons

        private void recordButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (!timerRunning)
            {
                timer.Change(0, 4000);
                timerRunning = true;
            }
            startRecord(true);
        }

        private void stopPlayButton_Tap(object sender, System.Windows.Input.GestureEventArgs e) //TODO: Merge with finalized stop/start button
        {
            if (timerRunning)
            {
                timer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                recTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                micTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                PlayAnimation.Stop();
                viewModel.AudioMan.StopClick();
                viewModel.AudioMan.StopAll();
                timerRunning = false;
                if (recording)
                {
                    if (!starting)
                    {
                    viewModel.AudioMan.RecordStop();
                    }
                    recording = false;
                    starting = false;
                }
                //stopPlayImage.Source = "/Assets/play.png"; //TODO: Change background of button
            }
            else
            {
                timer.Change(0, 4000);
                timerRunning = true;
                //stopPlayImage.Source = "/Assets/stop.png";
            }
        }

        private void metronomeButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.AudioMan.SetClickVolume(MetronomeSlider.IsChecked == true ? 1 : 0);
            if (!timerRunning)
            {
                timer.Change(0, 4000);
                timerRunning = true;
            }
        }

        #endregion

        #region editPanel Buttons

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double value = e.NewValue;
            if (value < -35.0)
            {
                value = -120;
            }
            if (viewModel.SelectedBank.Finalized)
            {
                viewModel.AudioMan.SetBankVolumeDB(viewModel.SelectedBank.bankID, value);
                viewModel.SelectedBank.Volume = value;
            }
            else
            {
                viewModel.AudioMan.SetVolumeDB(viewModel.SelectedBank.bankID, viewModel.SelectedTrack.trackID, value);
                viewModel.SelectedTrack.Volume = value;
            }
        }

        private void OffsetIncreaseButton_Click(object sender, RoutedEventArgs e)
        {
            double value = Convert.ToDouble(OffsetTextBlock.Text);
            if (value < 400)
            {
                value += 20.0;
                OffsetTextBlock.Text = value.ToString();
                if (viewModel.SelectedBank.Finalized)
                {
                    viewModel.AudioMan.SetBankOffsetMS(viewModel.SelectedBank.bankID, value);
                    viewModel.SelectedBank.Offset = (int)value;
                }
                else
                {
                    viewModel.AudioMan.SetOffsetMS(viewModel.SelectedBank.bankID, viewModel.SelectedTrack.trackID, value);
                    viewModel.SelectedTrack.Offset = (int)value;
                }
            }
        }
        private void OffsetDecreaseButton_Click(object sender, RoutedEventArgs e)
        {
            double value = Convert.ToDouble(OffsetTextBlock.Text);
            if (value > -400)
            {
                value -= 20.0;
                OffsetTextBlock.Text = value.ToString();
                if (viewModel.SelectedBank.Finalized)
                {
                    viewModel.AudioMan.SetBankOffsetMS(viewModel.SelectedBank.bankID, value);
                    viewModel.SelectedBank.Offset = (int)value;
                }
                else
                {
                    viewModel.AudioMan.SetOffsetMS(viewModel.SelectedBank.bankID, viewModel.SelectedTrack.trackID, value);
                    viewModel.SelectedTrack.Offset = (int)value;
                }
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            double zero = 0.0;

            VolumeSlider.Value = zero;
            //PitchRatioSlider.Value = zero; //TODO: FIX THIS
            OffsetTextBlock.Text = zero.ToString();

            if (viewModel.SelectedBank.Finalized)
            {
                viewModel.AudioMan.SetBankVolumeDB(viewModel.SelectedBank.bankID, zero);
                viewModel.SelectedBank.Volume = zero;
                viewModel.AudioMan.SetPitchSemitones(viewModel.SelectedBank.bankID, zero);
                viewModel.SelectedBank.Pitch = zero;
                viewModel.AudioMan.SetBankOffsetMS(viewModel.SelectedBank.bankID, zero);
                viewModel.SelectedBank.Offset = (int)zero;
            }
            else
            {
                viewModel.AudioMan.SetVolumeDB(viewModel.SelectedBank.bankID, viewModel.SelectedTrack.trackID, zero);
                viewModel.SelectedTrack.Volume = zero;
                viewModel.AudioMan.SetOffsetMS(viewModel.SelectedBank.bankID, viewModel.SelectedTrack.trackID, zero);
                viewModel.SelectedTrack.Offset = (int)zero;
            }
        }

        private void PlayBankButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.AudioMan.StopAll();
            viewModel.AudioMan.PlayBank(viewModel.SelectedBank.bankID);
        }

        private void delButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (viewModel.SelectedBank.Finalized)
            {
                VisualStateManager.GoToState(this, "Opened", true);
                viewModel.AudioMan.DeleteTrack(viewModel.SelectedBank.bankID, viewModel.SelectedTrack.trackID);
                viewModel.SelectedBank.tracks.Remove(viewModel.SelectedTrack);
                int skip = 0;
                bool foundSkip = false;
                foreach (Track t in viewModel.SelectedBank.tracks)
                {
                    if (foundSkip)
                    {
                        t.trackID -= 1;
                    }
                    else
                    {
                        if (skip - t.trackID > 1)
                        {
                            foundSkip = true;
                            t.trackID -= 1;
                        }
                        skip = t.trackID;
                    }
                }
            }
            else
            {
                VisualStateManager.GoToState(this, "Opened", true);
                Bank b = viewModel.SelectedBank;
                viewModel.SelectedBank = null;
                viewModel.SelectedProject.banks.Remove(b);
                NavigationService.GoBack();
            }
        }

        #endregion

        #region loopPanel Buttons

        private void LongListSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((Track)loopList.SelectedItem) != null)
            {
                VisualStateManager.GoToState(this, "TrackSelected", true);
                PlayAnimation.Stop();
                MetronomeSlider.IsChecked = false;
                viewModel.AudioMan.StopAll();
                viewModel.AudioMan.StopClick();
                viewModel.SelectedTrack = ((Track)loopList.SelectedItem);
                viewModel.SelectedTrack.IsSelected = true;
                timer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                recTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                micTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                timerRunning = false;
                VolumeSlider.Value = viewModel.SelectedTrack.Volume;
                OffsetTextBlock.Text = viewModel.SelectedTrack.Offset.ToString();
            }
        }

        private void NewTrackButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            VisualStateManager.GoToState(this, "Opened", true);
            viewModel.SelectedTrack.IsSelected = false;
            viewModel.SelectedTrack = null;
            loopList.SelectedItem = null;
        }

        private void FinalizeButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            timer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            recTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            micTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            PlayAnimation.Stop();
            viewModel.AudioMan.StopClick();
            viewModel.AudioMan.StopAll();
            timerRunning = false;
            VisualStateManager.GoToState(this, "Stopped", false); //TODO: Change this to ViewModel State
            MessageBoxResult sure = MessageBox.Show("Are you sure you want to finalize? This will mix your tracks down and delete the individual files.", "Finalize?", MessageBoxButton.OKCancel);
            if (sure == MessageBoxResult.OK)
            {
                //Mix-down and deletion code
                viewModel.AudioMan.MixDownBank(viewModel.SelectedBank.bankID);
                viewModel.SelectedBank.Finalized = true;
                timer.Dispose();
                recTimer.Dispose();
                micTimer.Dispose();

                VisualStateManager.GoToState(this, "Finalized", true);

                byte[] trackData;
                int trackLength = viewModel.AudioMan.GetBankAudioData(viewModel.SelectedBank.bankID, out trackData);
                viewModel.SelectedBank.finalTrack = trackData;
                viewModel.SelectedBank.Size = trackLength;
                viewModel.SelectedBank.Pitch = viewModel.AudioMan.GetPitchSemitones(viewModel.SelectedBank.bankID);
                viewModel.SelectedBank.Offset = viewModel.AudioMan.GetBankOffsetMS(viewModel.SelectedBank.bankID);
                viewModel.SelectedBank.Volume = viewModel.AudioMan.GetBankVolumeDB(viewModel.SelectedBank.bankID);
            }
        }

        #endregion

        private void PitchRatioSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            viewModel.AudioMan.SetPitchSemitones(viewModel.SelectedBank.bankID, e.NewValue);
            viewModel.SelectedBank.Pitch = e.NewValue;
        }

        private void openWavButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/WavPage.xaml", UriKind.RelativeOrAbsolute));
        }

        #endregion

        #region Button Helpers

        private void startRecord(bool one)
        {
            if (!timerRunning)
            {
                timer.Change(0, 4000);
                timerRunning = true;
            }
            recording = true;
            starting = true;
            ((MainViewModel)DataContext).lockUI(MainViewModel.LOCK_STATE.PPREC);
        }

        private void stopRecord()
        {
            recording = false;
            stop = true;
            starting = false;
            ((MainViewModel)DataContext).lockUI(MainViewModel.LOCK_STATE.ALL);
        }

        #endregion

        #endregion
    }


    /// <summary>
    /// Converter which turns True -> Collapsed, False -> Visible
    /// </summary>
    public sealed class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo language)
        {
            return (value is bool && (bool)value) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo language)
        {
            return value is Visibility && (Visibility)value == Visibility.Collapsed;
        }
    }

    /// <summary>
    /// Converter which turns True -> Green, False -> Orange
    /// </summary>
    public sealed class BooleanToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo language)
        {
            return (value is bool && (bool)value) ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Orange);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo language)
        {
            return value is SolidColorBrush && ((SolidColorBrush)value).Color == System.Windows.Media.Colors.Green;
        }
    }
}