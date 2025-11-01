using System;
using System.Collections.Generic;
using System.Linq;
using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Database;
using Stump.Server.WorldServer.Database.Items.Templates;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Npcs;
using Stump.Server.WorldServer.Game.Items;
using Stump.Server.WorldServer.Game.Maps.Cells;

namespace Stump.Server.WorldServer.Database.Npcs.Replies
{
    [Discriminator("TeleportCameOfDung", typeof(NpcReply), typeof(NpcReplyRecord))]
    public class TeleportCameOfDungReply : NpcReply
    {
        private bool m_mustRefreshPosition;
        private ObjectPosition m_position;

        public TeleportCameOfDungReply()
        {
            Record.Type = "Teleport";
        }

        public TeleportCameOfDungReply(NpcReplyRecord record) : base(record)
        { }

        public uint MapId
        {
            get
            {
                return Record.GetParameter<uint>(0);
            }
            set
            {
                Record.SetParameter(0, value);
                m_mustRefreshPosition = true;
            }
        }

        public int CellId
        {
            get
            {
                return Record.GetParameter<int>(1);
            }
            set
            {
                Record.SetParameter(1, value);
                m_mustRefreshPosition = true;
            }
        }

        public DirectionsEnum Direction
        {
            get
            {
                return (DirectionsEnum)Record.GetParameter<int>(2);
            }
            set
            {
                Record.SetParameter(2, (int)value);
                m_mustRefreshPosition = true;
            }
        }

        public string ItemsParameter
        {
            get
            {
                return Record.GetParameter<string>(3, true);
            }
            set
            {
                Record.SetParameter(3, value);
            }
        }

        public int KamasParameter
        {
            get
            {
                return Record.GetParameter<int>(4, true);
            }
            set
            {
                Record.SetParameter(4, value);
            }
        }

        private void RefreshPosition()
        {
            var map = Game.World.Instance.GetMap(MapId);

            if (map == null)
                throw new Exception(string.Format("Cannot load SkillTeleport id={0}, map {1} isn't found", Id, MapId));

            if (CellId == 0)
                CellId = map.GetRandomFreeCell().Id;

            var cell = map.Cells[CellId];

            m_position = new ObjectPosition(map, cell, Direction);
        }

        public ObjectPosition GetPosition()
        {
            if (m_position == null || m_mustRefreshPosition)
                RefreshPosition();

            m_mustRefreshPosition = false;

            return m_position;
        }

        public override bool CanShow(Npc npc, Character character) => base.CanShow(npc, character) && MapId != character.Map.Id;

        public override bool Execute(Npc npc, Character character)
        {
            if (!base.Execute(npc, character))
                return false;

            long[] _dungeonReturn = character.DungeonReturn.FirstOrDefault(dung => dung != null && dung.Contains(character.Map.Id));

            if (_dungeonReturn != null)
            {
                List<long[]> _newlistDungeons = character.DungeonReturn;

                _newlistDungeons.Remove(_dungeonReturn);

                character.DungeonReturn = _newlistDungeons;
            }

            if (string.IsNullOrEmpty(ItemsParameter) && KamasParameter == 0)
            {

                if (character.Record.MapBeforeDungeonId != 0)
                {
                    return character.Teleport((new ObjectPosition(Game.World.Instance.GetMap(character.Record.MapBeforeDungeonId), Game.World.Instance.GetMap(character.Record.MapBeforeDungeonId).GetCell(character.Record.CellBeforeDungeonId), character.Direction)));
                }
                else
                {
                    return character.Teleport(GetPosition());
                }
            }

            if (character.Kamas < KamasParameter)
            {
                character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 82); //Vous n'avez pas assez de kamas pour effectuer cette action.
                return false;
            }

            if (ItemsParameter != null)
            {
                var parameter = ItemsParameter.Split(',');
                var itemsToAdd = new Dictionary<ItemTemplate, int>();

                foreach (var itemParameter in parameter.Select(x => x.Split('_')))
                {

                    int itemId;
                    if (!int.TryParse(itemParameter[0], out itemId))
                        return false;
                    int amount;
                    if (!int.TryParse(itemParameter[1], out amount))
                        return false;

                    var template = ItemManager.Instance.TryGetTemplate(itemId);
                    if (template == null)
                        return false;

                    itemsToAdd.Add(template, amount);
                }

                foreach (var itemToAdd in itemsToAdd)
                {
                    character.Inventory.AddItem(itemToAdd.Key, itemToAdd.Value);
                }
            }

            character.Inventory.SubKamas(KamasParameter);

            if (character.Record.MapBeforeDungeonId != 0)
            {
                return character.Teleport((new ObjectPosition(Game.World.Instance.GetMap(character.Record.MapBeforeDungeonId), Game.World.Instance.GetMap(character.Record.MapBeforeDungeonId).GetCell(character.Record.CellBeforeDungeonId), character.Direction)));
            }
            else
            {
                return character.Teleport(GetPosition());
            }
        }
    }
}