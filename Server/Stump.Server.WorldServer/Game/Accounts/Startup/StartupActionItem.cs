using MongoDB.Bson;
using Stump.Core.Reflection;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Messages;
using Stump.DofusProtocol.Types;
using Stump.Server.BaseServer;
using Stump.Server.BaseServer.IPC.Messages;
using Stump.Server.BaseServer.Logging;
using Stump.Server.WorldServer.Core.IPC;
using Stump.Server.WorldServer.Core.Network;
using Stump.Server.WorldServer.Database;
using Stump.Server.WorldServer.Database.Characters;
using Stump.Server.WorldServer.Database.Items;
using Stump.Server.WorldServer.Database.Items.Templates;
using Stump.Server.WorldServer.Database.Startup;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Items;
using Stump.Server.WorldServer.Game.Items.Player;
using Stump.Server.WorldServer.Handlers.Startup;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
namespace Stump.Server.WorldServer.Game.Accounts.Startup
{
    public class StartupActionItem
    {
        public static ORM.Database OrmDatabase = ServerBase<WorldServer>.Instance.DBAccessor.Database;

        private ItemTemplate m_itemTemplate;
        public StartupActionItemRecord m_record;

        public StartupActionItem(StartupActionItemRecord record)
        {
            this.m_record = record;
        }

        public StartupActionItemRecord Record
        {
            get
            {
                return this.m_record;
            }
        }

        public ItemTemplate ItemTemplate
        {
            get
            {
                ItemTemplate arg_28_0;
                if ((arg_28_0 = this.m_itemTemplate) == null)
                {
                    arg_28_0 = (this.m_itemTemplate = Singleton<ItemManager>.Instance.TryGetTemplate(this.m_record.ItemTemplate));
                }
                return arg_28_0;
            }
            set
            {
                this.m_itemTemplate = value;
                this.m_record.ItemTemplate = value.Id;
            }
        }

        public uint Amount
        {
            get
            {
                return this.m_record.Amount;
            }
            set
            {
                this.m_record.Amount = value;
            }
        }

        public bool Ogrines
        {
            get
            {
                return this.m_record.Ogrines;
            }
            set
            {
                this.m_record.Ogrines = value;
            }
        }

        public bool MaxEffects
        {
            get
            {
                return this.m_record.MaxEffects;
            }
            set
            {
                this.m_record.MaxEffects = value;
            }
        }

        public bool LinkedAccount
        {
            get
            {
                return this.m_record.LinkedAccount;
            }
            set
            {
                this.m_record.LinkedAccount = value;
            }
        }

        public bool LinkedCharacter
        {
            get
            {
                return this.m_record.LinkedCharacter;
            }
            set
            {
                this.m_record.LinkedCharacter = value;
            }
        }

        public void GiveToOnlineItems(WorldClient client, Character record, StartupAction action)
        {
            int Amount = (int)this.Amount;

            if (record == null)
                return;

            if (Amount == 0)
                return;

            ItemTemplate Item = ItemTemplate;

            if (Item == null)
                return;

            var CreateItem = ItemManager.Instance.CreatePlayerItem(record, Item.Id, (int)this.Amount);

            if (LinkedAccount)
                CreateItem.Effects.Add(new EffectInteger(EffectsEnum.Effect_NonExchangeable_982, 0));

            if (LinkedCharacter)
                CreateItem.Effects.Add(new EffectInteger(EffectsEnum.Effect_NonExchangeable_981, 0));

            record.Inventory.AddItem(CreateItem);
            record.SaveLater();

            #region // ----------------- Sistema de Logs MongoDB Presente by: Kenshin ---------------- //
            try
            {
                var document = new BsonDocument
                                {
                                    { "HardwareId", client.Account.LastHardwareId },
                                    { "Title", action.Title },
                                    { "AccountId", client.Account.Id },
                                    { "AccountName", client.Account.Login },
                                    { "CharacterId", record.Id },
                                    { "CharacterName", record.Name },
                                    { "Fuction", "GiveToOnlineItems" },
                                    { "Items", Item.ToString() },
                                    { "Date", DateTime.Now.ToString(CultureInfo.CreateSpecificCulture("pt-BR")) }
                                };

                MongoLogger.Instance.Insert("Player_ReceiverGift", document);
            }
            catch (Exception e)
            {
                Console.WriteLine("Erro no Mongologs Presente : " + e.Message);
            }
            #endregion

            DeleteAction(client, action);
        }

