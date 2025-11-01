using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Types;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Spells;
using System;
using Stump.Server.WorldServer.Game.Effects.Handlers.Spells;

namespace Stump.Server.WorldServer.Game.Fights.Buffs.Customs
{
    public class TakeControlBuff : Buff
    {
        public TakeControlBuff(int id, FightActor target, FightActor caster, SpellEffectHandler effect, Spell spell, FightDispellableEnum dispelable, SummonedMonster summon)
            : base(id, target, caster, effect, spell, false, dispelable)
        {
            Summon = summon;
        }
        public TakeControlBuff(int id, FightActor target, FightActor caster, SpellEffectHandler effect, Spell spell, FightDispellableEnum dispelable, SummonedTurret summon)
          : base(id, target, caster, effect, spell, false, dispelable)
        {
            Summon_Turret = summon;
        }
        public TakeControlBuff(int id, FightActor target, FightActor caster, SpellEffectHandler effect, Spell spell, FightDispellableEnum dispelable, SummonedClone summon)
          : base(id, target, caster, effect, spell, false, dispelable)
        {
            Summon_Clone = summon;
        }

        public SummonedMonster Summon
        {
            get;
        }
        public SummonedTurret Summon_Turret
        {
            get;
        }
        public SummonedClone Summon_Clone
        {
            get;
        }

        public override void Apply()
        {
            base.Apply();

            if (!(Caster is CharacterFighter))
                return;
            if (Summon != null)            
                Summon.SetController(Caster as CharacterFighter);
            else if (Summon_Turret != null)
                Summon_Turret.SetController(Caster as CharacterFighter);
            else if (Summon_Clone != null)
                Summon_Clone.SetController(Caster as CharacterFighter);

        }

        public override void Dispell()
        {
            base.Dispell();
            
            if (Summon != null)
                Summon.SetController(null);
            else if (Summon_Turret != null)
                Summon_Turret.SetController(null);
            else if (Summon_Clone != null)
                Summon_Clone.SetController(null);
        }

        public override AbstractFightDispellableEffect GetAbstractFightDispellableEffect()
        {
            var values = Effect.GetValues();

            if (Delay == 0)
                return new FightTemporaryBoostEffect((uint)Id, Target.Id, Duration, (sbyte)Dispellable,
                    (ushort)Spell.Id, (uint)(EffectFix?.ClientEffectId ?? Effect.Id), 0, 0);

            return new FightTriggeredEffect((uint)Id, Target.Id, Delay,
                (sbyte)Dispellable,
                (ushort)Spell.Id, (uint)(EffectFix?.ClientEffectId ?? Effect.Id), 0,
                (values.Length > 0 ? Convert.ToInt32(values[0]) : 0),
                (values.Length > 1 ? Convert.ToInt32(values[1]) : 0),
                (values.Length > 2 ? Convert.ToInt32(values[2]) : 0),
                Delay);
        }
    }
}
