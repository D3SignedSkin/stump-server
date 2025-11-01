using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Items.Player;

namespace Stump.Server.WorldServer.Game.Effects.Handlers.Usables
{
    [EffectHandler(EffectsEnum.Effect_AddOgrines)]
    public class GiveOgrines : UsableEffectHandler
    {
        public GiveOgrines(EffectBase effect, Character target, BasePlayerItem item) : base(effect, target, item)
        { }

        protected override bool InternalApply()
        {
            var integerEffect = Effect.GenerateEffect(EffectGenerationContext.Item) as EffectInteger;

            if (integerEffect == null)
                return false;

            if (Target.Inventory.CanTokenBlock() == true)
            {
                //Hydra: A interação com Ogrines está em manutenção, por favor, tentar novamente mais tarde.
                Target.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 244);
                return false;
            }

            var amount = (int)(integerEffect.Value * NumberOfUses);

            UsedItems = NumberOfUses;

            if (Target.Client.Character.Inventory.CreateTokenItem(amount, Item.Template.Name))
            {
                Target.Client.Character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 21, amount, Settings.TokenTemplateId);

                if (Item.Template.Id == 30307 || Item.Template.Id == 30308 || Item.Template.Id == 30309)
                {
                    Target.Client.Character.PlayEmote(EmotesEnum.EMOTE_OGRINE, true);
                }

                return true;
            }

            return false;
        }
    }
}
