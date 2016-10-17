using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ForegroundTasks
{
    class Program
    {
        static void Main(string[] args)
        {
            Task fgTask =
                CreateForegroundTask<int>(() =>
                    {
                        Console.WriteLine("Running..");
                        Thread.Sleep(2000);
                        return 42;
                    })
                    .ContinueWith(t => Console.WriteLine("Result is {0}",t.Result),
                                  TaskContinuationOptions.ExecuteSynchronously);

            }
      
        public static Task<T> CreateForegroundTask<T>(Func<T> taskBody)
        {
            return CreateForegroundTask(taskBody, CancellationToken.None);
        }

        public static Task<T> CreateForegroundTask<T>(Func<T> taskBody,CancellationToken ct)
        {
            var tcs = new TaskCompletionSource<T>();

            var fgThread = new Thread(() => ExecuteForegroundTaskBody(taskBody, ct, tcs));

            fgThread.Start();

            return tcs.Task;
        }

        private static void ExecuteForegroundTaskBody<T>(Func<T> taskBody, CancellationToken ct,
                                                         TaskCompletionSource<T> tcs)
        {
            try
            {
                T result = taskBody();
                tcs.SetResult(result);
            }
            catch (OperationCanceledException cancelledException)
            {
                if (ct == cancelledException.CancellationToken)
                {
                    tcs.SetCanceled();
                }
                else
                {
                    tcs.SetException(cancelledException);
                }
            }
            catch (Exception error)
            {
                tcs.SetException(error);
            }
        }
    }
}
