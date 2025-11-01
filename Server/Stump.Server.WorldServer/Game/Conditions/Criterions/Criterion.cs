using System;
using System.Collections.Generic;

namespace Stump.Server.WorldServer.Game.Conditions.Criterions
{
    public abstract class Criterion : ConditionExpression
    {
        public ComparaisonOperatorEnum Operator
        {
            get;
            set;
        }

        public string Literal
        {
            get;
            set;
        }

        public abstract void Build();

        private static readonly Dictionary<string, Func<Criterion>> CriterionConstructors = new Dictionary<string, Func<Criterion>>
        {
            //Achievement
            { AchievementCriterion.Identifier, () => new AchievementCriterion() },

            //Guilds - Alliance
            { GuildLevelCriterion.Identifier, () => new GuildLevelCriterion() },
            { GuildRightsCriterion.Identifier, () => new GuildRightsCriterion() },
            { GuildValidCriterion.Identifier, () => new GuildValidCriterion() },
            { AllianceValidCriterion.Identifier, () => new AllianceValidCriterion() },

            //Aligment
            { AlignementLevelCriterion.Identifier, () => new AlignementLevelCriterion() },
            { AlignmentCriterion.Identifier, () => new AlignmentCriterion() },

            //SubArea - Maps - Cells
            { AreaCriterion.Identifier, () => new AreaCriterion() },
            { SubAreaCriterion.Identifier, () => new SubAreaCriterion() },
            { CellOccupedCriterion.Identifier, () => new CellOccupedCriterion() },

            //Character
            { CharacterAscensionCriterion.Identifier, () => new CharacterAscensionCriterion() },
            { CharacterGroupCriterion.Identifier, () => new CharacterGroupCriterion() },

            //Events
            { EventPPActiveCriterion.Identifier, () => new EventPPActiveCriterion() },
            { EventPPNpcActiveCriterion.Identifier, () => new EventPPNpcActiveCriterion() },
            { EventAutoActiveCriterion.Identifier, () => new EventAutoActiveCriterion() },
            { EventPaskwakActiveCriterion.Identifier, () => new EventPaskwakActiveCriterion() },
            { EventNatawActiveCriterion.Identifier, () => new EventNatawActiveCriterion() },
            { EventKoliseuCriterion.Identifier, () => new EventKoliseuCriterion() },

            //Quest
            { QuestActiveCriterion.Identifier, () => new QuestActiveCriterion() },
            { QuestObjectiveCriterion.Identifier, () => new QuestObjectiveCriterion() },
            { QuestDoneCriterion.Identifier, () => new QuestDoneCriterion() },
            { QuestStartableCriterion.Identifier, () => new QuestStartableCriterion() },
            { QuestNoAnyActiveCriterion.Identifier, () => new QuestNoAnyActiveCriterion() },
            { QuestObjectiveStepsCriterion.Identifier, () => new QuestObjectiveStepsCriterion() },

            //QuestDopples
            { TimeDopplesCriterion.Identifier, () => new TimeDopplesCriterion() },

            //Has
            { HasMandatory.Identifier, () => new HasMandatory() },
            { HasItemCriterion.Identifier, () => new HasItemCriterion() },
            { HasItemEquippedCriterion.Identifier, () => new HasItemEquippedCriterion() },
            { HasMountBehavior.Identifier, () => new HasMountBehavior() },
            { HasOrnamentCriterion.Identifier, () => new HasOrnamentCriterion() },
            { HasSpellCriterion.Identifier, () => new HasSpellCriterion() },
            { HasTitleCriterion.Identifier, () => new HasTitleCriterion() },
            { HasTitleCriterion.Identifier2, () => new HasTitleCriterion() },
            { HasManyItemsCriterion.Identifier, () => new HasManyItemsCriterion() },

            //Pvp
            { PvpRankCriterion.Identifier, () => new PvpRankCriterion() },
            { PvpRankCriterion.Identifier2, () => new PvpRankCriterion() },

            //Dungeon
            { DungeonReturnCriterion.Identifier, () => new DungeonReturnCriterion() },

            //Other
            { AdminRightsCriterion.Identifier, () => new AdminRightsCriterion() },
            { BonusSetCriterion.Identifier, () => new BonusSetCriterion() },
            { BonesCriterion.Identifier, () => new BonesCriterion() },
            { BreedCriterion.Identifier, () => new BreedCriterion() },
            { EmoteCriterion.Identifier, () => new EmoteCriterion() },
            { FriendListCriterion.Identifier, () => new FriendListCriterion() },
            { GiftCriterion.Identifier, () => new GiftCriterion() },
            { CreateDaysCriterion.Identifier, () => new CreateDaysCriterion() },
            { InteractiveStateCriterion.Identifier, () => new InteractiveStateCriterion() },
            { JobCriterion.Identifier, () => new JobCriterion() },
            { JobCriterion.Identifier1, () => new JobCriterion() },
            { KamaCriterion.Identifier, () => new KamaCriterion() },
            { LevelCriterion.Identifier, () => new LevelCriterion() },
            { MapCharactersCriterion.Identifier, () => new MapCharactersCriterion() },
            { MapCriterion.Identifier, () => new MapCriterion() },
            { OgrinesQuantCriterion.Identifier, () => new OgrinesQuantCriterion() },
            { MariedCriterion.Identifier, () => new MariedCriterion() },
            { MaxRankCriterion.Identifier, () => new MaxRankCriterion() },
            { MonthCriterion.Identifier, () => new MonthCriterion() },
            { DayCriterion.Identifier, () => new DayCriterion() },
            { NameCriterion.Identifier, () => new NameCriterion() },
            { PreniumAccountCriterion.Identifier, () => new PreniumAccountCriterion() },
            { RankCriterion.Identifier, () => new RankCriterion() },
            { RideCriterion.Identifier, () => new RideCriterion() },
            { ServerCriterion.Identifier, () => new ServerCriterion() },
            { SexCriterion.Identifier, () => new SexCriterion() },
            { SkillCriterion.Identifier, () => new SkillCriterion() },
            { SkillCriterion.Identifier2, () => new SkillCriterion() },
            { SmileyPackCriterion.Identifier, () => new SmileyPackCriterion() },
            { SoulStoneCriterion.Identifier, () => new SoulStoneCriterion() },
            { SpecializationCriterion.Identifier, () => new SpecializationCriterion() },
            { StaticCriterion.Identifier, () => new StaticCriterion() },
            { StaticCriterion.Identifier1, () => new StaticCriterion() },
            { SubscribeCriterion.Identifier, () => new SubscribeCriterion() },
            { SubscriptionTimeCriterion.Identifier, () => new SubscriptionTimeCriterion() },
            { SubscribeZoneCriterion.Identifier, () => new SubscribeZoneCriterion() },
            { UnusableCriterion.Identifier, () => new UnusableCriterion() },
            { UnknownCriterion.Identifier, () => new UnknownCriterion() },
            { UnknownCriterion.Identifier1, () => new UnknownCriterion() },
            { UnknownCriterion.Identifier2, () => new UnknownCriterion() },
            { UnknownCriterion.Identifier3, () => new UnknownCriterion() },
            { WeightCriterion.Identifier, () => new WeightCriterion() },
            { MountFamilyItemCriterion.Identifier, () => new MountFamilyItemCriterion() },
            { HuntingCriterion.Identifier, () => new HuntingCriterion() },
            { VipCriterion.Identifier, () => new VipCriterion() },
            { "Oc", () => new IgnoreCriterion() },
            { "Ox", () => new AllianceRightsCriterion() },

            //Unknown
            { UnknownFalseCriterion.Identifier, () => new UnknownFalseCriterion() },
            { UnknownFalseCriterion.Identifier1, () => new UnknownFalseCriterion() },
            { UnknownFalseCriterion.Identifier2, () => new UnknownFalseCriterion() },
            { UnknownFalseCriterion.Identifier3, () => new UnknownFalseCriterion() },
        };

