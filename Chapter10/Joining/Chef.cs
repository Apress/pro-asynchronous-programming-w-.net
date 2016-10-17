using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Joining
{
    public class Chef
    {
        private readonly Resteraunt resteraunt;

        public Chef(Resteraunt resteraunt)
        {
            this.resteraunt = resteraunt;
        }

        public async Task MakeFoodAsync()
        {
            Console.WriteLine("Chef making food");
            Knife knife= await resteraunt.Knife.ReceiveAsync();
            for (int nFoodItem = 0; nFoodItem < 4; nFoodItem++)
            {
                this.resteraunt.Food.Post(new Food());
            }
            Console.WriteLine("Chef made food..");
            resteraunt.Knife.Post(knife);
        }

      
    }
}