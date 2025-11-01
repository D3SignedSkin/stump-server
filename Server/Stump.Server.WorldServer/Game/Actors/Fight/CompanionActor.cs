using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Types;
using Stump.Server.WorldServer.Core.Network;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Actors.Look;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.Stats;
using Stump.Server.WorldServer.Game.Effects;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Fights.Buffs;
using Stump.Server.WorldServer.Game.Fights.Teams;
using Stump.Server.WorldServer.Game.Maps.Cells;
using Stump.Server.WorldServer.Game.Spells;
using Stump.Server.WorldServer.Handlers.Basic;
using Stump.Server.WorldServer.Game.Fights.Results;
using Stump.Server.WorldServer.Database.Items.Templates;
using Stump.Server.WorldServer.Database.Spells;
using Stump.DofusProtocol.Messages;
using Stump.Server.WorldServer.Database.Companion;
using Accord.Statistics.Kernels;

namespace Stump.Server.WorldServer.Game.Actors.Fight
{
    public sealed class CompanionActor : NamedFighter
    {
        // FIELDS

        int m_criticalWeaponBonus;
        int m_damageTakenBeforeFight;
        int m_weaponUses;
        bool m_left;

        // CONSTRUCTORS
        public CompanionActor(Character character, FightTeam team, ActorLook look, List<Spell> spell, byte companionId,
            sbyte id) : base(team)
        {
            FighterId = id;
            Master = character;
            Look = look;
            Look.RemoveAuras();
            CompanionId = companionId;
            CompanionSpell = spell;
            Cell cell;
            CompanionStats = new StatsFields(this);
            CompanionStats.Initialize(Master.Level);//Master.Level

            if (!Fight.FindRandomFreeCell(this, out cell, false))
                return;

            Position = new ObjectPosition(character.Map, cell, character.Direction);
            InitializeCharacterFighter();
        }

        public CompanionRecord Record { get; set; }

        // PROPERTIES
        public Character Master { get; }
        public byte CompanionId { get; set; }

        public ReadyChecker PersonalReadyChecker { get; set; }
        public List<Spell> CompanionSpell { get; set; }

        public int FighterId { get; set; }

        public override int Id => FighterId;

        public override string Name => Master.Name;

        public override bool Vip => Master.Vip;

        public override RoleEnum Role => Master.UserGroup.Role;

        public override Character Owner => Master;

        public override ActorLook Look { get; set; }

        public override ObjectPosition MapPosition => Position;

        public override ushort Level => Master.Level;

        public StatsFields CompanionStats { get; set; }

        public override StatsFields Stats => CompanionStats;

        public bool IsDisconnected { get; private set; }

        public int? RemainingRounds { get; private set; }



        // METHODS

        public void LeaveFight(bool force = false)
        {
            if (HasLeft())
                return;

            m_left = !force;

            OnLeft();
        }

        private void InitializeCharacterFighter()
        {
            m_damageTakenBeforeFight = Stats.Health.DamageTaken;
            if (Fight.FightType == FightTypeEnum.FIGHT_TYPE_CHALLENGE)
            {
                Stats.Health.DamageTaken = 0;
            }
        }

        public CharacterCharacteristicsInformations GetCompanionStatsMessage()
        {
            return new CharacterCharacteristicsInformations(
                experience: (ulong)Master.Experience,
                experienceLevelFloor: (ulong)Master.LowerBoundExperience,
                experienceNextLevelFloor: (ulong)Master.UpperBoundExperience,
                experienceBonusLimit: 0,
                kamas: 0,
                alignmentInfos: Master.GetActorAlignmentExtendInformations(),
                criticalHitWeapon: (ushort)Stats[PlayerFields.CriticalHit].Base,
                characteristics: GetFightActorCharacteristic(),
                spellModifiers: new List<SpellModifierMessage>().ToArray(), // TODO - 2.71
                probationTime: 0);
        }

        public List<SpellItem> GetSpellsItem()
        {
            return CompanionSpell.Select(spell => new SpellItem(spell.Id, (short)spell.CurrentLevel)).ToList();
        }

