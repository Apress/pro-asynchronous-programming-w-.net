using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

namespace RSKSchedulers.Test
{
    [TestClass]
    public class STATaskSchedulerTests
    {
        [TestMethod]
        public void QueueTask_WhenQueuesFirstTask_ShouldExecuteTaskOnSTAThread()
        {
            using (var scheduler = new STATaskScheduler(1, new StubIdleTrigger()))
            {
                ApartmentState apartment = ApartmentState.MTA;
                var evt = new ManualResetEventSlim();

                Task t = new Task(() =>
                    {
                        apartment = Thread.CurrentThread.GetApartmentState();
                        evt.Set();
                    });

                t.Start(scheduler);

                if (evt.Wait(1000))
                {
                    Assert.AreEqual(ApartmentState.STA, apartment);
                }
                else
                {
                    Assert.Fail();
                }
            }
        }

        [TestMethod]
        public void QueueTask_WhenQueuesTaskAndAllThreadsBusy_ShouldWaitUntilThreadFree()
        {
            using (var scheduler = new STATaskScheduler(1, new StubIdleTrigger()))
            {
                int maxConcurrency = 0;
                int currentConcurrency = 0;
                var evt = new ManualResetEventSlim();
                var countdownDone = new CountdownEvent(2);
                var incrementDoneEvt = new ManualResetEventSlim();

                Task t = new Task(() =>
                    {
                        maxConcurrency = Interlocked.Increment(ref currentConcurrency);
                        incrementDoneEvt.Set();
                        evt.Wait();
                        Interlocked.Decrement(ref currentConcurrency);
                        countdownDone.Signal();
                    });
                Task t2 = new Task(() =>
                {
                    maxConcurrency = Interlocked.Increment(ref currentConcurrency);
                    incrementDoneEvt.Set();
                    evt.Wait();
                    Interlocked.Decrement(ref currentConcurrency);
                    countdownDone.Signal();
                });

                t.Start(scheduler);
                t2.Start(scheduler);

                incrementDoneEvt.Wait();
                evt.Set();

                if (countdownDone.Wait(1000))
                {
                    Assert.AreEqual(1, maxConcurrency);
                }
                else
                {
                    Assert.Fail();
                }
            }
        }

        [TestMethod]
        public void QueueTask_WhenQueuesTaskAndThreadAvailable_ShouldRunTasks()
        {
            using (var scheduler = new STATaskScheduler(2, new StubIdleTrigger()))
            {
                int maxConcurrency = 0;
                int currentConcurrency = 0;
                var evt = new ManualResetEventSlim();
                var countdownDone = new CountdownEvent(2);
                var incrementDoneEvt = new CountdownEvent(2);

                Task t = new Task(() =>
                {
                    maxConcurrency = Interlocked.Increment(ref currentConcurrency);
                    incrementDoneEvt.Signal();
                    evt.Wait();
                    Interlocked.Decrement(ref currentConcurrency);
                    countdownDone.Signal();
                });
                Task t2 = new Task(() =>
                {
                    maxConcurrency = Interlocked.Increment(ref currentConcurrency);
                    incrementDoneEvt.Signal();
                    evt.Wait();
                    Interlocked.Decrement(ref currentConcurrency);
                    countdownDone.Signal();
                });

                t.Start(scheduler);
                t2.Start(scheduler);

                if(incrementDoneEvt.Wait(1000))
                {
                    evt.Set();
                }
                else
                {
                    Assert.Fail();
                }

                if (countdownDone.Wait(1000))
                {
                    Assert.AreEqual(2, maxConcurrency);
                }
                else
                {
                    Assert.Fail();
                }
            }
        }

        [TestMethod]
        public void Scheduler_WhenThreadIdleForTimeout_ShouldRemoveThreadFromPool()
        {
            var idleTrigger = new StubIdleTrigger(0);
            int numThreadsBeforeIdle = -1;
            int numThreadsAfterIdle = -1;
            using (var scheduler = new STATaskScheduler(1, idleTrigger))
            {
                Task t = new Task(() =>
                {
                });

                t.Start(scheduler);

                t.Wait();

                numThreadsBeforeIdle = scheduler.GetThreadCount();

                idleTrigger.FireCheckIdle();

                numThreadsAfterIdle = scheduler.GetThreadCount();
            }

            Assert.AreEqual(1, numThreadsBeforeIdle);
            Assert.AreEqual(0, numThreadsAfterIdle);
        }

