using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Fights;

namespace Stump.Server.WorldServer.Game.Spells.Casts.Ecaflip
{
    [SpellCastHandler(SpellIdEnum.BLOOD_PACT_12762)]
    public class BloodPactHandlre : DefaultSpellCastHandler //By Kenshin Version 2.61.10
    {
        public BloodPactHandlre(SpellCastInformations cast) : base(cast)
        { }

        public override void Execute()
        {
            if (!base.Initialize())
                Initialize();

            //Effect_AddVitalityPercent
            Handlers[0].Apply();

            //Effect_SubVitalityPercent_1048
            Handlers[1].Apply();

            //Effect_TriggerBuff
            Handlers[2].Apply();
        }
    }

    [SpellCastHandler(SpellIdEnum.SUFFERING_EVOLUTION_TRIGGERED_01_14087)]
    public class EvolutiveOneHandlre : DefaultSpellCastHandler //By Kenshin Version 2.61.10
    {
        public EvolutiveOneHandlre(SpellCastInformations cast) : base(cast)
        { }

        public override void Execute()
        {
            if (!base.Initialize())
                Initialize();

            //Effect_TriggerBuff - C,v100 (Remove os Estados)
            Handlers[0].Apply();

            //Sofrimento 1
            //Effect_TriggerBuff - C,V100,v90
            if (!Caster.HasState(616) && !Caster.HasState(617) && !Caster.HasState(618) && !Caster.HasState(619) && !Caster.HasState(620) && 
                !Caster.HasState(621) && !Caster.HasState(622) && !Caster.HasState(623) && !Caster.HasState(624) && !Caster.HasState(625))
            {
                Handlers[1].Apply();
                return;
            }

            //Sofrimento 2
            //Effect_TriggerBuff - C,V90,v80
            if (Caster.HasState(616))
            {
                Handlers[2].Apply();
                return;
            }

            //Sofrimento 3
            //Effect_TriggerBuff - C,V80,v70
            if (Caster.HasState(617))
            {
                Handlers[3].Apply();
                return;
            }

            //Sofrimento 4
            //Effect_TriggerBuff - C,V70,v60
            if (Caster.HasState(618))
            {
                Handlers[4].Apply();
                return;
            }

            //Sofrimento 5
            //Effect_TriggerBuff - C,V60,v50
            if (Caster.HasState(619))
            {
                Handlers[5].Apply();
                return;
            }

            //Sofrimento 6
            //Effect_TriggerBuff - C,V50,v40
            if (Caster.HasState(620))
            {
                Handlers[6].Apply();
                return;
            }

            //Sofrimento 7
            //Effect_TriggerBuff - C,V40,v30
            if (Caster.HasState(621))
            {
                Handlers[7].Apply();
                return;
            }

            //Sofrimento 8
            //Effect_TriggerBuff - C,V30,v20
            if (Caster.HasState(622))
            {
                Handlers[8].Apply();
                return;
            }

            //Sofrimento 9
            //Effect_TriggerBuff - C,V20,v10
            if (Caster.HasState(623))
            {
                Handlers[9].Apply();
                return;
            }

            //Sofrimento 10
            //Effect_TriggerBuff - C,V10
            if (Caster.HasState(624))
            {
                Handlers[10].Apply();
                return;
            }
        }
    }
}