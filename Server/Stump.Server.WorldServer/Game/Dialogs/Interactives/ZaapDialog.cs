using System;
using System.Collections.Generic;
using System.Linq;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Messages;
using Stump.DofusProtocol.Types;
using Stump.Server.BaseServer.Network;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Anomaly;
using Stump.Server.WorldServer.Game.Interactives;
using Stump.Server.WorldServer.Game.Maps;
using Stump.Server.WorldServer.Handlers.Dialogs;

namespace Stump.Server.WorldServer.Game.Dialogs.Interactives
{
    public class ZaapDialog : IDialog
    {
        readonly List<Map> m_destinations = new List<Map>();
        public Dictionary<Map, int> m_cellsId = new Dictionary<Map, int>();

        public ZaapDialog(Character character, InteractiveObject zaap)
        {
            Character = character;
            Zaap = zaap;
        }

        public ZaapDialog(Character character, InteractiveObject zaap, IEnumerable<Map> destinations)
        {
            Character = character;
            Zaap = zaap;

            if (character.Vip)
            {
                if (character.SubArea.Id == 966) //Ecaflip City
                {
                    var superArea = World.Instance.GetSuperArea(0);

                    foreach (var map in from map in superArea.Maps from interactive in map.GetInteractiveObjects().Where(interactive => interactive.Template != null && interactive.Template.Type == InteractiveTypeEnum.TYPE_ZAAP && interactive.Spawn.ElementId != 71355) select map)
                    {
                        if (map.Position.X == -1 && map.Position.Y == 0) //Zaap Fantasma
                            continue;

                        AddDestination(map);
                    }
                }
                else
                {
                    foreach (var map in from map in character.SuperArea.Maps from interactive in map.GetInteractiveObjects().Where(interactive => interactive.Template != null && interactive.Template.Type == InteractiveTypeEnum.TYPE_ZAAP && interactive.Spawn.ElementId != 71355) select map)
                    {
                        if (map.Position.X == -1 && map.Position.Y == 0) //Zaap Fantasma
                            continue;

                        AddDestination(map);
                    }
                }
            }
            else
            {
                m_destinations = destinations.ToList();
            }
        }

        public ZaapDialog(Character character, List<Map> destinations)
        {
            Character = character;
            m_destinations = destinations;
            UseTp = true;
        }

        public DialogTypeEnum DialogType => DialogTypeEnum.DIALOG_TELEPORTER;

        public bool UseTp
        {
            get;
            set;
        }

        public Character Character
        {
            get;
        }

        public InteractiveObject Zaap
        {
            get;
        }

        public void AddDestination(Map map)
        {
            m_destinations.Add(map);
        }

        public void Open()
        {
            Character.SetDialog(this);

            SendZaapListMessage(Character.Client);
        }

        public void Close()
        {
            Character.CloseDialog(this);
            DialogHandler.SendLeaveDialogMessage(Character.Client, DialogType);
        }

        public void Teleport(Map map)
        {
            if (!m_destinations.Contains(map))
                return;

            Cell cell;

            if (map.Zaap != null)
            {
                cell = map.GetCell(map.Zaap.Position.Point.GetCellInDirection(DirectionsEnum.DIRECTION_SOUTH_WEST, 1).CellId);

                if (!cell.Walkable)
                {
                    var adjacents = map.Zaap.Position.Point.GetAdjacentCells(entry => map.GetCell(entry).Walkable).ToArray();

                    if (adjacents.Length == 0)
                        throw new Exception(string.Format("Cannot find a free adjacent cell near the zaap (id:{0}) on map {1}", map.Zaap.Id, map.Id));

                    cell = map.GetCell(adjacents[0].CellId);
                }
            }
            else if (m_cellsId.ContainsKey(map))
            {
                cell = map.GetCell(m_cellsId.FirstOrDefault(x => x.Key == map).Value);
            }
            else
            {
                cell = map.GetFirstFreeCellNearMiddle();
            }

            if (!UseTp)
            {
                if (Character.Account.IsSubscribe == false)
                {
                    var cost = GetCostTo(map);

                    if (Character.Kamas < cost)
                        return;

                    Character.Inventory.SubKamas(cost);

                    //Você perdeu 1% kamas
                    Character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 46, cost);
                }
            }

            Character.Teleport(map, cell);

            Close();
        }

        public void SendZaapListMessage(IPacketReceiver client)
        {
            TeleportDestination[] _teleport = m_destinations.Select(entry => new TeleportDestination(
                type: (sbyte)TeleporterTypeEnum.TELEPORTER_ZAAP,
                mapId: entry.Id,
                subAreaId: (ushort)entry.SubArea.Id,
                level: (ushort)entry.SubArea.Record.Level,
                cost: (ushort)GetCostTo(entry))).ToArray();

            if (AnomalyManager.HasAnomaly())
            {
                _teleport = _teleport.Concat(AnomalyManager.GetMapsAnomaly()).ToArray();
            }

            client.Send(new ZaapDestinationsMessage((sbyte)TeleporterTypeEnum.TELEPORTER_ZAAP, _teleport, Character.Record.SpawnMapId ?? 0));
        }

        public short GetCostTo(Map map)
        {
            var pos = map.Position;
            var pos2 = Zaap.Map.Position;

            if (Character.Account.IsSubscribe == true)
            {
                return (short)Math.Floor(Math.Sqrt(0));
            }
            else
            {
                return (short)Math.Floor(Math.Sqrt((pos2.X - pos.X) * (pos2.X - pos.X) + (pos2.Y - pos.Y) * (pos2.Y - pos.Y)) * 10);
            }
        }
    }
}