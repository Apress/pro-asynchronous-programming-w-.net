using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsyncCombinators.Tests
{
    [TestClass]
    public class WhenAllOrFailTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void WhenAllOrFail_WhenCalledWithNoTasks_ShouldThrowArgumentException()
        {
            TaskCombinators.WhenAllOrFail<int>(Enumerable.Empty<Task<int>>());
        }

        [TestMethod]
        public void WhenAllOrFail_WhenCalledWithTwoTasks_ShouldReturnATaskThatIsCompletedWhenBothTasksHaveCompleted()
        {
         Task<int[]> allDone = TaskCombinators
                .WhenAllOrFail<int>(Enumerable.Range(1, 2).Select(Task.FromResult));

            allDone.Wait();

            Assert.AreEqual(1,allDone.Result[0]);
            Assert.AreEqual(2,allDone.Result[1]);
        }

        [TestMethod]
        public void WhenAllOrFail_WhenCalledWithTwoTasksFirstOneFailsSecondNeverCompletes_ShouldThrowAggregateExceptionContainingTheFailedTaskException()
        {
            var neverCompletes = new TaskCompletionSource<int>();
            var fails = new TaskCompletionSource<int>();
            
            
            Task<int[]> allDone = TaskCombinators
                .WhenAllOrFail<int>(new Task<int>[] { neverCompletes.Task,fails.Task});
            
            fails.SetException(new InvalidOperationException("Boom"));

            try
            {
                allDone.Wait();
                Assert.Fail();
            }
            catch (AggregateException errors)
            {
                Assert.AreEqual(typeof(InvalidOperationException),
                    errors.Flatten().InnerExceptions.First().GetType());
            }

        }

        [TestMethod]
        public void WhenAllOrFail_WhenCalledWithTwoTasksFirstOneSucceedsSecondIsCancelled_ShouldThrowAggregateExceptionContainingTaskCancelledExceptionAndHaveAStatusOfCancelled()
        {
     
            var cancelled = new TaskCompletionSource<int>();


            Task<int[]> allDone = TaskCombinators
                .WhenAllOrFail<int>(new Task<int>[] { Task.FromResult(1), cancelled.Task });

            cancelled.SetCanceled();

            try
            {
                allDone.Wait(3000);
                Assert.Fail();
            }
            catch (AggregateException errors)
            {
                Assert.AreEqual(typeof(TaskCanceledException),
                    errors.Flatten().InnerExceptions.First().GetType());
            }

        }
    }
}
