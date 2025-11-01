using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Types;
using Stump.Server.WorldServer.Core.Network;
using Stump.Server.WorldServer.Database.Monsters;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Actors.Interfaces;
using Stump.Server.WorldServer.Game.Actors.Stats;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Fights.Teams;
using Stump.Server.WorldServer.Game.Maps.Cells;
using Stump.Server.WorldServer.Game.Spells;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;

namespace Stump.Server.WorldServer.Game.Actors.Fight
{
    public sealed class SummonedTurret : SummonedFighter, ICreature
    {
        protected readonly StatsFields m_stats;
        protected Spell m_spell;

        public bool AlreadyRecursive
        {
            get;
            set;
        }

        public override bool CanSwitchPos() => false;

        public override bool CanTackle(FightActor fighter) => false;

        public override bool CanMove() => base.CanMove() && MonsterGrade.MovementPoints > 0;

        public FightActor Caster
        {
            get;
        }

        public MonsterGrade Monster
        {
            get;
        }

        public override Character Owner => (Caster as CharacterFighter).Owner;


        public MonsterGrade MonsterGrade => Monster;

        public override string Name => Monster.Template.Name;

        public override ObjectPosition MapPosition => Position;

        public override ushort Level => (byte)Monster.Level;

        public override bool Vip => false;

        public override RoleEnum Role => RoleEnum.Player;

        public override StatsFields Stats => m_stats;

        public override string GetMapRunningFighterName() => Name;

        public SummonedTurret(int id, FightActor summoner, MonsterGrade template, Spell spell, Cell cell) : base(id, summoner.Team, template.Spells, summoner, cell, template.Template.Id, template)
        {
            Caster = summoner;
            Monster = template;
            Look = Monster.Template.EntityLook.Clone();
            AlreadyRecursive = false;
            m_spell = spell;
            m_stats = new StatsFields(this);
            m_stats.Initialize(template);
            m_stats.MP.Modified += OnMPModified;

            AdjustStats();

            Team.FighterAdded += OnFighterAdded;
            Fight.TurnStarted += OnTurnStarted;
        }

        void OnFighterAdded(FightTeam team, FightActor actor)
        {
            if (actor != this)
                return;

            CastAutoSpell(new Spell((int)SpellIdEnum.TRANSKO_5223, 1), Cell);
        }

        static void OnMPModified(StatsData mpStats, int amount)
        {
            if (amount == 0)
                return;

            mpStats.Context = 0;
        }

        protected override void OnDisposed()
        {
            m_stats.MP.Modified -= OnMPModified;
            base.OnDisposed();
        }

