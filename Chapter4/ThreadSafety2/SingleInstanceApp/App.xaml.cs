using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace SingleInstanceApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private const string eventName = "84bb9974-fb13-4927-bf47-91f9fca1601c";
        private EventWaitHandle singleInstanceEvent;

        protected override void OnStartup(StartupEventArgs e)
        {
            bool created;
            singleInstanceEvent = new EventWaitHandle(false, 
                                                      EventResetMode.AutoReset, 
                                                      eventName, 
                                                      out created);

            if (!created)
            {
                singleInstanceEvent.Set();
                Shutdown();
            }
            else
            {
                SynchronizationContext ctx = SynchronizationContext.Current;
                Task.Factory.StartNew(() =>
                    {
                        while (true)
                        {
                            singleInstanceEvent.WaitOne();
                            ctx.Post(_ => MakeActiveApplication(), null);
                        }
                    });
            }

            base.OnStartup(e);
        }

        private void MakeActiveApplication()
        {
            MainWindow.Activate();
            MainWindow.Topmost = true;
            MainWindow.Topmost = false;
            MainWindow.Focus();
        }
    }
}
