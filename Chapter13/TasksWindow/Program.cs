using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TasksWindow
{
    class Program
    {
        static void Main(string[] args)
        {
            object guard1 = new object();
            object guard2 = new object();

            Task.Run(() =>
                {
                    lock (guard1)
                    {
                        Thread.Sleep(10);
                        lock (guard2)
                        {

                        }
                    }
                });

            Task.Run(() =>
            {
                lock (guard2)
                {
                    Thread.Sleep(10);
                    lock (guard1)
                    {

                    }
                }
            });

            //Task.Run(() => DoWork())
            //    .ContinueWith(t =>
            //    {
            //        Console.WriteLine("Previous completed: {0}", t.Status);
            //    });

            Console.ReadLine();
        }

        static void DoWork()
        {
            Thread.Sleep(10000);
        }

        static void OldMain(string[] args)
        {
            var rnd = new Random();
            var ints = new List<int>(1000000);

            for (int i = 0; i < 1000000; i++)
            {
                ints.Add(rnd.Next(10000));
            }

            Parallel.ForEach(ints,
                () => 0,
                (i, loopState, locali) =>
                {
                    Thread.SpinWait(i * 10);
                    //         Thread.Sleep(0);
                    Thread.SpinWait(i * 10);
                    return i;
                },
            locali =>
            {
                Thread.Sleep(locali);
            });
        }
    }
}
