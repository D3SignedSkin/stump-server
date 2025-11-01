using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Commands;
using Stump.Server.WorldServer.Commands.Trigger;
using Stump.Server.WorldServer.Game;

namespace Stump.Server.WorldServer.Commands.Commands.Teleport
{
    public class CheckPrestigeCommand : CommandBase
    {
        public CheckPrestigeCommand()
        {
            Aliases = new[] { "checkprestige", "prestigeinfo" };
            Description = "Permet de connaître votre niveau de prestige actuel.";
            RequiredRole = RoleEnum.Player;
        }

        public override void Execute(TriggerBase trigger)
        {
            if (!(trigger is GameTrigger))
                return;

            var character = ((GameTrigger)trigger).Character;

            // Notification du prestige actuel
            int currentPrestige = character.PrestigeRank;
            character.DisplayNotification($"Votre niveau de prestige actuel est : <b><u>{currentPrestige}</u></b>.");
        }
    }
}
