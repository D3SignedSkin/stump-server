using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Database;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Npcs;
using System.Linq;

namespace Stump.Server.WorldServer.Database.Npcs.Replies
{
    [Discriminator("AddScrollStats", typeof(NpcReply), new System.Type[]
    {
        typeof(NpcReplyRecord)
    })]
    public class GiveScrollStatsReply : NpcReply
    {

        public GiveScrollStatsReply(NpcReplyRecord record) : base(record)
        { }

        public int ElementParameter
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

        public int OgrinesParameter
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

        public override bool Execute(Npc npc, Character character)
        {
            if (!base.Execute(npc, character))
            {
                return false;
            }
            else if (character.Stats.Agility.Additional >= 100 && character.Stats.Strength.Additional >= 100 && character.Stats.Vitality.Additional >= 100 && character.Stats.Wisdom.Additional >= 100 && character.Stats.Intelligence.Additional >= 100 && character.Stats.Chance.Additional >= 100)
            {
                #region Mensagem informando que já existe scrolls em todos os elementos
                switch (character.Account.Lang)
                {
                    case "fr":
                        character.SendServerMessage("Votre personnage a déjà des parchemins sur tous les éléments !");
                        break;
                    case "es":
                        character.SendServerMessage("Tu personaje ya tiene Pergaminos en todos los elementos!");
                        break;
                    case "en":
                        character.SendServerMessage("Your character already has Scrolls on all elements!");
                        break;
                    default:
                        character.SendServerMessage("Seu personagem já possui Scrolls em todos os elementos!");
                        break;
                }
                #endregion

                return false;
            }
            else if (character.Inventory.CanTokenBlock() == true)
            {
                character.SendServerMessageLang(
                    "A interação com Ogrines está em manutenção, por favor, tentar novamente mais tarde.", 
                    "Interaction with Ogrines is under maintenance, please try again later.",
                    "La interacción con las ogrinas está en mantenimiento. Vuelve a intentarlo más tarde.",
                    "L'interaction avec les Ogrines est en maintenance, veuillez réessayer plus tard.");

                return false;
            }
            else
            {
                var Ogrines = character.Account.Tokens;

                if (KamasParameter != 0)
                {
                    if (character.Kamas < KamasParameter)
                    {
                        character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 82); //Não possui kamas o suficiente para da sequencia
                        return false;
                    }

                    character.Inventory.SubKamas(KamasParameter);
                    character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 46, KamasParameter);
                }

                if (OgrinesParameter != 0)
                {
                    if (Ogrines == 0)
                    {
                        character.SendServerMessageLang("Você não possui ogrines suficiente.", "You don't have enough ogrines.", "No tienes suficientes ogrinas.", "Vous n'avez pas assez d'ogrines.");
                        return false;
                    }
                    else if (Ogrines < OgrinesParameter)
                    {
                        character.SendServerMessageLang("Você não possui ogrines suficiente.", "You don't have enough ogrines.", "No tienes suficientes ogrinas.", "Vous n'avez pas assez d'ogrines.");
                        return false;
                    }
                    else
                    {
                        if (character.Inventory.RemoveTokenItem(OgrinesParameter, "Give Scrolls Reply"))
                        {
                            character.SendServerMessageLang(
                                $"Você perdeu {OgrinesParameter}x Ogrines.",
                                $"You lost {OgrinesParameter}x Ogrines.",
                                $"Perdiste {OgrinesParameter}x Ogrinas.",
                                $"Vous avez perdu {OgrinesParameter}x Ogrines.");
                        }
                        else
                        {
                            return false;
                        }
                    }
                }

                AddScrolls(character);
                character.RefreshStats();

                if (character.Inventory.GetItems().Count(x => x.Template.Id == Settings.TokenTemplateId) > 0)
                    character.Inventory.RefreshItem(character.Inventory.GetItems().FirstOrDefault(x => x.Template.Id == Settings.TokenTemplateId));

                character.SaveLater();

                return true;
            }
        }

        public void AddScrolls(Character character)
        {
            if (ElementParameter == 1)
            {
                character.Stats.Vitality.Additional = 101;
            }
            else if (ElementParameter == 2)
            {
                character.Stats.Agility.Additional = 101;
            }
            else if (ElementParameter == 3)
            {
                character.Stats.Chance.Additional = 101;
            }
            else if (ElementParameter == 4)
            {
                character.Stats.Strength.Additional = 101;
            }
            else if (ElementParameter == 5)
            {
                character.Stats.Intelligence.Additional = 101;
            }
            else if (ElementParameter == 6)
            {
                character.Stats.Wisdom.Additional = 101;
            }
            else
            {
                if (character.Stats.Vitality.Additional != 101)
                    character.Stats.Vitality.Additional = 101;

                if (character.Stats.Agility.Additional != 101)
                    character.Stats.Agility.Additional = 101;

                if (character.Stats.Chance.Additional != 101)
                    character.Stats.Chance.Additional = 101;

                if (character.Stats.Strength.Additional != 101)
                    character.Stats.Strength.Additional = 101;

                if (character.Stats.Intelligence.Additional != 101)
                    character.Stats.Intelligence.Additional = 101;

                if (character.Stats.Wisdom.Additional != 101)
                    character.Stats.Wisdom.Additional = 101;
            }

            #region Mensagem informando que foi realizado a compra
            if (ElementParameter >= 1 && ElementParameter <= 6)
            {
                switch (character.Account.Lang)
                {
                    case "fr":
                        character.SendServerMessage("Votre personnage est maintenant boosté en éléments.");
                        break;
                    case "es":
                        character.SendServerMessage("Tu personaje ahora está potenciado por elementos.");
                        break;
                    case "en":
                        character.SendServerMessage("Your character is now element boosted.");
                        break;
                    default:
                        character.SendServerMessage("Seu personagem agora está com o elemento aumentado.");
                        break;
                }
            }
            else
            {
                switch (character.Account.Lang)
                {
                    case "fr":
                        character.SendServerMessage("Votre personnage a maintenant tous les éléments augmentés.");
                        break;
                    case "es":
                        character.SendServerMessage("Tu personaje ahora tiene todos los elementos aumentados.");
                        break;
                    case "en":
                        character.SendServerMessage("Your character now has all elements increased.");
                        break;
                    default:
                        character.SendServerMessage("Seu personagem agora está com todos os elementos aumentados.");
                        break;
                }
            }
            #endregion
        }
    }
}