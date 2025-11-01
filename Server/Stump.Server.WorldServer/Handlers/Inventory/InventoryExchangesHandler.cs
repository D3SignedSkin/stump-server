using MongoDB.Bson;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Enums.Custom;
using Stump.DofusProtocol.Messages;
using Stump.DofusProtocol.Types;
using Stump.Server.BaseServer.Logging;
using Stump.Server.BaseServer.Network;
using Stump.Server.WorldServer.Core.Network;
using Stump.Server.WorldServer.Database.Interactives;
using Stump.Server.WorldServer.Database.Items.Templates;
using Stump.Server.WorldServer.Database.Jobs;
using Stump.Server.WorldServer.Discord;
using Stump.Server.WorldServer.Game;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Merchants;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Mounts;
using Stump.Server.WorldServer.Game.Actors.RolePlay.TaxCollectors;
using Stump.Server.WorldServer.Game.Dialogs;
using Stump.Server.WorldServer.Game.Dialogs.Jobs;
using Stump.Server.WorldServer.Game.Dialogs.Merchants;
using Stump.Server.WorldServer.Game.Dialogs.Npcs;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Exchanges;
using Stump.Server.WorldServer.Game.Exchanges.Bank;
using Stump.Server.WorldServer.Game.Exchanges.BidHouse;
using Stump.Server.WorldServer.Game.Exchanges.Craft;
using Stump.Server.WorldServer.Game.Exchanges.Craft.Runes;
using Stump.Server.WorldServer.Game.Exchanges.Merchant;
using Stump.Server.WorldServer.Game.Exchanges.MountsExchange;
using Stump.Server.WorldServer.Game.Exchanges.Paddock;
using Stump.Server.WorldServer.Game.Exchanges.TaxCollector;
using Stump.Server.WorldServer.Game.Exchanges.Trades;
using Stump.Server.WorldServer.Game.Exchanges.Trades.Players;
using Stump.Server.WorldServer.Game.Formulas;
using Stump.Server.WorldServer.Game.Interactives.Skills;
using Stump.Server.WorldServer.Game.Items;
using Stump.Server.WorldServer.Game.Items.BidHouse;
using Stump.Server.WorldServer.Game.Items.Player;
using Stump.Server.WorldServer.Game.Jobs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;

namespace Stump.Server.WorldServer.Handlers.Inventory
{
    public partial class InventoryHandler : WorldHandlerContainer
    {
        public static uint[] TradeRuneTypes = { 1, 9, 10, 11, 16, 17, 81, 82, 2, 3, 4, 5, 6, 7, 8, 19, 21, 22 };

        [WorldHandler(ExchangePlayerMultiCraftRequestMessage.Id)]
        public static void HandleExchangePlayerMultiCraftRequestMessage(WorldClient client, ExchangePlayerMultiCraftRequestMessage message)
        {
            var target = client.Character.Map.GetActor<Character>((int)message.target);

            if (target == null)
            {
                SendExchangeErrorMessage(client, ExchangeErrorEnum.BID_SEARCH_ERROR);
                return;
            }

            if (target.Map.Id != client.Character.Map.Id)
            {
                SendExchangeErrorMessage(client, ExchangeErrorEnum.REQUEST_CHARACTER_TOOL_TOO_FAR);
                return;
            }

            if (target.IsBusy() || target.IsTrading())
            {
                SendExchangeErrorMessage(client, ExchangeErrorEnum.REQUEST_CHARACTER_OCCUPIED);
                return;
            }

            if (target.FriendsBook.IsIgnored(client.Account.Id))
            {
                SendExchangeErrorMessage(client, ExchangeErrorEnum.REQUEST_CHARACTER_RESTRICTED);
                return;
            }

            if (!target.IsAvailable(client.Character, false))
            {
                SendExchangeErrorMessage(client, ExchangeErrorEnum.REQUEST_CHARACTER_OCCUPIED);
                return;
            }

            if (!client.Character.Map.AllowExchangesBetweenPlayers)
            {
                SendExchangeErrorMessage(client, ExchangeErrorEnum.REQUEST_IMPOSSIBLE);
                return;
            }

            var interactives = client.Character.Map.
                GetInteractiveObjects().Where(x => x.GetSkills().Any(y => y.SkillTemplate?.Id == message.skillId && y.IsEnabled(client.Character))).ToArray();

            if (interactives.All(x => x.Position.Point.EuclideanDistanceTo(client.Character.Position.Point) >= 2))
            {
                SendExchangeErrorMessage(client, ExchangeErrorEnum.REQUEST_CHARACTER_TOOL_TOO_FAR);
                return;
            }

            var interactive = interactives.FirstOrDefault();

            if (interactive == null)
            {
                SendExchangeErrorMessage(client, ExchangeErrorEnum.REQUEST_CHARACTER_TOOL_TOO_FAR);
                return;
            }

            var skill = interactive.GetSkills().First(x => x.SkillTemplate?.Id == message.skillId);

            var dialog = new MultiCraftRequest(client.Character, target, interactive, skill);
            dialog.Open();
        }

