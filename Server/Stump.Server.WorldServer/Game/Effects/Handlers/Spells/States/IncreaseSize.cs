using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Fights.Buffs.Customs;
using Stump.Server.WorldServer.Game.Spells.Casts;
using System.Collections.Generic;
using System.Linq;

namespace Stump.Server.WorldServer.Game.Effects.Handlers.Spells.States
{
    [EffectHandler(EffectsEnum.Effect_IncreaseSize)]
    public class IncreaseSize : SpellEffectHandler
    {
        public IncreaseSize(EffectDice effect, FightActor caster, SpellCastHandler castHandler, Cell targetedCell, bool critical) : base(effect, caster, castHandler, targetedCell, critical)
        {
        }

        List<int> MonsterIncreaseSize = new List<int> { (int)MonsterIdEnum.LA_SACRIFIE_116 };

        protected override bool InternalApply()
        {
            foreach (var actor in GetAffectedActors())
            {
                if (actor.Look.Scales.FirstOrDefault() > 200)
                    continue;

                if (actor is SummonedMonster && !MonsterIncreaseSize.Contains((actor as SummonedMonster).Monster.Template.Id) && actor.Look.Scales.FirstOrDefault() > 150)
                    continue;

                double DiceSize = (Dice.DiceNum / 100.0) + 1;

                if (actor is SummonedMonster && (actor as SummonedMonster).Monster.Template.Id == (int)MonsterIdEnum.LA_SACRIFIE_116)
                    DiceSize = 1.2;

                var buff = new RescaleSkinBuff(actor.PopNextBuffId(), actor, Caster, this, Spell, false, FightDispellableEnum.DISPELLABLE_BY_DEATH, DiceSize);
                actor.AddBuff(buff);
            }

            return true;
        }
    }
}
