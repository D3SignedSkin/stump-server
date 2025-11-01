using System;
using System.Collections.Generic;
using System.Linq;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Database.Monsters;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Monsters;
using Stump.Server.WorldServer.Game.Fights.Results;

namespace Stump.Server.WorldServer.Game.Formulas
{
    public class FightFormulas
    {
        public static event Func<IFightResult, double, double> WinXpModifier;

        public static event Func<IFightResult, long, long> WinKamasModifier;

        public static event Func<IFightResult, DroppableItem, double, double> DropRateModifier;

        public static event Func<IFightResult, DroppableDofusItem, double, double> DropDofusRateModifier;

        private int KamasAfter = 0;

        private static readonly int[] DofusItemsIds =
        {
            7115,//Dofus Marfim
            737,//Dofus Esmeralda
            739,//Dofus Turquesa
            7754,//Dofus Ocre
            6980,//Dofus Vulbis
            694,//Dofus Púrpura
            972,//Dofus Cenouwa
            17078,//Dokoko
            19398,//Dofus Forjalava
            15235,//Dotruz
            8072,//Dofus kaliptus
            7043,//Dofus do Gelo
            13344,//Dolmanax
            18043,//Dofus Abissal
            16061,//Dofus dos Vigilantes
            19629,//Dofus Prateado
        };

        public static int[] GetDofusItemsIds
        {
            get { return DofusItemsIds; }
        }

        public static readonly double[] GroupCoefficients =
        {
            1,
            1.1,
            1.5,
            2.3,
            3.1,
            3.6,
            4.2,
            4.7
        };

        public static long InvokeWinKamasModifier(IFightResult looter, long kamas)
        {
            var handler = WinKamasModifier;

            return handler != null ? handler(looter, kamas) : kamas;
        }

        public static double InvokeDropRateModifier(IFightResult looter, DroppableItem item, double rate)
        {
            var handler = DropRateModifier;

            return handler != null ? handler(looter, item, rate) : rate;
        }

        public static double InvokeDropRateModifier(IFightResult looter, DroppableDofusItem item, double rate)
        {
            var handler = DropDofusRateModifier;

            return handler != null ? handler(looter, item, rate) : rate;
        }

        public static double InvokeWinXpModifier(IFightResult looter, double xp)
        {
            long limit;

            if (looter is FightPlayerResult && (looter as FightPlayerResult).Character != null)
            {
                if ((looter as FightPlayerResult).Character.UserGroup.Role <= RoleEnum.Player)
                {
                    limit = new Random().Next(997083010, 1094508070);
                }
                else if ((looter as FightPlayerResult).Character.UserGroup.Role == RoleEnum.Vip)
                {
                    limit = new Random().Next(1397083010, 1524508070);
                }
                else if ((looter as FightPlayerResult).Character.UserGroup.Role >= RoleEnum.Gold_Vip)
                {
                    limit = new Random().Next(1867083010, int.MaxValue);
                }
                else
                {
                    limit = new Random().Next(997083010, 1094508070);
                }
            }
            else
            {
                limit = new Random().Next(997083010, 1094508070);
            }

            xp = xp > limit ? limit : xp;
            var handler = WinXpModifier;

            return handler != null ? handler(looter, xp) : xp;
        }

        public static int CalculateProtect(int kamas)
        {
            int MaxKamas = 6500000;
            int limit = new Random().Next(5530326, 6093326);

            kamas = kamas >= MaxKamas ? limit : kamas;

            return kamas;
        }

        public static int CalculatePushBackDamages(FightActor source, FightActor target, int range, int targets)
        {
            var level = source.Level;
            var summon = source as SummonedMonster;

            if (summon != null)
                level = summon.Summoner.Level;

            return (int)((level / 2 + (source.Stats[PlayerFields.PushDamageBonus] - target.Stats[PlayerFields.PushDamageReduction]) + 32) * range / (4 * (Math.Pow(2, targets))));
        }

