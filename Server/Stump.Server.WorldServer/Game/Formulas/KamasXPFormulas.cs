using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Stump.Server.WorldServer.Game.Formulas
{
    public class KamasXPFormulas
    {
        List<(double, RoleEnum)> KamasRateBase = new List<(double, RoleEnum)>
        {
            (Rates.AchievementKamasRate, RoleEnum.None),
            (Rates.AchievementKamasRate, RoleEnum.Player),
            (Rates.VipAchievementKamasRate, RoleEnum.Vip),
            (Rates.GoldAchievementKamasRate, RoleEnum.Gold_Vip),
            (Rates.GoldAchievementKamasRate, RoleEnum.Moderator_Helper),
            (Rates.GoldAchievementKamasRate, RoleEnum.GameMaster_Padawan),
            (Rates.GoldAchievementKamasRate, RoleEnum.GameMaster),
            (Rates.GoldAchievementKamasRate, RoleEnum.Administrator),
            (Rates.GoldAchievementKamasRate, RoleEnum.Non_ADM),
            (Rates.GoldAchievementKamasRate, RoleEnum.Developer),
        };

        List<(double, RoleEnum)> ExperienceRateBase = new List<(double, RoleEnum)>
        {
            (Rates.AchievementXpRate, RoleEnum.None),
            (Rates.AchievementXpRate, RoleEnum.Player),
            (Rates.VipAchievementXpRate, RoleEnum.Vip),
            (Rates.GoldAchievementXpRate, RoleEnum.Gold_Vip),
            (Rates.GoldAchievementXpRate, RoleEnum.Moderator_Helper),
            (Rates.GoldAchievementXpRate, RoleEnum.GameMaster_Padawan),
            (Rates.GoldAchievementXpRate, RoleEnum.GameMaster),
            (Rates.GoldAchievementXpRate, RoleEnum.Administrator),
            (Rates.GoldAchievementXpRate, RoleEnum.Non_ADM),
            (Rates.GoldAchievementXpRate, RoleEnum.Developer),
        };


        public long GetKamasReward(Character character, bool kamasScaleWithPlayerLevel, int level, double kamasRatio, double Duration, int a)
        {
            var playerKamasRate = KamasRateBase.FirstOrDefault(x => x.Item2 == character.UserGroup.Role).Item1;
            kamasRatio = kamasRatio == 0 ? 1.00 : 1.00 + kamasRatio;

            double s = kamasScaleWithPlayerLevel && a != -1 ? a : level;
            var result = Math.Floor((Math.Pow(s, 2) + 20 * s - 20) * kamasRatio * Duration * playerKamasRate);

            return (long)result;
        }

        public int GetXpReward(Character character, int XpScaleWithPlayerLevel, int Level, double ExperienceRatio, double Duration)
        {
            if (XpScaleWithPlayerLevel == -1)
                XpScaleWithPlayerLevel = 200;

            var result = 0;
            var playerXpRate = ExperienceRateBase.FirstOrDefault(x => x.Item2 == character.UserGroup.Role).Item1;
            ExperienceRatio = ExperienceRatio == 0 ? 10.00 : 10.00 + ExperienceRatio;

            if (XpScaleWithPlayerLevel > Level)
            {
                double a = Math.Min(XpScaleWithPlayerLevel, 1.5 * Level);
                double s = Level * Math.Pow((100 + 2 * Level), 2) / 20 * Duration * ExperienceRatio * playerXpRate;
                double i = a * Math.Pow((100 + 2 * a), 2) / 20 * Duration * ExperienceRatio * playerXpRate;
                double o = (1 - 0.7) * s;
                double u = 0.7 * i;

                result = (int)Math.Floor(u + o);

                return result;
            }

            result = (int)Math.Floor(XpScaleWithPlayerLevel * Math.Pow((100 + 2 * XpScaleWithPlayerLevel), 2) / 20 * Duration * ExperienceRatio * playerXpRate);

            return result;
        }
    }
}