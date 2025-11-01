using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Commands.Commands.Patterns;
using Stump.Server.WorldServer.Commands.Trigger;
using Stump.Server.WorldServer.Game.Actors.Look;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stump.Server.WorldServer.Commands.Commands.GameMaster
{

    class Skin : InGameCommand
    {
        public Skin()
        {
            base.Aliases = new string[]
            {
                "skin"
            };
            base.RequiredRole = RoleEnum.Developer;
            base.Description = "muda a skin";

        }


        public override void Execute(GameTrigger trigger)
        {
            Character player = trigger.Character;
            string look = Convert.ToString(player.DefaultLook);
            bool active = player.CustomLookActivated;
            ActorLook customlook = player.CustomLook;
            player.SendServerMessage("Look : {0} " + look + " Ativo : " + active + " Customlook: " + customlook + ".");
        }
    }
}

