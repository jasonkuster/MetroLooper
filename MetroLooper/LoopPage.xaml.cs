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
    public partial class LoopPage : PhoneApplicationPage
    {
        public enum LOCK_STATE { RECORDING, ALL, NONE };
        IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;

        #region Constructors

        public LoopPage()
        {
            InitializeComponent();
            viewModel = MainViewModel.Instance;
            this.DataContext = viewModel;
            timerRunning = false;
            if (!settings.Contains("projects"))
            {
                settings["projects"] = new ObservableCollection<Project>();
                ((ObservableCollection<Project>)settings["projects"]).Add(new Project("Project One"));
                ((ObservableCollection<Project>)settings["projects"])[0].banks.Add(new Bank(0));
            }
            //viewModel.SelectedProject = ((ObservableCollection<Project>)settings["projects"])[0];
            //viewModel.SelectedBank = viewModel.SelectedProject.banks[0];
        }

        #endregion

        #region Public and Protected Members

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ((MainViewModel)DataContext).lockUI(MainViewModel.LOCK_STATE.NONE);
            if (viewModel.SelectedBank.tracks.Count > 0)
            {
                ProgressBar.IsVisible = true;
                ProgressBar.Text = "Loading...";
                //foreach (Track t in viewModel.SelectedBank.tracks)
                //{
                //    StorageFile file = t.file;
                //    byte[] buffer = new byte[1024];
                //    using (var s = await file.OpenStreamForReadAsync())
                //    {
                //        s.Read(buffer, 0, (int)s.Length);
                //    }
                //    t.file = file;
                //}
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
            ProgressBar.IsVisible = false;
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            timer.Dispose();
            recTimer.Dispose();
            micTimer.Dispose();
            viewModel.AudioMan.StopClick();
        }

        protected async override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            //foreach (Track t in viewModel.SelectedBank.tracks)
            //{
            //    StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync("track_bank_" + viewModel.SelectedBank.bankID + "_track_" + t.trackID, CreationCollisionOption.ReplaceExisting);
            //    using (var s = await file.OpenStreamForWriteAsync())
            //    {
            //        s.Write(new byte[1], 0, 0);
            //    }
            //    t.file = file;
            //}
        }

        #endregion

        #region Private Members

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

        #region Start/Stop Recording

        private void StartMic(object state)
        {
            System.Diagnostics.Debug.WriteLine("StartMic ticked, recording is " + recording + ", starting is " + starting + ", and stop is " + stop + ".");
            if (recording)
            {
                if (starting) //!stop ||  <--continuous play scenario, remove else
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
                        viewModel.SelectedBank.tracks.Add(new Track(trackNum, null));
                        viewModel.AudioMan.RecordStopAndSubmit(viewModel.SelectedBank.bankID, trackNum);
                    });
                    recording = false;
                    //if (stop)
                    //{
                        Dispatcher.BeginInvoke(delegate
                        {
                            ((MainViewModel)DataContext).lockUI(MainViewModel.LOCK_STATE.NONE);
                        });
                    //}
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
                MeasureAnimation.Stop();
                MeasureAnimation.Begin();
                viewModel.AudioMan.StopAll();
                viewModel.AudioMan.PlayBank(viewModel.SelectedBank.bankID);
            });

            micTimer.Change(3950, System.Threading.Timeout.Infinite);
            
            //Music_Go(state);
            if (startTicking)
            {
                Dispatcher.BeginInvoke(delegate
                {
                    viewModel.AudioMan.PlayClick();
                });
                startTicking = false;
                ticking = true;
            }
            else if (ticking)
            {
                Dispatcher.BeginInvoke(delegate
                {
                    viewModel.AudioMan.StopClick();
                    viewModel.AudioMan.PlayClick();
                });
            }
        }

        #region Button Events

        private void continueButton_Click(object sender, RoutedEventArgs e)
        {
            startRecord(false);
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            stopRecord();
        }

        private void recOneButton_Click(object sender, RoutedEventArgs e)
        {
            if (!timerRunning)
            {
                timer.Change(0, 4000);
                timerRunning = true;
            }
            startRecord(true);
        }

        private async void saveButton_Click(object sender, RoutedEventArgs e)
        {
            if (viewModel.SelectedBank.tracks.Count > 0)
            {
                foreach (Track t in viewModel.SelectedBank.tracks)
                {
                    StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync("track_bank_" + viewModel.SelectedBank.bankID + "_track_" + t.trackID, CreationCollisionOption.ReplaceExisting);
                    byte[] trackData;
                    int trackLength = viewModel.AudioMan.GetAudioData(viewModel.SelectedBank.bankID, t.trackID, out trackData);
                    using (var s = await file.OpenStreamForWriteAsync())
                    {
                        s.Write(trackData, 0, trackLength);
                    }
                    t.file = file;
                }
            }
        }

        private void metronomeButton_Click(object sender, RoutedEventArgs e)
        {
            if (!timerRunning)
            {
                MeasureAnimation.Begin();
                ((MainViewModel)DataContext).AudioMan.PlayClick();
                timer.Change(4000, 4000);
                timerRunning = true;
                ticking = true;
            }
            else if (ticking)
            {
                //((MainViewModel)DataContext).AudioMan.StopClick();
                ((MainViewModel)DataContext).AudioMan.SetClickVolume((float)0.0);
                ticking = false;
            }
            else
            {
                ((MainViewModel)DataContext).AudioMan.SetClickVolume((float)1.0);
                startTicking = true;
            }
        }

        private void finalizeButton_Click(object sender, RoutedEventArgs e)
        {
            /*MessageBoxResult sure = MessageBox.Show("Are you sure you want to finalize? This will mix your tracks down and delete the individual files.", "Finalize?", MessageBoxButton.OKCancel);
            if (sure == MessageBoxResult.OK)
            {
                //Mix-down and deletion code
                if (NavigationService.CanGoBack)
                {
                    NavigationService.GoBack();
                }
            }*/

            viewModel.AudioMan.MixDownBank(viewModel.SelectedBank.bankID);
            trackPanel.Visibility = Visibility.Collapsed;
            BankPanel.Visibility = Visibility.Visible;

            timer.Dispose();
            recTimer.Dispose();
            micTimer.Dispose();
            viewModel.AudioMan.StopClick();
            viewModel.AudioMan.StopAll();
        }

        #endregion

        private void LongListSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((Track)loopList.SelectedItem) != null)
            {
                MeasureAnimation.Stop();
                viewModel.AudioMan.StopAll();
                viewModel.SelectedTrack = ((Track)loopList.SelectedItem);
                loopList.SelectedItem = null;
                NavigationService.Navigate(new Uri("/TrackPage.xaml", UriKind.RelativeOrAbsolute));
            }
        }

        private void startRecord(bool one)
        {
            if (!timerRunning)
            {
                timer.Change(0, 4000);
                timerRunning = true;
            }
            recording = true;
            starting = true;
            stop = one;
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

        private void PlayBankButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.AudioMan.StopAll();
            viewModel.AudioMan.PlayBank(viewModel.SelectedBank.bankID);
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double value = e.NewValue;
            if (value < -35.0)
            {
                value = -120;
            }
            viewModel.AudioMan.SetBankVolume(viewModel.SelectedBank.bankID, value);
        }

        private void PitchRatioSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            viewModel.AudioMan.SetPitchSemitones(viewModel.SelectedBank.bankID, e.NewValue);
        }

        private void OffsetIncreaseButton_Click(object sender, RoutedEventArgs e)
        {
            double value = Convert.ToDouble(OffsetText.Text);
            if (value < 400)
            {
                value += 20.0;
                OffsetText.Text = value.ToString();
                viewModel.AudioMan.SetBankOffsetMS(viewModel.SelectedBank.bankID, value);
            }
        }

        private void OffsetDecreaseButton_Click(object sender, RoutedEventArgs e)
        {
            double value = Convert.ToDouble(OffsetText.Text);
            if (value > -400)
            {
                value -= 20.0;
                OffsetText.Text = value.ToString();
                viewModel.AudioMan.SetBankOffsetMS(viewModel.SelectedBank.bankID, value);
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            double zero = 0.0;

            VolumeSlider.Value = zero;
            PitchRatioSlider.Value = zero;
            OffsetText.Text = zero.ToString();

            viewModel.AudioMan.SetBankVolume(viewModel.SelectedBank.bankID, zero);
            viewModel.AudioMan.SetPitchSemitones(viewModel.SelectedBank.bankID, zero);
            viewModel.AudioMan.SetBankOffsetMS(viewModel.SelectedBank.bankID, zero);
        }
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