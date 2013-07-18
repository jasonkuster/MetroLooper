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

        public int bankID { get; set; }
        public bool Finalized { get; private set; }
        public ObservableCollection<Track> tracks { get; set; }
        public string finalTrack { get; set; }
    }
}
