using System.Threading.Tasks.Dataflow;

namespace Joining
{
    public class Resteraunt
    {
        BufferBlock<Fork> forks = new BufferBlock<Fork>();
        BufferBlock<Knife> knives = new BufferBlock<Knife>();
        BufferBlock<Food> food = new BufferBlock<Food>();
        public Resteraunt(int numberOfForkAndKnifePairs)
        {
            Customers = new BufferBlock<Customer>();
            ReadyToGo = new JoinBlock<Fork, Knife, Food>( new GroupingDataflowBlockOptions(){Greedy = false});

            Forks.LinkTo(ReadyToGo.Target1);
            Knife.LinkTo(ReadyToGo.Target2);
            Food.LinkTo(ReadyToGo.Target3);

            for (int i = 0; i < numberOfForkAndKnifePairs; i++)
            {
                forks.Post(new Fork());
                knives.Post(new Knife());
            }
        }

        public BufferBlock<Fork> Forks { get { return forks; } }
        public BufferBlock<Knife> Knife { get { return knives; } }
        public BufferBlock<Food>  Food { get { return food; }}
        public BufferBlock<Customer> Customers { get; private set; } 

        public JoinBlock<Fork,Knife,Food> ReadyToGo { get; private set; }
    }
}