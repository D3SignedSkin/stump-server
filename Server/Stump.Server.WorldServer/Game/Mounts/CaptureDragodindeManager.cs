using Stump.Server.BaseServer.Database;
using Stump.Server.BaseServer.Initialization;
using Stump.Server.WorldServer.Database.Items.Templates;
using Stump.Server.WorldServer.Database.Mounts.Capture;
using System.Collections.Generic;
using System.Linq;

namespace Stump.Server.WorldServer.Game.Mounts
{
    class CaptureDragodindeManager : DataManager<CaptureDragodindeManager>
    {
        private Dictionary<int, DragodindeRecord> m_dragodinde;

        [Initialization(InitializationPass.Fourth)]
        public override void Initialize()
        {
            #region >> Limpando as variaveis para que o comando Reload não duplique elas.

            if (m_dragodinde != null)
                m_dragodinde.Clear();

            #endregion

            m_dragodinde = Database.Query<DragodindeRecord>(DragodindeRelator.FetchQuery).ToDictionary(entry => entry.MonsterId);
        }

        public ItemTemplate GetDrop(int MonsterTemplate)
        {
            return m_dragodinde[MonsterTemplate].ItemTemplate;
        }

        public bool MountExist(int MonsterTemplate)
        {
            DragodindeRecord dragodinde_exist;
            m_dragodinde.TryGetValue(MonsterTemplate, out dragodinde_exist);

            if (dragodinde_exist != null)
                return true;
            else
                return false;
        }

        public int ItemId(int MonsterTemplate)
        {
            return m_dragodinde[MonsterTemplate].DropId;
        }

        public float TauxDrop(int MonsterTemplate)
        {
            return m_dragodinde[MonsterTemplate].TauxDrop;
        }
    }
}