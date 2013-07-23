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
        public Track()
        {
            isSelected = false;
            Finalized = false;
        }

        public string trackName
        {
            get
            {
                return "Track " + (trackID + 1);
            }
        }
        public int trackID { get; set; }
        public int Size { get; set; }
        public int Latency { get; set; }
        public int Offset { get; set; }
        public double Volume { get; set; }
        public string fileName { get; set; }
        public byte[] trackData { get; set; }
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
