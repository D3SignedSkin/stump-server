using Stump.Core.Reflection;
using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Database;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Npcs;
using System;

namespace Stump.Server.WorldServer.Database.Npcs.Replies
{
    [Discriminator("DeleteKamas", typeof(NpcReply), new Type[] { typeof(NpcReplyRecord) })]
    public class DeleteKamas : NpcReply
    {
        public ulong KamasQuantitie
        {
            get
            {
                return this.Record.GetParameter<ulong>(0U, false);
            }
            set
            {
                this.Record.SetParameter<ulong>(0U, value);
            }
        }

        public DeleteKamas(NpcReplyRecord record)
            : base(record)
        {
        }

        public override bool Execute(Npc npc, Character character)
        {
            bool flag;
            if (!base.Execute(npc, character))
            {
                flag = false;
            }
            else
            {
                if (character.Kamas < (int)this.KamasQuantitie)
                {
                    character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 82);
                    return false;
                }

                character.Inventory.SubKamas((int)this.KamasQuantitie);
                character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 46, KamasQuantitie);

                flag = true;
            }

            return flag;
        }
    }
}
