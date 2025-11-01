using System;
using System.Collections.Generic;
using System.Linq;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Messages;
using Stump.Server.WorldServer.Database.Items.Shops;
using Stump.Server.WorldServer.Database.Items.Templates;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Npcs;
using Stump.Server.WorldServer.Game.Items;
using Stump.Server.WorldServer.Handlers.Basic;
using Stump.Server.WorldServer.Handlers.Inventory;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Items.Player;
using MongoDB.Bson;
using System.Globalization;
using Stump.Server.BaseServer.Logging;
using Stump.Server.WorldServer.Game.Npcs;
using Stump.Server.WorldServer.Database.Npcs.Effects;
using NLog;

namespace Stump.Server.WorldServer.Game.Dialogs.Npcs
{
    public class NpcShopDialog : IShopDialog
    {
        private int[] block_npcshop = new int[]
        {
            293, //Shop ID : Vendedor de Escuto
            291, //Shop ID : Vendedor de Alfaiate
            289, //Shop ID : Vendedor de Joalheiro
            287, //Shop ID : Vendedor de Sapateiro
            281, //Shop ID : Vendedor de Armas
        };

        private uint[] effects_itemstype = { 1, 9, 16, 17, 10, 11, 2, 4, 5, 6, 22, 19, 7, 8, 3, 81, 82 };
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public NpcShopDialog(Character character, Npc npc, IEnumerable<NpcItem> items)
        {
            Character = character;
            Npc = npc;
            Items = items;
            CanSell = true;
        }

        public NpcShopDialog(Character character, Npc npc, IEnumerable<NpcItem> items, ItemTemplate token)
        {
            Character = character;
            Npc = npc;
            Items = items;
            Token = token;
            CanSell = true;
        }

        public DialogTypeEnum DialogType => DialogTypeEnum.DIALOG_EXCHANGE;

        public IEnumerable<NpcItem> Items
        {
            get;
            protected set;
        }

        public ItemTemplate Token
        {
            get;
            protected set;
        }

        public Character Character
        {
            get;
            protected set;
        }

        public Npc Npc
        {
            get;
            protected set;
        }

        public bool CanSell
        {
            get;
            set;
        }

        public bool MaxStats
        {
            get;
            set;
        }

        #region IDialog Members

        public void Open()
        {
            Character.SetDialog(this);
            InventoryHandler.SendExchangeStartOkNpcShopMessage(Character.Client, this);
        }

        public void Close()
        {
            InventoryHandler.SendExchangeLeaveMessage(Character.Client, DialogType, false);
            Character.CloseDialog(this);
        }

        #endregion

        public virtual bool BuyItem(int itemId, int amount)
        {
            var itemToSell = Items.FirstOrDefault(entry => entry.Item.Id == itemId);
            var TokenName = "Null";

            #region Condições
            if (World.Instance.GetWorldStatus() == ServerStatusEnum.SAVING)
            {
                Character.SendServerMessageLang(
                    "Impossivel realizar uma compra nesse momento.. favor tentar novamente mais tarde ou após o término do save.",
                    "It is impossible to make a purchase at this time. Please try again later or after the save ends.",
                    "Es imposible realizar una compra en este momento. Vuelve a intentarlo más tarde o cuando finalice el guardado.",
                    "Il est impossible d'effectuer un achat pour le moment. Veuillez réessayer plus tard ou une fois la sauvegarde terminée.");

                return false;
            }

            if (itemToSell == null)
            {
                Character.Client.Send(new ExchangeErrorMessage((int)ExchangeErrorEnum.BUY_ERROR));
                return false;
            }

            if (!itemToSell.AreConditionsFilled(Character))
            {
                Character.Client.Send(new ExchangeErrorMessage((int)ExchangeErrorEnum.BUY_ERROR));
                return false;
            }

            if (Character.Inventory.IsFull(itemToSell.Item, amount))
            {
                Character.Client.Send(new ExchangeErrorMessage((int)ExchangeErrorEnum.REQUEST_CHARACTER_OVERLOADED));
                return false;
            }

            if (Token != null && Token.Id == Inventory.TokenTemplateId && Character.Inventory.CanTokenBlock() == true)
            {
                //Hydra: A interação com Ogrines está em manutenção, por favor, tentar novamente mais tarde.
                Character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 244);
                return false;
            }
            #endregion

            #region Descontos e Condições
            var finalPrice = (int)(itemToSell.Price * amount);

            // Desconto Criado por Opção DB - By:Kenshin
            if (DateTime.Now <= itemToSell.Discount_Date_End)
            {
                double percentual = itemToSell.Active_Percent_Discount;

                finalPrice = ((int)(finalPrice - (percentual * finalPrice)));

                if (finalPrice <= 1)
                    finalPrice = 1;
            }

            if (amount <= 0 || !CanBuy(itemToSell, amount, finalPrice))
            {
                Character.Client.Send(new ExchangeErrorMessage((int)ExchangeErrorEnum.BUY_ERROR));
                return false;
            }
            #endregion

            var item = ItemManager.Instance.CreatePlayerItem(Character, itemId, amount, MaxStats || itemToSell.MaxStats);

