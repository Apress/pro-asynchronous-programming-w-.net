using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Joining
{
    public class Customer
    {
        private readonly int id;
       
        private static Random rnd = new Random();
        private JoinBlock<Fork, Knife,Food> dinningBlock;
        private ActionBlock<Tuple<Fork, Knife,Food>> eatingBlock;

        public Customer(int id)
        {
            Console.WriteLine("New hungry customer {0}",id);
            this.id = id;
           
        }

        public async Task EatAsync(Fork fork, Knife knife, Food food)
        {
            Console.WriteLine("Yummy {0}",id);
            await Task.Delay(2000);
            Console.WriteLine("Burp {0}",id);
        }
    }
}