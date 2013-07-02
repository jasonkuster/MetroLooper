using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Xna.Framework;

namespace MetroLooper
{
    public class XNAAsyncDispatcher : IApplicationService
    {
        private DispatcherTimer _frameworkDispatcherTimer;
        public XNAAsyncDispatcher(TimeSpan dispatchInterval)
        {
            FrameworkDispatcher.Update();
            this._frameworkDispatcherTimer = new DispatcherTimer();
            this._frameworkDispatcherTimer.Tick += new EventHandler(frameworkDispatcherTimer_Tick);
            this._frameworkDispatcherTimer.Interval = dispatchInterval;
        }
        void IApplicationService.StartService(ApplicationServiceContext context)
        {
            this._frameworkDispatcherTimer.Start();
        }

        void IApplicationService.StopService()
        {
            this._frameworkDispatcherTimer.Stop();
        }

        void frameworkDispatcherTimer_Tick(object sender, EventArgs e)
        {
            FrameworkDispatcher.Update();
        }
    }
}
