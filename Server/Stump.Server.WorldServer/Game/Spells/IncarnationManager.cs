using NLog;
using Stump.Core.Extensions;
using Stump.Core.IO;
//BISMILLAH
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Types;
using Stump.Server.WorldServer.Handlers.Context;
using Stump.Server.BaseServer.Database;
using Stump.Server.WorldServer.Database.Items;
using Stump.Server.WorldServer.Core.Network;
using Stump.DofusProtocol.Messages;
using Stump.Server.WorldServer.Database.Items.Shops;
using Stump.Server.WorldServer.Database.Items.Templates;
using Stump.Server.BaseServer.Initialization;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Items.Player;
using System.Collections.Generic;
using Stump.Server.WorldServer.Database.Spells;
using System.IO;
using System.Linq;
using Stump.Server.WorldServer.Database.Characters;
using Stump.Server.WorldServer.Game.Actors.Look;
using Stump.Server.WorldServer.Handlers.Inventory;
using Stump.Server.WorldServer.Handlers.Shortcuts;
using Stump.Server.WorldServer.Database.Npcs.Replies;
using Stump.Server.WorldServer.Game.Maps;
using Stump.Server.WorldServer.Game.Items;

namespace Stump.Server.WorldServer.Game.Spells
{
    // SAAD LE BG
    public class IncarnationManager : DataManager<IncarnationManager>
    {
        //private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private List<CustomIncarnationRecord> CustomIncarnationRecords = new List<CustomIncarnationRecord>();
        public List<Jesaispasquoimettre> handlers = new List<Jesaispasquoimettre>();

        [Initialization(InitializationPass.Fourth)]
        public override void Initialize()
        {
            var database = WorldServer.Instance.DBAccessor.Database;

            foreach (var record in database.Fetch<CustomIncarnationRecord>(CustomIncarnationRelator.FetchQuery).ToList())
            {
                CustomIncarnationRecords.Add(record);
            }
        }

        public CustomIncarnationRecord GetCustomIncarnationRecord(int Id)
        {
            return CustomIncarnationRecords.FirstOrDefault(x => x.Id == Id);
        }

        public CustomIncarnationRecord GetCustomIncarnationRecordByItem(int ItemId)
        {
            return CustomIncarnationRecords.FirstOrDefault(x => x.ItemId == ItemId);
        }

        public void ApplyCustomIncarnation(Character character, CustomIncarnationRecord record)
        {
            var look = character.Sex == SexTypeEnum.SEX_FEMALE ? record.FemaleCustomLookString : record.MaleCustomLookString;
            var charspells = record.Spells.Select(x => GetCharacterSpell(x, character.Id)).ToList();
            ItemTemplate template = null;

            if (record.ItemId != null && !record.AllowEquip)
            {
                template = ItemManager.Instance.TryGetTemplate(record.ItemId.Value);
            }

            if (template != null)
            {
                foreach (var item in character.Inventory.GetEquipedItems().Where(x => x.Template.ItemSetId != template.ItemSetId))
                {
                    character.Inventory.MoveItem(item, CharacterInventoryPositionEnum.INVENTORY_POSITION_NOT_EQUIPED);
                }
            }
            else if (!record.AllowEquip)
            {

                foreach (var item in character.Inventory.GetEquipedItems())
                {
                    character.Inventory.MoveItem(item, CharacterInventoryPositionEnum.INVENTORY_POSITION_NOT_EQUIPED);
                }
            }

            character.CustomLook = ActorLook.Parse(look);
            character.CustomLookActivated = true;
            //ApplyCustomStats(character, record);
            character.RefreshActor();
            character.IsInIncarnation = true;
            character.IncarnationId = record.Id;
            character.Spells.SetCustomSpells(charspells);
            InventoryHandler.SendSpellListMessage(character.Client, false);

            int index = 0;

            foreach (var spell in charspells)
            {
                character.Shortcuts.AddSpellShortcut(index, (short)spell.Id, true, false);
                index++;
            }

            ShortcutHandler.SendShortcutBarContentMessage(character.Client, ShortcutBarEnum.SPELL_SHORTCUT_BAR);
            character.SaveLater();
        }

