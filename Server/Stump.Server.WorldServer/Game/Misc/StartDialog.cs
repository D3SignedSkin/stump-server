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
    public class StartZaapDialog : IDialog
    {
        readonly List<Map> m_destinations = new List<Map>(){
                    World.Instance.GetMap(191105026), //Astrub
                    World.Instance.GetMap(191106050), //Shop Map
                    World.Instance.GetMap(131596288), //Enutropia
                    World.Instance.GetMap(161351684), //Ecafliperama
                    World.Instance.GetMap(145100034), //Xelorium
                    World.Instance.GetMap(134351108), //Sramvil
                    World.Instance.GetMap(165153537), //Mapa Eventos
        };

        public StartZaapDialog(Character character, InteractiveObject zaap)
        {
            Character = character;
            Zaap = zaap;
        }

        public StartZaapDialog(Character character)
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
            //if (Character.Account.IsSubscribe == false && Character.WorldAccount.LastDungeonDate != null && Character.WorldAccount.LastDungeonDate.Date == DateTime.Now.Date)
            //{
            //    Character.OpenPopup("Você não pode usar o teletransporte novamente hoje");
            //    return;
            //}

            //if (Character.Map.IsDungeon())
            //{
            //    Character.OpenPopup("Você não pode usar o teletransporte dentro de uma Dungeon.");
            //    return;
            //}

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