using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ProducerConsumerDataFlow
{
    class Program
    {
        static void Main(string[] args)
        {
           // LazyProducerConsumer();

            var blockConfiguration = new ExecutionDataflowBlockOptions()
            {
                NameFormat="Type:{0},Id:{1}",
                MaxDegreeOfParallelism = 2,
                BoundedCapacity = 2,
            };


            var consumerBlock =
                new ActionBlock<int>(new Action<int>(SlowConsumer) , blockConfiguration);
            
           
            for (int i = 0; i < 5; i++)
            {
                consumerBlock.Post(i);
              //  Console.WriteLine("Sending {0}",i);
               // consumerBlock.SendAsync(i).Wait();
            }

            Console.WriteLine(consumerBlock.ToString());
          
            
            consumerBlock.Complete();
            consumerBlock.Completion.Wait();
        }

        private static void SlowConsumer(int val)
        {
            Console.WriteLine("{0}: Consuming {1}", Task.CurrentId,val);
            Thread.Sleep(1000);
            
        }

        private static void LazyProducerConsumer()
        {
            var consumerBlock = new ActionBlock<int>(new Action<int>(Consume));

            PrintThreadPoolUsage("Main");

            for (int i = 0; i < 5; i++)
            {
                consumerBlock.Post(i);
                Thread.Sleep(1000);
                PrintThreadPoolUsage("loop");
            }

            consumerBlock.Complete();
            consumerBlock.Completion.Wait();
        }

        private static void Consume(int val)
        {
            PrintThreadPoolUsage("Consume");
            Console.WriteLine("{0}:{1} is thread pool thread {2}",Task.CurrentId,val,
            Thread.CurrentThread.IsThreadPoolThread);
        }

        private static void PrintThreadPoolUsage(string label)
        {
            int cpu;
            int io;
            ThreadPool.GetAvailableThreads(out cpu,out io);
            Console.WriteLine("{0}:CPU:{1},IO:{2}",label,cpu,io);
        }
    }
}
