using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataMatching
{
    class Program
    {
        static void Main(string[] args)
        {
            var matchers = new[] {"dowjones", "ftse", "nasdaq", "dax"};
            var controlFileAvailable = new ManualResetEventSlim();
            var tasks = new List<Task>();
            var initializationComplete = new CountdownEvent(matchers.Length);
            foreach (string matcherName in matchers)
            {
                var matcher = new Matcher(matcherName, MatchesFound, controlFileAvailable, initializationComplete);
                tasks.Add(matcher.Process());
            }

            initializationComplete.Wait();

            Console.WriteLine("Press enter when control file ready");
            Console.ReadLine();

            controlFileAvailable.Set();

            Task.WaitAll(tasks.ToArray());
        }

        static object outputGuard = new object();

        static void MatchesFound(string dataSource, IEnumerable<TradeDay> matchingDays)
        {
            lock (outputGuard)
            {
                Console.WriteLine("Matches for {0}", dataSource);
                if (matchingDays == null)
                {
                    Console.WriteLine("\tNo match requested");
                }
                else if (!matchingDays.Any())
                {
                    Console.WriteLine("\tNo matches");
                }
                else
                {
                    foreach (TradeDay matchingDay in matchingDays)
                    {
                        Console.WriteLine("\t{0}: {1}", matchingDay.Date, matchingDay.Volume);
                    }
                }
            }
        }
    }
}
