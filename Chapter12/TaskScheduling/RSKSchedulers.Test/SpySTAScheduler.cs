using System.Collections.Generic;
using System.Threading.Tasks;

namespace RSKSchedulers.Test
{
    class SpySTAScheduler : STATaskScheduler
    {
        public SpySTAScheduler(int maxThreads, IIdleTrigger idleTrigger) : base(maxThreads, idleTrigger)
        {
        }

        public IEnumerable<Task> GetTasks()
        {
            return GetScheduledTasks();
        }
    }
}