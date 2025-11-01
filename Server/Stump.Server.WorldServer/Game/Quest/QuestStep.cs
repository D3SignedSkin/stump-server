using System;
using System.Linq;
using Stump.DofusProtocol.Types;
using Stump.Server.WorldServer.Database.Quests;
using Stump.DofusProtocol.Enums;
using System.Collections.Generic;

namespace Stump.Server.WorldServer.Game.Quests
{
    public class QuestStep
    {
        public event Action<QuestStep> Finished;

        public int Id => Template.Id;

        public Quest Quest
        {
            get;
            set;
        }

        public QuestStepTemplate Template
        {
            get;
            set;
        }

        public List<QuestObjective> Objectives
        {
            get;
            set;
        }

        public QuestReward[] Rewards
        {
            get;
            set;
        }

        public QuestStep(Quest quest, QuestStepTemplate template)
        {
            Quest = quest;
            Template = template;
            Objectives = template.Objectives.Where(x => !x.IsTriggeredByObjective).Select(x => x.GenerateObjective(Quest.Owner)).ToList();
            Rewards = template.Rewards.Select(x => new QuestReward(x)).ToArray();

            foreach (var objective in Objectives)
            {
                if (!objective.Finished)
                {
                    objective.Completed += OnObjectiveCompleted;
                    objective.EnableObjective();
                }
            }
        }

        public QuestStep(Quest quest, QuestStepTemplate template, List<QuestObjectiveStatus> status)
        {
            Quest = quest;
            Template = template;
            Objectives = status.Where(w => QuestManager.Instance.GetObjectiveTemplate(w.ObjectiveId).StepId == Id).Select(x => QuestManager.Instance.GetObjectiveTemplate(x.ObjectiveId).GenerateObjective(Quest.Owner, x)).ToList();
            Rewards = template.Rewards.Select(x => new QuestReward(x)).ToArray();

            foreach (var objective in Objectives)
            {
                if (!objective.Finished)
                {
                    objective.Completed += OnObjectiveCompleted;
                    objective.EnableObjective();
                }
            }
        }

        private void OnObjectiveCompleted(QuestObjective obj)
        {
            if (obj.Template.TriggeredByObjectiveId > 0)
            {
                var objective = QuestManager.Instance.GetObjectiveTemplate(obj.Template.TriggeredByObjectiveId).GenerateObjective(Quest.Owner);
                Objectives.Add(objective);
                objective.Completed += OnObjectiveCompleted;
                objective.EnableObjective();
            }

            Quest.Owner.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 55, Quest.Template.Id);

            if (Objectives.All(x => x.ObjectiveRecord.Status))
            {
                if (Quest.Template.Steps.Last() == Template)
                    FinishQuest();
                else
                    Quest.ChangeQuestStep(Quest.Template.Steps[Quest.Template.Steps.ToList().IndexOf(Template) + 1]);
            }
        }

        public void FinishQuest()
        {
            OnFinished();
            Quest.Finished = true;
            Quest.IsUpdated = true;
            Quest.Owner.QuestCompleted(Quest);
            Quest.Owner.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 56, Quest.Id);
        }

        public void CancelQuest()
        {
            OnFinished();
        }

        private void GiveRewards()
        {
            foreach (var reward in Rewards)
            {
                bool levelCheck = reward.Template.LevelMin != -1 && reward.Template.LevelMax != -1;

                if (levelCheck)
                {
                    int playerLevel = Quest.Owner.Level > 200 ? 200 : Quest.Owner.Level;

                    if (reward.Template.LevelMin <= playerLevel && playerLevel <= reward.Template.LevelMax)
                    {
                        reward.GiveReward(Quest.Owner);
                    }
                }
                else
                {
                    reward.GiveReward(Quest.Owner);
                }
            }
        }

        public QuestActiveInformations GetQuestActiveInformations()
        {
            return new QuestActiveDetailedInformations((ushort)Quest.Id, (ushort)Id, Objectives.Select(x => x.GetQuestObjectiveInformations()).ToArray());
        }

        protected virtual void OnFinished()
        {
            foreach (var objective in Objectives)
            {
                objective.Completed -= OnObjectiveCompleted;
                objective.DisableObjective();
            }

            Finished?.Invoke(this);

            #region >> Rewards Player
            GiveRewards();

            if (CalculateWonKamas() > 0)
            {
                var winKamas = (long)CalculateWonKamas();

                Quest.Owner.Inventory.AddKamas(winKamas);
                Quest.Owner.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 45, winKamas);
            }

            if (CalculateWonXp() > 0)
            {
                var winXp = CalculateWonXp();

                Quest.Owner.AddExperience(winXp);
                Quest.Owner.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 8, winXp);
            }
            #endregion

            Quest.Owner.RefreshStats();
        }

        private double CalculateWonKamas()
        {
            int level = Quest.Owner.Level;

            var questRewardTemplate = Template.Rewards.FirstOrDefault(x => x.StepId == this.Id);

            if (questRewardTemplate?.KamasScaleWithPlayerLevel ?? false)
            {
                level = Template.OptimalLevel;
            }

            return Math.Floor((Math.Pow(level, 2) + 20 * level - 20) * (questRewardTemplate?.KamasRatio ?? 1) * Template.Duration);
        }

        private double CalculateWonXp()
        {
            double xpRatio = Template.Rewards.Any(x => x.StepId == this.Id) ? Template.Rewards.First(x => x.StepId == this.Id)?.ExperienceRatio ?? 0 : 0;

            if (Quest.Owner.Level > Template.OptimalLevel)
            {
                double rewardLevel = Math.Min(Quest.Owner.Level, Template.OptimalLevel * 0.7);
                double optimalLevelExperienceReward = GetFixedExperienceReward(Template.OptimalLevel, xpRatio);
                double levelExperienceReward = GetFixedExperienceReward((int)rewardLevel, xpRatio);
                double reducedOptimalExperienceReward = (1 - 0.7) * optimalLevelExperienceReward;
                double reducedExperienceReward = 0.7 * levelExperienceReward;
                double sumExperienceRewards = Math.Floor(reducedOptimalExperienceReward + reducedExperienceReward);

                return Math.Floor(sumExperienceRewards * 2.25);
            }
            else
            {
                return Math.Floor(GetFixedExperienceReward(Quest.Owner.Level, xpRatio));
            }
        }

        private double GetFixedExperienceReward(int level, double xpRatio)
        {
            return Math.Pow(level, 2) + 20 * level - 20 * xpRatio;
        }

        public void Save(ORM.Database database)
        {
            foreach (var objective in Objectives)
            {
                objective.Save(database);
            }
        }
    }
}