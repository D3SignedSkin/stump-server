using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Stump.Core.Extensions;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Types;
using Stump.Server.WorldServer.Database.Shortcuts;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Items.Player;
using Stump.Server.WorldServer.Handlers.Shortcuts;
using Shortcut = Stump.Server.WorldServer.Database.Shortcuts.Shortcut;

namespace Stump.Server.WorldServer.Game.Shortcuts
{
    public class ShortcutBar
    {
        public const int MaxSlot = 40;

        private readonly object m_locker = new object();
        private readonly Queue<Shortcut> m_shortcutsToDelete = new Queue<Shortcut>();
        private Dictionary<int, SpellShortcut> m_spellShortcuts = new Dictionary<int, SpellShortcut>();
        private Dictionary<int, SpellShortcut> m_CustomspellShortcuts = new Dictionary<int, SpellShortcut>();
        private Dictionary<int, ItemShortcut> m_itemShortcuts = new Dictionary<int, ItemShortcut>();
        private Dictionary<int, PresetShortcut> m_presetShortcuts = new Dictionary<int, PresetShortcut>();
        private Dictionary<int, SmileyShortcut> m_smileyShortcuts = new Dictionary<int, SmileyShortcut>();
        private Dictionary<int, EmoteShortcut> m_emoteShortcuts = new Dictionary<int, EmoteShortcut>();

        public ShortcutBar(Character owner)
        {
            Owner = owner;
        }

        public Character Owner
        {
            get;
            private set;
        }

        public IReadOnlyDictionary<int, SpellShortcut> SpellsShortcuts
        {
            get
            {
                if (Owner.IsInIncarnation)
                    return new ReadOnlyDictionary<int, SpellShortcut>(m_CustomspellShortcuts);

                return new ReadOnlyDictionary<int, SpellShortcut>(m_spellShortcuts);
            }
        }

        public IReadOnlyDictionary<int, ItemShortcut> ItemsShortcuts
        {
            get { return new ReadOnlyDictionary<int, ItemShortcut>(m_itemShortcuts); }
        }

        public IReadOnlyDictionary<int, PresetShortcut> PresetShortcuts
        {
            get { return new ReadOnlyDictionary<int, PresetShortcut>(m_presetShortcuts); }
        }
        public IReadOnlyDictionary<int, SmileyShortcut> SmileyShortcuts
        {
            get { return new ReadOnlyDictionary<int, SmileyShortcut>(m_smileyShortcuts); }
        }
        public IReadOnlyDictionary<int, EmoteShortcut> EmoteShortcuts
        {
            get { return new ReadOnlyDictionary<int, EmoteShortcut>(m_emoteShortcuts); }
        }

        internal void Load()
        {
            var database = WorldServer.Instance.DBAccessor.Database;

            m_spellShortcuts = database.Query<SpellShortcut>(string.Format(SpellShortcutRelator.FetchByOwner, Owner.Id)).DistinctBy(x => x.Slot).ToDictionary(x => x.Slot);
            m_itemShortcuts = database.Query<ItemShortcut>(string.Format(ItemShortcutRelator.FetchByOwner, Owner.Id)).DistinctBy(x => x.Slot).ToDictionary(x => x.Slot);
            m_presetShortcuts = database.Query<PresetShortcut>(string.Format(PresetShortcutRelator.FetchByOwner, Owner.Id)).DistinctBy(x => x.Slot).ToDictionary(x => x.Slot);
            m_smileyShortcuts = database.Query<SmileyShortcut>(string.Format(SmileyShortcutRelator.FetchByOwner, Owner.Id)).DistinctBy(x => x.Slot).ToDictionary(x => x.Slot);
            m_emoteShortcuts = database.Query<EmoteShortcut>(string.Format(EmoteShortcutRelator.FetchByOwner, Owner.Id)).DistinctBy(x => x.Slot).ToDictionary(x => x.Slot);
        }

