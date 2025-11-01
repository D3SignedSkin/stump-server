using System.Linq;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Fights.Buffs;
using Stump.Server.WorldServer.Game.Spells.Casts;

namespace Stump.Server.WorldServer.Game.Effects.Handlers.Spells.Damage
{
    [EffectHandler(EffectsEnum.Effect_DamageIntercept)]
    public class DamageIntercept : SpellEffectHandler
    {
        public DamageIntercept(EffectDice effect, FightActor caster, SpellCastHandler castHandler, Cell targetedCell, bool critical)
            : base(effect, caster, castHandler, targetedCell, critical)
        {
            DefaultDispellableStatus = FightDispellableEnum.DISPELLABLE_BY_DEATH;

        }

        public override bool CanApply() => !GetAffectedActors().Any(x => x.GetBuffs(y => y.Effect.EffectId == EffectsEnum.Effect_DamageIntercept).Any());

        protected override bool InternalApply()
        {
            foreach (var actor in GetAffectedActors())
            {
                AddTriggerBuff(actor, TriggerBuffApply);
            }

            return true;
        }

        public void TriggerBuffApply(TriggerBuff buff, FightActor triggerrer, BuffTriggerType trigger, object token)
        {
            var target = buff.Target;

            if (target == null)
                return;

            var damage = token as Fights.Damage;
            if (damage == null || damage.Amount == 0)
                return;

            damage.IgnoreDamageBoost = true;
            damage.IgnoreDamageReduction = true;
            damage.Generated = true;

            // first, apply damage to sacrifier
            if (buff.Spell != null)
            {
                if (buff.Spell.Id == (int)SpellIdEnum.BREAKWATER_3280)
                {
                    //todo calcular se o personagem esta na area da torre
                    if (triggerrer.Position.Point.IsAdjacentTo(buff.Target.Position.Point))
                    {
                        Caster.InflictDamage(damage);
                        damage.Amount = 0;
                    }
                    return;
                }
            }

            Caster.InflictDamage(damage);

            // then, negate damage given to target
            damage.Amount = 0;
        }
    }
}