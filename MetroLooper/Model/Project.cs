using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroLooper.Model
{
    public class Project
    {
        public Project()
        {
            projName = "Project 1";
            banks = new ObservableCollection<Bank>();
        }

        public string projName { get; set; }
        public ObservableCollection<Bank> banks { get; set; }
        public int bpm { get; set; }
        public int measures { get; set; }
        public int trackLen
        {
            get
            {
                return ((measures * 4) / (bpm / 60) * 1000);
            }
        }
    }
}
