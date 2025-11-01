using System;
using System.Linq;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Items;
using Stump.Server.WorldServer.Game.Items.BidHouse;
using Stump.Server.WorldServer.Handlers.Inventory;

namespace Stump.Server.WorldServer.Game.Exchanges.BidHouse
{
    public class BidHouseExchanger : Exchanger
    {
        public BidHouseExchanger(Character character, BidHouseExchange exchange) : base(exchange)
        {
            Character = character;
        }

        public Character Character
        {
            get;
            private set;
        }

        public override bool MoveItem(int id, int quantity)
        {
            var item = BidHouseManager.Instance.GetBidHouseItem(id);

            if (item == null)
                return false;

            BidHouseManager.Instance.RemoveBidHouseItem(item);

            var newItem = ItemManager.Instance.CreatePlayerItem(Character, item.Template, (int)item.Stack, item.Effects);

            Character.Inventory.AddItem(newItem);

            InventoryHandler.SendExchangeBidHouseItemRemoveOkMessage(Character.Client, item.Guid);

            return true;
        }

        public bool MovePricedItem(uint id, int quantity, long price)
        {
            if (!BidHouseManager.Quantities.Contains(quantity))
                return false;

            var item = Character.Inventory.TryGetItem((int)id);

            if (item == null)
                return false;

            //if (item.IsLinkedToPlayer() || item.IsLinkedToAccount())
            //    return false;

            if (item.Template.Level > ((BidHouseExchange)Dialog).MaxItemLevel)
                return false;

            if (BidHouseManager.Instance.GetBidHouseItems(Character.Account.Id, ((BidHouseExchange)Dialog).Types).Count() >= Character.Level)
                return false;

            if (quantity > item.Stack)
                quantity = (int)item.Stack;

            long tax = (long)Math.Round((price * BidHouseManager.TaxPercent) / 100);

            //BIDHOUSE Kamas (DESATIVADO)
            //if (Character.Kamas < (long)tax)
            //{
            //    //Vous ne disposez pas d'assez de kamas pour acquiter la taxe de mise en vente...
            //    Character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 57);
            //    return false;
            //}

            if (Character.Client.Account.Tokens < tax)
            {
                Character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 57); //Vous ne disposez pas d'assez de kamas pour acquiter la taxe de mise en vente...
                return false;
            }

            //BIDHOUSE Kamas (DESATIVADO)
            //Character.Inventory.SubKamas(tax);

            var bidItem = BidHouseManager.Instance.CreateBidHouseItem(Character, item, quantity, price);

            if (Character.Inventory.RemoveTokenItem((int)tax, "BidHouseTax: " + bidItem.Template.Name))
            {
                BidHouseManager.Instance.AddBidHouseItem(bidItem);
                Character.Inventory.RemoveItem(item, quantity);
                InventoryHandler.SendExchangeBidHouseItemAddOkMessage(Character.Client, bidItem.GetObjectItemToSellInBid());
            }
            else
            {
                return false;
            }

            return true;
        }

        public bool ModifyItem(uint id, long price)
        {
            var item = BidHouseManager.Instance.GetBidHouseItem((int)id);

            if (item == null)
                return false;

            if (item.Template.Level > ((BidHouseExchange)Dialog).MaxItemLevel)
                return false;

            var diff = (item.Price - price);
            long tax = 0;

            tax = (diff < 0 ? (int)Math.Round((Math.Abs(diff) * BidHouseManager.TaxPercent) / 100) : (int)Math.Round((Math.Abs(price) * BidHouseManager.TaxModificationPercent) / 100));

            //BIDHOUSE Kamas (DESATIVADO)
            //if (Character.Kamas < tax)
            //{
            //    //Vous ne disposez pas d'assez de kamas pour acquiter la taxe de mise en vente...
            //    Character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 57);
            //    return false;
            //}

            if (Character.Client.Account.Tokens < tax)
            {
                Character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 57); //Vous ne disposez pas d'assez de kamas pour acquiter la taxe de mise en vente...
                return false;
            }

            //BIDHOUSE Kamas (DESATIVADO)
            //Character.Inventory.SubKamas(tax);

            if (Character.Inventory.RemoveTokenItem((int)tax, "BidHouseTax: " + item.Template.Name))
            {
                item.Price = (uint)price;
                InventoryHandler.SendExchangeBidHouseItemRemoveOkMessage(Character.Client, item.Guid);
                InventoryHandler.SendExchangeBidHouseItemAddOkMessage(Character.Client, item.GetObjectItemToSellInBid());
            }
            else
            {
                return false;
            }

            return true;
        }

        public override bool SetKamas(long amount)
        {
            return false;
        }
    }
}
