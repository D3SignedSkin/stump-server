namespace Stump.Server.WorldServer.Game.Breach
{
    public class BreachBoost
    {
        public uint Id { get; private set; }

        public uint Price { get; private set; }

        public BreachBoost(uint id, uint price)
        {
            Id = id;
            Price = price;
        }
    }
}