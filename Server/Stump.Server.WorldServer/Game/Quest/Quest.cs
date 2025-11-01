using System.Linq;
using NLog;
using Stump.DofusProtocol.Types;
using Stump.Server.WorldServer.Database.Quests;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using System.Collections.Generic;
using MongoDB.Driver;
using System;

namespace Stump.Server.WorldServer.Game.Quests
{
    public class Quest
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private QuestRecord m_record;

        public ushort Id => (ushort)Template.Id;

        public Character Owner
        {
            get;
            private set;
        }

        public QuestTemplate Template
        {
            get;
            private set;
        }

        public QuestStep CurrentStep
        {
            get;
            private set;
        }

        public bool Finished
        {
            get
            {
                return m_record.Finished;
            }
            set { m_record.Finished = value; }
        }

        public bool IsNew
        {
            get
            {
                return m_record.IsNew;
            }
            set { m_record.IsNew = value; }
        }

        public bool IsUpdated
        {
            get
            {
                return m_record.IsUpdated;
            }
            set { m_record.IsUpdated = value; }
        }

        private List<QuestObjectiveStatus> ObjectivesStatus
        {
            get { return m_record.Objectives; }
        }

        public Quest(Character owner, QuestRecord record)
        {
            m_record = record;
            Owner = owner;
            Template = QuestManager.Instance.GetQuestTemplate(record.QuestId);

            if (Template == null)
            {
                logger.Error($"Quest id {record.QuestId} doesn't exist");
                return;
            }

            CurrentStep = new QuestStep(this, QuestManager.Instance.GetQuestStep(record.StepId), ObjectivesStatus);
        }

        public Quest(Character owner, QuestStepTemplate step, bool IsUpdate)
        {
            if (IsUpdate)
            {
                m_record = new QuestRecord()
                {
                    Id = owner.m_questsRecord.FirstOrDefault(x => x.QuestId == step.QuestId).Id,
                    Finished = false,
                    QuestId = step.QuestId,
                    StepId = step.Id,
                    OwnerId = owner.Id,
                    IsNew = false,
                    IsUpdated = true
                };

                int index = owner.m_questsRecord.IndexOf(m_record);

                if (index >= 0)
                    owner.m_questsRecord[index] = m_record;
            }
            else
            {
                m_record = new QuestRecord()
                {
                    Finished = false,
                    QuestId = step.QuestId,
                    StepId = step.Id,
                    OwnerId = owner.Id,
                    IsNew = true,
                    IsUpdated = false
                };
            }

            Template = QuestManager.Instance.GetQuestTemplate(step.QuestId);
            Owner = owner;
            CurrentStep = new QuestStep(this, step);
        }

        public void ChangeQuestStep(QuestStepTemplate step)
        {
            CurrentStep?.CancelQuest();
            CurrentStep = new QuestStep(this, step);
            m_record.StepId = step.Id;
            m_record.IsUpdated = true;
        }

        public QuestActiveInformations GetQuestActiveInformations()
        {
            return new QuestActiveDetailedInformations(Id, (ushort)CurrentStep.Id, CurrentStep.Objectives.Select(x => x.GetQuestObjectiveInformations()).ToArray());
        }

        #region >> World Save
        public void Save(ORM.Database database)
        {
            WorldServer.Instance.IOTaskPool.ExecuteInContext(() =>
            {
                try
                {
                    if (m_record.IsUpdated && !m_record.IsNew)
                    {
                        database.Update(m_record);
                        m_record.IsUpdated = false;

                        CurrentStep.Save(database);
                    }
                    else if (m_record.IsNew)
                    {
                        Owner.m_questsRecord.Add(m_record);
                        database.Insert(m_record);
                        m_record.IsNew = false;

                        CurrentStep.Save(database);
                    }     
                }
                catch (Exception ex)
                {
                    logger.Error($"Error saving Character Quest: {ex.Message}");
                }
            });
        }
        #endregion
    }
}