using System;
using System.ServiceModel;
using WCF40;

namespace WCF45
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = new ServiceHost(typeof (Service));

            host.Open();

            Console.WriteLine("Service Ready ...");
            Console.ReadLine();

            host.Close();
        }
    }


}
