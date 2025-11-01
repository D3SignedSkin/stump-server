using NLog;
using Stump.Core.IO;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Messages;
using Stump.DofusProtocol.Types;
using Stump.Server.BaseServer.Database;
using Stump.Server.BaseServer.Initialization;
using Stump.Server.WorldServer.Core.Network;
using Stump.Server.WorldServer.Database;
using Stump.Server.WorldServer.Database.Presets;
using Stump.Server.WorldServer.Database.Shortcuts;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Items.Player;
using Stump.Server.WorldServer.Game.Spells;
using Stump.Server.WorldServer.Handlers.Inventory;
using Stump.Server.WorldServer.Handlers.Shortcuts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Stump.Server.WorldServer.Game.Presets
{
    public class PresetsManager : DataManager<PresetsManager>, ISaveable
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        readonly object m_lock = new object();

        private List<CharacterPresetsRecord> PresetRecords = new List<CharacterPresetsRecord>();
        private List<IdolsPresetRecord> PresetIdolsRecords = new List<IdolsPresetRecord>();
        private List<SpellsPresetsRecord> SpellsPresetRecords = new List<SpellsPresetsRecord>();
        private List<StatsPresetsRecord> StatsPresetRecords = new List<StatsPresetsRecord>();
        private List<ItemsPresetRecord> ItemsPresetRecords = new List<ItemsPresetRecord>();

        private List<CharacterPresetsRecord> newPresetRecords = new List<CharacterPresetsRecord>();
        private List<SpellsPresetsRecord> newSpellsPresetRecords = new List<SpellsPresetsRecord>();
        private List<StatsPresetsRecord> newStatsPresetRecords = new List<StatsPresetsRecord>();
        private List<ItemsPresetRecord> newItemsPresetRecords = new List<ItemsPresetRecord>();
        private List<IdolsPresetRecord> newIdolsPresetRecords = new List<IdolsPresetRecord>();

        private List<CharacterPresetsRecord> deletePresetRecords = new List<CharacterPresetsRecord>();
        private List<SpellsPresetsRecord> deleteSpellsPresetRecords = new List<SpellsPresetsRecord>();
        private List<StatsPresetsRecord> deleteStatsPresetRecords = new List<StatsPresetsRecord>();
        private List<ItemsPresetRecord> deleteItemsPresetRecords = new List<ItemsPresetRecord>();
        private List<IdolsPresetRecord> deleteIdolsPresetRecords = new List<IdolsPresetRecord>();

        [Initialization(InitializationPass.Fourth)]
        public override void Initialize()
        {
            foreach (var record in Database.Fetch<CharacterPresetsRecord>(CharacterPresetsRelator.FetchQuery).ToList())
            {
                PresetRecords.Add(record);
            }
            foreach (var record in Database.Fetch<IdolsPresetRecord>(IdolsPresetsRelator.FetchQuery).ToList())
            {
                PresetIdolsRecords.Add(record);
            }
            foreach (var record in Database.Fetch<SpellsPresetsRecord>(SpellsPresetsRelator.FetchQuery).ToList())
            {
                SpellsPresetRecords.Add(record);
            }
            foreach (var record in Database.Fetch<StatsPresetsRecord>(StatsPresetsRelator.FetchQuery).ToList())
            {
                StatsPresetRecords.Add(record);
            }
            foreach (var record in Database.Fetch<ItemsPresetRecord>(ItemsPresetRelator.FetchQuery).ToList())
            {
                ItemsPresetRecords.Add(record);
            }
            World.Instance.RegisterSaveableInstance(this);
        }

        #region GetPresetsRecord
        public SpellsPresetsRecord GetSpellsPresetRecord(CharacterPresetsRecord preset)
        {
            return SpellsPresetRecords.Where(x => x.PresetId == preset.PresetId && x.OwnerId == preset.OwnerId).FirstOrDefault();

        }

        public StatsPresetsRecord GetStatsPresetRecord(CharacterPresetsRecord preset)
        {
            return StatsPresetRecords.Where(x => x.PresetId == preset.PresetId && x.OwnerId == preset.OwnerId).FirstOrDefault();
        }

        public ItemsPresetRecord[] GetItemsPresetRecord(CharacterPresetsRecord preset)
        {
            return ItemsPresetRecords.Where(x => x.PresetId == preset.PresetId && x.OwnerId == preset.OwnerId).ToArray();
        }

        public IdolsPresetRecord[] GetIdolsPresetRecord(IdolsPresetRecord preset)
        {
            return PresetIdolsRecords.Where(x => x.PresetId == preset.PresetId && x.OwnerId == preset.OwnerId).ToArray();
        }

        public byte[] SerializeFullStatsPreset(FullStatsPreset preset)
        {
            return FormatterExtensions.ToBinary(preset);
        }

        public FullStatsPreset DeserializeFullStatsPreset(byte[] Serialized)
        {
            return FormatterExtensions.ToObject<FullStatsPreset>(Serialized);
        }
        #endregion

        public void Save()
        {
            lock (m_lock)
            {
                #region Presets

                if (newPresetRecords.Count > 0)
                {
                    foreach (var record in newPresetRecords)
                    {
                        Database.Insert(record);
                    }
                    newPresetRecords.Clear();
                }

                if (deletePresetRecords.Count > 0)
                {
                    foreach (var record in deletePresetRecords)
                    {
                        Database.Delete(record);
                    }
                    deletePresetRecords.Clear();
                }

                if (PresetRecords.Count > 0)
                    foreach (var record in PresetRecords)
                    {
                        Database.Save(record);
                    }

                #endregion

                #region Spells Presets

                if (newSpellsPresetRecords.Count > 0)
                {
                    foreach (var record in newSpellsPresetRecords)
                    {
                        Database.Insert(record);
                    }
                    newSpellsPresetRecords.Clear();
                }

                if (deleteSpellsPresetRecords.Count > 0)
                {
                    foreach (var record in deleteSpellsPresetRecords)
                    {
                        Database.Delete(record);
                    }
                    deleteSpellsPresetRecords.Clear();
                }

                if (SpellsPresetRecords.Count > 0)
                    foreach (var record in SpellsPresetRecords)
                    {
                        Database.Save(record);
                    }

                #endregion

                #region Stats Presets

                if (newStatsPresetRecords.Count > 0)
                {
                    foreach (var record in newStatsPresetRecords)
                    {
                        Database.Insert(record);
                    }
                    newStatsPresetRecords.Clear();
                }

                if (deleteStatsPresetRecords.Count > 0)
                {
                    foreach (var record in deleteStatsPresetRecords)
                    {
                        Database.Delete(record);
                    }
                    deleteStatsPresetRecords.Clear();
                }

                if (StatsPresetRecords.Count > 0)
                    foreach (var record in StatsPresetRecords)
                    {
                        Database.Save(record);
                    }

                #endregion

                #region Items Presets

                if (newItemsPresetRecords.Count > 0)
                {
                    foreach (var record in newItemsPresetRecords)
                    {
                        Database.Insert(record);
                    }
                    newItemsPresetRecords.Clear();
                }

                if (deleteItemsPresetRecords.Count > 0)
                {
                    foreach (var record in deleteItemsPresetRecords)
                    {
                        Database.Delete(record);
                    }
                    deleteItemsPresetRecords.Clear();
                }

                if (ItemsPresetRecords.Count > 0)
                    foreach (var record in ItemsPresetRecords)
                    {
                        Database.Save(record);
                    }

                #endregion

                #region Idols Presets

                if (newIdolsPresetRecords.Count > 0)
                {
                    foreach (var record in newIdolsPresetRecords)
                    {
                        Database.Insert(record);
                    }
                    newIdolsPresetRecords.Clear();
                }

                if (deleteIdolsPresetRecords.Count > 0)
                {
                    foreach (var record in deleteIdolsPresetRecords)
                    {
                        Database.Delete(record);
                    }
                    deleteIdolsPresetRecords.Clear();
                }

                if (PresetIdolsRecords.Count > 0)
                    foreach (var record in PresetIdolsRecords)
                    {
                        Database.Save(record);
                    }
                #endregion
            }
        }

        #region ApplyPresets
        public void ApplySpellsPreset(Character character, SpellForPreset preset)
        {
            foreach (var spellid in preset.shortcuts)
            {
                character.Spells.SpellVariantActivate(spellid, true);
            }

            character.Spells.SpellVariantsRefresh();
        }

        public ItemForPreset[] ApplyItemsPreset(Character character, ItemsPreset preset)
        {
            try
            {
                List<ItemForPreset> missingPresets = new List<ItemForPreset>();

                if (preset.items.Count() < 0)
                    return new ItemForPreset[0];

                foreach (var it in character.Inventory.GetEquipedItems())
                {
                    if (it.Position != CharacterInventoryPositionEnum.INVENTORY_POSITION_BOOST_FOOD && !preset.items.Any(x => x.objUid == it.Guid))
                    {
                        character.Inventory.MoveItem(it, CharacterInventoryPositionEnum.INVENTORY_POSITION_NOT_EQUIPED);
                    }
                }

                foreach (var pres in preset.items)
                {
                    var item = character.Inventory.GetItems().Where(x => x.GetObjectItem().objectGID == pres.objGid && x.GetObjectItem().objectUID == pres.objUid).FirstOrDefault();

                    if (item == null)
                    {
                        missingPresets.Add(pres);
                        continue;
                    }

                    var Activeitem = character.Inventory.GetEquipedItem(pres.objUid);

                    if (Activeitem != null)
                    {
                        character.Inventory.MoveItem(item, (CharacterInventoryPositionEnum)pres.position, true);
                    }
                }

                character.Inventory.CheckItemsCriterias();
                return missingPresets.ToArray();
            }

            catch
            {
                return new ItemForPreset[0];
            }
        }

        public void ApplyStatsPreset(Character character, FullStatsPreset preset)
        {
            character.Stats.Strength.Base = preset.stats.Where(x => x.keyword == "strength").FirstOrDefault().@base;
            character.Stats.Vitality.Base = preset.stats.Where(x => x.keyword == "vitality").FirstOrDefault().@base;
            character.Stats.Wisdom.Base = preset.stats.Where(x => x.keyword == "wisdom").FirstOrDefault().@base;
            character.Stats.Chance.Base = preset.stats.Where(x => x.keyword == "chance").FirstOrDefault().@base;
            character.Stats.Agility.Base = preset.stats.Where(x => x.keyword == "agility").FirstOrDefault().@base;
            character.Stats.Intelligence.Base = preset.stats.Where(x => x.keyword == "intelligence").FirstOrDefault().@base;
            character.StatsPoints = (ushort)preset.stats.Where(x => x.keyword == "statsPoints").FirstOrDefault().@base;

            character.RefreshStats();
        }
        #endregion

        #region DeletePreset
        public void DeletePreset(Character character, short PresetId, int OwnerId)
        {
            try
            {
                var preset = PresetRecords.Where(x => x.PresetId == PresetId && x.OwnerId == OwnerId).FirstOrDefault();
                var spellspreset = GetSpellsPresetRecord(preset);
                var statspreset = GetStatsPresetRecord(preset);
                var itemspreset = GetItemsPresetRecord(preset);
                try { character.Shortcuts.PresetShortcuts.Values.ToList().Where(x => x.PresetId == PresetId).ToList().ForEach(c => character.Shortcuts.RemoveShortcut(ShortcutBarEnum.GENERAL_SHORTCUT_BAR, c.Slot)); } catch { }

                deletePresetRecords.Add(preset);
                deleteSpellsPresetRecords.Add(spellspreset);
                deleteStatsPresetRecords.Add(statspreset);

                PresetRecords.Remove(preset);
                SpellsPresetRecords.Remove(spellspreset);
                StatsPresetRecords.Remove(statspreset);

                if (itemspreset.Count() > 0)
                {
                    List<ItemsPresetRecord> todelete = new List<ItemsPresetRecord>();
                    foreach (var item in itemspreset)
                    {
                        todelete.Add(item);
                    }
                    foreach (var item in todelete)
                    {
                        deleteItemsPresetRecords.Add(item);
                        ItemsPresetRecords.Remove(item);
                    }
                }

                InventoryHandler.PresetDeletedMessage(character.Client, PresetId, PresetDeleteResultEnum.PRESET_DEL_OK);
            }
            catch
            {
                InventoryHandler.PresetDeletedMessage(character.Client, PresetId, PresetDeleteResultEnum.PRESET_DEL_ERR_UNKNOWN);
            }

            character.SaveLater();
        }
        #endregion

        #region CheckPresetsLimit
        public int CheckLimit(Character character)
        {
            int PresetsOwner = PresetRecords.Where(x => x.OwnerId == character.Id).Count();
            int PresetsLimit = 0;

            if (character.UserGroup.Role <= RoleEnum.Player)
                PresetsLimit = 2;
            else if (character.UserGroup.Role == RoleEnum.Vip)
                PresetsLimit = 5;
            else if (character.UserGroup.Role >= RoleEnum.Gold_Vip)
                PresetsLimit = 18;
            else
                PresetsLimit = 2;

            if (character.SlotsArrangements > 0)
                PresetsLimit = (PresetsLimit + character.SlotsArrangements) > 18 ? 18 : (PresetsLimit + character.SlotsArrangements);

            return PresetsLimit;
        }

        public bool CheckPresetsLimit(Character character)
        {
            int PresetsOwner = PresetRecords.Where(x => x.OwnerId == character.Id).Count();
            int PresetsLimit = 0;

            if (character.UserGroup.Role <= RoleEnum.Player)
                PresetsLimit = 2;
            else if (character.UserGroup.Role == RoleEnum.Vip)
                PresetsLimit = 5;
            else if (character.UserGroup.Role >= RoleEnum.Gold_Vip)
                PresetsLimit = 18;
            else
                PresetsLimit = 2;

            if (character.SlotsArrangements > 0)
                PresetsLimit = (PresetsLimit + character.SlotsArrangements) > 18 ? 18 : (PresetsLimit + character.SlotsArrangements);

            if (character.SlotsArrangements == 0)
            {
                if (PresetsLimit == 2 && PresetsOwner == 2)
                {
                    character.SendServerMessageLangColor
                        (
                        "Você atingiu o limite máximo permitido para contas de Player. Você poderá aumentar seu limite <b>comprando poções na loja.",
                        "You have reached the maximum allowed limit for Player accounts. You can increase your limit <b>by buying potions in the shop.",
                        "Ha alcanzado el límite máximo permitido para las cuentas de jugador. Puedes aumentar tu límite <b>comprando pociones en la tienda.",
                        "Vous avez atteint la limite maximale autorisée pour les comptes Joueur. Vous pouvez augmenter votre limite <b>en achetant des potions dans la boutique.",
                        System.Drawing.Color.OrangeRed
                        );
                }
                if (PresetsLimit == 5 && PresetsOwner == 5)
                {
                    character.SendServerMessageLangColor
                        (
                        "Você atingiu o limite máximo permitido para contas de Vip. Você poderá aumentar seu limite <b>comprando poções na loja.",
                        "You have reached the maximum allowed limit for Vip accounts. You can increase your limit <b>by buying potions in the shop.",
                        "Ha alcanzado el límite máximo permitido para las cuentas de Vip. Puedes aumentar tu límite <b>comprando pociones en la tienda.",
                        "Vous avez atteint la limite maximale autorisée pour les comptes Vip. Vous pouvez augmenter votre limite <b>en achetant des potions dans la boutique.",
                        System.Drawing.Color.OrangeRed
                        );
                }
            }
            else if (character.SlotsArrangements > 0 && PresetsOwner == PresetsLimit)
            {
                character.SendServerMessageLangColor
                    (
                    "Você atingiu o limite máximo permitido para sua conta. Você poderá aumentar seu limite <b>comprando poções na loja.",
                    "You have reached the maximum limit allowed for your account. You can increase your limit <b>by buying potions in the shop.",
                    "Ha alcanzado el límite máximo permitido para su cuenta. Puedes aumentar tu límite <b>comprando pociones en la tienda.",
                    "Vous avez atteint la limite maximale autorisée pour votre compte. Vous pouvez augmenter votre limite <b>en achetant des potions dans la boutique.",
                    System.Drawing.Color.OrangeRed
                    );
            }

            return PresetsOwner < PresetsLimit;
        }
        #endregion

        #region UsePreset
        public void UsePreset(Character character, short PresetId, int OwnerId)
        {
            if (DateTime.Now < character.NextPresetTime)
            {
                InventoryHandler.PresetUsedMessage(character.Client, PresetId, PresetUseResultEnum.PRESET_USE_ERR_COOLDOWN);
                return;
            }

            if (character.IsInFight() && character.Fight.State == Fights.FightState.Fighting)
            {
                InventoryHandler.PresetUsedMessage(character.Client, PresetId, PresetUseResultEnum.PRESET_USE_ERR_UNKNOWN);
                return;
            }

            var preset = PresetRecords.Where(x => x.PresetId == PresetId && x.OwnerId == OwnerId).FirstOrDefault();

            if (preset == null)
            {
                InventoryHandler.PresetUsedMessage(character.Client, PresetId, PresetUseResultEnum.PRESET_USE_ERR_BAD_PRESET_ID);
                return;
            }

            var spellspreset = GetSpellsPresetRecord(preset);
            var statspreset = GetStatsPresetRecord(preset);
            var itemspreset = GetItemsPresetRecord(preset);

            ApplySpellsPreset(character, spellspreset.Spells);
            ApplyStatsPreset(character, statspreset.Stats);

            var vx = itemspreset.Select(x => x.Item).ToArray();
            var xv = GetItemsPreset((short)preset.ObjectId, vx, preset.MountEquiped, preset.Look);
            var missing = ApplyItemsPreset(character, xv);

            int UseCharTime = 0;

            if (character.UserGroup.Role <= RoleEnum.Player)
                UseCharTime = 60;
            else if (character.UserGroup.Role == RoleEnum.Vip)
                UseCharTime = 30;
            else if (character.UserGroup.Role >= RoleEnum.Gold_Vip)
                UseCharTime = 5;
            else
                UseCharTime = 5;

            character.NextPresetTime = DateTime.Now.AddSeconds(UseCharTime);

            if (missing.Count() > 0)
            {
                foreach (var item in missing) character.Client.Send(new ItemForPresetUpdateMessage(PresetId, item));
                InventoryHandler.PresetUsedMessage(character.Client, PresetId, PresetUseResultEnum.PRESET_USE_OK_PARTIAL);
                return;
            }

            InventoryHandler.PresetUsedMessage(character.Client, PresetId, PresetUseResultEnum.PRESET_USE_OK);
        }
        #endregion

        #region GetInformationsPresets
        public Preset GetPreset(short objectId, Preset[] presets, short IconId, string name)
        {
            return new IconNamedPreset(objectId, presets, IconId, name);
        }

        public ItemsPreset GetItemsPreset(short objectId, BasePlayerItem[] items, bool mountEquiped, EntityLook look)
        {
            List<ItemForPreset> ItemsForPreset = new List<ItemForPreset>();

            foreach (var item in items)
            {
                var ItemObject = item.GetObjectItem();
                ItemForPreset itemforpreset = new ItemForPreset(ItemObject.position, ItemObject.objectGID, ItemObject.objectUID);
                ItemsForPreset.Add(itemforpreset);
            }

            return new ItemsPreset(objectId, ItemsForPreset.ToArray(), mountEquiped, look);
        }

        public ItemsPreset GetItemsPreset(short objectId, ItemForPreset[] itemspresets, bool mountEquiped, EntityLook look)
        {
            return new ItemsPreset(objectId, itemspresets, mountEquiped, look);
        }

        public SpellForPreset GetSpellsPreset(uint objectId, short[] spellIds)
        {
            return new SpellForPreset((ushort)objectId, spellIds);
        }

        public SpellForPreset GetSpellsPreset(uint objectId, short[] spellIds, Character character)
        {
            List<short> SpellsList = new List<short>();

            foreach (var SpellsSelected in spellIds)
            {
                if (character.Spells.CanPlaySpell(SpellsSelected))
                {
                    SpellsList.Add(SpellsSelected);
                }
            }

            SpellForPreset SpellsActivo = new SpellForPreset((ushort)objectId, SpellsList.ToArray());

            return SpellsActivo;
        }

        public FullStatsPreset GetStatsPreset(short objectId, Character character)
        {
            var lifePoints = new CharacterCharacteristicForPreset("lifePoints", (short)character.LifePoints, 0, 0);
            var actionPoints = new CharacterCharacteristicForPreset("actionPoints", (short)character.Stats[PlayerFields.AP].Base, (short)character.Stats[PlayerFields.AP].Additional, (short)character.Stats[PlayerFields.AP].Equiped);
            var statsPoints = new CharacterCharacteristicForPreset("statsPoints", (short)character.StatsPoints, 0, 0);
            var strength = new CharacterCharacteristicForPreset("strength", (short)character.Stats[PlayerFields.Strength].Base, (short)character.Stats[PlayerFields.Strength].Additional, (short)character.Stats[PlayerFields.Strength].Equiped);
            var vitality = new CharacterCharacteristicForPreset("vitality", (short)character.Stats[PlayerFields.Vitality].Base, (short)character.Stats[PlayerFields.Vitality].Additional, (short)character.Stats[PlayerFields.Vitality].Equiped);
            var wisdom = new CharacterCharacteristicForPreset("wisdom", (short)character.Stats[PlayerFields.Wisdom].Base, (short)character.Stats[PlayerFields.Wisdom].Additional, (short)character.Stats[PlayerFields.Wisdom].Equiped);
            var chance = new CharacterCharacteristicForPreset("chance", (short)character.Stats[PlayerFields.Chance].Base, (short)character.Stats[PlayerFields.Chance].Additional, (short)character.Stats[PlayerFields.Chance].Equiped);
            var agility = new CharacterCharacteristicForPreset("agility", (short)character.Stats[PlayerFields.Agility].Base, (short)character.Stats[PlayerFields.Agility].Additional, (short)character.Stats[PlayerFields.Agility].Equiped);
            var intelligence = new CharacterCharacteristicForPreset("intelligence", (short)character.Stats[PlayerFields.Intelligence].Base, (short)character.Stats[PlayerFields.Intelligence].Additional, (short)character.Stats[PlayerFields.Intelligence].Equiped);
            var allDamagesBonus = new CharacterCharacteristicForPreset("allDamagesBonus", (short)character.Stats[PlayerFields.DamageBonus].Base, (short)character.Stats[PlayerFields.DamageBonus].Additional, (short)character.Stats[PlayerFields.DamageBonus].Equiped);
            var damagesBonusPercent = new CharacterCharacteristicForPreset("damagesBonusPercent", (short)character.Stats[PlayerFields.DamageBonusPercent].Base, (short)character.Stats[PlayerFields.DamageBonusPercent].Additional, (short)character.Stats[PlayerFields.DamageBonusPercent].Equiped);
            var healBonus = new CharacterCharacteristicForPreset("healBonus", (short)character.Stats[PlayerFields.HealBonus].Base, (short)character.Stats[PlayerFields.HealBonus].Additional, (short)character.Stats[PlayerFields.HealBonus].Equiped);
            var reflect = new CharacterCharacteristicForPreset("reflect", (short)character.Stats[PlayerFields.DamageReflection].Base, (short)character.Stats[PlayerFields.DamageReflection].Additional, (short)character.Stats[PlayerFields.DamageReflection].Equiped);
            var criticalHit = new CharacterCharacteristicForPreset("criticalHit", (short)character.Stats[PlayerFields.CriticalHit].Base, (short)character.Stats[PlayerFields.CriticalHit].Additional, (short)character.Stats[PlayerFields.CriticalHit].Equiped);
            var range = new CharacterCharacteristicForPreset("range", (short)character.Stats[PlayerFields.Range].Base, (short)character.Stats[PlayerFields.Range].Additional, (short)character.Stats[PlayerFields.Range].Equiped);
            var magicalReduction = new CharacterCharacteristicForPreset("magicalReduction", (short)character.Stats[PlayerFields.MagicDamageReduction].Base, (short)character.Stats[PlayerFields.MagicDamageReduction].Additional, (short)character.Stats[PlayerFields.MagicDamageReduction].Equiped);
            var physicalReduction = new CharacterCharacteristicForPreset("physicalReduction", (short)character.Stats[PlayerFields.PhysicalDamageReduction].Base, (short)character.Stats[PlayerFields.PhysicalDamageReduction].Additional, (short)character.Stats[PlayerFields.PhysicalDamageReduction].Equiped);
            var movementPoints = new CharacterCharacteristicForPreset("movementPoints", (short)character.Stats[PlayerFields.MP].Base, (short)character.Stats[PlayerFields.MP].Additional, (short)character.Stats[PlayerFields.MP].Equiped);
            var summonableCreaturesBoost = new CharacterCharacteristicForPreset("summonableCreaturesBoost", (short)character.Stats[PlayerFields.SummonLimit].Base, (short)character.Stats[PlayerFields.SummonLimit].Additional, (short)character.Stats[PlayerFields.SummonLimit].Equiped);
            var dodgePALostProbability = new CharacterCharacteristicForPreset("dodgePALostProbability", (short)character.Stats[PlayerFields.DodgeAPProbability].Base, (short)character.Stats[PlayerFields.DodgeAPProbability].Additional, (short)character.Stats[PlayerFields.DodgeAPProbability].Equiped);
            var dodgePMLostProbability = new CharacterCharacteristicForPreset("dodgePMLostProbability", (short)character.Stats[PlayerFields.DodgeMPProbability].Base, (short)character.Stats[PlayerFields.DodgeMPProbability].Additional, (short)character.Stats[PlayerFields.DodgeMPProbability].Equiped);
            var weaponDamagesBonusPercent = new CharacterCharacteristicForPreset("weaponDamagesBonusPercent", (short)character.Stats[PlayerFields.WeaponDamageDonePercent].Base, (short)character.Stats[PlayerFields.WeaponDamageDonePercent].Additional, (short)character.Stats[PlayerFields.WeaponDamageDonePercent].Equiped);
            var physicalDamages = new CharacterCharacteristicForPreset("physicalDamages", (short)character.Stats[PlayerFields.PhysicalDamage].Base, (short)character.Stats[PlayerFields.PhysicalDamage].Additional, (short)character.Stats[PlayerFields.PhysicalDamage].Equiped);
            var criticalMiss = new CharacterCharacteristicForPreset("criticalMiss", (short)character.Stats[PlayerFields.CriticalMiss].Base, (short)character.Stats[PlayerFields.CriticalMiss].Additional, (short)character.Stats[PlayerFields.CriticalMiss].Equiped);
            var initiative = new CharacterCharacteristicForPreset("initiative", (short)character.Stats[PlayerFields.Initiative].Base, (short)character.Stats[PlayerFields.Initiative].Additional, (short)character.Stats[PlayerFields.Initiative].Equiped);
            var prospecting = new CharacterCharacteristicForPreset("prospecting", (short)character.Stats[PlayerFields.Prospecting].Base, (short)character.Stats[PlayerFields.Prospecting].Additional, (short)character.Stats[PlayerFields.Prospecting].Equiped);
            var earthElementResistPercent = new CharacterCharacteristicForPreset("earthElementResistPercent", (short)character.Stats[PlayerFields.EarthResistPercent].Base, (short)character.Stats[PlayerFields.EarthResistPercent].Additional, (short)character.Stats[PlayerFields.EarthResistPercent].Equiped);
            var fireElementResistPercent = new CharacterCharacteristicForPreset("fireElementResistPercent", (short)character.Stats[PlayerFields.FireResistPercent].Base, (short)character.Stats[PlayerFields.FireResistPercent].Additional, (short)character.Stats[PlayerFields.FireResistPercent].Equiped);
            var waterElementResistPercent = new CharacterCharacteristicForPreset("waterElementResistPercent", (short)character.Stats[PlayerFields.WaterResistPercent].Base, (short)character.Stats[PlayerFields.WaterResistPercent].Additional, (short)character.Stats[PlayerFields.WaterResistPercent].Equiped);
            var airElementResistPercent = new CharacterCharacteristicForPreset("airElementResistPercent", (short)character.Stats[PlayerFields.AirResistPercent].Base, (short)character.Stats[PlayerFields.AirResistPercent].Additional, (short)character.Stats[PlayerFields.AirResistPercent].Equiped);
            var neutralElementResistPercent = new CharacterCharacteristicForPreset("neutralElementResistPercent", (short)character.Stats[PlayerFields.NeutralResistPercent].Base, (short)character.Stats[PlayerFields.NeutralResistPercent].Additional, (short)character.Stats[PlayerFields.NeutralResistPercent].Equiped);
            var earthElementReduction = new CharacterCharacteristicForPreset("earthElementReduction", (short)character.Stats[PlayerFields.EarthElementReduction].Base, (short)character.Stats[PlayerFields.EarthElementReduction].Additional, (short)character.Stats[PlayerFields.EarthElementReduction].Equiped);
            var fireElementReduction = new CharacterCharacteristicForPreset("fireElementReduction", (short)character.Stats[PlayerFields.FireElementReduction].Base, (short)character.Stats[PlayerFields.FireElementReduction].Additional, (short)character.Stats[PlayerFields.FireElementReduction].Equiped);
            var waterElementReduction = new CharacterCharacteristicForPreset("waterElementReduction", (short)character.Stats[PlayerFields.WaterElementReduction].Base, (short)character.Stats[PlayerFields.WaterElementReduction].Additional, (short)character.Stats[PlayerFields.WaterElementReduction].Equiped);
            var airElementReduction = new CharacterCharacteristicForPreset("airElementReduction", (short)character.Stats[PlayerFields.AirElementReduction].Base, (short)character.Stats[PlayerFields.AirElementReduction].Additional, (short)character.Stats[PlayerFields.AirElementReduction].Equiped);
            var neutralElementReduction = new CharacterCharacteristicForPreset("neutralElementReduction", (short)character.Stats[PlayerFields.NeutralElementReduction].Base, (short)character.Stats[PlayerFields.NeutralElementReduction].Additional, (short)character.Stats[PlayerFields.NeutralElementReduction].Equiped);
            var pvpEarthElementPercentResist = new CharacterCharacteristicForPreset("pvpEarthElementPercentResist", (short)character.Stats[PlayerFields.PvpEarthResistPercent].Base, (short)character.Stats[PlayerFields.PvpEarthResistPercent].Additional, (short)character.Stats[PlayerFields.PvpEarthResistPercent].Equiped);
            var pvpFireElementResistPercent = new CharacterCharacteristicForPreset("pvpFireElementResistPercent", (short)character.Stats[PlayerFields.PvpFireResistPercent].Base, (short)character.Stats[PlayerFields.PvpFireResistPercent].Additional, (short)character.Stats[PlayerFields.PvpFireResistPercent].Equiped);
            var pvpWaterElementPercentResist = new CharacterCharacteristicForPreset("pvpWaterElementPercentResist", (short)character.Stats[PlayerFields.PvpWaterResistPercent].Base, (short)character.Stats[PlayerFields.PvpWaterResistPercent].Additional, (short)character.Stats[PlayerFields.PvpWaterResistPercent].Equiped);
            var pvpAirElementPercentResist = new CharacterCharacteristicForPreset("pvpAirElementPercentResist", (short)character.Stats[PlayerFields.PvpAirResistPercent].Base, (short)character.Stats[PlayerFields.PvpAirResistPercent].Additional, (short)character.Stats[PlayerFields.PvpAirResistPercent].Equiped);
            var pvpNeutralElementPercentResist = new CharacterCharacteristicForPreset("pvpNeutralElementPercentResist", (short)character.Stats[PlayerFields.PvpNeutralResistPercent].Base, (short)character.Stats[PlayerFields.PvpNeutralResistPercent].Additional, (short)character.Stats[PlayerFields.PvpNeutralResistPercent].Equiped);
            var pvpEarthElementReduction = new CharacterCharacteristicForPreset("pvpEarthElementReduction", (short)character.Stats[PlayerFields.PvpEarthElementReduction].Base, (short)character.Stats[PlayerFields.PvpEarthElementReduction].Additional, (short)character.Stats[PlayerFields.PvpEarthElementReduction].Equiped);
            var pvpFireElementReduction = new CharacterCharacteristicForPreset("pvpFireElementReduction", (short)character.Stats[PlayerFields.PvpFireElementReduction].Base, (short)character.Stats[PlayerFields.PvpFireElementReduction].Additional, (short)character.Stats[PlayerFields.PvpFireElementReduction].Equiped);
            var pvpWaterElementReduction = new CharacterCharacteristicForPreset("pvpWaterElementReduction", (short)character.Stats[PlayerFields.PvpWaterElementReduction].Base, (short)character.Stats[PlayerFields.PvpWaterElementReduction].Additional, (short)character.Stats[PlayerFields.PvpWaterElementReduction].Equiped);
            var pvpAirElementReduction = new CharacterCharacteristicForPreset("pvpAirElementReduction", (short)character.Stats[PlayerFields.PvpAirElementReduction].Base, (short)character.Stats[PlayerFields.PvpAirElementReduction].Additional, (short)character.Stats[PlayerFields.PvpAirElementReduction].Equiped);
            var pvpNeutralElementReduction = new CharacterCharacteristicForPreset("pvpNeutralElementReduction", (short)character.Stats[PlayerFields.PvpNeutralElementReduction].Base, (short)character.Stats[PlayerFields.PvpNeutralElementReduction].Additional, (short)character.Stats[PlayerFields.PvpNeutralElementReduction].Equiped);
            var trapBonusPercent = new CharacterCharacteristicForPreset("trapBonusPercent", (short)character.Stats[PlayerFields.TrapBonusPercent].Base, (short)character.Stats[PlayerFields.TrapBonusPercent].Additional, (short)character.Stats[PlayerFields.TrapBonusPercent].Equiped);
            var trapBonus = new CharacterCharacteristicForPreset("trapBonus", (short)character.Stats[PlayerFields.TrapBonus].Base, (short)character.Stats[PlayerFields.TrapBonus].Additional, (short)character.Stats[PlayerFields.TrapBonus].Equiped);
            var permanentDamagePercent = new CharacterCharacteristicForPreset("permanentDamagePercent", (short)character.Stats[PlayerFields.PermanentDamagePercent].Base, (short)character.Stats[PlayerFields.PermanentDamagePercent].Additional, (short)character.Stats[PlayerFields.PermanentDamagePercent].Equiped);
            var tackleEvade = new CharacterCharacteristicForPreset("tackleEvade", (short)character.Stats[PlayerFields.TackleEvade].Base, (short)character.Stats[PlayerFields.TackleEvade].Additional, (short)character.Stats[PlayerFields.TackleEvade].Equiped);
            var tackleBlock = new CharacterCharacteristicForPreset("tackleBlock", (short)character.Stats[PlayerFields.TackleBlock].Base, (short)character.Stats[PlayerFields.TackleBlock].Additional, (short)character.Stats[PlayerFields.TackleBlock].Equiped);
            var PAAttack = new CharacterCharacteristicForPreset("PAAttack", (short)character.Stats[PlayerFields.APAttack].Base, (short)character.Stats[PlayerFields.APAttack].Additional, (short)character.Stats[PlayerFields.APAttack].Equiped);
            var PMAttack = new CharacterCharacteristicForPreset("PMAttack", (short)character.Stats[PlayerFields.MPAttack].Base, (short)character.Stats[PlayerFields.MPAttack].Additional, (short)character.Stats[PlayerFields.MPAttack].Equiped);
            var pushDamageBonus = new CharacterCharacteristicForPreset("pushDamageBonus", (short)character.Stats[PlayerFields.PushDamageBonus].Base, (short)character.Stats[PlayerFields.PushDamageBonus].Additional, (short)character.Stats[PlayerFields.PushDamageBonus].Equiped);
            var pushDamageReduction = new CharacterCharacteristicForPreset("pushDamageReduction", (short)character.Stats[PlayerFields.PushDamageReduction].Base, (short)character.Stats[PlayerFields.PushDamageReduction].Additional, (short)character.Stats[PlayerFields.PushDamageReduction].Equiped);
            var criticalDamageBonus = new CharacterCharacteristicForPreset("criticalDamageBonus", (short)character.Stats[PlayerFields.CriticalDamageBonus].Base, (short)character.Stats[PlayerFields.CriticalDamageBonus].Additional, (short)character.Stats[PlayerFields.CriticalDamageBonus].Equiped);
            var criticalDamageReduction = new CharacterCharacteristicForPreset("criticalDamageReduction", (short)character.Stats[PlayerFields.CriticalDamageReduction].Base, (short)character.Stats[PlayerFields.CriticalDamageReduction].Additional, (short)character.Stats[PlayerFields.CriticalDamageReduction].Equiped);
            var earthDamageBonus = new CharacterCharacteristicForPreset("earthDamageBonus", (short)character.Stats[PlayerFields.EarthDamageBonus].Base, (short)character.Stats[PlayerFields.EarthDamageBonus].Additional, (short)character.Stats[PlayerFields.EarthDamageBonus].Equiped);
            var fireDamageBonus = new CharacterCharacteristicForPreset("fireDamageBonus", (short)character.Stats[PlayerFields.FireDamageBonus].Base, (short)character.Stats[PlayerFields.FireDamageBonus].Additional, (short)character.Stats[PlayerFields.FireDamageBonus].Equiped);
            var waterDamageBonus = new CharacterCharacteristicForPreset("waterDamageBonus", (short)character.Stats[PlayerFields.WaterDamageBonus].Base, (short)character.Stats[PlayerFields.WaterDamageBonus].Additional, (short)character.Stats[PlayerFields.WaterDamageBonus].Equiped);
            var airDamageBonus = new CharacterCharacteristicForPreset("airDamageBonus", (short)character.Stats[PlayerFields.AirDamageBonus].Base, (short)character.Stats[PlayerFields.AirDamageBonus].Additional, (short)character.Stats[PlayerFields.AirDamageBonus].Equiped);
            var neutralDamageBonus = new CharacterCharacteristicForPreset("neutralDamageBonus", (short)character.Stats[PlayerFields.NeutralDamageBonus].Base, (short)character.Stats[PlayerFields.NeutralDamageBonus].Additional, (short)character.Stats[PlayerFields.NeutralDamageBonus].Equiped);
            var maxLifePoints = new CharacterCharacteristicForPreset("maxLifePoints", (short)character.MaxLifePoints, 0, 0);
            var spellPercentDamages = new CharacterCharacteristicForPreset("spellPercentDamages", 0, 0, 0);
            var percentResist = new CharacterCharacteristicForPreset("percentResist", 0, 0, 0);
            var meleeDamageDonePercent = new CharacterCharacteristicForPreset("meleeDamageDonePercent", (short)character.Stats[PlayerFields.MeleeDamageDonePercent].Base, (short)character.Stats[PlayerFields.MeleeDamageDonePercent].Additional, (short)character.Stats[PlayerFields.MeleeDamageDonePercent].Equiped);
            var meleeDamageReceivedPercent = new CharacterCharacteristicForPreset("meleeDamageReceivedPercent", (short)character.Stats[PlayerFields.MeleeDamageReceivedPercent].Base, (short)character.Stats[PlayerFields.MeleeDamageReceivedPercent].Additional, (short)character.Stats[PlayerFields.MeleeDamageReceivedPercent].Equiped);
            var rangedDamageDonePercent = new CharacterCharacteristicForPreset("rangedDamageDonePercent", (short)character.Stats[PlayerFields.RangedDamageDonePercent].Base, (short)character.Stats[PlayerFields.RangedDamageDonePercent].Additional, (short)character.Stats[PlayerFields.RangedDamageDonePercent].Equiped);
            var rangedDamageReceivedPercent = new CharacterCharacteristicForPreset("rangedDamageReceivedPercent", (short)character.Stats[PlayerFields.RangedDamageReceivedPercent].Base, (short)character.Stats[PlayerFields.RangedDamageReceivedPercent].Additional, (short)character.Stats[PlayerFields.RangedDamageReceivedPercent].Equiped);
            var weaponDamageDonePercent = new CharacterCharacteristicForPreset("weaponDamageDonePercent", (short)character.Stats[PlayerFields.WeaponDamageDonePercent].Base, (short)character.Stats[PlayerFields.WeaponDamageDonePercent].Additional, (short)character.Stats[PlayerFields.WeaponDamageDonePercent].Equiped);
            var weaponDamageReceivedPercent = new CharacterCharacteristicForPreset("weaponDamageReceivedPercent", (short)character.Stats[PlayerFields.WeaponDamageReceivedPercent].Base, (short)character.Stats[PlayerFields.WeaponDamageReceivedPercent].Additional, (short)character.Stats[PlayerFields.WeaponDamageReceivedPercent].Equiped);
            var spellDamageDonePercent = new CharacterCharacteristicForPreset("spellDamageDonePercent", (short)character.Stats[PlayerFields.SpellDamageDonePercent].Base, (short)character.Stats[PlayerFields.SpellDamageDonePercent].Additional, (short)character.Stats[PlayerFields.SpellDamageDonePercent].Equiped);
            var spellDamageReceivedPercent = new CharacterCharacteristicForPreset("spellDamageReceivedPercent", (short)character.Stats[PlayerFields.SpellDamageReceivedPercent].Base, (short)character.Stats[PlayerFields.SpellDamageReceivedPercent].Additional, (short)character.Stats[PlayerFields.SpellDamageReceivedPercent].Equiped);

            CharacterCharacteristicForPreset[] stats = { lifePoints, actionPoints, statsPoints, strength, vitality, wisdom, chance, agility, intelligence, allDamagesBonus, damagesBonusPercent, healBonus, reflect, criticalHit, range, magicalReduction, physicalReduction, movementPoints, summonableCreaturesBoost, dodgePALostProbability, dodgePMLostProbability, weaponDamagesBonusPercent, physicalDamages, criticalMiss, initiative, prospecting, earthElementResistPercent, fireElementResistPercent, waterElementResistPercent, airElementResistPercent, neutralElementResistPercent, earthElementReduction, fireElementReduction, waterElementReduction, airElementReduction, neutralElementReduction, pvpEarthElementPercentResist, pvpFireElementResistPercent, pvpWaterElementPercentResist, pvpAirElementPercentResist, pvpNeutralElementPercentResist, pvpEarthElementReduction, pvpFireElementReduction, pvpWaterElementReduction, pvpAirElementReduction, pvpNeutralElementReduction, trapBonusPercent, trapBonus, permanentDamagePercent, tackleEvade, tackleBlock, PAAttack, PMAttack, pushDamageBonus, pushDamageReduction, criticalDamageBonus, criticalDamageReduction, earthDamageBonus, fireDamageBonus, waterDamageBonus, airDamageBonus, neutralDamageBonus, maxLifePoints, spellPercentDamages, percentResist, meleeDamageDonePercent, meleeDamageReceivedPercent, rangedDamageDonePercent, rangedDamageReceivedPercent, weaponDamageDonePercent, weaponDamageReceivedPercent, spellDamageDonePercent, spellDamageReceivedPercent };


            return new FullStatsPreset(objectId, stats);
        }

        public Preset GetPresets(CharacterPresetsRecord preset)
        {
            var Spells = GetSpellsPresetRecord(preset);
            var Stats = GetStatsPresetRecord(preset).Stats;
            var Items = GetItemsPreset((short)preset.ObjectId, GetItemsPresetRecord(preset).Select(x => x.Item).ToArray(), preset.MountEquiped, preset.Look);
            var presetContainer = new PresetsContainerPreset((short)preset.ObjectId, new Preset[] { Stats, Items });

            return GetPreset((short)preset.ObjectId, new Preset[] { Stats, presetContainer }, (short)preset.SymbolId, preset.Name);
        }

        public Preset[] GetPresetsFromDatabase(Character owner)
        {
            List<Preset> Press = new List<Preset>();
            var presets = PresetRecords.Where(x => x.OwnerId == owner.Client.Character.Id);

            if (presets.Count() < 1)
                return Press.ToArray();

            foreach (var preset in presets)
            {
                var Spells = GetSpellsPresetRecord(preset).Spells;
                var Stats = GetStatsPresetRecord(preset).Stats;
                var Items = GetItemsPreset((short)preset.ObjectId, GetItemsPresetRecord(preset).Select(x => x.Item).ToArray(), preset.MountEquiped, preset.Look);
                var presetContainer = new PresetsContainerPreset((short)preset.ObjectId, new Preset[] { Stats, Items });
                //var Items
                Preset pres = GetPreset((short)preset.ObjectId, new Preset[] { Stats, presetContainer }, (short)preset.SymbolId, preset.Name);
                Press.Add(pres);
            }

            return Press.ToArray();
        }

        public int GetNewObjectId(Character character)
        {
            int result = 1;

            if (PresetRecords.Where(x => x.OwnerId == character.Id).Count() > 0) result = PresetRecords.Where(x => x.OwnerId == character.Id).OrderByDescending(x => x.ObjectId).FirstOrDefault().ObjectId + 1;
            return result;
        }

        public short GetNewIdolsObjectId(Character character)
        {
            short result = 1;

            if (PresetIdolsRecords.Where(x => x.OwnerId == character.Id).Count() > 0) result = (short)(PresetIdolsRecords.Where(x => x.OwnerId == character.Id).OrderByDescending(x => x.objectId).FirstOrDefault().objectId + 1);
            return result;
        }
        #endregion

        #region Save
        public void SendSavePresetResult(Character character, int ObjectId, CharacterPresetsRecord preset)
        {
            int CheckLimite = CheckLimit(character);
            int PresetsCharCount = PresetRecords.Where(x => x.OwnerId == character.Id).Count();
            int PresetsCount = CheckLimite > PresetsCharCount ? CheckLimite - PresetsCharCount : 0;
            var client = character.Client;
            var Preset = GetPresets(preset);
            var Presets = GetPresetsFromDatabase(character);
            InventoryHandler.SendPresetSavedMessage(client, (short)ObjectId, Preset, Presets);

            character.OpenPopupLang
                (
                "Limites de Arranjos \n Players: 2 \n VIPs: 5 \n VIPs Gold: 18 \n\n Você poderá também comprar a poção de novos arranjos definitivos em nosso mapa shop. Atualmente você possui " + PresetsCount + " novos arranjos disponiveis.",
                "Arrangement Limits \n Players: 2 \n VIPs: 5 \n Gold VIPs: 18 \n\n You can also buy the potion of new definitive arrangements in our map shop. Currently you have " + PresetsCount + " new arrangements available.",
                "Límites de arreglos \n Jugadores: 2 \n VIP: 5 \n VIP dorados: 18 \n\n También puedes comprar la poción de nuevos arreglos definitivos en nuestra tienda de mapas. Actualmente tienes " + PresetsCount + " nuevos arreglos disponibles.",
                "Limite d'arrangement \n Joueurs : 2 \n VIP : 5 \n VIP Or : 18 \n\n Vous pouvez également acheter la potion de nouveaux arrangements définitifs dans notre boutique de cartes. Actuellement, vous avez " + PresetsCount + " de nouveaux arrangements disponibles.",
                "Server",
                0
                );
        }

        public void SaveIdolsPreset(Character character, short PresetId, sbyte SymbolId, bool UpdateData)
        {
            int OwnerId = character.Id;
            short ObjectId = PresetId == -1 ? GetNewIdolsObjectId(character) : PresetId;

            if (PresetId == -1)
            {
                IdolsPresetRecord preset = new IdolsPresetRecord()
                {
                    OwnerId = OwnerId,
                    SymbolId = SymbolId,
                    PresetId = ObjectId,
                };

                newIdolsPresetRecords.Add(preset);
                PresetIdolsRecords.Add(preset);
            }
        }

        public void SavePreset(Character character, short PresetId, sbyte SymbolId, string Name, bool UpdateData, sbyte Type)
        {
            int OwnerId = character.Id;
            int ObjectId = PresetId == -1 ? GetNewObjectId(character) : PresetId;
            var spells = character.Spells.GetSpells().Where(x => x.Record.OwnerId == character.Id);
            List<short> spellids = new List<short>();

            foreach (var spell in spells)
            {
                spellids.Add((short)spell.Record.SpellId);
            }

            if (PresetId == -1)
            {
                if (!CheckPresetsLimit(character))
                    return;

                CharacterPresetsRecord preset = new CharacterPresetsRecord()
                {
                    OwnerId = OwnerId,
                    SymbolId = SymbolId,
                    PresetId = ObjectId,
                    ObjectId = ObjectId,
                    Name = Name,
                    MountEquiped = character.EquippedMount != null,
                    CharacterLook = character.Look.ToString()
                };

                SpellsPresetsRecord spellspreset = new SpellsPresetsRecord()
                {
                    OwnerId = OwnerId,
                    PresetId = ObjectId,
                    Spells = GetSpellsPreset((uint)ObjectId, spellids.ToArray(), character),
                };

                StatsPresetsRecord statspreset = new StatsPresetsRecord()
                {
                    OwnerId = OwnerId,
                    PresetId = ObjectId,
                    objectId = (short)ObjectId,
                    Stats = GetStatsPreset((short)ObjectId, character)
                };

                newPresetRecords.Add(preset);
                newSpellsPresetRecords.Add(spellspreset);
                newStatsPresetRecords.Add(statspreset);

                PresetRecords.Add(preset);
                SpellsPresetRecords.Add(spellspreset);
                StatsPresetRecords.Add(statspreset);

                if (character.Inventory.GetEquipedItems().Count() > 0)
                {
                    foreach (var item in character.Inventory.GetEquipedItems())
                    {
                        ItemsPresetRecord itemspreset = new ItemsPresetRecord()
                        {
                            OwnerId = OwnerId,
                            PresetId = ObjectId,
                            objectId = (uint)ObjectId,
                            Position = (short)item.Position,
                            ObjGid = item.GetObjectItem().objectGID,
                            ObjUid = item.GetObjectItem().objectUID
                        };
                        newItemsPresetRecords.Add(itemspreset);
                        ItemsPresetRecords.Add(itemspreset);
                    }
                }

                SendSavePresetResult(character, ObjectId, preset);
            }
            else
            {
                var preset = PresetRecords.FirstOrDefault(x => x.ObjectId == PresetId && x.OwnerId == OwnerId);
                var spellspreset = GetSpellsPresetRecord(preset);
                var statspreset = GetStatsPresetRecord(preset);

                preset.SymbolId = SymbolId;
                preset.Name = Name;
                preset.MountEquiped = character.EquippedMount != null;
                preset.CharacterLook = character.Look.ToString();

                spellspreset.Spells = GetSpellsPreset((uint)ObjectId, spellids.ToArray(), character);
                statspreset.Stats = GetStatsPreset((short)ObjectId, character);

                if (GetItemsPresetRecord(preset).Count() > 0)
                {
                    List<ItemsPresetRecord> todelete = new List<ItemsPresetRecord>();

                    foreach (var pres in ItemsPresetRecords.Where(x => x.OwnerId == OwnerId && x.PresetId == PresetId))
                    {
                        todelete.Add(pres);
                    }

                    foreach (var deleted in todelete)
                    {
                        deleteItemsPresetRecords.Add(deleted);
                        ItemsPresetRecords.Remove(deleted);
                    }
                }

                if (character.Inventory.GetEquipedItems().Count() > 0)
                {
                    foreach (var item in character.Inventory.GetEquipedItems())
                    {
                        ItemsPresetRecord itemspreset = new ItemsPresetRecord()
                        {
                            OwnerId = OwnerId,
                            PresetId = PresetId,
                            objectId = (uint)ObjectId,
                            Position = (short)item.Position,
                            ObjGid = item.GetObjectItem().objectGID,
                            ObjUid = item.GetObjectItem().objectUID
                        };
                        newItemsPresetRecords.Add(itemspreset);
                        ItemsPresetRecords.Add(itemspreset);
                    }
                }

                SendSavePresetResult(character, ObjectId, preset);
            }

        }
        #endregion

        #region PresetsList
        public void SendPresetsListMessage(WorldClient client)
        {
            List<Preset> Press = new List<Preset>();

            var presets = PresetRecords.Where(x => x.OwnerId == client.Character.Id);

            if (presets.Count() < 1)
                return;

            foreach (var preset in presets)
            {
                var presetSlot = client.Character.Shortcuts.PresetShortcuts.FirstOrDefault(x => x.Value.PresetId == preset.PresetId);

                if (presetSlot.Value != null)
                {
                    var shortcut = new PresetShortcut(client.Character.Record, presetSlot.Value.Slot, preset.PresetId);
                    ShortcutHandler.SendShortcutBarRefreshMessage(client, ShortcutBarEnum.GENERAL_SHORTCUT_BAR, shortcut);
                }
            }

            InventoryHandler.SendPresetsListMessage(client, GetPresetsFromDatabase(client.Character));
        }
        #endregion
    }
}