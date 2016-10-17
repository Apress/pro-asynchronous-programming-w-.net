using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ErrorHandling
{
    public static class BlockErrorExtensions
    {
        public static void BlockErrorHandler(this IDataflowBlock block, Action<Exception> errorHandler)
        {
            block.Completion.ContinueWith(b =>
            {
                foreach (Exception error in block.Completion.Exception.Flatten().InnerExceptions)
                {
                    errorHandler(error);
                }
            },TaskContinuationOptions.OnlyOnFaulted);
        }

        public static void ForwardError(this IDataflowBlock block, IDataflowBlock destinationBlock)
        {
            block.Completion.ContinueWith(b => destinationBlock.Fault(block.Completion.Exception), TaskContinuationOptions.OnlyOnFaulted);
        }
    }
    class Program
    {
        private static void Main(string[] args)
        {
            //UnhandledException();

           // InternalExceptionHandling();

            //ExternallyExceptionHandling();


            PropagatingAndHandlingExceptions();


            var divideBlock =
                new ActionBlock<Tuple<int, int>>(
                    (Action<Tuple<int, int>>) (input =>
                    {
                        Thread.Sleep(10);
                        Console.WriteLine(input.Item1/input.Item2);
                    }),
                    new ExecutionDataflowBlockOptions()
                    {
                        MaxDegreeOfParallelism = 2
                    });

            divideBlock.Post(Tuple.Create(10, 5));
            divideBlock.Post(Tuple.Create(20, 4));
            divideBlock.Post(Tuple.Create(10, 0));
            divideBlock.Post(Tuple.Create(10, 2));
            divideBlock.Post(Tuple.Create(50, 5));
            divideBlock.Post(Tuple.Create(10, 5));
            divideBlock.Post(Tuple.Create(20, 4));


         //   Console.Read();

            try
            {
                divideBlock.Completion.Wait();
            }
            catch (AggregateException errors)
            {
                foreach (Exception error in errors.InnerExceptions)
                {
                    Console.WriteLine(error.Message);
                }
                
            }

        }

        private static void SingleBlockCancellation()
        {
            CancellationTokenSource cts = new CancellationTokenSource();

            var slowAction = new ActionBlock<int>(
                (Action<int>) (i =>
                {
                    Console.WriteLine("{0}:Started", i);
                    Thread.Sleep(1000);
                    //cts.Token.WaitHandle.WaitOne(1000);
                    //cts.Token.ThrowIfCancellationRequested();
                    Console.WriteLine("{0}:Done", i);
                }),
                new ExecutionDataflowBlockOptions() {CancellationToken = cts.Token});

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

            var slowTransform = new TransformBlock<int,string>(
               i =>
               {
                   Console.WriteLine("{0}:Started", i);
                   cts.Token.WaitHandle.WaitOne(1000);
                   cts.Token.ThrowIfCancellationRequested();
                   Console.WriteLine("{0}:Done", i);
                   return i.ToString();
               },
               new ExecutionDataflowBlockOptions() { CancellationToken = cts.Token });

            var printAction = new ActionBlock<string>((Action<string>) Console.WriteLine,
                new ExecutionDataflowBlockOptions() { CancellationToken = cts.Token });

            slowTransform.LinkTo(printAction,new DataflowLinkOptions(){PropagateCompletion = true});

            slowTransform.Post(1);
            slowTransform.Post(2);
            slowTransform.Post(3);

        //    slowTransform.Complete();
            Console.ReadLine();
            cts.Cancel();

            printAction.Completion.ContinueWith(pat => Console.WriteLine(pat.Status));

            Console.ReadLine();

        }

       

        private static void PropagatingAndHandlingExceptions()
        {
            var divideBlock = new TransformBlock<Tuple<int, int>, int>(input => input.Item1/input.Item2);
            var printingBlock = new ActionBlock<int>((Action<int>) Console.WriteLine);

            divideBlock.LinkTo(printingBlock,new DataflowLinkOptions() {PropagateCompletion = true});

            divideBlock.Post(Tuple.Create(10, 5));
            divideBlock.Post(Tuple.Create(20, 4));
            divideBlock.Post(Tuple.Create(10, 0));
            divideBlock.Post(Tuple.Create(10, 2));

            divideBlock.Complete();

            try
            {
                printingBlock.Completion.Wait();
            }
            catch (AggregateException errors)
            {
                foreach (Exception error in errors.Flatten().InnerExceptions)
                {
                    Console.WriteLine("Divide block failed Reason:{0}", error.Message);
                }
            }
        }

        private static void ExternallyExceptionHandling()
        {
            var divideBlock =
                new ActionBlock<Tuple<int, int>>(
                    (Action<Tuple<int, int>>) (input => Console.WriteLine(input.Item1/input.Item2)), 
                    new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 4});

            divideBlock.Post(Tuple.Create(10, 5));
            divideBlock.Post(Tuple.Create(20, 0));
            divideBlock.Post(Tuple.Create(10, 0));
            divideBlock.Post(Tuple.Create(10, 2));

            divideBlock
                .Completion
                .ContinueWith(
                    dbt =>
                    {
                        foreach (Exception error in dbt.Exception.Flatten().InnerExceptions)
                        {
                            Console.WriteLine("Divide block failed Reason:{0}", error.Message);
                        }
                    },
                    TaskContinuationOptions.OnlyOnFaulted);

            Console.ReadLine();
        }

        private static void InternalExceptionHandling()
        {
            var divideBlock =
                new ActionBlock<Tuple<int, int>>(
                    (Action<Tuple<int, int>>) delegate(Tuple<int, int> pair)
                    {
                        try
                        {
                            Console.WriteLine(pair.Item1/pair.Item2);
                        }
                        catch (DivideByZeroException)
                        {
                            Console.WriteLine("Dude, can't divide by zero");
                        }
                    });

            divideBlock.Post(Tuple.Create(10, 5));
            divideBlock.Post(Tuple.Create(20, 4));
            divideBlock.Post(Tuple.Create(10, 0));
            divideBlock.Post(Tuple.Create(10, 2));

            Console.ReadLine();
        }

        private static void UnhandledException()
        {
            var divideBlock =
                new ActionBlock<Tuple<int, int>>(
                    (Action<Tuple<int, int>>) (input => Console.WriteLine(input.Item1/input.Item2)));

            divideBlock.Post(Tuple.Create(10, 5));
            divideBlock.Post(Tuple.Create(20, 4));
            divideBlock.Post(Tuple.Create(10, 0));
            divideBlock.Post(Tuple.Create(10, 2));
        }

        private static void FirstAttempt()
        {
            CancellationTokenSource cts = new CancellationTokenSource();

            var divideBlock =
                new TransformBlock<Tuple<int, int>, int>(pair => pair.Item1/pair.Item2,
                    new ExecutionDataflowBlockOptions()
                    {
                        CancellationToken = cts.Token
                    });

            var divideConsumer = new ActionBlock<int>((result =>
            {
                Console.WriteLine(result);

                Thread.Sleep(500);
            }), new ExecutionDataflowBlockOptions() {BoundedCapacity = 11, CancellationToken = cts.Token})
                ;

            divideBlock.LinkTo(divideConsumer, new DataflowLinkOptions() {PropagateCompletion = true});

            divideBlock.Completion.ContinueWith(dbt => divideConsumer.Complete());

            Random rnd = new Random();
            cts.CancelAfter(TimeSpan.FromSeconds(2));

            for (int i = 0; i < 10; i++)
            {
                // divideBlock.Post(new Tuple<int, int>(rnd.Next(0, 10), rnd.Next(0, 10)));
                divideBlock.Post(new Tuple<int, int>(rnd.Next(1, 10), rnd.Next(1, 10)));
                //Thread.Sleep(250);
            }

            divideBlock.Complete();
            divideBlock.Completion.Wait();
            Console.WriteLine("Posting done");
            divideConsumer.Completion.Wait();
        }
    }
}
