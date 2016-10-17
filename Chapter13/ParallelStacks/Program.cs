using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ParallelStacks
{
    class Program
    {
       static  Dictionary<DataTarget, string[]> dataTargetMap = new Dictionary<DataTarget, string[]>(); 
        static void Main(string[] args)
        {
            PopulateMap();

            CheckForCancellation();

            PrintValues().Wait();
        }

        private static void CheckForCancellation()
        {
            Task.Run(() =>
                {
                    Console.ReadLine();
                });
        }

        private static void PopulateMap()
        {
           dataTargetMap.Add(DataTarget.FTSE, new[]
               {
                   "http://www.microsoft.com",
                   "http://www.oracle.com",
                   "http://www.google.com",
                   "http://www.apple.com",
               });

           dataTargetMap.Add(DataTarget.HangSeng, new[]
               {
                   "http://www.samsung.com",
                   "http://www.bbc.co.uk",
               });


           dataTargetMap.Add(DataTarget.DowJones, new[]
               {
                   "http://www.facebook.com",
                   "http://www.twitter.com",
                   "http://www.dropbox.com",
               });

           dataTargetMap.Add(DataTarget.NASDAQ, new[]
               {
                   "http://www.cnn.com",
                   "http://www.sky.com",
                   "http://www.itv.com",
               });
        }

        static string[] GetUrls(DataTarget target)
        {
            return dataTargetMap[target];
        }

        static async Task PrintValues()
        {
            List<Task> tasks = new List<Task>();
            for (int i = 0; i < 4; i++)
            {
                int i1 = i;
                Task t = Task.Factory.StartNew(() =>
                    {
                        DataTarget dataTarget = (DataTarget) i1;
                        long value = GetCurrentValue(dataTarget).Result;

                        Console.WriteLine("{0} : {1}", dataTarget, value);
                    });

                tasks.Add(t);
            }

            await Task.WhenAll(tasks);
        }



        static async Task<long> GetCurrentValue(DataTarget dataTarget)
        {
            var bag = new ConcurrentBag<long>();
            Task parent = Task.Factory.StartNew(() =>
                {
                    foreach (string url in GetUrls(dataTarget))
                    {
                        CreateChildWorker(url, bag);
                    }
                });

            await parent;

            return bag.Max(i => i);
            //var tasks = new List<Task<WebResponse>>();

            //foreach (string url in GetUrls(dataTarget))
            //{
            //   WebRequest req = WebRequest.Create(url);

            //    tasks.Add(req.GetResponseAsync());
            //}

            //WebResponse[] resps = await Task.WhenAll(tasks);

            //return resps.Max(r => r.ContentLength);
        }

        private static void CreateChildWorker(string url, ConcurrentBag<long> bag)
        {
            WebRequest req = WebRequest.Create(url);

            Task.Factory.StartNew(() =>
                {
                    WebResponse resp = req.GetResponse();

                    bag.Add(resp.ContentLength);
                }, TaskCreationOptions.AttachedToParent);
        }


    }

    enum DataTarget
    {
        DowJones,
        FTSE,
        NASDAQ,
        HangSeng
    }
}
