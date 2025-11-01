using System;
using System.Collections.Generic;
using System.Linq;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Messages;
using Stump.Server.BaseServer.Network;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Interactives;
using Stump.Server.WorldServer.Game.Maps;
using Stump.Server.WorldServer.Handlers.Dialogs;
using Stump.Server.WorldServer.Game.Dialogs;
using Stump.Server.WorldServer.Game;
using Stump.Server.BaseServer.Commands;
using Stump.DofusProtocol.Types;

namespace Stump.Server.WorldServer.Commands.Commands.Players
{
    public class ZaapDialog : IDialog
    {
        readonly List<Map> m_destinations = new List<Map>(){
                    World.Instance.GetMap(191106052),
                    World.Instance.GetMap(188745734),
                    World.Instance.GetMap(191104004),
                    World.Instance.GetMap(191105026)
        };

        public ZaapDialog(Character character, InteractiveObject zaap)
        {
            Character = character;
            Zaap = zaap;
        }

        public ZaapDialog(Character character)
        {
            Character = character;
        }

        public DialogTypeEnum DialogType => DialogTypeEnum.DIALOG_TELEPORTER;

        public Character Character
        {
            get;
        }

        public InteractiveObject Zaap
        {
            get;
        }

        public static void AddDestination(Map map)
        {

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
            {
                Character.OpenPopup("Erreur : Map Invalide");
                return;
            }

            //var cost = 0;

            //if (Character.Kamas < cost)
            //    return;

            // Character.Inventory.SubKamas(cost);

            Character.Teleport(map, map.GetFirstFreeCellNearMiddle());
            Close();
        }

        public void SendZaapListMessage(IPacketReceiver client)
        {
            client.Send(new ZaapDestinationsMessage((sbyte)TeleporterTypeEnum.TELEPORTER_ZAAP,
                m_destinations.Select
                (entry => new TeleportDestination((sbyte)TeleporterTypeEnum.TELEPORTER_ZAAP,
                entry.Id,
                (ushort)entry.SubArea.Id,
                (ushort)entry.SubArea.Record.Level,0)).ToArray(),
                Character.Record.SpawnMapId ?? 0));
        }
    }
}