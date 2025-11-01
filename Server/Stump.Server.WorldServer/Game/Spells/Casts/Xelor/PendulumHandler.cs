using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Fights;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stump.Server.WorldServer.Game.Spells.Casts.Xelor
{
    [SpellCastHandler(SpellIdEnum.PENDULUM_13294)]
    public class PendulumOneHandler : DefaultSpellCastHandler //By Kenshin Version 2.61.10
    {
        public PendulumOneHandler(SpellCastInformations cast) : base(cast)
        { }

        public override void Execute()
        {
            if (!base.Initialize())
                Initialize();

            FightActor _target = Fight.GetOneFighter(TargetedCell);

            //Effect_SymetricTargetTeleport
            Handlers[0].Apply();

            //Effect_DamageAir
            Handlers[1].SetAffectedActor(_target);
            Handlers[1].Apply();

            //Effect_CastSpell_1160 - 13305 LVL 3
            Handlers[2].Apply();

            //Effect_CastSpell_1160 - 13320 LVL 1
            Handlers[3].Apply();

            //Effect_CastSpell_1160 - 13265 LVL 1
            Handlers[4].Apply();

            //Effect_CastSpell_1160 - 13305 LVL 1
            Handlers[5].Apply();

            //Effect_DispelState - 2231
            Handlers[6].Apply();
        }
    }

    //[SpellCastHandler(SpellIdEnum.PENDULUM_13305)]
    //public class SandGlassTwoHandler : DefaultSpellCastHandler //By Kenshin Version 2.61.10
    //{
    //    public SandGlassTwoHandler(SpellCastInformations cast) : base(cast)
    //    { }

    //    public override void Execute()
    //    {
    //        if (!base.Initialize())
    //            Initialize();

    //        FightActor _target = Fight.GetOneFighter(TargetedCell);

    //        //Effect_DamageAir
    //        Handlers[0].AddAffectedActor(_target);
    //        Handlers[0].Apply();

    //        //Effect_ReturnToLastPos
    //        Handlers[1].AddAffectedActor(Fight.FighterPlaying);
    //        Handlers[1].Apply();

    //        //Effect_CastSpell_1160 - 13320 LVL 1
    //        Handlers[2].Apply();

    //        //Effect_CastSpell_1160 - 13265 LVL 1
    //        Handlers[3].Apply();
    //    }
    //}
}
