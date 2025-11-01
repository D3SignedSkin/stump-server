using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Fights.Buffs;
using Stump.Server.WorldServer.Game.Spells;
using Stump.Server.WorldServer.Game.Spells.Casts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Stump.Server.WorldServer.Game.Effects.Handlers.Spells.Buffs
{
    [EffectHandler(EffectsEnum.Effect_SpellObstaclesDisable)]
    public class ObstaclesDisable : SpellEffectHandler
    {
        public ObstaclesDisable(EffectDice effect, FightActor caster, SpellCastHandler castHandler, Cell targetedCell, bool critical) : base(effect, caster, castHandler, targetedCell, critical)
        { }

        protected override bool InternalApply()
        {
            try
            {
                // Verifica se GetAffectedActors() retorna algum valor
                var affectedActors = GetAffectedActors().ToList();

                if (!affectedActors.Any())
                    return false;

                var playableSpells = new HashSet<CharacterSpell>(affectedActors.SelectMany(x => x.Owner.Spells.GetPlayableSpells().Where(s => s.Id != 0)));

                foreach (var actor in affectedActors)
                {
                    // Verifica se GenerateEffect() retornou um valor nulo
                    var integerEffect = GenerateEffect();

                    if (integerEffect == null)
                        continue;

                    // Verifica se o valor de Dice.Value é válido
                    if (Dice.Value <= 0)
                        continue;

                    var buff = new SpellBuff(actor.PopNextBuffId(), actor, Caster, this, Spell, Spell, (short)Dice.Value, false, FightDispellableEnum.DISPELLABLE_BY_DEATH, playableSpells.ToList());
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