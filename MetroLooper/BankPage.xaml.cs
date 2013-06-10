using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace MetroLooper
{
    public partial class BankPage : PhoneApplicationPage
    {
        public BankPage()
        {
            InitializeComponent();
            Project p = new Project("My Project");
            this.DataContext = p;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            int bankIndex = 0;
            foreach (UIElement ctrl in ContentPanel.Children)
            {
                if (ctrl.GetType() == typeof(StackPanel) && ((StackPanel)ctrl).Orientation == System.Windows.Controls.Orientation.Horizontal)
                {
                    if (bankIndex < ((Project)(this.DataContext)).banks.Count)
                    {
                        ((StackPanel)ctrl).DataContext = ((Project)(this.DataContext)).banks[bankIndex];
                        bankIndex++;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        private void Border_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/LoopPage.xaml?loopSelected=1", UriKind.RelativeOrAbsolute));
        }
    }
}