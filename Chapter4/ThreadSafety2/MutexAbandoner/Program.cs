using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MutexAbandoner
{
    class Program
    {
        static void Main(string[] args)
        {
            Mutex mtx = new Mutex(true, "foobar");

            Console.WriteLine("press enter to abandon");
            Console.ReadLine();
        }
    }
}
