using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncCombinators
{
    public static class TaskCombinators
    {
        public static IEnumerable<Task<T>> OrderByCompletion<T>(this IEnumerable<Task<T>> tasks)
        {
            if (tasks == null) throw new ArgumentNullException("tasks");

            List<Task<T>> allTasks = tasks.ToList();
            if ( allTasks.Count == 0 ) throw new ArgumentException("Must have at least one task");

            var taskCompletionsSources = new TaskCompletionSource<T>[allTasks.Count];

            int nextCompletedTask = -1;

            for (int nTask = 0; nTask < allTasks.Count; nTask++)
            {
               taskCompletionsSources[nTask] = new TaskCompletionSource<T>();
                allTasks[nTask].ContinueWith(t =>
                    {
                        int taskToComplete = Interlocked.Increment(ref nextCompletedTask);
                        switch (t.Status)
                        {
                            case TaskStatus.RanToCompletion:
                                taskCompletionsSources[taskToComplete].SetResult(t.Result);
                                break;

                            case TaskStatus.Faulted:
                                taskCompletionsSources[taskToComplete].SetException(t.Exception.InnerExceptions);
                                break;

                                case TaskStatus.Canceled:
                                taskCompletionsSources[taskToComplete].SetCanceled();
                                break;
                        }
                        
                    },TaskContinuationOptions.ExecuteSynchronously);
            }

            return taskCompletionsSources.Select(tcs => tcs.Task);
        } 

        public static IEnumerable<Task<T>> WhenAnyAndThrottle<T>(Func<Task<T>> nextTask, int maxOutstandingTasks)
        {
            return null;
        }



        public static Task<T[]> WhenAllOrFail<T>(IEnumerable<Task<T>> tasks)
        {
            List<Task<T>> allTasks = tasks.ToList();
            if ( allTasks.Count == 0) throw new ArgumentException("No tasks to wait on");

            var tcs = new TaskCompletionSource<T[]>();

            int tasksCompletedCount = 0;
            Action<Task<T>> completedAction = t =>
                {
                    if (t.IsFaulted)
                    {
                        tcs.TrySetException(t.Exception);
                        return;
                    }

                    if (t.IsCanceled)
                    {
                        tcs.TrySetCanceled();
                        return;
                    }

                    if (Interlocked.Increment(ref tasksCompletedCount) == allTasks.Count)
                    {
                        tcs.SetResult(allTasks.Select(ct => ct.Result).ToArray());
                    }
                };

            allTasks.ForEach(t => t.ContinueWith(completedAction));

            return tcs.Task;
        }
    }
}







