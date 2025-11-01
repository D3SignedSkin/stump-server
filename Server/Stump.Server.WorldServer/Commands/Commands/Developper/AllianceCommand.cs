using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Commands;

namespace Stump.Server.WorldServer.Commands.Commands.GameMaster
{
    public class AllianceCommand : SubCommandContainer
    {
        public AllianceCommand()
        {
            base.Aliases = new string[]
            {
                "alliance"
            };

            base.RequiredRole = RoleEnum.Developer;
            base.Description = "Provides many commands to manage alliances";
        }
    }
}
