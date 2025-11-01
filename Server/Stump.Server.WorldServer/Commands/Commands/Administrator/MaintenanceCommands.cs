using Stump.Core.Reflection;
using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer;
using Stump.Server.BaseServer.Commands;
using Stump.Server.WorldServer.Game;
using System;
using System.Drawing;

namespace Stump.Server.WorldServer.Commands.Commands.GameMaster
{
    public class MaintenanceAnnounce : CommandBase
    {
        public MaintenanceAnnounce()
        {
            base.Aliases = new string[]
            {
                "mantenimiento"
            };
            base.Description = "Envia un anuncio a todos los conectados";
            base.RequiredRole = RoleEnum.Administrator;
        }
        public override void Execute(TriggerBase trigger)
        {
            {
                ServerBase<WorldServer>.Instance.IOTaskPool.AddMessage(new Action(Singleton<World>.Instance.SendAnnounceAllPlayersShutDown));               

            }

        }
    }
}
