using Database.Mandatory;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Messages;
using Stump.Server.BaseServer.Database;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Npcs;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Items;
using System;
using System.Linq;

namespace Stump.Server.WorldServer.Database.Npcs.Replies
{
    [Discriminator("Mandatory", typeof(NpcReply), typeof(NpcReplyRecord))]
    public class MandatoryReply : NpcReply
    {
        public MandatoryReply(NpcReplyRecord record) : base(record)
        {
        }

        public int value
        {
            get
            {
                return Record.GetParameter<int>(0);
            }
            set
            {
                Record.SetParameter(0, value);
            }
        }

        public override bool Execute(Npc npc, Character character)
        {
            if (!base.Execute(npc, character))
                return false;

            var DeleteMandatory = new MandatoryRecord();
            var CompareTime = DateTime.Now;

            foreach (var Mandatory in character.MandatoryCollection.Mandatory.Where(Mandatory => Mandatory.MandatoryId == value && Mandatory.OwnerId == character.Id))
            {
                DeleteMandatory = Mandatory;
                CompareTime = Mandatory.Time;
                break;
            }

            if (!(CompareTime <= DateTime.Now))
            {
                #region MSG
                character.SendServerMessageLang(
                         "Espere, você não pode fazer isso! O Sacerdote está cansado, você deve esperar <b>" + CompareTime.Subtract(DateTime.Now).Hours + " hora(s) e " + CompareTime.Subtract(DateTime.Now).Minutes + " minuto(s) para conseguir fazer uma nova solicitação ao sacerdote</b>",
                         "Wait, you can't do that! Priest is tired, you should wait <b>" + CompareTime.Subtract(DateTime.Now).Hours + " hour(s) and " + CompareTime.Subtract(DateTime.Now).Minutes + " minute(s) to get make a new request to the priest</b>",
                         "¡Espera, no puedes hacer eso! Priest está cansado, debe esperar <b>" + CompareTime.Subtract(DateTime.Now).Hours + " hora(s) y " + CompareTime.Subtract(DateTime.Now).Minutes + " minute(s) para obtener make una nueva petición al sacerdote</b>",
                         "Attendez, vous ne pouvez pas faire ça ! Le prêtre est fatigué, vous devez attendre <b>" + CompareTime.Subtract(DateTime.Now).Hours + " heure(s) et " + CompareTime.Subtract(DateTime.Now).Minutes + " minute(s) pour obtenir make une nouvelle demande au prêtre</b>"
                          );
                #endregion

                character.LeaveDialog();
                return false;
            }
            else if (CompareTime <= DateTime.Now)
            {
                if (DeleteMandatory != null)
                    character.MandatoryCollection.DeleteMandatory.Add(DeleteMandatory);

                #region Troca de Nome do Personagem
                if (value == 1)
                {
                    if ((character.Record.MandatoryChanges & (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_NAME) == (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_NAME)
                    {
                        character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_POPUP, 43);
                        return false;
                    }

                    character.Record.MandatoryChanges |= (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_NAME;
                    character.Record.PossibleChanges |= (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_NAME;

                    character.SendSystemMessage(41, false);
                }
                #endregion

                #region Troca de Cor do Personagem
                else if (value == 2)
                {
                    if ((character.Record.MandatoryChanges & (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_COLORS) == (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_COLORS)
                    {
                        character.SendSystemMessage(43, false);
                        return false;
                    }

                    character.Record.MandatoryChanges |= (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_COLORS;
                    character.Record.PossibleChanges |= (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_COLORS;

                    character.SendSystemMessage(42, false);
                }
                #endregion

                #region Troca de Rosto do Personagem
                else if (value == 3)
                {
                    if ((character.Record.MandatoryChanges & (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_COSMETIC) == (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_COSMETIC)
                    {
                        character.SendSystemMessage(43, false);
                        return false;
                    }

                    character.Record.MandatoryChanges |= (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_COSMETIC;
                    character.Record.PossibleChanges |= (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_COSMETIC;

                    character.SendSystemMessage(58, false);
                }
                #endregion

                #region Troca de Sexo do Personagem
                else if (value == 4)
                {
                    if ((character.Record.MandatoryChanges & (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_GENDER) == (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_GENDER)
                    {
                        character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_POPUP, 43);
                        character.SendSystemMessage(43, false);
                        return false;
                    }

                    character.Record.MandatoryChanges |= (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_GENDER;
                    character.Record.PossibleChanges |= (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_GENDER;

                    character.SendSystemMessage(44, false);
                }
                #endregion

                #region Troca de Classe do Personagem
                else if (value == 5)
                {
                    if ((character.Record.MandatoryChanges & (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_BREED) == (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_BREED)
                    {
                        character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_POPUP, 43);
                        character.SendSystemMessage(43, false);
                        return false;
                    }

                    character.Record.MandatoryChanges |= (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_BREED;
                    character.Record.PossibleChanges |= (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_BREED;
                    character.Record.PossibleChanges |= (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_GENDER;
                    character.Record.PossibleChanges |= (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_COSMETIC;
                    character.Record.PossibleChanges |= (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_COLORS;

                    character.SendSystemMessage(63, false);
                }
                #endregion

                #region Mimibiótico
                else if (value == 6)
                {
                    var mimisymbic = character.Inventory.TryGetItem(ItemIdEnum.MIMIBIOTE_14485);

                    if (mimisymbic != null)
                    {
                        foreach (var mimi in character.Inventory.GetItems(x => x.Template.Id == 14485))
                        {
                            if (mimi.Effects.Exists(y => y.EffectId == EffectsEnum.Effect_BlockItemNpcShop))
                                character.Inventory.RemoveItem(mimi, 1, true);
                        }
                    }

                    var item = ItemManager.Instance.CreatePlayerItem(character, 14485, 1);
                    item.Effects.RemoveAll(x => x.EffectId == EffectsEnum.Effect_Exchangeable);
                    item.Effects.Add(new EffectInteger(EffectsEnum.Effect_NonExchangeable_981, 0));
                    item.Effects.Add(new EffectInteger(EffectsEnum.Effect_NonExchangeable_982, 0));
                    item.Effects.Add(new EffectString(EffectsEnum.Effect_BlockItemNpcShop, "Hydra"));
                    character.Inventory.AddItem(item);

                    character.Client.Send(new ClientUIOpenedByObjectMessage(3, (uint)item.Guid));
                }
                #endregion

                #region Final
                else
                {
                    return false;
                }
                #endregion

                if (value != 6)
                {
                    int Time = character.Client.UserGroup.Role >= RoleEnum.Gold_Vip ? 3 : 24;

                    character.MandatoryCollection.Mandatory.Add(new MandatoryRecord
                    {
                        OwnerId = character.Id,
                        MandatoryId = value,
                        Time = DateTime.Now.AddHours(Time),
                        IsNew = true,
                        Ip = character.Client.IP
                    });
                }
            }

            character.SaveLater();
            return true;
        }
    }
}
