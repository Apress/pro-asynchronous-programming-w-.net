using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bag
{
    class Program
    {
        // Change this directory to a root that will contain many folders and hopefully some .cs files
        private static readonly string directoryToWalkPath = @"D:\temp";

        static void Main(string[] args)
        {
            BenchMarkConcurrentInsertion();

//            BenchmarkConsumerProducer();

          
        }

        private static void BenchmarkConsumerProducer()
        {
            while (true)
            {
                TimeIt(ImprovedParallelFileFinderWithBag);
                TimeIt(BagFindFiles);
                TimeIt(RecurssionFindFiles);
                Console.Read();
            }
        }

        private static void ImprovedParallelFileFinderWithBag()
        {
            Console.WriteLine(BetterParallelFileFinderWithBag.FindAllFiles(directoryToWalkPath, "*.cs").Count);
        }

        private static void BenchMarkConcurrentInsertion()
        {
            while (true)
            {
                TimeIt(ListCollection);

                TimeIt(BagCollection);
                Console.Read();
            }
        }

        private static void RecurssionFindFiles()
        {
            Console.WriteLine(ParallelFindFilesWithCallStackRecursion.FindAllFiles(directoryToWalkPath, "*.cs").Count);
        }

        private static void BagFindFiles()
        {
            Console.WriteLine(ParallelFileFinderWithBag.FindAllFiles(directoryToWalkPath, "*.cs").Count);
        }

        private static void BagCollection()
        {
            ExerciseCollection(() => new ConcurrentBag<int>(),(bag,val) => bag.Add(val));
        }

        private static void ListCollection()
        {
            ExerciseCollection(
                () => new List<int>(),
                (list, val) => { lock (list) list.Add(val); });
        }

        public static void TimeIt(Action action)
        {
            Stopwatch timer = Stopwatch.StartNew();
            action();
            timer.Stop();
            Console.WriteLine("{0} took {1}",action.Method.Name,timer.Elapsed);
        }

        
    
        private const int NTASKS = 4;
        private const int INSERTIONS = 800000;

       
        private static void ExerciseCollection<T>(Func<T> createCollection , Action<T,int> insertion ) where T:IEnumerable<int>
        {
            T collection = createCollection();

            var tasks = new Task[NTASKS];
            var barrier = new Barrier(tasks.Length);
            for (int nTask = 0; nTask < tasks.Length; nTask++)
            {
                tasks[nTask] =
                    Task.Run(() =>
                    {
                        barrier.SignalAndWait();
                        Random rnd = new Random();
                        for (int i = 0; i < INSERTIONS; i++)
                        {
                            int val = rnd.Next(1, 1000);
                         
                                insertion(collection, val);
                            
                        }
                    });
            }
            Task.WaitAll(tasks);
            int total = collection.Sum();
        }


      
    }
}
