using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Commands.Commands.Patterns;
using Stump.Server.WorldServer.Commands.Trigger;
using Stump.Server.WorldServer.Game.Actors.Look;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using System.Drawing;

namespace Stump.Server.WorldServer.Commands.Commands.GameMaster
{
    class SizeCommand : InGameCommand
    {
        public SizeCommand()
        {
            base.Aliases = new[] { "size" };
            base.RequiredRole = RoleEnum.Developer;
            Description = "Augmente votre taille.";
            AddParameter<short>("taille", "taille", "définissez la taille");
        }
        public override void Execute(GameTrigger trigger)
        {
            Character player = trigger.Character;
            player.Look.SetScales(trigger.Get<short>("taille"));
            player.RefreshActor();
            trigger.Reply("Votre taille est maintenant de <b>" + trigger.Get<short>("taille") + "</b> !");
        }
    }
}