        [WorldHandler(ExchangePlayerRequestMessage.Id)]
        public static void HandleExchangePlayerRequestMessage(WorldClient client, ExchangePlayerRequestMessage message)
        {
            switch ((ExchangeTypeEnum)message.exchangeType)
            {
                case ExchangeTypeEnum.PLAYER_TRADE:
                    var target = World.Instance.GetCharacter((int)message.target);

                    if (target == null)
                    {
                        SendExchangeErrorMessage(client, ExchangeErrorEnum.BID_SEARCH_ERROR);
                        return;
                    }

                    if (target.Map.Id != client.Character.Map.Id)
                    {
                        SendExchangeErrorMessage(client, ExchangeErrorEnum.REQUEST_CHARACTER_TOOL_TOO_FAR);
                        return;
                    }

                    if (target.IsBusy() || target.IsTrading())
                    {
                        SendExchangeErrorMessage(client, ExchangeErrorEnum.REQUEST_CHARACTER_OCCUPIED);
                        return;
                    }

                    if (target.FriendsBook.IsIgnored(client.Account.Id))
                    {
                        SendExchangeErrorMessage(client, ExchangeErrorEnum.REQUEST_CHARACTER_RESTRICTED);
                        return;
                    }

                    if (!target.IsAvailable(client.Character, false))
                    {
                        SendExchangeErrorMessage(client, ExchangeErrorEnum.REQUEST_CHARACTER_OCCUPIED);
                        return;
                    }

                    //Restrição Staff Hydra By:Kenshin
                    if (target.IsGameMaster() && target.Client.UserGroup.Role >= RoleEnum.Moderator_Helper && target.Client.UserGroup.Role <= RoleEnum.GameMaster && client.Character.Client.UserGroup.Role <= RoleEnum.Gold_Vip)
                    {
                        SendExchangeErrorMessage(client, ExchangeErrorEnum.REQUEST_IMPOSSIBLE);
                        return;
                    }

                    //Restrição Staff Hydra By:Kenshin
                    if (client.Character.Client.UserGroup.Role >= RoleEnum.Moderator_Helper && client.Character.Client.UserGroup.Role <= RoleEnum.GameMaster && client.Character.IsGameMaster() && target.Client.UserGroup.Role >= RoleEnum.Gold_Vip || client.Character.Invisible)
                    {
                        SendExchangeErrorMessage(client, ExchangeErrorEnum.REQUEST_IMPOSSIBLE);
                        return;
                    }

                    if (!client.Character.Map.AllowExchangesBetweenPlayers)
                    {
                        SendExchangeErrorMessage(client, ExchangeErrorEnum.REQUEST_IMPOSSIBLE);
                        return;
                    }

                    var request = new PlayerTradeRequest(client.Character, target);
                    client.Character.OpenRequestBox(request);
                    target.OpenRequestBox(request);
                    request.Open();

                    if (DiscordIntegration.EnableDiscordWebHook)
                    {
                        //WebHook Report Staff Hydra By:Kenshin
                        if (target.IsGameMaster() && target.Client.UserGroup.Role >= RoleEnum.Moderator_Helper && target.Client.UserGroup.Role <= RoleEnum.Administrator && client.Character.Client.UserGroup.Role <= RoleEnum.Gold_Vip)
                        {
                            PlainText.SendWebHook(DiscordIntegration.DiscordChatStaffLogsUrl, $"O Staff **{target.Namedefault}** iniciou uma troca com o jogador **{client.Character.Namedefault}**.", DiscordIntegration.DiscordWHUsername);
                        }

                        //WebHook Report Staff Hydra By:Kenshin
                        if (client.Character.Client.UserGroup.Role >= RoleEnum.Moderator_Helper && client.Character.Client.UserGroup.Role <= RoleEnum.Administrator && client.Character.IsGameMaster() && target.Client.UserGroup.Role >= RoleEnum.Gold_Vip || client.Character.Invisible)
                        {
                            PlainText.SendWebHook(DiscordIntegration.DiscordChatStaffLogsUrl, $"O Staff **{client.Character.Namedefault}** iniciou uma troca com o jogador **{target.Namedefault}**.", DiscordIntegration.DiscordWHUsername);
                        }
                    }
                    break;
                default:
                    SendExchangeErrorMessage(client, ExchangeErrorEnum.REQUEST_IMPOSSIBLE);
                    break;
            }
        }

        [WorldHandler(ExchangeAcceptMessage.Id)]
        public static void HandleExchangeAcceptMessage(WorldClient client, ExchangeAcceptMessage message)
        {
            if (client.Character.IsInRequest() && client.Character.RequestBox.IsExchangeRequest)
                client.Character.AcceptRequest();
        }

        [WorldHandler(ExchangeObjectMoveKamaMessage.Id)]
        public static void HandleExchangeObjectMoveKamaMessage(WorldClient client, ExchangeObjectMoveKamaMessage message)
        {
            if (!client.Character.IsInExchange())
                return;

            client.Character.Exchanger.SetKamas(message.quantity);
        }

        [WorldHandler(ExchangeCraftPaymentModificationRequestMessage.Id)]
        public static void HandleExchangeCraftPaymentModificationRequestMessage(WorldClient client, ExchangeCraftPaymentModificationRequestMessage message)
        {
            if (!(client.Character.Dialoger is CraftCustomer))
                return;

            client.Character.Exchanger.SetKamas((int)message.quantity);
        }

        [WorldHandler(ExchangeObjectMoveMessage.Id)]
        public static void HandleExchangeObjectMoveMessage(WorldClient client, ExchangeObjectMoveMessage message)
        {
            if (client.Character.Dialoger is RuneCrafter)
            {
                uint slotRuneCount = 0;

                if ((client.Character.Dialoger as RuneCrafter).Trade.FirstTrader.Items.Any(x => x.Template.TypeId == 78 || x.Template.TypeId == 211 || x.Template.TypeId == 212))
                    slotRuneCount = (client.Character.Dialoger as RuneCrafter).Trade.FirstTrader.Items.FirstOrDefault(x => x.Template.TypeId == 78 || x.Template.TypeId == 211 || x.Template.TypeId == 212).Stack;

                if (slotRuneCount >= 1000)
                    return;

                int quantity = client.Character.Dialoger is RuneCrafter ? message.quantity > 1000 ? 1000 : message.quantity : message.quantity;

                if (client.Character.IsInExchange())
                    client.Character.Exchanger.MoveItem((int)message.objectUID, quantity);
            }
            else
            {
                if (client.Character.IsInExchange())
                    client.Character.Exchanger.MoveItem((int)message.objectUID, message.quantity);
            }
        }

        [WorldHandler(ExchangeReadyMessage.Id)]
        public static void HandleExchangeReadyMessage(WorldClient client, ExchangeReadyMessage message)
        {
            if (message == null || client == null)
                return;

            client.Character.Trader?.ToggleReady(message.ready);
        }

        [WorldHandler(FocusedExchangeReadyMessage.Id)]
        public static void HandleFocusedExchangeReadyMessage(WorldClient client, FocusedExchangeReadyMessage message)
        {
            if (message == null || client == null)
                return;

            HandleExchangeReadyMessage(client, message);
        }

        [WorldHandler(ExchangeBuyMessage.Id)]
        public static void HandleExchangeBuyMessage(WorldClient client, ExchangeBuyMessage message)
        {
            var dialog = client.Character.Dialog as IShopDialog;

            if (dialog != null)
                dialog.BuyItem((int)message.objectToBuyId, (int)message.quantity);
        }

        [WorldHandler(ExchangeSellMessage.Id)]
        public static void HandleExchangeSellMessage(WorldClient client, ExchangeSellMessage message)
        {
            var dialog = client.Character.Dialog as IShopDialog;

            if (dialog != null)
                dialog.SellItem((int)message.objectToSellId, (int)message.quantity);
        }

        //[WorldHandler(ExchangeShowVendorTaxMessage.Id)]
        //public static void HandleExchangeShowVendorTaxMessage(WorldClient client, ExchangeShowVendorTaxMessage message)
        //{
        //    const int objectValue = 0;
        //    var totalTax = client.Character.MerchantBag.GetMerchantTax();

        //    if (totalTax <= 0)
        //        totalTax = 1;

        //    client.Send(new ExchangeReplyTaxVendorMessage(objectValue, (ulong)totalTax));
        //}

        //[WorldHandler(ExchangeRequestOnShopStockMessage.Id)]
        //public static void HandleExchangeRequestOnShopStockMessage(WorldClient client, ExchangeRequestOnShopStockMessage message)
        //{
        //    if (client.Character.IsBusy())
        //        return;

        //    var exchange = new MerchantExchange(client.Character);
        //    exchange.Open();
        //}

