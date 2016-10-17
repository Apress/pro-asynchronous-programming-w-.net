using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RSKSchedulers
{
    class TaskWrapper
    {
        private readonly Task task;
        private bool isAvailable;
        private readonly object guard = new object();

        public TaskWrapper(Task task)
        {
            this.task = task;
            isAvailable = true;
        }

        public bool TryMarkAsUnavailable()
        {
            lock (guard)
            {
                if (isAvailable)
                {
                    isAvailable = false;
                    return true;
                }
            }
            return false;
        }

        public Task Task { get { return task; } }

        public bool TryGetUnscheduledTask(out Task outputTask)
        {
            outputTask = null;
            lock (guard)
            {
                if (isAvailable)
                {
                    isAvailable = false;
                    outputTask = task;
                    return true;
                }
            }
            return false;
        }
    }
    public class STATaskScheduler : TaskScheduler, IDisposable
    {
        private const int IDLE_CHECK_FRQUENCY = 5;
        private const int IDLE_TIMEOUT = 30;

        private readonly int maxThreads;
        private readonly IIdleTrigger idleTrigger;

        private readonly ConcurrentDictionary<ThreadControl, Thread> threads = new ConcurrentDictionary<ThreadControl, Thread>();
        private readonly BlockingCollection<TaskWrapper> taskQueue = new BlockingCollection<TaskWrapper>();

        private int threadsInUse;

        public STATaskScheduler(int maxThreads)
            : this(maxThreads, 
                   new TimerIdleTrigger(TimeSpan.FromSeconds(IDLE_CHECK_FRQUENCY), 
                                        TimeSpan.FromSeconds(IDLE_TIMEOUT)))
        {
            
        }

        public STATaskScheduler(int maxThreads, IIdleTrigger idleTrigger)
        {
            this.maxThreads = maxThreads;
            this.idleTrigger = idleTrigger;

            this.idleTrigger.CheckIdle += CheckForIdleThread;
        }

        protected override bool TryDequeue(Task task)
        {
            Console.WriteLine("TryDequeue called");
            foreach (TaskWrapper taskWrapper in taskQueue)
            {
                if (taskWrapper.Task == task)
                {
                    return taskWrapper.TryMarkAsUnavailable();
                }
            }

            return false;
        }

        public override int MaximumConcurrencyLevel
        {
            get
            {
                return maxThreads;
            }
        }

        private void CheckForIdleThread(object sender, EventArgs e)
        {
            TimeSpan timeoutToCheck = idleTrigger.IdleTimeout;

            foreach (ThreadControl threadControl in threads.Keys)
            {
                bool cancelled = threadControl.CancelIfIdle(timeoutToCheck);
                if (cancelled)
                {
                    Thread thread;
                    threads.TryRemove(threadControl, out thread);
                    thread.Join(TimeSpan.FromSeconds(5));
                }
            }
        }

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            Console.WriteLine("GetScheduledTasks called");
            return taskQueue.Select(tw => tw.Task).ToList();
        }

        protected override void QueueTask(Task task)
        {
            Console.WriteLine("QueueTask called");
            taskQueue.Add(new TaskWrapper(task));

            int threadCount = threads.Count;

            if (threadCount == threadsInUse && threadCount < maxThreads)
            {
                // we are creating this thread to run a piece of work so 
                // so pretend its already in use - otherwise there is a race 
                // condition when another task is queued as this thread is 
                // seemingly free thread even though it is already committed 
                Interlocked.Increment(ref threadsInUse);
                StartNewPoolThread();
            }
        }
         
        private void StartNewPoolThread()
        {
            bool firstTaskExecuted = false;
            var thread = new Thread(o =>
                {
                    try
                    {
                        var currentThreadControl = (ThreadControl) o;
                        foreach (TaskWrapper taskWrapper in taskQueue.GetConsumingEnumerable(currentThreadControl.CancellationToken))
                        {
                            currentThreadControl.SetInUse(); 
                            // if this is the first task executed then the thread was already 
                            // marked in-use before it was created
                            if (firstTaskExecuted)
                            {
                                Interlocked.Increment(ref threadsInUse);
                            }
                            else
                            {
                                firstTaskExecuted = true;
                            }

                            Task task;
                            bool canRunTask = taskWrapper.TryGetUnscheduledTask(out task);
                            if (canRunTask)
                            {
                                TryExecuteTask(task);
                            }
                            currentThreadControl.SetNotInUse();
                            Interlocked.Decrement(ref threadsInUse);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        // if we haven't yet run a task when cancelled
                        // then need to decrement the threadsInUse count as
                        // it won't yet have been reset
                        if (!firstTaskExecuted)
                        {
                            Interlocked.Decrement(ref threadsInUse);
                        }
                    }
                }); 

            thread.SetApartmentState(ApartmentState.STA);

            var threadControl = new ThreadControl();
            threads.TryAdd(threadControl, thread);

            thread.Start(threadControl);
        }

        public event EventHandler TaskNotInlined = delegate { }; 

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
            {
                if(taskWasPreviouslyQueued)
                {
                    if (!TryDequeue(task))
                    {
                        TaskNotInlined(this, EventArgs.Empty);
                        return false;
                    }
                }
                bool executeResult = TryExecuteTask(task);
                if (!executeResult)
                {
                    TaskNotInlined(this, EventArgs.Empty);
                }
                return executeResult;
            }

            TaskNotInlined(this, EventArgs.Empty);

            return false;
        }

        public void Dispose()
        {
            idleTrigger.Dispose();
            taskQueue.CompleteAdding();

            foreach (Thread thread in threads.Values)
            {
                thread.Join();
            }
        }

        public int GetThreadCount()
        {
            return threads.Count;
        }
    }
}
