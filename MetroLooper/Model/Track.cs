using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace MetroLooper.Model
{
    class Track
    {
        public Track(string name, StorageFile file)
        {
            trackName = name;
            this.file = file;
        }
        public string trackName { get; set; }
        public StorageFile file { get; set; }
    }
}
