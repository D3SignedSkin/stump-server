using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Database;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Npcs;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Items;
using System;

namespace Stump.Server.WorldServer.Database.Npcs.Replies
{
    [Discriminator("RebornAlignment", typeof(NpcReply), new Type[] { typeof(NpcReplyRecord) })]
    public class RebornAlign : NpcReply
    {
        public RebornAlign(NpcReplyRecord record) : base(record)
        { }

        public int AlignmentSide
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

        public int CostOgrines
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
            if (character.AlignmentSide <= 0)
            {
                character.SendServerMessageLang
                    (
                    "Você não possui alinhamento para gerar um certificado.",
                    "You do not have alignment to generate a certificate.",
                    "No tienes alineación para generar un certificado.",
                    "Vous n'avez pas d'alignement pour générer un certificat."
                    );
                return false;
            }
            else if (character.Honor <= 0)
            {
                character.SendServerMessageLang
                    (
                    "Você não possui honra o suficiente para gerar um certificado.",
                    "You don't have enough honor to generate a certificate.",
                    "No tienes suficiente honor para generar un certificado.",
                    "Vous n'avez pas assez d'honneur pour générer un certificat."
                    );
                return false;
            }
            else if (character.Account.Tokens < CostOgrines)
            {
                character.SendServerMessageLang
                    (
                    "Você não possui ogrines o suficiente para gerar um certificado.",
                    "You don't have enough ogrines to generate a certificate.",
                    "No tienes suficiente ogrines para generar un certificado.",
                    "Vous n'avez pas assez d'ogrines pour générer un certificat."
                    );
                return false;
            }
            else if (character.Account.Tokens != 0 && character.Inventory.CanTokenBlock() == true)
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
                short FinalHonor;
                FinalHonor = (short)(character.Honor - (0.10 * character.Honor));
                var item = ItemManager.Instance.CreatePlayerItem(character, 30017, 1);

                if (FinalHonor <= 1)
                    FinalHonor = 1;

                if (character.Inventory.RemoveTokenItem(CostOgrines, "NPC: [" + item.Template.Name + ": " + CostOgrines + "]"))
                {
                    character.ChangeAlignementSide(AlignmentSideEnum.ALIGNMENT_NEUTRAL);
                    item.Effects.RemoveAll(x => x.EffectId == EffectsEnum.Effect_AddHonor);
                    item.Effects.Add(new EffectDice(EffectsEnum.Effect_AddHonor, 0, FinalHonor, 0));

                    character.Inventory.AddItem(item);

                    character.SendServerMessageLang(
                        $"Você perdeu {CostOgrines}x Ogrines.",
                        $"You lost {CostOgrines}x Ogrines.",
                        $"Perdiste {CostOgrines}x Ogrinas.",
                        $"Vous avez perdu {CostOgrines}x Ogrines.");

                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}