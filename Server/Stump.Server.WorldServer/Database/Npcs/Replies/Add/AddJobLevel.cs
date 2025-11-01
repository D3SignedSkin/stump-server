using Stump.Server.BaseServer.Database;
using Stump.Server.WorldServer.Database.Jobs;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Npcs;
using Stump.Server.WorldServer.Game.Jobs;
using System;
using System.Linq;

namespace Stump.Server.WorldServer.Database.Npcs.Replies
{
    [Discriminator("AddJobLevel", typeof(NpcReply), new Type[] { typeof(NpcReplyRecord) })]
    public class AddJobLevel : NpcReply
    {
        public int JobId
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

        public ushort level
        {
            get
            {
                return this.Record.GetParameter<ushort>(1U, false);
            }
            set
            {
                this.Record.SetParameter<ushort>(1U, value);
            }
        }

        public int OgrinesParameter
        {
            get
            {
                return Record.GetParameter<int>(2U, true);
            }
            set
            {
                Record.SetParameter(2U, value);
            }
        }

        public AddJobLevel(NpcReplyRecord record) : base(record)
        { }

        public override bool Execute(Npc npc, Character character)
        {
            if (!base.Execute(npc, character))
            {
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

                JobTemplate jobTemplate = JobManager.Instance.GetJobTemplate(JobId);

                if (jobTemplate == null)
                    return false;

                var job = character.Jobs[JobId];
                job.Experience = ExperienceManager.Instance.GetJobLevelExperience(level);

                character.SaveLater();
                character.Inventory.RefreshItem(character.Inventory.GetItems().FirstOrDefault(x => x.Template.Id == Settings.TokenTemplateId));

                return true;
            }
        }
    }
}