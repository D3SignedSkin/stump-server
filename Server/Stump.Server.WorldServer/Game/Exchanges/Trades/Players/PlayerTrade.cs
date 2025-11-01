using System;
using System.Globalization;
using System.Linq;
using MongoDB.Bson;
using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Logging;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Items.Player;
using Stump.Server.WorldServer.Handlers.Inventory;

namespace Stump.Server.WorldServer.Game.Exchanges.Trades.Players
{
    public class PlayerTrade : Trade<PlayerTrader, PlayerTrader>
    {
        //  private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        public PlayerTrade(Character first, Character second)
        {
            FirstTrader = new PlayerTrader(first, this);
            SecondTrader = new PlayerTrader(second, this);
        }

        public override ExchangeTypeEnum ExchangeType
        {
            get
            {
                return ExchangeTypeEnum.PLAYER_TRADE;
            }
        }

        public override void Open()
        {
            base.Open();
            FirstTrader.Character.SetDialoger(FirstTrader);
            SecondTrader.Character.SetDialoger(SecondTrader);

            InventoryHandler.SendExchangeStartedWithPodsMessage(FirstTrader.Character.Client, this);
            InventoryHandler.SendExchangeStartedWithPodsMessage(SecondTrader.Character.Client, this);
        }

        public override void Close()
        {
            base.Close();

            InventoryHandler.SendExchangeLeaveMessage(FirstTrader.Character.Client, DialogTypeEnum.DIALOG_EXCHANGE, FirstTrader.ReadyToApply && SecondTrader.ReadyToApply);
            InventoryHandler.SendExchangeLeaveMessage(SecondTrader.Character.Client, DialogTypeEnum.DIALOG_EXCHANGE, FirstTrader.ReadyToApply && SecondTrader.ReadyToApply);

            FirstTrader.Character.CloseDialog(this);
            SecondTrader.Character.CloseDialog(this);
        }

        protected override void Apply()
        {
            // check all items are still there
            if (!FirstTrader.Items.All(x =>
            {
                var item = FirstTrader.Character.Inventory.TryGetItem(x.Guid);

                return item != null && item.Stack >= x.Stack && !item.IsEquiped();
            }))
            {
                return;
            }

            if (!SecondTrader.Items.All(x =>
            {
                var item = SecondTrader.Character.Inventory.TryGetItem(x.Guid);

                return item != null && item.Stack >= x.Stack && !item.IsEquiped();
            }))
            {
                return;
            }

            //Check if kamas still here
            if (FirstTrader.Character.Inventory.Kamas < FirstTrader.Kamas || SecondTrader.Character.Inventory.Kamas < SecondTrader.Kamas)
                return;

            FirstTrader.Character.Inventory.SetKamas((FirstTrader.Character.Inventory.Kamas + (SecondTrader.Kamas - FirstTrader.Kamas)));
            SecondTrader.Character.Inventory.SetKamas((SecondTrader.Character.Inventory.Kamas + (FirstTrader.Kamas - SecondTrader.Kamas)));

            // trade items
            foreach (var tradeItem in FirstTrader.Items)
            {
                var item = FirstTrader.Character.Inventory.TryGetItem(tradeItem.Guid);

                FirstTrader.Character.Inventory.ChangeItemOwner(SecondTrader.Character, item, (int)tradeItem.Stack);
            }

            foreach (var tradeItem in SecondTrader.Items)
            {
                var item = SecondTrader.Character.Inventory.TryGetItem(tradeItem.Guid);

                SecondTrader.Character.Inventory.ChangeItemOwner(FirstTrader.Character, item, (int)tradeItem.Stack);
            }

            InventoryHandler.SendInventoryWeightMessage(FirstTrader.Character.Client);
            InventoryHandler.SendInventoryWeightMessage(SecondTrader.Character.Client);

            #region // ----------------- Sistema de Logs MongoDB Player Trade by: Kenshin ---------------- //
            try
            {
                var FirstTraderRank = "Player";
                var SecondTraderRank = "Player";

                if (FirstTrader.Character.Client.Account.UserGroupId >= 4 && FirstTrader.Character.Client.Account.UserGroupId <= 9)
                    FirstTraderRank = "Staff";

                if (SecondTrader.Character.Client.Account.UserGroupId >= 4 && SecondTrader.Character.Client.Account.UserGroupId <= 9)
                    SecondTraderRank = "Staff";

                var document = new BsonDocument
                    {
                        { "FirstTraderId", FirstTrader.Character.Id },
                        { "FirstTraderName", FirstTrader.Character.Name },
                        { "FirstTraderHardwareId", FirstTrader.Character.Account.LastHardwareId },
                        { "FirstTraderUserGroup", FirstTraderRank },
                        { "SecondTraderId", SecondTrader.Character.Id },
                        { "SecondTraderName", SecondTrader.Character.Name },
                        { "SecondTraderHardwareId", SecondTrader.Character.Account.LastHardwareId },
                        { "SecondTraderUserGroup", SecondTraderRank },
                        { "FirstTraderKamas", FirstTrader.Kamas },
                        { "SecondTraderKamas", SecondTrader.Kamas },
                        { "FirstTraderItems", FirstTrader.ItemsString },
                        { "SecondTraderItems", SecondTrader.ItemsString },
                        { "Date", DateTime.Now.ToString(CultureInfo.CreateSpecificCulture("pt-BR")) }
                    };

                MongoLogger.Instance.Insert("Player_Trade", document);
            }
            catch (Exception e)
            {
                Console.WriteLine("Erro no Mongologs do Player Trade : " + e.Message);
            }
            #endregion

            // logger.Info("save from in queue {0}", "PlayerTrade1");
            FirstTrader.Character.SaveLater();
            // logger.Info("save from in queue {0}", "PlayerTrade2");
            SecondTrader.Character.SaveLater();
        }

