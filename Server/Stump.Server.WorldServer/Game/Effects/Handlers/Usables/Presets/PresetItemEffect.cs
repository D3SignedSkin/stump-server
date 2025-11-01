//using Stump.DofusProtocol.Enums;
//using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
//using Stump.Server.WorldServer.Game.Effects.Instances;
//using Stump.Server.WorldServer.Game.Items.Player;
//using System;

//namespace Stump.Server.WorldServer.Game.Effects.Handlers.Usables
//{
//    [EffectHandler(EffectsEnum.Effect_AddArrangement)]
//    public class PresetItemEffect : UsableEffectHandler
//    {
//        public PresetItemEffect(EffectBase effect, Character target, BasePlayerItem item) : base(effect, target, item)
//        {
//        }

//        protected override bool InternalApply()
//        {
//            var integerEffect = Effect.GenerateEffect(EffectGenerationContext.Item) as EffectInteger;

//            if (integerEffect == null)
//                return false;

//            if (Target.SlotsArrangements == 18)
//            {
//                Target.SendServerMessageLangColor
//                    (
//                    "Você atingiu o máximo de espaços para arranjos.",
//                    "You have reached the maximum arrangement slots.",
//                    "Has alcanzado los espacios máximos de disposición.",
//                    "Vous avez atteint les espaces de rangement maximum.",
//                    System.Drawing.Color.OrangeRed
//                    );
//                return false;
//            }

//            var amount = (int)(integerEffect.Value * NumberOfUses);

//            UsedItems = NumberOfUses;

//            if (Target.SlotsArrangements + amount > 18)
//            {
//                Target.SendServerMessageLangColor
//                    (
//                    "Você atingiu o máximo de espaços para arranjos.",
//                    "You have reached the maximum arrangement slots.",
//                    "Has alcanzado los espacios máximos de disposición.",
//                    "Vous avez atteint les espaces de rangement maximum.",
//                    System.Drawing.Color.OrangeRed
//                    );  
//                return false;
//            }

//            Target.AddArrangements((ushort)amount);
//            Target.SaveLater();
//            Target.SendServerMessageLang
//                (
//                "Você adicionou um novo espaço para arranjo com sucesso.",
//                "You have successfully added a new arrangement space.",
//                "Ha agregado con éxito un nuevo espacio de arreglos.",
//                "Vous avez ajouté avec succès un nouvel espace d'arrangement."
//                );

//            return true;
//        }
//    }
//}
