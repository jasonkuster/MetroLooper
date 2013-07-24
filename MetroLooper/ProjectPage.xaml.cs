using System;
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
        bool isPlaying = false;
        Timer playingTimer;
        Timer stopTimer;
        Dictionary<int,bool> banksToPlay;

        public ProjectPage()
        {
            InitializeComponent();
            viewModel = MainViewModel.Instance;
            this.DataContext = viewModel;
            if (!settings.Contains("projects"))
            {
                settings["projects"] = new ObservableCollection<Project>();
                ((ObservableCollection<Project>)settings["projects"]).Add(new Project() { projName = "MyProject", bpm = 120, measures = 2 });
                ((ObservableCollection<Project>)settings["projects"])[0].banks.Add(new Bank() { bankID = 0 });
            }
            viewModel.SelectedProject = ((ObservableCollection<Project>)settings["projects"])[0];
            //viewModel.SelectedBank = viewModel.SelectedProject.banks[0];
            IsolatedStorageSettings.ApplicationSettings.Save();
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
            });
        }

        private void PlayTracks(object state)
        {
            stopTimer.Change(3950, System.Threading.Timeout.Infinite);
            Dispatcher.BeginInvoke(delegate
            {
                MainProgress.Begin();
                foreach (int k in banksToPlay.Keys)
                {
                    if (banksToPlay[k])
                    {
                        viewModel.AudioMan.PlayBank(k);
                    }
                }
                //if (play1)
                //{
                //    viewModel.AudioMan.PlayBank(0);
                //    Bank1Go.Begin();
                //}
                //if (play2)
                //{
                //    viewModel.AudioMan.PlayBank(1);
                //    Bank2Go.Begin();
                //}
                //if (play3)
                //{
                //    viewModel.AudioMan.PlayBank(2);
                //    Bank3Go.Begin();
                //}
                //if (play4)
                //{
                //    viewModel.AudioMan.PlayBank(3);
                //    Bank4Go.Begin();
                //}
            });

        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e != null)
            {
                base.OnNavigatedTo(e);
            }

            playingTimer = new Timer(PlayTracks, new object(), System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            stopTimer = new Timer(StopTracks, new object(), System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);

            

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
            viewModel.AudioMan.StopAll();
            isPlaying = false;
        }

        private void Border_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var borderNum = sender as Border;
            switch (borderNum.Name)
            {
                case "bank1Border":
                    if (viewModel.SelectedProject.banks.Count == 0)
                    {
                        viewModel.SelectedProject.banks.Add(new Bank { bankID = 0 });
                    }
                    viewModel.SelectedBank = viewModel.SelectedProject.banks[0];
                    break;
                case "bank2Border":
                    if (viewModel.SelectedProject.banks.Count == 1)
                    {
                        viewModel.SelectedProject.banks.Add(new Bank { bankID = 1 });
                    }
                    viewModel.SelectedBank = viewModel.SelectedProject.banks[1];
                    break;
                case "bank3Border":
                    if (viewModel.SelectedProject.banks.Count == 2)
                    {
                        viewModel.SelectedProject.banks.Add(new Bank { bankID = 2 });
                    }
                    viewModel.SelectedBank = viewModel.SelectedProject.banks[2];
                    break;
                case "bank4Border":
                    if (viewModel.SelectedProject.banks.Count == 3)
                    {
                        viewModel.SelectedProject.banks.Add(new Bank { bankID = 3 });
                    }
                    viewModel.SelectedBank = viewModel.SelectedProject.banks[3];
                    break;
                default:
                    break;
            }
            NavigationService.Navigate(new Uri("/BankPage.xaml", UriKind.RelativeOrAbsolute));
        }

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
                case "bankSlider1":
                    bankNumber = 0;
                    break;
                case "bankSlider2":
                    bankNumber = 1;
                    break;
                case "bankSlider3":
                    bankNumber = 2;
                    break;
                case "bankSlider4":
                    bankNumber = 3;
                    break;
                default:
                    break;
            }
            viewModel.AudioMan.SetBankVolumeDB(bankNumber, value);
        }

        private void stopAllButton_Click(object sender, RoutedEventArgs e)
        {
            play1 = false;
            bankPlay1.Content = "Play";
            play2 = false;
            bankPlay2.Content = "Play";
            play3 = false;
            bankPlay3.Content = "Play";
            play4 = false;
            bankPlay4.Content = "Play";
            viewModel.AudioMan.StopAll();
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
            //viewModel.AudioMan.TestExport();
            ContentPanel.Visibility = ContentPanel.Visibility == System.Windows.Visibility.Collapsed ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            ContentPanel2.Visibility = ContentPanel2.Visibility == System.Windows.Visibility.Collapsed ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
        }

        private void editImageOne_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/BankPage.xaml", UriKind.RelativeOrAbsolute));
        }

        private void playButton_Tap_1(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (((Bank)BankList.SelectedItem) != null)
            {
                if (banksToPlay.ContainsKey(((Bank)BankList.SelectedItem).bankID))
                {
                    banksToPlay[((Bank)BankList.SelectedItem).bankID] = banksToPlay[((Bank)BankList.SelectedItem).bankID] == true ? false : true;
                }
                else
                {
                    banksToPlay[((Bank)BankList.SelectedItem).bankID] = true;
                }
                playingTimer.Change(0, 4000);
            }
        }

        private void editButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (((Bank)BankList.SelectedItem) != null)
            {
                if (!((Bank)BankList.SelectedItem).Initialized)
                {
                    ((Bank)BankList.SelectedItem).Initialized = true;
                    viewModel.SelectedProject.banks.Add(new Bank() { bankID = viewModel.SelectedProject.banks.Count });
                }
                viewModel.SelectedBank = ((Bank)BankList.SelectedItem);
                BankList.SelectedItem = null;
                NavigationService.Navigate(new Uri("/BankPage.xaml", UriKind.RelativeOrAbsolute));
            }
        }

        private void deleteButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (((Bank)BankList.SelectedItem) != null)
            {
                if (((Bank)BankList.SelectedItem).Finalized)
                {
                    viewModel.AudioMan.DeleteFinalizedBank(((Bank)BankList.SelectedItem).bankID);
                }
                else //TODO: FIX SO THAT DELETING BANK 2/4 DOESN'T BREAK EVERYTHING
                {
                    foreach (Track t in ((Bank)BankList.SelectedItem).tracks) //TODO: DECREMENT EVERY BANK AFTERWARD
                    {
                        viewModel.AudioMan.DeleteTrack(((Bank)BankList.SelectedItem).bankID, t.trackID);
                    }
                }
                viewModel.SelectedProject.banks.Remove(((Bank)BankList.SelectedItem));
                BankList.SelectedItem = null;
            }
        }
    }
}