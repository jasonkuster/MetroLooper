﻿using System;
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

namespace MetroLooper
{
    public partial class LoopPage : PhoneApplicationPage
    {

        private Timer timer;
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
            timerRunning = false;
        }
        
        private void Music_Go(object state)
        {
            System.Diagnostics.Debug.WriteLine("Timer ticked, recording is " + recording + ", starting is " + starting + ", and stop is " + stop + ".");
            if (recording)
            {
                if (!starting)
                {
                    viewModel.AudioMan.RecordStopAndSubmit(viewModel.SelectedBank.bankID, viewModel.SelectedBank.tracks.Count);
                    viewModel.SelectedBank.tracks.Add(new Track(viewModel.SelectedBank.tracks.Count, null));
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
            if (startTicking)
            {
                Dispatcher.BeginInvoke(delegate
                {
                    ((MainViewModel)DataContext).AudioMan.PlayClick();
                });
                startTicking = false;
                ticking = true;
            }
            //else if (ticking)
            //{
            //    ((MainViewModel)DataContext).AudioMan.StopClick();
            //    ((MainViewModel)DataContext).AudioMan.PlayClick();
            //}
            Dispatcher.BeginInvoke(delegate
            {
                MeasureAnimation.Stop();
                MeasureAnimation.Begin();
            });
        }
         

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ((MainViewModel)DataContext).lockUI(MainViewModel.LOCK_STATE.NONE);
        }

        protected async override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync("track", CreationCollisionOption.ReplaceExisting);
            using (var s = await file.OpenStreamForWriteAsync())
            {
                s.Write(new byte[1], 0, 0);
            }
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