        public static double RatesDimension(int m_subarea)
        {
            double PercentValor = 0;

            if (m_subarea == 820)
                return Rates.BonusKamasInEnutropia_Center;
            else if (m_subarea == 821)
                return Rates.BonusKamasInEnutropia_Right;
            else if (m_subarea == 822)
                return Rates.BonusKamasInEnutropia_Left;
            else if (m_subarea == 826)
                return Rates.BonusKamasInEnutropia_Top;
            else if (m_subarea == 823)
                return Rates.BonusKamasInEnutropia_Dung_Male;
            else if (m_subarea == 824)
                return Rates.BonusKamasInEnutropia_Dung_Gallery;
            else if (m_subarea == 825)
                return Rates.BonusKamasInEnutropia_Dung_Nidas;

            else return PercentValor;
        }

        #region // ---------------- Formula XP + Challenge + Idolos By: Kenshin ------------------//
        public static double CalculateWinExp(IFightResult fighter, IEnumerable<FightActor> alliesResults, IEnumerable<FightActor> droppersResults)
        {
            var droppers = droppersResults.ToArray();
            var allies = alliesResults.ToArray();

            if (!droppers.Any() || !allies.Any())
                return 0;

            var sumPlayersLevel = allies.Sum(entry => entry.Level);
            var maxPlayerLevel = allies.Max(entry => entry.Level);
            var sumMonstersLevel = droppers.Sum(entry => entry.Level);
            var sumMonstersHiddenLevel = droppers.OfType<MonsterFighter>().Sum(entry => entry.HiddenLevel == 0 ? entry.Level : entry.HiddenLevel);
            var maxMonsterLevel = droppers.Max(entry => entry.Level);
            var sumMonsterXp = droppers.Sum(entry => entry.GetGivenExperience());
            double levelCoeff = 1;

            if (sumPlayersLevel - 5 > sumMonstersLevel)
                levelCoeff = (double)sumMonstersLevel / sumPlayersLevel;
            else if (sumPlayersLevel + 10 < sumMonstersLevel)
                levelCoeff = (sumPlayersLevel + 10) / (double)sumMonstersLevel;

            var xpRatio = Math.Min(fighter.Level, Math.Truncate(2.5d * maxMonsterLevel)) / sumPlayersLevel * 100d;
            var regularGroupRatio = allies.Where(entry => entry.Level >= maxPlayerLevel / 3).Sum(entry => 1);

            if (regularGroupRatio <= 0)
                regularGroupRatio = 1;

            var baseXp = Math.Truncate(xpRatio / 100 * Math.Truncate(sumMonsterXp * GroupCoefficients[regularGroupRatio - 1] * levelCoeff));

            /* Iniciando o Calculo de porcentos de bonus*/
            long CalculateExperience = 0;
            long FinalExperience = 0;
            double MonoRate = 0;
            double RatesPercents = 0;
            double RatePercent = fighter.Role >= RoleEnum.Gold_Vip ? Rates.GoldXpRate : fighter.Role == RoleEnum.Vip ? Rates.VipXpRate : Rates.XpRate;

            /* Weekend Bonus */
            double WeekPlayerValue = 0;

            if (Settings.Weekend && DateTime.Now.DayOfWeek == DayOfWeek.Saturday || DateTime.Now.DayOfWeek == DayOfWeek.Sunday)
            {
                WeekPlayerValue = fighter.Role >= RoleEnum.Gold_Vip ? Settings.WeekGoldVipValue : fighter.Role == RoleEnum.Vip ? Settings.WeekVipValue : Settings.WeekPlayerValue;
            }

            var AnomalyBonus = fighter.Fight.AgeBonus <= 0 ? 0 : fighter.Fight.AgeBonus;
            var challengeBonus = fighter.Fight.ChallengeDropOrXp == (sbyte)ChallengeBonusEnum.EXPERIENCE_BONUS_0 ? fighter.Fight.GetChallengesBonus() : 0;

            /* Atribuindo mais 150% de Bonus para jogadores Monoconta*/
            if (fighter is FightPlayerResult player)
            {
                bool hasSameIpAlly = allies.OfType<CharacterFighter>().Any(ally => ally.Character != player.Character && ally.Character.Client.IP == player.Character.Client.IP);
                MonoRate = hasSameIpAlly ? 0 : MonoRate = 150;
            }

            RatesPercents = (fighter.Wisdom / 100) + RatePercent + challengeBonus + AnomalyBonus + MonoRate + WeekPlayerValue;
            CalculateExperience = (long)Math.Round(baseXp + (baseXp * RatesPercents));
            FinalExperience = CalculateExperience >= int.MaxValue ? int.MaxValue : CalculateExperience;

            if (fighter is FightPlayerResult)
            {
                if (fighter.Role >= RoleEnum.GameMaster_Padawan)
                {
                    (fighter as FightPlayerResult).Character.SendServerMessage
                    (
                    $"" +
                    $">XP Base = {baseXp}K, " +
                    $">RatePercent = {RatePercent}%, " +
                    $">WisPercent = {fighter.Wisdom / 100}%, " +
                    $">AnomalyBonus = {AnomalyBonus}%, " +
                    $">ChallengeBonus = {challengeBonus}%, " +
                    $">MonoRate = {MonoRate}%, " +
                    $">WeekPlayerValue = {WeekPlayerValue}%, " +
                    $">TotalPercents = {RatesPercents}, " +
                    $"XP Final = {FinalExperience}. "
                    );
                }
            }

            return InvokeWinXpModifier(fighter, FinalExperience);
        }
        #endregion

