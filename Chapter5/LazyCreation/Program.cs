using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LazyCreation
{
    class Program
    {
        static void Main(string[] args)
        {
            //  SimpleLazy();

         //   SharingLazyAcrossThreads();

       //     var lazyPerson = new Lazy<Person>(() => new Person("Andy"));

            var csvRepository = new CsvRepository(@"..\..\..\DoubleCheckLocking\WeatherData");

            Parallel.ForEach(csvRepository.Files, file =>
            {
                Console.WriteLine(file);
                foreach (WeatherEntry entry in csvRepository.Map<WeatherEntry>(file, r => new WeatherEntry(r)))
                {
                    Console.WriteLine(entry);
                }
            });

        }

        private static void SharingLazyAcrossThreads()
        {
            Lazy<Person> lazyPerson = new Lazy<Person>(LazyThreadSafetyMode.PublicationOnly);

            Task<Person> p1 = Task.Run<Person>(() => lazyPerson.Value);
            Task<Person> p2 = Task.Run<Person>(() => lazyPerson.Value);

            Console.WriteLine(object.ReferenceEquals(p1.Result, p2.Result));
        }

        private static void SimpleLazy()
        {
            Lazy<Person> lazyPerson = new Lazy<Person>();


            Console.WriteLine("Lazy object created");

            Console.WriteLine("has person been created {0}", lazyPerson.IsValueCreated ? "Yes" : "No");


            Console.WriteLine("Setting Name");
            lazyPerson.Value.Name = "Andy";
            Console.WriteLine("Setting Age");
            lazyPerson.Value.Age = 21;

            Person andy = lazyPerson.Value;
            Console.WriteLine(andy);
        }
    }
}
