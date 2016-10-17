using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Breakpoints
{
    class Program
    {
        static object o = new object();
        static void Main(string[] args)
        {
            Task.Factory.StartNew(PrintCharacters, "*");
            Task.Factory.StartNew(PrintCharacters, ".");

            lock (o)
            {
                Console.ReadLine();
            }
        }

        static void PrintCharacters(object obj)
        {
            string outputChar = (string)obj;
            while (true)
            {
                lock (o)
                {
                    Console.Write(outputChar);
                }
            }
        }
    }
}
