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

namespace MetroLooper
{
    public partial class MainPage : PhoneApplicationPage
    {
        AudioManager _manager;

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            _manager = new AudioManager();

            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }

        private void Record_Click(object sender, RoutedEventArgs e)
        {
            if (!_manager.isRecording)
            {
                _manager.RecordStart();
            }
            else
            {
                _manager.RecordStopAndSubmit(0, 0);
            }
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            if (!_manager.isPlaying)
            {
                _manager.PlayAll();
            }
            else
            {
                _manager.StopAll();
            }
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
    }
}