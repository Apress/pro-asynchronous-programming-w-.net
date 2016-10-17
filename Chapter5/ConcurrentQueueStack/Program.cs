using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConcurrentQueueStack
{
    public class WeatherSummary
    {
        public string Location { get; set; }
        public decimal? AverageHoursOfSun { get; set; }
    }
    class Program
    {
        static void Main(string[] args)
        {
            var queue = new ConcurrentQueue<string>();
            
            foreach (FileInfo file in new DirectoryInfo(@"C:\Repository").GetFiles("*.csv", SearchOption.AllDirectories))
            {
                queue.Enqueue(file.FullName);
            }
               

            var consumers = new Task[4];
            for (int i = 0; i < consumers.Length; i++)
            {
                consumers[i] = Task.Run(() => Consumer(queue));
            }

            Task.WaitAll(consumers);
        }

        public static void Consumer(ConcurrentQueue<string> queue)
        {
            string file;
            while (queue.TryDequeue(out file))
            {
                Console.WriteLine("Processing {0}:{1}",Task.CurrentId,file);
            }
        }


        public static void TimeIt(Action action)
        {
            Stopwatch timer = Stopwatch.StartNew();
            action();
            timer.Stop();
            Console.WriteLine("{0} took {1}", action.Method.Name, timer.Elapsed);
        }

        

        private static IEnumerable<string[]> LoadData(string filename)
        {
            using (var reader = new StreamReader(filename))
            {
                while (!reader.EndOfStream)
                {
                    yield return reader.ReadLine().Split(',');
                }
            }
        }
    }
}
