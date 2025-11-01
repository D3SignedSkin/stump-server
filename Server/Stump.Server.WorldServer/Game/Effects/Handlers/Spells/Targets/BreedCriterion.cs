using NLog;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.Fight;

namespace Stump.Server.WorldServer.Game.Effects.Handlers.Spells.Targets
{
    public class BreedCriterion : TargetCriterion
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        public BreedCriterion(int breed, bool caster, bool required)
        {
            Breed = breed;
            Caster = caster;
            Required = required;
        }

        public int Breed
        {
            get;
            set;
        }

        public bool Caster
        {
            get;
            set;
        }

        public bool Required
        {
            get;
            set;
        }

        public override bool IsDisjonction => false;

        public override bool IsTargetValid(FightActor actor, SpellEffectHandler handler)
        {
            if (Caster)
                actor = handler.Caster;

            if (Settings.App_Debug)
            {
                var actorName = actor.Summoner != null ? actor.Summoner.Owner.Name : (actor as SummonedFighter).Name;
                logger.Debug($"Verifying target validity for actor {actorName}, Breed: {Breed}");
            }

            bool isValid;

            if (actor is CharacterFighter)
            {
                isValid = Required ? (int)((CharacterFighter)actor).Character.BreedId == Breed : (int)((CharacterFighter)actor).Character.BreedId != Breed;

                if (Settings.App_Debug)
                {
                    logger.Debug($"Target is a character with BreedId: {((CharacterFighter)actor).Character.BreedId}, Required: {Required}, Valid: {isValid}");
                }
            }
            else
            {
                isValid = Required ? (int)BreedEnum.MONSTER == Breed : (int)BreedEnum.MONSTER != Breed;

                if (Settings.App_Debug)
                {
                    logger.Debug($"Target is a monster, BreedId: {BreedEnum.MONSTER}, Required: {Required}, Valid: {isValid}");
                }
            }

            return isValid;
        }
    }
}
