//using Stump.DofusProtocol.Enums;
//using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
//using Stump.Server.WorldServer.Game.Effects.Handlers.Usables;
//using Stump.Server.WorldServer.Game.Effects.Instances;
//using Stump.Server.WorldServer.Game.Items.Player;
//using System.Linq;

//namespace Stump.Server.WorldServer.Game.Effects.Handlers.Items
//{
//    [EffectHandler(EffectsEnum.Effect_Party)]
//    public class Party : UsableEffectHandler
//    {
//        public Party(EffectBase effect, Character target, BasePlayerItem item) : base(effect, target, item)
//        {
//        }

//        protected override bool InternalApply()
//        {
//            var integerEffect = Effect.GenerateEffect(EffectGenerationContext.Item) as EffectInteger;

//            if (integerEffect == null)
//                return false;

//            if (Target.IsInFight())
//                return false;

//            if (!Target.Account.IsSubscribe)
//            {
//                Target.SendServerMessageLang
//                    (
//                    "Você precisa ser VIP para poder utilizar esse item.",
//                    "You need to be a VIP to use this item.",
//                    "Necesitas ser un VIP para usar este artículo.",
//                    "Vous devez être un VIP pour utiliser cet article."
//                    );
//                return false;
//            }


//            UsedItems = NumberOfUses;

//            if (Target.IsInParty())
//            {
//                Target.SendServerMessageLang
//                    (
//                    "Você já faz parte de um grupo.",
//                    "You are already part of a group.",
//                    "Ya eres parte de un grupo.",
//                    "Vous faites déjà partie d'un groupe."
//                    );

//                return false;
//            }
//            else
//            {
//                var Characters = WorldServer.Instance.FindClients(x => x.IP == Target.Client.IP && x.Character != Target && Target.Account.IsSubscribe);

//                if (Characters.Count() < 1)
//                {
//                    Target.SendServerMessageLang
//                        (
//                        "Você não possui mais contas conectadas ao reino.",
//                        "You no longer have accounts connected to the realm.",
//                        "Ya no tienes cuentas conectadas al reino.",
//                        "Vous n'avez plus de comptes connectés au royaume."
//                        );

//                    return false;
//                }
//                else
//                {
//                    foreach (var perso in Characters.Take(8))
//                    {
//                        if (Target.Party != null && Target.Party.Members.Contains(perso.Character))
//                            continue;

//                        Target.Invite(perso.Character, PartyTypeEnum.PARTY_TYPE_CLASSICAL, true);
//                    }
//                }
//            }

//            return true;
//        }
//    }
//}
