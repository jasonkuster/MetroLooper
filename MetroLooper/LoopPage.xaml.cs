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

namespace MetroLooper
{
    public partial class LoopPage : PhoneApplicationPage
    {
        IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;

        public LoopPage()
        {
            InitializeComponent();
            Bank currBank = new Bank();
            this.DataContext = currBank;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            string selectedBankString;
            int selectedBank;
            if (NavigationContext.QueryString.TryGetValue("Edit", out selectedBankString))
            {
                selectedBank = int.Parse(selectedBankString);
            }
            else
            {
                //init code
                selectedBank = 0;
            }
            this.DataContext = ((ObservableCollection<Project>)settings["projects"])[0].banks[selectedBank];
        }


        private void continueButton_Click(object sender, RoutedEventArgs e)
        {
            //Starts recording, every n measures (converted to seconds via bpm/60 * measures * 4)
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            //Stops recording and deletes current track
        }

        async private void recOneButton_Click(object sender, RoutedEventArgs e)
        {
            StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync("track", CreationCollisionOption.ReplaceExisting);
            using (StreamWriter s = new StreamWriter(await file.OpenStreamForWriteAsync()))
            {
                //write data to file
            }
            ((Bank)(this.DataContext)).tracks.Add(file);
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