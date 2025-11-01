using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Fights.Buffs;
using Stump.Server.WorldServer.Game.Spells;
using Stump.Server.WorldServer.Game.Spells.Casts;
using System;
using System.Linq;

namespace Stump.Server.WorldServer.Game.Effects.Handlers.Spells.Buffs
{
    [EffectHandler(EffectsEnum.Effect_SpellRangeIncrease)]
    public class SpellRangeIncrease : SpellEffectHandler
    {
        public SpellRangeIncrease(EffectDice effect, FightActor caster, SpellCastHandler castHandler, Cell targetedCell, bool critical) : base(effect, caster, castHandler, targetedCell, critical)
        { }

        protected override bool InternalApply()
        {
            try
            {
                // Verifica se GetAffectedActors() retorna algum valor
                var affectedActors = GetAffectedActors().ToList();

                if (!affectedActors.Any())
                    return false;

                foreach (var actor in affectedActors)
                {
                    // Verifica se GenerateEffect() retornou um valor nulo
                    var integerEffect = GenerateEffect();

                    if (integerEffect == null)
                        continue;

                    var buff = new SpellBuff(actor.PopNextBuffId(), actor, Caster, this, Spell, new Spell(Dice.DiceNum, 1), (short)Dice.Value, false, FightDispellableEnum.DISPELLABLE_BY_DEATH);
                    actor.AddBuff(buff);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
    }
}