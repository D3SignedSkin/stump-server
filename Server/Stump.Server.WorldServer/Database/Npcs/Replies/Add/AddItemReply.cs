using Stump.Core.Reflection;
using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Database;
using Stump.Server.WorldServer.Database.Items.Templates;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Npcs;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Items;
using Stump.Server.WorldServer.Game.Items.Player;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Stump.Server.WorldServer.Database.Npcs.Replies
{
    [Discriminator("AddItem", typeof(NpcReply), new Type[] { typeof(NpcReplyRecord) })]
    public class AddItemReply : NpcReply
    {
        private ItemTemplate m_itemTemplate;

        public int ItemId
        {
            get
            {
                return this.Record.GetParameter<int>(0U, false);
            }
            set
            {
                this.Record.SetParameter<int>(0U, value);
            }
        }

        public ItemTemplate Item
        {
            get
            {
                ItemTemplate itemTemplate;

                if ((itemTemplate = this.m_itemTemplate) == null)
                    itemTemplate = this.m_itemTemplate = Singleton<ItemManager>.Instance.TryGetTemplate(this.ItemId);

                return itemTemplate;
            }
            set
            {
                this.m_itemTemplate = value;
                this.ItemId = value.Id;
            }
        }

        public int Amount
        {
            get
            {
                return this.Record.GetParameter<int>(1U, false);
            }
            set
            {
                this.Record.SetParameter<int>(1U, value);
            }
        }

        public int KamasPrice
        {
            get
            {
                return Record.GetParameter<int>(2, true);
            }
            set
            {
                Record.SetParameter(2, value);
            }
        }

        public AddItemReply(NpcReplyRecord record) : base(record)
        { }

        public override bool Execute(Npc npc, Character character)
        {
            bool flag;
            long CharacterKamas = character.Inventory.Kamas;

            if (!base.Execute(npc, character))
            {
                flag = false;
            }
            else
            {
                if (KamasPrice != 0 && CharacterKamas < KamasPrice)
                {
                    character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 128, KamasPrice);
                    flag = false;
                }
                else
                {
                    BasePlayerItem playerItem = Singleton<ItemManager>.Instance.CreatePlayerItem(character, Item, Amount, false);

                    if (KamasPrice != 0)
                    {
                        character.Inventory.SubKamas(KamasPrice);
                        character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 46, KamasPrice);
                    }

                    if (playerItem == null)
                    {
                        flag = false;
                    }
                    else if (ReplyId == 34214)
                    {
                        #region NPC Kenshin Add Ogrines MSG
                        BasePlayerItem ItemsBlock = character.Inventory.GetItems(CharacterInventoryPositionEnum.INVENTORY_POSITION_NOT_EQUIPED).FirstOrDefault(x => x.Template.Id == 30362);

                        if (character.Account.UserGroupId >= 4)
                        {
                            if (character.Account.Tokens <= 20000 && ItemsBlock == null)
                            {
                                int AmountOgrines = 20000;
                                var effects = playerItem.Effects;

                                effects.RemoveAll(x => x.EffectId == EffectsEnum.Effect_222);
                                effects.Add(new EffectDice(EffectsEnum.Effect_AddOgrines, 0, (short)AmountOgrines, 0));
                                effects.Add(new EffectString(EffectsEnum.Effect_988, character.Name));
                                effects.Add(new EffectInteger(EffectsEnum.Effect_NonExchangeable_981, 0));
                                effects = new List<EffectBase>(effects);

                                ((ItemsCollection<BasePlayerItem>)character.Inventory).AddItem(playerItem);
                                character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 243, playerItem.Template.Id, playerItem.Guid, 1);

                                flag = true;
                            }
                            else
                            {
                                #region MSG
                                switch (character.Account.Lang)
                                {
                                    case "fr":
                                        character.SendServerMessage("Vous avez déjà un coffre à ogrines ou plus de 20 000 ogrines dans votre inventaire.");
                                        break;
                                    case "es":
                                        character.SendServerMessage("Ya tienes un Cofre de Ogrinas o más de 20k Ogrinas en tu inventario.");
                                        break;
                                    case "en":
                                        character.SendServerMessage("You already have an Ogrines Chest or more than 20k Ogrines in your inventory.");
                                        break;
                                    default:
                                        character.SendServerMessage("Você já possui um Baú de Ogrines ou mais que 20k de Ogrines em seu inventário.");
                                        break;
                                }
                                #endregion
                                flag = false;
                            }
                        }
                        else
                        {
                            #region MSG
                            switch (character.Account.Lang)
                            {
                                case "fr":
                                    character.SendServerMessage("Vous n'avez pas le droit d'utiliser cette fonction.");
                                    break;
                                case "es":
                                    character.SendServerMessage("No tiene derechos para utilizar esta función.");
                                    break;
                                case "en":
                                    character.SendServerMessage("You do not have rights to use this function.");
                                    break;
                                default:
                                    character.SendServerMessage("Você não possui direitos para usar essa função.");
                                    break;
                            }
                            #endregion
                            flag = false;
                        }
                        #endregion
                    }
                    else if (ReplyId == 34215)
                    {
                        #region > MSG NPC Kenshin ADD Doplão
                        BasePlayerItem Item = character.Inventory.GetItems(CharacterInventoryPositionEnum.INVENTORY_POSITION_NOT_EQUIPED).FirstOrDefault(x => x.Template.Id == 13052);
                        uint ItemsCount = 0;

                        if (Item != null)
                            ItemsCount = Item.Stack;

                        if (character.Account.UserGroupId >= 4)
                        {
                            if (character.Account.UserGroupId >= 4 && ItemsCount < 20000)
                            {
                                ((ItemsCollection<BasePlayerItem>)character.Inventory).AddItem(playerItem);
                                character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, (short)21, (object)this.Amount, (object)playerItem.Template.Id);
                                flag = true;
                            }
                            else
                            {
                                #region MSG
                                switch (character.Account.Lang)
                                {
                                    case "fr":
                                        character.SendServerMessage("Vous avez déjà plus de 20k dans votre inventaire.");
                                        break;
                                    case "es":
                                        character.SendServerMessage("Ya tienes más de 20k en tu inventario.");
                                        break;
                                    case "en":
                                        character.SendServerMessage("You already have more than 20k in your inventory.");
                                        break;
                                    default:
                                        character.SendServerMessage("Você já possui mais que 20k em seu inventário.");
                                        break;
                                }
                                #endregion
                                flag = false;
                            }
                        }
                        else
                        {
                            #region MSG
                            switch (character.Account.Lang)
                            {
                                case "fr":
                                    character.SendServerMessage("Vous n'avez pas le droit d'utiliser cette fonction.");
                                    break;
                                case "es":
                                    character.SendServerMessage("No tiene derechos para utilizar esta función.");
                                    break;
                                case "en":
                                    character.SendServerMessage("You do not have rights to use this function.");
                                    break;
                                default:
                                    character.SendServerMessage("Você não possui direitos para usar essa função.");
                                    break;
                            }
                            #endregion
                            flag = false;
                        }
                        #endregion
                    }
                    else if (ReplyId == 34216)
                    {
                        #region > MSG NPC Kenshin ADD Peveficha
                        BasePlayerItem Item = character.Inventory.GetItems(CharacterInventoryPositionEnum.INVENTORY_POSITION_NOT_EQUIPED).FirstOrDefault(x => x.Template.Id == 10275);
                        uint ItemsCount = 0;

                        if (Item != null)
                            ItemsCount = Item.Stack;

                        if (character.Account.UserGroupId >= 4)
                        {
                            if (character.Account.UserGroupId >= 4 && ItemsCount < 20000)
                            {
                                ((ItemsCollection<BasePlayerItem>)character.Inventory).AddItem(playerItem);
                                character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, (short)21, (object)this.Amount, (object)playerItem.Template.Id);
                                flag = true;
                            }
                            else
                            {
                                #region MSG
                                switch (character.Account.Lang)
                                {
                                    case "fr":
                                        character.SendServerMessage("Vous avez déjà plus de 20k dans votre inventaire.");
                                        break;
                                    case "es":
                                        character.SendServerMessage("Ya tienes más de 20k en tu inventario.");
                                        break;
                                    case "en":
                                        character.SendServerMessage("You already have more than 20k in your inventory.");
                                        break;
                                    default:
                                        character.SendServerMessage("Você já possui mais que 20k em seu inventário.");
                                        break;
                                }
                                #endregion
                                flag = false;
                            }
                        }
                        else
                        {
                            #region MSG
                            switch (character.Account.Lang)
                            {
                                case "fr":
                                    character.SendServerMessage("Vous n'avez pas le droit d'utiliser cette fonction.");
                                    break;
                                case "es":
                                    character.SendServerMessage("No tiene derechos para utilizar esta función.");
                                    break;
                                case "en":
                                    character.SendServerMessage("You do not have rights to use this function.");
                                    break;
                                default:
                                    character.SendServerMessage("Você não possui direitos para usar essa função.");
                                    break;
                            }
                            #endregion
                            flag = false;
                        }
                        #endregion
                    }
                    else if (ReplyId == 34217)
                    {
                        #region > MSG NPC Kenshin ADD Kolifichas
                        BasePlayerItem Item = character.Inventory.GetItems(CharacterInventoryPositionEnum.INVENTORY_POSITION_NOT_EQUIPED).FirstOrDefault(x => x.Template.Id == 12736);
                        uint ItemsCount = 0;

                        if (Item != null)
                            ItemsCount = Item.Stack;

                        if (character.Account.UserGroupId >= 4)
                        {
                            if (character.Account.UserGroupId >= 4 && ItemsCount < 20000)
                            {
                                ((ItemsCollection<BasePlayerItem>)character.Inventory).AddItem(playerItem);
                                character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, (short)21, (object)this.Amount, (object)playerItem.Template.Id);
                                flag = true;
                            }
                            else
                            {
                                #region MSG
                                switch (character.Account.Lang)
                                {
                                    case "fr":
                                        character.SendServerMessage("Vous avez déjà plus de 20k dans votre inventaire.");
                                        break;
                                    case "es":
                                        character.SendServerMessage("Ya tienes más de 20k en tu inventario.");
                                        break;
                                    case "en":
                                        character.SendServerMessage("You already have more than 20k in your inventory.");
                                        break;
                                    default:
                                        character.SendServerMessage("Você já possui mais que 20k em seu inventário.");
                                        break;
                                }
                                #endregion
                                flag = false;
                            }
                        }
                        else
                        {
                            #region MSG
                            switch (character.Account.Lang)
                            {
                                case "fr":
                                    character.SendServerMessage("Vous n'avez pas le droit d'utiliser cette fonction.");
                                    break;
                                case "es":
                                    character.SendServerMessage("No tiene derechos para utilizar esta función.");
                                    break;
                                case "en":
                                    character.SendServerMessage("You do not have rights to use this function.");
                                    break;
                                default:
                                    character.SendServerMessage("Você não possui direitos para usar essa função.");
                                    break;
                            }
                            #endregion
                            flag = false;
                        }
                        #endregion
                    }
                    else
                    {
                        ((ItemsCollection<BasePlayerItem>)character.Inventory).AddItem(playerItem);
                        character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, (short)21, (object)this.Amount, (object)playerItem.Template.Id);
                        flag = true;
                    }
                }
            }

            return flag;
        }
    }
}