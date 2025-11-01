using System;
using Stump.Core.Reflection;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Enums.Custom;
using Stump.Server.WorldServer.Database.Achievements;
using Stump.Server.WorldServer.Database.Monsters;
using Stump.Server.WorldServer.Game.Achievements.Criterions.Data;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Monsters;
using Stump.Server.WorldServer.Game.Conditions;

namespace Stump.Server.WorldServer.Game.Achievements.Criterions
{
    public class KillBossWithChallengeCriterion : AbstractCriterion<KillBossWithChallengeCriterion, DefaultCriterionData>
    {
        // FIELDS
        private ushort? m_maxValue;
        public const string Identifier = "EH";

        // CONSTRUCTORS
        public KillBossWithChallengeCriterion(AchievementObjectiveRecord objective) : base(objective)
        {
            var monsterId = GetMonsterIdByChallId(ChallIdentifier);

            if (monsterId != 0)
                Monster = Singleton<MonsterManager>.Instance.GetTemplate(monsterId);
        }

        // PROPERTIES
        public int ChallIdentifier => this[0][0];

        public MonsterTemplate Monster { get; }

        public int Number => this[0][1];

        public override bool IsIncrementable => false;

        public override ushort MaxValue
        {
            get
            {
                if (m_maxValue == null)
                {
                    m_maxValue = (ushort)Number;

                    switch (base[0].Operator)
                    {
                        case ComparaisonOperatorEnum.EQUALS:
                            break;

                        case ComparaisonOperatorEnum.INFERIOR:
                            throw new Exception();

                        case ComparaisonOperatorEnum.SUPERIOR:
                            m_maxValue++;
                            break;
                    }
                }

                return m_maxValue.Value;
            }
        }