        public override void OnTurnStarted(IFight fight, FightActor fighter)
        {
            AlreadyRecursive = false;

            if (fighter != this)
                return;

            Spell spellToCast = null;
            FightActor target = null;

            //switch ((MonsterIdEnum)Monster.Template.Id)
            //{
            //    case MonsterIdEnum.TACTIRELLE_3289:
            //    case MonsterIdEnum.TACTIRELLE_5837:
            //        {
            //            if (this.GetStates().FirstOrDefault(x => x.State.Id == (int)SpellStatesEnum.EVOLUTION_III_135 && !x.IsDisabled) != null)
            //            {
            //                target = Fight.Fighters.FirstOrDefault(x => this != x && x.HasState((int)SpellStatesEnum.TRANSKO_364) && !x.HasState((int)SpellStatesEnum.ROOTED_6) && !x.HasState((int)SpellStatesEnum.UNMOVABLE_97));

            //                if (target == null)
            //                    spellToCast = new Spell((int)SpellIdEnum.TRANSKO_3240, 1);
            //                else
            //                    spellToCast = new Spell((int)SpellIdEnum.TRANSKO_3240, (short)target.GetStates().FirstOrDefault(x => x.State.Id == (int)SpellStatesEnum.TRANSKO_364 && !x.IsDisabled).Spell.CurrentLevel);
            //            }
            //            break;
            //        }
            //    case MonsterIdEnum.GARDIENNE_3288:
            //    case MonsterIdEnum.GARDIENNE_5835:
            //        {
            //            if (!HasState((int)SpellStatesEnum.EVOLUTION_III_135)) 
            //                break;

            //            spellToCast = new Spell((int)SpellIdEnum.RESCUE_3244, 1);
            //            target = fighter.Team.Fighters.FirstOrDefault(x => x.HasState((int)SpellStatesEnum.FIRST_AID_131));
            //            break;
            //        }


            //    case MonsterIdEnum.HARPONNEUSE_3287:
            //    case MonsterIdEnum.HARPONNEUSE_5836:
            //        {
            //            if (!HasState((int)SpellStatesEnum.EVOLUTION_III_135)) 
            //                break;

            //            if (HasState((int)SpellStatesEnum.TELLURIC_127))
            //                spellToCast = new Spell((int)SpellIdEnum.BOOBOOME_3239, 1);
            //            else if (HasState((int)SpellStatesEnum.AQUATIC_128))
            //                spellToCast = new Spell((int)SpellIdEnum.BWOOBWOOM_3232, 1);
            //            else if (HasState((int)SpellStatesEnum.ARDENT_129))
            //                spellToCast = new Spell((int)SpellIdEnum.BOOBOOMF_3231, 1);

            //            target = fighter.OpposedTeam.Fighters.FirstOrDefault(x => x.HasState((int)SpellStatesEnum.AMBUSH_130));
            //            break;
            //        }

            //    case MonsterIdEnum.CHALUTIER_5141:
            //    case MonsterIdEnum.CHALUTIER_5832:
            //        {
            //            if (!HasState((int)SpellStatesEnum.EVOLUTION_III_135))
            //                break;

            //            spellToCast = new Spell((int)SpellIdEnum.DOUBLE_GERMAN_9881, 1);
            //            target = Fight.Fighters.FirstOrDefault(x => this != x && (x.HasState((int)SpellStatesEnum.SPYGLASS_132)) && !x.HasState((int)SpellStatesEnum.ROOTED_6) && !x.HasState((int)SpellStatesEnum.UNMOVABLE_97));
                      
            //            if (target == null)
            //                target = Fight.Fighters.FirstOrDefault(x => this != x && (x.HasState((int)SpellStatesEnum.SIGHGLASS_147)) && !x.HasState((int)SpellStatesEnum.ROOTED_6) && !x.HasState((int)SpellStatesEnum.UNMOVABLE_97));
            //            break;
            //        }

            //    case MonsterIdEnum.FOREUSE_5142:
            //    case MonsterIdEnum.FOREUSE_5833:
            //        {
            //            if (!HasState((int)SpellStatesEnum.EVOLUTION_III_135))
            //                break;

            //            spellToCast = new Spell((int)SpellIdEnum.EXCAVATION_9883, 1);
            //            target = fighter.OpposedTeam.Fighters.FirstOrDefault(x => x.HasState((int)SpellStatesEnum.AMBUSH_130));
            //            break;
            //        }

            //    case MonsterIdEnum.BATHYSCAPHE_5143:
            //    case MonsterIdEnum.BATHYSCAPHE_5831:
            //        {
            //            if (!HasState((int)SpellStatesEnum.EVOLUTION_III_135)) 
            //                break;

            //            spellToCast = new Spell((int)SpellIdEnum.DECOMPRESSION_9887, 1);
            //            target = fighter.Team.Fighters.FirstOrDefault(x => x.HasState((int)SpellStatesEnum.FIRST_AID_131));
            //            break;
            //        }
            //}

            if (spellToCast == null || target == null)
            {
                base.OnTurnStarted(fight, fighter);
                return;
            }

            CastAutoSpell(spellToCast, target.Cell);

            base.OnTurnStarted(fight, fighter);
        }

