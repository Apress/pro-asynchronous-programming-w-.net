using LOHPool;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace App
{
    class Program
    {
        static void Main(string[] args)
        {
            var pool = new BufferPool(2);

            IBufferRegistration reg = pool.GetBuffer();

            Task.Factory.StartNew(() =>
                {
                    IBufferRegistration reg2 = pool.GetBuffer();
                    Console.WriteLine("task got buffer");
                });

            Console.WriteLine("press enter to dispose reg");
            Console.ReadLine();
            reg.Dispose();

            Console.ReadLine();
        }

        private static void RWSLock()
        {
            var rwLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

            rwLock.EnterReadLock();
            Console.WriteLine(rwLock.CurrentReadCount);
            rwLock.EnterReadLock();

            Console.WriteLine(rwLock.CurrentReadCount);

            Console.WriteLine("got write lock");
        }

        private static void TimeLocks()
        {
            var rwLock = new ReaderWriterLock();
            var rwls = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

            var sw = Stopwatch.StartNew();
            for (int i = 0; i < 100000000; i++)
            {
                rwls.EnterUpgradeableReadLock();
                rwls.ExitUpgradeableReadLock();
                //lock (rwLock)
                //{
                //}
                //rwLock.AcquireReaderLock(10);
                //rwLock.ReleaseReaderLock();
            }
            Console.WriteLine(sw.Elapsed);
        }

        private static void RWLock(ReaderWriterLock rwLock)
        {
            Console.ReadLine();
            rwLock.AcquireWriterLock(TimeSpan.FromSeconds(3));
            rwLock.AcquireReaderLock(TimeSpan.FromSeconds(1));

            Console.WriteLine("got lock");
            try
            {
                ReadState();
            }
            finally
            {
                rwLock.ReleaseReaderLock();
                rwLock.ReleaseWriterLock();
            }
        }

        private static void ReadState()
        {
           
        }

        private static void UseSemaphore()
        {
            var sem = new MonitorSemaphore(0, 6);

            Action<int, string> print =
                (task, msg) => { Console.WriteLine("{0} {1}: Sem count:{2}", task, msg, sem.CurrentCount); };
            var rnd = new Random();
            for (int i = 0; i < 10; i++)
            {
                int locali = i;
                Task.Factory.StartNew(() =>
                    {
                        while (true)
                        {
                            sem.Enter();
                            print(locali, "Acquired");
                            Thread.Sleep(rnd.Next(3000));
                            sem.Exit();
                            print(locali, "Released");
                        }
                    });
            }
        }

        private static void ProducerConsumer()
        {
            var queue = new Queue<int>();
            Task producer = Task.Factory.StartNew(Produce, queue);
            Task consumer = Task.Factory.StartNew(Consume, queue);
        }

        private static void Produce(object obj)
        {
            var queue = (Queue<int>)obj;
            var rnd = new Random();

            while (true)
            {
                lock (queue)
                {
                    queue.Enqueue(rnd.Next(100));

                    Monitor.Pulse(queue);
                }

                Thread.Sleep(rnd.Next(2000));
            }
        }

        private static void Consume(object obj)
        {
            var queue = (Queue<int>)obj;

            while (true)
            {
                int val;
                lock (queue)
                {
                    while (queue.Count == 0)
                    {
                        Monitor.Wait(queue);
                    }

                    val = queue.Dequeue();
                }

                ProcessValue(val);
            }
        }

        private static void ProcessValue(int val)
        {
            Console.WriteLine(val);
        }


        private static void perftest()
        {
            int count = 0;

            while (true)
            {
                new object();
                count++;
                if (count%100000000 == 0)
                {
                    Console.Write(".");
                }
            }
        }
    }

    public class SmallBusiness
    {
        private decimal cash;
        private decimal receivables;

        private readonly object stateGuard = new object();

        public SmallBusiness(decimal cash, decimal receivables)
        {
            this.cash = cash;
            this.receivables = receivables;
        }



        //public void ReceivePayment(decimal amount)
        //{
        //    lock(stateGuard)
        //    {
        //        cash += amount;
        //        receivables -= amount;
        //    }
        //}

        //public void ReceivePayment(decimal amount)
        //{
        //    bool lockTaken = false;

        //    try
        //    {
        //        Monitor.TryEnter(stateGuard, TimeSpan.FromSeconds(30), ref lockTaken);
        //        if (lockTaken)
        //        {
        //            cash += amount;
        //            receivables -= amount;
        //        }
        //        else
        //        {
        //            throw new TimeoutException("Failed to aquire stateGuard");
        //        }
        //    }
        //    finally
        //    {
        //        if (lockTaken)
        //        {
        //            Monitor.Exit(stateGuard);
        //        }
        //    }
        //}

        public void ReceivePayment(decimal amount)
        {
            using(stateGuard.Lock(TimeSpan.FromSeconds(30)))
            {
                cash += amount;
                receivables -= amount;
            }
        }

        public decimal NetWorth
        {
            get
            {
                Monitor.Enter(stateGuard);
                decimal netWorth = cash + receivables;
                Monitor.Exit(stateGuard);
                return netWorth;
            }
        }
    }

    public static class LockExtensions
    {
        public static IDisposable Lock(this object obj, TimeSpan timeout)
        {
            bool lockTaken = false;

            try
            {
                Monitor.TryEnter(obj, TimeSpan.FromSeconds(30), ref lockTaken);
                if (lockTaken)
                {
                    return new LockHelper(obj);
                }
                else
                {
                    throw new TimeoutException("Failed to aquire stateGuard");
                }
            }
            catch
            {
                if (lockTaken)
                {
                    Monitor.Exit(obj);
                }
                throw;
            }
        }

        private struct LockHelper : IDisposable
        {
            private readonly object obj;

            public LockHelper(object obj)
            {
                this.obj = obj;
            }

            public void Dispose()
            {
                Monitor.Exit(obj);
            }
        }

    }
}


