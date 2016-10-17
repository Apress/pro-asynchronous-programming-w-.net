using System;
using System.Threading.Tasks.Dataflow;

namespace Joining
{
    public class Waiter
    {
        private readonly Resteraunt resteraunt;
        private JoinBlock<Tuple<Fork, Knife, Food>, Customer> joinBlock;
        private ActionBlock<Tuple<Tuple<Fork, Knife, Food>, Customer>> serveFoodBlock; 
        public Waiter(Resteraunt resteraunt )
        {
            this.resteraunt = resteraunt;
            joinBlock = new JoinBlock<Tuple<Fork, Knife, Food>, Customer>(new GroupingDataflowBlockOptions(){Greedy = false});

            resteraunt.ReadyToGo.LinkTo(joinBlock.Target1);
            resteraunt.Customers.LinkTo(joinBlock.Target2);

            serveFoodBlock = new ActionBlock<Tuple<Tuple<Fork, Knife, Food>, Customer>>(
                new Action<Tuple<Tuple<Fork,Knife,Food>,Customer>>(ServeFood));

            joinBlock.LinkTo(serveFoodBlock);
        }

        private void ServeFood(Tuple<Tuple<Fork, Knife, Food>, Customer> foodServiceTuple)
        {
            Fork fork = foodServiceTuple.Item1.Item1;
            Knife knife = foodServiceTuple.Item1.Item2;
            Food food = foodServiceTuple.Item1.Item3;
            Customer customer = foodServiceTuple.Item2;

            customer.EatAsync(fork,knife,food)
                .ContinueWith(eatingTask =>
                {
                    resteraunt.Forks.Post(fork);
                    resteraunt.Knife.Post(knife);
                });
        }
    }
}