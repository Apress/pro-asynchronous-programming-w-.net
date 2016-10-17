using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace AsyncCollections
{
    public class AsyncQueue<T> : IAsyncQueue<T>
    {
        private ConcurrentQueue<T> items = new ConcurrentQueue<T>();
        private ConcurrentQueue<TaskCompletionSource<T>> dequeueQueue = new ConcurrentQueue<TaskCompletionSource<T>>();

        private object queueGuard = new object();

        public async void Enqueue(T item)
        {
            TaskCompletionSource<T> tcs;
            bool hasItem = false;

            if (!(hasItem = dequeueQueue.TryDequeue(out tcs)))
            {

                lock (queueGuard)
                {
                    if (!(hasItem = dequeueQueue.TryDequeue(out tcs)))
                    {
                        items.Enqueue(item);
                    }
                }
            }

            if (hasItem)
            {
                await Task.Yield();
                tcs.SetResult(item);
            }
        }

        public  Task<T> Dequeue()
        {
            var tcs = new TaskCompletionSource<T>();
            T item;
            bool hasItem = false;

            if (!(hasItem = items.TryDequeue(out item)))
            {
                lock (queueGuard)
                {
                    hasItem = items.TryDequeue(out item);
                    if (!hasItem)
                    {
                        dequeueQueue.Enqueue(tcs);
                    }
                }
            }
            if (hasItem)
            {
                tcs.SetResult(item);
            }
           

            return tcs.Task;
        }
    }
}