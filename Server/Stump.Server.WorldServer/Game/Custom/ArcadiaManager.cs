using Stump.Server.BaseServer.Database;
using Stump.Server.BaseServer.Initialization;
using Stump.Server.WorldServer.Database.Custom;
using Stump.Server.WorldServer.Database.World;
using System.Collections.Generic;
using System.Linq;

namespace Stump.Server.WorldServer.Game.Custom
{
    public class ArcadiaManager : DataManager<ArcadiaManager>
    {
        private Dictionary<int, ArcadiaTeleportPanelRecord> m_tpArcadia =
            new Dictionary<int, ArcadiaTeleportPanelRecord>();


        [Initialization(InitializationPass.Sixth)]
        public override void Initialize()
        {
            m_tpArcadia = Database.Query<ArcadiaTeleportPanelRecord>(ArcadiaTeleportPanelRelator.FetchQuery)
                .ToDictionary(entry => entry.Id);
        }

        public IEnumerable<ArcadiaTeleportPanelRecord> GetArcadiaTeleport() => m_tpArcadia.Values;

        public ArcadiaTeleportPanelRecord TryGetArcadiaTeleport(int id) =>
            GetArcadiaTeleport().FirstOrDefault(x => x.MapId == id);
    }
}