        [TestMethod]
        public void Scheduler_WhenThreadNotIdleForTimeout_ShouldNotThreadFromPool()
        {
            var idleTrigger = new StubIdleTrigger(10000);
            int numThreadsBeforeIdle = -1;
            int numThreadsAfterIdle = -1;
            var evt = new ManualResetEventSlim();
            using (var scheduler = new STATaskScheduler(1, idleTrigger))
            {
                Task t = new Task(() =>
                    {
                        evt.Wait();
                    });

                t.Start(scheduler);

                numThreadsBeforeIdle = scheduler.GetThreadCount();

                idleTrigger.FireCheckIdle();

                numThreadsAfterIdle = scheduler.GetThreadCount();

                evt.Set();
            }

            Assert.AreEqual(1, numThreadsBeforeIdle);
            Assert.AreEqual(1, numThreadsAfterIdle);
        }

        [TestMethod]
        public void Scheduler_WhenThreadRunsTask_ShouldUpdateLastUsed()
        {
            const int IDLE_TIMEOUT = 10;
            var idleTrigger = new StubIdleTrigger(IDLE_TIMEOUT);
            int numThreadsAfterIdle = -1;
            var evt = new ManualResetEventSlim();
            using (var scheduler = new STATaskScheduler(1, idleTrigger))
            {
                Task t1 = new Task(() =>
                {
                    
                });

                t1.Start(scheduler);

                Thread.Sleep(2 * IDLE_TIMEOUT);

                Task t2 = new Task(() =>
                {

                });

                t2.Start(scheduler);

                t2.Wait();
              
                idleTrigger.FireCheckIdle();

                numThreadsAfterIdle = scheduler.GetThreadCount();

                evt.Set();
            }

            Assert.AreEqual(1, numThreadsAfterIdle);
        }

        [TestMethod]
        public void Ctor_WhenPassedAnIdleTrigger_ShouldSubscribeToCheckIdleEvent()
        {
            var idleTrigger = new StubIdleTrigger();
            bool subscribed = false;
            using (var scheduler = new STATaskScheduler(1, idleTrigger))
            {
                subscribed = idleTrigger.FireCheckIdle();
            }

            Assert.IsTrue(subscribed);
        }

        [TestMethod]
        public void Dispose_WhenCalled_ShouldDisposeIdleTrigger()
        {
            var idleTrigger = new StubIdleTrigger();
            using (var scheduler = new STATaskScheduler(1, idleTrigger))
            {
            }

            Assert.IsTrue(idleTrigger.Disposed);
        }

        [TestMethod]
        public void IdleTrigger_WhenFired_ShouldCheckIdleTimeoutEveryTime()
        {
            var idleTrigger = new StubIdleTrigger();
            int checkCount = 0;
            using (var scheduler = new STATaskScheduler(1, idleTrigger))
            {
                idleTrigger.FireCheckIdle();
                idleTrigger.FireCheckIdle();
                checkCount = idleTrigger.IdleTimeoutCheckCount;
            }

            Assert.AreEqual(2, checkCount);
        }

      
        [TestMethod]
        public void GetScheduledTasks_WhenOneRunningAndTwoQueued_ShouldOnlyReturnQueued()
        {
            using (var scheduler = new SpySTAScheduler(1, new StubIdleTrigger()))
            {
                var evt = new ManualResetEventSlim();
                var evtRunning = new ManualResetEventSlim();

                Task t = new Task(() =>
                {
                    evtRunning.Set();  
                    evt.Wait();
                });
                Task t2 = new Task(() =>
                {
                });
                Task t3 = new Task(() =>
                {
                });

                t.Start(scheduler);
                evtRunning.Wait();
                t2.Start(scheduler);
                t3.Start(scheduler);

                int queuedCount = scheduler.GetTasks().Count();

                evt.Set();

                Assert.AreEqual(2, queuedCount);
            }
        }