        #region // ---------------- Formula Dropped Kamas Comum e Event By: Kenshin ------------------//
        public static int AdjustDroppedKamas(IFightResult looter, int TeamPP, long baseKamas, bool kamasRate = true)
        {
            double OwnerRatePercent = kamasRate ? looter.Role >= RoleEnum.Gold_Vip ? Rates.GoldKamasRate : looter.Role == RoleEnum.Vip ? Rates.VipKamasRate : Rates.KamasRate : 0;
            double RatesPercents = 0;

            //Jogador Dados
            var m_area = looter.Fight.Map.Area.Id;
            var m_subarea = looter.Fight.Map.SubArea.Id;

            /* Weekend Bonus */
            double WeekPlayerValue = 0;

            if (Settings.Weekend && DateTime.Now.DayOfWeek == DayOfWeek.Saturday || DateTime.Now.DayOfWeek == DayOfWeek.Sunday)
            {
                WeekPlayerValue = looter.Role >= RoleEnum.Gold_Vip ? Settings.WeekGoldVipValue : looter.Role == RoleEnum.Vip ? Settings.WeekVipValue : Settings.WeekPlayerValue;
            }

            bool IsMobLevelLow = false;
            var TeamLevel = looter.Fight.ChallengersTeam.Fighters.Select(x => x.Level > 200 ? 200 : x.Level).Sum();
            var MonsterLevel = looter.Fight.DefendersTeam.Fighters.Sum(x => x.Level);

            var MonsterLevelPercent = looter.Fight.DefendersTeam.Fighters.Sum(x => x.Level) / 100 < 1 ? 1 : looter.Fight.DefendersTeam.Fighters.Sum(x => x.Level) / 100;
            var TeamCount = looter.Fight.ChallengersTeam.Fighters.Count();
            int TeamAverage = TeamLevel / TeamCount;
            var challengeBonus = looter.Fight.GetChallengesBonus();
            var LooterPP = looter.Prospecting / TeamPP;
            var AnomalyBonus = looter.Fight.AgeBonus <= 0 ? 0 : looter.Fight.AgeBonus;
            double ratesdimension = m_area == 53 && Rates.BonusKamasInEnutropia ? RatesDimension(m_subarea) : 0;

            if (MonsterLevel < TeamLevel && Math.Abs(TeamLevel - MonsterLevel) > 50)
            {
                RatesPercents = (LooterPP + challengeBonus + MonsterLevel + AnomalyBonus + OwnerRatePercent + ratesdimension + WeekPlayerValue) / TeamCount <= 4 ? 4 : TeamCount;
                IsMobLevelLow = true;
            }
            else
            {
                RatesPercents = LooterPP + challengeBonus + MonsterLevel + AnomalyBonus + OwnerRatePercent + ratesdimension + WeekPlayerValue;
            }

            int kamas = 0;
            int FinalKamas = 0;
            int AfterKamas = 0;

            kamas = (int)Math.Round(baseKamas + (RatesPercents * baseKamas));
            FinalKamas = kamas >= int.MaxValue ? int.MaxValue : kamas;

            AfterKamas = FinalKamas;

            if (looter is FightPlayerResult && looter.Role >= RoleEnum.GameMaster_Padawan)
            {
                if (IsMobLevelLow)
                {
                    (looter as FightPlayerResult).Character.SendServerMessage
                    (
                    $"" +
                    $">Kamas Base = {baseKamas} K, " +
                    $">IsMobLevelLow = {IsMobLevelLow}, " +
                    $">OwnerRate = {OwnerRatePercent / 4}%, " +
                    $">MonsterPercent = {MonsterLevelPercent / 4}%, " +
                    $">ChallengeBonus = {challengeBonus / 4}%, " +
                    $">LooterPP = {LooterPP / 4}%, " +
                    $">AnomalyBonus = {AnomalyBonus / 4}%, " +
                    $">RatesDimension = {ratesdimension / 4}%, " +
                    $">WeekPlayerValue = {WeekPlayerValue / 4}%, " +
                    $"TotalPercent = {RatesPercents}%, " +
                    $"Kamas Final = {FinalKamas}. "
                    );
                }
                else
                {
                    (looter as FightPlayerResult).Character.SendServerMessage
                    (
                    $"" +
                    $">Kamas Base = {baseKamas} K, " +
                    $">IsMobLevelLow = {IsMobLevelLow}, " +
                    $">OwnerRate = {OwnerRatePercent}%, " +
                    $">MonsterPercent = {MonsterLevelPercent}%, " +
                    $">ChallengeBonus = {challengeBonus}%, " +
                    $">LooterPP = {LooterPP}%, " +
                    $">AnomalyBonus = {AnomalyBonus}%, " +
                    $">RatesDimension = {ratesdimension}%, " +
                    $">WeekPlayerValue = {WeekPlayerValue}%, " +
                    $"TotalPercent = {RatesPercents}%, " +
                    $"Kamas Final = {FinalKamas}. "
                    );
                }
            }

            FinalKamas = CalculateProtect(FinalKamas);

            return (int)InvokeWinKamasModifier(looter, FinalKamas);
        }
        #endregion

