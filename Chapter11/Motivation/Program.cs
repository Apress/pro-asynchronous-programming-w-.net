using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Motivation
{
    class Program
    {
        static void Main(string[] args)
        {
            TimeIt(Synchronous);
            TimeIt(OldStyleAsync);

            TimeIt(TaskBasedAsync);

            TimeIt(ParallelInvokeSum);

           // ParallelInvokeTimeoutManyActions();

          //  ParallelInvokeCancellation();

            CancellationTokenSource cts = new CancellationTokenSource();

            int sumX = 0;
            int sumY = 0;
            int sumZ = 0;

            cts.CancelAfter(2000);
           
            try
            {
                Parallel.Invoke(
                    new ParallelOptions() { CancellationToken = cts.Token },
                    () => { throw new Exception("Boom!"); },
                    () => sumY = SumY(cts.Token),
                    () => sumZ = SumZ(cts.Token)
                    );

                int total = sumX + sumY + sumZ;
                Console.WriteLine(total);
            }
            catch (OperationCanceledException operationCanceled)
            {
                Console.WriteLine("Cancelled");
            }
            catch (AggregateException errors)
            {
                foreach (Exception error in errors.Flatten().InnerExceptions)
                {
                    Console.WriteLine(error.Message);
                }
            }
        }

        private static int BadSumX()
        {
            throw new Exception("Boom");
        }

        private static void ParallelInvokeCancellation()
        {
            CancellationTokenSource cts = new CancellationTokenSource();

            int sumX = 0;
            int sumY = 0;
            int sumZ = 0;

            cts.CancelAfter(2000);

            try
            {
                Parallel.Invoke(
                    new ParallelOptions() {CancellationToken = cts.Token},
                    () => sumX = SumX(cts.Token),
                    () => sumY = SumY(cts.Token),
                    () => sumZ = SumZ(cts.Token)
                    );

                int total = sumX + sumY + sumZ;
                Console.WriteLine(total);
            }
            catch (OperationCanceledException operationCanceled)
            {
                Console.WriteLine("Cancelled");
            }
        }

        private static void ParallelInvokeTimeoutManyActions()
        {
            CancellationTokenSource cts = new CancellationTokenSource();

            int nSleepyMethods = 100;
            Action[] sleepyActions = new Action[nSleepyMethods];
            for (int nSleepyMethod = 0; nSleepyMethod < sleepyActions.Length; nSleepyMethod++)
            {
                sleepyActions[nSleepyMethod] = () =>
                    {
                        Console.WriteLine("{0} Running...", Task.CurrentId);
                        Thread.Sleep(5000);
                        Console.WriteLine("{0} End", Task.CurrentId);
                    };
            }

            cts.CancelAfter(2000);
            Parallel.Invoke(new ParallelOptions()
                {
                    CancellationToken = cts.Token
                }, sleepyActions);
            Console.WriteLine("Done");
        }

        private static void ParallelInvokeSum()
        {
            int sumX = 0;
            int sumY = 0;
            int sumZ = 0;

            Parallel.Invoke(
                () => sumX = SumX(),
                () => sumY = SumY(),
                () => sumZ = SumZ()
                );

            int total = sumX + sumY + sumZ;
            Console.WriteLine(total);
        }

        private static void TaskBasedAsync()
        {
            Task<int> sumX = Task.Factory.StartNew<int>(SumX);
            Task<int> sumY = Task.Factory.StartNew<int>(SumY);
            Task<int> sumZ = Task.Factory.StartNew<int>(SumZ);

            int total = sumX.Result + sumY.Result + sumZ.Result;

            Console.WriteLine(total);
        }

        public static void TimeIt(Action action)
        {
            Stopwatch timer = Stopwatch.StartNew();
            action();
            Console.WriteLine("{0} took {1}",action.Method.Name,timer.Elapsed);
        }

        private static void OldStyleAsync()
        {
            Func<int> sumX = SumX;
            Func<int> sumY = SumY;
            Func<int> sumZ = SumZ;

            IAsyncResult sumXAsyncResult = sumX.BeginInvoke(null, null);
            IAsyncResult sumYAsyncResult = sumY.BeginInvoke(null, null);
            IAsyncResult sumZAsyncResult = sumZ.BeginInvoke(null, null);

            int total = sumX.EndInvoke(sumXAsyncResult) +
                        sumY.EndInvoke(sumYAsyncResult) +
                        sumZ.EndInvoke(sumZAsyncResult);

            Console.WriteLine(total);
        }

        private static void Synchronous()
        {
            int total = SumX() + SumY() + SumZ();

            Console.WriteLine(total);
        }


        public static int SumX()
        {
            return SumX(new CancellationToken());
        }

        public static int SumX(CancellationToken ct)
        {
            //Thread.SpinWait(10*10000000);
            ct.WaitHandle.WaitOne(5*1000);
            ct.ThrowIfCancellationRequested();

            return 1;
        }

        public static int SumY()
        {
            return SumY(new CancellationToken());
        }

        public static int SumY(CancellationToken ct)
        {
           // Thread.SpinWait(5 * 10000000);
            ct.WaitHandle.WaitOne(1*1000);
            ct.ThrowIfCancellationRequested();
            return 2;
        }

        public static int SumZ()
        {
            return SumZ(new CancellationToken());
        }

        public static int SumZ(CancellationToken ct)
    {
        //Thread.SpinWait(10*10000000);
            ct.WaitHandle.WaitOne(4*1000);
            ct.ThrowIfCancellationRequested();
            return 3;
    }
    }
}