        [TestMethod]
        public void TryDequeue_WhenTaskCancelled_ShouldPreventTaskFromRunning()
        {
            var evt = new ManualResetEventSlim();
            bool t2Executed = false;
            int numExceptions = 0;
            bool cancelledExceptionFound = false;
            using (var scheduler = new STATaskScheduler(1))
            {
                var t1 = new Task(() =>
                    {
                        evt.Wait();
                    });

                var cts = new CancellationTokenSource();

                var t2 = new Task(() =>
                    {
                        t2Executed = true;
                    }, cts.Token);

                t1.Start(scheduler);
                t2.Start(scheduler);

                cts.Cancel();

                evt.Set();
                try
                {
                    t2.Wait();
                }
                catch (AggregateException x)
                {
                    numExceptions = x.InnerExceptions.Count;
                    if (numExceptions > 0 && x.InnerExceptions.First().GetType() == typeof(TaskCanceledException))
                    {
                        cancelledExceptionFound = true;
                    }
                }
            }

            Assert.IsFalse(t2Executed);
            Assert.AreEqual(1, numExceptions);
            Assert.IsTrue(cancelledExceptionFound);
        }

        [TestMethod]
        public void TryExecuteTaskInline_WhenTriggeredFromNonSTAThread_ShouldNotExecuteTask()
        {
            var evt = new ManualResetEventSlim();
            var notInlinedEvent = new ManualResetEventSlim();
            int callingThread = -1;
            int executionThread = -1;
            using (var scheduler = new STATaskScheduler(1))
            {
                scheduler.TaskNotInlined += (s, e) =>
                    {
                        notInlinedEvent.Set();
                    };

                var t1 = new Task(() =>
                {
                    evt.Wait();
                });

                var t2 = new Task(() =>
                {
                    executionThread = Thread.CurrentThread.ManagedThreadId;
                });

                t1.Start(scheduler);
                t2.Start(scheduler);

                var staThread = new Thread(() =>
                {
                    callingThread = Thread.CurrentThread.ManagedThreadId;
                    t2.Wait();
                });
                staThread.SetApartmentState(ApartmentState.MTA);
                staThread.IsBackground = true;
                staThread.Start();

                notInlinedEvent.Wait();
               
                evt.Set();

                t2.Wait();
            }

            Assert.AreNotEqual(callingThread, executionThread);
        }

        [TestMethod]
        public void TryExecuteTaskInline_WhenTriggeredFromSTAThread_ShouldExecuteTask()
        {
            var evt = new ManualResetEventSlim();
            int callingThread = -1;
            int executingThread = -2;

            using (var scheduler = new STATaskScheduler(1))
            {
                var t1 = new Task(() =>
                {
                    evt.Wait();
                });

                var t2 = new Task(() =>
                {
                    executingThread = Thread.CurrentThread.ManagedThreadId;
                });

                t1.Start(scheduler);
                t2.Start(scheduler);

                var staThread = new Thread(() =>
                    {
                        callingThread = Thread.CurrentThread.ManagedThreadId;
                        t2.Wait();
                    });
                staThread.SetApartmentState(ApartmentState.STA);
                staThread.Start();

                staThread.Join();

                evt.Set();
            }

            Assert.AreEqual(callingThread, executingThread);
        }

        [TestMethod]
        public void GetScheduledTasks_WhenCalled_ShouldReturnAllTasksNotYetExecuting()
        {
            IEnumerable<Task> tasks;
            Task t2;
            Task t3;

            using (var scheduler = new SpySTAScheduler(1, new StubIdleTrigger()))
            {
                var evt = new ManualResetEventSlim();

                Task t1 = new Task(() =>
                {
                    evt.Set();
                });

                t1.Start(scheduler);

                t2 = new Task(() =>
                {
                });

                t2.Start(scheduler);
                t3 = new Task(() =>
                {
                });

                t3.Start(scheduler);

                tasks = scheduler.GetTasks();

                evt.Set();
            }

            Assert.IsNotNull(tasks.SingleOrDefault(t => t == t2));
            Assert.IsNotNull(tasks.SingleOrDefault(t => t == t3));
        }
    }
}
