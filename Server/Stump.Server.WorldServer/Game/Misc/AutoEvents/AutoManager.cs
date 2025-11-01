using MongoDB.Bson;
using Stump.Core.Attributes;
using Stump.Server.BaseServer.Database;
using Stump.Server.BaseServer.Initialization;
using Stump.Server.BaseServer.Logging;
using Stump.Server.WorldServer.Database.Dopple;
using Stump.Server.WorldServer.Database.Items.Shops;
using Stump.Server.WorldServer.Database.Items.Templates;
using Stump.Server.WorldServer.Database.Misc;
using Stump.Server.WorldServer.Database.Payments;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Npcs;
using Stump.Server.WorldServer.Game.Items;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;

namespace Stump.Server.WorldServer.Game.Misc
{
    public class AutoManager : DataManager<AutoManager>
    {
        [Variable]
        public static int AnnouncesDelaySeconds = 900;
        [Variable]
        public static int AnnouncesWebsiteDelaySeconds = 700;
        [Variable]
        public static int AnnouncesItemsPromoDelaySeconds = 600;

        //Paymants
        private Dictionary<int, StaffPayments> m_Payments;
        private List<StaffPayments> PaymentsList = new List<StaffPayments>();

        //Announces
        private int m_lastId;
        private int m_lastItemId;
        private int m_PaymentId = 0;
        private Dictionary<int, AutoAnnounceMessage> m_announces;

        //NPCs
        private Dictionary<int, NpcItem> m_NpcShopItems = new Dictionary<int, NpcItem>();

        [Initialization(InitializationPass.Any)]
        public override void Initialize()
        {
            #region >> Limpando as variaveis para que o comando Reload não duplique elas.

            if (m_NpcShopItems != null)
                m_NpcShopItems.Clear();

            if (m_Payments != null)
                m_Payments.Clear();

            if (m_announces != null)
                m_announces.Clear();

            #endregion

            m_NpcShopItems = Database.Query<NpcItem>(NpcItemRelator.FetchQuery).ToDictionary(x => x.Id);
            m_Payments = Database.Query<StaffPayments>(StaffPaymentsTableRelator.FetchQuery).ToDictionary(x => x.Id);
            m_announces = Database.Query<AutoAnnounceMessage>(AutoAnnounceMessageRelator.FecthQuery).ToDictionary(x => x.Id);

            WorldServer.Instance.IOTaskPool.CallPeriodically(AnnouncesDelaySeconds * 1000, PromptNextAnnounce);
            WorldServer.Instance.IOTaskPool.CallPeriodically(AnnouncesWebsiteDelaySeconds * 1000, PromptNextAnnounceWebsite);
            WorldServer.Instance.IOTaskPool.CallPeriodically(AnnouncesItemsPromoDelaySeconds * 1000, PromptNextAnnounceItensPromo);
            WorldServer.Instance.IOTaskPool.CallDelayed(500, CheckPayments);
            WorldServer.Instance.IOTaskPool.CallDelayed(1000, CheckAndDeleteDoppleRecords);
            WorldServer.Instance.IOTaskPool.CallDelayed(1000, CheckAndDeleteMandatoryRecords);
        }

        #region >> Prompt Announce
        private void PromptNextAnnounce()
        {
            if (!m_announces.Any())
                return;

            AutoAnnounceMessage announce;

            if (m_lastId >= m_announces.Keys.Max())
                announce = m_announces.Values.OrderBy(x => x.Id).FirstOrDefault(x => x.Type != 2);
            else
                announce = m_announces.Values.FirstOrDefault(x => x.Id > m_lastId && x.Type != 2);

            if (announce == null)
                return;

            SendAnnounce(announce);

            m_lastId = announce.Id;
        }

        #region >> Prompt Announce Website
        private void PromptNextAnnounceWebsite()
        {
            if (Settings.Ogrines2xAnnounce == false)
                return;

            if (!m_announces.Any())
                return;

            AutoAnnounceMessage announce;

            if (m_lastId >= m_announces.Keys.Max())
                announce = m_announces.Values.OrderBy(x => x.Id).FirstOrDefault(x => x.Type == 2);
            else
                announce = m_announces.Values.FirstOrDefault(x => x.Id > m_lastId && x.Type == 2);

            if (announce == null)
                return;

            SendAnnounce(announce);

            m_lastId = announce.Id;
        }
        #endregion

