using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsyncCombinators.Tests
{
    [TestClass]
    public class OrderByCompletionTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void OrderByCompletion_WhenCalledWithNoTasks_ShouldThrowArgumentException()
        {
            TaskCombinators.OrderByCompletion(Enumerable.Empty<Task<object>>());
        }

        [TestMethod]
        public void OrderByCompletion_WhenCalledWithTwoTasks_ShouldShowTasksCompletingInOrder()
        {
            var firstTcs = new TaskCompletionSource<int>();
            var secondTcs = new TaskCompletionSource<int>();

            var tasks = new List<Task<int>>() {firstTcs.Task, secondTcs.Task};

           
            Task<int>[] tasksToWaitOn = TaskCombinators.OrderByCompletion(tasks).ToArray();

            firstTcs.SetResult(1);
            Assert.AreEqual(1,tasksToWaitOn[0].Result);
            secondTcs.SetResult(2);

            Assert.AreEqual(2,tasksToWaitOn[1].Result);

            
        }

        [TestMethod]
        public void OrderByCompletion_WhenCalledWithTwoTasks_ShouldShowTasksCompletingOutOfOrder()
        {
            var firstTcs = new TaskCompletionSource<int>();
            var secondTcs = new TaskCompletionSource<int>();

            var tasks = new List<Task<int>>() { firstTcs.Task, secondTcs.Task };


            Task<int>[] tasksToWaitOn = TaskCombinators.OrderByCompletion(tasks).ToArray();

            secondTcs.SetResult(1);
            Assert.AreEqual(1, tasksToWaitOn[0].Result);
            firstTcs.SetResult(2);

            Assert.AreEqual(2, tasksToWaitOn[1].Result);


        }

        [TestMethod]
        public void OrderByCompletion_WhenCalledWithTwoTasksFirstFailsSecondCompletes_ShouldShowFirstFailingSecondCompletingSucessfully()
        {
            var firstTcs = new TaskCompletionSource<int>();
            var secondTcs = new TaskCompletionSource<int>();

            var tasks = new List<Task<int>>() { firstTcs.Task, secondTcs.Task };


            Task<int>[] tasksToWaitOn = TaskCombinators.OrderByCompletion(tasks).ToArray();

            firstTcs.SetException(new InvalidOperationException("Boom"));
            try
            {
                tasksToWaitOn[0].Wait();
                Assert.Fail();
            }
            catch (AggregateException errors)
            {
                Assert.AreEqual(typeof(InvalidOperationException),
                    errors.InnerExceptions.First().GetType());
            }
            
            secondTcs.SetResult(2);

            Assert.AreEqual(2, tasksToWaitOn[1].Result);


        }

        [TestMethod]
        public void OrderByCompletion_WhenCalledWithTwoTasksFirstSucceedsSecondIsCancelled_ShouldShowFirstSucceedingSecondCancelled()
        {
            var firstTcs = new TaskCompletionSource<int>();
            var secondTcs = new TaskCompletionSource<int>();

            var tasks = new List<Task<int>>() { firstTcs.Task, secondTcs.Task };


            Task<int>[] tasksToWaitOn = TaskCombinators.OrderByCompletion(tasks).ToArray();

            firstTcs.SetResult(1);
            
            Assert.AreEqual(1,tasksToWaitOn[0].Result);

           secondTcs.SetCanceled();

           try
            {
                tasksToWaitOn[1].Wait();
                Assert.Fail();
            }
            catch (AggregateException errors)
            {
                Assert.AreEqual(typeof(TaskCanceledException),
                    errors.InnerExceptions.First().GetType());
            }


        }
    }
}
