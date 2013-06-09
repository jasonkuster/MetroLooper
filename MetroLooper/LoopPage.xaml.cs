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

namespace MetroLooper
{
    public partial class LoopPage : PhoneApplicationPage
    {
        public LoopPage()
        {
            InitializeComponent();
            Bank currBank = new Bank();
            this.DataContext = currBank;
        }

        private void continueButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {

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

        }

        private void finalizeButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void LongListSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}