        public void AddShortcut(ShortcutBarEnum barType, DofusProtocol.Types.Shortcut shortcut)
        {
            if (shortcut is ShortcutSpell && barType == ShortcutBarEnum.SPELL_SHORTCUT_BAR)
            {
                AddSpellShortcut(shortcut.slot, (short)((ShortcutSpell)shortcut).spellId, Owner.IsInIncarnation);
            }
            else if (shortcut is ShortcutObjectItem && barType == ShortcutBarEnum.GENERAL_SHORTCUT_BAR)
            {
                var item = Owner.Inventory.TryGetItem(((ShortcutObjectItem)shortcut).itemUID);

                if (item != null)
                {
                    AddItemShortcut(shortcut.slot, item);
                }
                else
                {
                    ShortcutHandler.SendShortcutBarAddErrorMessage(Owner.Client);
                }
            }
            else if (shortcut is ShortcutObjectPreset && barType == ShortcutBarEnum.GENERAL_SHORTCUT_BAR)
            {
                AddPresetShortcut(shortcut.slot, ((ShortcutObjectPreset)shortcut).presetId);
            }
            else if (shortcut is ShortcutEmote && barType == ShortcutBarEnum.GENERAL_SHORTCUT_BAR)
            {
                AddEmoteShortcut(shortcut.slot, ((ShortcutEmote)shortcut).emoteId);
            }
            else if (shortcut is ShortcutSmiley && barType == ShortcutBarEnum.GENERAL_SHORTCUT_BAR)
            {
                AddSmileyShortcut(shortcut.slot, ((ShortcutSmiley)shortcut).smileyId);
            }
            else
            {
                ShortcutHandler.SendShortcutBarAddErrorMessage(Owner.Client);
            }
        }

        public void AddSpellShortcut(int slot, short spellId, bool Custom = false, bool send = true)
        {
            if (Owner.Spells.CanPlaySpell(spellId))
            {
                if (!IsSlotFree(slot, ShortcutBarEnum.SPELL_SHORTCUT_BAR, Custom))
                {
                    RemoveShortcut(ShortcutBarEnum.SPELL_SHORTCUT_BAR, slot);
                }

                var shortcut = new SpellShortcut(Owner.Record, slot, spellId);

                if (Custom)
                {
                    m_CustomspellShortcuts.Add(slot, shortcut);
                }
                else
                {
                    m_spellShortcuts.Add(slot, shortcut);
                }

                if (send)
                    ShortcutHandler.SendShortcutBarRefreshMessage(Owner.Client, ShortcutBarEnum.SPELL_SHORTCUT_BAR, shortcut);
            }
        }

        public void AddItemShortcut(int slot, BasePlayerItem item)
        {
            if (!IsSlotFree(slot, ShortcutBarEnum.GENERAL_SHORTCUT_BAR))
                RemoveShortcut(ShortcutBarEnum.GENERAL_SHORTCUT_BAR, slot);

            var shortcut = new ItemShortcut(Owner.Record, slot, item.Template.Id, item.Guid);

            m_itemShortcuts.Add(slot, shortcut);
            ShortcutHandler.SendShortcutBarRefreshMessage(Owner.Client, ShortcutBarEnum.GENERAL_SHORTCUT_BAR, shortcut);
        }

        public void AddPresetShortcut(int slot, int presetId)
        {
            if (!IsSlotFree(slot, ShortcutBarEnum.GENERAL_SHORTCUT_BAR))
                RemoveShortcut(ShortcutBarEnum.GENERAL_SHORTCUT_BAR, slot);

            var shortcut = new PresetShortcut(Owner.Record, slot, presetId);

            m_presetShortcuts.Add(slot, shortcut);
            ShortcutHandler.SendShortcutBarRefreshMessage(Owner.Client, ShortcutBarEnum.GENERAL_SHORTCUT_BAR, shortcut);
        }