        protected override void OnTraderItemMoved(Trader trader, TradeItem item, bool modified, int difference)
        {
            bool HasTokenEffect = item.Effects.Any(effect => effect.EffectId == EffectsEnum.Effect_AddOgrines);

            //Restricción de Hydra del personal Por:Kenshin
            if (FirstTrader.Character.Client.UserGroup.Role >= RoleEnum.Moderator_Helper && SecondTrader.Character.Client.UserGroup.Role <= RoleEnum.Gold_Vip && HasTokenEffect)
                return;

            //Restricción de Hydra del personal Por:Kenshin
            if (SecondTrader.Character.Client.UserGroup.Role >= RoleEnum.Moderator_Helper && FirstTrader.Character.Client.UserGroup.Role <= RoleEnum.Gold_Vip && HasTokenEffect)
                return;

            if (item.Template.Id == Inventory.TokenTemplateId)
                return;

            base.OnTraderItemMoved(trader, item, modified, difference);

            FirstTrader.ToggleReady(false);
            SecondTrader.ToggleReady(false);

            if (item.Stack == 0)
            {
                InventoryHandler.SendExchangeObjectRemovedMessage(FirstTrader.Character.Client, trader != FirstTrader, item.Guid);
                InventoryHandler.SendExchangeObjectRemovedMessage(SecondTrader.Character.Client, trader != SecondTrader, item.Guid);
            }
            else if (modified)
            {
                InventoryHandler.SendExchangeObjectModifiedMessage(FirstTrader.Character.Client, trader != FirstTrader, item);
                InventoryHandler.SendExchangeObjectModifiedMessage(SecondTrader.Character.Client, trader != SecondTrader, item);
            }
            else
            {
                InventoryHandler.SendExchangeObjectAddedMessage(FirstTrader.Character.Client, trader != FirstTrader, item);
                InventoryHandler.SendExchangeObjectAddedMessage(SecondTrader.Character.Client, trader != SecondTrader, item);
            }
        }

        protected override void OnTraderKamasChanged(Trader trader, ulong amount)
        {
            base.OnTraderKamasChanged(trader, amount);

            InventoryHandler.SendExchangeKamaModifiedMessage(FirstTrader.Character.Client, trader != FirstTrader, (long)amount);
            InventoryHandler.SendExchangeKamaModifiedMessage(SecondTrader.Character.Client, trader != SecondTrader, (long)amount);
        }

        protected override void OnTraderReadyStatusChanged(Trader trader, bool status)
        {
            base.OnTraderReadyStatusChanged(trader, status);

            InventoryHandler.SendExchangeIsReadyMessage(FirstTrader.Character.Client, trader, status);
            InventoryHandler.SendExchangeIsReadyMessage(SecondTrader.Character.Client, trader, status);
        }
    }
}