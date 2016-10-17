using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WCFClient.PubsProxies;

namespace WCFClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var proxy = new GetPubsClient();


            FullDetails details = proxy.GetAuthorsAndTitles();

            Console.WriteLine("Authors");
            foreach (Author author in details.Authors)
            {
                Console.WriteLine("\t{0} {1}", author.FirstName, author.LastName);
            }
            Console.WriteLine();
            Console.WriteLine("Titles");
            foreach (Title  title in details.Titles)
            {
                Console.WriteLine("\t{0} {1:C}", title.Name, title.Price);
            }
            //foreach (Author author in proxy.GetAuthors())
            //{
            //    Console.WriteLine("{0} {1}", author.FirstName, author.LastName);
            //}
        }
    }
}
