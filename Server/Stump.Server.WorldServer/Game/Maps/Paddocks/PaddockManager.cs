using System.Collections.Generic;
using System.Linq;
using Stump.Server.BaseServer.Database;
using Stump.Server.BaseServer.Initialization;
using Stump.Server.WorldServer.Database.Mounts;
using Stump.Server.WorldServer.Database.World;

namespace Stump.Server.WorldServer.Game.Maps.Paddocks
{
    public class PaddockManager : DataManager<PaddockManager>
    {
        private Dictionary<int, Paddock> m_paddocks = new Dictionary<int, Paddock>();

        [Initialization(InitializationPass.Eighth)]
        public override void Initialize()
        {
            m_paddocks = Database.Query<WorldMapPaddockRecord, MountRecord, WorldMapPaddockRecord>(new WorldMapPaddockRelator().Map, WorldMapPaddockRelator.FetchQueryMaps).Where(x => x.Map != null).ToDictionary(entry => entry.Id, x => new Paddock(x));
        }

        public Paddock GetPaddock(int id)
        {
            Paddock paddock;
            return m_paddocks.TryGetValue(id, out paddock) ? paddock : null;
        }

        public Paddock GetPaddockByMap(long mapId)
        {
            return m_paddocks.Values.FirstOrDefault(x => x.Map.Id == mapId);
        }

        public Paddock[] GetPaddockByGuild(int guildId)
        {
            return m_paddocks.Values.Where(x => x.Guild.Id == guildId).ToArray();
        }

        public Paddock[] GetPaddocks()
        {
            return m_paddocks.Values.ToArray();
        }
    }
}