        #region >> Prompt Announce Promoção de Itens
        private void PromptNextAnnounceItensPromo()
        {
            if (!m_NpcShopItems.Any())
                return;

            List<int> list = new List<int>();
            DateTime Date = DateTime.Now;

            foreach (var item in m_NpcShopItems)
            {
                if (Date <= item.Value.Discount_Date_End)
                    list.Add(item.Value.Id);
            }

            NpcItem Item;

            if (!list.Any())
                return;

            if (m_lastItemId >= list.Max())
                Item = m_NpcShopItems.Values.OrderBy(x => x.Id).FirstOrDefault(x => Date <= x.Discount_Date_End);
            else
                Item = m_NpcShopItems.Values.FirstOrDefault(x => x.Id > m_lastItemId && Date <= x.Discount_Date_End);

            if (Item == null)
                return;

            SendPromoItemAnnounce(Item);

            m_lastItemId = Item.Id;
        }
        #endregion

        #region >> SendAnnounce
        private static void SendAnnounce(AutoAnnounceMessage announce)
        {
            var color = announce.Color != null ? (Color?)Color.FromArgb(announce.Color.Value) : null;

            WorldServer.Instance.IOTaskPool.AddMessage(() =>
                World.Instance.ForEachCharacter(character =>
                {
                    string msg;
                    if (announce.Type == 0) //Informações
                    {
                        #region Informations
                        switch (character.Account.Lang)
                        {
                            case "fr":
                                msg = character.IsGameMaster() ? $"<b>[Informations]</b> {announce.MessageFR}" : $"<b>[Informations]</b> {announce.MessageFR}";
                                break;
                            case "es":
                                msg = character.IsGameMaster() ? $"<b>[Información]</b> {announce.MessageES}" : $"<b>[Información]</b> {announce.MessageES}";
                                break;
                            case "en":
                                msg = character.IsGameMaster() ? $"<b>[Information]</b> {announce.MessageEN}" : $"<b>[Information]</b> {announce.MessageEN}";
                                break;
                            default:
                                msg = character.IsGameMaster() ? $"<b>[Informações]</b> {announce.Message}" : $"<b>[Informações]</b> {announce.Message}";
                                break;
                        }
                        #endregion
                    }
                    else if (announce.Type == 1)//Tutoriais
                    {
                        #region Tutorial
                        switch (character.Account.Lang)
                        {
                            case "fr":
                                msg = character.IsGameMaster() ? $"<b>[Didacticiel]</b> {announce.MessageFR}" : $"<b>[Didacticiel]</b> {announce.MessageFR}";
                                break;
                            case "es":
                                msg = character.IsGameMaster() ? $"<b>[Tutorial]</b> {announce.MessageES}" : $"<b>[Tutorial]</b> {announce.MessageES}";
                                break;
                            case "en":
                                msg = character.IsGameMaster() ? $"<b>[Tutorial]</b> {announce.MessageEN}" : $"<b>[Tutorial]</b> {announce.MessageEN}";
                                break;
                            default:
                                msg = character.IsGameMaster() ? $"<b>[Tutorial]</b> {announce.Message}" : $"<b>[Tutorial]</b> {announce.Message}";
                                break;
                        }
                        #endregion
                    }
                    else if (announce.Type == 2)//Website
                    {
                        #region Website
                        switch (character.Account.Lang)
                        {
                            case "fr":
                                msg = character.IsGameMaster() ? $"<b>[Website]</b> {announce.MessageFR}" : $"<b>[Website]</b> {announce.MessageFR}";
                                break;
                            case "es":
                                msg = character.IsGameMaster() ? $"<b>[Website]</b> {announce.MessageES}" : $"<b>[Website]</b> {announce.MessageES}";
                                break;
                            case "en":
                                msg = character.IsGameMaster() ? $"<b>[Website]</b> {announce.MessageEN}" : $"<b>[Website]</b> {announce.MessageEN}";
                                break;
                            default:
                                msg = character.IsGameMaster() ? $"<b>[Website]</b> {announce.Message}" : $"<b>[Website]</b> {announce.Message}";
                                break;
                        }
                        #endregion
                    }
                    else
                    {
                        #region Default
                        switch (character.Account.Lang)
                        {
                            case "fr":
                                msg = character.IsGameMaster() ? $"{announce.MessageFR}" : $"{announce.MessageFR}";
                                break;
                            case "es":
                                msg = character.IsGameMaster() ? $"{announce.MessageES}" : $"{announce.MessageES}";
                                break;
                            case "en":
                                msg = character.IsGameMaster() ? $"{announce.MessageEN}" : $"{announce.MessageEN}";
                                break;
                            default:
                                msg = character.IsGameMaster() ? $"{announce.Message}" : $"{announce.Message}";
                                break;
                        }
                        #endregion
                    }


                    if (color != null)
                        character.SendServerMessage(msg, color.Value);
                    else
                        character.SendServerMessage(msg);
                }));
        }

