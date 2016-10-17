using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncAwait
{
    class Program
    {
        static void Main(string[] args)
        {
            DoWork();

            Console.ReadLine();
        }

        static async Task RunWorkAsync()
        {
            Console.WriteLine("Starting work");

            await Task.Delay(2000);

            Console.WriteLine("background work complete");
        }

        static async Task DoWork()
        {
            await RunWorkAsync();

            Console.WriteLine("DoWork");
        }
    }
}