        public void AddEmoteShortcut(int slot, int emoteId)
        {
            if (!IsSlotFree(slot, ShortcutBarEnum.GENERAL_SHORTCUT_BAR))
                RemoveShortcut(ShortcutBarEnum.GENERAL_SHORTCUT_BAR, slot);

            var shortcut = new EmoteShortcut(Owner.Record, slot, emoteId);

            m_emoteShortcuts.Add(slot, shortcut);
            ShortcutHandler.SendShortcutBarRefreshMessage(Owner.Client, ShortcutBarEnum.GENERAL_SHORTCUT_BAR, shortcut);
        }

        public void AddSmileyShortcut(int slot, int smileyId)
        {
            if (!IsSlotFree(slot, ShortcutBarEnum.GENERAL_SHORTCUT_BAR))
                RemoveShortcut(ShortcutBarEnum.GENERAL_SHORTCUT_BAR, slot);

            var shortcut = new SmileyShortcut(Owner.Record, slot, smileyId);

            m_smileyShortcuts.Add(slot, shortcut);
            ShortcutHandler.SendShortcutBarRefreshMessage(Owner.Client, ShortcutBarEnum.GENERAL_SHORTCUT_BAR, shortcut);
        }

        public void SwapSpellShortcuts(short previousspellId, short newspellId)
        {
            var previouslist = GetShortcuts(ShortcutBarEnum.SPELL_SHORTCUT_BAR).Where(x => (x as SpellShortcut).SpellId == previousspellId);

            if (previouslist == null || previouslist.Count() == 0) return;

            foreach (var shortcut in previouslist)
            {
                (shortcut as SpellShortcut).SpellId = newspellId;
                ShortcutHandler.SendShortcutBarRefreshMessage(Owner.Client, ShortcutBarEnum.SPELL_SHORTCUT_BAR, shortcut);
            }
        }

        public void SwapShortcuts(ShortcutBarEnum barType, int slot, int newSlot)
        {
            if (IsSlotFree(slot, barType))
                return;

            var shortcutToSwitch = GetShortcut(barType, slot);
            var shortcutDestination = GetShortcut(barType, newSlot);

            RemoveInternal(shortcutToSwitch);
            RemoveInternal(shortcutDestination);

            if (shortcutDestination != null)
            {
                shortcutDestination.Slot = slot;
                AddInternal(shortcutDestination);
                ShortcutHandler.SendShortcutBarRefreshMessage(Owner.Client, barType, shortcutDestination);
            }
            else
            {
                ShortcutHandler.SendShortcutBarRemovedMessage(Owner.Client, barType, slot);
            }

            shortcutToSwitch.Slot = newSlot;
            AddInternal(shortcutToSwitch);
            ShortcutHandler.SendShortcutBarRefreshMessage(Owner.Client, barType, shortcutToSwitch);
        }

        public void ResetCustomSpellsShortcuts()
        {
            m_CustomspellShortcuts.Clear();
        }

        public void RemoveShortcut(ShortcutBarEnum barType, int slot)
        {
            var shortcut = GetShortcut(barType, slot);

            if (shortcut == null)
                return;

            switch (barType)
            {
                case ShortcutBarEnum.SPELL_SHORTCUT_BAR:
                    //if (m_CustomspellShortcuts.Count > 0) m_CustomspellShortcuts.Remove(slot);
                    if (Owner.IsInIncarnation)
                        m_CustomspellShortcuts.Remove(slot);
                    else
                        m_spellShortcuts.Remove(slot);
                    break;
                case ShortcutBarEnum.GENERAL_SHORTCUT_BAR:
                    {
                        if (shortcut is ItemShortcut)
                            m_itemShortcuts.Remove(slot);
                        else if (shortcut is PresetShortcut)
                            m_presetShortcuts.Remove(slot);
                        else if (shortcut is EmoteShortcut)
                            m_emoteShortcuts.Remove(slot);
                        else if (shortcut is SmileyShortcut)
                            m_smileyShortcuts.Remove(slot);
                    }

                    break;
            }

            if (Owner.IsInIncarnation && barType == ShortcutBarEnum.SPELL_SHORTCUT_BAR)
            { }
            else
                m_shortcutsToDelete.Enqueue(shortcut);

            ShortcutHandler.SendShortcutBarRemovedMessage(Owner.Client, barType, slot);
        }

