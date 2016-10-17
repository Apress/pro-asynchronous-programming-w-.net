using System;
using System.Threading;

namespace LazyCreation
{
    public class Person
    {
        public Person(string name)
        {
            Thread.Sleep(2000);
            Console.WriteLine("Created");
            Name = name;
        }
        public string Name { get; set; }
        public int Age { get; set; }

        public override string ToString()
        {
            return string.Format("Name: {0}, Age: {1}", Name, Age);
        }
    }
}