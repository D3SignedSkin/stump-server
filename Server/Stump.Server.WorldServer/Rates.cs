using System;
using Stump.Core.Attributes;

namespace Stump.Server.WorldServer
{
    [Serializable]
    public static class Rates
    {
        /// <summary>
        /// Life regen rate (default 2 => 2hp/seconds.)
        /// </summary>
        [Variable(true)]
        public static float RegenRate = 2;
        [Variable(true)]
        public static float VipRegenRate = 3;
        [Variable(true)]
        public static float GoldRegenRate = 5;


        [Variable(true)]
        public static double XpRate = 1000;
        [Variable(true)]
        public static double VipXpRate = 1500;
        [Variable(true)]
        public static double GoldXpRate = 3000;

        [Variable(true)]
        public static float KamasRate = 100;
        [Variable(true)]
        public static double VipKamasRate = 200;
        [Variable(true)]
        public static double GoldKamasRate = 300;


        [Variable(true)]
        public static float DropsRate = 400;
        [Variable(true)]
        public static float VipDropsRate = 600;
        [Variable(true)]
        public static float GoldDropsRate = 800;


        [Variable(true)]
        public static float JobXpRate = 200;
        [Variable(true)]
        public static float VipJobXpRate = 300;
        [Variable(true)]
        public static float GoldJobXpRate = 400;

        [Variable(true)]
        public static float AchievementXpRate = 2;
        [Variable(true)]
        public static float VipAchievementXpRate = 3;
        [Variable(true)]
        public static float GoldAchievementXpRate = 4;

        [Variable(true)]
        public static float AchievementKamasRate = 2;
        [Variable(true)]
        public static float VipAchievementKamasRate = 3;
        [Variable(true)]
        public static float GoldAchievementKamasRate = 4;

        [Variable(true)]
        public static double HonrorRate = 100;
        [Variable(true)]
        public static double VipHonrorRate = 150;
        [Variable(true)]
        public static double GoldHonrorRate = 250;

        [Variable(true)]
        public static float OrbsRate = 1;

        [Variable(true)]
        public static bool BonusKamasInEnutropia = true;
        [Variable(true)]
        public static double BonusKamasInEnutropia_Center = 1;
        [Variable(true)]
        public static double BonusKamasInEnutropia_Left = 1;
        [Variable(true)]
        public static double BonusKamasInEnutropia_Right = 1;
        [Variable(true)]
        public static double BonusKamasInEnutropia_Top = 1;
        [Variable(true)]
        public static double BonusKamasInEnutropia_Dung_Male = 1;
        [Variable(true)]
        public static double BonusKamasInEnutropia_Dung_Nidas = 1;
        [Variable(true)]
        public static double BonusKamasInEnutropia_Dung_Gallery = 1;
    }
}