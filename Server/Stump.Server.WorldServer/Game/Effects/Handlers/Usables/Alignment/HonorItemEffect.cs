using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Items.Player;

namespace Stump.Server.WorldServer.Game.Effects.Handlers.Usables
{
    [EffectHandler(EffectsEnum.Effect_AddHonor)]
    public class HonorItemEffect : UsableEffectHandler
    {
        public HonorItemEffect(EffectBase effect, Character target, BasePlayerItem item) : base(effect, target, item)
        { }

        protected override bool InternalApply()
        {
            var integerEffect = Effect.GenerateEffect(EffectGenerationContext.Item) as EffectInteger;

            if (integerEffect == null)
                return false;

            if (Target.AlignmentSide <= 0)
                return false;

            var amount = (int)(integerEffect.Value * NumberOfUses);

            UsedItems = NumberOfUses;

            Target.AddHonor((ushort)amount);

            Target.SendServerMessageLang(
                $"Você ganhou {amount} de honras.",
                $"You have earned {amount} of honors.",
                $"Has obtenido {amount} de honores.",
                $"Vous avez gagné {amount} d'honneurs.");

            return true;
        }
    }
}