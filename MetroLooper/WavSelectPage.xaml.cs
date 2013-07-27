using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using MetroLooper.ViewModels;
using Microsoft.Live;
using Microsoft.Live.Controls;
using MetroLooper.Model;
using System.IO.IsolatedStorage;

namespace MetroLooper
{
    public partial class WavSelectPage : PhoneApplicationPage
    {
        MainViewModel viewModel;

        public WavSelectPage()
        {
            InitializeComponent();
            viewModel = MainViewModel.Instance;
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (viewModel.Client == null)
            {
                NavigationService.GoBack();
            }
            //ProgressBar.IsVisible = true;

            try
            {
                LiveOperationResult folderResult = await viewModel.Client.GetAsync("me/skydrive/files");
                dynamic items = folderResult.Result;
                List<object> folderItems = (List<object>)(((IDictionary<string, object>)items)["data"]);
                List<SkydriveItem> objs = new List<SkydriveItem>();
                foreach (object o in folderItems)
                {
                    IDictionary<string, object> obj = (IDictionary<string, object>)o;
                    Dictionary<string, string> attrs = new Dictionary<string, string>();
                    if (!obj["type"].ToString().Equals("folder") && !obj["type"].ToString().Equals("album") && obj["name"].ToString().EndsWith(".wav"))
                    {
                        foreach (string key in obj.Keys)
                        {
                            if (obj[key] != null)
                            {
                                attrs.Add(key, obj[key].ToString());
                            }
                        }
                        SkydriveItem wavFile = new SkydriveItem() { name = obj["name"].ToString().TrimEnd(".wav".ToCharArray()), attributes = attrs };
                        objs.Add(wavFile);
                    }
                }
                wavList.ItemsSource = objs;
            }
            catch (LiveConnectException exception)
            {
                //this.infoTextBlock.Text = "Couldn't get list of projects because: " + exception.Message;
            }
        }

        private async void wavList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                //viewModel.Client.DownloadAsync(
                await viewModel.Client.BackgroundDownloadAsync(((SkydriveItem)wavList.SelectedItem).attributes["id"] + "/content", new Uri("/shared/transfers/" + ((SkydriveItem)wavList.SelectedItem).attributes["name"], UriKind.RelativeOrAbsolute));
                MessageBox.Show("Success!");
                IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();
                IsolatedStorageFileStream wavStream = new IsolatedStorageFileStream("/shared/transfers/"+((SkydriveItem)wavList.SelectedItem).attributes["name"], System.IO.FileMode.Open, store);
                viewModel.wavStream = wavStream;
                NavigationService.Navigate(new Uri("/WAVConfigPage.xaml", UriKind.RelativeOrAbsolute));
            }
            catch (System.Threading.Tasks.TaskCanceledException)
            {
                //this.infoTextBlock.Text = "Download cancelled.";
            }
            catch (LiveConnectException exception)
            {
                //this.infoTextBlock.Text = "Error getting file contents: " + exception.Message;
            }
        }
    }
}