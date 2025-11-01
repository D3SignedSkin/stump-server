using System.Collections.Generic;
using System.Linq;
using NLog;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Spells;
using Stump.Server.WorldServer.Game.Spells.Casts;

namespace Stump.Server.WorldServer.Game.Effects.Handlers.Spells.States
{
    [EffectHandler(EffectsEnum.Effect_AddState)]
    public class AddState : SpellEffectHandler
    {
        static readonly SpellStatesEnum[] DISPELABLE_STATES =
        {
            SpellStatesEnum.INVULNERABLE_56,
            SpellStatesEnum.DRUNK_1
        };

        static readonly SpellStatesEnum[] BYPASSMAXSTACK_STATES =
        {
            SpellStatesEnum.UNLOAD_122,
            SpellStatesEnum.OVERLOAD_123,
            SpellStatesEnum.UNSHAKABLE_157,
            SpellStatesEnum.WEAKENED_42
        };

        static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public AddState(EffectDice effect, FightActor caster, SpellCastHandler castHandler, Cell targetedCell, bool critical) : base(effect, caster, castHandler, targetedCell, critical)
        {
            DefaultDispellableStatus = DISPELABLE_STATES.Contains((SpellStatesEnum)Dice.Value) ? FightDispellableEnum.DISPELLABLE : FightDispellableEnum.DISPELLABLE_BY_DEATH;
        }

        private static readonly List<int> InvulnerableMonsterIds = new List<int> { 2967 }; //Monsters que devem não ser aplicado o estado imune.

        protected override bool InternalApply()
        {
            foreach (var affectedActor in GetAffectedActors())
            {
                var state = SpellManager.Instance.GetSpellState((uint)Dice.Value);

                if (state == null)
                {
                    logger.Error("Spell state {0} not found", Dice.Value);
                    return false;
                }

                if (state.Id == (int)SpellStatesEnum.INVULNERABLE_56 || state.Id == (int)SpellStatesEnum.HEAVY_63)
                {
                    if (affectedActor.Fight.DefendersTeam.GetAllFighters().OfType<MonsterFighter>().Select(m => m.Monster.Template.Id).Any(id => InvulnerableMonsterIds.Contains(id)))
                    {
                        return false;
                    }
                }

                if (state.Id == (int)SpellStatesEnum.TELEFRAG_244 || state.Id == (int)SpellStatesEnum.TELEFRAG_251)
                {
                    affectedActor.NeedTelefragState = false;
                }

                AddStateBuff(affectedActor, BYPASSMAXSTACK_STATES.Contains((SpellStatesEnum)state.Id), state);
            }

            return true;
        }
    }
}