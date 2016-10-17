using System;
using System.Threading;
namespace SpinLocks
{
    public struct SpinLock
    {
        private int locked;

        public void Lock()
        {
            while (Interlocked.Exchange(ref locked, 1) != 0) ;
        }

        public void Unlock()
        {
            locked = 0;
        }
   

  
    }
}
