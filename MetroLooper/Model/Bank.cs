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
            finalized = false;
            tracks = new ObservableCollection<Track>();
        }

        public int bankID { get; set; }
        public bool finalized { get; private set; }
        public ObservableCollection<Track> tracks { get; private set; }
        public StorageFile finalTrack { get; set; }
    }
}
