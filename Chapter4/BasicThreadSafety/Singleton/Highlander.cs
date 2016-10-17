using System;
using System.Threading;

namespace Singleton
{
    public class Highlander
    {
        private Highlander()
        {
            Console.WriteLine("created");
        }

        private static Highlander theInstance;

        public static Highlander GetInstance()
        {
            if (theInstance == null)
            {
                //theInstance = new Highlander();
                Interlocked.CompareExchange(ref theInstance, new Highlander(), null);
            }

            return theInstance;
        }

        public int Report()
        {
            return GetHashCode();
        }
    }
}