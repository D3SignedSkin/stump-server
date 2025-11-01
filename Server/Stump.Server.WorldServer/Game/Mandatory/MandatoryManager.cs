using Stump.Server.BaseServer.Database;
using System.Collections.Generic;
using System.Linq;
using Stump.Server.BaseServer.Initialization;
using System;
using Database.Mandatory;

namespace Stump.Server.WorldServer.Game.Mandatory
{
    class MandatoryManager : DataManager<MandatoryManager>
    {
        private List<MandatoryRecord> _records = new List<MandatoryRecord>();

        public static int MinimalDelaySeconds = 5;

        [Initialization(InitializationPass.Seventh)]
        public override void Initialize()
        {
            _records = Database.Fetch<MandatoryRecord>("select * from characters_mandatory");
            CheckRecordOld();
        }

        public List<MandatoryRecord> GetCharacterRecords(int characterId)
        {
            return _records.Where(x => x.OwnerId == characterId).ToList();
        }

        public List<MandatoryRecord> GetCharacterRecords(string ip)
        {
            return _records.Where(x => x.Ip == ip).ToList();
        }

        public void AddRecord(MandatoryRecord record)
        {
            _records.Add(record);
        }

        public void DeleteRecord(MandatoryRecord record)
        {
            try 
            {
                World.Instance.Database.Execute($"DELETE FROM characters_mandatory WHERE characters_mandatory.Id = {record.Id} AND characters_mandatory.OwnerId = {record.OwnerId}");
            }
            catch { }
        }

        private void CheckRecordOld()
        {
            foreach (MandatoryRecord MandDel in _records)
            {
                if (DateTime.Now.Subtract(MandDel.Time).Days > 2)
                {
                    DeleteRecord(MandDel);
                }
            }
        }
    }
}
