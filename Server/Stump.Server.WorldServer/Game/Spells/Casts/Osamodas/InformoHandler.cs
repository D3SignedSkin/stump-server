using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Fights;

namespace Stump.Server.WorldServer.Game.Spells.Casts.Osamodas
{
    [SpellCastHandler(SpellIdEnum._INFORMO_DECLENCHE_14235)]
    public class InformoHandler : DefaultSpellCastHandler //By Kenshin Version 2.61.10
    {
        public InformoHandler(SpellCastInformations cast) : base(cast)
        { }

        public override void Execute()
        {
            if (!base.Initialize())
                Initialize();

            FightActor _target = Fight.GetOneFighter(TargetedCell);

            if (Spell.CurrentLevel == 1 && _target != null)
            {
                //Effect_Kill
                Handlers[0].Apply();

                //Handlers[1].Apply();  //Effect_AddState - 1359 - [!] __OSAMODAS : PASSER LE TOUR
                //Handlers[2].Apply();  //Effect_AddState - 1235 - [!] __SLOT 1
                //Handlers[3].Apply();  //Effect_AddState - 1236 - [!] __SLOT 2
                //Handlers[4].Apply();  //Effect_AddState - 1237 - [!] __SLOT 3
                //Handlers[5].Apply();  //Effect_AddState - 1238 - [!] __SLOT 4
                //Handlers[6].Apply();  //Effect_AddState - 1239 - [!] __SLOT 5
                //Handlers[7].Apply();  //Effect_AddState - 1240 - [!] __SLOT 6
                //Handlers[8].Apply();  //Effect_AddState - 1232 - [!] __INVOC GRADE 1
                //Handlers[9].Apply();  //Effect_AddState - 1233 - [!] __INVOC GRADE 2
                //Handlers[10].Apply(); //Effect_AddState - 1234 - [!] __INVOC GRADE 3
                //Handlers[11].Apply(); //Effect_AddState - 2104 - [!] Transmission résistance naturelle
                //Handlers[12].Apply(); //Effect_AddState - 2106 - [!] Transmission baume protecteur

                Handlers[13].Apply(); //Effect_TriggerBuff - 14235 LVL 3
                Handlers[14].Apply(); //Effect_2794 - 14235 LVL 2
            }
            else if (Spell.CurrentLevel == 2)
            {
                #region >> FIRST_13998

                //Effect_2794 - 13998 LVL 1
                Handlers[0].AddAffectedActor(Fight.FighterPlaying);
                Handlers[0].Apply();

                //Effect_2794 - 13998 LVL 2
                Handlers[1].AddAffectedActor(Fight.FighterPlaying);
                Handlers[1].Apply();

                //Effect_2794 - 13998 LVL 3
                Handlers[2].AddAffectedActor(Fight.FighterPlaying);
                Handlers[2].Apply();

                #endregion

                #region >> SECOND_14006

                //Effect_2794 - 14006 LVL 1
                Handlers[3].AddAffectedActor(Fight.FighterPlaying);
                Handlers[3].Apply();

                //Effect_2794 - 14006 LVL 2
                Handlers[4].AddAffectedActor(Fight.FighterPlaying);
                Handlers[4].Apply();

                //Effect_2794 - 14006 LVL 3
                Handlers[5].AddAffectedActor(Fight.FighterPlaying);
                Handlers[5].Apply();

                #endregion

                #region >> THIRD_14007

                //Effect_2794 - 14007 LVL 1
                Handlers[6].AddAffectedActor(Fight.FighterPlaying);
                Handlers[6].Apply();

                //Effect_2794 - 14007 LVL 2
                Handlers[7].AddAffectedActor(Fight.FighterPlaying);
                Handlers[7].Apply();

                //Effect_666  - 14007 LVL 3
                Handlers[8].AddAffectedActor(Fight.FighterPlaying);
                Handlers[8].Apply();

                #endregion

                #region >> FOURTH_14008

                //Effect_2794 - 14008 LVL 1
                Handlers[9].AddAffectedActor(Fight.FighterPlaying);
                Handlers[9].Apply();

                //Effect_666  - 14008 LVL 2
                Handlers[10].AddAffectedActor(Fight.FighterPlaying);
                Handlers[10].Apply();

                //Effect_666  - 14008 LVL 3
                Handlers[11].AddAffectedActor(Fight.FighterPlaying);
                Handlers[11].Apply();

                #endregion

                #region >> FIFTH_14009

                //Effect_2794 - 14009 LVL 1
                Handlers[12].AddAffectedActor(Fight.FighterPlaying);
                Handlers[12].Apply();

                //Effect_666  - 14009 LVL 2
                Handlers[13].AddAffectedActor(Fight.FighterPlaying);
                Handlers[13].Apply();

                //Effect_666  - 14009 LVL 3
                Handlers[14].AddAffectedActor(Fight.FighterPlaying);
                Handlers[14].Apply();

                #endregion

                #region >> SIXTH_14010

                //Effect_2794 - 14010 LVL 1
                Handlers[15].AddAffectedActor(Fight.FighterPlaying);
                Handlers[15].Apply();

                //Effect_666  - 14010 LVL 2
                Handlers[16].AddAffectedActor(Fight.FighterPlaying);
                Handlers[16].Apply();

                //Effect_666  - 14010 LVL 3
                Handlers[17].AddAffectedActor(Fight.FighterPlaying);
                Handlers[17].Apply();

                #endregion

                //Handlers[18].Apply(); //Effect_DispelState - 1359
                //Handlers[19].Apply(); //Effect_TriggerBuff - 14004 LVL 1
            }
            else if (Spell.CurrentLevel == 3 && _target != null)
            {
                Handlers[0].Apply(); //Effect_RemoveSpellEffects - 14235
            }
            else if (Spell.CurrentLevel == 4 && _target != null)
            {
                Handlers[0].Apply(); //Effect_SkipTurn
            }
            else
            {
                base.Execute();
            }
        }
    }
}
