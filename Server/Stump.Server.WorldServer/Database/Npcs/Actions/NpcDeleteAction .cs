using System;
using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Database;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Npcs;
using Stump.Server.WorldServer.Game.Exchanges;

namespace Stump.Server.WorldServer.Database.Npcs.Actions
{
    [Discriminator(Discriminator, typeof(NpcActionDatabase), new System.Type[]
	{
		typeof(NpcActionRecord)
	})]
    public class NpcDeleteAction : NpcActionDatabase
    {
        public const string Discriminator = "ItensDelete";
        public override NpcActionTypeEnum[] ActionType => new NpcActionTypeEnum[] { NpcActionTypeEnum.ACTION_DELETE_ITEMS };

        public int Kamas
        {
            get
            {
                return base.Record.GetParameter<int>(0u, false);
            }
            set
            {
                base.Record.SetParameter<int>(0u, value);
            }
        }

        public NpcDeleteAction(NpcActionRecord record)
            : base(record)
		{
		}

		public override void Execute(Npc npc, Character character)
		{
            NpcDelete npcDialog = new NpcDelete(character, npc, Kamas);
			npcDialog.Open();
		}
    }
}
