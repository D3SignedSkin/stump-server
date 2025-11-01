using System;
using System.Drawing;
using Stump.Core.Attributes;

namespace Stump.Server.WorldServer
{
    /// <summary>
    ///   Global settings defined by the config file
    /// </summary>
    public class Settings
    {
        [Variable(true)]
        public static bool App_Debug = false;

        #region Settings de Modos do Servidor
        [Variable(true)]
        public static bool ModoPVM = true;
        [Variable(true)]
        public static bool ModoPVP = true;
        #endregion

        #region Settings de Controles do Servidor
        [Variable(true)]
        public static bool Ogrines2xAnnounce = false;
        [Variable(true)]
        public static ushort AnnouncePlayerOnlineGroup = 3;
        [Variable(true)]
        public static bool NpcExoEffects = false;
        [Variable(true)]
        public static bool BlockTempOgrines = false;
        #endregion

        #region Settings de Eventos do Servidor
        [Variable(true)]
        public static bool AutoMapEvent = false;
        [Variable(true)]
        public static bool EventReturn = false;
        [Variable(true)]
        public static int DaysEventReturn = 30;
        [Variable(true)]
        public static int PrimaryItemIdReturn = 30386;
        [Variable(true)]
        public static bool PegaPega = false;
        [Variable(true)]
        public static bool PegaPegaNpc = false;
        [Variable(true)]
        public static int ExoCommandPrice = 4500;
        [Variable(true)]
        public static bool Weekend = false;
        [Variable(true)]
        public static int WeekPlayerValue = 25;
        [Variable(true)]
        public static int WeekVipValue = 50;
        [Variable(true)]
        public static int WeekGoldVipValue = 75;
        [Variable(true)]
        public static DateTime StartPaskwak = new DateTime(2022, 05, 01);
        [Variable(true)]
        public static DateTime EndPaskwak = new DateTime(2022, 05, 01);
        [Variable(true)]
        public static DateTime StartNataw = new DateTime(2022, 12, 01);
        [Variable(true)]
        public static DateTime EndNataw = new DateTime(2023, 01, 10);
        [Variable(true)]
        public static DateTime StartHelloween = new DateTime(2022, 12, 01);
        [Variable(true)]
        public static DateTime EndHelloween = new DateTime(2023, 01, 10);
        [Variable(true)]
        public static bool CampKoliseu = false;
        #endregion

        [Variable(true)]
        public static bool LogsPlayers = true;
        [Variable(true)]
        public static bool LogsStaffs = true;
        [Variable(true)]
        public static string MOTD = "Bem Vindo ao Dofus  para acessar os comandos digite .ajuda !</b>";
        [Variable(true)]
        public static string MOTD_en = "Welcome to the Dofus  to access the commands type .help !</b>";
        [Variable(true)]
        public static string MOTD_es = "Bienvenido a Dofus  para acceder a los comandos escriba .ayuda !</b>";
        [Variable(true)]
        public static string MOTD_fr = "Bienvenue dans le Dofus  pour accéder aux commandes de type .aide !</b>";

        private static string m_htmlMOTDColor = ColorTranslator.ToHtml(Color.OrangeRed);
        private static Color m_MOTDColor = Color.OrangeRed;


        [Variable(true)]
        public static string HtmlMOTDColor
        {
            get { return m_htmlMOTDColor; }
            set
            {
                m_htmlMOTDColor = value;
                m_MOTDColor = ColorTranslator.FromHtml(value);
            }
        }

        [Variable(true)]
        public static ushort[] VipOrnament =
        {

        };

        [Variable(true)]
        public static ushort[] GoldVipOrnament =
{

        };

        [Variable(true)]
        public static ushort[] VipTitle =
{

        };

        [Variable(true)]
        public static ushort[] GoldTitle =
{

        };

        [Variable(true)]
        public static int[] VipEmote =
        {

        };

        [Variable(true)]
        public static int[] GoldEmote =
        {

        };

        [Variable]
        public static readonly int TokenTemplateId = 30000;

        public static Color MOTDColor
        {
            get { return m_MOTDColor; }
            set
            {
                m_htmlMOTDColor = ColorTranslator.ToHtml(value);
                m_MOTDColor = value;
            }
        }
    }
}