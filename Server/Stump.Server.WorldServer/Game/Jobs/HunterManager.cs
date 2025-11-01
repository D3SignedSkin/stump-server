using Stump.Server.BaseServer.Database;
using Stump.Server.BaseServer.Initialization;
using Stump.Server.WorldServer.Database.Jobs;
using System.Collections.Generic;
using System.Linq;

namespace Stump.Server.WorldServer.Game.Jobs
{
    class HunterManager : DataManager<HunterManager>
    {
        private Dictionary<int, HunterRecord> m_hunterInfos;

        [Initialization(InitializationPass.Fourth)]
        public override void Initialize()
        {
            #region >> Limpando as variaveis para que o comando Reload não duplique elas.

            if (m_hunterInfos != null)
                m_hunterInfos.Clear();

            #endregion

            m_hunterInfos = Database.Query<HunterRecord>(HunterRelator.FetchQuery).ToDictionary(entry => entry.MonsterId);
        }

        public bool DropExist(int MonsterTemplate)
        {
            HunterRecord drop_exist;
            m_hunterInfos.TryGetValue(MonsterTemplate, out drop_exist);

            if (drop_exist != null)
                return true;
            else
                return false;
        }

        public int ItemId(int MonsterTemplate)
        {
            return m_hunterInfos[MonsterTemplate].DropId;
        }

        public int Level(int MonsterTemplate)
        {
            return m_hunterInfos[MonsterTemplate].Level;
        }
    }
}
