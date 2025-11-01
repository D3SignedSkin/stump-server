using Stump.Core.Reflection;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Effects;
using Stump.Server.WorldServer.Game.Effects.Handlers.Spells;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Fights;
using System.Linq;

namespace Stump.Server.WorldServer.Game.Spells.Casts.Ecaflip
{
    [SpellCastHandler(SpellIdEnum.MUTILATION_12737)]
    public class MutilationHandler : DefaultSpellCastHandler //By Kenshin Version 2.61.10
    {
        public MutilationHandler(SpellCastInformations cast) : base(cast)
        { }

        public override void Execute()
        {
            if (!base.Initialize())
                Initialize();

            if (Caster.HasState((int)SpellStatesEnum.BLEEDING_I_1229))
            {
                setShieldBuff(Handlers[6]);
            }

            if (Caster.HasState((int)SpellStatesEnum.BLEEDING_II_1230))
            {
                setShieldBuff(Handlers[7]);
            }

            if (Caster.HasState((int)SpellStatesEnum.BLEEDING_III_1231))
            {
                setShieldBuff(Handlers[8]);
            }

            //Adicionar estado Multilação
            Handlers[2].Apply();

            if (Caster.HasState(1302))
            {
                //Primeiro Trigger - Effect_TriggerBuff : 12775
                Handlers[0].Apply();
                //Primeiro Trigger - Effect_TriggerBuff : 12775
                Handlers[1].Apply();
                //Effect_IncreaseDamage_138
                Handlers[3].Apply();
                //Effect_SubVitalityPercent_1048
                Handlers[4].Apply();
            }

            //Effect_TriggerBuff
            Handlers[5].Apply();
        }

        private void setShieldBuff(SpellEffectHandler handler)
        {
            EffectDice current = new EffectDice(EffectsEnum.Effect_AddShieldLevelPercent, handler.Dice.DiceNum, 0, 0);
            var buff = Singleton<EffectManager>.Instance.GetSpellEffectHandler(current, Caster, this, TargetedCell, Critical);
            var shieldAmount = Caster.Owner.Level > 200 ? (short)(200 * (handler.Dice.DiceNum / 100d)) : (short)(Caster.Owner.Level * (handler.Dice.DiceNum / 100d));

            buff.AddStatBuff(Caster, shieldAmount, PlayerFields.Shield, (short)EffectsEnum.Effect_AddShield);
        }
    }

    [SpellCastHandler(SpellIdEnum.COAGULATION_12775)]
    public class CoagulationHandler : DefaultSpellCastHandler //By Kenshin Version 2.61.10
    {
        public CoagulationHandler(SpellCastInformations cast) : base(cast)
        { }

        public override void Execute()
        {
            if (!base.Initialize())
                Initialize();

            Handlers[0].Apply();

            if (Caster.HasState(1229))
            {
                Handlers[1].Apply();
                return;
            }

            if (Caster.HasState(1230))
            {
                Handlers[2].Apply();
                return;
            }

            if (Caster.HasState(1231))
            {
                Handlers[3].Apply();
                return;
            }
        }
    }
}