        public static Criterion CreateCriterionByName(string name)
        {
            if (StatsCriterion.IsStatsIdentifier(name))
                return new StatsCriterion(name);

            if (CriterionConstructors.TryGetValue(name, out var constructor))
            {
                return constructor();
            }

            throw new Exception($"Criterion {name} does not exist or is not handled");
        }

        public static ComparaisonOperatorEnum? TryGetOperator(char c)
        {
            switch (c)
            {
                case '=':
                    return ComparaisonOperatorEnum.EQUALS;

                case '!':
                    return ComparaisonOperatorEnum.INEQUALS;

                case '>':
                    return ComparaisonOperatorEnum.SUPERIOR;

                case '<':
                    return ComparaisonOperatorEnum.INFERIOR;

                case '~':
                    return ComparaisonOperatorEnum.LIKE;

                // it uses symboles of criterions identifier ...
                // todo : compare the 2 first letters
                /*case 's':
                    return ComparaisonOperatorEnum.STARTWITH;

                case 'S':
                    return ComparaisonOperatorEnum.STARTWITHLIKE;

                case 'e':
                    return ComparaisonOperatorEnum.ENDWITH;

                case 'E':
                    return ComparaisonOperatorEnum.ENDWITHLIKE;

                case 'i':
                    return ComparaisonOperatorEnum.INVALID;

                case 'v':
                    return ComparaisonOperatorEnum.VALID;

                case '#':
                    return ComparaisonOperatorEnum.UNKNOWN_1;

                case '/':
                    return ComparaisonOperatorEnum.UNKNOWN_2;

                case 'X':
                    return ComparaisonOperatorEnum.UNKNOWN_3;*/
                default:
                    return null;
            }
        }

