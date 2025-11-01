using System.Globalization;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Types;
using System.Collections.Generic;
using Stump.Server.WorldServer.Core.Network;
using Stump.Server.WorldServer.Database.Monsters;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Actors.Stats;
using Stump.Server.WorldServer.Game.Fights.Teams;
using Stump.Server.WorldServer.Game.Maps.Cells;
using Stump.Server.WorldServer.Game.Actors.Interfaces;
using Stump.Server.WorldServer.Game.Spells;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using System;
using System.Linq;

namespace Stump.Server.WorldServer.Game.Actors.Fight
{
    public class SummonedMonster : SummonedFighter, ICreature
    {
        readonly StatsFields m_stats;

        // > Ordem : Zobal, Osamodas, Cra, Ecaflip, Feca, Sram, Iop, Sadida, Enutrof, Sacrier, Pandawa, Xelor, Eniripsa, Huppermago, Steamer, Ladino, Eliotrop, Kilorf
        private List<int> MontersDopplesId = new List<int> { 3136, 2609, 963, 960, 955, 958, 962, 964, 957, 2608, 969, 959, 961, 4312, 3303, 3131, 3990, 4802 };

        List<(int, int)> MontersLifeOsamodaId = new List<(int, int)>
        {
            ((int)MonsterIdEnum.INFORMO_5813, 70),

            ((int)MonsterIdEnum.DRAGONNET_ROUGE_5798, 210),
            ((int)MonsterIdEnum.DRAGONNET_ALBINOS_5800, 150),
            ((int)MonsterIdEnum.DRAGONNET_MLANIQUE_5799, 90),

            ((int)MonsterIdEnum.CRAPAUD_VERDOYANT_5793, 210),
            ((int)MonsterIdEnum.CRAPAUD_ALBINOS_5794, 150),
            ((int)MonsterIdEnum.CRAPAUD_MLANIQUE_5792, 90),

            ((int)MonsterIdEnum.BOUFTOU_CHTAIN_5796, 210),
            ((int)MonsterIdEnum.BOUFTOU_ALBINOS_5797, 150),
            ((int)MonsterIdEnum.BOUFTOU_MLANIQUE_5795, 90),

            ((int)MonsterIdEnum.TOFU_DOR_5791, 210),
            ((int)MonsterIdEnum.TOFU_ALBINOS_5790, 150),
            ((int)MonsterIdEnum.TOFU_MLANIQUE_5789, 90),
        };

        // > Ordem: A Sacrificada, A Inflavel, A Loka, A Bloqueadora, A Superpoderosa, Arvore
        private List<int> MontersSadidaId = new List<int> { 116, 117, 114, 115, 42, 282 };
        // > Ordem: Cofre Enutrof, Cofre Enutrof, Mochila Enutrof, Mochila Variante Enutrof, Mochila Animada
        private List<int> MontersEnutrofId = new List<int> { 285, 5127, 237, 5125 };
        // > Ordem: Pandawasta (Panda), Bambu Variante (Panda), Quadrante de Xelor (Xelor), Cumplice de Xelor (Xelor), Roleta Ecaflip (Ecaflip), Roleta Ecaflip (Ecaflip), Gatinho Enfurecido (Ecaflip), Gatinho Curandeiro (Ecaflip)
        // > Coelho (Eneripsa), Coelho Protetor (Eneripsa), Cão (Kilorf), Sincro (Xelor)
        private List<int> MontersClassesfId = new List<int> { 516, 5137, 3960, 5144, 5189, 5108, 45, 5107, 39, 4759, 4776, 3958 };

        public SummonedMonster(int id, FightTeam team, FightActor summoner, MonsterGrade template, Cell cell) : base(id, team, template.Spells.ToArray(), summoner, cell, template.MonsterId, template)
        {
            Monster = template;
            Look = Monster.Template.EntityLook.Clone();
            m_stats = new StatsFields(this);
            m_stats.Initialize(template);

            if (Monster.Template.RaceTemplate.SuperRaceId == 28) //Invocations Dopples
                AdjustStats();

            if (Monster.Template.Id == (int)MonsterIdEnum.ROULETTE_5189 || Monster.Template.Id == (int)MonsterIdEnum.ROULETTE_5849) //Roleta Classe
            {
                Summoner.Fight.TurnStarted += SpellRoleta;
            }

            if (Monster.Template.Id == (int)MonsterIdEnum.ROULETTE_5108 || Monster.Template.Id == (int)MonsterIdEnum.ROULETTE_5850) //Roleta Variante
            {
                Team.FighterAdded += OnFighterAddedVariantRoleta;
                this.DamageInflicted += DanoRoleta;
            }
        }

