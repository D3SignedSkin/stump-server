using Stump.Server.BaseServer.Database;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Npcs;
using Stump.Server.WorldServer.Game.Maps;
using Stump.Server.WorldServer.Game.Maps.Cells;
using System.Linq;

namespace Stump.Server.WorldServer.Database.Npcs.Replies
{
    [Discriminator("DungeonReturn", typeof(NpcReply), typeof(NpcReplyRecord))]
    public class DungeonReturnReply : NpcReply
    {
        public DungeonReturnReply(NpcReplyRecord record) : base(record)
        { }

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

            if (character.DungeonReturn.Count <= 0)
                return false;

            long[] _dungeonReturn = character.DungeonReturn.FirstOrDefault(dung => dung != null && dung.Contains(value));

            if (_dungeonReturn is null)
                return false;

            Map _teleMap = Game.World.Instance.GetMap(_dungeonReturn[1]);

            if (_teleMap is null)
                return false;

            ObjectPosition _telePosition = new ObjectPosition(
                map: _teleMap,
                cell: _teleMap.GetRandomFreeCell(),
                direction: character.Direction);

            character.Teleport(_telePosition);

            return true;
        }
    }
}