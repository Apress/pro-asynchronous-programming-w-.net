using System;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncUtils
{
    public class CountingAsyncResult : IAsyncResult
    {
        private readonly object state;
        private readonly AsyncCallback callback;

        // For the implementation of IAsyncResult.AsyncCallback
        private readonly ManualResetEventSlim asyncEvent = new ManualResetEventSlim();

        private int outstandingAsyncOperations;

        // Store the callback and state passed
        public CountingAsyncResult(AsyncCallback callback, object state, int asyncOperationCount)
        {
            this.state = state;
            this.callback = callback ?? delegate { };

            outstandingAsyncOperations = asyncOperationCount;
        }

        // Called by consumer to state an async operation has completed
        public void OperationComplete()
        {
            if (outstandingAsyncOperations == 0)
            {
                throw new InvalidOperationException("All expected operations have already completed");
            }

            // signal the event object
            int currentOutstanding = Interlocked.Decrement(ref outstandingAsyncOperations);
            if (currentOutstanding == 0)
            {
                asyncEvent.Set();
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
            get { return outstandingAsyncOperations == 0; }
        }

        // Clean up the event as it may have allocated a WaitHandle
        public void Dispose()
        {
            asyncEvent.Dispose();
        } 
    }
}