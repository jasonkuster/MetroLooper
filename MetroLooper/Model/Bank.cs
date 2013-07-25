using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace MetroLooper.Model
{
    public class Bank
    {
        public Bank()
        {
            Finalized = false;
            tracks = new ObservableCollection<Track>();
            Initialized = false;
        }

        private string _bankName = "";
        public string BankName
        {
            get
            {
                if (_bankName == "")
                {
                    return "Bank " + (bankID + 1);
                }
                return _bankName;
            }
            set
            {
                _bankName = value;
            }
        }

        private int _bankID;
        public int bankID
        {
            get
            {
                return _bankID;
            }
            set
            {
                _bankID = value;
                RaisePropertyChanged("BankName");
            }
        }
        public bool Finalized { get; set; }
        public ObservableCollection<Track> tracks { get; set; }
        public byte[] finalTrack { get; set; }
        public int Size { get; set; }
        public int Offset { get; set; }
        public double Pitch { get; set; }
        public double Volume { get; set; }
        public bool Initialized { get; set; }
        public bool NotInitialized
        {
            get
            {
                return !Initialized;
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
