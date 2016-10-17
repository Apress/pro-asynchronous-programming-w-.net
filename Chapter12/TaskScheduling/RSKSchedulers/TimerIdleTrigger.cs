using System;
using System.Threading;

namespace RSKSchedulers
{
    class TimerIdleTrigger : IIdleTrigger
    {
        private readonly TimeSpan idleTimeout;
        private readonly Timer idleTimer;

        public TimerIdleTrigger(TimeSpan checkFrequency, TimeSpan idleTimeout)
        {
            this.idleTimeout = idleTimeout;
            idleTimer= new Timer(_ => CheckIdle(this, EventArgs.Empty), null, TimeSpan.Zero, checkFrequency);
        }

        public event EventHandler CheckIdle = delegate { };

        public TimeSpan IdleTimeout
        {
            get { return idleTimeout; }
        }

        public void Dispose()
        {
            idleTimer.Dispose();
        }
    }
}