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

        private void projSelectList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            viewModel.SelectedProject = ((Project)projSelectList.SelectedItem);
            NavigationService.Navigate(new Uri("/ProjectPage.xaml", UriKind.RelativeOrAbsolute));
        }
    }
}