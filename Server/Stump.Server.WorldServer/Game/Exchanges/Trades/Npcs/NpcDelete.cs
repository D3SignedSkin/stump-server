using MongoDB.Bson;
using Stump.Core.Extensions;
using Stump.Core.IO;
using Stump.Core.Reflection;
using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Logging;
using Stump.Server.WorldServer.Database.Items.Templates;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Npcs;
using Stump.Server.WorldServer.Game.Exchanges.Trades;
using Stump.Server.WorldServer.Game.Exchanges.Trades.Npcs;
using Stump.Server.WorldServer.Game.Exchanges.Trades.Players;
using Stump.Server.WorldServer.Game.Items;
using Stump.Server.WorldServer.Game.Items.Player;
using Stump.Server.WorldServer.Handlers.Inventory;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
namespace Stump.Server.WorldServer.Game.Exchanges
{
    public class NpcDelete : Trade<PlayerTrader, NpcDeleter>
    {
        public int Kamas
        {
            get;
            set;
        }

        public override ExchangeTypeEnum ExchangeType
        {
            get
            {
                return ExchangeTypeEnum.NPC_TRADE;
            }
        }

        public NpcDelete(Character character, Npc npc, int kamas)
        {
            Kamas = kamas;
            base.FirstTrader = new PlayerTrader(character, this);
            base.SecondTrader = new NpcDeleter(npc, this);
        }

        public override void Open()
        {
            base.Open();
            base.FirstTrader.Character.SetDialoger(base.FirstTrader);
            InventoryHandler.SendExchangeStartOkNpcDeleteMessage(base.FirstTrader.Character.Client, this);
        }

        public override void Close()
        {
            base.Close();
            InventoryHandler.SendExchangeLeaveMessage(base.FirstTrader.Character.Client, DialogTypeEnum.DIALOG_EXCHANGE, base.FirstTrader.ReadyToApply);
            base.FirstTrader.Character.CloseDialog(this);
        }

        protected override void OnTraderItemMoved(Trader trader, TradeItem item, bool modified, int difference)
        {
            uint StacksCount = 0;

            base.OnTraderItemMoved(trader, item, modified, difference);

            foreach (TradeItem current in base.FirstTrader.Items)
            {
                StacksCount += current.Stack;
            }

            if (item.Stack == 0u)
            {
                if (trader is PlayerTrader)
                {
                    SecondTrader.SetKamas((int)(Kamas * StacksCount));
                }

                InventoryHandler.SendExchangeObjectRemovedMessage(base.FirstTrader.Character.Client, trader != base.FirstTrader, item.Guid);
            }
            else
            {
                if (modified)
                {
                    InventoryHandler.SendExchangeObjectModifiedMessage(base.FirstTrader.Character.Client, trader != base.FirstTrader, item);
                }
                else
                {
                    InventoryHandler.SendExchangeObjectAddedMessage(base.FirstTrader.Character.Client, trader != base.FirstTrader, item);
                }

                //NPC Calcular a quantidade de itens que está na troca e entrega a quantidade de kamas determinada
                if (trader is PlayerTrader)
                {
                    if (StacksCount != 0)
                    {
                        SecondTrader.SetKamas((int)(Kamas * StacksCount));
                    }
                    else
                    {
                        SecondTrader.SetKamas(0);
                    }
                }
            }
        }

        protected override void Apply()
        {
            uint StacksCount = 0;

            if (base.FirstTrader.Items.All(delegate (TradeItem x)
            {
                BasePlayerItem basePlayerItem = base.FirstTrader.Character.Inventory.TryGetItem(x.Guid);
                return basePlayerItem != null && basePlayerItem.Stack >= x.Stack;
            }))
            {
                base.FirstTrader.Character.Inventory.SetKamas(base.FirstTrader.Character.Inventory.Kamas + (base.SecondTrader.Kamas - base.FirstTrader.Kamas));
                base.FirstTrader.Character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 45, base.SecondTrader.Kamas);

                foreach (TradeItem current in base.FirstTrader.Items)
                {
                    BasePlayerItem item = base.FirstTrader.Character.Inventory.TryGetItem(current.Guid);
                    base.FirstTrader.Character.Inventory.RemoveItem(item, amount: (int)current.Stack, delete: true);

                    StacksCount += current.Stack;
                }

                InventoryHandler.SendInventoryWeightMessage(base.FirstTrader.Character.Client);
            }

            #region // ----------------- Sistema de Logs MongoDB Itens Destrui by: Kenshin ---------------- //
            try
            {
                var CharacterRank = "Player";

                if (FirstTrader.Character.Account.UserGroupId >= 4 && FirstTrader.Character.Account.UserGroupId <= 9)
                    CharacterRank = "Staff";

                var document = new BsonDocument
                                {
                                    { "AccountId", FirstTrader.Character.Account.Id },
                                    { "AccountName", FirstTrader.Character.Account.Login },
                                    { "AccountHardwareId", FirstTrader.Character.Account.LastHardwareId },
                                    { "AccountUserGroup", CharacterRank },
                                    { "CharacterId", FirstTrader.Character.Id },
                                    { "CharacterName", FirstTrader.Character.Name },
                                    { "ItemAmount", StacksCount},
                                    { "KamasAmount", (StacksCount * Kamas)},
                                    { "Date", DateTime.Now.ToString(CultureInfo.CreateSpecificCulture("pt-BR")) }
                                };

                MongoLogger.Instance.Insert("Npc_Destroy", document);
            }
            catch (Exception e)
            {
                Console.WriteLine("Erro no Mongologs das NPC Destroy : " + e.Message);
            }
            #endregion
        }

        protected override void OnTraderReadyStatusChanged(Trader trader, bool status)
        {
            base.OnTraderReadyStatusChanged(trader, status);
            InventoryHandler.SendExchangeIsReadyMessage(base.FirstTrader.Character.Client, trader, status);

            if (trader is PlayerTrader && status)
            {
                base.SecondTrader.ToggleReady(true);
            }
        }

        protected override void OnTraderKamasChanged(Trader trader, ulong amount)
        {
            base.OnTraderKamasChanged(trader, amount);
            InventoryHandler.SendExchangeKamaModifiedMessage(base.FirstTrader.Character.Client, trader != base.FirstTrader, (long)amount);
        }
    }
}
