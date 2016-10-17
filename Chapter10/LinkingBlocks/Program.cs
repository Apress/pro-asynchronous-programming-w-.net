using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace LinkingBlocks
{
    [Serializable]
    public class TooBusyException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public TooBusyException()
        {
        }

        public TooBusyException(string message) : base(message)
        {
        }

        public TooBusyException(string message, Exception inner) : base(message, inner)
        {
        }

        protected TooBusyException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
    public class LoadBalancerAsync<TInput,TOutput>
    {
        private class WorkItem
        {
            public TInput Input;
            public TaskCompletionSource<TOutput> Tcs;
        }

        private ConcurrentQueue<Func<TInput,TOutput>> processorQueue; 
        private ActionBlock<WorkItem> processorBlock;
 
        public LoadBalancerAsync(Func<TInput,TOutput>[] processors , int maxQueueLength)
        {
            processorQueue = new ConcurrentQueue<Func<TInput, TOutput>>(processors);
            processorBlock = new ActionBlock<WorkItem>((Action<WorkItem>) ProcessWorkItem, new ExecutionDataflowBlockOptions()
                {
                    BoundedCapacity = maxQueueLength,
                    MaxDegreeOfParallelism = processors.Length
                });
        }

        private void ProcessWorkItem(WorkItem workItem)
        {
            Func<TInput, TOutput> operation = null;
            if (processorQueue.TryDequeue(out operation))
            {
                try
                {
                    Task.Run(() =>
                             workItem.Tcs.SetResult(operation(workItem.Input)));
                }
                catch (Exception error)
                {
                    workItem.Tcs.SetException(error);
                }
                processorQueue.Enqueue(operation);
            }
        }

        public Task<TOutput> DoAsync(TInput input)
        {
            TaskCompletionSource<TOutput> tcs = new TaskCompletionSource<TOutput>();
            WorkItem wi = new WorkItem() {Input = input, Tcs = tcs};

            if (!processorBlock.Post(wi))
            {
                tcs.SetException(new TooBusyException());
            }

            return tcs.Task;
        }

       
    }

    public static class TaskExtensions
    {
        
        public static Task[] ContinueWithMany<T>(this Task<T> source, params Func<Task<T>,Task>[] continuations )
        {
            Task[] results = new Task[continuations.Length];
            for (int i = 0; i < continuations.Length; i++)
            {
                results[i] = continuations[i](source);
            }

            return results;
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            //  TransformMany();

          //  Batching();

          //  Errors();

         //   DangerOfCondiionalLinking();

           // SingleActionBlockLoadBalance();

            var greedy = new ExecutionDataflowBlockOptions();

            var nonGreedy = new ExecutionDataflowBlockOptions()
                {
                    BoundedCapacity = 1
                };

            ExecutionDataflowBlockOptions options = nonGreedy;

            var firstBlock = new ActionBlock<int>(i => Do(i,1,2),options);
            var secondBlock = new ActionBlock<int>(i => Do(i,2,1), options);
            var thirdBlock = new ActionBlock<int>(i => Do(i,3,2), options);

            var transform = new TransformBlock<int,int>(i=>i*2);

           transform.LinkTo(firstBlock);
           transform.LinkTo(secondBlock);
           transform.LinkTo(thirdBlock);

            for (int i = 0; i <= 10; i++)
            {
                transform.Post(i);
            }

            Console.ReadLine();
        }

        private static void Do(int workItem , int nWorker, int busyTimeInSeconds)
        {
            Console.WriteLine("Worker {0} Busy Processing {1}",nWorker,workItem);
            Thread.Sleep(busyTimeInSeconds * 1000 );
            Console.WriteLine("Worker {0} Done",nWorker);
        }

        private static void SingleActionBlockLoadBalance()
        {
            LoadBalancerAsync<int, string> balancer = new LoadBalancerAsync<int, string>(
                new Func<int, string>[]
                    {
                        i => (i*10).ToString(),
                        i => (i*100).ToString(),
                        i => (i*1000).ToString(),
                    }, 5);


            for (int i = 0; i < 10; i++)
            {
                balancer.DoAsync(i)
                        .ContinueWithMany(
                            ct =>
                            ct.ContinueWith(t => Console.WriteLine(t.Result),
                                            TaskContinuationOptions.OnlyOnRanToCompletion),
                            ct =>
                            ct.ContinueWith(t => Console.WriteLine(t.Exception.InnerExceptions.First()),
                                            TaskContinuationOptions.OnlyOnFaulted)
                    );
            }
            Console.ReadLine();
        }

        private static void DangerOfCondiionalLinking()
        {
            var block = new BufferBlock<int>();
            var nullBlock = DataflowBlock.NullTarget<int>();

            var oddBlock = new ActionBlock<int>(i => Console.WriteLine(i));

            block.LinkTo(oddBlock, i => i%2 == 1);
            block.LinkTo(nullBlock);

            int j = 0;
            while (true)
            {
                block.Post(++j);
            }
        }

        private static void Errors()
        {
            TransformBlock<int, int> errorBlock = new TransformBlock<int, int>(i =>
            {
                Console.WriteLine(Task.CurrentId);
                if (i%2 == 0)
                    throw new Exception("Boom");

                return i;
            }, new ExecutionDataflowBlockOptions() {BoundedCapacity = 4, MaxMessagesPerTask = 1});


            TransformBlock<int, int> nonErrorBlock = new TransformBlock<int, int>(i => i);


            ActionBlock<int> consumeBlock = new ActionBlock<int>(i => { Console.WriteLine(i); });

            errorBlock.LinkTo(consumeBlock); //, new DataflowLinkOptions() { PropagateCompletion = true});
            nonErrorBlock.LinkTo(consumeBlock);

            for (int i = 1; i < 10; i++)
            {
                bool postFailed = errorBlock.Post(i);
                nonErrorBlock.Post(i);
                if (errorBlock.Completion.IsFaulted)
                {
                    Console.WriteLine("Faulted");
                    ((IDataflowBlock) consumeBlock).Fault(errorBlock.Completion.Exception);
                }
                Thread.Sleep(1000);
            }
            Console.ReadLine();
            consumeBlock.Complete();
            try
            {
                consumeBlock.Completion.Wait();
            }
            catch (AggregateException errors)
            {
                foreach (Exception error in errors.Flatten().InnerExceptions)
                {
                    Console.WriteLine(error.Message);
                }
            }
        }

        private static void Batching()
        {
            BatchBlock<int> batchBlock = new BatchBlock<int>(10);
            ActionBlock<int[]> batchConsumer = new ActionBlock<int[]>(items =>
            {
                Console.WriteLine("Consuming batch");
                Thread.Sleep(1000);
            });

            batchBlock.LinkTo(batchConsumer, new DataflowLinkOptions() {PropagateCompletion = true});

            for (int i = 0; i < 20; i++)
            {
                batchBlock.Post(1);
            }

            batchBlock.Complete();
            batchConsumer.Completion.Wait();
        }

        private static void TransformMany()
        {
            var consumeBlock = new ActionBlock<int>(new Action<int>(Consumer));

            var tmb = new TransformManyBlock<int, int>(new Func<int, IEnumerable<int>>(ProduceMany));

            tmb.LinkTo(consumeBlock);

            tmb.Post(10);
            Console.ReadLine();
        }

        private static IEnumerable<int> ProduceMany(int i)
        {
            for (int j = 0; j < i; j++)
            {
                Console.WriteLine("{0}: Generating {1}", Task.CurrentId, j);
                yield return j;
                Thread.Sleep(1000);
            }
        }

        private static void Consumer(int obj)
        {
            Console.WriteLine("{0}:Consuming {1}",Task.CurrentId,obj);
        }
    }
}