        #region // ---------------- Formula Dropped Chance By: Kenshin ------------------//
        public static double AdjustDropChance(IFightResult looter, DroppableItem item, Monster dropper, int monsterAgeBonus)
        {
            var challengeBonus = looter.Fight.ChallengeDropOrXp == (sbyte)ChallengeBonusEnum.DROP_BONUS_1 ? looter.Fight.GetChallengesBonus() : 0;
            var looterPP = looter.Prospecting * (1 + (challengeBonus / 100d));

            double RatePercent = looter.Role >= RoleEnum.Gold_Vip ? Rates.GoldKamasRate : looter.Role == RoleEnum.Vip ? Rates.VipKamasRate : Rates.KamasRate;
            double rate = 0;

            if (dropper.Template.Race == 18)//dople temple
            {
                var monsterGradeId = 1;

                while (monsterGradeId < 5)
                {
                    if (dropper.Grade.GradeId > monsterGradeId * 2.4)
                        monsterGradeId++;
                    else
                        break;
                }

                rate = item.GetDropRate(monsterGradeId) * (looterPP / 100d);//
            }
            else if (looter is FightPlayerResult)
            {
                if ((looter as FightPlayerResult).Fighter.Team.Fighters.Where(x => x is CharacterFighter).Select(x => (x as CharacterFighter)).Any(y => y.Character.Client.IP == (looter as FightPlayerResult).Character.Client.IP && y.Character != (looter as FightPlayerResult).Character))
                {
                    if (looter.Vip)
                        if (DofusItemsIds.Contains(item.ItemId))
                        {
                            var grade_temp = item.GetDropRate((int)dropper.Grade.GradeId);
                            rate = (grade_temp * (looterPP / 100d) * (RatePercent / 100)) > (grade_temp * 2) ? (grade_temp * 2) : (grade_temp * (looterPP / 100d)); //* Rates.VipDropRate);
                        }
                        else
                            rate = item.GetDropRate((int)dropper.Grade.GradeId) * (looterPP / 100d) * ((monsterAgeBonus / 100d) + 1) * (RatePercent / 100);
                    else
                    {
                        if (DofusItemsIds.Contains(item.ItemId))
                        {
                            var grade_temp = item.GetDropRate((int)dropper.Grade.GradeId);
                            rate = (grade_temp * (looterPP / 100d) * (RatePercent / 100)) > (grade_temp * 2) ? (grade_temp * 2) : (grade_temp * (looterPP / 100d)); //* Rates.DropsRate);
                        }
                        else
                            rate = item.GetDropRate((int)dropper.Grade.GradeId) * (looterPP / 100d) * ((monsterAgeBonus / 100d) + 1) * (RatePercent / 100);
                    }
                }
                else
                {
                    if (looter.Vip)
                        if (DofusItemsIds.Contains(item.ItemId))
                        {
                            var grade_temp = item.GetDropRate((int)dropper.Grade.GradeId);
                            rate = (grade_temp * (looterPP / 100d) * (RatePercent / 100)) > (grade_temp * 2) ? (grade_temp * 2) : (grade_temp * (looterPP / 100d)); //* Rates.VipDropRate);
                        }
                        else
                            rate = item.GetDropRate((int)dropper.Grade.GradeId) * (looterPP / 100d) * ((monsterAgeBonus / 100d) + 1) * ((RatePercent / 100) * 1.50);
                    else
                    {
                        if (DofusItemsIds.Contains(item.ItemId))
                        {
                            var grade_temp = item.GetDropRate((int)dropper.Grade.GradeId);
                            rate = (grade_temp * (looterPP / 100d) * (RatePercent / 100)) > (grade_temp * 2) ? (grade_temp * 2) : (grade_temp * (looterPP / 100d)); //* Rates.DropsRate);
                        }
                        else
                            rate = item.GetDropRate((int)dropper.Grade.GradeId) * (looterPP / 100d) * ((monsterAgeBonus / 100d) + 1) * ((RatePercent / 100) * 1.50);
                    }
                }

            }
            else if (looter.Vip)
                if (DofusItemsIds.Contains(item.ItemId))
                {
                    var grade_temp = item.GetDropRate((int)dropper.Grade.GradeId);
                    rate = (grade_temp * (looterPP / 100d) * (RatePercent / 100)) > (grade_temp * 2) ? (grade_temp * 2) : (grade_temp * (looterPP / 100d)); //* Rates.VipDropRate);
                }
                else
                    rate = item.GetDropRate((int)dropper.Grade.GradeId) * (looterPP / 100d) * ((monsterAgeBonus / 100d) + 1) * (RatePercent / 100);
            else
            {
                if (DofusItemsIds.Contains(item.ItemId))
                {
                    var grade_temp = item.GetDropRate((int)dropper.Grade.GradeId);
                    rate = (grade_temp * (looterPP / 100d) * (RatePercent / 100)) > (grade_temp * 2) ? (grade_temp * 2) : (grade_temp * (looterPP / 100d)); //* Rates.DropsRate);
                }
                else
                    rate = item.GetDropRate((int)dropper.Grade.GradeId) * (looterPP / 100d) * ((monsterAgeBonus / 100d) + 1) * (RatePercent / 100);
            }

            return InvokeDropRateModifier(looter, item, rate);
        }
        #endregion

