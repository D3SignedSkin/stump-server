using Stump.Server.BaseServer.Database;
using System.Collections.Generic;
using System.Linq;
using Database.Mandatory;
using NLog;
using System;

namespace Stump.Server.WorldServer.Game.Mandatory
{
    public class MandatoryCollection : DataManager<MandatoryCollection>
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        public List<MandatoryRecord> Mandatory { get; set; }
        public List<MandatoryRecord> DeleteMandatory { get; set; }

        public void Load(int id)
        {
            Mandatory = FindByOwner(id);
            DeleteMandatory = new List<MandatoryRecord>();
        }

        public void Load(string ip)
        {
            Mandatory = FindByOwner(ip);
            DeleteMandatory = new List<MandatoryRecord>();
        }

        public List<MandatoryRecord> FindByOwner(int ownerId)
        {
            return Database.Fetch<MandatoryRecord>($"SELECT * FROM characters_mandatory WHERE OwnerId = {ownerId}");
        }

        public List<MandatoryRecord> FindByOwner(string Ip)
        {
            return Database.Fetch<MandatoryRecord>($"SELECT * FROM characters_mandatory WHERE Ip = '{Ip}'");
        }

        #region >> World Save
        public void Save(ORM.Database database)
        {
            WorldServer.Instance.IOTaskPool.AddMessage(() =>
            {
                try
                {
                    foreach (var Mandatory in DeleteMandatory)
                    {
                        if (!Mandatory.IsNew)
                            database.Delete(Mandatory);
                    }

                    DeleteMandatory.Clear();

                    foreach (var Mandatory in Mandatory.ToList())
                    {
                        if (Mandatory.IsUpdated && !Mandatory.IsNew)
                        {
                            database.Update(Mandatory);
                            Mandatory.IsUpdated = false;
                        }

                        if (Mandatory.IsNew)
                        {
                            database.Insert(Mandatory);
                            Mandatory.IsNew = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error($"Error saving Character Mandatory: {ex.Message}");
                }
            });
        }
        #endregion
    }
}