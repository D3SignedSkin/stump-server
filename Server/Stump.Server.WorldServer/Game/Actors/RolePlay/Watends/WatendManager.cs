using System.Collections.Generic;
using System.Linq;
using Stump.Server.BaseServer.Database;
using Stump.Server.BaseServer.Initialization;
using Stump.Server.WorldServer.Database.World;

namespace Stump.Server.WorldServer.Game.Actors.RolePlay.Npcs
{
    public class WatendManager : DataManager<WatendManager>
    {
        private Dictionary<int, WatendSpawn> m_watendsTemplates;

        [Initialization(InitializationPass.Fifth)]
        public override void Initialize()
        {
            m_watendsTemplates = Database.Query<WatendSpawn>(WorldMapWantedRelator.FetchQuery).ToDictionary(entry => entry.WatendId);
        }

        public IEnumerable<WatendSpawn> GetNpcWatendSpawns()
        {
            return m_watendsTemplates.Values;
        }

        public WatendSpawn GetWantendByItemId(int itemId) 
        {
            return m_watendsTemplates.FirstOrDefault(entry => entry.Value.WatendId == itemId).Value;
        }
    }
}