        public void GiveToOfflineItems(WorldClient client, CharacterRecord record, StartupAction action)
        {
            uint Amount = this.Amount;

            if (Amount == 0)
                return;

            if (record == null)
                return;

            List<EffectBase> effects = new List<EffectBase>();
            ItemTemplate Item = ItemTemplate;

            if (Item == null)
                return;

            effects = ItemManager.Instance.GenerateItemEffects(ItemManager.Instance.TryGetTemplate(ItemTemplate.Id), MaxEffects);

            if (LinkedAccount)
                effects.Add(new EffectInteger(EffectsEnum.Effect_NonExchangeable_982, 0));

            if (LinkedCharacter)
                effects.Add(new EffectInteger(EffectsEnum.Effect_NonExchangeable_981, 0));

            WorldServer.Instance.IOTaskPool.AddMessage(() =>
            {
                var recorditem = new PlayerItemRecord
                {
                    Id = PlayerItemRecord.PopNextId(),
                    OwnerId = record.Id,
                    Template = ItemManager.Instance.TryGetTemplate(ItemTemplate.Id),
                    Stack = Amount,
                    Position = CharacterInventoryPositionEnum.INVENTORY_POSITION_NOT_EQUIPED,
                    Effects = effects,
                    IsNew = true
                };

                try
                {
                    WorldServer.Instance.DBAccessor.Database.Insert(recorditem);

                    Console.WriteLine("Insertion successful. Record item has been successfully added.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error during database insertion: {ex.Message}. The record item could not be added.");
                }
            });

            #region // ----------------- Sistema de Logs MongoDB Presente by: Kenshin ---------------- //
            try
            {
                var document = new BsonDocument
                                {
                                    { "HardwareId", client.Account.LastHardwareId },
                                    { "Title", action.Title },
                                    { "AccountId", client.Account.Id },
                                    { "AccountName", client.Account.Login },
                                    { "CharacterId", record.Id },
                                    { "CharacterName", record.Name },
                                    { "Fuction", "GiveToOfflineItems" },
                                    { "Items", Item.ToString() },
                                    { "Date", DateTime.Now.ToString(CultureInfo.CreateSpecificCulture("pt-BR")) }
                                };

                MongoLogger.Instance.Insert("Player_ReceiverGift", document);
            }
            catch (Exception e)
            {
                Console.WriteLine("Erro no Mongologs Presente : " + e.Message);
            }
            #endregion

            DeleteAction(client, action);
        }

        public void GiveToOgrines(WorldClient client, CharacterRecord record, StartupAction action)
        {
            uint Amount = this.Amount;

            if (Amount == 0)
                return;

            if (record == null)
                return;

            if (ItemTemplate.Id == Settings.TokenTemplateId && Ogrines == true)
            {
                if (client.CreateAccountToken((int)Amount, "GiveToOgrines Gift", "Token Ogrines", record.Id, record.Name))
                {
                    #region // ----------------- Sistema de Logs MongoDB Presente by: Kenshin ---------------- //
                    try
                    {
                        var document = new BsonDocument
                                {
                                    { "HardwareId", client.Account.LastHardwareId },
                                    { "Title", action.Title },
                                    { "AccountId", client.Account.Id },
                                    { "AccountName", client.Account.Login },
                                    { "CharacterId", record.Id },
                                    { "CharacterName", record.Name },
                                    { "Fuction", "GiveToOgrines" },
                                    { "Items", "Token Ogrines" },
                                    { "Date", DateTime.Now.ToString(CultureInfo.CreateSpecificCulture("pt-BR")) }
                                };

                        MongoLogger.Instance.Insert("Player_ReceiverGift", document);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Erro no Mongologs Presente : " + e.Message);
                    }
                    #endregion

                    DeleteAction(client, action);
                }
                else
                {
                    return;
                }
            }
        }

        private void DeleteAction(WorldClient client, StartupAction action)
        {
            if (client == null)
                return;

            if (action == null)
                return;

            client.StartupActions.Remove(action);

            if (action.IsVeteranReward)
            {
                if (client.Account.RecievedRewards == "")
                    client.Account.RecievedRewards += $"{action.Id}";
                else
                    client.Account.RecievedRewards += $",{action.Id}";

                IPCAccessor.Instance.Send(new UpdateAccountMessage(client.Account));
            }
            else
            {
                OrmDatabase.Execute($"DELETE FROM characters_startup_actions_items_binds WHERE characters_startup_actions_items_binds.OwnerId = {client.Account.Id} AND characters_startup_actions_items_binds.Id = {action.Id}");
            }

            StartupHandler.SendGameActionItemConsumedMessage(client, action, true);
        }

        public ObjectItemInformationWithQuantity GetObjectItemInformationWithQuantity()
        {
            return new ObjectItemInformationWithQuantity((ushort)this.ItemTemplate.Id, ( from entry in this.ItemTemplate.Effects select entry.GetObjectEffect()).ToArray<ObjectEffect>(), (uint)this.Amount);
        }
    }
}
