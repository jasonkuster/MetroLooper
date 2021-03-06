﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.IO.IsolatedStorage;
using System.Collections.ObjectModel;
using MetroLooper.Model;
using MetroLooper.ViewModels;
using System.Threading;
using System.IO;

namespace MetroLooper
{
    public partial class ProjectPage : PhoneApplicationPage
    {

        IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
        MainViewModel viewModel;

        bool play1 = false;
        bool play2 = false;
        bool play3 = false;
        bool play4 = false;
        bool play5 = false;
        bool play6 = false;
        bool isPlaying = false;
        Timer playingTimer;
        Timer stopTimer;
        Dictionary<int,bool> banksToPlay;

        public ProjectPage()
        {
            InitializeComponent();
            viewModel = MainViewModel.Instance;
            this.DataContext = viewModel;

            banksToPlay = new Dictionary<int, bool>();
        }

        private void StopTracks(object state)
        {
            Dispatcher.BeginInvoke(delegate
            {
                MainProgress.Stop();
                viewModel.AudioMan.StopAll();
                Bank1Go.Stop();
                Bank2Go.Stop();
                Bank3Go.Stop();
                Bank4Go.Stop();
                Bank5Go.Stop();
                Bank6Go.Stop();
            });
        }

        private void PlayTracks(object state)
        {
            stopTimer.Change(3950, System.Threading.Timeout.Infinite);
            Dispatcher.BeginInvoke(delegate
            {
                MainProgress.Begin();
                //foreach (int k in banksToPlay.Keys)
                //{
                //    if (banksToPlay[k])
                //    {
                //        viewModel.AudioMan.PlayBank(k);
                //    }
                //}
                if (play1)
                {
                    viewModel.AudioMan.PlayMixedBank(0);
                    Bank1Go.Begin();
                }
                if (play2)
                {
                    viewModel.AudioMan.PlayMixedBank(1);
                    Bank2Go.Begin();
                }
                if (play3)
                {
                    viewModel.AudioMan.PlayMixedBank(2);
                    Bank3Go.Begin();
                }
                if (play4)
                {
                    viewModel.AudioMan.PlayMixedBank(3);
                    Bank4Go.Begin();
                }
                if (play5)
                {
                    viewModel.AudioMan.PlayMixedBank(4);
                    Bank5Go.Begin();
                }
                if (play6)
                {
                    viewModel.AudioMan.PlayMixedBank(5);
                    Bank6Go.Begin();
                }
            });

        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e != null)
            {
                base.OnNavigatedTo(e);
            }

            if (!viewModel.SelectedProject.Initialized)
            {
                viewModel.SelectedProject.Initialized = true;
                NavigationService.RemoveBackEntry();
            }

