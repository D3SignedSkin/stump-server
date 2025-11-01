using Stump.Core.Reflection;
using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Commands;
using Stump.Server.WorldServer.Commands.Trigger;
using Stump.Server.WorldServer.Game;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Maps.Cells;

namespace Stump.Server.WorldServer.Commands.Commands.Players
{
    class StartCommand : CommandBase
    {
        public static void Teleport (Character player, int mapId, short cellId, DirectionsEnum playerDirection)
        {
            player.Teleport (new ObjectPosition (Singleton<World>.Instance.GetMap (mapId), cellId, playerDirection));
        }

        public StartCommand ()
        {
            Aliases = new [] { "start" };
            RequiredRole = RoleEnum.Player;
            Description = "Teletransporta para o Zaap de Astrub.";
            Description_en = "Teleports to Zaap from Astrub.";
            Description_es = "Teletransportarse al Astrub Zap.";
            Description_fr = "Téléportez-vous à l'Astrub Zap.";
        }

        public override void Execute (TriggerBase trigger)
        {
            var gameTrigger = trigger as GameTrigger;
            if (gameTrigger != null)
            {
                var player = gameTrigger.Character;
                Teleport (player, 192416776, 455, DirectionsEnum.DIRECTION_SOUTH_EAST);
            }
        }
    }
}