        private static void SendPromoItemAnnounce(NpcItem item)
        {
            DateTime PromoItemEnd = item.Discount_Date_End;
            TimeSpan CompareResultDate = PromoItemEnd - DateTime.Now;

            #region >> NpcTemplate Name Functions
            var npcTemplate = NpcManager.Instance.GetNpcTemplate(item.NpcTempId);
            var npcSpawn = NpcManager.Instance.GetNpcSpawns().FirstOrDefault(x => x.NpcId == npcTemplate.Id);
            var npcMapPosition = World.Instance.GetMap(npcSpawn.MapId);
            var npcPositionString = "{map," + npcMapPosition.Position.X + "," + npcMapPosition.Position.Y + ",1," + Uri.EscapeDataString(npcTemplate.Name) + "}";
            #endregion

            #region >> Item Construction Menssage
            var itemName = "{item," + item.Item.Id + "}";
            #endregion

            #region >> Npc Current Token Name
            var tokenCurrent = "Kamas";
            ItemTemplate itemTemplate = null;

            var npcTemplateAction = NpcManager.Instance.GetNpcActions(item.NpcTempId);
            var npcAction = npcTemplateAction.FirstOrDefault(x => x.Id == item.NpcShopId);

            if (npcAction != null)
            {
                var itemId = npcAction.Record.GetParameter<int>(0);
                itemTemplate = ItemManager.Instance.TryGetTemplate(itemId);

                if (itemTemplate != null)
                {
                    if (itemTemplate.Id == Settings.TokenTemplateId)
                    {
                        tokenCurrent = "Ogrines";
                    }
                    else
                    {
                        tokenCurrent = itemTemplate.Name;
                    }
                }
            }
            #endregion

            var color = Color.FromArgb(59135);

            WorldServer.Instance.IOTaskPool.AddMessage(() =>
                World.Instance.ForEachCharacter(character =>
                {
                    string msg;

                    if (CompareResultDate.TotalHours <= 24)
                    {
                        #region Mensagem em menos de 24 horas
                        switch (character.Account.Lang)
                        {
                            case "fr":
                                msg = $"<b>[Promotion]</b> L'article <b>" + itemName + "</b> est en vente avec une réduction de <b>" + (item.Active_Percent_Discount * 100) + "%</b> dans la boutique " + npcPositionString + " se terminant dans <b>" + (PromoItemEnd.Subtract(DateTime.Now).Hours) + " heure(s) et " + (PromoItemEnd.Subtract(DateTime.Now).Minutes) + " minute(s)</b>, vendu pour <b>" + tokenCurrent + "</b>.";
                                break;
                            case "es":
                                msg = $"<b>[Promoción]</b> El artículo <b>" + itemName + "</b> está en oferta con un descuento del <b>" + (item.Active_Percent_Discount * 100) + "%</b> en la tienda " + npcPositionString + " que finaliza en <b>" + (PromoItemEnd.Subtract(DateTime.Now).Hours) + " hora(s) y " + (PromoItemEnd.Subtract(DateTime.Now).Minutes) + " minuto(s)</b>, vendido por <b>" + tokenCurrent + "</b>.";
                                break;
                            case "en":
                                msg = $"<b>[Promotion]</b> The item <b>" + itemName + "</b> is on sale with <b>" + (item.Active_Percent_Discount * 100) + "%</b> discount at the store " + npcPositionString + " ending in <b>" + (PromoItemEnd.Subtract(DateTime.Now).Hours) + " hour(s) and " + (PromoItemEnd.Subtract(DateTime.Now).Minutes) + " minute(s)</b>, sold for <b>" + tokenCurrent + "</b>.";
                                break;
                            default:
                                msg = $"<b>[Promoção]</b> O Item <b>" + itemName + "</b> está com <b>" + (item.Active_Percent_Discount * 100) + "%</b> de desconto na loja " + npcPositionString + " com término em <b>" + (PromoItemEnd.Subtract(DateTime.Now).Hours) + " hora(s) e " + (PromoItemEnd.Subtract(DateTime.Now).Minutes) + " minuto(s)</b>, vendido por <b>" + tokenCurrent + "</b>.";
                                break;
                        }
                        #endregion
                    }
                    else
                    {
                        #region Mensagem por tempo limitado
                        switch (character.Account.Lang)
                        {
                            case "fr":
                                msg = $"<b>[Promotion]</b> L'article <b>" + itemName + "</b> est en vente avec une réduction de <b>" + Math.Round(item.Active_Percent_Discount * 100) + "%</b> à la boutique " + npcPositionString + " pour une durée limitée, vendu pour <b>" + tokenCurrent + "</b>.";
                                break;
                            case "es":
                                msg = $"<b>[Promoción]</b> El artículo <b>" + itemName + "</b> está en oferta con un descuento del <b>" + Math.Round(item.Active_Percent_Discount * 100) + "%</b> en la tienda " + npcPositionString + " por tiempo limitado, vendido por <b>" + tokenCurrent + "</b>.";
                                break;
                            case "en":
                                msg = $"<b>[Promotion]</b> The item <b>" + itemName + "</b> is on sale with <b>" + Math.Round(item.Active_Percent_Discount * 100) + "%</b> discount at the store " + npcPositionString + " for a limited time, sold for <b>" + tokenCurrent + "</b>.";
                                break;
                            default:
                                msg = $"<b>[Promoção]</b> O Item <b>" + itemName + "</b> está com <b>" + Math.Round(item.Active_Percent_Discount * 100) + "%</b> de desconto na loja " + npcPositionString + " por tempo limitado, vendido por <b>" + tokenCurrent + "</b>.";
                                break;
                        }
                        #endregion
                    }
                    character.SendServerMessage(msg, color);
                }));
        }
        #endregion
        #endregion

