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
        IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;

        public LoopPage()
        {
            InitializeComponent();
            this.DataContext = MainViewModel.Instance;
            //this.timer = new Timer(Music_Go, new object(), 0, System.Threading.Timeout.Infinite);
            //this.timer = new DispatcherTimer();
            //timer.Interval = TimeSpan.FromMilliseconds(4000);
            //timer.Tick += Music_Go;

            //metronome = new Timer(Progress_Go, new object(), 0, System.Threading.Timeout.Infinite);
            //metronome.Interval = TimeSpan.FromMilliseconds(20);
            //metronome.Tick += ProgressGo;
            //metronome.Tick += Music_Go;

            //ticking = false;
            //recording = false;
            //stop = false;
            //starting = false;

            //settings["projects"] = new ObservableCollection<Project>();
            //((ObservableCollection<Project>)settings["projects"]).Add(new Project("proj 1"));
            //((ObservableCollection<Project>)settings["projects"])[0].banks.Add(new Bank());

        }

        /*
        private void Music_Go(object state)
        {
            System.Diagnostics.Debug.WriteLine("Timer ticked, recording is " + recording + ", starting is " + starting + ", and stop is " + stop + ".");
            if (recording)
            {
                if (!starting)
                {
                    //Finalize
                    //Recorder.stop()
                    ((Bank)this.DataContext).tracks.Add(new Track("Track"));
                    loopList.ItemsSource = null;
                    loopList.ItemsSource = ((Bank)this.DataContext).tracks;
                    System.Diagnostics.Debug.WriteLine("Tracks contains " + ((Bank)this.DataContext).tracks.Count + " items.");
                    recording = false;
                    if (stop)
                    {
                        continueButton.IsEnabled = true;
                        stopButton.IsEnabled = false;
                        recOneButton.IsEnabled = true;
                    }
                }
                if (!stop || starting)
                {
                    starting = false;
                    //Recorder.startRecording();
                    recording = true;
                    if (!stop)
                    {
                        stopButton.IsEnabled = true;
                    }
                }
            }
            else
            {
                continueButton.IsEnabled = true;
                stopButton.IsEnabled = false;
                recOneButton.IsEnabled = true;
            }
        }

        private void Progress_Go(object state)
        {

            if (measureBar.Value < 100 )
            {
                measureBar.Value += 1;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Progress bar reset.");
                measureBar.Value = 0;
            }
        }
         */

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ((MainViewModel)DataContext).stop = true;
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


        private void continueButton_Click(object sender, RoutedEventArgs e)
        {
            //Starts recording, every n measures (converted to seconds via bpm/60 * measures * 4)
            //recording = true;
            //stop = false;
            //starting = true;
            //continueButton.IsEnabled = false;
            //recOneButton.IsEnabled = false;
            //if (ticking)
            //{
            //    //timer.Start();
            //    metronome.;
            //    Music_Go(null);
            //    Progress_Go(null);
            //}

            ((MainViewModel)DataContext).startRecord(false);
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            //Stops recording and deletes current track
            //recording = false;
            //stop = true;
            //starting = false;
            //stopButton.IsEnabled = false;

            ((MainViewModel)DataContext).stopRecord();
        }

        private void recOneButton_Click(object sender, RoutedEventArgs e)
        {

            ((MainViewModel)DataContext).startRecord(true);
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
        }
    }
}