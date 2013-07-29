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
using System.IO.IsolatedStorage;
using System.Collections.ObjectModel;
using MetroLooper.Model;

namespace MetroLooper
{
    public partial class MainPage : PhoneApplicationPage
    {
        MainViewModel viewModel;
        IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
        int count;

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            if (!settings.Contains("projects"))
            {
                settings["projects"] = new ObservableCollection<Project>();
            }

            viewModel = MainViewModel.Instance;
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
            NavigationService.Navigate(new Uri("/SettingsPage.xaml", UriKind.RelativeOrAbsolute));
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

        //private async void btnSignin_SessionChanged(object sender, LiveConnectSessionChangedEventArgs e)
        //{
        //    if (e.Status == LiveConnectSessionStatus.Connected)
        //    {
        //        viewModel.Client = new LiveConnectClient(e.Session);
        //        btnSignin.Visibility = System.Windows.Visibility.Collapsed;
        //        try
        //        {
        //            LiveOperationResult folderResult = await viewModel.Client.GetAsync("me/skydrive/files");
        //            dynamic items = folderResult.Result;
        //            List<object> folderItems = (List<object>)(((IDictionary<string,object>)items)["data"]);
        //            List<Dictionary<string,string>> objs = new List<Dictionary<string,string>>();
        //            foreach (object o in folderItems)
        //            {
        //                IDictionary<string, object> obj = (IDictionary<string, object>)o;
        //                Dictionary<string,string> newobj = new Dictionary<string,string>();
        //                if (!obj["type"].ToString().Equals("folder") && !obj["type"].ToString().Equals("album"))
        //                {
        //                    foreach(string key in obj.Keys)
        //                    {
        //                        if (obj[key] != null)
        //                        {
        //                            newobj.Add(key, obj[key].ToString());
        //                        }
        //                    }
        //                    objs.Add(newobj);
        //                }
        //            }
        //            listselector.ItemsSource = objs;
        //        }
        //        catch (LiveConnectException exception)
        //        {
        //            this.infoTextBlock.Text = "Couldn't get list of projects because: " + exception.Message;
        //        }
        //    }
        //    else
        //    {
        //        this.infoTextBlock.Text = "not signed in";
        //    }
        //}

        //private System.Threading.CancellationTokenSource ctsUpload;

        //private async void saveButton_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        LiveOperationResult clientResult = await viewModel.Client.GetAsync("me/skydrive");
        //        dynamic res = clientResult.Result;
        //        string path = res.id;
        //        //var picker = new Windows.Storage.Pickers.FileOpenPicker();
        //        //picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
        //        //picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
        //        //picker.FileTypeFilter.Add("*");
        //        //Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
        //        //if (file != null)
        //        {
        //            this.progressBar.Value = 0;
        //            var progressHandler = new Progress<LiveOperationProgress>(
        //                (progress) => { this.progressBar.Value = progress.ProgressPercentage; });
        //            this.ctsUpload = new System.Threading.CancellationTokenSource();
        //            await viewModel.Client.BackgroundUploadAsync(path, new Uri("/shared/transfers/12 - Don't Stop Me Now.MP3", UriKind.RelativeOrAbsolute), OverwriteOption.DoNotOverwrite, this.ctsUpload.Token, progressHandler);
        //            //"folder.8c8ce076ca27823f.8C8CE076CA27823F!134","MyUploadedPicture.jpg", file, true, this.ctsUpload.Token, progressHandler);
        //            this.infoTextBlock.Text = "Upload completed.";
        //        }
        //    }
        //    catch (System.Threading.Tasks.TaskCanceledException)
        //    {
        //        this.infoTextBlock.Text = "Upload cancelled.";
        //    }
        //    catch (LiveConnectException exception)
        //    {
        //        this.infoTextBlock.Text = "Error uploading file: " + exception.Message;
        //    }
        //}

        //private void btnCancelUpload_Click(object sender, RoutedEventArgs e)
        //{
        //    if (this.ctsUpload != null)
        //    {
        //        this.ctsUpload.Cancel();
        //    }
        //}

        //private System.Threading.CancellationTokenSource ctsDownload;

        //private async void btnDownloadFile_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        LiveOperationResult clientResult = await viewModel.Client.GetAsync("me/skydrive");
        //        dynamic res = clientResult.Result;
        //        string path = res.id;
        //        //var picker = new Windows.Storage.Pickers.FileSavePicker();
        //        //picker.SuggestedFileName = "MyDownloadedPicutre.jpg";
        //        //picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Downloads;
        //        //picker.FileTypeChoices.Add("Picture", new List<string>(new string[] { ".jpg" }));
        //        //StorageFile file = await picker.PickSaveFileAsync();
        //        //if (file != null)
        //        {
        //            //this.progressBar.Value = 0;
        //            //var progressHandler = new Progress<LiveOperationProgress>(
        //                //(progress) => { this.progressBar.Value = progress.ProgressPercentage; });
        //            //this.ctsDownload = new System.Threading.CancellationTokenSource();
        //            //LiveConnectClient liveClient = new LiveConnectClient(this.session);
        //            await viewModel.Client.BackgroundDownloadAsync(path, new Uri(""));
        //            this.infoTextBlock.Text = "Download completed.";
        //        }
        //    }
        //    catch (System.Threading.Tasks.TaskCanceledException)
        //    {
        //        this.infoTextBlock.Text = "Download cancelled.";
        //    }
        //    catch (LiveConnectException exception)
        //    {
        //        this.infoTextBlock.Text = "Error getting file contents: " + exception.Message;
        //    }
        //}

        //private void btnCancelDownload_Click(object sender, RoutedEventArgs e)
        //{
        //    if (this.ctsDownload != null)
        //    {
        //        this.ctsDownload.Cancel();
        //    }
        //}

        //private void btnSignin_Click(object sender, RoutedEventArgs e)
        //{
        //    this.SupportedOrientations = SupportedPageOrientation.PortraitOrLandscape;
        //}

        private void newProjButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/newProjPage.xaml", UriKind.RelativeOrAbsolute));
        }

        private void openProjButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (viewModel.Projects.Count > 0)
            {
                NavigationService.Navigate(new Uri("/ProjSelectPage.xaml", UriKind.RelativeOrAbsolute));
            }
        }

        private void settingsButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/SettingsPage.xaml", UriKind.RelativeOrAbsolute));
        }
    }
}