        void SpellRoleta(IFight fight, FightActor player)
        {
            if (player != Summoner)
                return;

            if (player.IsFighterTurn())
                CastAutoSpell(new Spell((int)SpellIdEnum.ROULETTE_12900, 1), Cell);
        }

        void OnFighterAddedVariantRoleta(FightTeam team, FightActor actor)
        {
            if (actor != this)
                return;

            CastAutoSpell(new Spell((int)SpellIdEnum.SPELL_STRIKE_12886, 1), Summoner.Cell);
        }

        void DanoRoleta(FightActor fighter, Damage damage)
        {
            if (fighter != this)
                return;
            if (damage.Source == null)
                return;

            CastAutoSpell(new Spell((int)SpellIdEnum.SPELL_STRIKE_12887, 1), damage.Source.Cell);
        }

        void AdjustStats()
        {
            var osaInvocs = MontersLifeOsamodaId.FirstOrDefault(x => x.Item1 == Monster.Template.Id);
            int lifePoints = osaInvocs != default ? osaInvocs.Item2 : Monster.LifePoints;

            if (Summoner.Level > 200)
            {
                m_stats.Health.Base = (int)(lifePoints * ((1 + (200 / 100d)) * 2));
            }
            else
            {
                m_stats.Health.Base = (int)(lifePoints * ((1 + ((Summoner.Level) / 100d)) * 2));
            }

            switch (this.Monster.Template.Id)
            {
                case int i when MontersDopplesId.Contains(i): //Dopples Monsters
                    if (Summoner is CharacterFighter)
                    {
                        m_stats[PlayerFields.FireDamageBonus].Base = Summoner.Stats[PlayerFields.FireDamageBonus].Equiped / 2;
                        m_stats.Intelligence.Base = (Monster.Intelligence + (1 + (Summoner.Stats.Intelligence.Total / 2)));
                        m_stats.Chance.Base = (Monster.Chance + (1 + (Summoner.Stats.Chance.Total / 2)));
                        m_stats.Agility.Base = (Monster.Agility + (1 + (Summoner.Stats.Agility.Total / 2)));
                        m_stats.Strength.Base = (Monster.Strength + (1 + (Summoner.Stats.Strength.Total / 2)));
                        m_stats.Wisdom.Base = (Monster.Wisdom + (1 + (Summoner.Stats.Wisdom.Total / 2)));
                    }
                    break;
                case int i when MontersLifeOsamodaId.Any(x => x.Item1 == i): //Osamodas Monsters
                    if (Summoner is CharacterFighter)
                    {
                        if (this.Monster.Template.Id == (int)MonsterIdEnum.DRAGONNET_ROUGE_5798 || this.Monster.Template.Id == (int)MonsterIdEnum.DRAGONNET_ALBINOS_5800 || this.Monster.Template.Id == (int)MonsterIdEnum.DRAGONNET_MLANIQUE_5799)
                        {
                            m_stats.Wisdom.Base = Monster.Wisdom + (Summoner.Stats.Wisdom.Total / 2);
                            m_stats.Intelligence.Base = Monster.Intelligence + (Summoner.Stats.Intelligence.Total / 2);
                            m_stats[PlayerFields.HealBonus].Base = Summoner.Stats[PlayerFields.HealBonus].Equiped / 2;
                            m_stats[PlayerFields.FireDamageBonus].Base = Summoner.Stats[PlayerFields.FireDamageBonus].Equiped / 2;
                            m_stats[PlayerFields.CriticalHit].Base = Summoner.Stats[PlayerFields.CriticalHit].Equiped / 2;
                        }
                        else if (this.Monster.Template.Id == (int)MonsterIdEnum.TOFU_DOR_5791 || this.Monster.Template.Id == (int)MonsterIdEnum.TOFU_ALBINOS_5790 || this.Monster.Template.Id == (int)MonsterIdEnum.TOFU_MLANIQUE_5789)
                        {
                            m_stats.Wisdom.Base = Monster.Wisdom + (Summoner.Stats.Wisdom.Total / 2);
                            m_stats.Agility.Base = Monster.Agility + (Summoner.Stats.Agility.Total / 2);
                            m_stats[PlayerFields.TackleEvade].Base = Summoner.Stats[PlayerFields.TackleEvade].Equiped / 2;
                            m_stats[PlayerFields.AirDamageBonus].Base = Summoner.Stats[PlayerFields.AirDamageBonus].Equiped / 2;
                            m_stats[PlayerFields.CriticalHit].Base = Summoner.Stats[PlayerFields.CriticalHit].Equiped / 2;
                        }
                        else if (this.Monster.Template.Id == (int)MonsterIdEnum.BOUFTOU_CHTAIN_5796 || this.Monster.Template.Id == (int)MonsterIdEnum.BOUFTOU_ALBINOS_5797 || this.Monster.Template.Id == (int)MonsterIdEnum.BOUFTOU_MLANIQUE_5795)
                        {
                            m_stats.Wisdom.Base = Monster.Wisdom + (Summoner.Stats.Wisdom.Total / 2);
                            m_stats.Strength.Base = Monster.Strength + (Summoner.Stats.Strength.Total / 2);
                            m_stats.Agility.Base = Monster.Agility + (Summoner.Stats.Agility.Total / 2);
                            m_stats[PlayerFields.TackleBlock].Base = Summoner.Stats[PlayerFields.TackleBlock].Equiped / 2;
                            m_stats[PlayerFields.EarthDamageBonus].Base = Summoner.Stats[PlayerFields.EarthDamageBonus].Equiped / 2;
                            m_stats[PlayerFields.NeutralDamageBonus].Base = Summoner.Stats[PlayerFields.NeutralDamageBonus].Equiped / 2;
                            m_stats[PlayerFields.CriticalHit].Base = Summoner.Stats[PlayerFields.CriticalHit].Equiped / 2;
                        }
                        else if (this.Monster.Template.Id == (int)MonsterIdEnum.CRAPAUD_VERDOYANT_5793 || this.Monster.Template.Id == (int)MonsterIdEnum.CRAPAUD_ALBINOS_5794 || this.Monster.Template.Id == (int)MonsterIdEnum.CRAPAUD_MLANIQUE_5792)
                        {
                            m_stats.Wisdom.Base = Monster.Wisdom + (Summoner.Stats.Wisdom.Total / 2);
                            m_stats.Chance.Base = Monster.Chance + (Summoner.Stats.Chance.Total / 2);
                            m_stats[PlayerFields.NeutralDamageBonus].Base = Summoner.Stats[PlayerFields.NeutralDamageBonus].Equiped / 2;
                            m_stats[PlayerFields.WaterDamageBonus].Base = Summoner.Stats[PlayerFields.WaterDamageBonus].Equiped / 2;
                            m_stats[PlayerFields.CriticalHit].Base = Summoner.Stats[PlayerFields.CriticalHit].Equiped / 2;
                        }
                    }
                    break;
                case int i when MontersSadidaId.Contains(i): //Sadida Monsters
                    if (Summoner is CharacterFighter)
                    {
                        m_stats[PlayerFields.FireDamageBonus].Base = Summoner.Stats[PlayerFields.FireDamageBonus].Equiped / 2;
                        m_stats.Intelligence.Base = (Monster.Intelligence + (1 + (Summoner.Stats.Intelligence.Total / 2)));
                        m_stats.Chance.Base = (Monster.Chance + (1 + (Summoner.Stats.Chance.Total / 2)));
                        m_stats.Agility.Base = (Monster.Agility + (1 + (Summoner.Stats.Agility.Total / 2)));
                        m_stats.Strength.Base = (Monster.Strength + (1 + (Summoner.Stats.Strength.Total / 2)));
                        m_stats.Wisdom.Base = (Monster.Wisdom + (1 + (Summoner.Stats.Wisdom.Total / 2)));
                    }
                    break;
                case int i when MontersEnutrofId.Contains(i): //Enutrof Monsters
                    if (Summoner is CharacterFighter)
                    {
                        m_stats[PlayerFields.FireDamageBonus].Base = Summoner.Stats[PlayerFields.FireDamageBonus].Equiped / 2;
                        m_stats.Intelligence.Base = (Monster.Intelligence + (1 + (Summoner.Stats.Intelligence.Total / 2)));
                        m_stats.Chance.Base = (Monster.Chance + (1 + (Summoner.Stats.Chance.Total / 2)));
                        m_stats.Agility.Base = (Monster.Agility + (1 + (Summoner.Stats.Agility.Total / 2)));
                        m_stats.Strength.Base = (Monster.Strength + (1 + (Summoner.Stats.Strength.Total / 2)));
                        m_stats.Wisdom.Base = (Monster.Wisdom + (1 + (Summoner.Stats.Wisdom.Total / 2)));
                    }
                    break;
                case int i when MontersClassesfId.Contains(i): //Classes Monsters
                    if (Summoner is CharacterFighter)
                    {
                        m_stats[PlayerFields.FireDamageBonus].Base = Summoner.Stats[PlayerFields.FireDamageBonus].Equiped / 2;
                        m_stats.Intelligence.Base = (Monster.Intelligence + (1 + (Summoner.Stats.Intelligence.Total / 2)));
                        m_stats.Chance.Base = (Monster.Chance + (1 + (Summoner.Stats.Chance.Total / 2)));
                        m_stats.Agility.Base = (Monster.Agility + (1 + (Summoner.Stats.Agility.Total / 2)));
                        m_stats.Strength.Base = (Monster.Strength + (1 + (Summoner.Stats.Strength.Total / 2)));
                        m_stats.Wisdom.Base = (Monster.Wisdom + (1 + (Summoner.Stats.Wisdom.Total / 2)));
                    }
                    break;
                default:
                    m_stats[PlayerFields.Intelligence].Base = Summoner.Stats[PlayerFields.Intelligence].Equiped / 2;
                    m_stats.Intelligence.Base = (Monster.Intelligence + (2 + (Summoner.Stats.Intelligence.Total / 2)));
                    m_stats.Chance.Base = (Monster.Chance + (2 + (Summoner.Stats.Chance.Total / 2)));
                    m_stats.Agility.Base = (Monster.Agility + (2 + (Summoner.Stats.Agility.Total / 2)));
                    m_stats.Strength.Base = (Monster.Strength + (2 + (Summoner.Stats.Strength.Total / 2)));
                    m_stats.Wisdom.Base = (Monster.Wisdom + (2 + (Summoner.Stats.Wisdom.Total / 2)));
                    break;
            }

            List<StatsData> stat = new List<StatsData>
            {
                m_stats.Health,
                m_stats.Intelligence,
                m_stats.Chance,
                m_stats.Strength,
                m_stats.Agility,
                m_stats.Wisdom
            };

            stat.AddRange(stat);
        }

