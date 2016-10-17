using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BlockingCollections
{
    public static class Utils
    {
        public static T[] Assign<T>(this T[] items, Func<int, T> assign)
        {
            for (int i = 0; i < items.Length; i++)
            {
                items[i] = assign(i);
            }
            return items;
        }

        public static T[] ElementArrayOf<T>(this int size, Func<int, T> assign)
        {
            return Assign(new T[size], assign);
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:9000/Time/");

            listener.Start();
          // ConcurrentQueueProducerConsumer(listener);

             BlockingCollectionProducerConsumer(listener);

           //BlockingCollectionProducerForeachConsumer(listener);

          


        }

        private static void BlockingCollectionProducerConsumer(HttpListener listener)
        {
            var requestQueue = new BlockingCollection<HttpListenerContext>(new ConcurrentQueue<HttpListenerContext>(),2);

            var produer = Task.Run(() => Producer(requestQueue, listener));

            Task[] consumers = new Task[4];
            for (int nConsumer = 0; nConsumer < consumers.Length; nConsumer++)
            {
                consumers[nConsumer] = Task.Run(() => Consumer(requestQueue));
            }


            Console.WriteLine("Listening...");

            Console.ReadLine();
            listener.Stop();

            produer.Wait();
            Task.WaitAll(consumers);
        }

        private static void BlockingCollectionProducerForeachConsumer(HttpListener listener)
         {
            var requestQueue = new BlockingCollection<HttpListenerContext>(new ConcurrentQueue<HttpListenerContext>());

            ;
            var produer = Task.Run(() => Producer(requestQueue, listener));

            Task[] consumers = new Task[4];
            for (int nConsumer = 0; nConsumer < consumers.Length; nConsumer++)
            {
                consumers[nConsumer] = Task.Run(() => ForeachConsumer(requestQueue));
            }


            Console.WriteLine("Listening...");


            Console.ReadLine();
            listener.Stop();

            produer.Wait();
            Task.WaitAll(consumers);
        }

        private static void ConcurrentQueueProducerConsumer(HttpListener listener)
        {
            var requestQueue = new ConcurrentQueue<HttpListenerContext>();
            var produer = Task.Run(() => Producer(requestQueue, listener));

            Task[] consumers = new Task[1];
            for (int nConsumer = 0; nConsumer < consumers.Length; nConsumer++)
            {
                consumers[nConsumer] = Task.Run(() => Consumer(requestQueue));
            }


            Console.WriteLine("Listening...");

            produer.Wait();
            Task.WaitAll(consumers);
        }

        public static void Producer(ConcurrentQueue<HttpListenerContext> queue, HttpListener listener)
        {
                while (true)
                {
                    queue.Enqueue(listener.GetContext());
                }    
        }

        public static  void Consumer(ConcurrentQueue<HttpListenerContext> queue)
        {
            while (true)
            {
                HttpListenerContext ctx;
                if (queue.TryDequeue(out ctx))
                {
                    Console.WriteLine(ctx.Request.Url);
                    using (StreamWriter writer = new StreamWriter(ctx.Response.OutputStream))
                    {
                        writer.WriteLine(DateTime.Now);                   
                    }
                }
            }
        }

        public static void Producer(BlockingCollection<HttpListenerContext> queue, HttpListener listener)
        {
            while (true)
            {
                HttpListenerContext ctx = listener.GetContext();
                if (ctx.Request.QueryString.AllKeys.Contains("stop")) break;

                if (!queue.TryAdd(ctx))
                {
                    ctx.Response.StatusCode = (int) HttpStatusCode.ServiceUnavailable;
                    ctx.Response.Close();
                }
            }
            queue.CompleteAdding();
            Console.WriteLine("Producer stopped");
        }

        public static void Consumer(BlockingCollection<HttpListenerContext> queue)
        {
            try
            {
                while (true)
                {
                    HttpListenerContext ctx = queue.Take();
                    Console.WriteLine(ctx.Request.Url);
                    Thread.Sleep(5000);
                    using (var writer = new StreamWriter(ctx.Response.OutputStream))
                    {
                        writer.WriteLine(DateTime.Now);
                    }
                }
            }
            catch (InvalidOperationException error)
            {
               
            }
          
            Console.WriteLine("{0}:Stopped",Task.CurrentId);
        }

        public static void ForeachConsumer(BlockingCollection<HttpListenerContext> queue)
        {

            foreach (HttpListenerContext ctx in queue.GetConsumingEnumerable())
            {
                Console.WriteLine(ctx.Request.Url);
                using (var writer = new StreamWriter(ctx.Response.OutputStream))
                {
                    writer.WriteLine(DateTime.Now);
                }

            }

            Console.WriteLine("{0}:Stopped", Task.CurrentId);
        }
    }
}
