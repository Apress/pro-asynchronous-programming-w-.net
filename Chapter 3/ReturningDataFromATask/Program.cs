using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ReturningDataFromATask
{
    class Program
    {
        static void Main(string[] args)
        {
           
            TimeIt(SequentialChancesToWin);
            TimeIt(TaskBasedChancesToWin);


        }
        private static void TimeIt(Action action)
        {
            Stopwatch timer = Stopwatch.StartNew();
            action();
            Console.WriteLine(timer.Elapsed);
        }
        private static void TaskBasedChancesToWin()
        {
            BigInteger n = 49000;
            BigInteger r = 600;

            Task<BigInteger> part1 = Task<BigInteger>.Factory.StartNew(() => Factorial(n));
            Task<BigInteger> part2 = Task<BigInteger>.Factory.StartNew(() => Factorial(n - r));
            Task<BigInteger> part3 = Task<BigInteger>.Factory.StartNew(() => Factorial(r));
            
            BigInteger chances = part1.Result/(part2.Result*part3.Result);


           

            
            Console.WriteLine(chances);
        }

        private static void SequentialChancesToWin()
        {
            BigInteger n = 49000;
            BigInteger r = 600;

            BigInteger part1 = Factorial(n);
            BigInteger part2 = Factorial(n - r);
            BigInteger part3 = Factorial(r);

            BigInteger chances = part1/(part2*part3);

            Console.WriteLine(chances);
        }


        static BigInteger Factorial(BigInteger factor)
        {
            BigInteger factorial = 1;

            for (BigInteger i = 1; i <= factor; i++){
                factorial *= i;
            }
            return factorial;
        }
    }
}
