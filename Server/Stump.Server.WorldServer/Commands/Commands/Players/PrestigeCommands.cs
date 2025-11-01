using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Commands;
using Stump.Server.WorldServer.Commands.Trigger;
using Stump.Server.WorldServer.Game;

namespace Stump.Server.WorldServer.Commands.Commands.Teleport
{
    public class PrestigeCommands : CommandBase
    {
        public PrestigeCommands()
        {
            Aliases = new[] { "prestige" };
            Description = "Permet de passer un prestige si vous êtes au niveau 200.";
            RequiredRole = RoleEnum.Player;
        }

        public override void Execute(TriggerBase trigger)
        {
            if (!(trigger is GameTrigger))
                return;

            var character = ((GameTrigger)trigger).Character;

            // Vérification si le joueur est en combat
            if (character.IsFighting())
            {
                character.DisplayNotification("Vous ne pouvez pas utiliser cette commande en combat.");
                return;
            }

            // Vérification du niveau requis
            const int requiredLevel = 200;
            if (character.Level < requiredLevel)
            {
                character.DisplayNotification($"Vous devez être au niveau <b><u>{requiredLevel}</u></b> pour passer un prestige.");
                return;
            }

            // Augmentation du prestige
            character.IncrementPrestige();
            character.DisplayNotification("Félicitations ! Vous avez passé un prestige.");
        }
    }
}