        [WorldHandler(ExchangeObjectMovePricedMessage.Id)]
        public static void HandleExchangeObjectMovePricedMessage(WorldClient client, ExchangeObjectMovePricedMessage message)
        {
            if (message.price <= 0)
                return;

            if (message.quantity <= 0)
                return;

            if (!client.Character.IsInExchange())
                return;

            //Restrição Staff Hydra By:Kenshin
            if (client.Character.UserGroup.Role >= RoleEnum.Moderator_Helper && client.Character.UserGroup.Role <= RoleEnum.GameMaster || client.Character.Invisible)
            {
                #region Menssagem Infor
                switch (client.Character.Account.Lang)
                {
                    case "fr":
                        client.Character.SendServerMessage("Vous n'êtes pas autorisé à utiliser cette fonction. Vérifiez vos droits auprès de STAFF.", Color.Red);
                        break;
                    case "es":
                        client.Character.SendServerMessage("No está autorizado a utilizar esta función. Consulta tus derechos con STAFF.", Color.Red);
                        break;
                    case "en":
                        client.Character.SendServerMessage("You are not allowed to use this function. Check your rights with STAFF.", Color.Red);
                        break;
                    default:
                        client.Character.SendServerMessage("Você não tem permissão para usar essa função. Consulte seus direitos com a STAFF.", Color.Red);
                        break;
                }
                #endregion

                #region MongoDB Logs Staff
                var document = new BsonDocument
                    {
                        { "AccountId", client.Account.Id },
                        { "AccountName", client.Account.Login },
                        { "CharacterId", client.Character.Id },
                        { "CharacterName", client.Character.Namedefault},
                        { "AbuseReason", "Exchange Object Move Priced"},
                        { "IPAddress", client.Character.Client.IP },
                        { "ClientKey", client.Character.Client.Account.LastHardwareId },
                        { "Date", DateTime.Now.ToString(CultureInfo.CreateSpecificCulture("pt-BR")) }
                    };

                MongoLogger.Instance.Insert("Staff_AbuseRights", document);
                #endregion

                return;
            }
            //Restrição Staff Hydra By:Kenshin
            else if (client.Character.UserGroup.Role >= RoleEnum.Administrator && client.Character.UserGroup.Role <= RoleEnum.Non_ADM && (uint)message.price <= 8000)
            {
                #region Menssagem Infor
                switch (client.Character.Account.Lang)
                {
                    case "fr":
                        client.Character.SendServerMessage("Vous n'êtes pas autorisé à utiliser cette fonction. Vérifiez vos droits auprès de STAFF.", Color.Red);
                        break;
                    case "es":
                        client.Character.SendServerMessage("No está autorizado a utilizar esta función. Consulta tus derechos con STAFF.", Color.Red);
                        break;
                    case "en":
                        client.Character.SendServerMessage("You are not allowed to use this function. Check your rights with STAFF.", Color.Red);
                        break;
                    default:
                        client.Character.SendServerMessage("Você não tem permissão para usar essa função. Consulte seus direitos com a STAFF.", Color.Red);
                        break;
                }
                #endregion

                #region MongoDB Logs Staff
                var document = new BsonDocument
                    {
                        { "AccountId", client.Account.Id },
                        { "AccountName", client.Account.Login },
                        { "CharacterId", client.Character.Id },
                        { "CharacterName", client.Character.Namedefault},
                        { "AbuseReason", "Exchange Object Move Priced"},
                        { "IPAddress", client.Character.Client.IP },
                        { "ClientKey", client.Character.Client.Account.LastHardwareId },
                        { "Date", DateTime.Now.ToString(CultureInfo.CreateSpecificCulture("pt-BR")) }
                    };

                MongoLogger.Instance.Insert("Staff_AbuseRights", document);
                #endregion

                return;
            }
            else
            {
                if (client.Character.Exchanger is CharacterMerchant)
                {
                    ((CharacterMerchant)client.Character.Exchanger).MovePricedItem((int)message.objectUID, message.quantity, (uint)message.price);
                }
                else if (client.Character.Exchanger is BidHouseExchanger)
                {
                    ((BidHouseExchanger)client.Character.Exchanger).MovePricedItem(message.objectUID, message.quantity, (long)message.price);
                }
            }
        }

        [WorldHandler(ExchangeObjectModifyPricedMessage.Id)]
        public static void HandleExchangeObjectModifyPricedMessage(WorldClient client, ExchangeObjectModifyPricedMessage message)
        {
            if (!client.Character.IsInExchange())
            {
                return;
            }

            if (client.Character.Exchanger is CharacterMerchant)
            {
                ((CharacterMerchant)client.Character.Exchanger).ModifyItem((int)message.objectUID, message.quantity, (uint)message.price);
            }
            else if (client.Character.Exchanger is BidHouseExchanger)
            {
                if (message.price <= 0)
                    return;

                ((BidHouseExchanger)client.Character.Exchanger).ModifyItem((uint)message.objectUID, (uint)message.price);
            }
        }

        //[WorldHandler(ExchangeStartAsVendorMessage.Id)]
        //public static void HandleExchangeStartAsVendorMessage(WorldClient client, ExchangeStartAsVendorMessage message)
        //{
        //    client.Character.EnableMerchantMode();
        //}

        //[WorldHandler(ExchangeOnHumanVendorRequestMessage.Id)]
        //public static void HandleExchangeOnHumanVendorRequestMessage(WorldClient client, ExchangeOnHumanVendorRequestMessage message)
        //{
        //    var merchant = client.Character.Map.GetActor<Merchant>((int)message.humanVendorId);

        //    if (merchant == null || merchant.Cell.Id != message.humanVendorCell)
        //        return;

        //    var shop = new MerchantShopDialog(merchant, client.Character);
        //    shop.Open();
        //}

        [WorldHandler(ExchangeRequestOnTaxCollectorMessage.Id)]
        public static void HandleExchangeRequestOnTaxCollectorMessage(WorldClient client, ExchangeRequestOnTaxCollectorMessage message)
        {
            if (client.Character.Guild == null)
                return;

            if (client.Character.IsInFight())
                return;

            var taxCollectorNpc = client.Character.Map.TaxCollector;

            if (taxCollectorNpc == null)
                return;

            var guildMember = client.Character.GuildMember;

            if (!taxCollectorNpc.IsTaxCollectorOwner(guildMember))
            {
                client.Send(new TaxCollectorErrorMessage((sbyte)TaxCollectorErrorReasonEnum.TAX_COLLECTOR_NOT_OWNED));
                return;
            }

            if (!((string.Equals(taxCollectorNpc.Record.CallerName, client.Character.Name) && guildMember.HasRight(GuildRightsBitEnum.GUILD_RIGHT_COLLECT_MY_TAX_COLLECTOR)) || guildMember.HasRight(GuildRightsBitEnum.GUILD_RIGHT_COLLECT)))
            {
                client.Send(new TaxCollectorErrorMessage((sbyte)TaxCollectorErrorReasonEnum.TAX_COLLECTOR_NO_RIGHTS));
                return;
            }

            if (taxCollectorNpc.IsBusy())
                return;

            var exchange = new TaxCollectorExchange(taxCollectorNpc, client.Character);
            exchange.Open();
        }

