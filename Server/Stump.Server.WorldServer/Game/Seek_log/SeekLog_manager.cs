using Stump.Server.BaseServer.Database;
using System.Collections.Generic;
using System.Linq;
using Stump.Server.BaseServer.Initialization;
using System;
using Stump.Server.WorldServer.Database;
using Database.Kolilog;
using Database.Seeklog;

namespace Stump.Server.WorldServer.Game.Seeklog
{
    class SeekLog_manager : DataManager<SeekLog_manager>, ISaveable
    {
        private List<seek_logRecord> _records = new List<seek_logRecord>();
        readonly object m_lock = new object();

        [Initialization(InitializationPass.Seventh)]
        public override void Initialize()
        {
            _records = Database.Fetch<seek_logRecord>("select * from characters_seek_logs");
            World.Instance.RegisterSaveableInstance(this);
        }

        public List<seek_logRecord> GetHardwareRecord(string hardware)
        {
            return _records.Where(x => x.Hardware_own == hardware && DateTime.Now - x.Time <= TimeSpan.FromSeconds(21600)).ToList();
        }

        public List<seek_logRecord> GetIpRecords(string ip)
        {
            return _records.Where(x => x.Ip_own == ip && DateTime.Now - x.Time <= TimeSpan.FromSeconds(21600)).ToList();
        }

        public void AddRecord(seek_logRecord record)
        {
            _records.Add(record);
        }

        public void DeleteRecord(seek_logRecord record)
        {
            _records.Remove(record);
        }

        public void Save()
        {
            lock (m_lock)
            {
                foreach (var log in _records.Where(record => record.IsNew == true).ToList())
                {
                    if (log == null)
                        continue;

                    Database.Save(log);
                    log.IsNew = false;
                }
            }
        }
    }
}
