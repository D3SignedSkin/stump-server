using System;
using System.Collections.Generic;
using System.Linq;
using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Database;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Database.World.Triggers;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Items;
using Stump.Server.WorldServer.Game.Items.Player;

namespace Stump.Server.WorldServer.Game.Maps.Cells.Triggers
{
    [Discriminator("Teleport", typeof(CellTrigger), typeof(CellTriggerRecord))]
    public class TeleportTrigger : CellTrigger
    {
        public TeleportTrigger(CellTriggerRecord record)
            : base(record)
        {
        }

        private short? m_destinationCellId;
        private uint? m_destinationMapId;
        private ObjectPosition m_destinationPosition;
        private bool m_mustRefreshDestinationPosition;

        /// <summary>
        /// Parameter 0
        /// </summary>
        public short DestinationCellId
        {
            get
            {
                return m_destinationCellId ?? ( m_destinationCellId = Record.GetParameter<short>(0) ).Value;
            }
            set
            {
                Record.SetParameter(0, value);
                m_destinationCellId = value;
                m_mustRefreshDestinationPosition = true;
            }
        }

        /// <summary>
        /// Parameter 1
        /// </summary>
        public uint DestinationMapId
        {
            get
            {
                return m_destinationMapId ?? ( m_destinationMapId = Record.GetParameter<uint>(1) ).Value;
            }
            set
            {
                m_destinationMapId = value;
                m_mustRefreshDestinationPosition = true;
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

        private void RefreshPosition()
        {
            Map map = World.Instance.GetMap(DestinationMapId);

            if (map == null)
                throw new Exception(string.Format("Cannot load CellTeleport id={0}, DestinationMapId {1} isn't found", Record.Id, DestinationMapId));

            Cell cell = map.Cells[DestinationCellId];

            m_destinationPosition = new ObjectPosition(map, cell, DirectionsEnum.DIRECTION_EAST);
        }

        public ObjectPosition GetDestinationPosition()
        {
            if (m_destinationPosition == null || m_mustRefreshDestinationPosition)
                RefreshPosition();

            m_mustRefreshDestinationPosition = false;

            return m_destinationPosition;
        }

        public override void Apply(Character character)
        {
            if (!Record.IsConditionFilled(character))
            {
                character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 1); //Certaines conditions ne sont pas satisfaites
                return;
            }

            if (ItemsParameter != null)
            {
                var parameter = ItemsParameter.Split(',');
                var itemsToDelete = new Dictionary<BasePlayerItem, int>();

                foreach (var itemParameter in parameter.Select(x => x.Split('_')))
                {

                    int itemId;
                    if (!int.TryParse(itemParameter[0], out itemId))
                        return;
                    int amount;
                    if (!int.TryParse(itemParameter[1], out amount))
                        return;

                    var template = ItemManager.Instance.TryGetTemplate(itemId);
                    if (template == null)
                        return;

                    var item = character.Inventory.TryGetItem(template);

                    if (item == null)
                    {
                        //Vous ne possédez pas l'objet nécessaire.
                        character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 4);
                        return;
                    }

                    if (item.Stack < amount)
                    {
                        //Vous ne possédez pas l'objet en quantité suffisante.
                        character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 252);
                        return;
                    }

                    itemsToDelete.Add(item, amount);
                }

                foreach (var itemToDelete in itemsToDelete)
                {
                    character.Inventory.RemoveItem(itemToDelete.Key, itemToDelete.Value);
                    character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 22, itemToDelete.Value, itemToDelete.Key.Template.Id);
                }
            }

            var destination = GetDestinationPosition();
            character.Teleport(destination.Map, destination.Cell);
        }
    }
}