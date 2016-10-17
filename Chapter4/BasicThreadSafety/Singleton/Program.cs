using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Singleton
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("started main");
            var evt = new ManualResetEventSlim();

            for (int i = 0; i < 10; i++)
            {
                Task.Factory.StartNew(() =>
                    {
                        evt.Wait();

                        Highlander h = Highlander.GetInstance();

                        Console.WriteLine(h.Report());
                    });
            }

            Thread.Sleep(1000);

            evt.Set();

            Console.ReadLine();


        }
    }
}
