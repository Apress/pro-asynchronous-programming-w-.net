using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MutexOwner
{
    class Program
    {
        static void Main(string[] args)
        {
            Mutex mutant = new Mutex(false, "foobar");

            try
            {
                mutant.WaitOne();
            }
            catch (AbandonedMutexException x)
            {

                Console.WriteLine(x);
                Console.WriteLine();
            }
            finally
            {
                mutant.ReleaseMutex();
            }
        }
    }
}
