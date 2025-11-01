using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Stump.Core.Attributes;
using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Database;
using Stump.Server.BaseServer.Initialization;
using Stump.Server.WorldServer.Database.Breeds;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Presets;

namespace Stump.Server.WorldServer.Game.Breeds
{
    public class BreedManager : DataManager<BreedManager>
    {
        /// <summary>
        /// List of available breeds
        /// </summary>
        [Variable]
        public readonly static List<PlayableBreedEnum> AvailableBreeds = new List<PlayableBreedEnum>
            {
                PlayableBreedEnum.Feca,
                PlayableBreedEnum.Osamodas,
                PlayableBreedEnum.Enutrof,
                PlayableBreedEnum.Sram,
                PlayableBreedEnum.Xelor,
                PlayableBreedEnum.Ecaflip,
                PlayableBreedEnum.Eniripsa,
                PlayableBreedEnum.Iop,
                PlayableBreedEnum.Cra,
                PlayableBreedEnum.Sadida,
                PlayableBreedEnum.Sacrieur,
                PlayableBreedEnum.Pandawa,
                PlayableBreedEnum.Roublard,
                PlayableBreedEnum.Zobal,
                PlayableBreedEnum.Steamer,
                PlayableBreedEnum.Eliotrope,
                PlayableBreedEnum.Huppermage,
                PlayableBreedEnum.Ouginak
            };

        public uint AvailableBreedsFlags
        {
            get
            {
                return (uint)AvailableBreeds.Aggregate(0, (current, breedEnum) => current | (1 << ((int)breedEnum - 1)));
            }
        }

        private readonly Dictionary<int, Breed> m_breeds = new Dictionary<int, Breed>();
        private Dictionary<int, Head> m_heads = new Dictionary<int, Head>();

        public IReadOnlyDictionary<int, Head> Heads => new ReadOnlyDictionary<int, Head>(m_heads);

        [Initialization(InitializationPass.Third)]
        public override void Initialize()
        {
            base.Initialize();
            m_breeds.Clear();

            foreach (var breed in Database.Query<Breed, BreedItem, BreedSpell, Breed>(new BreedRelator().Map, BreedRelator.FetchQuery))
            {
                m_breeds.Add(breed.Id, breed);
            }

            m_heads = Database.Query<Head>(HeadRelator.FetchQuery).ToDictionary(x => x.Id);
        }

        public Breed GetBreed(PlayableBreedEnum breed)
        {
            return GetBreed((int)breed);
        }

        /// <summary>
        /// Get the breed associated to the given id
        /// </summary>
        /// <param name="id"></param>
        public Breed GetBreed(int id)
        {
            Breed breed;
            m_breeds.TryGetValue(id, out breed);

            return breed;
        }

        public Head GetHead(int id)
        {
            Head head;
            m_heads.TryGetValue(id, out head);

            return head;
        }

        public Head GetHead(Predicate<Head> predicate)
        {
            return m_heads.Values.FirstOrDefault(x => predicate(x));
        }

        public bool IsBreedAvailable(int id) => AvailableBreeds.Contains((PlayableBreedEnum)id);

        /// <summary>
        /// Add a breed instance to the database
        /// </summary>
        /// <param name="breed">Breed instance to add</param>
        /// <param name="defineId">When set to true the breed id will be auto generated</param>
        public void AddBreed(Breed breed, bool defineId = false)
        {
            if (defineId)
            {
                var id = m_breeds.Keys.Max() + 1;
                breed.Id = id;
            }

            if (m_breeds.ContainsKey(breed.Id))
                throw new Exception(string.Format("Breed with id {0} already exists", breed.Id));

            m_breeds.Add(breed.Id, breed);

            Database.Insert(breed);
        }

        /// <summary>
        /// Remove a breed from the database
        /// </summary>
        /// <param name="breed"></param>
        public void RemoveBreed(Breed breed)
        {
            RemoveBreed(breed.Id);
        }

        /// <summary>
        /// Remove a breed from the database by his id
        /// </summary>
        /// <param name="id"></param>
        public void RemoveBreed(int id)
        {
            if (!m_breeds.ContainsKey(id))
                throw new Exception(string.Format("Breed with id {0} does not exist", id));

            // it's safer to delete the breed in the dictionary first next in the database
            var breed = m_breeds[id];
            m_breeds.Remove(id);

            Database.Delete(breed);
        }

        public static void ChangeBreed(Character character, PlayableBreedEnum breed)
        {
            character.ResetStats();

            //Removendo todos os presets de arranjos do personagem antigo.
            foreach (var Preset in PresetsManager.Instance.GetPresetsFromDatabase(character))
            {
                PresetsManager.Instance.DeletePreset(character, Preset.id, character.Id);
            }

            //Removendo todos os shortcuts do personagem.
            foreach (var shortcut in character.Shortcuts.SpellsShortcuts.ToArray())
            {
                character.Shortcuts.RemoveShortcut(ShortcutBarEnum.SPELL_SHORTCUT_BAR, shortcut.Key);
            }

            //Removendo as Spells da Breed antiga.
            foreach (var breedSpell in character.Breed.Spells)
            {
                character.Spells.UnLearnSpell(breedSpell.Spell);
                character.Spells.UnLearnSpell(breedSpell.VariantId);
            }

            character.SetBreed(breed);
            character.SaveNow();
        }

        #region Desativado
        //static void ForgetSpecialSpells(Character character)
        //{
        //    var specialSpellsList = new List<SpellIdEnum>
        //    {
        //        SpellIdEnum.REINFORCED_PROTECTION,
        //        SpellIdEnum.SPIRITUAL_LEASH_420,
        //        SpellIdEnum.PULL_OUT,
        //        SpellIdEnum.JINX,
        //        SpellIdEnum.RHOL_BAK,
        //        SpellIdEnum.FELINTION,
        //        SpellIdEnum.BRUTAL_WORD,//SpellIdEnum.DECISIVE_WORD, ANALISE
        //        SpellIdEnum.OUTPOURING,//SpellIdEnum.BROKLE, ANALISE
        //        SpellIdEnum.DISPERSING_ARROW,
        //        SpellIdEnum.THE_TREE_OF_LIFE,
        //        (SpellIdEnum)421,//SpellIdEnum.PAIN_SHARED, ANALISE
        //        SpellIdEnum.DIFFRACTION,
        //        SpellIdEnum.FOCUS,
        //        SpellIdEnum.BOOMBOT,
        //        SpellIdEnum.DRUNKENNESS,
        //        SpellIdEnum.BREAKWATER_3277,
        //        SpellIdEnum.FOCUS,
        //        SpellIdEnum.JOURNEY
        //    };

        //    specialSpellsList.ForEach(x => character.Spells.UnLearnSpell((int)x));
        //}
        #endregion
    }
}
