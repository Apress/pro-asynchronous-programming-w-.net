using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConcurrencyVisualizer
{
    class Program
    {
        static void Main(string[] args)
        {
            var rnd = new Random();

            for (int i = 0; i < 10; i++)
            {
                Task.Factory.StartNew(() =>
                    {
                        lock (rnd)
                        {
                            Thread.Sleep(rnd.Next(2000));
                        }
                    });
            }

            Console.WriteLine("Press enter to exit");
            Console.ReadLine();
        }
    }
}
