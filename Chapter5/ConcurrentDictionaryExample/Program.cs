using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConcurrentDictionaryExample
{
    class Program
    {
        static void Main(string[] args)
        {
          

            string csvFilesFolder = @"..\..\..\DoubleCheckLocking\WeatherData";

            ICSVRepository csvRepository = 
                //new NonLazyCsvRepository(csvFilesFolder);
                new ConcurrentDitionaryLazyCsvRepository(csvFilesFolder);            

            Task[] workers = new Task[4];

            Stopwatch timer = Stopwatch.StartNew();
            for (int nWorker = 0; nWorker < workers.Length; nWorker++)
            {
                workers[nWorker] = Task.Run(() => ProcessRandomFiles(csvRepository, 500000));
            }

            Task.WaitAll(workers);
            Console.WriteLine(timer.Elapsed);
            if (!csvRepository.VerifyEachFileOnlyLoadedOnce())
            {
                Console.WriteLine("Loaded multiple times");
            }
        }

        private static void ProcessRandomFiles(ICSVRepository repository, int iterations)
        {
            string[] files = repository.Files.ToArray();
            Random rnd = new Random();
            for (int i = 0; i < iterations; i++)
            {
                repository.Map<WeatherEntry>(files[rnd.Next(files.Length)], r => new WeatherEntry(r));
            }
        }
    }
}