        private void AddInternal(Shortcut shortcut)
        {
            if (shortcut is SpellShortcut && !m_spellShortcuts.ContainsKey(shortcut.Slot))
                m_spellShortcuts.Add(shortcut.Slot, (SpellShortcut)shortcut);
            else if (shortcut is ItemShortcut && !m_itemShortcuts.ContainsKey(shortcut.Slot))
                m_itemShortcuts.Add(shortcut.Slot, (ItemShortcut)shortcut);
            else if (shortcut is PresetShortcut && !m_presetShortcuts.ContainsKey(shortcut.Slot))
                m_presetShortcuts.Add(shortcut.Slot, (PresetShortcut)shortcut);
            else if (shortcut is EmoteShortcut && !m_emoteShortcuts.ContainsKey(shortcut.Slot))
                m_emoteShortcuts.Add(shortcut.Slot, (EmoteShortcut)shortcut);
            else if (shortcut is SmileyShortcut && !m_smileyShortcuts.ContainsKey(shortcut.Slot))
                m_smileyShortcuts.Add(shortcut.Slot, (SmileyShortcut)shortcut);
        }

        private void RemoveInternal(Shortcut shortcut)
        {
            if (shortcut is SpellShortcut)
            {
                m_spellShortcuts.Remove(shortcut.Slot);
                return;
            }

            if (shortcut is ItemShortcut)
            {
                m_itemShortcuts.Remove(shortcut.Slot);
                return;
            }

            if (shortcut is PresetShortcut)
            {
                m_presetShortcuts.Remove(shortcut.Slot);
                return;
            }

            if (shortcut is EmoteShortcut)
            {
                m_emoteShortcuts.Remove(shortcut.Slot);
                return;
            }

            if (shortcut is SmileyShortcut)
            {
                m_smileyShortcuts.Remove(shortcut.Slot);
                return;
            }
        }

        public int GetNextFreeSlot(ShortcutBarEnum barType)
        {
            for (var i = 0; i < MaxSlot; i++)
            {
                if (IsSlotFree(i, barType))
                    return i;
            }

            return MaxSlot;
        }

        public bool IsSlotFree(int slot, ShortcutBarEnum barType, bool Custom = false)
        {
            switch (barType)
            {

                case ShortcutBarEnum.SPELL_SHORTCUT_BAR:
                    if (Custom) return !m_CustomspellShortcuts.ContainsKey(slot);
                    else return !m_spellShortcuts.ContainsKey(slot);
                case ShortcutBarEnum.GENERAL_SHORTCUT_BAR:
                    return !m_itemShortcuts.ContainsKey(slot) && !m_presetShortcuts.ContainsKey(slot) && !m_emoteShortcuts.ContainsKey(slot) && !m_smileyShortcuts.ContainsKey(slot);
            }

            return true;
        }

        public Shortcut GetShortcut(ShortcutBarEnum barType, int slot)
        {
            switch (barType)
            {
                case ShortcutBarEnum.SPELL_SHORTCUT_BAR:
                    return GetSpellShortcut(slot);
                case ShortcutBarEnum.GENERAL_SHORTCUT_BAR:
                    {

                        if (GetItemShortcut(slot) != null)
                            return GetItemShortcut(slot);

                        if (GetPresetShortcut(slot) != null)
                            return GetPresetShortcut(slot);

                        if (GetEmoteShortcut(slot) != null)
                            return GetEmoteShortcut(slot);

                        if (GetSmileyShortcut(slot) != null)
                            return GetSmileyShortcut(slot);

                        return null;
                    }
                default:
                    return null;
            }
        }

