using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ParallelLoops
{
    class Program
    {
        static void Main(string[] args)
        {
            // LoopOrderMatters();

       //     TradeShowLoops();

            const int piIterations = 1000000000;
       
            for (int i = 0; i < 3; i++)
            {
                TimeIt(CalculatePi, piIterations);
                TimeIt(ParallelCalculatePi, piIterations);
                TimeIt(OptimizedParallelCalculatePi, piIterations);
                TimeIt(OptimizedParallelCalculatePiWithPartitioner, piIterations);
                Console.WriteLine();
            }
            


            TimeIt(CalculatePiWithDelegateBody, piIterations);
            TimeIt(ReverseCalculatePi, piIterations);

            //SimpleForEach();
          //  Partitioning();

          //  EarlyLoopTermination();
            // PerformanceOfParallelForEachWithPureEnumerable();

        //    TwoLoopsCalculatePi();
        }

        private static double OptimizedParallelCalculatePi(int iterations)
        {
            IEnumerable<Tuple<int, int>> ranges = Range(3, iterations, 10000 ).ToList();
            double pi = 1;
            object combineLock = new object();
            //foreach (var range in ranges)
            Parallel.ForEach(ranges, () => 0.0,
                (range, loopState, localPi) =>
                {
                    double multiplier = range.Item1%2 == 0 ? 1 : -1;

                    for (int i = range.Item1; i < range.Item2; i += 2)
                    {
                        localPi += 1.0/(double) i*multiplier;
                        multiplier *= -1;
                    }

                    return localPi;
                },
                localPi =>
                {
                    lock (combineLock)
                    {
                        pi += localPi;
                    }
                });
            pi *= 4.0;

            return pi;
        }

        private static double OptimizedParallelCalculatePiWithPartitioner(int iterations)
        {
            //IEnumerable<Tuple<int, int>> ranges = Range(3, iterations, 10000);
            var ranges = Partitioner.Create(3, iterations, 10000);

            double pi = 1;
            object combineLock = new object();
            //foreach (var range in ranges)
            Parallel.ForEach(ranges, () => 0.0,
                (range, loopState, localPi) =>
                {
                    double multiplier = range.Item1 % 2 == 0 ? 1 : -1;

                    for (int i = range.Item1; i < range.Item2; i += 2)
                    {
                        localPi += 1.0 / (double)i * multiplier;
                        multiplier *= -1;
                    }

                    return localPi;
                },
                localPi =>
                {
                    lock (combineLock)
                    {
                        pi += localPi;
                    }
                });
            pi *= 4.0;

            return pi;
        }

        private static void TwoLoopsCalculatePi()
        {
            IEnumerable<Tuple<int, int>> ranges = Range(3, 100000000, 10000);
            double pi = 1;

            foreach (var range in ranges)
            {
                double multiplier = range.Item1%2 == 0 ? 1 : -1;

                for (int i = range.Item1; i < range.Item2; i += 2)
                {
                    pi += 1.0/(double) i*multiplier;
                    multiplier *= -1;
                }
            }
            pi *= 4.0;
            Console.WriteLine(pi);
        }

        private static IEnumerable<Tuple<int, int>> Range(int start, int end, int size)
        {
            for (int i = start; i < end; i+=size)
            {
                yield return Tuple.Create(i, Math.Min(i + size,end));
            }
        }

        private static void EarlyLoopTermination()
        {
            Random rnd = new Random();
            ParallelLoopResult loopResult =
                Parallel.For(0, 100, (i, loopState) =>
                {
                    if (rnd.Next(1, 50) == 1)
                    {
                        Console.WriteLine("{0} : Stopping on {1}", Task.CurrentId, i);
                        //loopState.Stop();
                        throw new Exception("Boom!");
                        return;
                    }

                    Thread.Sleep(10);

                    if (loopState.IsStopped)
                    {
                        Console.WriteLine("{0}:STOPPED", Task.CurrentId);
                        return;
                    }

                    Console.WriteLine("{0} : {1}", Task.CurrentId, i);
                });


            Console.WriteLine("Loop ran to completion {0}", loopResult.IsCompleted);
        }

        private static void PerformanceOfParallelForEachWithPureEnumerable()
        {
            int total = 0;
            IEnumerable<int> range = Enumerable.Range(1, 50000000).ToList();

            //range = new AsEnumerableAdapter<int>(range);

            Stopwatch timer = Stopwatch.StartNew();
            Parallel.ForEach(range, () => 0, (loopIndex, loopState, localSum) =>
            {
                localSum += loopIndex;
                return localSum;
            }, (localSum) => Interlocked.Add(ref total, localSum));

            Console.WriteLine(timer.Elapsed);
            Console.WriteLine(total);
        }

        private static void Partitioning()
        {
            int?[] taskCounters = new int?[100];

            var items =
                //Enumerable.Range(0, 12000);
                Partitioner.Create(0, 2000, 500);


            foreach (var partiion in items.GetOrderableDynamicPartitions())
            {
                Console.WriteLine(partiion);
            }

            return;

            Parallel.ForEach(items, i =>
            {
                int index = (int) Task.CurrentId;
                if (!taskCounters[index].HasValue)
                {
                    taskCounters[index] = 0;
                }
                taskCounters[index]++;
            });

            for (int nTaskCounter = 0; nTaskCounter < taskCounters.Length; nTaskCounter++)
            {
                if (taskCounters[nTaskCounter].HasValue)
                {
                    Console.WriteLine("{0} = {1}", nTaskCounter, taskCounters[nTaskCounter].Value);
                }
            }

            // Parallel.ForEach(FromTo(1, 2, 0.1), Console.WriteLine);
        }

        private static IEnumerable<double> FromTo(double start, double end,double step)
        {
            for (double current = start; current < end; current += step)
            {
                yield return current;
            }
        }

        private static void SimpleForEach()
        {
            IEnumerable<int> items = Enumerable.Range(0, 20);

            Parallel.ForEach(items, i => { Console.WriteLine(i); });
        }

        private static void TimeIt( Func<int,double> func, int iterations)
        {
            Stopwatch timer = Stopwatch.StartNew();
            double result = func(iterations);
            Console.WriteLine("{0}({1}) produced {2} and took {3}",
                func.Method.Name,iterations,result,timer.Elapsed);
        }

        private static void TradeShowLoops()
        {
            Parallel.For(0, 20,
                         new ParallelOptions() {MaxDegreeOfParallelism = 2},
                         i =>
                             {
                                 Console.WriteLine("Executing {0} : {1}", Task.CurrentId, i);
                                 Thread.SpinWait(100000000);
                                 Console.WriteLine("Completing {0} : {1}", Task.CurrentId, i);
                             });

            Parallel.For(0, 20,
                         new ParallelOptions() {MaxDegreeOfParallelism = 2},
                         i =>
                             {
                                 Console.WriteLine("{0} : {1}", Task.CurrentId, i);
                                 Thread.SpinWait(100000000);
                             });
        }

        private static double CalculatePi(int iterations)
        {
            double pi = 1;
            double multiplier = -1;
            
            for (int i = 3; i < iterations; i+=2)
            {
                pi += 1.0/(double) i*multiplier;
                multiplier *= -1;
                
            }
            
            return pi*4.0;
        }

        private static double CalculatePiWithDelegateBody(int iterations)
        {
            double pi = 1;
            double multiplier = -1;

            Func<int,double,double> body = (i,local) =>
            {
                local += 1.0 / (double)i * multiplier;
                multiplier *= -1;

                return local;
            };

            double localLoopValue = 0;
            for (int i = 3; i < iterations; i += 2)
            {
                localLoopValue = body(i, localLoopValue);
            }

            pi += localLoopValue;

            return pi*4.0;
        }

       

        private static double ReverseCalculatePi(int iterations)
        {
            double pi = 1;
            double multiplier = -1;
           
            for (int i = iterations-1; i >= 3; i -=2)
            {
                pi += 1.0 / (double)i * multiplier;
                multiplier *= -1;
               
            }
            
            return pi * 4.0;
        }

        private static double BadParallelCalculatePi(int iterations)
        {
            double pi = 1;
            double multiplier = -1;
            Parallel.For(0, (iterations - 3)/2, loopIndex =>
                {
                    int i = 3 + loopIndex*2;
                    pi += 1.0/(double) i*multiplier;
                    multiplier *= -1;
                });

            return pi * 4.0;
        }

        private static double ParallelCalculatePi(int iterations)
        {
            double pi = 1;
            object combineLock = new object();

            List<double> results = new List<double>();
            Parallel.For(0, (iterations - 3) / 2,
                InitialiseLocalPi,
                (int loopIndex, ParallelLoopState loopState, double localPi) =>
                {
                    double multiplier = loopIndex%2 == 0 ? -1 : 1;
                    int i = 3 + loopIndex*2;
                    localPi += 1.0/(double) i*multiplier;
                   
                    return localPi;
                },
            (double localPi) =>
            {
                lock (combineLock)
                {
                    //pi += localPi;
                    results.Add(localPi);
                }
            });

            pi += results.OrderByDescending(r => r).Sum();

            return pi * 4.0;
        }

        private static double InitialiseLocalPi()
        {
            return 0.0;
        }

        private static void LoopOrderMatters()
        {
            int prev = 0;
            int prevPrev = 0;
            int current = 1;

            for (int nFib = 0; nFib < 20; nFib++)
            {
                Console.WriteLine(current);

                prevPrev = prev;
                prev = current;

                current = prev + prevPrev;
            }
        }
    }
}
