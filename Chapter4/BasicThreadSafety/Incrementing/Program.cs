using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Incrementing
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Press enter to start");
            Console.ReadLine();
            const int iterations = 500000000;
            const int numTasks = 2;
            List<Task<TimeSpan>> tasks = new List<Task<TimeSpan>>();
            int value = 0;

            for (int nTask = 0; nTask < numTasks; nTask++)
            {
                Task<TimeSpan> t = Task.Factory.StartNew<TimeSpan>(() =>
                    {
                        Stopwatch sw = Stopwatch.StartNew();
                        IncrementValue(ref value, iterations);
                        return sw.Elapsed;
                    });

                tasks.Add(t);
            }

            Task.WaitAll(tasks.ToArray());

            var totalTime = new TimeSpan();
            foreach (Task<TimeSpan> task in tasks)
            {
                totalTime += task.Result;
            }
            Console.WriteLine("Expected value: {0}, Actual value: {1}, Time taken: {2}", numTasks * iterations, value, totalTime);
        }

        private static void IncrementValue(ref int value, int iterations)
        {
            for (int i = 0; i < iterations; i++)
            {
               // value++;
                Interlocked.Increment(ref value);
            }
        }
    }
}
