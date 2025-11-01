using System.Collections.Generic;
using Stump.Server.BaseServer.Database;
using Stump.Server.BaseServer.Initialization;
using Stump.Server.WorldServer.Database.MapsReset;
using NLog;
using Stump.Server.WorldServer.Core.Network;

namespace Stump.Server.WorldServer.Game.MapsReset
{
    class MapsResetManager : DataManager<MapsResetManager>
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly Dictionary<int, MapsResetModal> m_mapsreset = new Dictionary<int, MapsResetModal>();

        [Initialization(InitializationPass.Any)]
        public override void Initialize()
        {
            foreach (
                var map in Database.Query<MapsResetModal>(MapsResetModalTableRelator.FetchQuery))
            {
                m_mapsreset.Add(map.Id, map);
            }
        }

        public void ExitPlayerMap(WorldClient client)
        {
            if (GetMapByMapId(client.Character.Map.Id) != null)
            {
                uint mapTpId = 165153537;
                int mapTpCell = 131;
                client.Character.Teleport(new Maps.Cells.ObjectPosition(World.Instance.GetMap(mapTpId), World.Instance.GetMap(mapTpId).GetCell(mapTpCell), DofusProtocol.Enums.DirectionsEnum.DIRECTION_SOUTH_EAST));
            }
        }

        public MapsResetModal GetMapByMapId(long mapId)
        {
            foreach (var item in m_mapsreset)
            {
                if (item.Value.MapId == mapId)
                    return item.Value;
            }
            return null;
        }

    }
}
