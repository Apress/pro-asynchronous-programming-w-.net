using System;

namespace RSKSchedulers.Test
{
    class StubIdleTrigger : IIdleTrigger
    {
        private readonly int timeoutInMilliseconds;

        public bool Disposed { get; private set; }

        public StubIdleTrigger()
            : this(5000)
        {
            
        }

        public StubIdleTrigger(int timeoutInMilliseconds)
        {
            this.timeoutInMilliseconds = timeoutInMilliseconds;
        }

        public event EventHandler CheckIdle;

        public int IdleTimeoutCheckCount { get; private set; }

        public TimeSpan IdleTimeout
        {
            get
            {
                IdleTimeoutCheckCount++;
                return TimeSpan.FromMilliseconds(timeoutInMilliseconds);
            }
        }

        public bool FireCheckIdle()
        {
            if (CheckIdle == null)
            {
                return false;
            }

            CheckIdle(this, EventArgs.Empty);
            return true;
        }

        public void Dispose()
        {
            Disposed = true;
        }
    }
}