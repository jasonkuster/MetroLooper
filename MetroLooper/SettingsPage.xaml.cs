using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Live.Controls;
using Microsoft.Live;
using MetroLooper.ViewModels;

namespace MetroLooper
{
    public partial class SettingsPage : PhoneApplicationPage
    {
        MainViewModel viewModel;

        public SettingsPage()
        {
            InitializeComponent();
            viewModel = MainViewModel.Instance;
        }

        private void signInButton_SessionChanged(object sender, LiveConnectSessionChangedEventArgs e)
        {
            if (e.Status == LiveConnectSessionStatus.Connected)
            {
                viewModel.Client = new LiveConnectClient(e.Session);
            }
            else
            {
                this.infoTextBlock.Text = "not signed in";
            }
        }
    }
}