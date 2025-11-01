using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Stump.Server.WorldServer.Database.Characters;
using Stump.Server.WorldServer.Database.Spells;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Handlers.Inventory;
using Stump.Server.WorldServer.Database.Shortcuts;
using System;
using NLog.Targets;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.D2oClasses;

namespace Stump.Server.WorldServer.Game.Spells
{
    public class SpellInventory : IEnumerable<CharacterSpell>
    {
        private readonly Dictionary<int, CharacterSpell> m_spells = new Dictionary<int, CharacterSpell>();
        private readonly Queue<CharacterSpellRecord> m_spellsToDelete = new Queue<CharacterSpellRecord>();
        private List<CharacterSpell> m_CustomSpells = new List<CharacterSpell>();
        private readonly object m_locker = new object();

        private List<int> SpellsCustomPergas = new List<int>() { 0, 364, 366, 414, 367, 373, 369, 368, 350, 370, 3506, 413};

        public SpellInventory(Character owner)
        {
            Owner = owner;
        }

        public Character Owner
        {
            get;
            private set;
        }

        internal void LoadSpells()
        {
            var database = WorldServer.Instance.DBAccessor.Database;

            foreach (var spell in database.Query<CharacterSpellRecord>(string.Format(CharacterSpellRelator.FetchByOwner, Owner.Id)).Select(record => new CharacterSpell(record)))
            {
                if (m_spells.ContainsKey(spell.Id))
                    continue;

                m_spells.Add(spell.Id, spell);
            }

            var spellsToLearn = from spell in Owner.Breed.Spells
                                where spell.ObtainLevel <= Owner.Level && !Character.SpellsBlock.Contains(spell.Spell)
                                orderby spell.ObtainLevel, spell.Spell ascending
                                select spell;

            var spellsToLearnVariant = from variant in Owner.Breed.Spells
                                       where variant.VariantLevel <= Owner.Level && !Character.SpellsBlock.Contains(variant.VariantId)
                                       orderby variant.VariantLevel, variant.VariantId ascending
                                       select variant;
            var slot = 0;

            foreach (var spellRecord in spellsToLearn.Select(learnableSpell => SpellManager.Instance.CreateSpellRecord(Owner.Record, SpellManager.Instance.GetSpellTemplate(learnableSpell.Spell))))
            {
                if (!m_spells.ContainsKey(spellRecord.SpellId))
                {
                    m_spells.Add(spellRecord.SpellId, new CharacterSpell(spellRecord));
                    database.Insert(spellRecord);

                    spellRecord.Selected = spellRecord.SpellId == 0 ? true : IsVariant((ushort)spellRecord.SpellId) ? false : true;

                    var shortcut = new SpellShortcut(Owner.Record, slot, (short)spellRecord.SpellId);
                    database.Insert(shortcut);
                    slot++;
                }
            }

            foreach (var spellRecord in spellsToLearnVariant.Select(learnableSpell => SpellManager.Instance.CreateSpellRecord(Owner.Record, SpellManager.Instance.GetSpellTemplate(learnableSpell.VariantId))))
            {
                if (!m_spells.ContainsKey(spellRecord.SpellId))
                {
                    m_spells.Add(spellRecord.SpellId, new CharacterSpell(spellRecord));
                    database.Insert(spellRecord);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool UnLearnSpell(CharacterSpell spell) => UnLearnSpell(spell.Id);

        public bool UnLearnSpell(SpellTemplate spell) => UnLearnSpell(spell.Id);

        public IEnumerator<CharacterSpell> GetEnumerator() => m_spells.Values.GetEnumerator();

        public void SetCustomSpells(List<CharacterSpell> spells)
        {
            m_CustomSpells = spells;
        }

        public void ResetCustomSpells()
        {
            m_CustomSpells = new List<CharacterSpell>();
        }

        public bool HasSpell(int id, bool WithoutCustom = false)
        {
            if (Owner.IsInIncarnation && !WithoutCustom)
            {
                return m_CustomSpells.Any(x => x.Id == id);
            }

            return m_spells.ContainsKey(id);
        }

        public bool HasSpell(CharacterSpell spell, bool WithoutCustom = false)
        {
            if (Owner.IsInIncarnation && !WithoutCustom)
            {
                return m_CustomSpells.Any(x => x.Id == spell.Id);
            }

            return m_spells.ContainsKey(spell.Id);
        }

        public CharacterSpell GetSpell(int id, bool WithoutCustom = false)
        {
            CharacterSpell spell;

            if (Owner.IsInIncarnation && !WithoutCustom)
            {
                return m_CustomSpells.FirstOrDefault(x => x.Id == id);
            }

            return m_spells.TryGetValue(id, out spell) ? spell : null;
        }

        public bool CanPlaySpell(int id)
        {
            if (m_CustomSpells.Count > 0)
            {
                return m_CustomSpells.Any(x => x.Id == id);
            }

            return m_spells.ContainsKey(id) && m_spells.Where(x => x.Key == id).FirstOrDefault().Value.Selected == true || SpellsCustomPergas.Contains(id);
        }

        public IEnumerable<CharacterSpell> GetPlayableSpells()
        {
            if (m_CustomSpells.Any())
                return m_CustomSpells;

            return m_spells.Where(x => x.Value.Selected || SpellsCustomPergas.Contains(x.Value.Id)).Select(x => x.Value).ToList();
        }

        public IEnumerable<CharacterSpell> GetSpells(bool WithoutCustom = false)
        {
            if (Owner.IsInIncarnation && !WithoutCustom)
                return m_CustomSpells;

            return m_spells.Values;
        }

        public CharacterSpell LearnSpell(int id, bool SendMessage = true)
        {
            var template = SpellManager.Instance.GetSpellTemplate(id);

            return template == null ? null : LearnSpell(template, SendMessage);
        }

        public CharacterSpell LearnSpell(SpellTemplate template, bool SendMessage = true)
        {
            var record = SpellManager.Instance.CreateSpellRecord(Owner.Record, template);
            var spell = new CharacterSpell(record);
            m_spells.Add(spell.Id, spell);

            record.Selected = IsVariant((ushort)record.SpellId) ? false : true;

            if(SendMessage)
                Owner.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 3, spell); //Você aprendeu a conjurar o feitiço : <b>$spell%1</b>.

            InventoryHandler.SendSpellListMessage(Owner.Client, true);
            return spell;
        }

        public bool UnLearnSpell(int id)
        {
            var spell = GetSpell(id);

            if (spell == null)
                return true;

            m_spells.Remove(id);
            m_spellsToDelete.Enqueue(spell.Record);

            Owner.SpellsPoints += (ushort)CalculateSpellPoints(spell.CurrentLevel);

            InventoryHandler.SendSpellListMessage(Owner.Client, true);
            return true;
        }

        public int CalculateSpellPoints(int level, int currentLevel = 1)
        {
            var spentPoints = 0;

            if (currentLevel > 1)
                spentPoints = CalculateSpellPoints(currentLevel);

            return ((level * (level - 1)) / 2) - spentPoints;
        }

        public bool CanBoostSpell(Spell spell, ushort level, bool send = true)
        {
            if (Owner.IsFighting())
            {
                if (send)
                {
                    InventoryHandler.SendSpellListMessage(Owner.Client, true);
                }

                return false;
            }

            if (spell.CurrentLevel == level || level > 3)
            {
                if (send)
                {
                    InventoryHandler.SendSpellListMessage(Owner.Client, true);
                }

                return false;
            }

            if (Owner.SpellsPoints < CalculateSpellPoints(level, spell.CurrentLevel))
            {
                if (send)
                {
                    InventoryHandler.SendSpellListMessage(Owner.Client, true);
                }

                return false;
            }

            if (spell.ByLevel[level].MinPlayerLevel > Owner.Level)
            {
                if (send)
                {
                    InventoryHandler.SendSpellListMessage(Owner.Client, true);
                }

                return false;
            }

            return true;
        }

        public bool BoostSpell(int id, ushort level)
        {
            var spell = GetSpell(id);

            if (spell == null)
            {
                InventoryHandler.SendSpellListMessage(Owner.Client, true);

                return false;
            }

            if (!CanBoostSpell(spell, level))
                return false;

            spell.CurrentLevel = (byte)level;

            InventoryHandler.SendSpellListMessage(Owner.Client, true);

            return true;
        }

        public bool ForgetSpell(SpellTemplate spell)
        {
            return ForgetSpell(spell.Id);
        }

        public bool ForgetSpell(int id)
        {
            if (!HasSpell(id))
                return false;

            var spell = GetSpell(id);

            return ForgetSpell(spell);
        }

        public bool ForgetSpell(CharacterSpell spell)
        {
            if (!HasSpell(spell.Id))
                return false;

            var level = spell.CurrentLevel;

            for (var i = 1; i < level; i++)
            {
                DowngradeSpell(spell, false);
            }

            InventoryHandler.SendSpellListMessage(Owner.Client, true);
            return true;
        }

        public void ForgetAllSpells()
        {
            foreach (var spell in m_spells)
            {
                var level = spell.Value.CurrentLevel;

                for (var i = 1; i < level; i++)
                {
                    DowngradeSpell(spell.Value, false);
                }
            }

            InventoryHandler.SendSpellListMessage(Owner.Client, true);
            Owner.RefreshStats();
        }

        public int DowngradeSpell(SpellTemplate spell)
        {
            return DowngradeSpell(spell.Id);
        }

        public int DowngradeSpell(int id)
        {
            if (!HasSpell(id))
                return 0;

            var spell = GetSpell(id);

            return DowngradeSpell(spell);
        }

        public int DowngradeSpell(CharacterSpell spell, bool send = true)
        {
            if (!HasSpell(spell.Id))
                return 0;

            if (spell.CurrentLevel <= 1)
                return 0;

            spell.CurrentLevel -= 1;
            Owner.SpellsPoints += spell.CurrentLevel;

            if (!send)
                return spell.CurrentLevel;

            InventoryHandler.SendSpellListMessage(Owner.Client, true);

            Owner.RefreshStats();

            return spell.CurrentLevel;
        }

        public void MoveSpell(int id, byte position)
        {
            var spell = GetSpell(id);

            if (spell == null)
                return;

            Owner.Shortcuts.AddSpellShortcut(position, (short)id);
        }

        public int CountSpentBoostPoint()
        {
            var count = 0;

            foreach (var spell in this)
            {
                for (var i = 1; i < spell.CurrentLevel; i++)
                {
                    count += i;
                }
            }

            return count;
        }

        public void SpellVariantActivate(int SpellId, bool Silent = false)
        {
            if (Owner.IsFighting() && Owner.Fight.State == Fights.FightState.Fighting)
                return;

            int DisabledSpellId = SpellManager.Instance.GetSpellPairVariant(SpellId);

            CharacterSpell ActivatedSpell;
            m_spells.TryGetValue(SpellId, out ActivatedSpell);

            if (ActivatedSpell == null)
                return;

            CharacterSpell DisabledSpell;
            m_spells.TryGetValue(DisabledSpellId, out DisabledSpell);

            if (DisabledSpell == null)
                return;

            if (DisabledSpellId == SpellId)
                return;

            ActivatedSpell.Selected = true;
            DisabledSpell.Selected = false;

            if (!Silent)
                InventoryHandler.SendSpellVariantActivationMessage(Owner.Client, SpellId, true);

            Owner.Shortcuts.SwapSpellShortcuts((short)DisabledSpellId, (short)SpellId);
        }

        public void SpellVariantsRefresh()
        {
            InventoryHandler.SendSpellListMessage(Owner.Client, true);
        }

        public bool IsVariant(ushort SpellId)
        {
            return Breeds.BreedManager.Instance.GetBreed(Owner.BreedId).Spells.Any(x => x.VariantId == SpellId);
        }

        public void Save()
        {
            lock (m_locker)
            {
                try
                {
                    var database = WorldServer.Instance.DBAccessor.Database;

                    foreach (var characterSpell in m_spells.ToList())
                    {
                        if (characterSpell.Value == null)
                            continue;

                        database.Save(characterSpell.Value.Record);
                    }

                    while (m_spellsToDelete.Count > 0)
                    {
                        var record = m_spellsToDelete.Dequeue();

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