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
    public partial class ComposePage : PhoneApplicationPage
    {

        IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
        MainViewModel viewModel;

        bool play1 = false;
        bool play2 = false;
        bool play3 = false;
        bool play4 = false;
        bool play5 = false;
        bool play6 = false;
        bool recording = false;
        bool isPlaying = false;
        Timer playingTimer;
        Timer stopTimer;
        bool[][] instructions;
        int currentMeasure;

        public ComposePage()
        {
            InitializeComponent();
            viewModel = MainViewModel.Instance;
            this.DataContext = viewModel;
            currentMeasure = 0;

            instructions = new bool[6][];
            for (int i = 0; i < 6; i++)
            {
                instructions[i] = new bool[60];
                for (int j = 0; j < 60; j++)
                {
                    instructions[i][j] = false;
                }
            }
        }

        private void StopTracks(object state)
        {
            Dispatcher.BeginInvoke(delegate
            {
                MainProgress.Stop();
                viewModel.AudioMan.StopAll();
                PlayBank1.Stop();
                PlayBank2.Stop();
                PlayBank3.Stop();
                PlayBank4.Stop();
                PlayBank5.Stop();
                PlayBank6.Stop();
            });
        }

        private void PlayTracks(object state)
        {
            stopTimer.Change(3950, System.Threading.Timeout.Infinite);
            Dispatcher.BeginInvoke(delegate
            {
                MainProgress.Begin();
                if (play1)
                {
                    viewModel.AudioMan.PlayMixedBank(0);
                    PlayBank1.Begin();
                    if (recording)
                    {
                        instructions[0][currentMeasure] = true;
                    }
                }
                if (play2)
                {
                    viewModel.AudioMan.PlayMixedBank(1);
                    PlayBank2.Begin();
                    if (recording)
                    {
                        instructions[1][currentMeasure] = true;
                    }
                }
                if (play3)
                {
                    viewModel.AudioMan.PlayMixedBank(2);
                    PlayBank3.Begin();
                    if (recording)
                    {
                        instructions[2][currentMeasure] = true;
                    }
                }
                if (play4)
                {
                    viewModel.AudioMan.PlayMixedBank(3);
                    PlayBank4.Begin();

                    if (recording)
                    {
                        instructions[3][currentMeasure] = true;
                    }
                }
                if (play5)
                {
                    viewModel.AudioMan.PlayMixedBank(4);
                    PlayBank5.Begin();

                    if (recording)
                    {
                        instructions[4][currentMeasure] = true;
                    }
                }
                if (play6)
                {
                    viewModel.AudioMan.PlayMixedBank(5);
                    PlayBank6.Begin();

                    if (recording)
                    {
                        instructions[5][currentMeasure] = true;
                    }
                }

                if (recording)
                {
                    currentMeasure++;
                }
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

            play1 = false;
            play2 = false;
            play3 = false;
            play4 = false;
            play5 = false;
            play6 = false;
            playButton1.Visibility = System.Windows.Visibility.Visible;
            playButton2.Visibility = System.Windows.Visibility.Visible;
            playButton3.Visibility = System.Windows.Visibility.Visible;
            playButton4.Visibility = System.Windows.Visibility.Visible;
            playButton5.Visibility = System.Windows.Visibility.Visible;
            playButton6.Visibility = System.Windows.Visibility.Visible;
            stopButton1.Visibility = System.Windows.Visibility.Collapsed;
            stopButton2.Visibility = System.Windows.Visibility.Collapsed;
            stopButton3.Visibility = System.Windows.Visibility.Collapsed;
            stopButton4.Visibility = System.Windows.Visibility.Collapsed;
            stopButton5.Visibility = System.Windows.Visibility.Collapsed;
            stopButton6.Visibility = System.Windows.Visibility.Collapsed;

            switch (viewModel.SelectedProject.banks.Count)
            {
                case 0:
                    bank1Panel.Visibility = System.Windows.Visibility.Collapsed;
                    bank2Panel.Visibility = System.Windows.Visibility.Collapsed;
                    bank3Panel.Visibility = System.Windows.Visibility.Collapsed;
                    bank4Panel.Visibility = System.Windows.Visibility.Collapsed;
                    bank5Panel.Visibility = System.Windows.Visibility.Collapsed;
                    bank6Panel.Visibility = System.Windows.Visibility.Collapsed;
                    break;
                case 1:
                    //Code to handle one bank
                    bank1Panel.Visibility = System.Windows.Visibility.Visible;
                    nameBlock1.Text = viewModel.SelectedProject.banks[0].BankName;
                    bank2Panel.Visibility = System.Windows.Visibility.Collapsed;
                    bank3Panel.Visibility = System.Windows.Visibility.Collapsed;
                    bank4Panel.Visibility = System.Windows.Visibility.Collapsed;
                    bank5Panel.Visibility = System.Windows.Visibility.Collapsed;
                    bank6Panel.Visibility = System.Windows.Visibility.Collapsed;
                    break;
                case 2:
                    bank1Panel.Visibility = System.Windows.Visibility.Visible;
                    nameBlock1.Text = viewModel.SelectedProject.banks[0].BankName;
                    bank2Panel.Visibility = System.Windows.Visibility.Visible;
                    nameBlock2.Text = viewModel.SelectedProject.banks[1].BankName;
                    bank3Panel.Visibility = System.Windows.Visibility.Collapsed;
                    bank4Panel.Visibility = System.Windows.Visibility.Collapsed;
                    bank5Panel.Visibility = System.Windows.Visibility.Collapsed;
                    bank6Panel.Visibility = System.Windows.Visibility.Collapsed;
                    break;
                case 3:
                    bank1Panel.Visibility = System.Windows.Visibility.Visible;
                    nameBlock1.Text = viewModel.SelectedProject.banks[0].BankName;
                    bank2Panel.Visibility = System.Windows.Visibility.Visible;
                    nameBlock2.Text = viewModel.SelectedProject.banks[1].BankName;
                    bank3Panel.Visibility = System.Windows.Visibility.Visible;
                    nameBlock3.Text = viewModel.SelectedProject.banks[2].BankName;
                    bank4Panel.Visibility = System.Windows.Visibility.Collapsed;
                    bank5Panel.Visibility = System.Windows.Visibility.Collapsed;
                    bank6Panel.Visibility = System.Windows.Visibility.Collapsed;
                    break;
                case 4:
                    bank1Panel.Visibility = System.Windows.Visibility.Visible;
                    nameBlock1.Text = viewModel.SelectedProject.banks[0].BankName;
                    bank2Panel.Visibility = System.Windows.Visibility.Visible;
                    nameBlock2.Text = viewModel.SelectedProject.banks[1].BankName;
                    bank3Panel.Visibility = System.Windows.Visibility.Visible;
                    nameBlock3.Text = viewModel.SelectedProject.banks[2].BankName;
                    bank4Panel.Visibility = System.Windows.Visibility.Visible;
                    nameBlock4.Text = viewModel.SelectedProject.banks[3].BankName;
                    bank5Panel.Visibility = System.Windows.Visibility.Collapsed;
                    bank6Panel.Visibility = System.Windows.Visibility.Collapsed;
                    break;
                case 5:
                    bank1Panel.Visibility = System.Windows.Visibility.Visible;
                    nameBlock1.Text = viewModel.SelectedProject.banks[0].BankName;
                    bank2Panel.Visibility = System.Windows.Visibility.Visible;
                    nameBlock2.Text = viewModel.SelectedProject.banks[1].BankName;
                    bank3Panel.Visibility = System.Windows.Visibility.Visible;
                    nameBlock3.Text = viewModel.SelectedProject.banks[2].BankName;
                    bank4Panel.Visibility = System.Windows.Visibility.Visible;
                    nameBlock4.Text = viewModel.SelectedProject.banks[3].BankName;
                    bank5Panel.Visibility = System.Windows.Visibility.Visible;
                    nameBlock5.Text = viewModel.SelectedProject.banks[4].BankName;
                    bank6Panel.Visibility = System.Windows.Visibility.Collapsed;
                    break;
                case 6:
                    bank1Panel.Visibility = System.Windows.Visibility.Visible;
                    nameBlock1.Text = viewModel.SelectedProject.banks[0].BankName;
                    bank2Panel.Visibility = System.Windows.Visibility.Visible;
                    nameBlock2.Text = viewModel.SelectedProject.banks[1].BankName;
                    bank3Panel.Visibility = System.Windows.Visibility.Visible;
                    nameBlock3.Text = viewModel.SelectedProject.banks[2].BankName;
                    bank4Panel.Visibility = System.Windows.Visibility.Visible;
                    nameBlock4.Text = viewModel.SelectedProject.banks[3].BankName;
                    bank5Panel.Visibility = System.Windows.Visibility.Visible;
                    nameBlock5.Text = viewModel.SelectedProject.banks[4].BankName;
                    bank6Panel.Visibility = System.Windows.Visibility.Visible;
                    nameBlock6.Text = viewModel.SelectedProject.banks[5].BankName;
                    break;
                default:
                    break;
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            playingTimer.Dispose();
            stopTimer.Dispose();
            MainProgress.Stop();
            PlayBank1.Stop();
            PlayBank2.Stop();
            PlayBank3.Stop();
            PlayBank4.Stop();
            PlayBank5.Stop();
            PlayBank6.Stop();
            viewModel.AudioMan.StopAll();
            isPlaying = false;
        }

        private void bankPlay_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var playButton = sender as Image;
            switch (playButton.Name.Remove(0,4))
            {
                case "Button1":
                    play1 = play1 ? false : true;
                    playButton1.Visibility = play1 ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
                    stopButton1.Visibility = play1 ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                    break;
                case "Button2":
                    play2 = play2 ? false : true;
                    playButton2.Visibility = play2 ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
                    stopButton2.Visibility = play2 ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                    break;
                case "Button3":
                    play3 = play3 ? false : true;
                    playButton3.Visibility = play3 ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
                    stopButton3.Visibility = play3 ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                    break;
                case "Button4":
                    play4 = play4 ? false : true;
                    playButton4.Visibility = play4 ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
                    stopButton4.Visibility = play4 ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                    break;
                case "Button5":
                    play5 = play5 ? false : true;
                    playButton5.Visibility = play5 ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
                    stopButton6.Visibility = play5 ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                    break;
                case "Button6":
                    play6 = play6 ? false : true;
                    playButton6.Visibility = play6 ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
                    stopButton6.Visibility = play6 ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
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

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (recording)
            {
                playingTimer.Dispose();
                stopTimer.Dispose();
                isPlaying = false;
                recording = false;

                play1 = false;
                play2 = false;
                play3 = false;
                play4 = false;
                play5 = false;
                play6 = false;
                MainProgress.Stop();
                PlayBank1.Stop();
                PlayBank2.Stop();
                PlayBank3.Stop();
                PlayBank4.Stop();
                PlayBank5.Stop();
                PlayBank6.Stop();
                viewModel.AudioMan.StopAll();

                int numMeasures = currentMeasure - 1;
                //string fileName = FileNameTextBox.Text;
                string fileName = "ML_Performance_" + DateTime.Now.ToShortDateString();
                if (string.IsNullOrEmpty(fileName) || string.IsNullOrWhiteSpace(fileName))
                {
                    fileName = "MyWave";
                }
                fileName += ".wav";
                viewModel.AudioMan.ExportAndUpload(instructions, viewModel.SelectedProject.banks.Count, numMeasures, fileName);

                StartButton.Content = "Start";

                NavigationService.GoBack();
            }
            recording = true;
            if (!isPlaying)
            {
                StartButton.Content = "Finalize";
                playingTimer.Change(0, 4000);
                isPlaying = true;
                recording = true;
            }
        }
    }
}