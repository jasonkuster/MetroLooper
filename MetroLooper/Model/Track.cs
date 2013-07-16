using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace MetroLooper.Model
{
    public class Track : INotifyPropertyChanged
    {
        public Track(int trackID, StorageFile file)
        {
            this.trackID = trackID;
            this.file = file;
            isSelected = false;
            Finalized = false;
        }
        public int trackID { get; set; }
        public StorageFile file { get; set; }
        public bool _Finalized;
        public bool Finalized {
            get
            {
                return _Finalized;
            }
            set
            {
                if (_Finalized != value)
                {
                    _Finalized = value;
                    this.RaisePropertyChanged("Finalized");
                }
            }
        }
        private bool isSelected;
        public bool IsSelected
        {
            get
            {
                return isSelected;
            }
            set
            {
                if (isSelected != value)
                {
                    isSelected = value;
                    this.RaisePropertyChanged("IsSelected");
                }
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
