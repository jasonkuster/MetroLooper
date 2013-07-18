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
                ((ObservableCollection<Project>)settings["projects"])[0].banks.Add(new Bank() { bankID = 0 });
            }
            //viewModel.SelectedProject = ((ObservableCollection<Project>)settings["projects"])[0];
            //viewModel.SelectedBank = viewModel.SelectedProject.banks[0];
            //IsolatedStorageSettings.ApplicationSettings.Save();

            VolumeSlider.Value = viewModel.SelectedBank.Volume;
            PitchRatioSlider.Value = viewModel.SelectedBank.Pitch;
            OffsetText.Text = viewModel.SelectedBank.Offset.ToString();

            viewModel.AudioMan.GetPerf(); //DO NOT REMOVE
        }

        #endregion

        #region Public and Protected Members

        private void LoadData()
        {
            foreach (Track t in viewModel.SelectedBank.tracks)
            {
                if (t.Size > 0)
                {
                    IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication();
                    if (isoStore.FileExists(t.fileName))
                    {
                        System.Diagnostics.Debug.WriteLine("File " + t.fileName + " exists! t's size is " + t.Size);
                        IsolatedStorageFileStream file = isoStore.OpenFile(t.fileName, FileMode.Open);
                        byte[] buffer;
                        using (BinaryReader r = new BinaryReader(file))
                        {
                            buffer = r.ReadBytes(t.Size);
                        }
                        viewModel.AudioMan.LoadTrack(viewModel.SelectedBank.bankID, t.trackID, buffer, t.Size, t.Offset, t.Latency, t.Volume);
                    }
                }
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ((MainViewModel)DataContext).lockUI(MainViewModel.LOCK_STATE.NONE);
            if (!viewModel.SelectedBank.Finalized)
            {
                trackPanel.Visibility = Visibility.Visible;
                BankPanel.Visibility = Visibility.Collapsed;
                if (viewModel.SelectedBank.tracks.Count > 0)
                {
                    LoadData();
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
            else
            {
                trackPanel.Visibility = Visibility.Collapsed;
                BankPanel.Visibility = Visibility.Visible;

                IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication();
                if (isoStore.FileExists(viewModel.SelectedBank.finalTrack))
                {
                    System.Diagnostics.Debug.WriteLine("File " + viewModel.SelectedBank.finalTrack + " exists! t's size is " + viewModel.SelectedBank.Size);
                    IsolatedStorageFileStream file = isoStore.OpenFile(viewModel.SelectedBank.finalTrack, FileMode.Open);
                    byte[] buffer;
                    using (BinaryReader r = new BinaryReader(file))
                    {
                        buffer = r.ReadBytes(viewModel.SelectedBank.Size);
                    }
                    viewModel.AudioMan.LoadBank(viewModel.SelectedBank.bankID, buffer, viewModel.SelectedBank.Size, viewModel.SelectedBank.Offset, viewModel.SelectedBank.Volume, viewModel.SelectedBank.Pitch);
                }
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (!viewModel.SelectedBank.Finalized)
            {
                if (!viewModel.SelectedBank.Finalized)
                {
                    timer.Dispose();
                    recTimer.Dispose();
                    micTimer.Dispose();
                }
                viewModel.AudioMan.StopAll();
                viewModel.AudioMan.StopClick();
            }
            else
            {
                if (!viewModel.SelectedBank.Finalized)
                {
                    timer.Dispose();
                    recTimer.Dispose();
                    micTimer.Dispose();
                    viewModel.AudioMan.StopClick();
                }
                viewModel.AudioMan.StopAll();
            }
            base.OnNavigatingFrom(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
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
            viewModel.AudioMan.StopClick();
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
                        viewModel.SelectedBank.tracks.Add(new Track() { trackID = trackNum });
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
            //FinalizeAnimation.Begin();

            if (viewModel.SelectedBank.tracks.Count > 0)
            {
                int selBank = viewModel.SelectedBank.bankID;
                foreach (Track t in viewModel.SelectedBank.tracks)
                {
                    StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync("bank_" + viewModel.SelectedBank.bankID + "_track_" + t.trackID, CreationCollisionOption.ReplaceExisting);
                    byte[] trackData;
                    int trackLength = viewModel.AudioMan.GetAudioData(viewModel.SelectedBank.bankID, t.trackID, out trackData);
                    using (var s = await file.OpenStreamForWriteAsync())
                    {
                        s.Write(trackData, 0, trackLength);
                    }
                    t.fileName = file.Path;
                    t.Size = trackLength;
                    t.Latency = viewModel.AudioMan.GetTrackLatency(selBank, t.trackID);
                    t.Offset = viewModel.AudioMan.GetOffsetMS(selBank, t.trackID);
                    t.Volume = viewModel.AudioMan.GetVolumeDB(selBank, t.trackID);
                }
            }
        }

        private void metronomeButton_Click(object sender, RoutedEventArgs e)
        {
            if (!timerRunning)
            {
                MeasureAnimation.Begin();
                ((MainViewModel)DataContext).AudioMan.SetClickVolume(1.0f);
                ((MainViewModel)DataContext).AudioMan.PlayClick();
                timer.Change(4000, 4000);
                timerRunning = true;
                ticking = true;
            }
            else
            {
                double value = ((MainViewModel)DataContext).AudioMan.GetClickVolume();
                value = Math.Abs(1.0 - value);
                ((MainViewModel)DataContext).AudioMan.SetClickVolume((float)value);
            }
        }

        private async void finalizeButton_Click(object sender, RoutedEventArgs e)
        {
            timer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            recTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            micTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            MessageBoxResult sure = MessageBox.Show("Are you sure you want to finalize? This will mix your tracks down and delete the individual files.", "Finalize?", MessageBoxButton.OKCancel);
            if (sure == MessageBoxResult.OK)
            {
                //Mix-down and deletion code
                viewModel.AudioMan.MixDownBank(viewModel.SelectedBank.bankID);
                viewModel.SelectedBank.Finalized = true;
                timer.Dispose();
                recTimer.Dispose();
                micTimer.Dispose();
                FinalizeAnimation.Begin();

                StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync("bank_" + viewModel.SelectedBank.bankID + "_final", CreationCollisionOption.ReplaceExisting);
                byte[] trackData;
                int trackLength = viewModel.AudioMan.GetBankAudioData(viewModel.SelectedBank.bankID, out trackData);
                using (var s = await file.OpenStreamForWriteAsync())
                {
                    s.Write(trackData, 0, trackLength);
                }
                viewModel.SelectedBank.finalTrack = file.Path;
                viewModel.SelectedBank.Size = trackLength;
                viewModel.SelectedBank.Pitch = viewModel.AudioMan.GetPitchSemitones(viewModel.SelectedBank.bankID);
                viewModel.SelectedBank.Offset = viewModel.AudioMan.GetBankOffsetMS(viewModel.SelectedBank.bankID);
                viewModel.SelectedBank.Volume = viewModel.AudioMan.GetBankVolumeDB(viewModel.SelectedBank.bankID);
                viewModel.AudioMan.StopClick();
                viewModel.AudioMan.StopAll();
            }
            else
            {
                timer.Change(0, 4000);
            }
        }

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
            viewModel.AudioMan.SetBankVolumeDB(viewModel.SelectedBank.bankID, value);
            viewModel.SelectedBank.Volume = value;
        }

        private void PitchRatioSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            viewModel.AudioMan.SetPitchSemitones(viewModel.SelectedBank.bankID, e.NewValue);
            viewModel.SelectedBank.Pitch = e.NewValue;
        }

        private void OffsetIncreaseButton_Click(object sender, RoutedEventArgs e)
        {
            double value = Convert.ToDouble(OffsetText.Text);
            if (value < 400)
            {
                value += 20.0;
                OffsetText.Text = value.ToString();
                viewModel.AudioMan.SetBankOffsetMS(viewModel.SelectedBank.bankID, value);
                viewModel.SelectedBank.Offset = (int)value;
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
                viewModel.SelectedBank.Offset = (int)value;
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            double zero = 0.0;

            VolumeSlider.Value = zero;
            PitchRatioSlider.Value = zero;
            OffsetText.Text = zero.ToString();

            viewModel.AudioMan.SetBankVolumeDB(viewModel.SelectedBank.bankID, zero);
            viewModel.SelectedBank.Volume = zero;
            viewModel.AudioMan.SetPitchSemitones(viewModel.SelectedBank.bankID, zero);
            viewModel.SelectedBank.Pitch = zero;
            viewModel.AudioMan.SetBankOffsetMS(viewModel.SelectedBank.bankID, zero);
            viewModel.SelectedBank.Offset = (int)zero;
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