            #region Items Effects
            if (item.Effects.Exists(x => x.EffectId == EffectsEnum.Effect_Exchangeable))
            {
                item.Effects.RemoveAll(x => x.EffectId == EffectsEnum.Effect_Exchangeable);
            }

            //Ticekts V.I.P
            if (item.Template.Id == 30001 || item.Template.Id == 30002 || item.Template.Id == 30003 || item.Template.Id == 30004 && DateTime.Now <= itemToSell.Discount_Date_End)
            {
                item.Effects.RemoveAll(x => x.EffectId == EffectsEnum.Effect_Apparence_Wrapper);
            }
            #endregion

            if (Token != null)
            {
                if (Token.Id == Inventory.TokenTemplateId)
                {
                    TokenName = "Ogrines";

                    if (Settings.NpcExoEffects == true && effects_itemstype.Contains(item.Template.TypeId))
                    {
                        int EffectAmount = 0;

                        if (Character.UserGroup.Role == RoleEnum.Vip)
                        {
                            EffectAmount = 1;
                        }
                        else if (Character.UserGroup.Role >= RoleEnum.Gold_Vip)
                        {
                            EffectAmount = 2;
                        }

                        RandomEffect(item, EffectAmount);
                    }

                    if (Character.Inventory.RemoveTokenItem(finalPrice, "Npc Shop: " + item.Template.Name))
                    {
                        Character.Inventory.AddItem(item);
                        Character.SendServerMessage($"{amount} x " + "{item," + $"{item.Template.Id},{item.Guid}"+ "}" + $" ({finalPrice} Ogrines)", color: System.Drawing.ColorTranslator.FromHtml("#46A324"));
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    TokenName = Token.Name;
                    Character.Inventory.UnStackItem(Character.Inventory.TryGetItem(Token), finalPrice);
                    Character.Inventory.AddItem(item);
                }
            }
            else
            {
                if (Settings.NpcExoEffects == true && effects_itemstype.Contains(item.Template.TypeId))
                {
                    int EffectAmount = 0;

                    if (Character.UserGroup.Role == RoleEnum.Vip)
                    {
                        EffectAmount = 1;
                    }
                    else if (Character.UserGroup.Role >= RoleEnum.Gold_Vip)
                    {
                        EffectAmount = 2;
                    }

                    RandomEffect(item, EffectAmount);
                }

                if (block_npcshop.Contains(itemToSell.NpcShopId))
                {
                    //Status ForjaMagia Bloqueada.
                    item.Effects.Add(new EffectInteger(EffectsEnum.Effect_CantFM, 0));
                    //Rastreamento de compra por Kamas NPC.
                    item.Effects.Add(new EffectString(EffectsEnum.Effect_BlockItemNpcShop, "Hydra"));
                    //No permite que el artículo sea intercambiable dentro del juego.
                    item.Effects.Add(new EffectInteger(EffectsEnum.Effect_NonExchangeable_981, 0));
                }

                TokenName = "Kamas";
                Character.Inventory.SubKamas(finalPrice);
                Character.Inventory.AddItem(item);
                Character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 252, item.Template.Id, item.Guid, item.Stack, finalPrice); //%3 x {item,%1,%2} (%4 kamas)
            }

            #region // ----------------- Sistema de Logs MongoDB Compra por Ogrines ou Kamas by: Kenshin ---------------- //
            try
            {
                var CharacterRank = "Player";

                if (Character.Client.Account.UserGroupId >= 4 && Character.Client.Account.UserGroupId <= 9)
                    CharacterRank = "Staff";

                var document = new BsonDocument
                                {
                                    { "HardwareId", Character.Client.Account.LastHardwareId },
                                    { "AccountId", Character.Account.Id },
                                    { "AccountName", Character.Account.Login },
                                    { "AccountUserGroup", CharacterRank },
                                    { "CharacterId", Character.Id },
                                    { "CharacterName", Character.Name },
                                    { "Ogrines", Character.Client.Account.Tokens},
                                    { "Kamas", Character.Kamas},
                                    { "ItemId", itemToSell.ItemId },
                                    { "ItemName", itemToSell.Item.Name },
                                    { "Amount", amount },
                                    { "FinalPrice", (itemToSell.Price * amount) },
                                    { "NpcToken", TokenName},
                                    { "NpcName", Npc.Template.Name},
                                    { "NpcId", Npc.Template.Id},
                                    { "Date", DateTime.Now.ToString(CultureInfo.CreateSpecificCulture("pt-BR")) }
                                };

                MongoLogger.Instance.Insert("Player_ShopBuy", document);
            }
            catch (Exception e)
            {
                Console.WriteLine("Erro no Mongologs das Compras por Ogrines : " + e.Message);
            }
            #endregion

            Character.SaveLater();
            Character.Client.Send(new ExchangeBuyOkMessage());

            return true;
        }

