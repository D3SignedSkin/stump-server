using Stump.Server.BaseServer.Database;
using System.Collections.Generic;
using System.Linq;
using Stump.Server.BaseServer.Initialization;
using Stump.Server.WorldServer.Database;
using Stump.Server.WorldServer.Database.Arena;

namespace Stump.Server.WorldServer.Game.Arena
{
    class ArenaChampsManager : DataManager<ArenaChampsManager>, ISaveable
    {
        private List<ArenaChampionship> _records = new List<ArenaChampionship>();
        readonly object m_lock = new object();

        [Initialization(InitializationPass.Seventh)]
        public override void Initialize()
        {
            _records = Database.Fetch<ArenaChampionship>("SELECT * FROM arena_championship");
            World.Instance.RegisterSaveableInstance(this);
        }

        public void AddRecord(ArenaChampionship record)
        {
            _records.Add(record);
        }

        public void DeleteRecord(ArenaChampionship record)
        {
            _records.Remove(record);
        }

        public List<ArenaChampionship> GetAllRecord()
        {
            return _records.ToList();
        }

        public List<ArenaChampionship> GetOwnerRecord(int OwnerId)
        {
            return _records.Where(x => x.OwnerId == OwnerId).ToList();
        }

        public List<ArenaChampionship> GetIpOwnerRecord(string Ip)
        {
            return _records.Where(x => x.Ip == Ip).ToList();
        }

        public void Save()
        {
            lock (m_lock)
            {
                foreach (var log in _records.Where(record => record.IsNew == true).ToList())
                {
                    Database.Save(log);
                    log.IsNew = false;
                }
            }
        }
    }
}