        #region >> Check Delete Records Dopples/Mandatory
        private void CheckAndDeleteDoppleRecords()
        {
            try
            {
                var records = Database.Fetch<DoppleRecord>("SELECT * FROM characters_dopple WHERE Time < @expirationDate", new { expirationDate = DateTime.Now.AddDays(-2) });

                foreach (var record in records)
                {
                    try
                    {
                        Database.Execute("DELETE FROM characters_dopple WHERE Id = @id AND CharacterId = @charId", new { id = record.Id, charId = record.CharacterId });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error deleting Dopple record with Id {record.Id} and CharacterId {record.CharacterId}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching Dopple records: {ex.Message}");
            }
        }

        private void CheckAndDeleteMandatoryRecords()
        {
            try
            {
                var records = Database.Fetch<DoppleRecord>("SELECT * FROM characters_mandatory WHERE Time < @expirationDate", new { expirationDate = DateTime.Now.AddDays(-7) });

                foreach (var record in records)
                {
                    try
                    {
                        Database.Execute("DELETE FROM characters_mandatory WHERE Id = @id AND OwnerId = @ownerId", new { id = record.Id, ownerId = record.CharacterId });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error deleting mandatory record with Id {record.Id} and OwnerId {record.CharacterId}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching mandatory records: {ex.Message}");
            }
        }

        #endregion

        #region >> Payments
        private void CheckPayments()
        {
            if (!m_Payments.Any())
                return;

            var itemtemplate = ItemManager.Instance.TryGetTemplate(Settings.TokenTemplateId);

            foreach (var Pay in m_Payments.Values.Where(x => x.Activo != 0 && DateTime.Now.Subtract(x.StartDate).Days >= x.PayDays && x.LastPayment == null || DateTime.Now.Subtract(x.LastPayment).Days >= x.PayDays).OrderBy(x => x.Id))
            {
                if (Pay == null)
                    return;

                if (Pay.Id >= m_PaymentId)
                {
                    PaymentsList.Add(Pay);
                    m_PaymentId = Pay.Id;
                }
                else
                    break;
            }

            foreach (var payments in PaymentsList)
            {
                if (payments == null)
                    return;

                Payment(payments.Wage, payments.AccountId, "Pay Ogrines", payments.Id);

                #region // ----------------- Sistema de Logs MongoDB Staff Pagamentos by: Kenshin ---------------- //
                try
                {
                    var document = new BsonDocument
                                {
                                    { "Name", payments.Name },
                                    { "LastName", payments.LastName },
                                    { "AccountId", payments.AccountId },
                                    { "StaffName", payments.StaffName },
                                    { "Email", payments.Email },
                                    { "Valor", payments.Wage },
                                    { "Date", DateTime.Now.ToString(CultureInfo.CreateSpecificCulture("pt-BR")) }
                                };

                    MongoLogger.Instance.Insert("Staff_Payments", document);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Erro no Mongologs Staff Pagamentos : " + e.Message);
                }
                #endregion
            }
        }

        public void Payment(uint Wage, int AccountId, string ItemName, int PaymentId)
        {
            var itemtemplate = ItemManager.Instance.TryGetTemplate(Settings.TokenTemplateId);

            Gifts.Gifts.Instance.SetGift(AccountId, ItemName, itemtemplate, Wage, true, false, false, false, true, "Staff Payments");

            UpdatePayment(PaymentId);
        }

        public void UpdatePayment(int paymentId)
        {
            try
            {
                string query = "UPDATE staffs_payments SET LastPayment = @LastPayment WHERE Id = @PaymentId";
                DateTime currentDateTime = DateTime.Now;

                // Use parâmetros para evitar injeção de SQL
                var parameters = new { LastPayment = currentDateTime, PaymentId = paymentId };

                WorldServer.Instance.DBAccessor.Database.Execute(query, parameters);

                m_Payments.Remove(paymentId);

                Console.WriteLine($"Payment with ID {paymentId} updated successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during payment update: {ex.Message}. The payment could not be updated.");
                // Você pode escolher relançar a exceção se necessário ou realizar ações adicionais de tratamento aqui
            }
        }
        #endregion
    }
}