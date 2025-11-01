using System.Collections.Generic;
using System.Linq;
using Stump.Core.Extensions;
using Stump.Core.Threading;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Types;
using Stump.Server.WorldServer.Core.Network;
using Stump.Server.WorldServer.Database.Monsters;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.Stats;
using Stump.Server.WorldServer.Game.Effects;
using Stump.Server.WorldServer.Game.Fights.Results;
using Stump.Server.WorldServer.Game.Fights.Teams;
using Stump.Server.WorldServer.Game.Formulas;
using Stump.Server.WorldServer.Game.Items;
using Stump.Server.WorldServer.Game.Maps.Cells;
using Monster = Stump.Server.WorldServer.Game.Actors.RolePlay.Monsters.Monster;
using Stump.Server.WorldServer.Game.Actors.Interfaces;
using Stump.Core.Mathematics;
using System;
using Stump.Server.WorldServer.Game.Effects.Handlers.Spells.Summon;
//using Stump.Server.WorldServer.Game.Jobs;

namespace Stump.Server.WorldServer.Game.Actors.Fight
{
    public sealed class MonsterFighter : AIFighter, ICreature
    {
        readonly Dictionary<DroppableItem, int> m_dropsCount = new Dictionary<DroppableItem, int>();
        readonly Dictionary<DroppableDofusItem, int> m_dropsDofusCount = new Dictionary<DroppableDofusItem, int>();
        readonly StatsFields m_stats;

        public MonsterFighter(FightTeam team, Monster monster, int waveNumber = 0, int minPlayerLevel = 0) : base(team, monster.Grade.Spells.ToArray(), monster.Grade.MonsterId)
        {
            Id = Fight.GetNextContextualId();
            Monster = monster;
            Look = monster.Look.Clone();

            m_stats = new StatsFields(this);
            m_stats.Initialize(Monster.Grade);

            Fight.FindRandomFreeCell(this, out var cell, false);
            Position = new ObjectPosition(monster.Group.Map, cell, monster.Group.Direction);
            WaveNumber = waveNumber;
            MinPlayerLevel = minPlayerLevel;
        }

        public int WaveNumber
        {
            get;
            set;
        }

        public int MinPlayerLevel
        {
            get;
            set;
        }

        public Monster Monster
        {
            get;
        }

        public MonsterGrade MonsterGrade
        {
            get { return Monster.Grade; }
        }

        public override string Name
        {
            get { return Monster.Template.Name; }
        }

        public override ObjectPosition MapPosition
        {
            get { return Monster.Group.Position; }
        }

        public override bool Vip
        {
            get { return false; }
        }

        public override RoleEnum Role
        {
            get { return RoleEnum.Player; }
        }

        public override Character Owner
        {
            get { return Summoner != null && Summoner.Owner is Character ? Summoner.Owner : null; }
        }

        public override ushort Level
        {
            get
            {
                return (byte)Monster.Grade.Level;
            }
        }

        public byte HiddenLevel
        {
            get
            {
                return (byte)Monster.Grade.HiddenLevel;
            }
        }

        public override StatsFields Stats
        {
            get { return m_stats; }
        }


        // monster ignore tackles ...
        //public override int GetTackledAP(int mp, Cell cell)
        //{
        //    //return 0;
        //    if (VisibleState != GameActionFightInvisibilityStateEnum.VISIBLE)
        //        return 0;

        //    if (HasState((int)SpellStatesEnum.INTACLABLE_96))
        //        return 0;

        //    return (int)Math.Round(ap * (1 - GetTacklePercent(cell)));
        //}

        //public override int GetTackledMP(int mp, Cell cell)
        //{
        //    //return 0;
        //    if (VisibleState != GameActionFightInvisibilityStateEnum.VISIBLE)
        //        return 0;

        //    if (HasState((int)SpellStatesEnum.INTACLABLE_96))
        //        return 0;

        //    return (int)Math.Round(mp * (1 - GetTacklePercent(cell)));
        //}

        public override bool CanTackle(FightActor fighter) => base.CanTackle(fighter) && Monster.Template.CanTackle;

        public override bool CanBePushed() => base.CanBePushed() && Monster.Template.CanBePushed;

        public override bool CanSwitchPos() => base.CanSwitchPos() && Monster.Template.CanSwitchPos;

        public override uint GetDroppedKamas()
        {
            var random = new AsyncRandom();
            if ((uint)random.Next(this.Level / 2, Level + 1) > (uint)random.Next(Monster.Template.MinDroppedKamas, Monster.Template.MaxDroppedKamas + 1))
            {
                return (uint)random.Next(this.Level / 2, Level + 1);
            }
            else { return (uint)random.Next(Monster.Template.MinDroppedKamas, Monster.Template.MaxDroppedKamas + 1); }

        }

