using System.Collections.Generic;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Messages;
using Stump.DofusProtocol.Types;
using Stump.Server.WorldServer.Core.Network;
using Stump.Server.WorldServer.Game;
using Stump.Server.WorldServer.Game.Custom;
using System.Linq;
using Stump.Server.WorldServer.Database.Custom;

namespace Stump.Server.WorldServer.Handlers.Custom
{
    public class ArcadiaHandler : WorldHandlerContainer
    {
        [WorldHandler(PocketTeleporterRequestInfosMessage.Id)]
        public static void HandlePocketTeleporterRequestInfosMessage(WorldClient client,
            PocketTeleporterRequestInfosMessage message)
        {
            if (client.Character.IsBusy() || client.Character.IsFighting() || client.Character.IsInFight() ||
                client.Character.IsInJail())
                return;

            var customMaps = ArcadiaManager.Instance.GetArcadiaTeleport();
            //client.Character.PlayEmote((EmotesEnum)247, true);
            SendPocketTeleporterDestinationsMessage(client, customMaps.ToList());
        }


        [WorldHandler(PocketTeleporterRequestTeleportMessage.Id)]
        public static void HandlePocketTeleporterRequestTeleportMessage(WorldClient client,
            PocketTeleporterRequestTeleportMessage message)
        {
            if (client.Character.IsBusy() || client.Character.IsFighting() || client.Character.IsInFight() ||
                client.Character.IsInJail())
                return;

            var arcadiateleport = ArcadiaManager.Instance.TryGetArcadiaTeleport((int)message.mapId);
            if (arcadiateleport == null)
                return;

            var map = World.Instance.GetMap((int)arcadiateleport.MapId);
            if (map == null)
                return;

            //client.Character.PlayEmote((EmotesEnum)248, true);
            if (client.Character.Map.Id == arcadiateleport.MapId)
                return;
            client.Character.Teleport(map, map.GetCell(300));
        }


        private static void SendPocketTeleporterDestinationsMessage(WorldClient client, List<ArcadiaTeleportPanelRecord> maps)
        {
            client.Character.PlayEmote((EmotesEnum)69, true);
            client.Send(new PocketTeleporterDestinationsMessage(maps.Select(x =>
                new PocketTeleporterDestination
                {
                    type = x.Type,
                    order = x.Order,
                    icon = x.Icon,
                    nameId = x.NameId,
                    mapId = x.MapId,
                    level = x.Level,
                    cost = x.Cost,
                })));
        }

       
    }
}