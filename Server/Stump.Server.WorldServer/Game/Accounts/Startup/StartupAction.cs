using System;
using System.Globalization;
using System.Linq;
using MongoDB.Bson;
using NLog;
using Stump.DofusProtocol.Types;
using Stump.Server.BaseServer;
using Stump.Server.BaseServer.IPC.Messages;
using Stump.Server.BaseServer.Logging;
using Stump.Server.WorldServer.Core.IPC;
using Stump.Server.WorldServer.Core.Network;
using Stump.Server.WorldServer.Database.Characters;
using Stump.Server.WorldServer.Database.Startup;
using Stump.Server.WorldServer.Game.HavenBags;
using Stump.Server.WorldServer.Game.Items.Player;

namespace Stump.Server.WorldServer.Game.Accounts.Startup
{
    public class StartupAction
    {
        // private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        public StartupAction(StartupActionRecord record)
        {
            Record = record;

            WorldServer.Instance.IOTaskPool.ExecuteInContext(() =>
            { 
                Items = ( from entry in record.Items select new StartupActionItem(entry)).ToArray();
            });
        }

        public StartupAction() { }

        public StartupActionRecord Record { get; set; }

        public int Id
        {
            get { return Record.Id; }
            set { Record.Id = value; }
        }

        public string Title
        {
            get { return Record.Title; }
            set { Record.Title = value; }
        }

        public string Text
        {
            get { return Record.Text; }
            set { Record.Text = value; }
        }

        public string DescUrl
        {
            get { return Record.DescUrl; }
            set { Record.DescUrl = value; }
        }

        public string PictureUrl
        {
            get { return Record.PictureUrl; }
            set { Record.PictureUrl = value; }
        }

        public bool IsVeteranReward { get; set; }

        public StartupActionItem[] Items { get; set; }

        public void GiveGiftTo(WorldClient client, CharacterRecord character, StartupAction action)
        {
            var items = Items;
            var Function = "";
            var account = AccountManager.Instance.FindById(character.AccountId);
            int lastitem = 0;

            if (items == null)
                return;

            if (account == null)
                return;

            var characteronline = (account.ConnectedCharacter.HasValue) ? World.Instance.GetCharacter(account.ConnectedCharacter.Value) : null;

            foreach (var startupActionItem in items.OrderBy(x => x.Record.Id).Where(y => y.Record.Id != lastitem))
            {
                if (startupActionItem.ItemTemplate.Id == Inventory.TokenTemplateId && startupActionItem.Ogrines == true)
                {
                    startupActionItem.GiveToOgrines(client, character, action);
                    Function = "GiveToOgrines";
                }
                else if (account.ConnectedCharacter.HasValue && characteronline != null)
                {
                    startupActionItem.GiveToOnlineItems(client, characteronline, action);
                    Function = "GiveToOnlineItems";
                }
                else
                {
                    startupActionItem.GiveToOfflineItems(client, character, action);
                    Function = "GiveToOfflineItems";
                }
                
                lastitem = Record.Id;
            }

            #region // ----------------- Sistema de Logs MongoDB Gift by: Kenshin ---------------- //
            try
            {
                var document = new BsonDocument
                            {
                                { "AccountId", account.Id },
                                { "AccountName", account.Nickname },
                                { "CharacterId", character.Id },
                                { "CharacterName", character.Name },
                                { "Function", Function.ToString() },
                                { "Date", DateTime.Now.ToString(CultureInfo.CreateSpecificCulture("pt-BR")) }
                            };

                MongoLogger.Instance.Insert("Player_GiveGift", document);
            }
            catch (Exception e)
            {
                Console.WriteLine("Erro no Mongologs Presente de Retorno : " + e.Message);
            }
            #endregion
        }

        // TODO - 2.71
        public GameActionItem GetStartupActionAddObject()
        {
            GameActionItem _startupAction = new GameActionItem()
            {
                uid = Id,
                title = Title,
                text = Text,
                descUrl = DescUrl,
                pictureUrl = PictureUrl,
                items = (from entry in Items select entry.GetObjectItemInformationWithQuantity()).ToArray(),
            };

            return _startupAction;
        }
    }
}