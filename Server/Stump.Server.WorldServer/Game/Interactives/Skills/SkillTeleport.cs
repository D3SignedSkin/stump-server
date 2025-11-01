using System;
using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Database;
using Stump.Server.WorldServer.Database.Interactives;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Maps.Cells;
using System.Collections.Generic;
using Stump.Server.WorldServer.Game.Items.Player;
using System.Linq;
using Stump.Server.WorldServer.Game.Items;
using Stump.Server.WorldServer.Database.World.Triggers;
using Stump.Core.Reflection;

namespace Stump.Server.WorldServer.Game.Interactives.Skills
{
    [Discriminator("Teleport", typeof(Skill), typeof(int), typeof(InteractiveCustomSkillRecord), typeof(InteractiveObject))]
    public class SkillTeleport : CustomSkill
    {
        bool m_mustRefreshPosition;
        ObjectPosition m_position;

        public uint MapId => Record.GetParameter<uint>(0);

        public int CellId => Record.GetParameter<int>(1);

        public DirectionsEnum Direction => (DirectionsEnum)Record.GetParameter<int>(2, true);

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

        public string AdditionalParameters
        {
            get
            {
                return Record.GetParameter<string>(5, true);
            }
            set
            {
                Record.SetParameter(5, value);
            }
        }

        Dictionary<PlayableBreedEnum, uint> teleportPositions = new Dictionary<PlayableBreedEnum, uint>() // Mapear os valores de PlayableBreedEnum para as posições de teleporte
        {
            { PlayableBreedEnum.Feca, 152045571 },
            { PlayableBreedEnum.Osamodas, 152043523 },
            { PlayableBreedEnum.Enutrof, 152046595 },
            { PlayableBreedEnum.Sram, 152046599 },
            { PlayableBreedEnum.Xelor, 152044551 },
            { PlayableBreedEnum.Ecaflip, 152044545 },
            { PlayableBreedEnum.Eniripsa, 152046593 },
            { PlayableBreedEnum.Iop, 152044547 },
            { PlayableBreedEnum.Cra, 152043521 },
            { PlayableBreedEnum.Sadida, 152046597 },
            { PlayableBreedEnum.Sacrieur, 152045573 },
            { PlayableBreedEnum.Pandawa, 152043525 },
            { PlayableBreedEnum.Roublard, 152044549 },
            { PlayableBreedEnum.Zobal, 152043527 },
            { PlayableBreedEnum.Steamer, 152045575 },
            { PlayableBreedEnum.Eliotrope, 152045569 },
            { PlayableBreedEnum.Huppermage, 152043529 },
            { PlayableBreedEnum.Ouginak, 152044553 }
        };

        public SkillTeleport(int id, InteractiveCustomSkillRecord record, InteractiveObject interactiveObject) : base(id, record, interactiveObject)
        {
            //record.CustomTemplateId = 339;
            var cellTrigger = new CellTriggerRecord { Type = "Teleport", CellId = interactiveObject.Cell.Id, MapId = interactiveObject.Map.Id, TriggerType = 0, Condition = record.Condition, Parameter0 = record.Parameter1, Parameter1 = record.Parameter0, Parameter3 = record.Parameter3 };
            var trigger = cellTrigger.GenerateTrigger();

            trigger.Position.Map.AddTrigger(trigger);
        }

        public override int StartExecute(Character character)
        {
            if (!Record.AreConditionsFilled(character))
            {
                character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 1);
                return -1;
            }

            if (ItemsParameter != null)
            {
                var parameter = ItemsParameter.Split(',');
                var itemsToDelete = new Dictionary<BasePlayerItem, int>();

                foreach (var itemParameter in parameter.Select(x => x.Split('_')))
                {

                    int itemId;
                    if (!int.TryParse(itemParameter[0], out itemId))
                        return -1;
                    int amount;
                    if (!int.TryParse(itemParameter[1], out amount))
                        return -1;

                    var template = ItemManager.Instance.TryGetTemplate(itemId);
                    if (template == null)
                        return -1;

                    var item = character.Inventory.TryGetItem(template);

                    if (item == null)
                    {
                        //Vous ne possédez pas l'objet nécessaire.
                        character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 4);
                        return -1;
                    }

                    if (item.Stack < amount)
                    {
                        //Vous ne possédez pas l'objet en quantité suffisante.
                        character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 252);
                        return -1;
                    }

                    itemsToDelete.Add(item, amount);
                }

                foreach (var itemToDelete in itemsToDelete)
                {
                    character.Inventory.RemoveItem(itemToDelete.Key, itemToDelete.Value);
                    character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 22, itemToDelete.Value, itemToDelete.Key.Template.Id);
                }
            }

            if (AdditionalParameters == "TemploCeleste")
            {
                if (teleportPositions.TryGetValue(character.BreedId, out uint mapId)) // Verificar se o BreedId existe no dicionário e obter a posição de teleporte correspondente
                {
                    character.Teleport(new ObjectPosition(Singleton<World>.Instance.GetMap(mapId), 289, DirectionsEnum.DIRECTION_SOUTH_WEST));
                }
                else
                {
                    character.Teleport(GetPosition());
                }
            }
            else
            {
                character.Teleport(GetPosition());
            }

            return base.StartExecute(character);
        }

        void RefreshPosition()
        {
            var map = World.Instance.GetMap(MapId);

            if (map == null)
                throw new Exception(string.Format("Cannot load SkillTeleport id={0}, map {1} isn't found", Id, MapId));

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
    }
}