        public ShortcutSpell[] GetShortcut()
        {
            var shortcutSpell = new ShortcutSpell[CompanionSpell.Count];
            for (var i = 0; i < CompanionSpell.Count; i++)
            {
                shortcutSpell[i] = new ShortcutSpell((sbyte)i, (ushort)CompanionSpell[i].Id);
            }

            return shortcutSpell;
        }

        public override ObjectPosition GetLeaderBladePosition()
        {
            return GetPositionBeforeMove();
        }

        public override bool CastSpell(SpellCastInformations spell)
        {

            bool result;
            if (!IsFighterTurn())
            {
                result = false;
            }
            else
            {
                if (spell.Spell.Id != 0)
                {
                    result = base.CastSpell(spell);
                }
                else
                {
                    return false;
                }
            }

            return result;
        }

        public override SpellCastResult CanCastSpell(SpellCastInformations cast)
        {
            var result = base.CanCastSpell(cast);

            if (result == SpellCastResult.OK || cast.IsConditionBypassed(result))
                return result;

            if (cast.Silent)
                return result;

            switch (result)
            {
                case SpellCastResult.NO_LOS:
                    // Impossible de lancer ce sort : un obstacle gène votre vue !
                    Master.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 174);
                    break;
                case SpellCastResult.HAS_NOT_SPELL:
                    // Impossible de lancer ce sort : vous ne le possédez pas !
                    Master.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 169);
                    break;
                case SpellCastResult.NOT_ENOUGH_AP:
                    // Impossible de lancer ce sort : Vous avez %1 PA disponible(s) et il vous en faut %2 pour ce sort !
                    Master.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 170, AP, cast.SpellLevel.ApCost);
                    break;
                case SpellCastResult.UNWALKABLE_CELL:
                    // Impossible de lancer ce sort : la cellule visée n'est pas disponible !
                    Master.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 172);
                    break;
                case SpellCastResult.CELL_NOT_FREE:
                    //Impossible de lancer ce sort : la cellule visée n'est pas valide !
                    Master.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 193);
                    break;
                default:
                    //Impossible de lancer ce sort actuellement.
                    Master.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 175);
                    break;
            }

            return result;
        }

        public override FightSpellCastCriticalEnum RollCriticalDice(SpellLevelTemplate spell)
            => Master.CriticalMode ? FightSpellCastCriticalEnum.CRITICAL_HIT : base.RollCriticalDice(spell);

        public override FightSpellCastCriticalEnum RollCriticalDice(WeaponTemplate weapon)
            => Master.CriticalMode ? FightSpellCastCriticalEnum.CRITICAL_HIT : base.RollCriticalDice(weapon);

        /*public override Damage CalculateDamageBonuses(Damage damage)
        {
            if (Master.GodMode)
                damage.Amount = short.MaxValue;

            if (m_isUsingWeapon)
                damage.Amount += m_criticalWeaponBonus;

            return base.CalculateDamageBonuses(damage);
        }*/

        public bool CanUseWeapon(Cell cell, WeaponTemplate weapon)
        {
            if (!IsFighterTurn())
                return false;

            if (HasState((int)SpellStatesEnum.WEAKENED_42))
                return false;

            var point = new MapPoint(cell);

            if ((weapon.CastInDiagonal && (point.EuclideanDistanceTo(Position.Point) > weapon.Range /* old WeaponRange */ || point.EuclideanDistanceTo(Position.Point) < weapon.MinRange)) ||
                (!weapon.CastInDiagonal && point.ManhattanDistanceTo(Position.Point) > weapon.Range  /* old WeaponRange */  || point.ManhattanDistanceTo(Position.Point) < weapon.MinRange))
                return false;

            if (m_weaponUses >= weapon.MaxCastPerTurn)
                return false;

            return AP >= weapon.ApCost && Fight.CanBeSeen(cell, Position.Cell);
        }

        public override Spell GetSpell(int id) => CompanionSpell.FirstOrDefault(x => x.Template.Id == id);

        public override bool HasSpell(int id) => CompanionSpell.Exists(x => x.Template.Id == id);


        public override void ResetFightProperties()
        {
            base.ResetFightProperties();

            if (Fight.IsDeathTemporarily)
                Stats.Health.DamageTaken = m_damageTakenBeforeFight;
            else if (Stats.Health.Total <= 0)
                Stats.Health.DamageTaken = (Stats.Health.TotalMax - 1);
        }

        public override bool MustSkipTurn() => base.MustSkipTurn() || (IsDisconnected && Team.GetAllFighters<CharacterFighter>().Any(x => x.IsAlive() && !x.IsDisconnected));

        public override IFightResult GetFightResult(FightOutcomeEnum outcome) => new FightResult(this, outcome, Loot);

        public override FightTeamMemberInformations GetFightTeamMemberInformations() => new FightTeamMemberEntityInformation(Id, (sbyte)CompanionId, (byte)Level, Master.Id);

        public override GameFightFighterInformations GetGameFightFighterInformations(WorldClient client = null)
        {
            return new GameFightEntityInformation(
                Id,
                GetEntityDispositionInformations(client),
                Look.GetEntityLook(),
                GetGameContextBasicSpawnInformation(),
                0,
                GetGameFightMinimalStats(client),
                new ushort[0],
                (sbyte)CompanionId,
                Level > 200 ? (byte)200 : (byte)Level,
                Master.Id);
        }

        //Version 2.61 by Kenshin
        public override GameFightFighterLightInformations GetGameFightFighterLightInformations(WorldClient client = null)
        {
            return new GameFightFighterEntityLightInformation(
                        sex: Master.Sex == SexTypeEnum.SEX_FEMALE,
                        alive: IsAlive(),
                        id: Id,
                        wave: 0,
                        level: Level,
                        breed: (sbyte)Master.Breed.Id,
                        entityModelId: (sbyte)CompanionId,
                        masterId: Master.Id);
        }

        public override string ToString()
        {
            return Master.ToString();
        }

        #region God state
        public override bool UseAP(short amount)
        {
            if (!Master.GodMode)
                return base.UseAP(amount);

            base.UseAP(amount);
            RegainAP(amount);

            return true;
        }

        public override bool UseMP(short amount)
        {
            return Master.GodMode || base.UseMP(amount);
        }

        public override bool LostAP(short amount, FightActor source)
        {
            if (!Master.GodMode)
                return base.LostAP(amount, source);

            base.LostAP(amount, source);
            RegainAP(amount);

            return true;
        }

        public override bool LostMP(short amount, FightActor source)
        {
            return Master.GodMode || base.LostMP(amount, source);
        }


        public override int InflictDamage(Damage damage)
        {
            if (!Master.GodMode)
                return base.InflictDamage(damage);

            damage.GenerateDamages();
            OnBeforeDamageInflicted(damage);

            damage.Source.TriggerBuffs(damage.Source, BuffTriggerType.BeforeAttack, damage);
            TriggerBuffs(damage.Source, BuffTriggerType.BeforeDamaged, damage);
            TriggerBuffs(damage.Source, BuffTriggerType.OnDamaged, damage);

            OnDamageReducted(damage.Source, damage.Amount);

            damage.Source.TriggerBuffs(damage.Source, BuffTriggerType.AfterAttack, damage);
            TriggerBuffs(damage.Source, BuffTriggerType.AfterDamaged, damage);

            OnDamageInflicted(damage);

            return 0;
        }

        public PartyEntityMemberInformation GetPartyCompanionMemberInformations()
        {
            return new PartyEntityMemberInformation((sbyte)FighterId, (sbyte)CompanionId, Look.GetEntityLook(),
                (ushort)Stats[PlayerFields.Initiative].Total, (uint)LifePoints, (uint)MaxLifePoints,
                (ushort)Stats[PlayerFields.Prospecting].Total, (byte)Master.RegenSpeed);
        }
        #endregion
    }
}