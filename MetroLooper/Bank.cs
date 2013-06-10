using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace MetroLooper
{
    class Bank
    {
        public Bank()
        {
            finalized = false;
            tracks = new ObservableCollection<StorageFile>();
        }
        public string bankID { get; set; }
        public bool finalized { get; private set; }
        public ObservableCollection<StorageFile> tracks { get; private set; }
    }
}
