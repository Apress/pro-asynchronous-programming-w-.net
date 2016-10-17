using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ShuttingDown
{
    class Program
    {
        static void Main(string[] args)
        {
            //SingleBlockShutdown();

            PropagatingCompletion();


        }

        private static void PropagatingCompletion()
        {
            var firstBlock = new TransformBlock<int, int>(i => i*2);
            var secondBlock = new ActionBlock<int>(i =>
            {
                Thread.Sleep(500);
                Console.WriteLine(i);
            });

            var thirdBlock = new ActionBlock<int>((Action<int>) Console.WriteLine);

            firstBlock.LinkTo(secondBlock, new DataflowLinkOptions() {PropagateCompletion = true});
            firstBlock.LinkTo(thirdBlock, new DataflowLinkOptions() {PropagateCompletion = true});

            for (int i = 0; i < 10; i++)
            {
                firstBlock.Post(i);
            }

            firstBlock.Complete();
            // firstBlock.Completion.Wait();

            //secondBlock.Complete();
            secondBlock.Completion.Wait();

            //Task.WaitAll(new Task[] {secondBlock.Completion, thirdBlock.Completion});
        }

        private static void SingleBlockShutdown()
        {
            var actionBlock = new ActionBlock<int>((Action<int>) Console.WriteLine);
            for (int i = 0; i < 10; i++)
            {
                actionBlock.Post(i);
            }

            Console.WriteLine("Completing..");
            actionBlock.Complete();
            Console.WriteLine("Waiting..");
            actionBlock.Completion.Wait();
            Console.WriteLine(actionBlock.Completion.Status);
        }
    }
}
