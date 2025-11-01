using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Stump.Server.BaseServer;
using Stump.Server.BaseServer.Database;
using Stump.Server.BaseServer.Initialization;
using Stump.Server.WorldServer.Database.Misc.Notify;

namespace Stump.Server.WorldServer.Game.Misc.Notify
{
    class NotifyManager : DataManager<NotifyManager>
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly Dictionary<long, NotifyRecord> m_notify = new Dictionary<long, NotifyRecord>();

        [Initialization(InitializationPass.Any)]
        public override void Initialize()
        {
            foreach (var notify in Database.Query<NotifyRecord>(NotifyRelator.FetchQuery))
            {
                m_notify.Add(notify.Id, notify);
            }
        }

        public void SetNotifyMember(long accountId, string notifyMessage, string adminName)
        {
            NotifyRecord _record = new NotifyRecord()
            {
                Id = m_notify.Keys.Max() + 1,
                AccountId = accountId,
                NotifyMessage = notifyMessage,
                Active = true,
                ByAdmin = adminName,
                CreateDate = DateTime.Now,
                UpdateDate = DateTime.Now,
                IsNew = true
            };

            m_notify.Add(_record.Id, _record);
            this.Save(accountId);
        }

        public Boolean HasNotifyMessage(long accountId)
        {
            return m_notify.Any(entry => entry.Value.AccountId == accountId && entry.Value.Active);
        }

        public NotifyRecord GetNotifyMemberByAccountId(int accountId)
        {
            return m_notify.FirstOrDefault(x => x.Value.AccountId == accountId && x.Value.Active).Value ?? null;
        }

        public void SetNotifyMemberByAccountIdUpdate(NotifyRecord notify)
        {
            NotifyRecord modify = m_notify.FirstOrDefault(record => record.Value == notify).Value;
            modify.Active = false;
            modify.UpdateDate = DateTime.Now;
            modify.IsUpdated = true;
            this.Save(notify.AccountId);
        }

        private void Save(long accountId)
        {
            WorldServer.Instance.IOTaskPool.AddMessage(() =>
            {
                ORM.Database database = ServerBase<WorldServer>.Instance.DBAccessor.Database;

                try
                {
                    foreach (var notify in m_notify.Where(x => x.Value.AccountId == accountId))
                    {
                        if (notify.Value.IsNew && !notify.Value.IsUpdated)
                        {
                            database.Insert(notify.Value);
                            notify.Value.IsNew = false;
                        }
                        else if (!notify.Value.IsNew && notify.Value.IsUpdated)
                        {
                            database.Update(notify.Value);
                            notify.Value.IsUpdated = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error($"Error saving Character Notify: {ex.Message}");
                }
            });
        }
    }
}