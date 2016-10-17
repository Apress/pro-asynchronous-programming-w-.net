using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Batching
{
    class Program
    {
        private static void Main(string[] args)
        {
            // IntervalBatcher();

           // QuantityBatcher();

           
        }

        private static void DisplayValues(int[] values)
        {
            foreach (int value in values)
            {
                Console.WriteLine(value);
            }
            Console.WriteLine();
        }

        private static IEnumerable<int> TimesTable(int input, int delay)
        {
            for (int i = 0; i < 12; i++)
            {
                yield return i*input;
                Thread.Sleep(delay);
            }
        }

        private static void QuantityBatcher()
        {
            int batchSize = 100;
            var batcher = new BatchBlock<int>(batchSize);

            var averager = new ActionBlock<int[]>(values => Console.WriteLine("{0} reduced to {1}",values.Length,values.Average()));

            batcher.LinkTo(averager);

            Console.WriteLine("Press Q to quit");
            var rnd = new Random();
            bool quit = false;
            do
            {
                Thread.Sleep(10);
                batcher.Post(rnd.Next(1, 100));
                quit =  (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Q) ;
            } while (!quit);
            batcher.Complete();
            batcher.Completion.Wait();
        }

        private static void IntervalBatcher()
        {
            var batcher = new BatchBlock<int>(int.MaxValue);
            var averager = new ActionBlock<int[]>(values => Console.WriteLine(values.Average()));

            batcher.LinkTo(averager);

            var timer = new Timer(_ => batcher.TriggerBatch(), null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));

            var rnd = new Random();
            while (true)
            {
                batcher.Post(rnd.Next(1, 100));
            }
        }
    }
}