        public override int GetGivenExperience() => Monster.Grade.GradeXp;

        public override bool CanDrop() => true;

        public override bool CanPlay() => base.CanPlay() && Monster.Template.CanPlay;

        public override bool CanMove() => base.CanMove() && MonsterGrade.MovementPoints > 0;

        public override IEnumerable<DroppedItem> RollLoot(IFightResult looter)
        {
            if (!IsDead()) //Tem que está morto antes de da seguencia.
                return new DroppedItem[0];

            var random = new AsyncRandom();
            var items = new List<DroppedItem>();

            var prospectingSum = OpposedTeam.GetAllFighters<CharacterFighter>().Sum(entry => entry.Stats[PlayerFields.Prospecting].Total);
            var droppedGroups = new List<int>();
            bool monoconta = false;

            if (looter is FightPlayerResult)
            {
                monoconta = !((looter as FightPlayerResult).Fighter.Team.Fighters.Where(x => x is CharacterFighter).Select(x => (x as CharacterFighter)).Any(y => y.Character.Client.IP == (looter as FightPlayerResult).Character.Client.IP && y.Character != (looter as FightPlayerResult).Character));
            }

            #region >> Drop Itens Comun
            foreach (var droppableItem in Monster.Template.DroppableItems.Where(droppableItem => (prospectingSum >= droppableItem.ProspectingLock) || !(FightFormulas.GetDofusItemsIds.Contains(droppableItem.ItemId) && monoconta)).Shuffle())
            {
                if (!(looter is TaxCollectorProspectingResult) && (looter is FightPlayerResult) && !droppableItem.AreConditionsFilled((looter as FightPlayerResult).Character))
                    continue;

                if (droppableItem.IsDisabled)
                    continue;

                if (droppedGroups.Contains(droppableItem.DropGroup))
                    continue;

                if (!(looter is FightPlayerResult) && droppableItem.TaxCollectorCannotLoot)
                    continue;

                if (looter is TaxCollectorProspectingResult && droppableItem.TaxCollectorCannotLoot)
                    continue;

                for (var i = 0; i < droppableItem.RollsCounter; i++)
                {
                    if (Monster.Template.Race == 18) //Templos de Classes (Doples)
                    {
                        var monsterGradeId = 1;

                        while (monsterGradeId < 5)
                        {
                            if (Monster.Grade.GradeId > monsterGradeId * 2.4)
                                monsterGradeId++;
                            else
                                break;
                        }

                        if (m_dropsCount.ContainsKey(droppableItem) && m_dropsCount[droppableItem] >= Math.Round(((droppableItem.GetDropRate((int)monsterGradeId) * droppableItem.RollsCounter) / 100), 0))
                            break;
                    }

                    if (droppableItem.DropLimit > 0 && m_dropsCount.ContainsKey(droppableItem) && m_dropsCount[droppableItem] >= droppableItem.DropLimit)
                        break;

                    var chance = (random.Next(0, 100) + random.NextDouble() + random.Next(0, 10));
                    var dropRate = FightFormulas.AdjustDropChance(looter, droppableItem, Monster, Fight.AgeBonus);

                    if (dropRate < chance)
                        continue;

                    if (droppableItem.DropGroup != 0)
                        droppedGroups.Add(droppableItem.DropGroup);

                    #region Desativado para Estudo
                    //if (droppableItem.ItemId == 1328)//Moeda PVM
                    //{
                    //    if (Monster.Group.Map.Id == 179568640)//Mapa do Chefe
                    //    { 
                    //        if (Monster.Template.Id == 4882)//Monstro Chefe
                    //        {
                    //            Random rnd = new Random();
                    //            items.Add(new DroppedItem(droppableItem.ItemId, (uint)rnd.Next(2, 3)));

                    //            if (!m_dropsCount.ContainsKey(droppableItem))
                    //                m_dropsCount.Add(droppableItem, rnd.Next(2, 3));
                    //            else
                    //                m_dropsCount[droppableItem]++;
                    //        }
                    //    }
                    //    else
                    //    {
                    //        items.Add(new DroppedItem(droppableItem.ItemId, 1));

                    //        if (!m_dropsCount.ContainsKey(droppableItem))
                    //            m_dropsCount.Add(droppableItem, 1);
                    //        else
                    //            m_dropsCount[droppableItem]++;
                    //    }
                    //}
                    //else
                    //{
                    //    items.Add(new DroppedItem(droppableItem.ItemId, 1));
                    //    if (!m_dropsCount.ContainsKey(droppableItem))
                    //        m_dropsCount.Add(droppableItem, 1);
                    //    else
                    //        m_dropsCount[droppableItem]++;
                    //}
                    #endregion

                    items.Add(new DroppedItem(droppableItem.ItemId, 1));

                    if (!m_dropsCount.ContainsKey(droppableItem))
                        m_dropsCount.Add(droppableItem, 1);
                    else
                        m_dropsCount[droppableItem]++;
                }
            }
            #endregion

            #region >> Drop Itens Dofus
            foreach (var droppableDofusItem in Monster.Template.DroppableDofusItems.Where(droppableItem => (prospectingSum >= droppableItem.ProspectingLock) || (FightFormulas.GetDofusItemsIds.Contains(droppableItem.ItemId) && monoconta)).Shuffle())
            {
                if (!(looter is TaxCollectorProspectingResult) && (looter is FightPlayerResult) && !droppableDofusItem.AreConditionsFilled((looter as FightPlayerResult).Character))
                    continue;

                if (droppableDofusItem.IsDisabled)
                    continue;

                if (droppedGroups.Contains(droppableDofusItem.DropGroup))
                    continue;

                if (!(looter is FightPlayerResult) && droppableDofusItem.TaxCollectorCannotLoot)
                    continue;

                if (looter is TaxCollectorProspectingResult && droppableDofusItem.TaxCollectorCannotLoot)
                    continue;

                for (var i = 0; i < droppableDofusItem.RollsCounter; i++)
                {
                    if (droppableDofusItem.DropLimit > 0 && m_dropsDofusCount.ContainsKey(droppableDofusItem) && m_dropsDofusCount[droppableDofusItem] >= droppableDofusItem.DropLimit)
                        break;

                    var chance = (random.Next(0, 100) + random.NextDouble() + random.Next(0, 10));
                    var dropRate = FightFormulas.AdjustDropChance(looter, droppableDofusItem, Monster, Fight.AgeBonus);

                    if (dropRate < chance)
                        continue;

                    if (droppableDofusItem.DropGroup != 0)
                        droppedGroups.Add(droppableDofusItem.DropGroup);

                    items.Add(new DroppedItem(droppableDofusItem.ItemId, 1));

                    if (!m_dropsDofusCount.ContainsKey(droppableDofusItem))
                        m_dropsDofusCount.Add(droppableDofusItem, 1);
                    else
                        m_dropsDofusCount[droppableDofusItem]++;
                }
            }
            #endregion

            return items;
        }

