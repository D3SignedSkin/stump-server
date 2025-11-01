using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using MongoDB.Bson;
using NLog;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Messages;
using Stump.Server.BaseServer.Logging;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Merchants;
using Stump.Server.WorldServer.Game.Items;
using Stump.Server.WorldServer.Game.Items.Player;
using Stump.Server.WorldServer.Handlers.Inventory;

namespace Stump.Server.WorldServer.Game.Dialogs.Merchants
{
    public class MerchantShopDialog : IShopDialog
    {
       // private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        public MerchantShopDialog(Merchant merchant, Character character)
        {
            Merchant = merchant;
            Character = character;
        }

        public Merchant Merchant
        {
            get;
        }

        public Character Character
        {
            get;
        }

        public DialogTypeEnum DialogType => DialogTypeEnum.DIALOG_EXCHANGE;

        public void Open()
        {
            Character.SetDialog(this);
            Merchant.OnDialogOpened(this);
            InventoryHandler.SendExchangeStartOkHumanVendorMessage(Character.Client, Merchant);
        }

        public void Close()
        {
            InventoryHandler.SendExchangeLeaveMessage(Character.Client, DialogType, false);
            Character.CloseDialog(this);
            Merchant.OnDialogClosed(this);
        }

        public bool BuyItem(int itemGuid, int quantity)
        {
            //Restricción de Hydra del personal Por:Kenshin
            if (Character.UserGroup.Role >= RoleEnum.Moderator_Helper && Character.UserGroup.Role <= RoleEnum.Administrator || Character.Invisible)
            {
                #region Menssagem Infor
                switch (Character.Account.Lang)
                {
                    case "fr":
                        Character.SendServerMessage("Vous n'êtes pas autorisé à utiliser cette fonction. Vérifiez vos droits auprès de STAFF.", Color.Red);
                        break;
                    case "es":
                        Character.SendServerMessage("No está autorizado a utilizar esta función. Consulta tus derechos con STAFF.", Color.Red);
                        break;
                    case "en":
                        Character.SendServerMessage("You are not allowed to use this function. Check your rights with STAFF.", Color.Red);
                        break;
                    default:
                        Character.SendServerMessage("Você não tem permissão para usar essa função. Consulte seus direitos com a STAFF.", Color.Red);
                        break;
                }
                #endregion

                #region MongoDB Logs Staff
                var document = new BsonDocument
                    {
                        { "AccountId", Character.Client.Account.Id },
                        { "AccountName", Character.Client.Account.Login },
                        { "CharacterId", Character.Id },
                        { "CharacterName", Character.Namedefault},
                        { "AbuseReason", "Buy Merchant"},
                        { "IPAddress", Character.Client.IP },
                        { "ClientKey", Character.Client.Account.LastHardwareId },
                        { "Date", DateTime.Now.ToString(CultureInfo.CreateSpecificCulture("pt-BR")) }
                    };

                MongoLogger.Instance.Insert("Staff_AbuseRights", document);
                #endregion
                return false;
            }

            if (Character.IsInFight())
                return false;

            var item = Merchant.Bag.FirstOrDefault(x => x.Guid == itemGuid);

            if (item == null || item.Stack <= 0 || quantity <= 0 || !CanBuy(item, quantity))
            {
                Character.Client.Send(new ExchangeErrorMessage((int)ExchangeErrorEnum.BUY_ERROR));
                return false;
            }

            if (Character.Inventory.IsFull(item.Template, quantity))
            {
                Character.Client.Send(new ExchangeErrorMessage((int)ExchangeErrorEnum.REQUEST_CHARACTER_OVERLOADED));
                return false;
            }

            ulong removed = (ulong)Merchant.Bag.RemoveItem(item, quantity);

            var newItem = ItemManager.Instance.CreatePlayerItem(Character, item.Template, (int)removed, item.Effects);
            Character.Inventory.AddItem(newItem);

            var finalPrice = item.Price * removed;
            Character.Inventory.SubKamas((long)finalPrice);

            //Vous avez perdu %1 kamas.
            Character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 46, finalPrice);

            Character.Client.Send(new ExchangeBuyOkMessage());

            Merchant.Save(MerchantManager.Instance.Database);
            //Npcs.NpcShopDialogLogger teste = null;
            //teste.BuyItem(itemGuid, quantity);
            
           // logger.Info("save from in queue {0}", "MerchantShopDialog");
            Character.SaveLater();

            return true;
        }

        public bool CanBuy(MerchantItem item, long amount) => Character.Inventory.Kamas >= (long)item.Price * amount || !Merchant.CanBeSee(Character);

        public bool SellItem(int id, int quantity)
        {
            //Restricción de Hydra del personal Por:Kenshin
            if (Character.UserGroup.Role >= RoleEnum.Moderator_Helper && Character.UserGroup.Role <= RoleEnum.Administrator || Character.Invisible)
            {
                #region Menssagem Infor
                switch (Character.Account.Lang)
                {
                    case "fr":
                        Character.SendServerMessage("Vous n'êtes pas autorisé à utiliser cette fonction. Vérifiez vos droits auprès de STAFF.", Color.Red);
                        break;
                    case "es":
                        Character.SendServerMessage("No está autorizado a utilizar esta función. Consulta tus derechos con STAFF.", Color.Red);
                        break;
                    case "en":
                        Character.SendServerMessage("You are not allowed to use this function. Check your rights with STAFF.", Color.Red);
                        break;
                    default:
                        Character.SendServerMessage("Você não tem permissão para usar essa função. Consulte seus direitos com a STAFF.", Color.Red);
                        break;
                }
                #endregion

                #region MongoDB Logs Staff
                var document = new BsonDocument
                    {
                        { "AccountId", Character.Client.Account.Id },
                        { "AccountName", Character.Client.Account.Login },
                        { "CharacterId", Character.Id },
                        { "CharacterName", Character.Namedefault},
                        { "AbuseReason", "Sell Merchant"},
                        { "IPAddress", Character.Client.IP },
                        { "ClientKey", Character.Client.Account.LastHardwareId },
                        { "Date", DateTime.Now.ToString(CultureInfo.CreateSpecificCulture("pt-BR")) }
                    };

                MongoLogger.Instance.Insert("Staff_AbuseRights", document);
                #endregion
                return false;
            }

            if (Character.IsInFight())
                return false;

            Character.Client.Send(new ExchangeErrorMessage((int)ExchangeErrorEnum.SELL_ERROR));
            return false;
        }
    }
}