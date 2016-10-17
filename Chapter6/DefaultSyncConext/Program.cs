using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DefaultSyncConext
{
    class Program
    {
        static void Main(string[] args)
        {
            SynchronizationContext ctx = new SynchronizationContext();

            PrintThreadInfo();


            ctx.Post(
                _ =>
                PrintThreadInfo(), null);


            Console.ReadLine();

            ctx.Send(_=>PrintThreadInfo(),null);



        }

        private static void PrintThreadInfo()
        {
            Console.WriteLine("Thread {0} , {1}", Thread.CurrentThread.ManagedThreadId,
                              Thread.CurrentThread.IsThreadPoolThread);
        }
    }
}
