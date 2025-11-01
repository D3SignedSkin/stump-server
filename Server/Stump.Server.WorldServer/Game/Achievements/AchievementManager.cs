using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Stump.Server.BaseServer.Database;
using Stump.Server.BaseServer.Initialization;
using Stump.Server.WorldServer.Database.Achievements;
using Stump.Server.WorldServer.Database.Monsters;
using Stump.Server.WorldServer.Game.Achievements.Criterions;

namespace Stump.Server.WorldServer.Game.Achievements
{
    public class AchievementManager : DataManager<AchievementManager>
    {
        // FIELDS
        protected readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private Dictionary<uint, AchievementRewardRecord> m_achievemenstRewards;
        private Dictionary<AchievementTemplate, AchievementCriterion> m_achievementCriterions;
        private Dictionary<uint, AchievementCategoryRecord> m_achievementsCategories;
        private Dictionary<uint, AchievementObjectiveRecord> m_achievementsObjectives;
        private Dictionary<uint, AchievementTemplate> m_achievementsTemplates;
        private Dictionary<string, AbstractCriterion> m_criterions;
        private List<HaveItemCriterion> m_haveItemCriterion;
        private Dictionary<KillBossWithChallengeCriterion, MonsterTemplate> m_killBossWithChallengeCriterion;
        private Dictionary<MonsterTemplate, KillBossCriterion> m_monsterBossCriterions;
        private Dictionary<MonsterTemplate, KillMonsterWithChallengeCriterion> m_monsterCriterions;
        private List<QuestFinishedCriterion> m_questFinishedCriterion;

        // CONSTRUCTORS
        private AchievementManager()
        { }

        // PROPERTIES
        public Dictionary<Type, AbstractCriterion> IncrementableCriterion { get; private set; }

        public LevelCriterion MinLevelCriterion => IncrementableCriterion[typeof(LevelCriterion)] as LevelCriterion;

        public JobLevelCriterion MinJobLevelCriterion => IncrementableCriterion[typeof(JobLevelCriterion)] as JobLevelCriterion;

        public AchievementPointsCriterion MinAchievementPointsCriterion => IncrementableCriterion[typeof(AchievementPointsCriterion)] as AchievementPointsCriterion;

        public CraftCountCriterion CraftCountCriterion => IncrementableCriterion[typeof(CraftCountCriterion)] as CraftCountCriterion;

        public DecraftCountCriterion DecraftCountCriterion => IncrementableCriterion[typeof(DecraftCountCriterion)] as DecraftCountCriterion;

        public ChallengeCountCriterion ChallengeCountCriterion => IncrementableCriterion[typeof(ChallengeCountCriterion)] as ChallengeCountCriterion;

        public ChallengeInDungeonCountCriterion ChallengeInDungeonCountCriterion => IncrementableCriterion[typeof(ChallengeInDungeonCountCriterion)] as ChallengeInDungeonCountCriterion;

        public QuestFinishedCountCriterion QuestFinishedCountCriterion => IncrementableCriterion[typeof(QuestFinishedCountCriterion)] as QuestFinishedCountCriterion;

        public UnknowCriterion UnknowCriterion => IncrementableCriterion[typeof(UnknowCriterion)] as UnknowCriterion;


        // METHODS
        [Initialization(InitializationPass.Sixth)]
        public override void Initialize()
        {
            base.Initialize();

            m_criterions = new Dictionary<string, AbstractCriterion>();
            m_monsterCriterions = new Dictionary<MonsterTemplate, KillMonsterWithChallengeCriterion>();
            m_monsterBossCriterions = new Dictionary<MonsterTemplate, KillBossCriterion>();
            m_killBossWithChallengeCriterion = new Dictionary<KillBossWithChallengeCriterion, MonsterTemplate>();
            m_questFinishedCriterion = new List<QuestFinishedCriterion>();
            m_haveItemCriterion = new List<HaveItemCriterion>();
            m_achievementCriterions = new Dictionary<AchievementTemplate, AchievementCriterion>();

            IncrementableCriterion = new Dictionary<Type, AbstractCriterion>();
            m_achievementsTemplates = Database.Query<AchievementTemplate>(AchievementTemplateRelator.FetchQuery).ToDictionary(entry => entry.Id);
            m_achievemenstRewards = Database.Query<AchievementRewardRecord>(AchievementRewardRelator.FetchQuery).ToDictionary(entry => entry.Id);
            m_achievementsCategories = Database.Query<AchievementCategoryRecord>(AchievementCategoryRelator.FetchQuery).ToDictionary(entry => entry.Id);
            m_achievementsObjectives = Database.Query<AchievementObjectiveRecord>(AchievementObjectiveRelator.FetchQuery).ToDictionary(entry => entry.Id);

            foreach (var pair in m_achievementsTemplates)
            {
                pair.Value.Initialize();
            }

            foreach (var pair in m_achievementsObjectives)
            {
                pair.Value.Initialize();
            }
        }

        public IEnumerable<ushort> GetAchievementsIds()
        {
            return m_achievementsTemplates.Keys.Select(entry => (ushort)entry);
        }

        public IEnumerable<ushort> GetAchievementsIdsByCategory(uint category)
        {
            return m_achievementsTemplates.Where(entry => entry.Value.CategoryId == category).Select(entry => (ushort)entry.Key);
        }

