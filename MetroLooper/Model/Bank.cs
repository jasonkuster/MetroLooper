using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public int bankID { get; set; }
        public bool Finalized { get; set; }
        public ObservableCollection<Track> tracks { get; set; }
        public byte[] finalTrack { get; set; }
        public int Size { get; set; }
        public int Offset { get; set; }
        public double Pitch { get; set; }
        public double Volume { get; set; }
    }
}