        public void UnApplyCustomIncarnation(Character character)
        {
            character.CustomLookActivated = false;
            character.Spells.ResetCustomSpells();
            character.UpdateLook();
            character.RefreshActor();
            character.Shortcuts.ResetCustomSpellsShortcuts();
            character.IsInIncarnation = false;
            character.IncarnationId = 0;


            InventoryHandler.SendSpellListMessage(character.Client, true);
            ShortcutHandler.SendShortcutBarContentMessage(character.Client, ShortcutBarEnum.SPELL_SHORTCUT_BAR);
            foreach (var spell in character.Shortcuts.GetShortcuts(ShortcutBarEnum.SPELL_SHORTCUT_BAR))
            {
                if (spell is Database.Shortcuts.SpellShortcut)
                    if ((spell as Database.Shortcuts.SpellShortcut).SpellId != 0)
                    {
                        var variant = SpellManager.GetSpellVariant((ushort)(spell as Database.Shortcuts.SpellShortcut).SpellId);
                        if (variant == null)
                            return;
                        character.Client.Send(new SpellVariantActivationMessage(((ushort)(spell as Database.Shortcuts.SpellShortcut).SpellId), true));
                    }
            }

            UnApplyCustomStats(character);
            character.SaveLater();
        }

        public void ConnectWithCustomIncarnation(Character character, int incarnId)
        {
            var record = GetCustomIncarnationRecord(incarnId);
            var charspells = record.Spells.Select(x => GetCharacterSpell(x, character.Id)).ToList();

            //ApplyCustomStats(character, record);
            character.Spells.SetCustomSpells(charspells);
            InventoryHandler.SendSpellListMessage(character.Client, false);

            int index = 0;

            foreach (var spell in charspells)
            {
                character.Shortcuts.AddSpellShortcut(index, (short)spell.Id, true, false);
                index++;
            }

            ShortcutHandler.SendShortcutBarContentMessage(character.Client, ShortcutBarEnum.SPELL_SHORTCUT_BAR);
            character.SaveLater();
        }

        public void ApplyCustomStats(Character character, CustomIncarnationRecord record)
        {
            var char_record = UpdateRecord(character);

            character.CustomStatsActivated = true;
            character.Stats = new Actors.Stats.StatsFields(character);
            character.Stats.Initialize(record, char_record);
            character.Inventory.ReloadEffectsItemsEquiped();
            character.RefreshStats();

        }

        public void UnApplyCustomStats(Character character)
        {
            character.ResetStats();
            character.CustomStatsActivated = false;
            character.Stats = new Actors.Stats.StatsFields(character);
            character.Stats.Initialize(character.Record);
            character.Inventory.ReloadEffectsItemsEquiped();
            character.RefreshStats();
        }

        internal CharacterRecord UpdateRecord(Character character )
        {
            character.Record.AP = character.Stats[PlayerFields.AP].Base;
            character.Record.MP = character.Stats[PlayerFields.MP].Base;
            character.Record.Strength = character.Stats[PlayerFields.Strength].Base;
            character.Record.Agility = character.Stats[PlayerFields.Agility].Base;
            character.Record.Chance = character.Stats[PlayerFields.Chance].Base;
            character.Record.Intelligence = character.Stats[PlayerFields.Intelligence].Base;
            character.Record.Wisdom = character.Stats[PlayerFields.Wisdom].Base;
            character.Record.Vitality = character.Stats[PlayerFields.Vitality].Base;

            character.Record.PermanentAddedStrength = (short)character.Stats[PlayerFields.Strength].Additional;
            character.Record.PermanentAddedAgility = (short)character.Stats[PlayerFields.Agility].Additional;
            character.Record.PermanentAddedChance = (short)character.Stats[PlayerFields.Chance].Additional;
            character.Record.PermanentAddedIntelligence = (short)character.Stats[PlayerFields.Intelligence].Additional;
            character.Record.PermanentAddedWisdom = (short)character.Stats[PlayerFields.Wisdom].Additional;
            character.Record.PermanentAddedVitality = (short)character.Stats[PlayerFields.Vitality].Additional;

            return character.Record;
        }

        public CharacterSpell GetCharacterSpell(Spell spell, int ChracterId)
        {
            CharacterSpellRecord record = new CharacterSpellRecord()
            {
                Level = 1,
                SpellId = spell.Id,
                OwnerId = ChracterId
            };

            return new CharacterSpell(record);
        }

        public void CheckArea(Character character, Map map)
        {
            #region Aparenta está desativado
            var handlr = handlers.FirstOrDefault(x => x.chracter == character.Id);
            if (handlr == null)
            {
                IncarnationManager.Instance.UnApplyCustomIncarnation(character);
                character.Teleport(character.Breed.GetStartPosition());
                return;
            }

            var areas = handlr.areas;
            if (!areas.Contains(map.SubArea))
            {
                IncarnationManager.Instance.UnApplyCustomIncarnation(character);
                IncarnationManager.Instance.handlers.Remove(handlr);
            }
            #endregion
        }
    }
}