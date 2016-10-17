using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProducerConsumerNonSleepyThreads
{
    public class AsyncCollection<T>
    {
        private IProducerConsumerCollection<T> collection;
        private ConcurrentQueue<TaskCompletionSource<T>> consumers;
        
        public AsyncCollection(IProducerConsumerCollection<T> collection )
        {
            this.collection = collection;
            consumers = new ConcurrentQueue<TaskCompletionSource<T>>();
        }

        public Task<T> TakeAsync()
        {
            var tcs = new TaskCompletionSource<T>();
            if ( !TryGetItem(tcs))
            {
                lock (collection)
                {
                    if (!TryGetItem(tcs))
                    {
                        consumers.Enqueue(tcs);
                    }
                }
            }

            

            return tcs.Task;
        }

        public void Add(T item)
        {
            if (!TryWaitingConsumer(item))
            {
                lock (collection)
                {
                    if (!TryWaitingConsumer(item))
                    {
                        collection.TryAdd(item);
                    }
                }
            }
        }

        private bool TryWaitingConsumer(T item)
        {
            TaskCompletionSource<T> tcs;

            if (consumers.TryDequeue(out tcs))
            {
                Task.Run(() => tcs.SetResult(item));
                return true;
            }
            return false;
        }

        private bool TryGetItem(TaskCompletionSource<T> tcs )
        {
            T item;
            if (collection.TryTake(out item))
            {
                tcs.SetResult(item);
                return true;
            }

            return false;
        }
    }
    
    public class LazyProducerConsumerQueue<T>
    {
        private readonly Action<T> consumer;
        private AsyncCollection<T> queue = new AsyncCollection<T>(new ConcurrentQueue<T>());

        public LazyProducerConsumerQueue(int numberOfConsumers,Action<T> consumer)
        {
            this.consumer = consumer;
            for (int nConsumer = 0; nConsumer < numberOfConsumers; nConsumer++)
            {
               ConsumerBody();
            }
        }
         
        public void Enqueue(T item)
        {
            queue.Add(item);
        }

        private async void ConsumerBody()
        {
            while (true)
            {
                T result = await queue.TakeAsync().ConfigureAwait(false);
                consumer(result);
            }
        }
    }
    
    class Program
    {
        static void Main(string[] args)
        {
            LazyProducerConsumerQueue<int> dcq = new LazyProducerConsumerQueue<int>(3, i =>
            {
                Console.WriteLine("{0}:Processing...",Thread.CurrentThread.ManagedThreadId);
                Thread.Sleep(i);
                Console.WriteLine("{0}:Done", Thread.CurrentThread.ManagedThreadId);
            });


            dcq.Enqueue(1000);
            Thread.Sleep(2000);
            dcq.Enqueue(200);
            Thread.Sleep(1000);

            Random rnd = new Random();
            for (int i = 0; i < 10; i++)
            {
              
                dcq.Enqueue(rnd.Next(1000,5000));
            }
            Console.ReadLine();
            dcq.Enqueue(2000);
            Thread.Sleep(2000);
            dcq.Enqueue(1000);

            Console.ReadLine();
        }


    }
} 
