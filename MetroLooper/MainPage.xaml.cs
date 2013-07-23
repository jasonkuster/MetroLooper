using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using MetroLooper.Resources;
using MetroLooper.ViewModels;
using Microsoft.Live;
using Microsoft.Live.Controls;
using Newtonsoft.Json;

namespace MetroLooper
{
    public partial class MainPage : PhoneApplicationPage
    {
        MainViewModel viewModel;
        private LiveConnectClient client;
        int count;

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            viewModel = new MainViewModel();
            this.DataContext = viewModel;

            //_manager = new AudioManager();
            //count = 0;

            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }

        private void Record_Click(object sender, RoutedEventArgs e)
        {
            //if (!_manager.isRecording)
            //{
            //    _manager.RecordAndPlay(0);
            //    RecordStatus.Text = "Recording";
            //}
            //else
            //{
            //    _manager.RecordStopAndSubmit(0, count);
            //    count++;
            //    if (count >= 10)
            //    {
            //        count = 0;
            //    }
            //    RecordStatus.Text = "Not Recording";
            //}
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            //if (!_manager.isPlaying)
            //{
            //    _manager.PlayAll();
            //    PlayStatus.Text = "Playing";
            //}
            //else
            //{
            //    _manager.StopAll();
            //    PlayStatus.Text = "Not Playing";
            //}
        }

        private void Perf_Click(object sender, RoutedEventArgs e)
        {
            //_manager.GetPerf();
        }

        private void projListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void settingsButton_Click(object sender, EventArgs e)
        {

        }

        private void newProjButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/newProjPage.xaml", UriKind.RelativeOrAbsolute));
        }

        // Sample code for building a localized ApplicationBar
        //private void BuildLocalizedApplicationBar()
        //{
        //    // Set the page's ApplicationBar to a new instance of ApplicationBar.
        //    ApplicationBar = new ApplicationBar();

        //    // Create a new button and set the text value to the localized string from AppResources.
        //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
        //    appBarButton.Text = AppResources.AppBarButtonText;
        //    ApplicationBar.Buttons.Add(appBarButton);

        //    // Create a new menu item with the localized string from AppResources.
        //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
        //    ApplicationBar.MenuItems.Add(appBarMenuItem);
        //}

        private async void btnSignin_SessionChanged(object sender, LiveConnectSessionChangedEventArgs e)
        {
            if (e.Status == LiveConnectSessionStatus.Connected)
            {
                client = new LiveConnectClient(e.Session);
                LiveOperationResult fileList = await client.GetAsync("me/skydrive/files");
                //LiveOperationResult operationResult = await client.GetAsync("me");
                try
                {
                    //dynamic meResult = operationResult.Result;
                    //if (meResult.first_name != null &&
                    //    meResult.last_name != null)
                    //{
                    //    infoTextBlock.Text = "Hello " +
                    //        meResult.first_name + " " +
                    //        meResult.last_name + "!";
                    //}
                    //else
                    //{
                    //    infoTextBlock.Text = "Hello, signed-in user!";
                    //}
                    List<object> fileResult = (List<object>)(fileList.Result["data"]);
                    //var result = fileResult.ToString(); //JsonConvert.DeserializeObject<Dictionary<string,string>>(fileResult.ToString());
                    List<string> l = new List<string>();

                    foreach (IDictionary<string,object> k in fileResult)
                    {
                        l.Add(k["name"].ToString());
                    }

                    listselector.ItemsSource = l;
                    //infoTextBlock.Text = result;

                }
                catch (LiveConnectException exception)
                {
                    this.infoTextBlock.Text = "Error calling API: " +
                        exception.Message;
                }
                btnSignin.Visibility = System.Windows.Visibility.Collapsed;
                infoTextBlock.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                infoTextBlock.Text = "Not signed in.";
            }
        }

        private void btnSignin_Click(object sender, RoutedEventArgs e)
        {
            this.SupportedOrientations = SupportedPageOrientation.PortraitOrLandscape;
        }
    }
}