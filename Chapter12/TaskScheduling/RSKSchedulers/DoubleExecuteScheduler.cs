using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RSKSchedulers
{
    public class DoubleExecuteScheduler : TaskScheduler
    {
        
        protected override IEnumerable<Task> GetScheduledTasks()
        {
            yield break;
        }

        protected override void QueueTask(Task task)
        {
            Thread.Sleep(1000);
            bool ret = TryExecuteTask(task);
            Console.WriteLine("first execute result: {0}", ret);
            ret = TryExecuteTask(task);
            Console.WriteLine("second execute result: {0}", ret);
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return false;
        }
    }
}
