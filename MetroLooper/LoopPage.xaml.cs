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

        private Timer timer;
        private Timer recTimer;
        private Timer micTimer;
        private int timingLength = 4000;
        private bool ticking = false;
        private bool startTicking = false;
        private bool recording = false;
        private bool stop = true;
        private bool starting = false;
        public enum LOCK_STATE { RECORDING, ALL, NONE };
        IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
        private bool timerRunning;
        private MainViewModel viewModel;

        public LoopPage()
        {
            InitializeComponent();
            viewModel = MainViewModel.Instance;
            this.DataContext = viewModel;
            this.timer = new Timer(Progress_Go, new object(), System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            this.recTimer = new Timer(CompleteRecord, new object(), System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            this.micTimer = new Timer(StartMic, new object(), System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            timerRunning = false;
        }

        private void StartMic(object state)
        {
            if (recording)
            {
                if (starting) //!stop ||  <--continuous play scenario, remove else
                {
                    starting = false;
                    recording = true;
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
            if (recording)
            {
                if (!starting)
                {
                    Dispatcher.BeginInvoke(delegate
                    {
                        viewModel.SelectedBank.tracks.Add(new Track(viewModel.SelectedBank.tracks.Count, null));
                        viewModel.AudioMan.RecordStopAndSubmit(viewModel.SelectedBank.bankID, viewModel.SelectedBank.tracks.Count);
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
        
        private void Music_Go(object state)
        {
            System.Diagnostics.Debug.WriteLine("Timer ticked, recording is " + recording + ", starting is " + starting + ", and stop is " + stop + ".");
            
            
            {
                Dispatcher.BeginInvoke(delegate
                {
                    foreach (Track t in viewModel.SelectedBank.tracks)
                    {
                        t.Finalized = true;
                    }
                    viewModel.AudioMan.StopAll();
                    viewModel.AudioMan.PlayBank(viewModel.SelectedBank.bankID);
                    viewModel.lockUI(MainViewModel.LOCK_STATE.NONE);
                });
            }
        }
        
        private void Progress_Go(object state)
        {
            Dispatcher.BeginInvoke(delegate
            {
                foreach (Track t in viewModel.SelectedBank.tracks)
                {
                    t.Finalized = true;
                }
                viewModel.AudioMan.StopAll();
                viewModel.AudioMan.PlayBank(viewModel.SelectedBank.bankID);
                MeasureAnimation.Stop();
                MeasureAnimation.Begin();
            });
            //Music_Go(state);
            if (startTicking)
            {
                Dispatcher.BeginInvoke(delegate
                {
                    ((MainViewModel)DataContext).AudioMan.PlayClick();
                });
                startTicking = false;
                ticking = true;
            }
            else if (ticking)
            {
                Dispatcher.BeginInvoke(delegate
                {
                    ((MainViewModel)DataContext).AudioMan.StopClick();
                    ((MainViewModel)DataContext).AudioMan.PlayClick();
                });
            }
        }
         

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ((MainViewModel)DataContext).lockUI(MainViewModel.LOCK_STATE.NONE);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            //StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync("track", CreationCollisionOption.ReplaceExisting);
            //using (var s = await file.OpenStreamForWriteAsync())
            //{
            //    s.Write(new byte[1], 0, 0);
            //}
            MeasureAnimation.Stop();
            viewModel.AudioMan.StopAll();
        }


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
                timer.Change(0, 4000);
                timerRunning = true;
                ((MainViewModel)DataContext).AudioMan.PlayClick();
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
            int size = viewModel.AudioMan.GetAudioData(viewModel.SelectedBank.bankID, viewModel.SelectedTrack.trackID, out data);
            for (int sample = 0; sample < size; sample++)
            {
                System.Diagnostics.Debug.WriteLine(data[sample]);
            }
        }

        private void LongListSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Code to show delete button
            //((Model.Track)e.AddedItems[0]).IsSelected = true;
            //if (e.RemovedItems[0] != null)
            //{
            //    ((Model.Track)e.RemovedItems[0]).IsSelected = false;
            //}
            if (((Track)loopList.SelectedItem) != null)
            {
                viewModel.SelectedTrack = ((Track)loopList.SelectedItem);
                NavigationService.Navigate(new Uri("/TrackPage.xaml", UriKind.RelativeOrAbsolute));
            }
        }

        private void startRecord(bool one)
        {
            //Start timer here
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
    }

    public sealed class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo language)
        {
            return (value is bool && (bool)value) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo language)
        {
            return value is Visibility && (Visibility)value == Visibility.Visible;
        }
    }

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