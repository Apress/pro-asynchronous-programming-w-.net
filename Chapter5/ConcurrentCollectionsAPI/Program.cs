using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConcurrentCollectionsAPI
{
    class Program
    {
        static void Main(string[] args)
        {
//            NonThreadSafeQueue();
            ConcurrentQueue<int> queue = new ConcurrentQueue<int>();

            int val;

            if (queue.TryDequeue(out val))
            {
                Console.WriteLine(val);
            }
        }

        private static void NonThreadSafeQueue()
        {
            Queue<int> queue = new Queue<int>();


            if (queue.Count > 0)
            {
                int val = queue.Dequeue();
            }
        }
    }
}
