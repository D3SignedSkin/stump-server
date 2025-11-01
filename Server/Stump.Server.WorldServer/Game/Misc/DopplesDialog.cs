using System;
using System.Collections.Generic;
using System.Linq;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Messages;
using Stump.Server.BaseServer.Network;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Interactives;
using Stump.Server.WorldServer.Game.Maps;
using Stump.Server.WorldServer.Handlers.Dialogs;
using Stump.Server.WorldServer.Game.Dialogs;
using Stump.DofusProtocol.Types;

namespace Stump.Server.WorldServer.Game.Misc
{
    public class DopplesZaapDialog : IDialog
    {
        readonly List<Map> m_destinations = new List<Map>(){
                    World.Instance.GetMap(183767050), //Sacrier
                    World.Instance.GetMap(183766020), //Ecaflip
                    World.Instance.GetMap(183769090), //Panda
                    World.Instance.GetMap(148636161), //Eliotrope
                    World.Instance.GetMap(69207040),  //Zobal
                    World.Instance.GetMap(177210626), //Kilorf
                    World.Instance.GetMap(163053570), //Huppermago
                    World.Instance.GetMap(67109888),  //Ladinos
                    World.Instance.GetMap(183768066), //Sram
                    World.Instance.GetMap(183762944), //Osamodas
                    World.Instance.GetMap(17048578),  //Enutrof
                    World.Instance.GetMap(183762956), //Iop
                    World.Instance.GetMap(183765002), //Cra
                    World.Instance.GetMap(183763980), //Xelor
                    World.Instance.GetMap(183763974), //Eni
                    World.Instance.GetMap(96471552),  //Steamer
                    World.Instance.GetMap(183764994), //Feca
                    World.Instance.GetMap(183768076), //Sadida
        };

        public DopplesZaapDialog(Character character, InteractiveObject zaap)
        {
            Character = character;
            Zaap = zaap;
        }

        public DopplesZaapDialog(Character character)
        {
            Character = character;
        }

        public DialogTypeEnum DialogType => DialogTypeEnum.DIALOG_TELEPORTER;

        public Character Character
        {
            get;
        }

        public bool UseTp
        {
            get;
            set;
        }


        public InteractiveObject Zaap
        {
            get;
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

        public void Teleport(Map map, int cell)
        {

            Character.Teleport(map, map.GetCell(cell));

            if (!UseTp)
            {
                var cost = GetCostTo(map);

                if (Character.Kamas < cost)
                    return;

                Character.Inventory.SubKamas(cost);

                //Você perdeu 1% kamas
                Character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 46, cost);
            }
            Close();
        }

        public void SendZaapListMessage(IPacketReceiver client)
        {
            client.Send(new ZaapDestinationsMessage((sbyte)TeleporterTypeEnum.TELEPORTER_ZAAP,
                m_destinations.Select
                (entry => new TeleportDestination((sbyte)TeleporterTypeEnum.TELEPORTER_ZAAP,
                entry.Id,
                (ushort)entry.SubArea.Id,
                (ushort)entry.SubArea.Record.Level,
                (ushort)GetCostTo(entry))).ToArray(),
                Character.Record.SpawnMapId ?? 0));
        }

        public short GetCostTo(Map map)
        {
            var pos = map.Position;
            var pos2 = Character.Map.Position;

            return (short)Math.Floor(Math.Sqrt(((pos2.X - pos.X) * (pos2.X - pos.X) + (pos2.Y - pos.Y) * (pos2.Y - pos.Y)) * 10) + 95000);
        }
    }
}