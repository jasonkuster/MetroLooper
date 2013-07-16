using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MetroLooper.Model;

namespace MetroLooper.ViewModels
{
    class TrackViewModel
    {
        private Bank _bank;

        static readonly object padlock = new object();
        private static TrackViewModel trackVM = null;
        public static TrackViewModel TrackVM
        {
            get
            {
                lock (padlock)
                {
                    if (trackVM == null)
                    {
                        trackVM = new TrackViewModel();
                    }
                    return trackVM;
                }
            }
        }
    }
}