            playingTimer = new Timer(PlayTracks, new object(), System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            stopTimer = new Timer(StopTracks, new object(), System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);

            play1 = false;
            play2 = false;
            play3 = false;
            play4 = false;
            play5 = false;
            play6 = false;
            playButtonOne.Visibility = System.Windows.Visibility.Visible;
            stopButtonOne.Visibility = System.Windows.Visibility.Collapsed;
            playButtonTwo.Visibility = System.Windows.Visibility.Visible;
            stopButtonTwo.Visibility = System.Windows.Visibility.Collapsed;
            playButtonThree.Visibility = System.Windows.Visibility.Visible;
            stopButtonThree.Visibility = System.Windows.Visibility.Collapsed;
            playButtonFour.Visibility = System.Windows.Visibility.Visible;
            stopButtonFour.Visibility = System.Windows.Visibility.Collapsed;
            playButtonFive.Visibility = System.Windows.Visibility.Visible;
            stopButtonFive.Visibility = System.Windows.Visibility.Collapsed;
            playButtonSix.Visibility = System.Windows.Visibility.Visible;
            stopButtonSix.Visibility = System.Windows.Visibility.Collapsed;

            switch (viewModel.SelectedProject.banks.Count)
            {
                case 0:
                    bankPanel2.Visibility = System.Windows.Visibility.Collapsed;
                    bankPanel3.Visibility = System.Windows.Visibility.Collapsed;
                    bankPanel4.Visibility = System.Windows.Visibility.Collapsed;
                    bankPanel5.Visibility = System.Windows.Visibility.Collapsed;
                    bankPanel6.Visibility = System.Windows.Visibility.Collapsed;
                    //bankPlay1.IsEnabled = false;
                    //bankSlider1.IsEnabled = false;
                    break;
                case 1:
                    //Code to handle one bank
                    playPanel1.Visibility = System.Windows.Visibility.Visible;
                    bank1Name.Text = viewModel.SelectedProject.banks[0].BankName;
                    newButtonOne.Visibility = System.Windows.Visibility.Collapsed;
                    bankPanel2.Visibility = System.Windows.Visibility.Visible;
                    bankPanel3.Visibility = System.Windows.Visibility.Collapsed;
                    bankPanel4.Visibility = System.Windows.Visibility.Collapsed;
                    bankPanel5.Visibility = System.Windows.Visibility.Collapsed;
                    bankPanel6.Visibility = System.Windows.Visibility.Collapsed;
                    //bankPlay1.IsEnabled = true;
                    //bankSlider1.IsEnabled = true;
                    //bankPlay2.IsEnabled = false;
                    //bankSlider2.IsEnabled = false;
                    break;
                case 2:
                    playPanel1.Visibility = System.Windows.Visibility.Visible;
                    bank1Name.Text = viewModel.SelectedProject.banks[0].BankName;
                    newButtonOne.Visibility = System.Windows.Visibility.Collapsed;
                    playPanel2.Visibility = System.Windows.Visibility.Visible;
                    bank2Name.Text = viewModel.SelectedProject.banks[1].BankName;
                    newButtonTwo.Visibility = System.Windows.Visibility.Collapsed;
                    bankPanel2.Visibility = System.Windows.Visibility.Visible;
                    bankPanel3.Visibility = System.Windows.Visibility.Visible;
                    bankPanel4.Visibility = System.Windows.Visibility.Collapsed;
                    bankPanel5.Visibility = System.Windows.Visibility.Collapsed;
                    bankPanel6.Visibility = System.Windows.Visibility.Collapsed;
                    //bankPlay1.IsEnabled = true;
                    //bankSlider1.IsEnabled = true;
                    //bankPlay2.IsEnabled = true;
                    //bankSlider2.IsEnabled = true;
                    //bankPlay3.IsEnabled = false;
                    //bankSlider3.IsEnabled = false;
                    break;
                case 3:
                    playPanel1.Visibility = System.Windows.Visibility.Visible;
                    bank1Name.Text = viewModel.SelectedProject.banks[0].BankName;
                    newButtonOne.Visibility = System.Windows.Visibility.Collapsed;
                    playPanel2.Visibility = System.Windows.Visibility.Visible;
                    bank2Name.Text = viewModel.SelectedProject.banks[1].BankName;
                    newButtonTwo.Visibility = System.Windows.Visibility.Collapsed;
                    playPanel3.Visibility = System.Windows.Visibility.Visible;
                    bank3Name.Text = viewModel.SelectedProject.banks[2].BankName;
                    newButtonThree.Visibility = System.Windows.Visibility.Collapsed;
                    bankPanel2.Visibility = System.Windows.Visibility.Visible;
                    bankPanel3.Visibility = System.Windows.Visibility.Visible;
                    bankPanel4.Visibility = System.Windows.Visibility.Visible;
                    bankPanel5.Visibility = System.Windows.Visibility.Collapsed;
                    bankPanel6.Visibility = System.Windows.Visibility.Collapsed;
                    //bankPlay1.IsEnabled = true;
                    //bankSlider1.IsEnabled = true;
                    //bankPlay2.IsEnabled = true;
                    //bankSlider2.IsEnabled = true;
                    //bankPlay3.IsEnabled = true;
                    //bankSlider3.IsEnabled = true;
                    //bankPlay4.IsEnabled = false;
                    //bankSlider4.IsEnabled = false;
                    break;
                case 4:
                    playPanel1.Visibility = System.Windows.Visibility.Visible;
                    bank1Name.Text = viewModel.SelectedProject.banks[0].BankName;
                    newButtonOne.Visibility = System.Windows.Visibility.Collapsed;
                    playPanel2.Visibility = System.Windows.Visibility.Visible;
                    bank2Name.Text = viewModel.SelectedProject.banks[1].BankName;
                    newButtonTwo.Visibility = System.Windows.Visibility.Collapsed;
                    playPanel3.Visibility = System.Windows.Visibility.Visible;
                    bank3Name.Text = viewModel.SelectedProject.banks[2].BankName;
                    newButtonThree.Visibility = System.Windows.Visibility.Collapsed;
                    playPanel4.Visibility = System.Windows.Visibility.Visible;
                    bank4Name.Text = viewModel.SelectedProject.banks[3].BankName;
                    newButtonFour.Visibility = System.Windows.Visibility.Collapsed;
                    bankPanel2.Visibility = System.Windows.Visibility.Visible;
                    bankPanel3.Visibility = System.Windows.Visibility.Visible;
                    bankPanel4.Visibility = System.Windows.Visibility.Visible;
                    bankPanel5.Visibility = System.Windows.Visibility.Visible;
                    bankPanel6.Visibility = System.Windows.Visibility.Collapsed;
                    //bankPlay1.IsEnabled = true;
                    //bankSlider1.IsEnabled = true;
                    //bankPlay2.IsEnabled = true;
                    //bankSlider2.IsEnabled = true;
                    //bankPlay3.IsEnabled = true;
                    //bankSlider3.IsEnabled = true;
                    //bankPlay4.IsEnabled = true;
                    //bankSlider4.IsEnabled = true;
                    break;
                case 5:
                    playPanel1.Visibility = System.Windows.Visibility.Visible;
                    bank1Name.Text = viewModel.SelectedProject.banks[0].BankName;
                    newButtonOne.Visibility = System.Windows.Visibility.Collapsed;
                    playPanel2.Visibility = System.Windows.Visibility.Visible;
                    bank2Name.Text = viewModel.SelectedProject.banks[1].BankName;
                    newButtonTwo.Visibility = System.Windows.Visibility.Collapsed;
                    playPanel3.Visibility = System.Windows.Visibility.Visible;
                    bank3Name.Text = viewModel.SelectedProject.banks[2].BankName;
                    newButtonThree.Visibility = System.Windows.Visibility.Collapsed;
                    playPanel4.Visibility = System.Windows.Visibility.Visible;
                    bank4Name.Text = viewModel.SelectedProject.banks[3].BankName;
                    newButtonFour.Visibility = System.Windows.Visibility.Collapsed;
                    playPanel5.Visibility = System.Windows.Visibility.Visible;
                    bank5Name.Text = viewModel.SelectedProject.banks[4].BankName;
                    newButtonFive.Visibility = System.Windows.Visibility.Collapsed;
                    bankPanel2.Visibility = System.Windows.Visibility.Visible;
                    bankPanel3.Visibility = System.Windows.Visibility.Visible;
                    bankPanel4.Visibility = System.Windows.Visibility.Visible;
                    bankPanel5.Visibility = System.Windows.Visibility.Visible;
                    bankPanel6.Visibility = System.Windows.Visibility.Visible;
                    break;
                case 6:
                    playPanel1.Visibility = System.Windows.Visibility.Visible;
                    bank1Name.Text = viewModel.SelectedProject.banks[0].BankName;
                    newButtonOne.Visibility = System.Windows.Visibility.Collapsed;
                    playPanel2.Visibility = System.Windows.Visibility.Visible;
                    bank2Name.Text = viewModel.SelectedProject.banks[1].BankName;
                    newButtonTwo.Visibility = System.Windows.Visibility.Collapsed;
                    playPanel3.Visibility = System.Windows.Visibility.Visible;
                    bank3Name.Text = viewModel.SelectedProject.banks[2].BankName;
                    newButtonThree.Visibility = System.Windows.Visibility.Collapsed;
                    playPanel4.Visibility = System.Windows.Visibility.Visible;
                    bank4Name.Text = viewModel.SelectedProject.banks[3].BankName;
                    newButtonFour.Visibility = System.Windows.Visibility.Collapsed;
                    playPanel5.Visibility = System.Windows.Visibility.Visible;
                    bank5Name.Text = viewModel.SelectedProject.banks[4].BankName;
                    newButtonFive.Visibility = System.Windows.Visibility.Collapsed;
                    playPanel6.Visibility = System.Windows.Visibility.Visible;
                    bank6Name.Text = viewModel.SelectedProject.banks[5].BankName;
                    newButtonSix.Visibility = System.Windows.Visibility.Collapsed;
                    bankPanel2.Visibility = System.Windows.Visibility.Visible;
                    bankPanel3.Visibility = System.Windows.Visibility.Visible;
                    bankPanel4.Visibility = System.Windows.Visibility.Visible;
                    bankPanel5.Visibility = System.Windows.Visibility.Visible;
                    bankPanel6.Visibility = System.Windows.Visibility.Visible;
                    break;
                default:
                    break;
            }

            foreach (Bank b in viewModel.SelectedProject.banks)
            {
                if (!b.Finalized)
                {
                    foreach (Track t in b.tracks)
                    {
                        if (t.Size > 0)
                        {
                            viewModel.AudioMan.LoadTrack(b.bankID, t.trackID, t.trackData, t.Size, t.Offset, t.Latency, t.Volume);
                        }
                    }
                }
                else
                {
                    viewModel.AudioMan.LoadBank(b.bankID, b.finalTrack, b.Size, b.Offset, b.Volume, b.Pitch);
                }
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            playingTimer.Dispose();
            stopTimer.Dispose();
            viewModel.AudioMan.StopAll();
            isPlaying = false;
        }

        //private void Border_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        //{
        //    var borderNum = sender as Border;
        //    switch (borderNum.Name)
        //    {
        //        case "bank1Border":
        //            if (viewModel.SelectedProject.banks.Count == 0)
        //            {
        //                viewModel.SelectedProject.banks.Add(new Bank { bankID = 0 });
        //            }
        //            viewModel.SelectedBank = viewModel.SelectedProject.banks[0];
        //            break;
        //        case "bank2Border":
        //            if (viewModel.SelectedProject.banks.Count == 1)
        //            {
        //                viewModel.SelectedProject.banks.Add(new Bank { bankID = 1 });
        //            }
        //            viewModel.SelectedBank = viewModel.SelectedProject.banks[1];
        //            break;
        //        case "bank3Border":
        //            if (viewModel.SelectedProject.banks.Count == 2)
        //            {
        //                viewModel.SelectedProject.banks.Add(new Bank { bankID = 2 });
        //            }
        //            viewModel.SelectedBank = viewModel.SelectedProject.banks[2];
        //            break;
        //        case "bank4Border":
        //            if (viewModel.SelectedProject.banks.Count == 3)
        //            {
        //                viewModel.SelectedProject.banks.Add(new Bank { bankID = 3 });
        //            }
        //            viewModel.SelectedBank = viewModel.SelectedProject.banks[3];
        //            break;
        //        default:
        //            break;
        //    }
        //    NavigationService.Navigate(new Uri("/BankPage.xaml", UriKind.RelativeOrAbsolute));
        //}

        private void bankPlay_Click(object sender, RoutedEventArgs e)
        {
            var playButton = sender as Button;
            switch (playButton.Name)
            {
                case "bankPlay1":
                    play1 = play1 ? false : true;
                    break;
                case "bankPlay2":
                    play2 = play2 ? false : true;
                    break;
                case "bankPlay3":
                    play3 = play3 ? false : true;
                    break;
                case "bankPlay4":
                    play4 = play4 ? false : true;
                    break;
                default:
                    break;
            }
            playButton.Content = ((string)(playButton.Content)) == "Play" ? "Stop" : "Play";
            if (!isPlaying)
            {
                playingTimer.Change(0, 4000);
                isPlaying = true;
            }
        }

        private void bankSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double value = e.NewValue;
            if (value < -35.0)
            {
                value = -120;
            }
            int bankNumber = -1;
            var sliderNum = sender as Slider;
            switch (sliderNum.Name)
            {
                case "volumeSliderOne":
                    bankNumber = 0;
                    break;
                case "volumeSliderTwo":
                    bankNumber = 1;
                    break;
                case "volumeSliderThree":
                    bankNumber = 2;
                    break;
                case "volumeSliderFour":
                    bankNumber = 3;
                    break;
                case "volumeSliderFive":
                    bankNumber = 4;
                    break;
                case "volumeSliderSix":
                    bankNumber = 5;
                    break;
                default:
                    break;
            }
            viewModel.AudioMan.SetBankVolumeDB(bankNumber, value);
            viewModel.SelectedBank.Volume = value;
        }

