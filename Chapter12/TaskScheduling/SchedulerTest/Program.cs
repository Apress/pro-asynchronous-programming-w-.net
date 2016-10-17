using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RSKSchedulers;

namespace SchedulerTest
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            //STATaskScheduler scheduler = new STATaskScheduler(1);
            //var evt = new ManualResetEventSlim();

            //Task t1= new Task(() =>
            //    {
            //        evt.Wait();
            //    });

            //t1.Start(scheduler);

            //CancellationTokenSource cts = new CancellationTokenSource();
            //Task t2 = new Task(() =>
            //    {
            //        Console.WriteLine("Task 2 running");
            //        evt.Set();
            //    }, cts.Token
            //);

            //t2.Start(scheduler);

            ////t2.Wait();

            //cts.Cancel();
            //evt.Set();

            //scheduler.Dispose();

            var cts = new CancellationTokenSource();

            Task.Run(() =>
                {
                    Task t = Task.Factory.StartNew(() =>
                        {

                        }, cts.Token, TaskCreationOptions.None, new DoubleExecuteScheduler());
                });

            cts.Cancel();

            Console.ReadLine();


        }
    }
}
