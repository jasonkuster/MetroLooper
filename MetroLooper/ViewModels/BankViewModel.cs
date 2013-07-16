using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroLooper.ViewModels
{
    class BankViewModel
    {
        static readonly object padlock = new object();

        public TrackViewModel trackVM;

        private static BankViewModel bankVM = null;
        public static BankViewModel BankVM
        {
            get
            {
                lock (padlock)
                {
                    if (bankVM == null)
                    {
                        bankVM = new BankViewModel();
                    }
                    return bankVM;
                }
            }
        }
    }
}