        public void AddCriterion(AbstractCriterion criterion)
        {
            if (criterion == null)
                _logger.Error("O argumento 'criterion' não pode ser nulo.");

            try
            {
                if (!m_criterions.ContainsKey(criterion.Criterion))
                {
                    m_criterions.Add(criterion.Criterion, criterion);

                    if (criterion.IsIncrementable)
                    {
                        AddIncrementableCriterion(criterion);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Warn($"Ocorreu uma exceção: {e.Message}");
            }
        }

        public bool AddAchievementCriterion(AchievementCriterion criterion)
        {
            bool result;

            if (m_achievementCriterions.ContainsKey(criterion.Achievement))
            {
                result = false;
            }
            else
            {
                m_achievementCriterions.Add(criterion.Achievement, criterion);
                result = true;
            }

            return result;
        }

        public bool AddKillMonsterWithChallengeCriterion(KillMonsterWithChallengeCriterion criterion)
        {
            bool result;

            if (m_monsterCriterions.ContainsKey(criterion.Monster))
            {
                result = false;
            }
            else
            {
                m_monsterCriterions.Add(criterion.Monster, criterion);
                result = true;
            }

            return result;
        }

        public bool AddKillBossCriterion(KillBossCriterion criterion)
        {
            bool result;

            if (m_monsterBossCriterions.ContainsKey(criterion.Monster))
            {
                result = false;
            }
            else
            {
                m_monsterBossCriterions.Add(criterion.Monster, criterion);
                result = true;
            }

            return result;
        }

        public bool AddKillBossWithChallengeCriterion(KillBossWithChallengeCriterion criterion)
        {
            bool result;

            if (m_killBossWithChallengeCriterion.ContainsKey(criterion))
            {
                result = false;
            }
            else
            {
                m_killBossWithChallengeCriterion.Add(criterion, criterion.Monster);
                result = true;
            }

            return result;
        }

        public bool AddQuestFinishedCriterion(QuestFinishedCriterion criterion)
        {
            bool result;

            if (m_questFinishedCriterion.Contains(criterion))
            {
                result = false;
            }
            else
            {
                m_questFinishedCriterion.Add(criterion);
                result = true;
            }

            return result;
        }

        public bool AddhaveItemCriterion(HaveItemCriterion criterion)
        {
            bool result;

            if (m_haveItemCriterion.Contains(criterion))
            {
                result = false;
            }
            else
            {
                m_haveItemCriterion.Add(criterion);
                result = true;
            }

            return result;
        }

        private bool AddIncrementableCriterion(AbstractCriterion criterion)
        {
            var criterionType = criterion.GetType();

            if (!IncrementableCriterion.ContainsKey(criterionType))
            {
                IncrementableCriterion.Add(criterionType, criterion);
            }
            else
            {
                var min = IncrementableCriterion[criterionType];

                if (min < criterion)
                {
                    var temp = min;
                    var next = min.Next;

                    while (next != null && next < criterion)
                    {
                        temp = next;
                        next = temp.Next;
                    }

                    if (next == null)
                    {
                        temp.Next = criterion;
                    }
                    else
                    {
                        criterion.Next = next;
                        temp.Next = criterion;
                    }
                }
                else
                {
                    criterion.Next = min;
                    IncrementableCriterion[criterionType] = criterion;
                }
            }

            return true;
        }

        public AchievementCriterion TryGetAchievementCriterion(AchievementTemplate achievement)
        {
            return m_achievementCriterions.ContainsKey(achievement) ? m_achievementCriterions[achievement] : null;
        }

        public bool TryGetAbstractCriterion(string criterion, out AbstractCriterion result)
        {
            result = null;

            if (m_criterions.ContainsKey(criterion))
            {
                result = m_criterions[criterion];
                return true;
            }

            return false;
        }

        public AchievementTemplate TryGetAchievement(uint id)
        {
            if (m_achievementsTemplates is null)
                return null;

            return m_achievementsTemplates.ContainsKey(id) ? m_achievementsTemplates[id] : null;
        }

        public AchievementCategoryRecord TryGetAchievementCategory(uint id)
        {
            if (m_achievementsCategories is null)
                return null;

            return m_achievementsCategories.ContainsKey(id) ? m_achievementsCategories[id] : null;
        }

        public AchievementObjectiveRecord TryGetAchievementObjective(uint id)
        {
            if (m_achievementsObjectives is null)
                return null;

            return m_achievementsObjectives.ContainsKey(id) ? m_achievementsObjectives[id] : null;
        }

        public AchievementRewardRecord TryGetAchievementReward(uint id)
        {
            if (m_achievemenstRewards is null)
                return null;

            return m_achievemenstRewards.ContainsKey(id) ? m_achievemenstRewards[id] : null;
        }

        public KillMonsterWithChallengeCriterion TryGetCriterionByMonster(MonsterTemplate template)
        {
            return m_monsterCriterions.ContainsKey(template) ? m_monsterCriterions[template] : null;
        }

        public KillBossCriterion TryGetCriterionByBoss(MonsterTemplate template)
        {
            return m_monsterBossCriterions.ContainsKey(template) ? m_monsterBossCriterions[template] : null;
        }

        public QuestFinishedCriterion TryGetQuestFinishedCriterionByQuestId(int id)
        {
            return m_questFinishedCriterion.FirstOrDefault(x => x.QuestId == id);
        }

        public HaveItemCriterion TryGetHaveItemCriterionByItemId(int id)
        {
            return m_haveItemCriterion.FirstOrDefault(x => x.ItemId == id);
        }

        public IEnumerable<KillBossWithChallengeCriterion> TryGetCriterionsByBossWithChallenge(MonsterTemplate template)
        {
            return m_killBossWithChallengeCriterion.ContainsValue(template) ? m_killBossWithChallengeCriterion.Where(x => x.Value == template).Select(y => y.Key) : null;
        }
    }
}