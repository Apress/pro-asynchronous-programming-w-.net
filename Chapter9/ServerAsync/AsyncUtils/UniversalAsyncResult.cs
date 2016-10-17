using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncUtils
{
    public class UniversalAsyncResult : IAsyncResult, IDisposable
    {
        // To store the callback and state passed
        private readonly AsyncCallback callback;
        private readonly object state;

        // For the implementation of IAsyncResult.AsyncCallback
        private readonly ManualResetEventSlim asyncEvent = new ManualResetEventSlim();

        // complete flag that is set to true on completion (supports IAsyncResult.IsCompleted)
        private volatile bool complete;

        // Store the callback and state passed
        public UniversalAsyncResult(AsyncCallback callback, object state)
        {
            this.callback = callback;
            this.state = state;
        }

        // Called by consumer to set the call object to a completed state
        public void Complete()
        {
            // flag as complete
            complete = true;

            // signal the event object
            asyncEvent.Set();

            // Fire callback to signal the call is complete
            if (callback != null)
            {
                callback(this);
            }
        }

        public object AsyncState
        {
            get { return state; }
        }

        public WaitHandle AsyncWaitHandle
        {
            get { return asyncEvent.WaitHandle; }
        }

        public bool CompletedSynchronously
        {
            get { return false; }
        }

        public bool IsCompleted
        {
            get { return complete; }
        }

        // Clean up the event as it may have allocated a WaitHandle
        public void Dispose()
        {
            asyncEvent.Dispose();
        }
    }
}
