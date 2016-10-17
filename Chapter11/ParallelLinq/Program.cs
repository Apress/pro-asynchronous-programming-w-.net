using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ParallelLinq
{
    class Program
    {
        static void Main(string[] args)
        {
           // SimpleBenchmark();



            double pi =
                Inputs(1000000000).AsParallel()
                    .Select(i => 1.0/(double) i)
                    .Aggregate<double,double>( 0.0,
                        (total, nextValue) => total + nextValue);
            Console.WriteLine(pi*4.0 );

        }

        public static IEnumerable<int> Inputs(int iterations)
        {
            int multiplier = 1;
            for (int i = 3; i < iterations; i+=2)
            {
                yield return multiplier*i;
                multiplier *= -1;
            }
        }

        private static void SimpleBenchmark()
        {
            IEnumerable<int> numbers = Enumerable.Range(0, 100000000).ToList();

            while (true)
            {
                TimeIt(() =>
                {
                    var evenNumbers = from number in numbers.AsParallel()
                        where number%2 == 0
                        select number;

                    Console.WriteLine(evenNumbers.Count());
                });
            }
        }

        private static void TimeIt(Action func)
        {
            Stopwatch timer = Stopwatch.StartNew();
            func();
            Console.WriteLine("{0}() and took {1}",
                func.Method.Name, timer.Elapsed);
        }
    }
}