        [WorldHandler(ExchangeHandleMountsMessage.Id)]
        public static void HandleExchangeHandleMountsMessage_temp(WorldClient client, ExchangeHandleMountsMessage message)
        {
            HandleExchangeHandleMountStableMessage(client, (IEnumerable<int>)message.ridesId, message.actionType);
        }

        public static void HandleExchangeHandleMountStableMessage(WorldClient client, IEnumerable<int> Rideid, sbyte ActionType)
        {
            if (!client.Character.IsInExchange())
                return;

            var exchanger = client.Character.Exchanger as PaddockExchanger;

            if (exchanger == null)
                return;

            bool _continueForEach = true;

            foreach (var rideId in Rideid)
            {
                switch ((StableExchangeActionsEnum)ActionType)
                {
                    case StableExchangeActionsEnum.EQUIP_TO_STABLE:
                        _continueForEach = exchanger.EquipToStable(rideId);
                        break;

                    case StableExchangeActionsEnum.STABLE_TO_EQUIP:
                        _continueForEach = exchanger.StableToEquip(rideId);
                        break;

                    case StableExchangeActionsEnum.STABLE_TO_INVENTORY:
                        _continueForEach = exchanger.StableToInventory(rideId);
                        break;

                    case StableExchangeActionsEnum.INVENTORY_TO_STABLE:
                        _continueForEach = exchanger.InventoryToStable(rideId);
                        break;

                    case StableExchangeActionsEnum.STABLE_TO_PADDOCK:
                        _continueForEach = exchanger.StableToPaddock(rideId);
                        break;

                    case StableExchangeActionsEnum.PADDOCK_TO_STABLE:
                        _continueForEach = exchanger.PaddockToStable(rideId);
                        break;

                    case StableExchangeActionsEnum.EQUIP_TO_PADDOCK:
                        _continueForEach = exchanger.EquipToPaddock(rideId);
                        break;

                    case StableExchangeActionsEnum.PADDOCK_TO_EQUIP:
                        _continueForEach = exchanger.PaddockToEquip(rideId);
                        break;

                    case StableExchangeActionsEnum.PADDOCK_TO_INVENTORY:
                        _continueForEach = exchanger.PaddockToInventory(rideId);
                        break;

                    case StableExchangeActionsEnum.INVENTORY_TO_PADDOCK:
                        _continueForEach = exchanger.InventoryToPaddock(rideId);
                        break;

                    case StableExchangeActionsEnum.EQUIP_TO_INVENTORY:
                        _continueForEach = exchanger.EquipToInventory(rideId);
                        break;

                    case StableExchangeActionsEnum.INVENTORY_TO_EQUIP:
                        _continueForEach = exchanger.InventoryToEquip(rideId);
                        break;
                }

                if (!_continueForEach)
                    break;
            }
        }

        [WorldHandler(ExchangeBidHouseTypeMessage.Id)]
        public static void HandleExchangeBidHouseTypeMessage(WorldClient client, ExchangeBidHouseTypeMessage message)
        {
            var exchange = client.Character.Exchange as BidHouseExchange;

            if (exchange == null)
                return;

            var items = BidHouseManager.Instance.GetBidHouseItems((ItemTypeEnum)message.type, exchange.MaxItemLevel).ToArray();

            SendExchangeTypesExchangerDescriptionForUserMessage(client, (int)message.type, items.Select(x => x.Template.Id));
        }

        [WorldHandler(ExchangeBidHouseListMessage.Id)]
        public static void HandleExchangeBidHouseListMessage(WorldClient client, ExchangeBidHouseListMessage message)
        {
            var exchange = client.Character.Exchange as BidHouseExchange;
            if (exchange == null)
                return;

            exchange.UpdateCurrentViewedItem((int)message.objectGID);
        }

        [WorldHandler(ExchangeBidHousePriceMessage.Id)]
        public static void HandleExchangeBidHousePriceMessage(WorldClient client, ExchangeBidHousePriceMessage message)
        {
            if (!client.Character.IsInExchange())
            {
                return;
            }

            var averagePrice = BidHouseManager.Instance.GetAveragePriceForItem((int)message.objectGID);

            SendExchangeBidPriceMessage(client, (int)message.objectGID, averagePrice);
            SendExchangeBidPriceForSellerMessage(client, (ushort)message.objectGID, BidHouseManager.Instance.GetAveragePriceForItem((int)message.objectGID), true, BidHouseManager.Instance.GetMinimalPricesForItem((int)message.objectGID));
        }

        [WorldHandler(ExchangeBidHouseSearchMessage.Id)]
        public static void HandleExchangeBidHouseSearchMessage(WorldClient client, ExchangeBidHouseSearchMessage message)
        {
            var exchange = client.Character.Exchange as BidHouseExchange;

            if (exchange == null)
            {
                return;
            }

            var items = ItemManager.Instance.GetTemplates();

            if (items.All(x => x.Id != message.objectGID))
            {
                SendExchangeErrorMessage(client, ExchangeErrorEnum.BID_SEARCH_ERROR);
                return;
            }

            var categories = BidHouseManager.Instance.GetBidHouseCategories((int)message.objectGID, exchange.MaxItemLevel).Select(x => x.GetBidExchangerObjectInfo()).ToArray();
            var item = ItemManager.Instance.TryGetTemplate((int)message.objectGID);

            if (!categories.Any())
            {
                SendExchangeErrorMessage(client, ExchangeErrorEnum.BID_SEARCH_ERROR);
                return;
            }

            SendExchangeTypesItemsExchangerDescriptionForUserMessage(client, (uint)item.Id, categories, (int)item.TypeId);
        }

        [WorldHandler(ExchangeBidHouseBuyMessage.Id)]
        public static void HandleExchangeBidHouseBuyMessage(WorldClient client, ExchangeBidHouseBuyMessage message)
        {
            if (!client.Character.IsInExchange())
                return;

            if (client.Character.IsInFight())
                return;

            //Restrição Staff Hydra By:Kenshin
            if (client.Character.UserGroup.Role >= RoleEnum.Moderator_Helper && client.Character.UserGroup.Role <= RoleEnum.Administrator || client.Character.Invisible)
            {
                #region Menssagem Infor
                switch (client.Character.Account.Lang)
                {
                    case "fr":
                        client.Character.SendServerMessage("Vous n'êtes pas autorisé à utiliser cette fonction. Vérifiez vos droits auprès de STAFF.", Color.Red);
                        break;
                    case "es":
                        client.Character.SendServerMessage("No está autorizado a utilizar esta función. Consulta tus derechos con STAFF.", Color.Red);
                        break;
                    case "en":
                        client.Character.SendServerMessage("You are not allowed to use this function. Check your rights with STAFF.", Color.Red);
                        break;
                    default:
                        client.Character.SendServerMessage("Você não tem permissão para usar essa função. Consulte seus direitos com a STAFF.", Color.Red);
                        break;
                }
                #endregion

                #region MongoDB Logs Staff
                var document = new BsonDocument
                    {
                        { "AccountId", client.Account.Id },
                        { "AccountName", client.Account.Login },
                        { "CharacterId", client.Character.Id },
                        { "CharacterName", client.Character.Namedefault},
                        { "AbuseReason", "Exchange Bid House Buy"},
                        { "IPAddress", client.Character.Client.IP },
                        { "ClientKey", client.Character.Client.Account.LastHardwareId },
                        { "Date", DateTime.Now.ToString(CultureInfo.CreateSpecificCulture("pt-BR")) }
                    };

                MongoLogger.Instance.Insert("Staff_AbuseRights", document);
                #endregion

                return;
            }

            var category = BidHouseManager.Instance.GetBidHouseCategory((uint)message.uid);

            if (category == null)
                return;

            var item = category.GetItem((uint)message.qty, (int)message.price);

            if (item.Price > client.Character.Account.Tokens)
            {
                client.Character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 63); //Você não tem kamas suficientes para comprar este item.

                SendExchangeBidHouseBuyResultMessage(client, (int)message.uid, false);
                return;
            }

