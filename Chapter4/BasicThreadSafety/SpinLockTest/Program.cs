using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SpinLocks;
using SpinLock = System.Threading.SpinLock;

namespace SpinLockTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var spin = new SpinLocks.SpinLock();
            Task t1 = Task.Factory.StartNew(() =>
                {
                    spin.Lock();

                    Console.WriteLine("Press enter to release lock");
                    Console.ReadLine();

                    spin.Unlock();
                });

            Task t2 = Task.Factory.StartNew(() =>
                {
                    Thread.Sleep(500);
                    Console.WriteLine("task 2 trying to get lock");
                    spin.Lock();
                    Console.WriteLine("Task 2 has lock");
                    spin.Unlock();
                });

            Task.WaitAll(t1, t2);
        }
    }
}
