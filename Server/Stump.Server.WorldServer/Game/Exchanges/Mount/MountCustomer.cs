using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using System.Collections.Generic;
using System.Linq;

namespace Stump.Server.WorldServer.Game.Exchanges.MountsExchange
{
    public class MountCustomer : Exchanger
    {
        public MountCustomer(Character character, MountDialog dialog) : base(dialog)
        {
            Character = character;
        }

        public Character Character
        {
            get;
        }

        public override bool MoveItem(int id, int quantity)
        {
            if (quantity > 0)
            {
                int CharLimitMount = 0;
                int ItemsInventoryCount = Character.EquippedMount.Inventory.Count();

                if (Character.UserGroup.Role <= RoleEnum.Player)
                    CharLimitMount = 150;
                else if (Character.UserGroup.Role == RoleEnum.Vip)
                    CharLimitMount = 500;
                else if (Character.UserGroup.Role >= RoleEnum.Gold_Vip)
                    CharLimitMount = 1500;

                if ((ItemsInventoryCount + quantity) > CharLimitMount)
                {
                    Character.SendServerMessageLang
                        (
                        "Você atingiu a capacidade máxima disponível de items no inventário da sua montaria.",
                        "You have reached the maximum available capacity of items in your mount's inventory.",
                        "Has alcanzado la capacidad máxima disponible de artículos en el inventario de tu montura.",
                        "Vous avez atteint la capacité maximale disponible d'objets dans l'inventaire de votre monture."
                        );
                }

                var item = Character.Inventory.TryGetItem(id);

                return item != null && Character.EquippedMount.Inventory.StoreItem(item, quantity, true) != null;
            }

            if (quantity >= 0)
                return false;

            var deleteItem = Character.EquippedMount.Inventory.TryGetItem(id);

            return Character.EquippedMount.Inventory.TakeItemBack(deleteItem, -quantity, true) != null;
        }

        public override bool SetKamas(long amount)
        {
            if (amount > 0)
                return Character.Bank.StoreKamas(amount);

            return amount < 0 && Character.Bank.TakeKamas(-amount);
        }

        public void MoveItems(bool store, IEnumerable<uint> guids, bool all, bool existing)
        {
            var guids_ = new List<int>();

            foreach(var id in guids)
            {
                guids_.Add((int)id);
            }

            if (store)
                Character.EquippedMount.Inventory.StoreItems(guids_, all, existing);
            else
                Character.EquippedMount.Inventory.TakeItemsBack(guids_, all, existing);
        }
    }
}