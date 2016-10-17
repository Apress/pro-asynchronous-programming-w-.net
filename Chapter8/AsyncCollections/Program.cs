using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncCollections
{
    public class MySynchronizationContext : SynchronizationContext
    {
         
     
    }
    class Program
    {
        private static void Main(string[] args)
        {
            SemaphoreTests();

            AsyncQueueTests();
        }

        private static void AsyncQueueTests()
        {
            IAsyncQueue<int> queue = new AsyncQueue<int>();

            for (int i = 0; i < 2; i++)
            {
                Task.Run(() => Consumer(queue));
            }

            Random rnd = new Random();
            while (true)
            {
                Console.ReadLine();

                queue.Enqueue(rnd.Next(1, 1000));
            }
        }

        public static async void Consumer(IAsyncQueue<int> queue )
        {
            while (true)
            {
                Console.WriteLine("{0}:Waiting",Thread.CurrentThread.ManagedThreadId);
                int val=await queue.Dequeue();
                Console.WriteLine("{0}: Processing {1}" , Thread.CurrentThread.ManagedThreadId,val);
                Thread.Sleep(2000);
            }
        }

        private static void SemaphoreTests()
        {

            SemaphoreAsync semaphore = new SemaphoreAsync(1);

            for (int nWork = 0; nWork < 2; nWork++)
            {
                int work = nWork;
                Task.Run(() => RunWorker(work, semaphore));
            }


            Console.ReadLine();
        }

        private static async void RunWorker(int nWork, SemaphoreAsync semaphore)
        {
            SynchronizationContext.SetSynchronizationContext(new MySynchronizationContext());
            while (true)
            {

                await semaphore.AquireAsync();//.ContinueWith(t => { });
                Console.WriteLine("{0}:{1},Aquired",nWork,Thread.CurrentThread.ManagedThreadId);
                await Task.Delay(2000);
                //Thread.Sleep(2000);
                Console.WriteLine("{0}:{1}:Releasing",nWork,Thread.CurrentThread.ManagedThreadId);
                semaphore.Release();
                Console.WriteLine("{0}:{1}:Released", nWork, Thread.CurrentThread.ManagedThreadId);
                Thread.Sleep(1000);
            }
        }
    }
}
