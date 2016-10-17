using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Joining
{
    [ServiceContract]
    public interface IGridNode<in T>
    {
        [OperationContract]
        Task InvokeAsync(T workItem);
    }

    public class GridDispatcher<T>
    {
        private BufferBlock<T> workItems = new BufferBlock<T>();
        private BufferBlock<Uri> nodes = new BufferBlock<Uri>();
        private JoinBlock<Uri, T> scheduler = new JoinBlock<Uri, T>(new GroupingDataflowBlockOptions() { Greedy = false});
   
        private ActionBlock<Tuple<Uri, T>> dispatcher;
        private ActionBlock<T> localDispatcher;

        public GridDispatcher(IGridNode<T> localService )
        {
            localDispatcher = new ActionBlock<T>(wi =>
            {
                Console.WriteLine("Executing Locally");
                return localService.InvokeAsync(wi);
            },
            new ExecutionDataflowBlockOptions(){ BoundedCapacity = 2,MaxDegreeOfParallelism = 2});

            dispatcher  = new ActionBlock<Tuple<Uri, T>>((Action<Tuple<Uri, T>>) Dispatch);

            workItems.LinkTo(localDispatcher);
            workItems.LinkTo(scheduler.Target2);
            nodes.LinkTo(scheduler.Target1);


            scheduler.LinkTo(dispatcher);
        }

        
        public void RegisterNode(Uri uri)
        {
            nodes.Post(uri);
        }

        public void SubmitWork(T workItem)
        {
            workItems.Post(workItem);
        }

        private void Dispatch(Tuple<Uri, T> nodeAndWorkItemPair)
        {
            var cf = new ChannelFactory<IGridNode<T>>(new NetTcpBinding(), new EndpointAddress(nodeAndWorkItemPair.Item1));

            IGridNode<T> proxy = cf.CreateChannel();
            proxy.InvokeAsync(nodeAndWorkItemPair.Item2)
                .ContinueWith(t =>
                {
                    ((IClientChannel)proxy).Close();
                    nodes.Post(nodeAndWorkItemPair.Item1);
                });
        }

    }

    public class WorkItem
    {
        public int lhs { get; set; }
        public int rhs { get; set; }
    }

    public class NodeWorker : IGridNode<WorkItem>
    {
        public Task InvokeAsync(WorkItem workItem)
        {
            int result = workItem.lhs + workItem.rhs;
            Console.WriteLine(result);
            return Task.Delay(3000);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
              //TheAtomicCafe();

            //SimpleJoin();

           // InPairs();

            var host = new ServiceHost(typeof(NodeWorker));
            host.AddServiceEndpoint(typeof (IGridNode<WorkItem>), new NetTcpBinding(),"net.tcp://localhost:9000/Worker" );
            host.Open();

            GridDispatcher<WorkItem> grid = new GridDispatcher<WorkItem>(new NodeWorker());

           // grid.RegisterNode(new Uri("net.tcp://localhost:9000/Worker"));
           // grid.RegisterNode(new Uri("net.tcp://localhost:9000/Worker"));

            for (int i = 0; i < 10; i++)
            {
                
                grid.SubmitWork(new WorkItem()
                {
                    lhs = i,
                    rhs = 4,
                });
            }

            Console.ReadLine();
        }

        private static void InPairs()
        {
            var broadcastBlock = new BroadcastBlock<int>(i => i);

            var bufferOne = new BufferBlock<int>();
            var bufferTwo = new BufferBlock<int>();

            var firstJoinBlock = new JoinBlock<int, int>(new GroupingDataflowBlockOptions() {Greedy = false});

            var consumer = new ActionBlock<Tuple<int, int>>(tuple =>
            {
                if (tuple.Item1 != tuple.Item2)
                    Console.WriteLine(tuple);
            });

            bufferOne.LinkTo(firstJoinBlock.Target1);
            bufferTwo.LinkTo(firstJoinBlock.Target2);

            firstJoinBlock.LinkTo(consumer);

            broadcastBlock.LinkTo(bufferOne);
            broadcastBlock.LinkTo(bufferTwo);


            for (int nTask = 0; nTask < 4; nTask++)
            {
                int localTask = nTask;
                Task.Factory.StartNew(() =>
                {
                    while (true)
                    {
                        broadcastBlock.Post(localTask);
                        // Could cause join to pair incorrectly

                        //bufferOne.Post(localTask);
                        //bufferTwo.Post(localTask);
                    }
                });
            }


            Console.ReadLine();
        }

        private static void SimpleJoin()
        {
            var bufferOne = new BufferBlock<int>();
            var bufferTwo = new BufferBlock<int>();

            var firstJoinBlock = new JoinBlock<int, int>(new GroupingDataflowBlockOptions() {Greedy = false});

            var consumer = new ActionBlock<Tuple<int, int>>(tuple => { Console.WriteLine(tuple); });

            bufferOne.LinkTo(firstJoinBlock.Target1);
            bufferTwo.LinkTo(firstJoinBlock.Target2);

            firstJoinBlock.LinkTo(consumer);


            bufferOne.Post(1);
            bufferTwo.Post(1);

            Console.ReadLine();
        }

        private static void TheAtomicCafe()
        {
            Resteraunt resteraunt = new Resteraunt(2);
            
            Waiter waiter = new Waiter(resteraunt);

            

            ConsoleKey key;
            Chef chef = new Chef(resteraunt);
            int id = 1;

            while ((key = Console.ReadKey(true).Key) != ConsoleKey.Q)
            {
                if (key == ConsoleKey.C)
                {
                    chef.MakeFoodAsync();
                }

                if (key == ConsoleKey.N)
                {
                    resteraunt.Customers.Post(new Customer(id++));
                }
            }
        }
    }
}
