using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Handlers.Basic;

namespace Stump.Server.WorldServer.Game.Exchanges.Merchant
{
    public class CharacterMerchant : Exchanger
    {
        public CharacterMerchant(Character character, MerchantExchange merchantTrade)
            : base(merchantTrade)
        {
            Character = character;
        }

        public Character Character
        {
            get;
        }

        public override bool MoveItem(int id, int quantity)
        {
            if (quantity >= 0)
                return false;

            var deleteItem = Character.MerchantBag.TryGetItem(id);

            return deleteItem != null && Character.MerchantBag.TakeBack(deleteItem, -quantity);
        }

        public bool MovePricedItem(int id, int quantity, uint price)
        {
            if (quantity <= 0)
                return false;

            var item = Character.Inventory.TryGetItem(id);

            // TODO- Bloqueio para o erro do BIDHouse da montaria blindada
            //if (item.Template.TypeId == 97)
            //    return false;

            //Bloqueio de Venda de Itens comprados em NPCs Shops
            if (item.Effects.Exists(x => x.EffectId == EffectsEnum.Effect_BlockItemNpcShop) && Character.Account.UserGroupId <= 3)
            {
                BasicHandler.SendTextInformationMessage(Character.Client, TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 345, item.Template.Id, item.Guid);
                return false;
            }

            return item != null && Character.MerchantBag.StoreItem(item, quantity, price);
        }

        public bool ModifyItem(int id, int quantity, uint price)
        {
            var item = Character.MerchantBag.TryGetItem(id);

            return item != null && Character.MerchantBag.ModifyItem(item, quantity, price);
        }

        public override bool SetKamas(long amount) => false;
    }
}
