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
using System.Windows.Data;

namespace MetroLooper
{
    public partial class LoopPage : PhoneApplicationPage
    {

        private Timer timer;
        private Timer metronome;
        private bool ticking;
        private bool recording;
        private bool stop;
        private bool starting;
        private int met = 0;
        private int count = 1;
        public enum LOCK_STATE { RECORDING, ALL, NONE };
        IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
        private bool timerRunning;

        public LoopPage()
        {
            InitializeComponent();
            this.DataContext = MainViewModel.Instance;
            this.timer = new Timer(Progress_Go, new object(), System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            timerRunning = false;
        }

        
        private void Music_Go(object state)
        {
            System.Diagnostics.Debug.WriteLine("Timer ticked, recording is " + recording + ", starting is " + starting + ", and stop is " + stop + ".");
            if (recording)
            {
                if (!starting)
                {
                    //Finalize
                    //Recorder.stop()
                    //loopList.ItemsSource = null;
                    //loopList.ItemsSource = ((Bank)this.DataContext).tracks;
                    //System.Diagnostics.Debug.WriteLine("Tracks contains " + ((Bank)this.DataContext).tracks.Count + " items.");
                    Dispatcher.BeginInvoke(delegate
                    {
                        ((MainViewModel)DataContext).SelectedBank.tracks.Add(new Model.Track("the Track", null));
                    });
                    recording = false;
                    if (stop)
                    {
                        Dispatcher.BeginInvoke(delegate
                        {
                            ((MainViewModel)DataContext).lockUI(MainViewModel.LOCK_STATE.NONE);
                        });
                    }
                }
                if (!stop || starting)
                {
                    starting = false;
                    //Recorder.startRecording();
                    recording = true;
                    Dispatcher.BeginInvoke(delegate
                    {
                        ((MainViewModel)DataContext).lockUI(MainViewModel.LOCK_STATE.RECORDING);
                    });
                }
            }
            else
            {
                Dispatcher.BeginInvoke(delegate
                {
                    ((MainViewModel)DataContext).lockUI(MainViewModel.LOCK_STATE.NONE);
                });
            }
        }
        
        private void Progress_Go(object state)
        {
            Music_Go(state);
            Dispatcher.BeginInvoke(delegate
            {
                MeasureAnimation.Stop();
                MeasureAnimation.Begin();
            });
        }
         

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ((MainViewModel)DataContext).Stop = false;
            /*
            string selectedBankString;
            int selectedBank;
            string selectedProjString;
            int selectedProj;
            if (NavigationContext.QueryString.TryGetValue("projSelected", out selectedProjString))
            {
                selectedProj = int.Parse(selectedProjString);
            }
            else
            {
                //freak the fuck out
                //this should only happen in debug
                selectedProj = 0;
            }
            if (NavigationContext.QueryString.TryGetValue("bankSelected", out selectedBankString))
            {
                selectedBank = int.Parse(selectedBankString);
            }
            else
            {
                //freak the fuck out
                //this should only happen in debug
                selectedBank = 0;
            }
            this.DataContext = ((ObservableCollection<Project>)settings["projects"])[selectedProj].banks[selectedBank];
            loopList.ItemsSource = ((Bank)this.DataContext).tracks;
             * */
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }


        private void continueButton_Click(object sender, RoutedEventArgs e)
        {
            startRecord(false);
            //Starts recording, every n measures (converted to seconds via bpm/60 * measures * 4)
            //recording = true;
            //stop = false;
            //starting = true;
            

            //if (ticking)
            //{
            //    //timer.Start();
            //    metronome.;
            //    Music_Go(null);
            //    Progress_Go(null);
            //}

            //((MainViewModel)DataContext).startRecord(false);
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            stopRecord();
            //Stops recording and deletes current track
            //recording = false;
            //stop = true;
            //starting = false;
            //stopButton.IsEnabled = false;

            //((MainViewModel)DataContext).stopRecord();
        }

        private void recOneButton_Click(object sender, RoutedEventArgs e)
        {

            //((MainViewModel)DataContext).startRecord(true);
            startRecord(true);
            //recording = true;
            //stop = true;
            //starting = true;
            //continueButton.IsEnabled = false;
            //recOneButton.IsEnabled = false;
            //if (!metronome.IsEnabled)
            //{
            //    //timer.Start();
            //    metronome.Start();
            //    Music_Go(null, null);
            //    ProgressGo(null, null);
            //}
            //StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync("track", CreationCollisionOption.ReplaceExisting);
            //using (StreamWriter s = new StreamWriter(await file.OpenStreamForWriteAsync()))
            //{
            //    //write data to file
            //}
            //((Bank)(this.DataContext)).tracks.Add(file);
        }

        private void metronomeButton_Click(object sender, RoutedEventArgs e)
        {
            //Play metronome
        }

        private void finalizeButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult sure = MessageBox.Show("Are you sure you want to finalize? This will mix your tracks down and delete the individual files.", "Finalize?", MessageBoxButton.OKCancel);
            if (sure == MessageBoxResult.OK)
            {
                //Mix-down and deletion code
                if (NavigationService.CanGoBack)
                {
                    NavigationService.GoBack();
                }
            }
        }

        private void LongListSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Code to show delete button
            ((Model.Track)e.AddedItems[0]).IsSelected = true;
            if (e.RemovedItems[0] != null)
            {
                ((Model.Track)e.RemovedItems[0]).IsSelected = false;
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
}