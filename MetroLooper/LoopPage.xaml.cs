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
        }

        #endregion

        #region Public and Protected Members

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ((MainViewModel)DataContext).lockUI(MainViewModel.LOCK_STATE.NONE);
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
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            //StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync("track", CreationCollisionOption.ReplaceExisting);
            //using (var s = await file.OpenStreamForWriteAsync())
            //{
            //    s.Write(new byte[1], 0, 0);
            //}
            timer.Dispose();
            recTimer.Dispose();
            micTimer.Dispose();
            viewModel.AudioMan.StopClick();
        }

        #endregion

        #region Private Members

        private Timer timer;
        private Timer recTimer;
        private Timer micTimer;
        private int timingLength = 3900;
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
                    recTimer.Change(4600, System.Threading.Timeout.Infinite);
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
            System.Diagnostics.Debug.WriteLine("Progress_go ticked, recording is " + recording + ", starting is " + starting + ", and stop is " + stop + ".");
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

            micTimer.Change(3500, System.Threading.Timeout.Infinite);
            
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
                ((MainViewModel)DataContext).AudioMan.StopClick();
                ticking = false;
            }
            else
            {
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

            short[] data;
            int size = viewModel.AudioMan.GetAudioData(0, 0, out data);
            for (int sample = 0; sample < size; sample++)
            {
                System.Diagnostics.Debug.WriteLine(data[sample]);
            }
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