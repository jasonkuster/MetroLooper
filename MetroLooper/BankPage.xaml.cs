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
    public partial class BankPage : PhoneApplicationPage
    {

        IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
        MainViewModel viewModel;
        string selectedProjString = "";
        int selectedProj = -1;

        bool play1 = false;
        bool play2 = false;
        bool play3 = false;
        bool play4 = false;
        bool isPlaying = false;
        Timer playingTimer;

        public BankPage()
        {
            InitializeComponent();
            viewModel = MainViewModel.Instance;
            this.DataContext = viewModel;
            //if (!settings.Contains("projects"))
            {
                settings["projects"] = new ObservableCollection<Project>();
                ((ObservableCollection<Project>)settings["projects"]).Add(new Project("Project One"));
                ((ObservableCollection<Project>)settings["projects"])[0].banks.Add(new Bank() { bankID = 0 });
            }
            viewModel.SelectedProject = ((ObservableCollection<Project>)settings["projects"])[0];
            viewModel.SelectedBank = viewModel.SelectedProject.banks[0];
            IsolatedStorageSettings.ApplicationSettings.Save();
        }

        private void PlayTracks(object state)
        {
            Dispatcher.BeginInvoke(delegate
            {
                MainProgress.Stop();
                MainProgress.Begin();
                viewModel.AudioMan.StopAll();
                if (play1)
                {
                    viewModel.AudioMan.PlayBank(0);
                    Bank1Go.Begin();
                }
                if (play2)
                {
                    viewModel.AudioMan.PlayBank(1);
                    Bank2Go.Begin();
                }
                if (play3)
                {
                    viewModel.AudioMan.PlayBank(2);
                    Bank3Go.Begin();
                }
                if (play4)
                {
                    viewModel.AudioMan.PlayBank(3);
                    Bank4Go.Begin();
                }
            });

        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            playingTimer = new Timer(PlayTracks, new object(), System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);

            play1 = false;
            play2 = false;
            play3 = false;
            play4 = false;

            switch (viewModel.SelectedProject.banks.Count)
            {
                case 0:
                    bankPanel2.Visibility = System.Windows.Visibility.Collapsed;
                    bankPanel3.Visibility = System.Windows.Visibility.Collapsed;
                    bankPanel4.Visibility = System.Windows.Visibility.Collapsed;
                    bankPlay1.IsEnabled = false;
                    bankSlider1.IsEnabled = false;
                    break;
                case 1:
                    //Code to handle one bank
                    bankPanel2.Visibility = System.Windows.Visibility.Visible;
                    bankPanel3.Visibility = System.Windows.Visibility.Collapsed;
                    bankPanel4.Visibility = System.Windows.Visibility.Collapsed;
                    bankPlay2.IsEnabled = false;
                    bankSlider2.IsEnabled = false;
                    break;
                case 2:
                    bankPanel2.Visibility = System.Windows.Visibility.Visible;
                    bankPanel3.Visibility = System.Windows.Visibility.Visible;
                    bankPanel4.Visibility = System.Windows.Visibility.Collapsed;
                    bankPlay2.IsEnabled = true;
                    bankSlider2.IsEnabled = true;
                    bankPlay3.IsEnabled = false;
                    bankSlider3.IsEnabled = false;
                    break;
                case 3:
                    bankPanel2.Visibility = System.Windows.Visibility.Visible;
                    bankPanel3.Visibility = System.Windows.Visibility.Visible;
                    bankPanel4.Visibility = System.Windows.Visibility.Visible;
                    bankPlay2.IsEnabled = true;
                    bankSlider2.IsEnabled = true;
                    bankPlay3.IsEnabled = true;
                    bankSlider3.IsEnabled = true;
                    bankPlay4.IsEnabled = false;
                    bankSlider4.IsEnabled = false;
                    break;
                case 4:
                    bankPlay2.IsEnabled = true;
                    bankSlider2.IsEnabled = true;
                    bankPlay3.IsEnabled = true;
                    bankSlider3.IsEnabled = true;
                    bankPlay4.IsEnabled = true;
                    bankSlider4.IsEnabled = true;
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
                                viewModel.AudioMan.LoadTrack(b.bankID, t.trackID, buffer, t.Size, t.Offset, t.Latency, t.Volume);
                            }
                        }
                    }
                }
                else
                {
                    IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication();
                    if (isoStore.FileExists(b.finalTrack))
                    {
                        System.Diagnostics.Debug.WriteLine("File " + b.finalTrack + " exists! t's size is " + b.Size);
                        IsolatedStorageFileStream file = isoStore.OpenFile(b.finalTrack, FileMode.Open);
                        byte[] buffer;
                        using (BinaryReader r = new BinaryReader(file))
                        {
                            buffer = r.ReadBytes(viewModel.SelectedBank.Size);
                        }
                        viewModel.AudioMan.LoadBank(b.bankID, buffer, b.Size, b.Offset, b.Volume, b.Pitch);
                    }
                }
            }

            

            //int bankIndex = 0;
            //foreach (UIElement ctrl in ContentPanel.Children)
            //{
            //    if (ctrl.GetType() == typeof(StackPanel) && ((StackPanel)ctrl).Orientation == System.Windows.Controls.Orientation.Horizontal)
            //    {
            //        if (bankIndex < ((Project)(this.DataContext)).banks.Count)
            //        {
            //            ((StackPanel)ctrl).DataContext = ((Project)(this.DataContext)).banks[bankIndex];
            //            bankIndex++;
            //        }
            //        else
            //        {
            //            break;
            //        }
            //    }
            //}
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
            NavigationService.Navigate(new Uri("/LoopPage.xaml", UriKind.RelativeOrAbsolute));
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
        }
    }
}