using Stump.Server.WorldServer.Database.Items.Templates;

namespace Stump.Server.WorldServer.Game.Items
{
    public class ItemProbability
    {

        private int itemTemplate;
        private int amount;
        private double probability;

        public ItemProbability(int itemTemplate, int amount, double probability)
        {
            this.itemTemplate = itemTemplate;
            this.amount = amount;
            this.probability = probability;
        }

        public int getItemTemplate()
        {
            return this.itemTemplate;
        }
        public int getAmount()
        {
            return this.amount;
        }
        public double getProbability()
        {
            return this.probability;
        }
    }
}