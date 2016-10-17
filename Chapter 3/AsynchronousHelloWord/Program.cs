using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AsynchronousHelloWorld
{
    class Program
    {
        static void Main(string[] args)
        {

            // Listing 3 - 1
            //Task t = new Task(Speak);
            //t.Start();
         
            // Listing 3 - 2
            //Task t = new Task(Speak);
            // t.Start();
            // Console.WriteLine("Waiting for completion");
            // t.Wait();
            //Console.WriteLine("All Done");
            
            // Listing 3 - 3
            //Task.Factory
            //  .StartNew(WhatTypeOfThreadAmI)
            //  .Wait();

            // Listing 3 - 4
            //Task.Factory
            //    .StartNew(WhatTypeOfThreadAmI, TaskCreationOptions.LongRunning)
            //    .Wait();

        }

        private static void Speak()
        {
            Console.WriteLine("Hello World");
        }

        private static void WhatTypeOfThreadAmI()
        {
            Console.WriteLine("I'm a {0} thread" , 
                Thread.CurrentThread.IsThreadPoolThread ? "Thread Pool" : "Custom");
        }
    }
}