        private void stopAllButton_Click(object sender, RoutedEventArgs e)
        {
            //play1 = false;
            //bankPlay1.Content = "Play";
            //play2 = false;
            //bankPlay2.Content = "Play";
            //play3 = false;
            //bankPlay3.Content = "Play";
            //play4 = false;
            //bankPlay4.Content = "Play";
            //viewModel.AudioMan.StopAll();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            settings["projects"] = new ObservableCollection<Project>();
            ((ObservableCollection<Project>)settings["projects"]).Add(new Project() { projName = "MyProject", bpm = 120, measures = 2 });
            viewModel.SelectedProject = ((ObservableCollection<Project>)settings["projects"])[0];
            IsolatedStorageSettings.ApplicationSettings.Save();
            OnNavigatedTo(null);

            viewModel.AudioMan.ResetAll();
        }

        private void playButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            PlayBank1.Begin();
        }

        private void swapButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            //viewModel.AudioMan.TestExport("testWAV.wav");
            ContentPanel.Visibility = ContentPanel.Visibility == System.Windows.Visibility.Collapsed ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            ContentPanel2.Visibility = ContentPanel2.Visibility == System.Windows.Visibility.Collapsed ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
        }

        private void editImageOne_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/BankPage.xaml", UriKind.RelativeOrAbsolute));
        }

        private void playButton_Tap_1(object sender, System.Windows.Input.GestureEventArgs e)
        {
            //if (((Bank)BankList.SelectedItem) != null)
            //{
            //    if (banksToPlay.ContainsKey(((Bank)BankList.SelectedItem).bankID))
            //    {
            //        banksToPlay[((Bank)BankList.SelectedItem).bankID] = banksToPlay[((Bank)BankList.SelectedItem).bankID] == true ? false : true;
            //    }
            //    else
            //    {
            //        banksToPlay[((Bank)BankList.SelectedItem).bankID] = true;
            //    }
            //    playingTimer.Change(0, 4000);
            //}
            var clicker = sender as Border;
            
            switch (clicker.Name.Remove(0,4))
            {
                case "ButtonOne":
                    play1 = play1 ? false : true;
                    playButtonOne.Visibility = play1 ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
                    stopButtonOne.Visibility = play1 ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                    break;
                case "ButtonTwo":
                    play2 = play2 ? false : true;
                    playButtonTwo.Visibility = play2 ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
                    stopButtonTwo.Visibility = play2 ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                    break;
                case "ButtonThree":
                    play3 = play3 ? false : true;
                    playButtonThree.Visibility = play3 ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
                    stopButtonThree.Visibility = play3 ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                    break;
                case "ButtonFour":
                    play4 = play4 ? false : true;
                    playButtonFour.Visibility = play4 ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
                    stopButtonFour.Visibility = play4 ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                    break;
                case "ButtonFive":
                    play5 = play5 ? false : true;
                    playButtonFive.Visibility = play5 ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
                    stopButtonFive.Visibility = play5 ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                    break;
                case "ButtonSix":
                    play6 = play6 ? false : true;
                    playButtonSix.Visibility = play6 ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
                    stopButtonSix.Visibility = play6 ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                    break;
                default:
                    break;
            }
            if (!isPlaying)
            {
                playingTimer.Change(0, 4000);
                isPlaying = true;
            }
        }

        private void editButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            //if (((Bank)BankList.SelectedItem) != null)
            //{
            //    if (!((Bank)BankList.SelectedItem).Initialized)
            //    {
            //        ((Bank)BankList.SelectedItem).Initialized = true;
            //        viewModel.SelectedProject.banks.Add(new Bank() { bankID = viewModel.SelectedProject.banks.Count });
            //    }
            //    viewModel.SelectedBank = ((Bank)BankList.SelectedItem);
            //    BankList.SelectedItem = null;
            //    NavigationService.Navigate(new Uri("/BankPage.xaml", UriKind.RelativeOrAbsolute));
            //}
            Border b = sender as Border;
            int buttonNum = -1;
            switch (b.Name)
            {
                case "editButtonOne":
                    buttonNum = 0;
                    break;
                case "editButtonTwo":
                    buttonNum = 1;
                    break;
                case "editButtonThree":
                    buttonNum = 2;
                    break;
                case "editButtonFour":
                    buttonNum = 3;
                    break;
                case "editButtonFive":
                    buttonNum = 4;
                    break;
                case "editButtonSix":
                    buttonNum = 5;
                    break;
                default:
                    buttonNum = viewModel.SelectedProject.banks.Count;
                    Bank newBank = new Bank() { bankID = viewModel.SelectedProject.banks.Count, Finalized = false, Initialized = false };
                    viewModel.SelectedProject.banks.Add(newBank);
                    viewModel.SelectedBank = newBank;
                    break;
            }

            viewModel.SelectedBank = viewModel.SelectedProject.banks[buttonNum];

            NavigationService.Navigate(new Uri("/BankPage.xaml", UriKind.RelativeOrAbsolute));
        }

        private void deleteButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            //if (((Bank)BankList.SelectedItem) != null)
            //{
            //    if (((Bank)BankList.SelectedItem).Finalized)
            //    {
            //        viewModel.AudioMan.DeleteFinalizedBank(((Bank)BankList.SelectedItem).bankID);
            //    }
            //    else //TODO: FIX SO THAT DELETING BANK 2/4 DOESN'T BREAK EVERYTHING
            //    {
            //        foreach (Track t in ((Bank)BankList.SelectedItem).tracks) //TODO: DECREMENT EVERY BANK AFTERWARD
            //        {
            //            viewModel.AudioMan.DeleteTrack(((Bank)BankList.SelectedItem).bankID, t.trackID);
            //        }
            //    }
            //    viewModel.SelectedProject.banks.Remove(((Bank)BankList.SelectedItem));
            //    BankList.SelectedItem = null;
            //}


        }

        private void ComposeButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (Bank b in viewModel.SelectedProject.banks)
            {
                viewModel.AudioMan.MixDownBank(b.bankID);
            }
            NavigationService.Navigate(new Uri("/ComposePage.xaml", UriKind.RelativeOrAbsolute));
        }
    }
}