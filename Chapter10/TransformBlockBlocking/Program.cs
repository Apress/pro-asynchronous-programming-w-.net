using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace TransformBlockBlocking
{
    /// <summary>
    /// TransformBlock<> task will not process any more queued items until the item has been
    /// received.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            var divideBlock =
                new TransformBlock<Tuple<int, int>, int>(pair => pair.Item1/pair.Item2);

            var divideConsumer = new ActionBlock<int>((Action<int>)(result =>
            {
                Console.WriteLine(result);
                Thread.Sleep(1000);
            }), new ExecutionDataflowBlockOptions() { BoundedCapacity = 1 })
            ;

            divideBlock.LinkTo(divideConsumer, new DataflowLinkOptions() { PropagateCompletion = true });

            divideBlock.Completion.ContinueWith(dbt => divideConsumer.Complete());

            Random rnd = new Random();
          
            for (int i = 0; i < 10; i++)
            {

               
                divideBlock.Post(new Tuple<int, int>(rnd.Next(1, 100), rnd.Next(1, 10)));
                
            }

            divideBlock.Complete();
            divideBlock.Completion.Wait();
            Console.WriteLine("Posting done");
            divideConsumer.Completion.Wait();
        }
    }
}