            if (item == null)
            {
                //Cet objet n'est plus disponible à ce prix. Quelqu'un a été plus rapide...
                client.Character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 64);

                SendExchangeBidHouseBuyResultMessage(client, (int)message.uid, false);
                return;
            }

            if (client.Character.Inventory.IsFull(item.Template, (int)item.Stack))
            {
                //Action annulée pour cause de surcharge...
                client.Character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 70);

                SendExchangeBidHouseBuyResultMessage(client, (int)message.uid, false);
                return;
            }

            bool result = false;

            if (client.Character.Inventory.RemoveTokenItem((int)item.Price, "BidHouseBuyItem: " + item.Template.Name))
            {
                result = client.Character.Exchanger.MoveItem(item.Guid, (int)item.Stack);

                if (result)
                {
                    item.SellItem(client.Character);
                    client.Character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 144, item.Template.Id, item.Guid, item.Stack, item.Price); //%3 x {item,%1,%2} (%4 Ogrines)
                    SendExchangeBidHouseBuyResultMessage(client, item.Guid, result);
                }
                else
                {
                    client.Character.Inventory.CreateTokenItem((int)item.Price, "BidHouseBuyItemError: " + item.Template.Name);
                    SendExchangeBidHouseBuyResultMessage(client, (int)message.uid, false);
                    return;
                }
            }
            else
            {
                return;
            }

            //BIDHOUSE Kamas (DESATIVADO)
            //if (result)
            //    client.Character.Inventory.SubKamas((int)item.Price);

            //BIDHOUSE Kamas (DESATIVADO)
            //%3 x {item,%1,%2} (%4 kamas)
            //client.Character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 252, item.Template.Id, item.Guid, item.Stack, item.Price);
        }

        [WorldHandler(ExchangeCraftCountRequestMessage.Id)]
        public static void HandleExchangeCraftCountRequestMessage(WorldClient client, ExchangeCraftCountRequestMessage message)
        {
            (client.Character.Dialog as BaseCraftDialog)?.ChangeAmount(message.count);
        }

        [WorldHandler(ExchangeSetCraftRecipeMessage.Id)]
        public static void HandleExchangeSetCraftRecipeMessage(WorldClient client, ExchangeSetCraftRecipeMessage message)
        {
            if (!JobManager.Instance.Recipes.ContainsKey((int)message.objectGID))
                return;

            var craftActor = client.Character.Dialoger as CraftingActor;

            if (craftActor == null)
                return;

            var dialog = craftActor.CraftDialog as CraftDialog;

            if (dialog == null)
                return;

            var recipe = JobManager.Instance.Recipes[(int)message.objectGID];
            dialog.ChangeRecipe(craftActor, recipe);
        }

        [WorldHandler(JobCrafterDirectoryListRequestMessage.Id)]
        public static void HandleJobCrafterDirectoryListRequestMessage(WorldClient client, JobCrafterDirectoryListRequestMessage message)
        {
            (client.Character.Dialog as JobIndexDialog)?.RequestAvailableCrafters(message.jobId);
        }

        //Modificação Feita por [DEV]Kenshin
        //Banco: Transferir todos os objetos do inventario para o Banco.
        [WorldHandler(ExchangeObjectTransfertAllFromInvMessage.Id)]
        public static void HandleExchangeObjectTransfertAllFromInvMessage(WorldClient client, ExchangeObjectTransfertAllFromInvMessage message)
        {
            var bank = client.Character.Dialog as BankDialog;
            var mount = client.Character.Dialog as MountDialog;
            var trade = client.Character.Dialog as PlayerTrade;
            var tradeNpcDelete = client.Character.Dialog as NpcDelete;

            if (bank != null)
            {
                bank.Customer.MoveItems(true, new uint[0], true, false);
            }
            else if (mount != null)
            {
                mount.Customer.MoveItems(true, new uint[0], true, false);
            }
            else if (trade != null)
            {
                if (client.Character == trade.FirstTrader.Character)
                {
                    foreach (var item in trade.FirstTrader.Character.Inventory.Where(x => !x.IsTokenItem() && !x.IsEquiped() && !x.IsLinkedToPlayer() && !x.IsLinkedToAccount()))
                    {
                        trade.FirstTrader.MoveItemToPanel(item, (int)item.Stack);
                    }
                }
                else
                {
                    foreach (var item in trade.SecondTrader.Character.Inventory.Where(x => !x.IsTokenItem() && !x.IsEquiped() && !x.IsLinkedToPlayer() && !x.IsLinkedToAccount()))
                    {
                        trade.SecondTrader.MoveItemToPanel(item, (int)item.Stack);
                    }
                }
            }
            else if (tradeNpcDelete != null)
            {
                if (client.Character == tradeNpcDelete.FirstTrader.Character)
                {
                    foreach (var item in tradeNpcDelete.FirstTrader.Character.Inventory.Where(x => !x.IsTokenItem() && !x.IsEquiped() && !x.IsLinkedToPlayer() && !x.IsLinkedToAccount()))
                    {
                        tradeNpcDelete.FirstTrader.MoveItemToPanel(item, (int)item.Stack);
                    }
                }
            }
            else
            {
                return;
            }
        }

        //Modificação Feita por [DEV]Kenshin
        //Banco: Transferir todos os objetos visiveis do inventario para o Banco.
        [WorldHandler(ExchangeObjectTransfertListFromInvMessage.Id)]
        public static void HandleExchangeObjectTransfertListFromInvMessage(WorldClient client, ExchangeObjectTransfertListFromInvMessage message)
        {
            var bank = client.Character.Dialog as BankDialog;
            var mount = client.Character.Dialog as MountDialog;
            var trade = client.Character.Dialog as PlayerTrade;
            var tradeNpcDelete = client.Character.Dialog as NpcDelete;

            if (bank != null)
            {
                bank.Customer.MoveItems(true, message.ids, false, false);
            }
            else if (mount != null)
            {
                mount.Customer.MoveItems(true, message.ids, false, false);
            }
            else if (trade != null)
            {
                if (client.Character == trade.FirstTrader.Character)
                {
                    foreach (var id in message.ids)
                    {
                        var item = client.Character.Inventory[(int)id];
                        trade.FirstTrader.MoveItemToPanel(item, (int)item.Stack);
                    }
                }
                else
                {
                    foreach (var id in message.ids)
                    {
                        var item = client.Character.Inventory[(int)id];
                        trade.SecondTrader.MoveItemToPanel(item, (int)item.Stack);
                    }
                }
            }
            else if (tradeNpcDelete != null)
            {
                if (client.Character == tradeNpcDelete.FirstTrader.Character)
                {
                    foreach (var id in message.ids)
                    {
                        var item = client.Character.Inventory[(int)id];
                        tradeNpcDelete.FirstTrader.MoveItemToPanel(item, (int)item.Stack);
                    }
                }
            }
            else
            {
                return;
            }
        }

        //Modificação Feita por [DEV]Kenshin - TODO
        //Banco: Transferir todos os objetos do Inventario existentes no Banco.
        [WorldHandler(ExchangeObjectTransfertExistingFromInvMessage.Id)]
        public static void HandleExchangeObjectTransfertExistingFromInvMessage(WorldClient client, ExchangeObjectTransfertExistingFromInvMessage message)
        {
            var bank = client.Character.Dialog as BankDialog;
            var mount = client.Character.Dialog as MountDialog;
            var tradeNpcDelete = client.Character.Dialog as NpcDelete;

            if (bank != null)
            {
                bank.Customer.MoveItems(true, new uint[0], false, true);
            }
            else if (mount != null)
            {
                mount.Customer.MoveItems(true, new uint[0], false, true);
            }
            else if (tradeNpcDelete != null)
            {
                if (client.Character == tradeNpcDelete.FirstTrader.Character)
                {
                    foreach (var item in tradeNpcDelete.FirstTrader.Character.Inventory.Where(x => !x.IsTokenItem() && !x.IsEquiped() && !x.IsLinkedToPlayer() && !x.IsLinkedToAccount() && tradeNpcDelete.FirstTrader.Items.Any(entry => entry.Template.Id == x.Template.Id)))
                    {
                        tradeNpcDelete.FirstTrader.MoveItemToPanel(item, (int)item.Stack);
                    }
                }
            }
            else
            {
                return;
            }
        }

        //Criação Feita pelo [DEV]Kenshin
        //Banco : Opção de Transferir todos os itens para o inventario
        [WorldHandler(ExchangeObjectTransfertAllToInvMessage.Id)]
        public static void HandleExchangeObjectTransfertAllToInvMessage(WorldClient client, ExchangeObjectTransfertAllToInvMessage message)
        {
            var bank = client.Character.Dialog as BankDialog;
            var mount = client.Character.Dialog as MountDialog;
            var tradeNpcDelete = client.Character.Dialog as NpcDelete;

            if (bank != null)
            {
                bank.Customer.MoveItems(false, new uint[0], true, false);
            }
            else if (mount != null)
            {
                mount.Customer.MoveItems(false, new uint[0], true, false);
            }
            else if (tradeNpcDelete != null)
            {
                foreach (var item in tradeNpcDelete.FirstTrader.Items.OfType<PlayerTradeItem>().ToArray())
                {
                    tradeNpcDelete.FirstTrader.MoveItemToInventory(item, 0);
                }
            }
            else
            {
                return;
            }
        }

        //Banco: Usado para transferir objetos visiveis para o inventario
        [WorldHandler(ExchangeObjectTransfertListToInvMessage.Id)]
        public static void HandleExchangeObjectTransfertListToInvMessage(WorldClient client, ExchangeObjectTransfertListToInvMessage message)
        {
            var bank = client.Character.Dialog as BankDialog;
            var mount = client.Character.Dialog as MountDialog;

            if (bank != null)
            {
                bank.Customer.MoveItems(false, message.ids, false, false);
            }
            else if (mount != null)
            {
                mount.Customer.MoveItems(false, message.ids, false, false);
            }
            else
            {
                return;
            }
        }

        //Banco: Transferir todos os objetos do Banco existentes no Inventario.
        [WorldHandler(ExchangeObjectTransfertExistingToInvMessage.Id)]
        public static void HandleExchangeObjectTransfertExistingToInvMessage(WorldClient client, ExchangeObjectTransfertExistingToInvMessage message)
        {
            var bank = client.Character.Dialog as BankDialog;
            var mount = client.Character.Dialog as MountDialog;

            if (bank != null)
            {
                bank.Customer.MoveItems(false, new uint[0], false, true);
            }
            else if (mount != null)
            {
                mount.Customer.MoveItems(false, new uint[0], false, true);
            }
            else
            {
                return;
            }
        }

        [WorldHandler(ExchangeReplayStopMessage.Id)]
        public static void HandleExchangeReplayStopMessage(WorldClient client, ExchangeReplayStopMessage message)
        {
            (client.Character.Dialog as RuneMagicCraftDialog)?.StopAutoCraft();
        }

        [WorldHandler(ExchangeObjectUseInWorkshopMessage.Id)]
        public static void HandleExchangeObjectUseInWorkshopMessage(WorldClient client, ExchangeObjectUseInWorkshopMessage message)
        {
            (client.Character.Dialog as MultiRuneMagicCraftDialog)?.MoveItemFromBag((int)message.objectUID, message.quantity);
        }

        [WorldHandler(ExchangeRequestOnMountStockMessage.Id)]
        public static void HandleExchangeRequestOnMountStockMessage(WorldClient client, ExchangeRequestOnMountStockMessage message)
        {
            if (client.Character.HasEquippedMount())
            {
                var exchange = new MountDialog(client.Character);

                exchange.Open();
            }
        }

        public static void SendExchangeStartedMountStockMessage(IPacketReceiver client, MountInventory inventory)
        {
            if (inventory != null)
                client.Send(new ExchangeStartedMountStockMessage(inventory.Select(x => x.GetObjectItem()).ToArray()));
        }

        public static void SendExchangeRequestedTradeMessage(IPacketReceiver client, ExchangeTypeEnum type, Character source, Character target)
        {
            client.Send(new ExchangeRequestedTradeMessage(
                            (sbyte)type,
                            (ulong)source.Id,
                            (ulong)target.Id));
        }

        public static void SendExchangeStartedWithPodsMessage(IPacketReceiver client, PlayerTrade playerTrade)
        {
            client.Send(new ExchangeStartedWithPodsMessage(
                            (sbyte)ExchangeTypeEnum.PLAYER_TRADE,
                            playerTrade.FirstTrader.Character.Id,
                            (uint)playerTrade.FirstTrader.Character.Inventory.Weight,
                            (uint)playerTrade.FirstTrader.Character.Inventory.WeightTotal,
                            playerTrade.SecondTrader.Character.Id,
                            (uint)playerTrade.SecondTrader.Character.Inventory.Weight,
                            (uint)playerTrade.SecondTrader.Character.Inventory.WeightTotal
                            ));
        }

        public static void SendExchangeStartedWithStorageMessage(IPacketReceiver client, ExchangeTypeEnum type, int storageMaxSlot)
        {
            client.Send(new ExchangeStartedWithStorageMessage((sbyte)type, (uint)storageMaxSlot));
        }

        public static void SendExchangeStartedMessage(IPacketReceiver client, ExchangeTypeEnum type)
        {
            client.Send(new ExchangeStartedMessage((sbyte)type));
        }

        public static void SendExchangeStartedTaxCollectorShopMessage(IPacketReceiver client, TaxCollectorNpc taxCollector)
        {
            client.Send(new ExchangeStartedTaxCollectorShopMessage(taxCollector.Bag.Select(x => x.GetObjectItem()).ToArray(), (ulong)taxCollector.GatheredKamas));
        }

        public static void SendExchangeStartOkHumanVendorMessage(IPacketReceiver client, Merchant merchant)
        {
            //client.Send(new ExchangeStartOkHumanVendorMessage(merchant.Id, merchant.Bag.Where(x => x.Stack > 0).Select(x => x.GetObjectItemToSellInHumanVendorShop()).ToArray()));
        }

        public static void SendExchangeStartOkNpcTradeMessage(IPacketReceiver client, NpcTrade trade)
        {
            client.Send(new ExchangeStartOkNpcTradeMessage(trade.SecondTrader.Npc.Id, true));
        }

        public static void SendExchangeStartOkNpcDeleteMessage(IPacketReceiver client, NpcDelete trade)
        {
            client.Send(new ExchangeStartOkNpcTradeMessage(trade.SecondTrader.Npc.Id, false));
        }

        public static void SendExchangeStartOkNpcShopMessage(IPacketReceiver client, NpcShopDialog dialog)
        {
            client.Send(new ExchangeStartOkNpcShopMessage(dialog.Npc.Id, dialog.Token != null ? (ushort)dialog.Token.Id : (ushort)0, dialog.Items.Select(entry => entry.GetNetworkItem() as ObjectItemToSellInNpcShop)));
        }

        public static void SendExchangeLeaveMessage(IPacketReceiver client, DialogTypeEnum dialogType, bool success)
        {
            client.Send(new ExchangeLeaveMessage((sbyte)dialogType, success));
        }

        public static void SendExchangeObjectAddedMessage(IPacketReceiver client, bool remote, TradeItem item)
        {
            client.Send(new ExchangeObjectAddedMessage(remote, item.GetObjectItem()));
            client.Send(new ObjectAveragePricesMessage(new uint[] { item.GetObjectItem().objectGID }, new ulong[] { PriceFormulas.getItemPrice(item.Template.Id) }));
        }

        public static void SendExchangeObjectModifiedMessage(IPacketReceiver client, bool remote, TradeItem item)
        {
            client.Send(new ExchangeObjectModifiedMessage(remote, item.GetObjectItem()));
        }

        public static void SendExchangeObjectRemovedMessage(IPacketReceiver client, bool remote, int guid)
        {
            client.Send(new ExchangeObjectRemovedMessage(remote, (uint)guid));
        }

        public static void SendExchangeIsReadyMessage(IPacketReceiver client, Trader trader, bool ready)
        {
            client.Send(new ExchangeIsReadyMessage(trader.Id, ready));
        }

        public static void SendExchangeErrorMessage(IPacketReceiver client, ExchangeErrorEnum errorEnum)
        {
            client.Send(new ExchangeErrorMessage((sbyte)errorEnum));
        }

        public static void SendExchangeShopStockStartedMessage(IPacketReceiver client, CharacterMerchantBag merchantBag)
        {
            //client.Send(new ExchangeShopStockStartedMessage(merchantBag.Where(x => x.Stack > 0).Select(x => x.GetObjectItemToSell())));
        }

        public static void SendExchangeStartOkMountMessage(IPacketReceiver client, IEnumerable<Mount> stabledMounts, IEnumerable<Mount> paddockedMounts)
        {
            client.Send(new ExchangeStartOkMountMessage(stabledMounts.Select(x => x.GetMountClientData()).ToArray(), paddockedMounts.Select(x => x.GetMountClientData()).ToArray()));
        }

        public static void SendExchangeMountPaddockAddMessage(IPacketReceiver client, Mount mount)
        {
            client.Send(new ExchangeMountsPaddockAddMessage(new[] { mount.GetMountClientData() }));
        }

        public static void SendExchangeMountStableAddMessage(IPacketReceiver client, Mount mount)
        {
            client.Send(new ExchangeMountsStableAddMessage(new[] { mount.GetMountClientData() }));
        }

        public static void SendExchangeMountPaddockRemoveMessage(IPacketReceiver client, Mount mount)
        {
            client.Send(new ExchangeMountsPaddockRemoveMessage(new[] { mount.Id }));
        }

        public static void SendExchangeMountStableRemoveMessage(IPacketReceiver client, Mount mount)
        {
            client.Send(new ExchangeMountsStableRemoveMessage(new[] { mount.Id }));
        }

        public static void SendExchangeStartedBidBuyerMessage(IPacketReceiver client, BidHouseExchange exchange)
        {
            client.Send(new ExchangeStartedBidBuyerMessage(exchange.GetBuyerDescriptor()));
        }

        public static void SendExchangeStartedBidSellerMessage(IPacketReceiver client, BidHouseExchange exchange, IEnumerable<ObjectItemToSellInBid> items)
        {
            client.Send(new ExchangeStartedBidSellerMessage(exchange.GetBuyerDescriptor(), items));
        }

        public static void SendExchangeTypesExchangerDescriptionForUserMessage(IPacketReceiver client, int type, IEnumerable<int> items)
        {
            client.Send(new ExchangeTypesExchangerDescriptionForUserMessage(type, items.ToArray().Select(x => (uint)x)));
        }

        public static void SendExchangeTypesItemsExchangerDescriptionForUserMessage(IPacketReceiver client, uint itemId, IEnumerable<BidExchangerObjectInfo> items, int objectType)
        {
            client.Send(new ExchangeTypesItemsExchangerDescriptionForUserMessage(itemId, objectType, items.ToArray()));
        }

        public static void SendExchangeBidPriceMessage(IPacketReceiver client, int itemId, long averagePrice)
        {
            client.Send(new ExchangeBidPriceMessage((ushort)itemId, averagePrice));
        }

        public static void SendExchangeBidPriceForSellerMessage(IPacketReceiver client, ushort itemId, long average, bool allIdentical, IEnumerable<ulong> prices)
        {
            client.Send(new ExchangeBidPriceForSellerMessage(itemId, average, allIdentical, prices.ToArray()));
        }

        public static void SendExchangeBidHouseItemAddOkMessage(IPacketReceiver client, ObjectItemToSellInBid item)
        {
            client.Send(new ExchangeBidHouseItemAddOkMessage(item));
        }

        public static void SendExchangeBidHouseItemRemoveOkMessage(IPacketReceiver client, int sellerId)
        {
            client.Send(new ExchangeBidHouseItemRemoveOkMessage(sellerId));
        }

        public static void SendExchangeBidHouseBuyResultMessage(IPacketReceiver client, int guid, bool bought)
        {
            client.Send(new ExchangeBidHouseBuyResultMessage((uint)guid, bought));
        }

        public static void SendExchangeBidHouseInListAddedMessage(IPacketReceiver client, BidHouseCategory category)
        {
            client.Send(new ExchangeBidHouseInListAddedMessage(category.Id, (ushort)category.TemplateId, (int)category.ItemType, category.Effects.Select(x => x.GetObjectEffect()), category.GetPrices()));
        }

        public static void SendExchangeBidHouseInListRemovedMessage(IPacketReceiver client, BidHouseCategory category)
        {
            client.Send(new ExchangeBidHouseInListRemovedMessage(category.Id, (ushort)category.TemplateId, (int)category.ItemType));
        }

        public static void SendExchangeBidHouseInListUpdatedMessage(IPacketReceiver client, BidHouseCategory category)
        {
            client.Send(new ExchangeBidHouseInListUpdatedMessage(category.Id, (ushort)category.TemplateId, (int)category.ItemType, category.Effects.Select(x => x.GetObjectEffect()), category.GetPrices()));
        }

        public static void SendExchangeBidHouseGenericItemAddedMessage(IPacketReceiver client, BidHouseItem item)
        {
            client.Send(new ExchangeBidHouseGenericItemAddedMessage((ushort)item.Template.Id));
        }

        public static void SendExchangeBidHouseGenericItemRemovedMessage(IPacketReceiver client, BidHouseItem item)
        {
            client.Send(new ExchangeBidHouseGenericItemRemovedMessage((ushort)item.Template.Id));
        }

        public static void SendExchangeOfflineSoldItemsMessage(IPacketReceiver client, ObjectItemQuantityPriceDateEffects[] merchantItems, ObjectItemQuantityPriceDateEffects[] bidHouseItems)
        {
            client.Send(new ExchangeOfflineSoldItemsMessage(bidHouseItems)); //merchantItems));
        }

        public static void SendExchangeStartOkCraftWithInformationMessage(IPacketReceiver client, Skill skill)
        {
            client.Send(new ExchangeStartOkCraftWithInformationMessage((uint)skill.SkillTemplate.Id));
        }

        public static void SendExchangeCraftCountModifiedMessage(IPacketReceiver client, int amount)
        {
            client.Send(new ExchangeCraftCountModifiedMessage(amount));
        }

        public static void SendExchangeCraftResultMessage(IPacketReceiver client, ExchangeCraftResultEnum result)
        {
            client.Send(new ExchangeCraftResultMessage((sbyte)result));
        }

        public static void SendExchangeCraftResultWithObjectIdMessage(IPacketReceiver client, ExchangeCraftResultEnum result, ItemTemplate item)
        {
            client.Send(new ExchangeCraftResultWithObjectIdMessage((sbyte)result, (ushort)item.Id));
        }

        public static void SendExchangeCraftResultWithObjectDescMessage(IPacketReceiver client, ExchangeCraftResultEnum result, BasePlayerItem createdItem, int amount)
        {
            client.Send(new ExchangeCraftResultWithObjectDescMessage((sbyte)result, new ObjectItemNotInContainer((ushort)createdItem.Template.Id, createdItem.Effects.Select(x => x.GetObjectEffect()), (uint)createdItem.Guid, (uint)amount)));
        }

        //Version 2.61 by Kenshin
        public static void SendExchangeCraftInformationObjectMessage(IPacketReceiver client, IItem item, ExchangeCraftResultEnum result)
        {
            client.Send(new ExchangeCraftResultWithObjectIdMessage((sbyte)result, (ushort)item.Template.Id));
        }

        public static void SendExchangeOkMultiCraftMessage(IPacketReceiver client, Character initiator, Character other, ExchangeTypeEnum role)
        {
            client.Send(new ExchangeOkMultiCraftMessage((ulong)initiator.Id, (ulong)other.Id, (sbyte)role));
        }

        public static void SendExchangeStartOkMulticraftCrafterMessage(IPacketReceiver client, InteractiveSkillTemplate skillTemplate)
        {
            client.Send(new ExchangeStartOkMulticraftCrafterMessage((uint)skillTemplate.Id));
        }

        public static void SendExchangeStartOkMulticraftCustomerMessage(IPacketReceiver client, InteractiveSkillTemplate skillTemplate, Job job)
        {
            client.Send(new ExchangeStartOkMulticraftCustomerMessage((uint)skillTemplate.Id, (byte)job.Level));
        }

        public static void SendExchangeCraftPaymentModifiedMessage(IPacketReceiver client, long kamas)
        {
            client.Send(new ExchangeCraftPaymentModifiedMessage((ulong)kamas));
        }

        public static void SendExchangeStartOkJobIndexMessage(IPacketReceiver client, IEnumerable<JobTemplate> jobs)
        {
            client.Send(new ExchangeStartOkJobIndexMessage(jobs.Select(x => (uint)x.Id)));
        }

        public static void SendJobCrafterDirectoryListMessage(IPacketReceiver client, IEnumerable<Job> entries)
        {
            client.Send(new JobCrafterDirectoryListMessage(entries.Select(x => x.GetJobCrafterDirectoryListEntry())));
        }

        public static void SendJobCrafterDirectoryAddMessage(IPacketReceiver client, Job entry)
        {
            client.Send(new JobCrafterDirectoryAddMessage(entry.GetJobCrafterDirectoryListEntry()));
        }

        public static void SendJobCrafterDirectoryRemoveMessage(IPacketReceiver client, Job entry)
        {
            client.Send(new JobCrafterDirectoryRemoveMessage((sbyte)entry.Template.Id, (ulong)entry.Owner.Id));
        }

        public static void SendExchangeCraftResultMagicWithObjectDescMessage(IPacketReceiver client, CraftResultEnum craftResult, IItem item, IEnumerable<EffectBase> effects, MagicPoolStatus poolStatus)
        {
            client.Send(new ExchangeCraftResultMagicWithObjectDescMessage((sbyte)craftResult, new ObjectItemNotInContainer((ushort)item.Template.Id, effects.Select(x => x.GetObjectEffect()), (uint)item.Guid, (uint)item.Stack), (sbyte)poolStatus));
        }

        public static void SendExchangeItemAutoCraftStopedMessage(IPacketReceiver client, ExchangeReplayStopReasonEnum reason)
        {
            client.Send(new ExchangeItemAutoCraftStopedMessage((sbyte)reason));
        }

        public static void SendExchangeStartOkRunesTradeMessage(IPacketReceiver client)
        {
            client.Send(new ExchangeStartOkRunesTradeMessage());
        }

        public static void SendDecraftResultMessage(IPacketReceiver client, IEnumerable<DecraftedItemStackInfo> itemsInfo)
        {
            client.Send(new DecraftResultMessage(itemsInfo));
        }

        public static void SendExchangeStartPaddockBuySell(IPacketReceiver client, bool bsell, uint ownerId, ulong price)
        {
            client.Send(new PaddockSellBuyDialogMessage(bsell, ownerId, price));
        }
    }
}