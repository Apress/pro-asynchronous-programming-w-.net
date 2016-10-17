using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CacheLines
{
    public interface IIncrement
    {
        void Increment(int offset);
        
    }

    public class SameCacheLine : IIncrement
    {
        private int[] data = new int[20];

        public void Increment(int offset)
        {
            data[offset]++;
        }
    }

    public class DifferentCacheLine : IIncrement
    {
       
        private int[] data = new int[64];

        public void Increment(int offset)
        {
            data[offset*32]++;
        }
    }

    class Program
    {
        private static void Main(string[] args)
        {
            IIncrement sameCacheLine = new SameCacheLine();
            IIncrement differentCacheLine = new DifferentCacheLine();

            while (true)
            {
                DoItAndTimetIt(sameCacheLine);

                DoItAndTimetIt(differentCacheLine);
            }
        

        Console.ReadLine();
        }

        private static void DoItAndTimetIt(IIncrement toIncrement)
        {
            
            int iterations = 1000000000;

            Task[] tasks = new Task[2];
            Barrier barrier = new Barrier(tasks.Length+1);

            for (int nTask = 0; nTask < tasks.Length; nTask++)
            {
                int localTask = nTask;

                tasks[nTask] = Task.Factory.StartNew(() =>
                {
                    barrier.SignalAndWait();
                    for (int i = 0; i < 1000000000; i++)
                    {
                        toIncrement.Increment(localTask);
                    }
                });
            }
            barrier.SignalAndWait();
            Stopwatch timer = Stopwatch.StartNew();

            Task.WaitAll(tasks);
            timer.Stop();
            Console.WriteLine("{0} took {1}",toIncrement.GetType().Name,timer.Elapsed);
          
        }
    }
}
