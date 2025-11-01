using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Commands;
using Stump.Server.WorldServer.Commands.Commands.Patterns;
using Stump.Server.WorldServer.Commands.Trigger;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Exchanges.Bank;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Handlers.Inventory;

namespace Stump.Server.WorldServer.Commands.Commands.Players
{
    public class PassCommand : InGameCommand
    {
        public PassCommand()
        {
            Aliases = new[] { "pass" };
            Description = "Permite que você pule as lutas automaticamente.";
            Description_es = "Te permite saltarte peleas automáticamente.";
            Description_en = "Allows you to skip fights automatically.";
            Description_fr = "Vous permet de sauter automatiquement les combats.";
            RequiredRole = RoleEnum.Vip;
        }

        CharacterFighter Fighter;

        public override void Execute(GameTrigger trigger)
        {
            if (!trigger.Character.ForcePassTurn)
            {
                if(trigger.Character.Fighter != null)
                {
                    Fighter = trigger.Character.Fighter;
                    trigger.Character.ContextChanged += OnContextChanged;
                    trigger.Character.Fighter.Fight.TurnStarted += OnTurnStarted;
                }
                else
                {
                    trigger.Character.ContextChanged += OnContextChanged;
                }
                switch (trigger.Character.Account.Lang)
                {
                    case "fr":
                        trigger.Character.SendServerMessage("Maintenant, vous sauterez automatiquement vos combats.");
                        trigger.Character.ForcePassTurn = true;
                        break;
                    case "es":
                        trigger.Character.SendServerMessage("Ahora te saltearás automáticamente tus peleas.");
                        trigger.Character.ForcePassTurn = true;
                        break;
                    case "en":
                        trigger.Character.SendServerMessage("Now you will automatically skip your fights.");
                        trigger.Character.ForcePassTurn = true;
                        break;
                    default:
                        trigger.Character.SendServerMessage("Agora você irá pular automaticamente suas lutas.");
                        trigger.Character.ForcePassTurn = true;
                        break;
                }
            }
            else
            {
                if (trigger.Character.Fighter != null)
                {
                    Fighter = trigger.Character.Fighter;
                    trigger.Character.ContextChanged -= OnContextChanged;
                    trigger.Character.Fighter.Fight.TurnStarted -= OnTurnStarted;
                }
                else
                {
                    trigger.Character.ContextChanged -= OnContextChanged;
                }
                switch (trigger.Character.Account.Lang)
                {
                    case "fr":
                        trigger.Character.SendServerMessage("Vous pouvez maintenant jouer normalement.");
                        trigger.Character.ForcePassTurn = true;
                        break;
                    case "es":
                        trigger.Character.SendServerMessage("Ahora puedes jugar normalmente.");
                        trigger.Character.ForcePassTurn = true;
                        break;
                    case "en":
                        trigger.Character.SendServerMessage("You can now play normally.");
                        trigger.Character.ForcePassTurn = true;
                        break;
                    default:
                        trigger.Character.SendServerMessage("Agora você pode jogar normalmente.");
                        trigger.Character.ForcePassTurn = true;
                        break;
                }
            }
        }

        private void OnContextChanged(Character character, bool inFight)
        {
            if(character.Fighter != null)
            {
                Fighter = character.Fighter;
                character.Fighter.Fight.TurnStarted += OnTurnStarted;
            }
        }

        private void OnTurnStarted(IFight fight, FightActor actor)
        {
            if (Fighter != null && actor != Fighter)
                return;

            Fighter.PassTurn();
        }
    }
}