        public int GetMonsterIdByChallId(int id)
        {
            switch (id)
            {
                case (int)ChallengeEnum.KARDORIM__CHALLENGE_1_:
                case (int)ChallengeEnum.KARDORIM__CHALLENGE_2_:
                case (int)ChallengeEnum.KARDORIM__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.KARDORIM_4051;

                case (int)ChallengeEnum.TOURNESOL_AFFAMÉ__CHALLENGE_1_:
                case (int)ChallengeEnum.TOURNESOL_AFFAMÉ__CHALLENGE_2_:
                case (int)ChallengeEnum.TOURNESOL_AFFAMÉ__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.TOURNESOL_AFFAM_799;

                case (int)ChallengeEnum.CHAFER_RONIN__CHALLENGE_1_:
                case (int)ChallengeEnum.CHAFER_RONIN__CHALLENGE_2_:
                case (int)ChallengeEnum.CHAFER_RONIN__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.CHAFER_RONIN_3238;

                case (int)ChallengeEnum.BWORKETTE__CHALLENGE_1_:
                case (int)ChallengeEnum.BWORKETTE__CHALLENGE_2_:
                case (int)ChallengeEnum.BWORKETTE__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.BWORKETTE_792;

                case (int)ChallengeEnum.COFFRE_DES_FORGERONS__CHALLENGE_1_:
                case (int)ChallengeEnum.COFFRE_DES_FORGERONS__CHALLENGE_2_:
                case (int)ChallengeEnum.COFFRE_DES_FORGERONS__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.COFFRE_DES_FORGERONS_252;

                case (int)ChallengeEnum.KANNIBOUL_EBIL__CHALLENGE_1_:
                case (int)ChallengeEnum.KANNIBOUL_EBIL__CHALLENGE_2_:
                case (int)ChallengeEnum.KANNIBOUL_EBIL__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.KANNIBOUL_EBIL_2960;

                case (int)ChallengeEnum.CRAQUELEUR_LÉGENDAIRE__CHALLENGE_1_:
                case (int)ChallengeEnum.CRAQUELEUR_LÉGENDAIRE__CHALLENGE_2_:
                case (int)ChallengeEnum.CRAQUELEUR_LÉGENDAIRE__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.CRAQUEBOULE_DE_SILICATE_699;

                case (int)ChallengeEnum.DAÏGORO__CHALLENGE_1_:
                case (int)ChallengeEnum.DAÏGORO__CHALLENGE_2_:
                case (int)ChallengeEnum.DAÏGORO__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.DAGORO_3208;

                case (int)ChallengeEnum.REINE_NYÉE__CHALLENGE_1_:
                case (int)ChallengeEnum.REINE_NYÉE__CHALLENGE_2_:
                case (int)ChallengeEnum.REINE_NYÉE__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.REINE_NYE_3996;

                case (int)ChallengeEnum.ABRAKNYDE_ANCESTRAL__CHALLENGE_1_:
                case (int)ChallengeEnum.ABRAKNYDE_ANCESTRAL__CHALLENGE_2_:
                case (int)ChallengeEnum.ABRAKNYDE_ANCESTRAL__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.ABRAKNYDE_ANCESTRAL_173;

                case (int)ChallengeEnum.MEULOU__CHALLENGE_1_:
                case (int)ChallengeEnum.MEULOU__CHALLENGE_2_:
                case (int)ChallengeEnum.MEULOU__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.MEULOU_232;

                case (int)ChallengeEnum.SILF_LE_RASBOUL_MAJEUR__CHALLENGE_1_:
                case (int)ChallengeEnum.SILF_LE_RASBOUL_MAJEUR__CHALLENGE_2_:
                case (int)ChallengeEnum.SILF_LE_RASBOUL_MAJEUR__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.SILF_LE_RASBOUL_MAJEUR_1071;

                case (int)ChallengeEnum.MAÎTRE_CORBAC__CHALLENGE_1_:
                case (int)ChallengeEnum.MAÎTRE_CORBAC__CHALLENGE_2_:
                case (int)ChallengeEnum.MAÎTRE_CORBAC__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.MATRE_CORBAC_289;

                case (int)ChallengeEnum.RAT_BLANC__CHALLENGE_1_:
                case (int)ChallengeEnum.RAT_BLANC__CHALLENGE_2_:
                case (int)ChallengeEnum.RAT_BLANC__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.RAT_BLANC_940;

                case (int)ChallengeEnum.ROYALMOUTH__CHALLENGE_1_:
                case (int)ChallengeEnum.ROYALMOUTH__CHALLENGE_2_:
                case (int)ChallengeEnum.ROYALMOUTH__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.ROYALMOUTH_2854;

                case (int)ChallengeEnum.MAÎTRE_PANDORE__CHALLENGE_1_:
                case (int)ChallengeEnum.MAÎTRE_PANDORE__CHALLENGE_2_:
                case (int)ChallengeEnum.MAÎTRE_PANDORE__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.MATRE_PANDORE_612;

                case (int)ChallengeEnum.HAUTE_TRUCHE__CHALLENGE_1_:
                case (int)ChallengeEnum.HAUTE_TRUCHE__CHALLENGE_2_:
                case (int)ChallengeEnum.HAUTE_TRUCHE__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.HAUTE_TRUCHE_3618;

                case (int)ChallengeEnum.CHÊNE_MOU__CHALLENGE_1_:
                case (int)ChallengeEnum.CHÊNE_MOU__CHALLENGE_2_:
                case (int)ChallengeEnum.CHÊNE_MOU__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.CHNE_MOU_257;

                case (int)ChallengeEnum.KIMBO__CHALLENGE_1_:
                case (int)ChallengeEnum.KIMBO__CHALLENGE_2_:
                case (int)ChallengeEnum.KIMBO__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.KIMBO_1045;

                case (int)ChallengeEnum.MINOTOT__CHALLENGE_1_:
                case (int)ChallengeEnum.MINOTOT__CHALLENGE_2_:
                case (int)ChallengeEnum.MINOTOT__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.MINOTOT_827;

                case (int)ChallengeEnum.OBSIDIANTRE__CHALLENGE_1_:
                case (int)ChallengeEnum.OBSIDIANTRE__CHALLENGE_2_:
                case (int)ChallengeEnum.OBSIDIANTRE__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.OBSIDIANTRE_2924;

                case (int)ChallengeEnum.KANIGROULA__CHALLENGE_1_:
                case (int)ChallengeEnum.KANIGROULA__CHALLENGE_2_:
                case (int)ChallengeEnum.KANIGROULA__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.KANIGROULA_3556;

                case (int)ChallengeEnum.USH_GALESH__CHALLENGE_1_:
                case (int)ChallengeEnum.USH_GALESH__CHALLENGE_2_:
                case (int)ChallengeEnum.USH_GALESH__DUO_:
                    return (int)MonsterIdEnum.USH_GALESH_4264;

                case (int)ChallengeEnum.TENGU_GIVREFOUX__CHALLENGE_1_:
                case (int)ChallengeEnum.TENGU_GIVREFOUX__CHALLENGE_2_:
                case (int)ChallengeEnum.TENGU_GIVREFOUX__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.TENGU_GIVREFOUX_2967;

                case (int)ChallengeEnum.PÈRE_VER__CHALLENGE_1_:
                case (int)ChallengeEnum.PÈRE_VER__CHALLENGE_2_:
                case (int)ChallengeEnum.PÈRE_VER__DUO_:
                    return (int)MonsterIdEnum.PRE_VER_4726;

                case (int)ChallengeEnum.KOLOSSO__CHALLENGE_1_:
                case (int)ChallengeEnum.KOLOSSO__CHALLENGE_2_:
                case (int)ChallengeEnum.KOLOSSO__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.KOLOSSO_2986;

                case (int)ChallengeEnum.GLOURSÉLESTE__CHALLENGE_1_:
                case (int)ChallengeEnum.GLOURSÉLESTE__CHALLENGE_2_:
                case (int)ChallengeEnum.GLOURSÉLESTE__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.GLOURSLESTE_2864;

                case (int)ChallengeEnum.OMBRE__CHALLENGE_1_:
                case (int)ChallengeEnum.OMBRE__CHALLENGE_2_:
                case (int)ChallengeEnum.OMBRE__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.OMBRE_3564;

                case (int)ChallengeEnum.COMTE_RAZOF__CHALLENGE_1_:
                case (int)ChallengeEnum.COMTE_RAZOF__CHALLENGE_2_:
                case (int)ChallengeEnum.COMTE_RAZOF__DUO_:
                    return (int)MonsterIdEnum.COMTE_RAZOF_4803;

                case (int)ChallengeEnum.ROI_NIDAS__CHALLENGE_1_:
                case (int)ChallengeEnum.ROI_NIDAS__CHALLENGE_2_:
                case (int)ChallengeEnum.ROI_NIDAS__CHALLENGE_TRIO_:
                    return (int)MonsterIdEnum.ROI_NIDAS_3648;

                case (int)ChallengeEnum.REINE_DES_VOLEURS__CHALLENGE_1_:
                case (int)ChallengeEnum.REINE_DES_VOLEURS__CHALLENGE_2_:
                case (int)ChallengeEnum.REINE_DES_VOLEURS__CHALLENGE_TRIO_:
                    return (int)MonsterIdEnum.REINE_DES_VOLEURS_3726;

                case (int)ChallengeEnum.ANERICE_LA_SHUSHESS__CHALLENGE_1_:
                case (int)ChallengeEnum.ANERICE_LA_SHUSHESS__CHALLENGE_2_:
                case (int)ChallengeEnum.ANERICE_LA_SHUSHESS__DUO_:
                    return (int)MonsterIdEnum.ANERICE_LA_SHUSHESS_4882;

                case (int)ChallengeEnum.DAZAK_MARTEGEL__CHALLENGE_1_:
                case (int)ChallengeEnum.DAZAK_MARTEGEL__CHALLENGE_2_:
                case (int)ChallengeEnum.DAZAK_MARTEGEL__DUO_:
                    return (int)MonsterIdEnum.DAZAK_MARTEGEL_5319;

                case (int)ChallengeEnum.KANKREBLATH__CHALLENGE_1_:
                case (int)ChallengeEnum.KANKREBLATH__CHALLENGE_2_:
                case (int)ChallengeEnum.KANKREBLATH__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.KANKREBLATH_3945;

                case (int)ChallengeEnum.GELÉE_ROYALE_BLEUE__CHALLENGE_1_:
                case (int)ChallengeEnum.GELÉE_ROYALE_BLEUE__CHALLENGE_2_:
                case (int)ChallengeEnum.GELÉE_ROYALE_BLEUE__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.GELE_ROYALE_BLEUE_58;

                case (int)ChallengeEnum.GELÉE_ROYALE_CITRON__CHALLENGE_1_:
                case (int)ChallengeEnum.GELÉE_ROYALE_CITRON__CHALLENGE_2_:
                case (int)ChallengeEnum.GELÉE_ROYALE_CITRON__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.GELE_ROYALE_CITRON_430;

                case (int)ChallengeEnum.GELÉE_ROYALE_FRAISE__CHALLENGE_1_:
                case (int)ChallengeEnum.GELÉE_ROYALE_FRAISE__CHALLENGE_2_:
                case (int)ChallengeEnum.GELÉE_ROYALE_FRAISE__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.GELE_ROYALE_FRAISE_86;

                case (int)ChallengeEnum.GELÉE_ROYALE_MENTHE__CHALLENGE_1_:
                case (int)ChallengeEnum.GELÉE_ROYALE_MENTHE__CHALLENGE_2_:
                case (int)ChallengeEnum.GELÉE_ROYALE_MENTHE__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.GELE_ROYALE_MENTHE_85;

                case (int)ChallengeEnum.CROCABULIA__CHALLENGE_1_:
                case (int)ChallengeEnum.CROCABULIA__CHALLENGE_2_:
                case (int)ChallengeEnum.CROCABULIA__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.CROCABULIA_854;

                case (int)ChallengeEnum.OUGAH__CHALLENGE_1_:
                case (int)ChallengeEnum.OUGAH__CHALLENGE_2_:
                case (int)ChallengeEnum.OUGAH__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.OUGAH_1159;

                case (int)ChallengeEnum.MISSIZ_FRIZZ__CHALLENGE_1_:
                case (int)ChallengeEnum.MISSIZ_FRIZZ___CHALLENGE_2_:
                case (int)ChallengeEnum.MISSIZ_FRIZZ__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.MISSIZ_FRIZZ_3391;

                case (int)ChallengeEnum.SCARABOSSE_DORÉ__CHALLENGE_1_:
                case (int)ChallengeEnum.SCARABOSSE_DORÉ__CHALLENGE_2_:
                case (int)ChallengeEnum.SCARABOSSE_DORÉ__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.SCARABOSSE_DOR_797;

                case (int)ChallengeEnum.KWAKWA__CHALLENGE_1_:
                case (int)ChallengeEnum.KWAKWA__CHALLENGE_2_:
                case (int)ChallengeEnum.KWAKWA__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.KWAKWA_2995;

                case (int)ChallengeEnum.MANTISCORE__CHALLENGE_1_:
                case (int)ChallengeEnum.MANTISCORE__CHALLENGE_2_:
                case (int)ChallengeEnum.MANTISCORE__DUO_:
                    return (int)MonsterIdEnum.MANTISCORE_4621;

                case (int)ChallengeEnum.KOULOSSE__CHALLENGE_1_:
                case (int)ChallengeEnum.KOULOSSE__CHALLENGE_2_:
                case (int)ChallengeEnum.KOULOSSE__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.KOULOSSE_670;

                case (int)ChallengeEnum.TYNRIL_AHURI__CHALLENGE_1_:
                case (int)ChallengeEnum.TYNRIL_AHURI__CHALLENGE_2_:
                case (int)ChallengeEnum.TYNRIL_AHURI__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.TYNRIL_AHURI_1087;

                case (int)ChallengeEnum.XLII__CHALLENGE_1_:
                case (int)ChallengeEnum.XLII__CHALLENGE_2_:
                case (int)ChallengeEnum.XLII__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.XLII_3849;

                case (int)ChallengeEnum.KORRIANDRE__CHALLENGE_1_:
                case (int)ChallengeEnum.KORRIANDRE__CHALLENGE_2_:
                case (int)ChallengeEnum.KORRIANDRE__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.XLII_3849;

                case (int)ChallengeEnum.BETHEL_AKARNA__CHALLENGE_1_:
                case (int)ChallengeEnum.BETHEL_AKARNA__CHALLENGE_2_:
                case (int)ChallengeEnum.BETHEL_AKARNA__DUO_:
                    return (int)MonsterIdEnum.BETHEL_AKARNA_5110;

                case (int)ChallengeEnum.MOB_L_EPONGE__CHALLENGE_1_:
                case (int)ChallengeEnum.MOB_L_EPONGE__CHALLENGE_2_:
                case (int)ChallengeEnum.MOB_L_EPONGE__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.MOB_LPONGE_928;

                case (int)ChallengeEnum.BOOSTACHE__CHALLENGE_1_:
                case (int)ChallengeEnum.BOOSTACHE__CHALLENGE_2_:
                case (int)ChallengeEnum.BOOSTACHE__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.BOOSTACHE_2975;

                case (int)ChallengeEnum.BULBIG_BROZEUR__CHALLENGE_1_:
                case (int)ChallengeEnum.BULBIG_BROZEUR__CHALLENGE_2_:
                case (int)ChallengeEnum.BULBIG_BROZEUR__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.BULBIG_BROZEUR_3232;

                case (int)ChallengeEnum.MINOTOROR__CHALLENGE_1_:
                case (int)ChallengeEnum.MINOTOROR__CHALLENGE_2_:
                case (int)ChallengeEnum.MINOTOROR__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.MINOTOROR_121;

                case (int)ChallengeEnum.FRAKTALE__CHALLENGE_1_:
                case (int)ChallengeEnum.FRAKTALE__CHALLENGE_2_:
                case (int)ChallengeEnum.FRAKTALE__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.FRAKTALE_3852;

                case (int)ChallengeEnum.TOXOLIATH__CHALLENGE_1_:
                case (int)ChallengeEnum.TOXOLIATH__CHALLENGE_2_:
                case (int)ChallengeEnum.TOXOLIATH__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.TOXOLIATH_3752;

                case (int)ChallengeEnum.SYLARGH__CHALLENGE_1_:
                case (int)ChallengeEnum.SYLARGH__CHALLENGE_2_:
                case (int)ChallengeEnum.SYLARGH__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.SYLARGH_3409;

                case (int)ChallengeEnum.BATOFU__CHALLENGE_1_:
                case (int)ChallengeEnum.BATOFU__CHALLENGE_2_:
                case (int)ChallengeEnum.BATOFU__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.BATOFU_800;

                case (int)ChallengeEnum.SHIN_LARVE__CHALLENGE_1_:
                case (int)ChallengeEnum.SHIN_LARVE__CHALLENGE_2_:
                case (int)ChallengeEnum.SHIN_LARVE__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.SHIN_LARVE_457;

                case (int)ChallengeEnum.DRAGON_COCHON__CHALLENGE_1_:
                case (int)ChallengeEnum.DRAGON_COCHON__CHALLENGE_2_:
                case (int)ChallengeEnum.DRAGON_COCHON__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.DRAGON_COCHON_113;

                case (int)ChallengeEnum.MOON__CHALLENGE_1_:
                case (int)ChallengeEnum.MOON__CHALLENGE_2_:
                case (int)ChallengeEnum.MOON__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.MOON_226;

                case (int)ChallengeEnum.GROLLOUM__CHALLENGE_1_:
                case (int)ChallengeEnum.GROLLOUM__CHALLENGE_2_:
                case (int)ChallengeEnum.GROLLOUM__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.GROLLOUM_2942;

                case (int)ChallengeEnum.HAREBOURG__CHALLENGE_1_:
                case (int)ChallengeEnum.HAREBOURG__CHALLENGE_2_:
                case (int)ChallengeEnum.HAREBOURG__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.COMTE_HAREBOURG_3416;

                case (int)ChallengeEnum.SOLAR__CHALLENGE_1_:
                case (int)ChallengeEnum.SOLAR__CHALLENGE_2_:
                case (int)ChallengeEnum.SOLAR__DUO_:
                    return (int)MonsterIdEnum.SOLAR_5100;

                case (int)ChallengeEnum.BLOP_COCO_ROYAL__CHALLENGE_1_:
                case (int)ChallengeEnum.BLOP_COCO_ROYAL__CHALLENGE_2_:
                case (int)ChallengeEnum.BLOP_COCO_ROYAL__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.BLOP_COCO_ROYAL_1184;

                case (int)ChallengeEnum.BLOP_GRIOTTE_ROYAL__CHALLENGE_1_:
                case (int)ChallengeEnum.BLOP_GRIOTTE_ROYAL__CHALLENGE_2_:
                case (int)ChallengeEnum.BLOP_GRIOTTE_ROYAL__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.BLOP_GRIOTTE_ROYAL_1185;

                case (int)ChallengeEnum.BLOP_INDIGO_ROYAL__CHALLENGE_1_:
                case (int)ChallengeEnum.BLOP_INDIGO_ROYAL__CHALLENGE_2_:
                case (int)ChallengeEnum.BLOP_INDIGO_ROYAL__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.BLOP_INDIGO_ROYAL_1186;

                case (int)ChallengeEnum.BLOP_REINETTE_ROYAL__CHALLENGE_1_:
                case (int)ChallengeEnum.BLOP_REINETTE_ROYAL__CHALLENGE_2_:
                case (int)ChallengeEnum.BLOP_REINETTE_ROYAL__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.BLOP_REINETTE_ROYAL_1187;

                case (int)ChallengeEnum.BOUFTOU_ROYAL__CHALLENGE_1_:
                case (int)ChallengeEnum.BOUFTOU_ROYAL__CHALLENGE_2_:
                case (int)ChallengeEnum.BOUFTOU_ROYAL__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.BOUFTOU_ROYAL_147;

                case (int)ChallengeEnum.KLIME__CHALLENGE_1_:
                case (int)ChallengeEnum.KLIME__CHALLENGE_2_:
                case (int)ChallengeEnum.KLIME__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.KLIME_3384;

                case (int)ChallengeEnum.NILEZA__CHALLENGE_1_:
                case (int)ChallengeEnum.NILEZA__CHALLENGE_2_:
                case (int)ChallengeEnum.NILEZA__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.NILEZA_3397;

                case (int)ChallengeEnum.CHALOEIL__CHALLENGE_1_:
                case (int)ChallengeEnum.CHALOEIL__CHALLENGE_2_:
                case (int)ChallengeEnum.CHALOEIL__TRIO_:
                    return (int)MonsterIdEnum.CHALIL_4263;

                case (int)ChallengeEnum.CAPITAINE_MENO__CHALLENGE_1_:
                case (int)ChallengeEnum.CAPITAINE_MENO__CHALLENGE_2_:
                case (int)ChallengeEnum.CAPITAINE_MENO__DUO_:
                    return (int)MonsterIdEnum.CAPITAINE_MENO_4460;

                case (int)ChallengeEnum.LARVE_DE_KOUTOULOU__CHALLENGE_1_:
                case (int)ChallengeEnum.LARVE_DE_KOUTOULOU__CHALLENGE_2_:
                case (int)ChallengeEnum.LARVE_DE_KOUTOULOU__DUO_:
                    return (int)MonsterIdEnum.LARVE_DE_KOUTOULOU_4453;

                case (int)ChallengeEnum.CORAILLEUR_MAGISTRAL__CHALLENGE_1_:
                case (int)ChallengeEnum.CORAILLEUR_MAGISTRAL__CHALLENGE_2_:
                case (int)ChallengeEnum.CORAILLEUR_MAGISTRAL__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.CORAILLEUR_MAGISTRAL_1027;

                case (int)ChallengeEnum.WA_WABBIT__CHALLENGE_1_:
                case (int)ChallengeEnum.WA_WABBIT__CHALLENGE_2_:
                case (int)ChallengeEnum.WA_WABBIT__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.WA_WABBIT_180;

                case (int)ChallengeEnum.WA_WOBOT__CHALLENGE_1_:
                case (int)ChallengeEnum.WA_WOBOT__CHALLENGE_2_:
                case (int)ChallengeEnum.WA_WOBOT__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.WA_WOBOT_3460;

                case (int)ChallengeEnum.MALLÉFISK__CHALLENGE_1_:
                case (int)ChallengeEnum.MALLÉFISK__CHALLENGE_2_:
                case (int)ChallengeEnum.MALLÉFISK__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.MALLFISK_3652;

                case (int)ChallengeEnum.RAT_NOIR__CHALLENGE_1_:
                case (int)ChallengeEnum.RAT_NOIR__CHALLENGE_2_:
                case (int)ChallengeEnum.RAT_NOIR__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.RAT_NOIR_939;

                case (int)ChallengeEnum.POUNICHEUR__CHALLENGE_1_:
                case (int)ChallengeEnum.POUNICHEUR__CHALLENGE_2_:
                case (int)ChallengeEnum.POUNICHEUR__DUO_:
                    return (int)MonsterIdEnum.POUNICHEUR_4278;

                case (int)ChallengeEnum.SKEUNK__CHALLENGE_1_:
                case (int)ChallengeEnum.SKEUNK__CHALLENGE_2_:
                case (int)ChallengeEnum.SKEUNK__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.SKEUNK_780;

                case (int)ChallengeEnum.EL_PIKO__CHALLENGE_1_:
                case (int)ChallengeEnum.EL_PIKO__CHALLENGE_2_:
                case (int)ChallengeEnum.EL_PIKO__DUO_:
                    return (int)MonsterIdEnum.EL_PIKO_4609;

                case (int)ChallengeEnum.KRALAMOUR_GÉANT__CHALLENGE_1_:
                case (int)ChallengeEnum.KRALAMOUR_GÉANT__CHALLENGE_2_:
                case (int)ChallengeEnum.KRALAMOUR_GÉANT__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.KRALAMOURE_GANT_423;

                case (int)ChallengeEnum.BWORKER__CHALLENGE_1_:
                case (int)ChallengeEnum.BWORKER__CHALLENGE_2_:
                case (int)ChallengeEnum.BWORKER__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.BWORKER_478;

                case (int)ChallengeEnum.MAÎTRE_DES_PANTINS__CHALLENGE_1_:
                case (int)ChallengeEnum.MAÎTRE_DES_PANTINS__CHALLENGE_2_:
                case (int)ChallengeEnum.MAÎTRE_DES_PANTINS__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.MATRE_DES_PANTINS_3476;

                case (int)ChallengeEnum.TOFU_ROYAL__CHALLENGE_1_:
                case (int)ChallengeEnum.TOFU_ROYAL__CHALLENGE_2_:
                case (int)ChallengeEnum.TOFU_ROYAL__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.TOFU_ROYAL_382;

                case (int)ChallengeEnum.CAPITAINE_EKARLATTE__CHALLENGE_1_:
                case (int)ChallengeEnum.CAPITAINE_EKARLATTE__CHALLENGE_2_:
                case (int)ChallengeEnum.CAPITAINE_EKARLATTE__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.CAPITAINE_EKARLATTE_3753;

                case (int)ChallengeEnum.PÉKI_PÉKI__CHALLENGE_1_:
                case (int)ChallengeEnum.PÉKI_PÉKI__CHALLENGE_2_:
                case (int)ChallengeEnum.PÉKI_PÉKI__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.PKI_PKI_605;

                case (int)ChallengeEnum.BEN_LE_RIPATE__CHALLENGE_1_:
                case (int)ChallengeEnum.BEN_LE_RIPATE__CHALLENGE_2_:
                case (int)ChallengeEnum.BEN_LE_RIPATE__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.BEN_LE_RIPATE_2877;

                case (int)ChallengeEnum.FUJI_GIVREFOUX_NOURRICIÈRE__CHALLENGE_1_:
                case (int)ChallengeEnum.FUJI_GIVREFOUX_NOURRICIÈRE__CHALLENGE_2_:
                case (int)ChallengeEnum.FUJI_GIVREFOUX_NOURRICIÈRE__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.FUJI_GIVREFOUX_NOURRICIRE_3234;

                case (int)ChallengeEnum.PROTOZORREUR__CHALLENGE_1_:
                case (int)ChallengeEnum.PROTOZORREUR__CHALLENGE_2_:
                case (int)ChallengeEnum.PROTOZORREUR__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.PROTOZORREUR_3828;

                case (int)ChallengeEnum.DANTINEA__CHALLENGE_1_:
                case (int)ChallengeEnum.DANTINEA__CHALLENGE_2_:
                case (int)ChallengeEnum.DANTINEA__DUO_:
                    return (int)MonsterIdEnum.DANTINA_4444;

                case (int)ChallengeEnum.TAL_KASHA__CHALLENGE_1_:
                case (int)ChallengeEnum.TAL_KASHA__CHALLENGE_2_:
                case (int)ChallengeEnum.TAL_KASHA__DUO_:
                    return (int)MonsterIdEnum.TAL_KASHA_4744;

                case (int)ChallengeEnum.CHOUDINI__CHALLENGE_1_:
                case (int)ChallengeEnum.CHOUDINI__CHALLENGE_2_:
                case (int)ChallengeEnum.CHOUDINI__DUO_:
                    return (int)MonsterIdEnum.CHOUDINI_4860;

                case (int)ChallengeEnum.BLOP_MULTICOLORE_ROYAL__CHALLENGE_1_:
                case (int)ChallengeEnum.BLOP_MULTICOLORE_ROYAL__CHALLENGE_2_:
                case (int)ChallengeEnum.BLOP_MULTICOLORE_ROYAL__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.BLOP_MULTICOLORE_ROYAL_1188;

                case (int)ChallengeEnum.TANUKOUÏ_SAN__CHALLENGE_1_:
                case (int)ChallengeEnum.TANUKOUÏ_SAN__CHALLENGE_2_:
                case (int)ChallengeEnum.TANUKOUÏ_SAN__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.TANUKOU_SAN_568;

                case (int)ChallengeEnum.SPHINCTER_CELL__CHALLENGE_1_:
                case (int)ChallengeEnum.SPHINCTER_CELL__CHALLENGE_2_:
                case (int)ChallengeEnum.SPHINCTER_CELL__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.SPHINCTER_CELL_943;

                case (int)ChallengeEnum.PHOSSILE__CHALLENGE_1_:
                case (int)ChallengeEnum.PHOSSILE__CHALLENGE_2_:
                case (int)ChallengeEnum.PHOSSILE__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.PASPARTOU_3656;

                case (int)ChallengeEnum.VORTEX__CHALLENGE_1_:
                case (int)ChallengeEnum.VORTEX__CHALLENGE_2_:
                case (int)ChallengeEnum.VORTEX__CHALLENGE_TRIO_:
                    return (int)MonsterIdEnum.VORTEX_3835;

                case (int)ChallengeEnum.NELWEEN__CHALLENGE_1_:
                case (int)ChallengeEnum.NELWEEN__CHALLENGE_2_:
                case (int)ChallengeEnum.NELWEEN__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.NELWEEN_3100;

                case (int)ChallengeEnum.CHOUQUE__CHALLENGE_1_:
                case (int)ChallengeEnum.CHOUQUE__CHALLENGE_2_:
                case (int)ChallengeEnum.CHOUQUE__DUO_:
                    return (int)MonsterIdEnum.LE_CHOUQUE_230;

                case (int)ChallengeEnum.MANSOT_ROYAL__CHALLENGE_1_:
                case (int)ChallengeEnum.MANSOT_ROYAL__CHALLENGE_2_:
                case (int)ChallengeEnum.MANSOT_ROYAL__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.MANSOT_ROYAL_2848;

                case (int)ChallengeEnum.MERKATOR__CHALLENGE_1_:
                case (int)ChallengeEnum.MERKATOR__CHALLENGE_2_:
                case (int)ChallengeEnum.MERKATOR__CHALLENGE_DUO_:
                    return (int)MonsterIdEnum.MERKATOR_3534;

                case (int)ChallengeEnum.ILYZAELLE__CHALLENGE_1_:
                case (int)ChallengeEnum.ILYZAELLE__CHALLENGE_2_:
                case (int)ChallengeEnum.ILYZAELLE__DUO_:
                    return (int)MonsterIdEnum.ILYZAELLE_4967;

                default:
                    return 0;
            }
        }

        // METHODS
        public override DefaultCriterionData Parse(ComparaisonOperatorEnum @operator, params string[] parameters)
        {
            return new DefaultCriterionData(@operator, parameters);
        }

        public override bool Eval(Character character)
        {
            return character.Achievement.GetRunningCriterion(this) >= Number;
        }

        public override bool Lower(AbstractCriterion left)
        {
            return Number < ((KillBossWithChallengeCriterion)left).Number;
        }

        public override bool Greater(AbstractCriterion left)
        {
            return Number > ((KillBossWithChallengeCriterion)left).Number;
        }

        public override ushort GetPlayerValue(PlayerAchievement player)
        {
            return (ushort)Math.Min(MaxValue, player.GetRunningCriterion(this));
        }
    }
}