        void AdjustStats()
        {
            var baseCoef = 0.25;

            switch (Monster.Template.Id)
            {
                case (int)MonsterIdEnum.HARPONNEUSE_3287:
                case (int)MonsterIdEnum.HARPONNEUSE_5836:
                case (int)MonsterIdEnum.FOREUSE_5142:
                case (int)MonsterIdEnum.FOREUSE_5833:
                    baseCoef = 0.3;
                    break;
                case (int)MonsterIdEnum.GARDIENNE_3288:
                case (int)MonsterIdEnum.GARDIENNE_5835:
                case (int)MonsterIdEnum.BATHYSCAPHE_5143:
                case (int)MonsterIdEnum.BATHYSCAPHE_5831:
                    baseCoef = 0.25;
                    break;
                case (int)MonsterIdEnum.TACTIRELLE_3289:
                case (int)MonsterIdEnum.TACTIRELLE_5837:
                case (int)MonsterIdEnum.CHALUTIER_5141:
                case (int)MonsterIdEnum.CHALUTIER_5832:
                    baseCoef = 0.2;
                    break;
            }

            var coef = baseCoef + (0.02 * (m_spell.CurrentLevel - 1));
            m_stats.Health.Base += (int)(((Summoner.Level - 1) * 5 + 55) * coef) + (int)((Summoner.MaxLifePoints) * coef);

            m_stats.Intelligence.Base = (short)(Summoner.Stats.Intelligence.Base * (1 + (Summoner.Level / 100d)));
            m_stats.Chance.Base = (short)(Summoner.Stats.Chance.Base * (1 + (Summoner.Level / 100d)));
            m_stats.Strength.Base = (short)(Summoner.Stats.Strength.Base * (1 + (Summoner.Level / 100d)));
            m_stats.Agility.Base = (short)(Summoner.Stats.Agility.Base * (1 + (Summoner.Level / 100d)));
            m_stats.Wisdom.Base = (short)(Summoner.Stats.Wisdom.Base * (1 + (Summoner.Level / 100d)));

            m_stats[PlayerFields.DamageBonus].Base = Summoner.Stats[PlayerFields.DamageBonus].Equiped;
            m_stats[PlayerFields.DamageBonusPercent].Base = Summoner.Stats[PlayerFields.DamageBonusPercent].Equiped;
            m_stats[PlayerFields.AirDamageBonus].Base = Summoner.Stats[PlayerFields.AirDamageBonus].Equiped;
            m_stats[PlayerFields.FireDamageBonus].Base = Summoner.Stats[PlayerFields.FireDamageBonus].Equiped;
            m_stats[PlayerFields.WaterDamageBonus].Base = Summoner.Stats[PlayerFields.WaterDamageBonus].Equiped;
            m_stats[PlayerFields.EarthDamageBonus].Base = Summoner.Stats[PlayerFields.EarthDamageBonus].Equiped;
            m_stats[PlayerFields.PushDamageBonus].Base = Summoner.Stats[PlayerFields.PushDamageBonus].Equiped;
        }

        //Version 2.61 by Kenshin
        public override GameFightFighterInformations GetGameFightFighterInformations(WorldClient client = null)
        {
            return new GameFightMonsterInformations(
                contextualId: Id,
                look: Look.GetEntityLook(),
                disposition: GetEntityDispositionInformations(),
                spawnInfo: GetGameContextBasicSpawnInformation(client),
                wave: 0,
                stats: GetGameFightMinimalStats(),
                previousPositions: new ushort[0],
                creatureGenericId: (ushort)Monster.MonsterId,
                creatureGrade: (sbyte)Monster.GradeId,
                creatureLevel: (short)Monster.Level);
        }

        public override FightTeamMemberInformations GetFightTeamMemberInformations()
        {
            return new FightTeamMemberMonsterInformations(id: Id, monsterId: Monster.Template.Id, grade: (sbyte)Monster.GradeId);
        }

        public override GameFightCharacteristics GetGameFightMinimalStats(WorldClient client = null)
        {
            return new GameFightCharacteristics(
                characteristics: new CharacterCharacteristics(characteristics: GetFightActorCharacteristic()),
                summoner: Summoner.Id,
                summoned: true,
                invisibilityState: (sbyte)(client == null ? VisibleState : GetVisibleStateFor(client.Character)));
        }
    }
}