using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Database;
using Stump.Server.BaseServer.Initialization;
using Stump.Server.WorldServer.Database.Monsters;
using MonsterGrade = Stump.Server.WorldServer.Database.Monsters.MonsterGrade;
using MonsterSpawn = Stump.Server.WorldServer.Database.Monsters.MonsterSpawn;
using MonsterSpell = Stump.Server.WorldServer.Database.Monsters.MonsterSpell;
using MonsterSuperRace = Stump.Server.WorldServer.Database.Monsters.MonsterSuperRace;

namespace Stump.Server.WorldServer.Game.Actors.RolePlay.Monsters
{
    public enum GradeSelection
    {
        First,
        Last
    }

    public class MonsterManager : DataManager<MonsterManager>
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        private Dictionary<int, MonsterTemplate> m_monsterTemplates;
        private Dictionary<int, List<MonsterSpell>> m_monsterSpells;
        private Dictionary<int, MonsterSpawn> m_monsterSpawns;
        private Dictionary<int, MonsterDungeonSpawn> m_monsterDungeonsSpawns;
        private Dictionary<int, DroppableItem> m_droppableItems;
        private Dictionary<int, DroppableDofusItem> m_droppableDofusItems;
        private Dictionary<int, MonsterGrade> m_monsterGrades;
        private Dictionary<int, MonsterRace> m_monsterRaces;
        private Dictionary<int, MonsterSuperRace> m_monsterSuperRaces;
        private Dictionary<int, MonsterStaticSpawn> m_monsterStaticSpawns;
        private List<MonsterDungeonWaveSpawnEntity> m_dungeonswaves;

        [Initialization(InitializationPass.Sixth)]
        public override void Initialize()
        {
            #region >> Limpando as variaveis para que o comando Reload não duplique elas.

            if (m_monsterTemplates != null)
                m_monsterTemplates.Clear();

            if (m_monsterGrades != null)
                m_monsterGrades.Clear();

            if (m_monsterSpells != null)
                m_monsterSpells.Clear();

            if (m_monsterSpawns != null)
                m_monsterSpawns.Clear();

            if (m_monsterDungeonsSpawns != null)
                m_monsterDungeonsSpawns.Clear();

            if (m_droppableItems != null)
                m_droppableItems.Clear();

            if (m_droppableDofusItems != null)
                m_droppableDofusItems.Clear();

            if (m_monsterRaces != null)
                m_monsterRaces.Clear();

            if (m_monsterSuperRaces != null)
                m_monsterSuperRaces.Clear();

            if (m_monsterStaticSpawns != null)
                m_monsterStaticSpawns.Clear();

            if (m_dungeonswaves != null)
                m_dungeonswaves.Clear();

            #endregion

            m_monsterTemplates = Database.Query<MonsterTemplate>(MonsterTemplateRelator.FetchQuery).ToDictionary(entry => entry.Id);
            m_monsterGrades = Database.Query<MonsterGrade>(MonsterGradeRelator.FetchQuery).ToDictionary(entry => entry.Id);
            m_monsterSpells = new Dictionary<int, List<MonsterSpell>>();

            foreach (var spell in Database.Query<MonsterSpell>(MonsterSpellRelator.FetchQuery))
            {
                List<MonsterSpell> list;

                if (!m_monsterSpells.TryGetValue(spell.MonsterGradeId, out list))
                    m_monsterSpells.Add(spell.MonsterGradeId, list = new List<MonsterSpell>());

                list.Add(spell);
            }

            m_monsterSpawns = Database.Query<MonsterSpawn>(MonsterSpawnRelator.FetchQuery).ToDictionary(entry => entry.Id);
            m_monsterDungeonsSpawns = Database.Query<MonsterDungeonSpawn, MonsterDungeonSpawnEntity, MonsterGrade, MonsterDungeonSpawn>(new MonsterDungeonSpawnRelator().Map, MonsterDungeonSpawnRelator.FetchQuery).ToDictionary(entry => entry.Id);
            m_droppableItems = Database.Query<DroppableItem>(DroppableItemRelator.FetchQuery).ToDictionary(entry => entry.Id);
            m_droppableDofusItems = Database.Query<DroppableDofusItem>(DroppableDofusItemRelator.FetchQuery).ToDictionary(entry => entry.Id);
            m_monsterRaces = Database.Query<MonsterRace>(MonsterRaceRelator.FetchQuery).ToDictionary(entry => entry.Id);
            m_monsterSuperRaces = Database.Query<MonsterSuperRace>(MonsterSuperRaceRelator.FetchQuery).ToDictionary(entry => entry.Id);
            m_monsterStaticSpawns = Database.Query<MonsterStaticSpawn, MonsterStaticSpawnEntity, MonsterGrade, MonsterStaticSpawn>(new MonsterStaticSpawnRelator().Map, MonsterStaticSpawnRelator.FetchQuery).ToDictionary(entry => entry.Id);
            m_dungeonswaves = Database.Query<MonsterDungeonWaveSpawnEntity>(MonsterDungeonWaveSpawnRelator.FetchQuery).ToList();
        }

