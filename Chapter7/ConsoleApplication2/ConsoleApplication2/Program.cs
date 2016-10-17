using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace CustomWhenAny
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //TaskDelayExmaples();

            //WhenAny();

           // BenchmarkWhenAnyImplementations();


            GetFirstGoodResult();

            
        }

        private static void GetFirstGoodResult()
        {
            string page = GetGooglePage("maps").Result;
            Console.WriteLine("Done");


            CountWordsOnPageAsync("http://www.google.co.uk")
                .ContinueWith(wct => Console.WriteLine(wct.Result));
        }

        private static void BenchmarkWhenAnyImplementations()
        {
            const int nTasks = 2000;
            while (true)
            {
                TimeIt(CostOfWaitAnyEx, nTasks);
                TimeIt(CostOfWaitAnyCollection, nTasks);
                TimeIt(CostOfWaitAny, nTasks);
                TimeIt(CostOfWhenAll, nTasks);
                Console.WriteLine();
            }
        }

        private static void WhenAny()
        {
            DownloadDocumentsWhenAny(
                new Uri("http://www.bbc.co.uk"),
                new Uri("http://www.microsoft.com"),
                new Uri("http://www.cisco.com"))
                .Wait();
        }

        private static void TimeIt(Func<List<Task<object>>, Task> waitAnyOperation, int nTasks)
        {
            var tasks = new List<Task<object>>();
            for (int nTask = 0; nTask < nTasks; nTask++)
            {
                int localTask = nTask;
                tasks.Add(Task.Run<object>(new Func<object>(() =>
                    {
                        Thread.Sleep(100);
                        return null;
                    })));
            }

            var beforeProcessorTime =
            System.Diagnostics.Process.GetCurrentProcess().TotalProcessorTime;

            waitAnyOperation(tasks).Wait();

            var nowProcessorTime = System.Diagnostics.Process.GetCurrentProcess().TotalProcessorTime;

            Console.WriteLine("Elapsed processor time {1}:{0}" ,nowProcessorTime-beforeProcessorTime,
                waitAnyOperation.Method.Name);
        }

        private static void TaskDelayExmaples()
        {
            DoOperation()
                // Try(3, TimeSpan.FromSeconds(2), AttemptOperation)
                .Wait();
        }

        public static async Task Try(
            int nTimes,
            TimeSpan every,
            Action action)
        {
            try
            {
                action();
                return;
            }
            catch (Exception)
            {
                if (nTimes == 1) throw;
            }

            await Task.Delay(every);
            if (nTimes > 1)
            {
                await Try(nTimes - 1, every, action);
            }

        }


        private static async Task DoOperation()
        {
            for (int nTry = 0; nTry < 3; nTry++)
            {
                try
                {
                    AttemptOperation();
                    break;
                }
                catch (OperationFailedException){}
                await Task.Delay(2000);
            }
        }

        private static void AttemptOperation()
        {
            Console.WriteLine("Trying..");
            throw new OperationFailedException();
        }

        public static Regex wordRegEx = new Regex(
            "[\\w]+",
            RegexOptions.Multiline
            | RegexOptions.CultureInvariant
            | RegexOptions.Compiled
            );

        public static int CountWordsOnPage(string url)
        {
            using (var client = new WebClient())
            {
                string page = client.DownloadString(url);

                return wordRegEx.Matches(page).Count;
            }
        }

        public static async Task<int> CountWordsOnPageAsync(string url)
        {
            using (var client = new WebClient())
            {
                string page = await client.DownloadStringTaskAsync(url);

                return wordRegEx.Matches(page).Count;
            }
        }

        public static async Task DownloadDocuments(
            params Uri[] downloads)
        {
            var client = new WebClient();
            foreach (Uri uri in downloads)
            {
                string content = await client
                 .DownloadStringTaskAsync(uri);

                UpdateUI(content);
            }
        }

        public static async Task DownloadDocumentsWhenAll(
            params Uri[] downloads)
        {
            
            List<Task<string>> tasks = new List<Task<string>>();
            foreach (Uri uri in downloads)
            {
                var client = new WebClient();

                tasks.Add(client
                              .DownloadStringTaskAsync(uri));
            }

            Task allDownloads = Task.WhenAll(tasks);
            try
            {
                await allDownloads;
                //
                tasks.ForEach(t => UpdateUI(t.Result));
            }
            catch (Exception )
            {
                // With await only get the first exception from the Aggregate
                // need access to the task to get to all exceptions

                for (int nTask = 0; nTask < tasks.Count; nTask++)
                {
                    if (tasks[nTask].IsFaulted)
                    {
                        Console.WriteLine("Download {0} failed",downloads[nTask]);
                        tasks[nTask].Exception.Handle( exception =>
                            {
                                Console.WriteLine("\t{0}",exception.Message);
                                return true;
                            });
                    }
                }

               //allDownloads.Exception.Handle(exception =>
               //    {
               //        Console.WriteLine(exception.GetType());
               //        return true;
               //    });
                
               
            }
         
        }

        public static async Task<string> GetGooglePage(string relativeUrl)
        {
            string[] hosts = {
                                 "",
                                 "google.co.uk",
                                 "google.com",
                                 "www.google.com.sg"
                             };
            Console.WriteLine("Starting..");
            List<Task<string>> tasks =
                (from host in hosts
                 let urlBuilder = new UriBuilder("http", host, 80, relativeUrl)
                 let webClient = new WebClient()
                 select webClient
                    .DownloadStringTaskAsync(urlBuilder.Uri)
                    .ContinueWith<string>(dt => 
                                { 
                                  webClient.Dispose();
                                  return dt.Result;
                                })
                ).ToList();

            Console.WriteLine("Queries gone");

            // just wait for the first to complete, could have failed
            // second one could have succeeded but we don't get it

          //  return await Task.WhenAny(tasks).Result;

            // Will wait for the first to succeed
            // if none succeed the exceptions are re-thrown
            
            var errors = new List<Exception>();
            do
            {
                Task<string> completedTask = null;
              
               completedTask= await Task.WhenAny(tasks);
               if (completedTask.Status == TaskStatus.RanToCompletion)
               {
                   return completedTask.Result;
               }
                tasks.Remove(completedTask);
               
                errors.Add(completedTask.Exception);

            } while (tasks.Count > 0 );

            throw new AggregateException(errors);
        }

        public static async Task CostOfWaitAny(List<Task<object>> tasks)
        {
            while (tasks.Count > 0)
            {
               var t = await Task.WhenAny(tasks);
                tasks.Remove(t);
            }
        }

        public static async Task CostOfWaitAnyCollection(List<Task<object>> tasks)
        {
            TaskCollection<object> pending = new TaskCollection<object>(tasks);
            while (pending.IsMoreTasksPending)
            {
                object result = await pending.WhenAny();
            }
        }

        public static async Task CostOfWaitAnyEx(List<Task<object>> tasks)
        {
            foreach (var nextTask in WaitForAnyEx(tasks))
            {
               Task<object> completd =  await nextTask;
            }
        }

        public static Task<Task<T>>[] WaitForAnyEx<T>(IEnumerable<Task<T>> tasks)
        {
            Task<T>[] toComplete = tasks.ToArray();

            var completionSources =
                new TaskCompletionSource<Task<T>>[toComplete.Length];

            int nextCompletedTask = -1;
            for (int i = 0; i < completionSources.Length; i++)
            {
                completionSources[i] = new TaskCompletionSource<Task<T>>();
                toComplete[i].ContinueWith(t => 
                    {
                        completionSources[Interlocked.Increment(ref nextCompletedTask)]
                            .SetResult(t);
                    });
            }


            return completionSources.Select(tcs => tcs.Task).ToArray();
        }

        public static async Task CostOfWhenAll(List<Task<object>> tasks)
        {
            await Task.WhenAll(tasks);
        }

        public static async Task DownloadDocumentsWhenAny(
           params Uri[] downloads)
        {

            List<Task<string>> tasks = new List<Task<string>>();
            foreach (Uri uri in downloads)
            {
                var client = new WebClient();

                tasks.Add(client
                              .DownloadStringTaskAsync(uri));
            }

            while (tasks.Count > 0)
            {
                Task<string> download =
                    await Task.WhenAny(tasks);

                UpdateUI(download.Result);

                int nDownloadCompleted = tasks.IndexOf(download);
                Console.WriteLine("Downloaded {0}",downloads[nDownloadCompleted]);
                tasks.RemoveAt(nDownloadCompleted);
            }


        }

        private static void UpdateUI(string result)
        {
            
        }

        private static void Process(string s)
        {
            return;
        }


    }

    public static class TaskUtils
    {
        public static void Handle<TException>(this IEnumerable<Task> tasks,
                                     Action<TException> errorHandler) where TException:Exception
        {
            var exceptions = from task in tasks
                             where task.IsFaulted
                             from error in task.Exception.InnerExceptions.OfType<TException>()
                             select error;

            foreach (TException error in exceptions)
            {
                    errorHandler(error);
            }

        }
    }

    internal class OperationFailedException : Exception
    {
    }
}
