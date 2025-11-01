using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Commands;
using Stump.Server.WorldServer.Commands.Commands.Patterns;
using Stump.Server.WorldServer.Commands.Trigger;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Merchants;
using Stump.Server.WorldServer.Handlers.Inventory;
using System.Linq;
using System;
using Stump.Server.WorldServer.Core.Network;
using MongoDB.Bson;
using Stump.Server.BaseServer.Logging;
using System.Globalization;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;

namespace Stump.Server.WorldServer.Commands.Commands.GameMaster
{
    public class MerchantKickCommands : SubCommandContainer
    {
        public MerchantKickCommands()
        {
            Aliases = new[] { "merchant" };
            Description = "Provides commands to manage merchants";
            RequiredRole = RoleEnum.GameMaster;
        }
    }

    public class MerchantKickCommand : InGameSubCommand
    {
        public MerchantKickCommand()
        {
            Aliases = new[] { "kick" };
            Description = "Kick the target merchant";
            ParentCommandType = typeof(MerchantKickCommands);
            RequiredRole = RoleEnum.GameMaster;
            AddParameter<string>("target", "t", "Target merchant");
        }

        public override void Execute(GameTrigger trigger)
        {
            var target = trigger.Character.Map.GetActors<Merchant>(x => x.Name == trigger.Get<string>("target")).FirstOrDefault();
            Character player = trigger.Character;

            if (target == null)
            {
                trigger.ReplyError("Target not found !");
                return;
            }

            MerchantManager.Instance.RemoveMerchantSpawn(target.Record);
            MerchantManager.Instance.UnActiveMerchant(target);

            trigger.Reply("Target Merchant kicked");

            var source = trigger.GetSource() as WorldClient;

            #region // ----------------- Sistema de Logs MongoDB Kick by: Kenshin ---------------- //
            try
            {
                var document = new BsonDocument
                                {
                                    { "Staff_IP", source.IP },
                                    { "StaffName", source.Character.Name },
                                    { "Target_ID", target.Id },
                                    { "Target_Name", player.NameClean },
                                    { "Target_IP", player.Client.IP },
                                    { "Date", DateTime.Now.ToString(CultureInfo.CreateSpecificCulture("pt-BR")) }
                                };

                MongoLogger.Instance.Insert("Staff_ManagementMerchant", document);
            }
            catch (Exception e)
            {
                Console.WriteLine("Erro no Mongologs Merchant : " + e.Message);
            }
            #endregion

        }
    }

    public class MerchantShowCommand : TargetSubCommand
    {
        public MerchantShowCommand()
        {
            Aliases = new[] { "show" };
            Description = "Show the target merchantbag";
            ParentCommandType = typeof(MerchantKickCommands);
            RequiredRole = RoleEnum.GameMaster;
            AddTargetParameter();
        }

        public override void Execute(TriggerBase trigger)
        {
            var gameTrigger = trigger as GameTrigger;
            if (gameTrigger == null)
                return;

            var target = GetTarget(trigger);

            InventoryHandler.SendExchangeStartedMessage(gameTrigger.Character.Client, ExchangeTypeEnum.SHOP_STOCK);
            InventoryHandler.SendExchangeShopStockStartedMessage(gameTrigger.Character.Client, target.MerchantBag);
        }
    }
}