        public override int CalculateArmorValue(int reduction)
        {
            return (int)(reduction * (100 + 5 * Summoner.Level) / 100d);
        }

        public override bool CanPlay() => base.CanPlay() && Monster.Template.CanPlay;

        public override bool CanMove() => base.CanMove() && MonsterGrade.MovementPoints > 0;

        public override bool CanTackle(FightActor fighter) => base.CanTackle(fighter) && Monster.Template.CanTackle;

        public MonsterGrade Monster
        {
            get;
        }

        public override ObjectPosition MapPosition
        {
            get { return Position; }
        }

        public override ushort Level
        {
            get { return (byte)Monster.Level; }
        }

        public override bool Vip
        {
            get { return false; }
        }

        public override RoleEnum Role
        {
            get { return RoleEnum.Player; }
        }

        public override Character Owner => (Summoner as CharacterFighter).Character;

        public MonsterGrade MonsterGrade
        {
            get { return Monster; }
        }

        public override StatsFields Stats
        {
            get { return m_stats; }
        }

        public override string GetMapRunningFighterName()
        {
            return Monster.Id.ToString(CultureInfo.InvariantCulture);
        }

        public override string Name
        {
            get { return Monster.Template.Name; }
        }

        public override bool CanBePushed()
        {
            return base.CanBePushed() && Monster.Template.CanBePushed;
        }

        public override bool CanSwitchPos()
        {
            return base.CanSwitchPos() && Monster.Template.CanSwitchPos;
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

        public override GameFightFighterLightInformations GetGameFightFighterLightInformations(WorldClient client = null)
        {
            return new GameFightFighterMonsterLightInformations(
                sex: true,
                alive: IsAlive(),
                id: Id,
                wave: 0,
                level: Level,
                breed: (sbyte)BreedEnum.MONSTER,
                creatureGenericId: (ushort)Monster.Template.Id);
        }

        public override GameFightCharacteristics GetGameFightMinimalStats(WorldClient client = null)
        {
            return new GameFightCharacteristics(
                characteristics: new CharacterCharacteristics(GetFightActorCharacteristic()),
                summoner: Summoner.Id,
                summoned: true,
                invisibilityState: (sbyte)(client == null ? VisibleState : GetVisibleStateFor(client.Character)));
        }

        public override FightTeamMemberInformations GetFightTeamMemberInformations()
        {
            return new FightTeamMemberMonsterInformations(id: Id, monsterId: Monster.Template.Id, grade: (sbyte)Monster.GradeId);
        }
    }
}