        public static char GetOperatorChar(ComparaisonOperatorEnum op)
        {
            switch (op)
            {
                case ComparaisonOperatorEnum.EQUALS:
                    return '=';

                case ComparaisonOperatorEnum.INEQUALS:
                    return '!';

                case ComparaisonOperatorEnum.SUPERIOR:
                    return '>';

                case ComparaisonOperatorEnum.INFERIOR:
                    return '<';

                case ComparaisonOperatorEnum.LIKE:
                    return '~';

                case ComparaisonOperatorEnum.STARTWITH:
                    return 's';

                case ComparaisonOperatorEnum.STARTWITHLIKE:
                    return 'S';

                case ComparaisonOperatorEnum.ENDWITH:
                    return 'e';

                case ComparaisonOperatorEnum.ENDWITHLIKE:
                    return 'E';

                case ComparaisonOperatorEnum.INVALID:
                    return 'i';

                case ComparaisonOperatorEnum.VALID:
                    return 'v';

                case ComparaisonOperatorEnum.UNKNOWN_1:
                    return '#';

                case ComparaisonOperatorEnum.UNKNOWN_2:
                    return '/';

                case ComparaisonOperatorEnum.UNKNOWN_3:
                    return 'X';

                default:
                    throw new Exception(string.Format("{0} is not a valid comparaison operator", op));
            }
        }

        protected bool Compare(object obj, object comparand)
        {
            switch (Operator)
            {
                case ComparaisonOperatorEnum.EQUALS:
                    return obj.Equals(comparand);

                case ComparaisonOperatorEnum.INEQUALS:
                    return !obj.Equals(comparand);

                default:
                    throw new NotImplementedException(string.Format("Cannot use {0} comparator on objects {1} and {2}", Operator, obj, comparand));
            }
        }

        protected bool Compare<T>(IComparable<T> obj, T comparand)
        {
            var comparaison = obj.CompareTo(comparand);

            switch (Operator)
            {
                case ComparaisonOperatorEnum.EQUALS:
                    return comparaison == 0;

                case ComparaisonOperatorEnum.INEQUALS:
                    return comparaison != 0;

                case ComparaisonOperatorEnum.INFERIOR:
                    return comparaison < 0;

                case ComparaisonOperatorEnum.SUPERIOR:
                    return comparaison > 0;

                default:
                    throw new NotImplementedException(string.Format("Cannot use {0} comparator on IComparable {1} and {2}", Operator, obj, comparand));
            }
        }

        protected bool Compare(string str, string comparand)
        {
            switch (Operator)
            {
                case ComparaisonOperatorEnum.EQUALS:
                    return str == comparand;

                case ComparaisonOperatorEnum.INEQUALS:
                    return str != comparand;

                case ComparaisonOperatorEnum.LIKE:
                    return str.Equals(comparand, StringComparison.InvariantCultureIgnoreCase);

                case ComparaisonOperatorEnum.STARTWITH:
                    return str.StartsWith(comparand);

                case ComparaisonOperatorEnum.ENDWITH:
                    return str.EndsWith(comparand);

                case ComparaisonOperatorEnum.STARTWITHLIKE:
                    return str.StartsWith(comparand, StringComparison.InvariantCultureIgnoreCase);

                case ComparaisonOperatorEnum.ENDWITHLIKE:
                    return str.EndsWith(comparand, StringComparison.InvariantCultureIgnoreCase);

                default:
                    throw new NotImplementedException(string.Format("Cannot use {0} comparator on strings '{1}' and '{2}'", Operator, str, comparand));
            }
        }

        protected string FormatToString(string identifier)
        {
            return string.Format("{0}{1}{2}", identifier, GetOperatorChar(Operator), Literal);
        }
    }
}