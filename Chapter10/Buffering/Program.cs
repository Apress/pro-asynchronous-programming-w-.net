using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Buffering
{
    class Program
    {
        static void Main(string[] args)
        {
            var nonGreedy = new ExecutionDataflowBlockOptions() {BoundedCapacity = 1};
            var flowComplete = new DataflowLinkOptions() {PropagateCompletion = true};

            var processorA = new ActionBlock<int>((Action<int>)( i => Processor("A",i)),nonGreedy);
            var processorB = new ActionBlock<int>((Action<int>)( i => Processor("B",i)),nonGreedy);
            var transform = new TransformBlock<int, int>(i => i*2);

            var buffer = new BufferBlock<int>();
            buffer.LinkTo(processorA,flowComplete);
            buffer.LinkTo(processorB,flowComplete);

            transform.LinkTo(processorA);
            transform.LinkTo(processorB);

            for (int i = 0; i < 5; i++)
            {
                transform.Post(i);            
            }
            transform.Complete();
            transform.Completion.Wait();
            Console.WriteLine("All transformed");

            Console.ReadLine();
        }

        private static void Processor( string name , int value)
        {
            Console.WriteLine("Processor {0}, starting : {1}",name,value);
            Thread.Sleep(1000);
            Console.WriteLine("Processor {0}, done : {1}",name,value);
        }

      
    }
}