        #region >> Gerar Efeitos
        public void RandomEffect(BasePlayerItem item, int EffectAmount)
        {
            List<int> effects = RandomEffectsToItem(Npcs_Effects_Manager.Instance.GetEffects(), item, EffectAmount);
            List<int> LogEffects = RandomEffectsToItem(Npcs_Effects_Manager.Instance.GetEffects(), item, EffectAmount);

            foreach (int selectedEffect in effects)
            {
                Npcs_Effects effect = Npcs_Effects_Manager.Instance.GetEffectById(selectedEffect);
                int randomQuantity = new Random().Next(effect.Min, effect.Max);
                item.Effects.Add(new EffectInteger((EffectsEnum)selectedEffect, (short)randomQuantity));
            }

            #region // ----------------- Sistema de Logs MongoDB Effeitos em Items by: Kenshin ---------------- //
            try
            {
                var CharacterRank = "Player";

                if (Character.Client.Account.UserGroupId >= 4 && Character.Client.Account.UserGroupId <= 9)
                    CharacterRank = "Staff";

                var document = new BsonDocument
                                {
                                    { "AccountId", Character.Account.Id },
                                    { "AccountName", Character.Account.Login },
                                    { "AccountHardwareId", Character.Account.LastHardwareId },
                                    { "AccountUserGroup", CharacterRank },
                                    { "CharacterId", Character.Id },
                                    { "CharacterName", Character.Name },
                                    { "ItemId", item.Template.Id},
                                    { "ItemName", item.Template.Name},
                                    //Remover Efeitos que não entrão
                                    { "Effects", effects.ToJson()},
                                    { "Date", DateTime.Now.ToString(CultureInfo.CreateSpecificCulture("pt-BR")) }
                                };

                MongoLogger.Instance.Insert("Npc_ExosEffects", document);
            }
            catch (Exception e)
            {
                Console.WriteLine("Erro no Mongologs das Compras por Ogrines : " + e.Message);
            }
            #endregion
        }

        public Random r = new Random();

        private List<int> RandomEffectsToItem(Dictionary<int, Npcs_Effects> Effects, BasePlayerItem playerItem, int quantity)
        {
            List<int> list = new List<int>();
            int CountItemEffects = playerItem.Template.Effects.Count();
            int CountWhile = 0;

            try
            {
                if (CountItemEffects != 0)
                {
                    while ((list.Count < quantity) && CountWhile < 5)
                    {
                        int poolSize = 0;

                        foreach (var EffectList in Effects)
                        {
                            poolSize += EffectList.Value.Probability;
                        }

                        int randomNumber = r.Next(0, poolSize) + 1;
                        int probability = 0;

                        foreach (var EffectList in Effects)
                        {
                            probability += EffectList.Value.Probability;

                            if (randomNumber <= probability)
                            {
                                if (!list.Contains(EffectList.Value.EffectID) && list.Count < quantity)
                                {
                                    if (!playerItem.Effects.Exists(x => x.EffectId == (EffectsEnum)EffectList.Value.EffectID))
                                    {
                                        list.Add(EffectList.Value.EffectID);
                                    }
                                }
                            }
                        }

                        CountWhile += 1;
                    }
                }

                //if (Character.UserGroup.Id >= 4)
                //    Character.SendServerMessage("Effects: " + list.ToJson() + " Count Effects " + list.Count() + " Count While: " + CountWhile + ". by Kenshin");
            }
            catch (Exception ex)
            {
                logger.Error("NpcShopDialog Error: " + ex);
            }

            return list;
        }
        #endregion

        public bool CanBuy(NpcItem item, int amount, int finalprice)
        {
            if (Token != null) // se o Token for fornecido pelo NPC
            {
                var coinToken = Character.Inventory.TryGetItem(Token); // tenta obter o item do inventário do personagem

                if (coinToken.Template.Id == Settings.TokenTemplateId) // se o token for um HydraCoin
                {
                    return Character.Account.Tokens >= finalprice; // retorna true se o personagem tem tokens suficientes
                }
                else // se o token não for um HydraCoin
                {
                    return coinToken != null && coinToken.Stack >= finalprice; // retorna true se o personagem tiver moedas suficientes
                }
            }
            else // se o NPC não exigir um token
            {
                return Character.Inventory.Kamas >= finalprice; // retorna true se o personagem tiver kamas suficientes
            }
        }

        public bool SellItem(int guid, int amount)
        {
            if (!CanSell || amount <= 0)
            {
                Character.Client.Send(new ExchangeErrorMessage((int)ExchangeErrorEnum.SELL_ERROR));
                return false;
            }

            var item = Character.Inventory.TryGetItem(guid);

            if (item == null)
            {
                Character.Client.Send(new ExchangeErrorMessage((int)ExchangeErrorEnum.SELL_ERROR));
                return false;
            }

            if (item.Stack < amount)
            {
                Character.Client.Send(new ExchangeErrorMessage((int)ExchangeErrorEnum.SELL_ERROR));
                return false;
            }

            var price = (int)Math.Ceiling(item.Template.Price / 10) * amount;

            BasicHandler.SendTextInformationMessage(Character.Client, TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 22, amount, item.Template.Id);

            Character.Inventory.RemoveItem(item, amount);

            Character.Inventory.AddKamas(price);

            Character.Client.Send(new ExchangeSellOkMessage());
            return true;
        }
    }
}