        public Dictionary<int, DroppableItem> GetDrops()
        {
            return m_droppableItems;
        }

        public List<DroppableItem> GetDropsByItemId(int itemId)
        {
            return m_droppableItems.Values.Where(x => x.ItemId == itemId).ToList();
        }

        public MonsterGrade[] GetMonsterGrades()
        {
            return m_monsterGrades.Values.ToArray();
        }

        public MonsterGrade GetMonsterGrade(int id)
        {
            MonsterGrade result;
            return !m_monsterGrades.TryGetValue(id, out result) ? null : result;
        }

        public MonsterGrade GetMonsterGrade(int monsterId, int grade)
        {
            var template = GetTemplate(monsterId);

            return template.Grades.Count <= grade - 1 ? null : template.Grades[grade - 1];
        }

        public List<MonsterGrade> GetMonsterGrades(int monsterId)
        {
            return m_monsterGrades.Where(entry => entry.Value.MonsterId == monsterId).Select(entry => entry.Value).ToList();
        }

        public List<MonsterSpell> GetMonsterGradeSpells(int id)
        {
            List<MonsterSpell> list;
            return m_monsterSpells.TryGetValue(id, out list) ? list : new List<MonsterSpell>();
        }

        public List<DroppableItem> GetMonsterDroppableItems(int id)
        {
            return m_droppableItems.Where(entry => entry.Value.MonsterOwnerId == id).Select(entry => entry.Value).ToList();
        }

        public List<DroppableDofusItem> GetMonsterDroppableDofusItems(int id)
        {
            return m_droppableDofusItems.Where(entry => entry.Value.MonsterOwnerId == id).Select(entry => entry.Value).ToList();
        }

        public MonsterRace GetMonsterRace(int id)
        {
            MonsterRace result;
            return !m_monsterRaces.TryGetValue(id, out result) ? null : result;
        }

        public MonsterSuperRace GetMonsterSuperRace(int id)
        {
            MonsterSuperRace result;
            return !m_monsterSuperRaces.TryGetValue(id, out result) ? null : result;
        }

        public MonsterTemplate GetTemplate(int id)
        {
            if (m_monsterTemplates is null)
                return null;

            MonsterTemplate result;
            return !m_monsterTemplates.TryGetValue(id, out result) ? null : result;
        }

        public MonsterTemplate[] GetTemplates()
        {
            if (m_monsterTemplates is null)
                return null;

            return m_monsterTemplates.Values.ToArray();
        }

        public MonsterTemplate GetTemplate(string name, bool ignoreCommandCase)
        {
            return m_monsterTemplates.Values.FirstOrDefault(entry => entry.Name.Equals(name, ignoreCommandCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture));
        }

        public void AddMonsterSpell(MonsterSpell spell)
        {
            Database.Insert(spell);
            List<MonsterSpell> list;

            if (!m_monsterSpells.TryGetValue(spell.MonsterGradeId, out list))
                m_monsterSpells.Add(spell.MonsterGradeId, list = new List<MonsterSpell>());

            list.Add(spell);
        }

