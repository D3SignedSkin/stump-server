using Stump.Server.BaseServer.Database;
using System.Collections.Generic;
using System.Linq;
using Stump.Server.BaseServer.Initialization;
using System;
using Database.Kolilog;
using Stump.Server.WorldServer.Database;

namespace Stump.Server.WorldServer.Game.KoliLog
{
    class KoliLog_manager : DataManager<KoliLog_manager>, ISaveable
    {
        private List<koli_logRecord> _records = new List<koli_logRecord>();
        readonly object m_lock = new object();

        [Initialization(InitializationPass.Seventh)]
        public override void Initialize()
        {
            _records = Database.Fetch<koli_logRecord>("select * from koli_log");
            World.Instance.RegisterSaveableInstance(this);
        }

        public List<koli_logRecord> GetHardwareRecord(string hardware)
        {
            return _records.Where(x => x.Hardware_own == hardware && DateTime.Now - x.Time <= TimeSpan.FromSeconds(21600)).ToList();
        }

        public List<koli_logRecord> GetIpRecords(string ip)
        {
            return _records.Where(x => x.Ip_own == ip && DateTime.Now - x.Time <= TimeSpan.FromSeconds(21600)).ToList();
        }

        public void AddRecord(koli_logRecord record)
        {
            _records.Add(record);
        }

        public void DeleteRecord(koli_logRecord record)
        {
            _records.Remove(record);
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
