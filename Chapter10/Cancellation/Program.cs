using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Cancellation
{
    class Program
    {
        static void Main(string[] args)
        {
            LinkedCancellation();
        }

        private static void SingleBlockCancellation()
        {
            CancellationTokenSource cts = new CancellationTokenSource();

            var slowAction = new ActionBlock<int>(
                (Action<int>)(i =>
                {
                    Console.WriteLine("{0}:Started", i);
                    Thread.Sleep(1000);
                    //cts.Token.WaitHandle.WaitOne(1000);
                    //cts.Token.ThrowIfCancellationRequested();
                    Console.WriteLine("{0}:Done", i);
                }),
                new ExecutionDataflowBlockOptions() { CancellationToken = cts.Token });

            slowAction.Post(1);
            slowAction.Post(2);
            slowAction.Post(3);

            slowAction.Complete();

            slowAction
                .Completion
                .ContinueWith(sab => Console.WriteLine("Blocked finished in state of {0}", sab.Status));


            Console.ReadLine();
            cts.Cancel();

            Console.ReadLine();
        }

        private static void LinkedCancellation()
        {
            var cts = new CancellationTokenSource();

            var slowTransform = new TransformBlock<int, string>(
               i =>
               {
                   Console.WriteLine("{0}:Started", i);
                   cts.Token.WaitHandle.WaitOne(1000);
                   cts.Token.ThrowIfCancellationRequested();
                   Console.WriteLine("{0}:Done", i);
                   return i.ToString();
               },
               new ExecutionDataflowBlockOptions() { CancellationToken = cts.Token });

            var printAction = new ActionBlock<string>((Action<string>)Console.WriteLine,
                new ExecutionDataflowBlockOptions() { CancellationToken = cts.Token });

            slowTransform.LinkTo(printAction, new DataflowLinkOptions() { PropagateCompletion = true });

            slowTransform.Post(1);
            slowTransform.Post(2);
            slowTransform.Post(3);
            
            Console.ReadLine();
            cts.Cancel();

            printAction.Completion.ContinueWith(pat => Console.WriteLine(pat.Status));

            Console.ReadLine();

        }

    }
}
