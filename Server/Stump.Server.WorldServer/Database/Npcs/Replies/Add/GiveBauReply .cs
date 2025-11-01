using MongoDB.Bson;
using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Database;
using Stump.Server.BaseServer.Logging;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Npcs;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Items;
using Stump.Server.WorldServer.Game.Items.Player;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Stump.Server.WorldServer.Database.Npcs.Replies
{
    [Discriminator("GiveBauOgrines", typeof(NpcReply), new Type[] { typeof(NpcReplyRecord) })]
    public class GiveBauReply : NpcReply
    {
        public GiveBauReply(NpcReplyRecord record) : base(record)
        { }

        public int OgrinesParameter
        {
            get
            {
                return Record.GetParameter<int>(0, true);
            }
            set
            {
                Record.SetParameter(0, value);
            }
        }

        public int KamasParameter
        {
            get
            {
                return Record.GetParameter<int>(1, true);
            }
            set
            {
                Record.SetParameter(1, value);
            }
        }

        public override bool Execute(Npc npc, Character character)
        {
            bool result;

            int BauId = 0;
            int KamasPrice = 0;
            long CharacterKamas = character.Inventory.Kamas;
            BasePlayerItem tokens = character.Inventory.GetItems(CharacterInventoryPositionEnum.INVENTORY_POSITION_NOT_EQUIPED).FirstOrDefault(x => x.Template.Id == Inventory.TokenTemplateId);

            if (!base.Execute(npc, character))
            {
                result = false;
            }
            else if (CharacterKamas <= 0)
            {
                result = false;
            }
            else if (character.Client.Account.Tokens <= 0)
            {
                result = false;
            }
            else
            {
                KamasPrice = (OgrinesParameter * KamasParameter);

                if (CharacterKamas < KamasPrice)
                {
                    character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 128, KamasPrice);
                    result = false;
                }
                else if (character.Client.Account.Tokens < OgrinesParameter)
                {
                    character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 295, OgrinesParameter);
                    result = false;
                }
                else if (tokens.Stack < OgrinesParameter)
                {
                    character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 295, OgrinesParameter);
                    result = false;
                }
                else if (character.Inventory.CanTokenBlock() == true)
                {
                    //Servidor: La interacción con las ogrinas está en mantenimiento. Vuelve a intentarlo más tarde.
                    character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 244);
                    return false;
                }
                else
                {
                    int TokenAfter = character.Client.Account.Tokens;

                    if (OgrinesParameter <= 500)
                        BauId = 30006;
                    else if (OgrinesParameter >= 501 && OgrinesParameter <= 10000)
                        BauId = 30007;
                    else
                        BauId = 30008;

                    var item = ItemManager.Instance.CreatePlayerItem(character, BauId, 1, false);

                    if (character.Inventory.RemoveTokenItem(OgrinesParameter, "NPC: [" + item.Template.Name + ": " + OgrinesParameter + "]"))
                    {
                        var effects = item.Effects;
                        item.Effects.RemoveAll(x => x.EffectId == EffectsEnum.Effect_222);
                        effects.Add(new EffectDice(EffectsEnum.Effect_AddOgrines, 0, (short)OgrinesParameter, 0));
                        effects.Add(new EffectString(EffectsEnum.Effect_988, character.Name));
                        item.Effects = new List<EffectBase>(effects);

                        character.Inventory.SubKamas(KamasPrice);
                        character.Inventory.AddItem(item);

                        character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 46, KamasPrice);
                        character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 22, OgrinesParameter, Settings.TokenTemplateId);
                        //character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 243, item.Template.Id, item.Guid, 1);

                        #region // ----------------- Sistema de Logs MongoDB Criação de Baús by: Kenshin ---------------- //
                        try
                        {
                            var CharacterRank = "Player";

                            if (character.Client.Account.UserGroupId >= 4 && character.Client.Account.UserGroupId <= 9)
                                CharacterRank = "Staff";

                            var document = new BsonDocument
                        {
                          { "AccountUserGroup", CharacterRank },
                          { "AccountId", character.Account.Id },
                          { "AccountName", character.Account.Login },
                          { "CharacterId", character.Id },
                          { "CharacterName", character.Name },
                          { "Status", "Create" },
                          { "Item", item.Template.Name + ": " + OgrinesParameter + " Ogrines" },
                          { "AfterToken", TokenAfter },
                          { "BeforeToken", character.Client.Account.Tokens },
                          { "HardwareId", character.Client.Account.LastHardwareId },
                          { "Date", DateTime.Now.ToString(CultureInfo.CreateSpecificCulture("pt-BR")) }
                        };

                            MongoLogger.Instance.Insert("Player_CreateBau", document);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Erro no Mongologs do NPC de criação de Baú : " + e.Message);
                        }
                        #endregion

                        result = true;
                    }

                    result = false;
                }
            }

            return result;
        }
    }
}
