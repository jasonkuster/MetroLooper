﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using MetroLooper.Model;
using System.Collections.ObjectModel;
using System.IO.IsolatedStorage;
using Windows.Storage;
using System.ComponentModel;
using System.Windows.Media;
//using Windows.UI;

namespace MetroLooper.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public enum LOCK_STATE { RECORDING, PPREC, ALL, NONE };

        static readonly object padlock = new object();
        private static MainViewModel mainVM = null;
        public static MainViewModel Instance
        {
            get
            {
                lock (padlock)
                {
                    if (mainVM == null)
                    {
                        mainVM = new MainViewModel();
                    }
                    return mainVM;
                }
            }
        }

        public MainViewModel()
        {
            recButtons = true;
        }

        public ObservableCollection<Project> Projects
        {
            get
            {
                return (ObservableCollection<Project>)IsolatedStorageSettings.ApplicationSettings["projects"];
            }
            set
            {
                IsolatedStorageSettings.ApplicationSettings["projects"] = value;
            }
        }

        //private RecordingManager _recordingManager;
        //public RecordingManager recordingManager;

        private Project selectedProject = null;
        public Project SelectedProject
        {
            get
            {
                if (selectedProject == null)
                {
                    selectedProject = new Project("Test Project");
                    selectedProject.banks.Add(new Bank());
                    selectedBank = selectedProject.banks[0];
                }
                return selectedProject;
            }
            set
            {
                selectedProject = value;
            }
        }

        private Bank selectedBank = null;
        public Bank SelectedBank
        {
            get
            {
                if (selectedBank == null)
                {
                    selectedBank = new Bank();
                }
                return selectedBank;
            }
            set
            {
                selectedBank = value;
            }
        }

        private void addTrack(StorageFile newTrack)
        {
            SelectedBank.tracks.Add(new Track("myTrack", newTrack));
        }

        private bool recButtons = true;
        public bool RecButtons
        {
            get
            {
                return recButtons;
            }
            set
            {
                this.recButtons = value;
                this.RaisePropertyChanged("RecButtons");
            }
        }

        private bool stop = false;
        public bool Stop
        {
            get
            {
                return stop;
            }
            set
            {
                this.stop = value;
                this.RaisePropertyChanged("Stop");
            }
        }

        private Brush barColor = new SolidColorBrush(Colors.Blue);
        public Brush BarColor
        {
            get
            {
                return barColor;
            }
            set
            {
                if (!this.barColor.Equals(value))
                {
                    this.barColor = value;
                    this.RaisePropertyChanged("BarColor");
                }
            }
        }

        public void lockUI(LOCK_STATE lockState)
        {
            switch (lockState)
            {
                case LOCK_STATE.RECORDING:
                    RecButtons = false;
                    Stop = true;
                    BarColor = new SolidColorBrush(Colors.Red);
                    break;
                case LOCK_STATE.PPREC:
                    recButtons = false;
                    Stop = true;
                    BarColor = new SolidColorBrush(Colors.Orange);
                    break;
                case LOCK_STATE.ALL:
                    RecButtons = false;
                    Stop = false;
                    BarColor = new SolidColorBrush(Colors.Orange);
                    break;
                case LOCK_STATE.NONE:
                    RecButtons = true;
                    Stop = false;
                    BarColor = new SolidColorBrush(Colors.Blue);
                    break;
                default:
                    RecButtons = false;
                    Stop = false;
                    BarColor = new SolidColorBrush(Colors.Blue);
                    break;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propertyName)
        {
            System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                PropertyChangedEventHandler handler = this.PropertyChanged;
                if (handler != null)
                {
                    handler(this, new PropertyChangedEventArgs(propertyName));
                }
            });
        }
    }
}