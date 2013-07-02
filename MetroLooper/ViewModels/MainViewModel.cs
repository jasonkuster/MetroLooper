using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MetroLooper.Model;
using System.Collections.ObjectModel;
using System.IO.IsolatedStorage;
using Windows.Storage;

namespace MetroLooper.ViewModels
{
    class MainViewModel
    {
        private readonly RecordingManager rm;

        private BankViewModel bankVM;

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

        private MainViewModel()
        {
            rm = new RecordingManager(lockUI, addTrack, 0, 4000);
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

        private void addTrack(StorageFile newTrack){
            SelectedBank.tracks.Add(new Track("myTrack", newTrack));
        }

        public bool recButtons { get; set; }
        public bool stop { get; set; }

        private void lockUI(RecordingManager.LOCK_STATE lockState)
        {
            switch (lockState)
	        {
		        case RecordingManager.LOCK_STATE.RECORDING:
                    recButtons = false;
                    stop = true;
                    break;
                case RecordingManager.LOCK_STATE.ALL:
                    recButtons = false;
                    stop = false;
                    break;
                case RecordingManager.LOCK_STATE.NONE:
                    recButtons = true;
                    stop = true;
                    break;
                default:
                    recButtons = false;
                    stop = false;
                    break;
	        }
        }

        public void startRecord(bool one)
        {
            rm.startRecord(one);
        }

        public void stopRecord()
        {
            rm.stopRecord();
        }
    }
}
