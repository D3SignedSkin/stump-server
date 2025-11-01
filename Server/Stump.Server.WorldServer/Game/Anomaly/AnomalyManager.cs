using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Types;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using System.Collections.Generic;

namespace Stump.Server.WorldServer.Game.Anomaly
{
    public class AnomalyManager
    {
        public static bool HasAnomaly() => false;

        public static ushort GetAnomalylevel() => 200;

        public static ulong GetAnomalyClosing() => 225265;

        public static void AnomalyTeleport(Character character)
        {
            var map = World.Instance.GetMap(196083720);

            character.Teleport(map, character.Cell);
        }

        public static List<TeleportDestination> GetMapsAnomaly()
        {
            List<TeleportDestination> anomaly = new List<TeleportDestination>();

            anomaly.Add(new TeleportDestination(
                    type: (sbyte)TeleporterTypeEnum.TELEPORTER_ANOMALY,
                    mapId: 196083720,
                    subAreaId: 10,
                    level: 200,
                    cost: 10));

            return anomaly;
        }
    }
}