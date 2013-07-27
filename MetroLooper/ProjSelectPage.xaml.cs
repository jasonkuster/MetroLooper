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
using MetroLooper.Model;

namespace MetroLooper
{
    public partial class ProjSelectPage : PhoneApplicationPage
    {

        MainViewModel viewModel;

        public ProjSelectPage()
        {
            InitializeComponent();
            viewModel = MainViewModel.Instance;
            this.DataContext = viewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            projSelectList.SelectedItem = null;
        }

        private void projSelectList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((Project)projSelectList.SelectedItem != null)
            {
                viewModel.SelectedProject = ((Project)projSelectList.SelectedItem);
                NavigationService.Navigate(new Uri("/ProjectPage.xaml", UriKind.RelativeOrAbsolute));
            }
        }
    }
}