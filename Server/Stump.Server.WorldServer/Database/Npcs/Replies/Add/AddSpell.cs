using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Database;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Npcs;
using Stump.Server.WorldServer.Game.Spells;
using System;

namespace Stump.Server.WorldServer.Database.Npcs.Replies
{
    [Discriminator("AddSpell", typeof(NpcReply), new Type[] { typeof(NpcReplyRecord) })]
    public class AddSpellReply : NpcReply
    {
        public AddSpellReply(NpcReplyRecord record) : base(record)
        { }

        public short Spellid
        {
            get
            {
                return this.Record.GetParameter<short>(0U, false);
            }
            set
            {
                this.Record.SetParameter<short>(0U, value);
            }
        }

        public override bool Execute(Npc npc, Character character)
        {
            bool result;

            if (!base.Execute(npc, character))
            {
                result = false;
            }
            else
            {
                if (character.Spells.HasSpell(Spellid))
                {
                    var spelltemplate = SpellManager.Instance.GetSpellTemplate(Spellid);
                    character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 13, spelltemplate); //Você não pode aprender o feitiço $spell%1.
                    result = false;
                }
                else
                {

                    character.Spells.LearnSpell(Spellid);
                    result = true;
                }

            }
            return result;
        }
    }
}
