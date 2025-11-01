using NLog;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.Fight;

namespace Stump.Server.WorldServer.Game.Effects.Handlers.Spells.Targets
{
    public class TargetTypeCriterion : TargetCriterion
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public TargetTypeCriterion(SpellTargetType type, bool caster)
        {
            TargetType = type;
            Caster = caster;
        }

        public SpellTargetType TargetType { get; set; }
        public bool Caster { get; set; }

        public override bool IsTargetValid(FightActor actor, SpellEffectHandler handler)
        {
            if (Caster)
                actor = handler.Caster;

            if (TargetType == SpellTargetType.NONE)
            {
                if (Settings.App_Debug)
                    logger.Debug($"TargetType is NONE for actor {actor}.");

                return true;
            }

            if (handler.Caster == actor && (TargetType.HasFlag(SpellTargetType.SELF) || TargetType.HasFlag(SpellTargetType.SELF_ONLY) || TargetType.HasFlag(SpellTargetType.ALLY_ALL)))
            {
                if (Settings.App_Debug)
                    logger.Debug($"Actor {actor} is caster and target type is {TargetType}.");

                return true;
            }

            if (TargetType.HasFlag(SpellTargetType.SELF_ONLY) && actor != handler.Caster)
            {
                if (Settings.App_Debug)
                    logger.Debug($"TargetType is SELF_ONLY and actor {actor} is not the caster.");

                return false;
            }

            if (handler.Caster.IsFriendlyWith(actor) && (handler.Caster != actor || Caster))
            {
                if (Settings.App_Debug)
                    logger.Debug($"Actor {actor} is friendly with caster {handler.Caster} and target type is {TargetType}.");

                if (TargetType == SpellTargetType.ALLY_ALL_EXCEPT_SELF || TargetType == SpellTargetType.ALLY_ALL)
                    return true;

                if (TargetType.HasFlag(SpellTargetType.ALLY_PLAYER) && actor is CharacterFighter)
                    return true;

                if (TargetType.HasFlag(SpellTargetType.ALLY_MONSTER) && actor is MonsterFighter)
                    return true;

                if (TargetType.HasFlag(SpellTargetType.ALLY_SUMMON) && actor is SummonedFighter)
                    return true;

                if (TargetType.HasFlag(SpellTargetType.ALLY_SUMMONER) && handler.Caster is SummonedFighter && ((SummonedFighter)handler.Caster).Summoner == actor)
                    return true;

                if (TargetType.HasFlag(SpellTargetType.ALLY_MONSTER_SUMMON) || TargetType.HasFlag(SpellTargetType.ALLY_NON_MONSTER_SUMMON) && actor is SummonedMonster)
                    return true;
            }

            if (!handler.Caster.IsEnnemyWith(actor))
            {
                if (Settings.App_Debug)
                    logger.Debug($"Actor {actor} is not an enemy of caster {handler.Caster}.");

                return false;
            }

            if (TargetType == SpellTargetType.ENEMY_ALL)
                return true;

            if (TargetType.HasFlag(SpellTargetType.ENEMY_PLAYER) || TargetType.HasFlag(SpellTargetType.ENEMY_HUMAN) && actor is CharacterFighter)
                return true;

            if (TargetType.HasFlag(SpellTargetType.ENEMY_MONSTER) && actor is MonsterFighter)
                return true;

            if (TargetType.HasFlag(SpellTargetType.ENEMY_SUMMON) && actor is SummonedFighter)
                return true;

            if (TargetType.HasFlag(SpellTargetType.ENEMY_MONSTER_SUMMON) || TargetType.HasFlag(SpellTargetType.ENEMY_NON_MONSTER_SUMMON) && actor is SummonedMonster)
                return true;

            if (Settings.App_Debug)
                logger.Debug($"No matching target type found for actor {actor} with target type {TargetType}.");

            return false;
        }
    }
}