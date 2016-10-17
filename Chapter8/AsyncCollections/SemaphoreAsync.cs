using System.Collections.Generic;
using System.Threading.Tasks;

namespace AsyncCollections
{
    public interface ISemaphoreAsync
    {
        Task AquireAsync();
        void Release();
    }

    public class SemaphoreAsync : ISemaphoreAsync
    {
        private readonly int maxCount;
        private Queue<TaskCompletionSource<object>> waiting = new Queue<TaskCompletionSource<object>>();
        private int count = 0;
        private object countGuard = new object();

        private Task<object> completedTask;

        public SemaphoreAsync(int maxCount)
        {
            this.maxCount = maxCount;
            var tcs = new TaskCompletionSource<object>();
            tcs.SetResult(null);
            completedTask = tcs.Task;
        }

        public Task AquireAsync()
        {
            lock (countGuard)
            {
                if (count != maxCount)
                {
                    count++;
                    return completedTask;
                }
                else
                {
                    var tcs = new TaskCompletionSource<object>();
                    waiting.Enqueue(tcs);
                    return tcs.Task;
                }
            }
        }

        public void Release()
        {
            lock (countGuard)
            {
                if (waiting.Count > 0)
                {
                    TaskCompletionSource<object> tcs = waiting.Dequeue();
                    tcs.SetResult(null);
                }
                else
                {
                    count--;
                }
            }
        }
    }
}