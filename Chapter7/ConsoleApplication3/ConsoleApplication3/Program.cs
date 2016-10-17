using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApplication3
{
    public class AsyncProducerConsumerCollection<T>
    {
        private readonly Queue<T> m_collection = new Queue<T>();
        private readonly Queue<TaskCompletionSource<T>> m_waiting =
            new Queue<TaskCompletionSource<T>>();

        public async void Add(T item)
        {

            
            TaskCompletionSource<T> tcs = null;
            lock (m_collection)
            {
                if (m_waiting.Count > 0) tcs = m_waiting.Dequeue();
                else m_collection.Enqueue(item);
            }
            if (tcs != null)
            {
                await Task.Yield();
                tcs.TrySetResult(item);
            }
        }

        public Task<T> Take()
        {
            lock (m_collection)
            {
                if (m_collection.Count > 0)
                {
                    return Task.FromResult(m_collection.Dequeue());
                }
                else
                {
                    var tcs = new TaskCompletionSource<T>();
                    m_waiting.Enqueue(tcs);
                    return tcs.Task;
                }
            }
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            AsyncProducerConsumerCollection<int> queue = new AsyncProducerConsumerCollection<int>();

            // How many consumers to create
            for (int nConsumer = 0; nConsumer < 2; nConsumer++)
            {
                Consumer(queue);
            }


            Console.WriteLine("Press enter to add work items to queue");
            Random rnd = new Random();
            while (true)
            {
                Console.ReadLine();
                queue.Add(3000);
                Console.WriteLine("Added..");

            }
        }

        private static async void Consumer(AsyncProducerConsumerCollection<int> queue)
        {
            while (true)
            {
                int val = await queue.Take();
                Console.WriteLine("Procesing");
                Thread.Sleep(val);
                Console.WriteLine("Done");
            }
        }
    }
}
