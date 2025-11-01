//using Stump.DofusProtocol.Enums;
//using Stump.Server.WorldServer.Game.Actors.Fight;
//using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
//using Stump.Server.WorldServer.Game.Effects.Handlers.Usables;
//using Stump.Server.WorldServer.Game.Effects.Instances;
//using Stump.Server.WorldServer.Game.Fights;
//using Stump.Server.WorldServer.Game.Items.Player;

//namespace Stump.Server.WorldServer.Game.Effects.Handlers.Items
//{
//    [EffectHandler(EffectsEnum.Effect_FightPass)]
//    public class FightPass : UsableEffectHandler
//    {
//        public FightPass(EffectBase effect, Character target, BasePlayerItem item) : base(effect, target, item)
//        {
//        }

//        CharacterFighter Fighter;

//        protected override bool InternalApply()
//        {
//            var integerEffect = Effect.GenerateEffect(EffectGenerationContext.Item) as EffectInteger;

//            if (integerEffect == null)
//                return false;

//            if (Target.IsInFight())
//                return false;

//            if (!Target.Account.IsSubscribe)
//                return false;

//            UsedItems = NumberOfUses;

//            if (!Target.ForcePassTurn)
//            {
//                if (Target.Fighter != null)
//                {
//                    Fighter = Target.Fighter;
//                    Target.ContextChanged += OnContextChanged;
//                    Target.Fighter.Fight.TurnStarted += OnTurnStarted;
//                }
//                else
//                {
//                    Target.ContextChanged += OnContextChanged;
//                }

//                #region MSG
//                switch (Target.Account.Lang)
//                {
//                    case "fr":
//                        Target.SendServerMessage("Maintenant, vous sauterez automatiquement vos combats.");
//                        Target.ForcePassTurn = true;
//                        break;
//                    case "es":
//                        Target.SendServerMessage("Ahora te saltearás automáticamente tus peleas.");
//                        Target.ForcePassTurn = true;
//                        break;
//                    case "en":
//                        Target.SendServerMessage("Now you will automatically skip your fights.");
//                        Target.ForcePassTurn = true;
//                        break;
//                    default:
//                        Target.SendServerMessage("Agora você irá pular automaticamente suas lutas.");
//                        Target.ForcePassTurn = true;
//                        break;
//                }
//                #endregion
//            }
//            else
//            {
//                if (Target.Fighter != null)
//                {
//                    Fighter = Target.Fighter;
//                    Target.ContextChanged -= OnContextChanged;
//                    Target.Fighter.Fight.TurnStarted -= OnTurnStarted;
//                }
//                else
//                {
//                    Target.ContextChanged -= OnContextChanged;
//                }

//                #region MSG
//                switch (Target.Account.Lang)
//                {
//                    case "fr":
//                        Target.SendServerMessage("Vous pouvez maintenant jouer normalement.");
//                        Target.ForcePassTurn = true;
//                        break;
//                    case "es":
//                        Target.SendServerMessage("Ahora puedes jugar normalmente.");
//                        Target.ForcePassTurn = true;
//                        break;
//                    case "en":
//                        Target.SendServerMessage("You can now play normally.");
//                        Target.ForcePassTurn = true;
//                        break;
//                    default:
//                        Target.SendServerMessage("Agora você pode jogar normalmente.");
//                        Target.ForcePassTurn = true;
//                        break;
//                }
//                #endregion
//            }

//            return true;
//        }

//        private void OnContextChanged(Character character, bool inFight)
//        {
//            if (character.Fighter != null)
//            {
//                Fighter = character.Fighter;
//                character.Fighter.Fight.TurnStarted += OnTurnStarted;
//            }
//        }

//        private void OnTurnStarted(IFight fight, FightActor actor)
//        {
//            if (Fighter != null && actor != Fighter)
//                return;

//            Fighter.PassTurn();
//        }
//    }
//}
