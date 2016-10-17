using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TCS
{
    class Program
    {
        static void Main(string[] args)
        {
            //    SimpleTCS();

          //  ChildTCS();

          //  ForegroundTask();

           
           
        }

        public static IEnumerable<Task<T>> WhenAny<T>(
            IEnumerable<Task<T>> tasks)
        {
            int nextTaskToComplete = -1;
           
            Task<T>[] tasksArray = tasks.ToArray();
            var taskCompletionSources = 
                new TaskCompletionSource<T>[tasksArray.Length];

           
            for(int nTask = 0 ; nTask < tasksArray.Length;nTask++)
            {
                taskCompletionSources[nTask]=new TaskCompletionSource<T>();
                tasksArray[nTask].ContinueWith(t =>
                    {
                        int nTaskCompleted = Interlocked.Increment(ref nextTaskToComplete);

                        if (t.Exception != null)
                        {
                                
                        }

                      
                        taskCompletionSources[nextTaskToComplete]
                            .SetResult(t.Result);
                    });
            }

            return taskCompletionSources.Select(t => t.Task);
        }

        private static Task CreateSTATask(Action action)
        {
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            Thread staThread =
                new Thread(() =>
                {
                    try
                    {
                        action();
                        tcs.SetResult(null);
                    }
                    catch (Exception error)
                    {
                        tcs.SetException(error);
                    }
                });

            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();

            return tcs.Task;
        }

        private static Task CreateForegroundTask(Action action)
        {
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            Thread foregroundThread = 
                new Thread(() =>
                    {
                        try
                        {
                            action();
                            tcs.SetResult(null);
                        }
                        catch (Exception error)
                        {
                            tcs.SetException(error);
                        }
                    });
        
            foregroundThread.Start();

            return tcs.Task;
        }

        private static void ChildTCS()
        {
            Task t = Task.Factory.StartNew(() =>
                {
                    var tcs = new TaskCompletionSource<object>(TaskCreationOptions.AttachedToParent);

                    new Timer(_ => tcs.SetResult(42),
                              null,
                              TimeSpan.FromSeconds(2),
                              TimeSpan.FromSeconds(0));

                    Console.WriteLine("Parent done");
                });

            t.Wait();
            Console.WriteLine("All done");
        }

        private static void SimpleTCS()
        {
            var tcs = new TaskCompletionSource<int>();

            Task<int> syntheticTask = tcs.Task;

            syntheticTask
                .ContinueWith(t => Console.WriteLine("Result {0}", t.Result));

            Console.ReadLine();
            tcs.SetResult(42);
            Console.ReadLine();

        }
    }
}
