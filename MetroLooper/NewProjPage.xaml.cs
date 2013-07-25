using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using MetroLooper.Model;
using System.IO.IsolatedStorage;
using System.Collections.ObjectModel;
using MetroLooper.ViewModels;

namespace MetroLooper
{
    public partial class NewProjPage : PhoneApplicationPage
    {
        IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
        MainViewModel viewModel;

        public NewProjPage()
        {
            InitializeComponent();
            viewModel = MainViewModel.Instance;
        }

        private void startButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
			if (projNameBox.Text != "" && bpmBox.Text != "" && measuresBox.Text != "")
			{
                int projBpm = Convert.ToInt32(bpmBox.Text);
                int projMeasures = Convert.ToInt32(measuresBox.Text); //TODO: Make these more robust.
                Project newProj = new Project() { bpm = projBpm, measures = projMeasures, projName = projNameBox.Text };
                newProj.banks.Add(new Bank() { bankID = 0 });
                ((ObservableCollection<Project>)settings["projects"]).Add(newProj);
                viewModel.SelectedProject = newProj;
                NavigationService.Navigate(new Uri("/ProjectPage.xaml", UriKind.RelativeOrAbsolute));
			}
        }
    }
}