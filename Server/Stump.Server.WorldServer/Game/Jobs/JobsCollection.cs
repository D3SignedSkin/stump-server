using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;

namespace Stump.Server.WorldServer.Game.Jobs
{
    public class JobsCollection : IEnumerable<Job>
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        private Dictionary<int, Job> m_jobs = new Dictionary<int, Job>();

        public JobsCollection(Character owner)
        {
            Owner = owner;
        }

        public Character Owner
        {
            get;
            private set;
        }

        public void LoadJobs()
        {
            m_jobs = JobManager.Instance.GetCharacterJobs(Owner.Id).ToDictionary(x => x.TemplateId, x => new Job(Owner, x));

            foreach (var job in JobManager.Instance.EnumerateJobTemplates().Where(x => !m_jobs.ContainsKey(x.Id)).ToArray())
            {
                m_jobs.Add(job.Id, new Job(Owner, job));
            }

            var weightBonus = JobManager.Instance.GetWeightBonus(m_jobs.Count, m_jobs.Sum(x => x.Value.Level));
            Owner.Stats[DofusProtocol.Enums.PlayerFields.Weight].Base += weightBonus;
        }

        public Job this[int templateId] => m_jobs[templateId];

        public IEnumerator<Job> GetEnumerator() => m_jobs.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #region >> World Save
        public void Save(ORM.Database database)
        {
            try
            {
                foreach (var job in m_jobs.Values)
                {
                    job.Save(database);
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error saving JobCollection: {ex.Message}");
            }
        }
        #endregion
    }
}