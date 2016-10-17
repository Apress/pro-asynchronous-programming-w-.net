using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncAwaitMechanics
{


    public class TickTockAsyncStateMachine
    {
        private int state = 0;
        private TaskAwaiter awaiter;

        public void MoveNext()
        {

            switch (state)
            {
                case 0:
                    {
                        goto firstState;
                    }
                    break;
                case 1:
                    {
                        goto secondState;
                    }
                    break;
                case 2:
                    {
                        goto thirdState;
                    }
                    break;
            }
            firstState:
            Console.WriteLine("Starting clock");
           
            secondState:
            Console.Write("Tick");
            awaiter = Task.Delay(500).GetAwaiter();
            if (!awaiter.IsCompleted)
            {
                state = 2;
                awaiter.OnCompleted(MoveNext);
                return;
            }
            thirdState:
            Console.WriteLine("Tock");
            awaiter = Task.Delay(500).GetAwaiter();
            if (!awaiter.IsCompleted)
            {
                state = 1;
                awaiter.OnCompleted(MoveNext);
                return;
            }

            goto secondState;
        }
    }

    public class MySynchronizationContext : SynchronizationContext
    {
        BlockingCollection<Tuple<SendOrPostCallback,object>> queue = 
            new BlockingCollection<Tuple<SendOrPostCallback,object>>(
            new ConcurrentQueue<Tuple<SendOrPostCallback,object>>());

        public MySynchronizationContext()
        {
            Task.Run(() =>
                {
                    SynchronizationContext.SetSynchronizationContext(this);
                    foreach (var callback in queue.GetConsumingEnumerable())
                    {

                        callback.Item1(callback.Item2);
                    }
                });
        }
        public override void Post(SendOrPostCallback d, object state)
        {
            queue.Add(new Tuple<SendOrPostCallback, object>(d,state));
        }

        public override void Send(SendOrPostCallback d, object state)
        {
            base.Send(d, state);
        }
    }

    public struct WaitingForEverAwaiter : INotifyCompletion
    {
        public bool IsCompleted { get { return false; } }
        public void OnCompleted(Action continuation)
        {
            return;
        }

        public int GetResult()
        {
            return 0;
        }
    }

    public class WaitingForEverAwaiterSource
    {
        public  WaitingForEverAwaiter GetAwaiter()
        {
            return default(WaitingForEverAwaiter);
        }
    }

   

    class Program
    {
        static void Main(string[] args)
        {
            SynchronizationContext.SetSynchronizationContext(new MySynchronizationContext());

            //SynchronizationContext.Current.Post(_ => DoItAsync(), null);
            TickTockAsync();
            Console.ReadLine();
        }

        private static void TickTockAsync()
        {
            var stateMachine = new TickTockAsyncStateMachine();
            stateMachine.MoveNext();
        }

        private static async void DoItAsyncCompiler()
        {
            
            await new WaitingForEverAwaiterSource();

            Console.WriteLine("Starting the clock..");
            while (true)
            {
                Console.Write("Tick ");

                await Task.Delay(500);

                Console.WriteLine("Tock");

                await Task.Delay(500);
            }
        }
    }
}