        public void RemoveMonsterSpell(MonsterSpell spell)
        {
            Database.Delete(spell);
            m_monsterSpells.Remove(spell.Id);
        }

        public MonsterSpawn[] GetMonsterSpawns()
        {
            return m_monsterSpawns.Values.Where(entry => !entry.IsDisabled && !entry.Template.IsBoss).ToArray();
        }

        public MonsterDungeonSpawn[] GetMonsterDungeonsSpawns()
        {
            return m_monsterDungeonsSpawns.Values.ToArray();
        }

        public MonsterStaticSpawn[] GetMonsterStaticSpawns()
        {
            return m_monsterStaticSpawns.Values.ToArray();
        }

        public List<MonsterDungeonWaveSpawnEntity> GetMonsterDungeonsWaveSpawnsByMapId(long MapId)
        {
            var djsSpawns = m_monsterDungeonsSpawns.Values.Where(x => x.MapId == MapId).Select(x => x.Id);
            var toreturn = m_dungeonswaves.Where(x => djsSpawns.Contains(x.DungeonSpawnId)).ToList();

            return toreturn;
        }

        public MonsterSpawn GetOneMonsterSpawn(Predicate<MonsterSpawn> predicate)
        {
            return m_monsterSpawns.Values.SingleOrDefault(entry => predicate(entry));
        }

        public void AddMonsterSpawn(MonsterSpawn spawn)
        {
            Database.Insert(spawn);
            m_monsterSpawns.Add(spawn.Id, spawn);
        }

        public void RemoveMonsterSpawn(MonsterSpawn spawn)
        {
            Database.Delete(spawn);
            m_monsterSpawns.Remove(spawn.Id);
        }

        public void AddMonsterDrop(DroppableItem drop)
        {
            Database.Insert(drop);
            m_droppableItems.Add(drop.Id, drop);
        }

        public void RemoveMonsterDrop(DroppableItem drop)
        {
            Database.Delete(drop);
            m_droppableItems.Remove(drop.Id);
        }

        public int GetMonsterGradeByIdAndSelection(int monsterId, int BreachStep)
        {
            var template = GetTemplate(monsterId);

            if (template == null || template.Grades.Count == 0)
                return -1;

            int levelGroup = BreachStep <= 25 ? levelGroup = 1 : BreachStep <= 50 ? levelGroup = 2 : BreachStep <= 100 ? levelGroup = 3 : BreachStep <= 150 ? levelGroup = 4 : BreachStep <= 200 ? 5 : (int)template.Grades[template.Grades.Count - 1].GradeId;
            var filteredGrades = template.Grades.Where(grade => grade.GradeId == levelGroup).ToList();

            if (filteredGrades.Count == 0)
                return -1;

            return (int)filteredGrades[0].GradeId;
        }

        public int GetMonsterGradeByIdAndSelection(int monsterId, GradeSelection gradeSelection)
        {
            var template = GetTemplate(monsterId);

            if (template == null || template.Grades.Count == 0)
                return -1;

            switch (gradeSelection)
            {
                case GradeSelection.First:
                    return (int)template.Grades[0].GradeId;
                case GradeSelection.Last:
                    return (int)template.Grades[template.Grades.Count - 1].GradeId;
                default:
                    return -1;
            }
        }

        public MonsterSpawn GetMonsterSpawn(int subAreaId, double frequency, int monsterId)
        {
            MonsterSpawn monsterSpawn = null;
            var monsterGrade = GetMonsterGrades(monsterId);

            if (monsterGrade != null)
            {
                monsterSpawn = new MonsterSpawn
                {
                    SubAreaId = subAreaId,
                    Frequency = frequency,
                    MonsterId = monsterId,
                    MinGrade = 1,
                    MaxGrade = monsterGrade.Count(),
                    IsDisabled = false
                };
            }

            return monsterSpawn;
        }
    }
}