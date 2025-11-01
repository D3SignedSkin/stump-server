using Stump.Server.WorldServer.Game.Dialogs.Alliances;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Commands.Commands.Patterns;
using Stump.Server.WorldServer.Commands.Trigger;


namespace Stump.Server.WorldServer.Commands.Commands.GameMaster
{
    public class AllianceCreateCommand : InGameSubCommand
    {
        public AllianceCreateCommand()
        {
            base.Aliases = new string[]
          {
              "create"
          };

            base.RequiredRole = RoleEnum.Developer;
            ParentCommandType = typeof(AllianceCommand);
        }

        public override void Execute(GameTrigger trigger)
        {
            var allianceCreationPanel = new AllianceCreationPanel(trigger.Character);
            allianceCreationPanel.Open();
        }
    }
}
