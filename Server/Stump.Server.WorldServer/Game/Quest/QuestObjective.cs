using System;
using Stump.DofusProtocol.Types;
using Stump.Server.WorldServer.Database.Quests;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using System.Linq;
using Stump.Server.WorldServer.Handlers.Context.RolePlay;
using MongoDB.Driver;
using NLog;

namespace Stump.Server.WorldServer.Game.Quests
{
    public abstract class QuestObjective
    {
        private new readonly Logger logger = LogManager.GetCurrentClassLogger();

        public event Action<QuestObjective> Completed;
        public abstract QuestObjectiveInformations GetQuestObjectiveInformations();

        public abstract void EnableObjective();
        public abstract void DisableObjective();
        public abstract bool CanSee();
        public abstract int Completion();

        public QuestObjective(Character character, QuestObjectiveTemplate template, bool finished)
        {
            Character = character;
            Template = template;

            var questRecord = character.m_questsRecord.FirstOrDefault(x => x.QuestId == StepTemplate.QuestId);
            var objectivesRecord = character.m_questsObjectiveRecord.Where(x => x.QuestId == StepTemplate.QuestId);

            bool hasQuest = questRecord != null && character.Quests.Any(x => x.CurrentStep.Quest.Id == StepTemplate.QuestId && x.CurrentStep.Quest.Finished);
            bool hasQuestObjective = objectivesRecord != null && objectivesRecord.Any(x => x.QuestId == StepTemplate.QuestId && x.Status && x.Completion == 1);

            Finished = questRecord != null && objectivesRecord != null ? false : finished;

            if (hasQuest && hasQuestObjective)
            {
                ObjectiveRecord = new QuestObjectiveStatus()
                {
                    Id = character.m_questsObjectiveRecord.FirstOrDefault(x => x.QuestId == StepTemplate.QuestId && x.ObjectiveId == Template.Id).Id,
                    QuestId = StepTemplate.QuestId,
                    ObjectiveId = Template.Id,
                    Status = Finished,
                    OwnerId = character.Id,
                    Completion = 0,
                    IsNew = false,
                    IsUpdated = true,
                };

                int index = character.m_questsObjectiveRecord.IndexOf(ObjectiveRecord);

                if (index >= 0)
                    character.m_questsObjectiveRecord[index] = ObjectiveRecord;
            }
            else
            {
                ObjectiveRecord = new QuestObjectiveStatus()
                {
                    QuestId = StepTemplate.QuestId,
                    ObjectiveId = Template.Id,
                    Status = Finished,
                    OwnerId = character.Id,
                    Completion = 0,
                    IsNew = true,
                    IsUpdated = false
                };
            }
        }

        public QuestObjective(Character character, QuestObjectiveTemplate template, QuestObjectiveStatus record)
        {
            Character = character;
            Template = template;
            ObjectiveRecord = record;
            Finished = record.Status;
        }

        public QuestObjectiveStatus ObjectiveRecord
        {
            get;
            set;
        }

        public QuestObjectiveTemplate Template
        {
            get;
        }

        public bool Finished
        {
            get;
            set;
        }

        public Character Character
        {
            get;
            set;
        }

        public QuestStepTemplate StepTemplate
        {
            get
            {
                return QuestManager.Instance.GetQuestStep(Template.StepId);
            }
        }

        public void CompleteObjective()
        {
            OnCompleted();
        }

        protected virtual void OnCompleted()
        {
            DisableObjective();
            ObjectiveRecord.Status = true;
            ObjectiveRecord.IsUpdated = true;
            Finished = true;
            Completed?.Invoke(this);
            ContextRoleplayHandler.SendRefreshMapQuestWithout(Character.Client, QuestManager.Instance.GetQuestTemplateWithStepId(Template.StepId).Id);
            ContextRoleplayHandler.SendQuestStepValidatedMessage(Character.Client, (ushort)QuestManager.Instance.GetQuestTemplateWithStepId(Template.StepId).Id, (ushort)Template.StepId);
        }

        #region >> World Save
        public void Save(ORM.Database database)
        {
            WorldServer.Instance.IOTaskPool.ExecuteInContext(() =>
            {
                try
                {
                    if (ObjectiveRecord.IsUpdated && !ObjectiveRecord.IsNew)
                    {
                        database.Update(ObjectiveRecord);
                        ObjectiveRecord.IsUpdated = false;
                    }
                    else if (ObjectiveRecord.IsNew)
                    {
                        Character.m_questsObjectiveRecord.Add(ObjectiveRecord);
                        database.Insert(ObjectiveRecord);
                        ObjectiveRecord.IsNew = false;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error($"Error saving Character QuestObjective: {ex.Message}");
                }
            });
        }
        #endregion
    }
}