        public override int CalculateDamageResistance(int damage, EffectSchoolEnum type, bool critical, bool withArmor, bool poison, bool isRanged)
        {
            var percentResistance = CalculateTotalResistances(type, true, poison);
            var fixResistance = CalculateTotalResistances(type, false, poison);
            var armorResistance = withArmor && !poison ? CalculateArmorReduction(type) : 0;

            var result = (int)((1 - percentResistance / 100d) * (damage - armorResistance - fixResistance)) -
                         (critical ? Stats[PlayerFields.CriticalDamageReduction].Total : 0);

            return result;
        }

        public override GameContextActorInformations GetGameContextActorInformations(Character character)
        {
            return GetGameFightFighterInformations();
        }

        public override GameFightFighterInformations GetGameFightFighterInformations(WorldClient client = null)
        {
            return new GameFightMonsterInformations(
                Id,
                GetEntityDispositionInformations(client),
                Look.GetEntityLook(),
                GetGameContextBasicSpawnInformation(),
                0,
                GetGameFightMinimalStats(client),
                new ushort[0],
                (ushort)Monster.Template.Id,
                (sbyte)Monster.Grade.GradeId,
                (short)Monster.Grade.Level);
        }

        public override GameFightFighterLightInformations GetGameFightFighterLightInformations(WorldClient client = null)
        {
            return new GameFightFighterMonsterLightInformations(
                true,
                IsAlive(),
                Id,
                0,
                Level,
                (sbyte)BreedEnum.MONSTER,
                (ushort)Monster.Template.Id);
        }

        public override FightTeamMemberInformations GetFightTeamMemberInformations()
        {
            return new FightTeamMemberMonsterInformations(Id, Monster.Template.Id, (sbyte)Monster.Grade.GradeId);
        }

        public override string GetMapRunningFighterName()
        {
            return Monster.Template.Id.ToString();
        }

        public override string ToString()
        {
            return Monster.ToString();
        }

        protected override void OnDisposed()
        {
            base.OnDisposed();

            if (!Monster.Group.IsDisposed)
                Monster.Group.Delete();
        }
    }
}