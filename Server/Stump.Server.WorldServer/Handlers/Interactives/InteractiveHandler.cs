using System.Collections.Generic;
using Stump.Core.Reflection;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Messages;
using Stump.DofusProtocol.Types;
using Stump.Server.BaseServer.Network;
using Stump.Server.WorldServer.Core.Network;
using Stump.Server.WorldServer.Game;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Anomaly;
using Stump.Server.WorldServer.Game.Interactives;
using Stump.Server.WorldServer.Game.Interactives.Skills;

namespace Stump.Server.WorldServer.Handlers.Interactives
{
    public class InteractiveHandler : WorldHandlerContainer
    {
        [WorldHandler(InteractiveUseRequestMessage.Id)]
        public static void HandleInteractiveUseRequestMessage(WorldClient client, InteractiveUseRequestMessage message)
        {
            client.Character.Map.UseInteractiveObject(client.Character, (int)message.elemId, (int)message.skillInstanceUid, 1);
        }

        [WorldHandler(InteractiveUseWithParamRequestMessage.Id)]
        public static void HandleInteractiveUseWithParamRequestMessage(WorldClient client, InteractiveUseWithParamRequestMessage message)
        {
            client.Character.Map.UseInteractiveObject(client.Character, (int)message.elemId, (int)message.skillInstanceUid, message.id);
        }

        [WorldHandler(TeleportRequestMessage.Id)]
        public static void HandleTeleportRequestMessage(WorldClient client, TeleportRequestMessage message)
        {
            if (client.Character.IsInZaapDialog() && message.destinationType == (sbyte)TeleporterTypeEnum.TELEPORTER_ANOMALY)
            {
                AnomalyManager.AnomalyTeleport(client.Character);
            }
            else if (client.Character.IsInZaapDialog())
            {
                var map = World.Instance.GetMap((uint)message.mapId);

                if (map == null)
                    return;

                client.Character.ZaapDialog.Teleport(map);
            }
            else if (client.Character.IsInZaapiDialog())
            {
                var map = World.Instance.GetMap((uint)message.mapId);

                if (map == null)
                    return;

                client.Character.ZaapiDialog.Teleport(map);
            }
            else if (client.Character.IsInCustomZaapDialog())
            {
                var map = World.Instance.GetMap((uint)message.mapId);

                if (map == null)
                    return;

                client.Character.CustomZaapDialog.Teleport(map);
            }
            else if (client.Character.IsInDonjonZaapDialog())
            {
                var map = World.Instance.GetMap((uint)message.mapId);

                if (map == null)
                    return;

                client.Character.DonjonZaapDialog.Teleport(map, Singleton<World>.Instance.GetMap(map.Id).GetRandomFreeCell().Id);
                {
                    client.Character.SaveLater();
                }
            }
            else if (client.Character.IsInDoplonDialog())
            {
                var map = World.Instance.GetMap((uint)message.mapId);

                if (map == null)
                    return;

                client.Character.DopplesZaapDialog.Teleport(map, World.Instance.GetMap(map.Id).GetRandomFreeCell().Id);
            }
            else if (client.Character.IsInStartDialog())
            {
                var map = World.Instance.GetMap((uint)message.mapId);

                if (map == null)
                    return;

                client.Character.StartZaapDialog.Teleport(map, World.Instance.GetMap(map.Id).GetRandomFreeCell().Id);
            }
        }

        [WorldHandler(ZaapRespawnSaveRequestMessage.Id)]
        public static void HandleZaapRespawnSaveRequestMessage(WorldClient client, ZaapRespawnSaveRequestMessage message)
        {
            if (client.Character.Map.Zaap == null)
                return;

            client.Character.SetSpawnPoint(client.Character.Map);
        }

        public static void SendZaapRespawnUpdatedMessage(WorldClient client)
        {
            client.Send(new ZaapRespawnUpdatedMessage(client.Character.Record.SpawnMapId ?? 0));
        }

        public static void SendInteractiveUsedMessage(IPacketReceiver client, Character user, InteractiveObject interactiveObject, Skill skill)
        {
            //todo: CanMove
            client.Send(new InteractiveUsedMessage((ulong)user.Id, (uint)interactiveObject.Id, (ushort)skill.SkillTemplate.Id, (ushort)(skill.GetDuration(user, true) / 100), true));
        }

        public static void SendInteractiveUseErrorMessage(IPacketReceiver client, int interactiveId, int skillId)
        {
            client.Send(new InteractiveUseErrorMessage((uint)interactiveId, (uint)skillId));
        }

        public static void SendStatedElementUpdatedMessage(IPacketReceiver client, int elementId, short elementCellId, int state)
        {
            client.Send(new StatedElementUpdatedMessage(new StatedElement(elementId, (ushort)elementCellId, (uint)state, true)));
        }

        public static void SendMapObstacleUpdatedMessage(IPacketReceiver client, IEnumerable<MapObstacle> obstacles)
        {
            client.Send(new MapObstacleUpdateMessage(obstacles));
        }

        public static void SendInteractiveElementUpdatedMessage(IPacketReceiver client, Character character, InteractiveObject interactive)
        {
            client.Send(new InteractiveElementUpdatedMessage(interactive.GetInteractiveElement(character)));
        }

        public static void SendInteractiveUseEndedMessage(IPacketReceiver client, InteractiveObject interactive, Skill skill)
        {
            client.Send(new InteractiveUseEndedMessage((uint)interactive.Id, (ushort)skill.SkillTemplate.Id));
        }

        public static void SendInteractiveMapUpdateMessage(IPacketReceiver client, Character character, InteractiveObject interactive)
        {
            List<InteractiveElement> listInteractiveUpdate = new List<InteractiveElement>()
            {
                interactive.GetInteractiveElement(character)
            };

            client.Send(new InteractiveMapUpdateMessage(listInteractiveUpdate));
        }
    }
}