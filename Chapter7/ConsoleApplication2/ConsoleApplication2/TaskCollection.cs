using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomWhenAny
{
    public class TaskCollection<T>
    {
       
        private object _lock = new object();
        private List<Task> pendingTasks;

        public TaskCollection(params Task<T>[] tasks)
            : this((IEnumerable<Task<T>>)tasks)
        {
        }

        public TaskCollection(IEnumerable<Task<T>> tasks)
        {
               pendingTasks = new List<Task>(tasks);

                foreach (Task<T> task in pendingTasks)
                {
                    task.ContinueWith(MarkTaskAsCompleted);
                }
        }

        public bool IsMoreTasksPending
        {
            get
            {
                lock (_lock)
                {
                    return pendingTasks.Count > 0 || results.Count > 0;
                }
            }
        }

        public Task<T> WhenAny()
        {
            lock (_lock)
            {
                TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
                if (results.Count == 0)
                {
                    completionSources.Enqueue(tcs);
                }
                else
                {
                    Task<T> resultingTask = results.Dequeue();
                    SetResult(tcs,resultingTask);
                   
                }

                return tcs.Task;
            }
        }

        private Queue<TaskCompletionSource<T>> completionSources = new Queue<TaskCompletionSource<T>>();
        private Queue<Task<T>>  results = new Queue<Task<T>>();

        private void MarkTaskAsCompleted(Task<T> task)
        {
            lock (_lock)
            {
                pendingTasks.Remove(task);
                if (completionSources.Count > 0)
                {
                    SetResult(completionSources.Dequeue(), task);
                }
                else
                {
                    results.Enqueue(task);
                }
            }
        }

        private void SetResult(TaskCompletionSource<T> tcs , Task<T> task)
        {
            if (task.IsFaulted)
            {
                tcs.SetException(task.Exception);
                return;
            }

            if (task.IsCanceled)
            {
                tcs.SetCanceled();
                return;
            }

            tcs.SetResult(task.Result);
            
        }
    }
}
