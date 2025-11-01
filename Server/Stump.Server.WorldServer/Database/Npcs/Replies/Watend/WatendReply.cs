using Stump.Server.BaseServer.Database;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Npcs;
using Stump.Server.WorldServer.Handlers.Context.RolePlay;

namespace Stump.Server.WorldServer.Database.Npcs.Replies
{
	[Discriminator("WantedComplete", typeof(NpcReply), new System.Type[]
	{
		typeof(NpcReplyRecord)
	})]

	public class WatendReply : NpcReply
	{
        public WatendReply(NpcReplyRecord record) : base(record)
		{ }

		public override bool Execute(Npc npc, Character character)
		{
            ContextRoleplayHandler.SendMapComplementaryInformationsDataMessage(character.Client);
            return true;
		}
	}
}