        #region // ---------------- Formula Dropped Dofus Chance By: Kenshin ------------------//
        public static double AdjustDropChance(IFightResult looter, DroppableDofusItem item, Monster dropper, int monsterAgeBonus)
        {
            var challengeBonus = looter.Fight.GetChallengesBonus();
            var looterPP = looter.Prospecting * (1 + ((challengeBonus) / 100d));

            double RatePercent = looter.Role >= RoleEnum.Gold_Vip ? Rates.GoldKamasRate : looter.Role == RoleEnum.Vip ? Rates.VipKamasRate : Rates.KamasRate;
            double rate = 0;

            if (looter is FightPlayerResult)
            {
                if ((looter as FightPlayerResult).Fighter.Team.Fighters.Where(x => x is CharacterFighter).Select(x => (x as CharacterFighter)).Any(y => y.Character.Client.IP == (looter as FightPlayerResult).Character.Client.IP && y.Character != (looter as FightPlayerResult).Character))
                {
                    if (looter.Vip)
                        if (DofusItemsIds.Contains(item.ItemId))
                        {
                            var grade_temp = item.GetDropRate((int)dropper.Grade.GradeId);
                            rate = (grade_temp * (looterPP / 100d) * (RatePercent / 100)) > (grade_temp * 2) ? (grade_temp * 2) : (grade_temp * (looterPP / 100d)); //* Rates.VipDropRate);
                        }
                        else
                            rate = item.GetDropRate((int)dropper.Grade.GradeId) * (looterPP / 100d) * ((monsterAgeBonus / 100d) + 1) * (RatePercent / 100);
                    else
                    {
                        if (DofusItemsIds.Contains(item.ItemId))
                        {
                            var grade_temp = item.GetDropRate((int)dropper.Grade.GradeId);
                            rate = (grade_temp * (looterPP / 100d) * (RatePercent / 100)) > (grade_temp * 2) ? (grade_temp * 2) : (grade_temp * (looterPP / 100d)); //* Rates.DropsRate);
                        }
                        else
                            rate = item.GetDropRate((int)dropper.Grade.GradeId) * (looterPP / 100d) * ((monsterAgeBonus / 100d) + 1) * (RatePercent / 100);
                    }
                }
                else
                {
                    if (looter.Vip)
                        if (DofusItemsIds.Contains(item.ItemId))
                        {
                            var grade_temp = item.GetDropRate((int)dropper.Grade.GradeId);
                            rate = (grade_temp * (looterPP / 100d) * (RatePercent / 100)) > (grade_temp * 2) ? (grade_temp * 2) : (grade_temp * (looterPP / 100d)); //* Rates.VipDropRate);
                        }
                        else
                            rate = item.GetDropRate((int)dropper.Grade.GradeId) * (looterPP / 100d) * ((monsterAgeBonus / 100d) + 1) * ((RatePercent / 100) * 1.50);
                    else
                    {
                        if (DofusItemsIds.Contains(item.ItemId))
                        {
                            var grade_temp = item.GetDropRate((int)dropper.Grade.GradeId);
                            rate = (grade_temp * (looterPP / 100d) * (RatePercent / 100)) > (grade_temp * 2) ? (grade_temp * 2) : (grade_temp * (looterPP / 100d)); //* Rates.DropsRate);
                        }
                        else
                            rate = item.GetDropRate((int)dropper.Grade.GradeId) * (looterPP / 100d) * ((monsterAgeBonus / 100d) + 1) * ((RatePercent / 100) * 1.50);
                    }
                }

            }
            else if (looter.Vip)
                if (DofusItemsIds.Contains(item.ItemId))
                {
                    var grade_temp = item.GetDropRate((int)dropper.Grade.GradeId);
                    rate = (grade_temp * (looterPP / 100d) * (RatePercent / 100)) > (grade_temp * 2) ? (grade_temp * 2) : (grade_temp * (looterPP / 100d)); //* Rates.VipDropRate);
                }
                else
                    rate = item.GetDropRate((int)dropper.Grade.GradeId) * (looterPP / 100d) * ((monsterAgeBonus / 100d) + 1) * (RatePercent / 100);
            else
            {
                if (DofusItemsIds.Contains(item.ItemId))
                {
                    var grade_temp = item.GetDropRate((int)dropper.Grade.GradeId);
                    rate = (grade_temp * (looterPP / 100d) * (RatePercent / 100)) > (grade_temp * 2) ? (grade_temp * 2) : (grade_temp * (looterPP / 100d)); //* Rates.DropsRate);
                }
                else
                    rate = item.GetDropRate((int)dropper.Grade.GradeId) * (looterPP / 100d) * ((monsterAgeBonus / 100d) + 1) * (RatePercent / 100);
            }

            return InvokeDropRateModifier(looter, item, rate);
        }
        #endregion
    }
}