        public IEnumerable<Shortcut> GetShortcuts(ShortcutBarEnum barType)
        {
            switch (barType)
            {
                case ShortcutBarEnum.SPELL_SHORTCUT_BAR:
                    //return m_CustomspellShortcuts.Count > 0 ? m_CustomspellShortcuts.Values : m_spellShortcuts.Values;
                    return Owner.IsInIncarnation ? m_CustomspellShortcuts.Values : m_spellShortcuts.Values;
                case ShortcutBarEnum.GENERAL_SHORTCUT_BAR:
                    return m_itemShortcuts.Values.Concat<Shortcut>(m_presetShortcuts.Values).Concat<Shortcut>(m_emoteShortcuts.Values).Concat<Shortcut>(m_smileyShortcuts.Values);
                default:
                    return new Shortcut[0];
            }
        }

        public SpellShortcut GetSpellShortcut(int slot)
        {
            SpellShortcut shortcut;
            //if (m_CustomspellShortcuts.Count > 0) return m_CustomspellShortcuts.TryGetValue(slot, out shortcut) ? shortcut : null;
            if (Owner.IsInIncarnation) return m_CustomspellShortcuts.TryGetValue(slot, out shortcut) ? shortcut : null;
            return m_spellShortcuts.TryGetValue(slot, out shortcut) ? shortcut : null;
        }

        public ItemShortcut GetItemShortcut(int slot)
        {
            ItemShortcut shortcut;
            return m_itemShortcuts.TryGetValue(slot, out shortcut) ? shortcut : null;
        }

        public PresetShortcut GetPresetShortcut(int slot)
        {
            PresetShortcut shortcut;
            return m_presetShortcuts.TryGetValue(slot, out shortcut) ? shortcut : null;
        }

        public EmoteShortcut GetEmoteShortcut(int slot)
        {
            EmoteShortcut shortcut;
            return m_emoteShortcuts.TryGetValue(slot, out shortcut) ? shortcut : null;
        }

        public SmileyShortcut GetSmileyShortcut(int slot)
        {
            SmileyShortcut shortcut;
            return m_smileyShortcuts.TryGetValue(slot, out shortcut) ? shortcut : null;
        }

        public void Save()
        {
            lock (m_locker)
            {
                try
                {
                    var database = WorldServer.Instance.DBAccessor.Database;

                    foreach (var shortcut in m_itemShortcuts.Where(shortcut => shortcut.Value?.IsDirty == true || shortcut.Value?.IsNew == true).ToList())
                    {
                        if (shortcut.Value == null)
                            continue;

                        database.Save(shortcut.Value);
                    }

                    foreach (var shortcut in m_spellShortcuts.Where(shortcut => shortcut.Value?.IsDirty == true || shortcut.Value?.IsNew == true).ToList())
                    {
                        if (shortcut.Value == null)
                            continue;

                        database.Save(shortcut.Value);
                    }

                    foreach (var shortcut in m_presetShortcuts.Where(shortcut => shortcut.Value?.IsDirty == true || shortcut.Value?.IsNew == true).ToList())
                    {
                        if (shortcut.Value == null)
                            continue;

                        database.Save(shortcut.Value);
                    }

                    foreach (var shortcut in m_emoteShortcuts.Where(shortcut => shortcut.Value?.IsDirty == true || shortcut.Value?.IsNew == true).ToList())
                    {
                        if (shortcut.Value == null)
                            continue;

                        database.Save(shortcut.Value);
                    }

                    foreach (var shortcut in m_smileyShortcuts.Where(shortcut => shortcut.Value?.IsDirty == true || shortcut.Value?.IsNew == true).ToList())
                    {
                        if (shortcut.Value == null)
                            continue;

                        database.Save(shortcut.Value);
                    }

                    while (m_shortcutsToDelete.Count > 0)
                    {
                        var record = m_shortcutsToDelete.Dequeue();

                        if (record != null)
                            database.Delete(record);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error during save operation: {ex.Message}");
                }
            }
        }
    }
}