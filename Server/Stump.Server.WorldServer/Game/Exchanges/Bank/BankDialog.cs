using MongoDB.Bson;
using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Logging;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Handlers.Inventory;
using System;
using System.Globalization;
using System.Linq;

namespace Stump.Server.WorldServer.Game.Exchanges.Bank
{
    public class BankDialog : IExchange
    {
        public BankDialog(Character character)
        {
            Customer = new BankCustomer(character, this);
        }

        public Character Character => Customer.Character;

        public BankCustomer Customer
        {
            get;
        }

        public ExchangeTypeEnum ExchangeType => ExchangeTypeEnum.BANK;

        public DialogTypeEnum DialogType => DialogTypeEnum.DIALOG_EXCHANGE;

        //Modificações Criadas por Kenshin.
        public void Open()
        {
            int CostKamas = 0;
            CostKamas = Character.Bank.Count();
            CostKamas = CostKamas * 1;

            if (Character.Kamas > CostKamas)
            {
                Character.Inventory.SubKamas(CostKamas);
                OpenBank();
                Character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 46, CostKamas);
            }
            else if (Character.Bank.Kamas > CostKamas)
            {
                Character.Bank.SubKamas(CostKamas);
                OpenBank();
                Character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 46, CostKamas);
            }
            else 
            {
                Character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 128, CostKamas);
                Character.Dialog.Close();
            }

            #region // ----------------- Sistema de Logs MongoDB Uso do Banco by: Kenshin ---------------- //
            try
            {
                var document = new BsonDocument
                {
                    { "CharacterID", Character.Id },
                    { "CharacterName", Character.Name },
                    { "CharacterAccountId", Character.Account.Id },
                    { "CharacterAccountName", Character.Account.Login },
                    { "BancoCountItems", Character.Bank.Count() },
                    { "BancoCostUse", CostKamas },
                    { "Date", DateTime.Now.ToString(CultureInfo.CreateSpecificCulture("pt-BR")) }
                };

                MongoLogger.Instance.Insert("Npc_Banco", document);
            }
            catch (Exception e)
            {
                Console.WriteLine("Erro no Mongologs do Uso do Banco : " + e.Message);
            }
            #endregion

        }

        private void OpenBank()
        {
            InventoryHandler.SendExchangeStartedWithStorageMessage(Character.Client, ExchangeType, int.MaxValue);
            InventoryHandler.SendStorageInventoryContentMessage(Character.Client, Customer.Character.Bank);
            Character.SetDialoger(Customer);
        }

        public void Close()
        {
            InventoryHandler.SendExchangeLeaveMessage(Character.Client, DialogType, false);
            Character.ResetDialog();
        }
    }
}