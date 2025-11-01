using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Prisms;
using Stump.Server.WorldServer.Game.Exchanges.Trades;


namespace Stump.Server.WorldServer.Game.Exchanges.Prism
{
    public class PrismTrader : Trader
    {
        public PrismTrader(PrismNpc taxCollector, Character character, AlliancePrismTrade taxCollectorTrade)
            : base(taxCollectorTrade)
        {
            PrismNpc = taxCollector;
            Character = character;
        }

        public override int Id => Character.Id;

        public PrismNpc PrismNpc { get; }

        public Character Character { get; }

        public override bool MoveItem(int id, int quantity)
        {
            bool result;
            if (quantity <= 0 || quantity > 1 || Character.Map.Prism == null || Character.Map.Prism != PrismNpc ||
                (Character.Guild != null && Character.Map.Prism.Alliance.Id != Character.Guild?.Alliance?.Id))
            {
                Character.SendInformationMessage(DofusProtocol.Enums.TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 101);
                return false;
            }
            var taxCollectorItem = Character.Inventory.TryGetItem(id);
            if (taxCollectorItem == null)
                result = false;
            else
            {
                PrismNpc.Bag.MoveToInventory(taxCollectorItem, Character, 1);
                result = true;
            }
            return result;
        }

        public override bool SetKamas(long amount)
        {
            return false;
        }
    }
}