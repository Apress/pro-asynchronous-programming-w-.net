using System;
using System.Threading;

namespace RSKSchedulers
{
    class ThreadControl
    {
        private readonly CancellationTokenSource cts;
        public ThreadControl()
        {
            LastUsed = DateTime.Now;
            cts = new CancellationTokenSource();

        }
        public DateTime LastUsed { get; private set; }
        public bool InUse { get; private set; }
        public CancellationToken CancellationToken { get { return cts.Token; } }

        public bool CancelIfIdle(TimeSpan idleTimeout)
        {
            if (!InUse && DateTime.Now - LastUsed > idleTimeout)
            {
                cts.Cancel();
            }

            return cts.IsCancellationRequested;
        }

        public void SetNotInUse()
        {
            LastUsed = DateTime.Now;
            InUse = false;
        }

        public void SetInUse()
        {
            InUse = true;
        }
    }
}