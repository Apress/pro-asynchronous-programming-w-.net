using System;
using System.ComponentModel;
using System.Threading;

namespace AsyncOpManager
{
    public class Clock
    {
        public int Hour { get { return DateTime.Now.Hour; } }
        public int Minutes { get { return DateTime.Now.Minute; } }
        public int Seconds { get { return DateTime.Now.Second; } }

        public event EventHandler<EventArgs> Tick = delegate { };

        private Timer timer;
        private SynchronizationContext uiCtx;

        public Clock()
        {
            uiCtx = SynchronizationContext.Current ?? new SynchronizationContext();
            timer = new Timer(OnTick,null,1000,1000);
        }

        private void OnTick(object state)
        {
            uiCtx.Post(_ => Tick(this, EventArgs.Empty), null);
        }
    }
}