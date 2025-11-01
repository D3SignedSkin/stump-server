using NLog;
using Stump.Server.WorldServer.Game.Actors.Fight;

namespace Stump.Server.WorldServer.Game.Effects.Handlers.Spells.Targets
{
    public class CanSummonCriterion : TargetCriterion
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        public CanSummonCriterion(bool caster, bool required)
        {
            this.Caster = caster;
            this.Required = required;
        }

        private bool Required { get; set; }

        public bool Caster { get; private set; }

        public override bool IsDisjonction => false;

        public override bool IsTargetValid(FightActor actor, SpellEffectHandler handler)
        {
            if (Caster)
                actor = handler.Caster;

            bool isValid = actor.CanSummon();

            if (Settings.App_Debug)
            {
                logger.Debug($"Verifying target validity, CanSummon: {isValid}");
            }

            if (!Required)
            {
                return !isValid;
            }
            else
            {
                return isValid;
            }
        }
    }
}
