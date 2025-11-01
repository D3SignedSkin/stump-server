using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NLog;
using Stump.Core.Reflection;
using Stump.Server.BaseServer.Database;
using Stump.Server.BaseServer.Initialization;
using Stump.Server.WorldServer.Database.Characters;
using Stump.Server.WorldServer.Database.Spells;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Effects;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Spells.Casts;
using SpellState = Stump.Server.WorldServer.Database.Spells.SpellState;
using SpellType = Stump.Server.WorldServer.Database.Spells.SpellType;
using Stump.Server.WorldServer.Database.Companion;
using Stump.Server.WorldServer.Database.Breeds;
using Stump.DofusProtocol.Enums;

namespace Stump.Server.WorldServer.Game.Spells
{
    public class SpellManager : DataManager<SpellManager>
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private Dictionary<uint, SpellLevelTemplate> m_spellsLevels;
        private Dictionary<int, SpellTemplate> m_spells;
        private Dictionary<int, SpellBombTemplate> m_spellsBomb;
        private Dictionary<int, SpellType> m_spellsTypes;
        private Dictionary<int, SpellState> m_spellsState;
        private Dictionary<int, SpellEffectFix> m_spellsEffectsFixs;
        private Dictionary<int, FinishMoveTemplate> m_finishMoves;
        private Dictionary<int, CompanionSpellRecord> m_companionSpell = new Dictionary<int, CompanionSpellRecord>();
        private static List<BreedSpell> m_spellsVariants;
        private Dictionary<int, SpellVariant> m_variants;

        private delegate SpellCastHandler SpellCastConstructor(SpellCastInformations cast);
        private readonly Dictionary<int, SpellCastConstructor> m_spellsCastHandler = new Dictionary<int, SpellCastConstructor>();

        #region Fields

        #endregion

        [Initialization(typeof(EffectManager))]
        public override void Initialize()
        {
            m_spellsLevels = Database.Fetch<SpellLevelTemplate>(SpellLevelTemplateRelator.FetchQuery).ToDictionary(entry => entry.Id);
            m_spells = Database.Fetch<SpellTemplate>(SpellTemplateRelator.FetchQuery).ToDictionary(entry => entry.Id);
            m_spellsTypes = Database.Fetch<SpellType>(SpellTypeRelator.FetchQuery).ToDictionary(entry => entry.Id);
            m_spellsState = Database.Fetch<SpellState>(SpellStateRelator.FetchQuery).ToDictionary(entry => entry.Id);
            m_spellsBomb = Database.Fetch<SpellBombTemplate>(SpellBombRelator.FetchQuery).ToDictionary(entry => entry.Id);
            m_spellsEffectsFixs = Database.Fetch<SpellEffectFix>(SpellEffectFixRelator.FetchQuery).ToDictionary(entry => entry.Id);
            m_finishMoves = Database.Fetch<FinishMoveTemplate>(FinishMoveRelator.FetchQuery).ToDictionary(entry => entry.Id);
            m_companionSpell = Database.Fetch<CompanionSpellRecord>("SELECT * FROM companion_spells").ToDictionary(x => x.Id);
            m_spellsVariants = Database.Query<BreedSpell>("SELECT * FROM breeds_spells;").ToList();
            m_variants = Database.Fetch<SpellVariant>(SpellVariantTemplateRelator.FetchQuery).ToDictionary(entry => entry.Id);
            ApplyEffectFixes();
            InitializeHandlers();
        }

        private void InitializeHandlers()
        {
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes().Where(entry => entry.IsSubclassOf(typeof(SpellCastHandler)) && !entry.IsAbstract))
            {
                if (type.GetCustomAttribute<DefaultSpellCastHandlerAttribute>(false) != null)
                    continue; // we don't mind about default handlers

                var attributes = type.GetCustomAttributes<SpellCastHandlerAttribute>().ToArray();

                if (attributes.Length == 0)
                {
                    logger.Error("SpellCastHandler '{0}' has no SpellCastHandlerAttribute", type.Name);
                    continue;
                }

                foreach (var attribute in attributes)
                {
                    var spell = GetSpellTemplate(attribute.Spell);

                    if (spell == null)
                    {
                        logger.Error("SpellCastHandler '{0}' -> Spell {1} not found", type.Name, attribute.Spell);
                        continue;
                    }

                    AddSpellCastHandler(type, spell);
                }
            }
        }

        private void ApplyEffectFixes()
        {
            foreach (var fix in m_spellsEffectsFixs.Values)
            {
                IEnumerable<EffectBase> effects;

                if (fix.SpellId != null)
                {
                    var spell = GetSpellTemplate(fix.SpellId.Value);

                    if (spell == null)
                    {
                        logger.Error($"Cannot apply spell effect fix {fix.Id} because both SpellId {fix.SpellId} doesn't exist");
                        continue;
                    }

                    effects = GetSpellLevels(spell).SelectMany(x => x.Effects);
                }
                else if (fix.SpellLevelId != null)
                {
                    var spellLevel = GetSpellLevel(fix.SpellLevelId.Value);

                    if (spellLevel == null)
                    {
                        logger.Error($"Cannot apply spell effect fix {fix.Id} because both SpellLevelId {fix.SpellLevelId} doesn't exist");
                        continue;
                    }

                    effects = spellLevel.Effects;
                }
                else
                {
                    logger.Error($"Cannot apply spell effect fix {fix.Id} because both SpellId and SpellLevelId are null");
                    continue;
                }

                foreach (var effect in effects.Where((x, i) => fix.EffectId == null || (fix.EffectId == x.Id && (fix.EffectIndex == null || fix.EffectIndex == i))))
                {
                    effect.EffectFix = fix;
                }
            }
        }

        public CharacterSpellRecord CreateSpellRecord(CharacterRecord owner, SpellTemplate template)
        {
            short nivel = 1;

            try
            {
                if (GetSpellTemplate(template.SpellLevelsIds[1]).MinPlayerLevel <= Actors.RolePlay.Characters.ExperienceManager.Instance.GetCharacterLevel(owner.Experience))
                    nivel = 2;

                if (GetSpellTemplate(template.SpellLevelsIds[2]).MinPlayerLevel <= Actors.RolePlay.Characters.ExperienceManager.Instance.GetCharacterLevel(owner.Experience))
                    nivel = 3;
            }
            catch
            { }

            return new CharacterSpellRecord
            {
                OwnerId = owner.Id,
                Level = nivel,
                Position = 63, // always 63
                SpellId = template.Id
            };
        }

        public SpellTemplate GetSpellTemplate(int id)
        {
            SpellTemplate template;
            var spell = m_spells.TryGetValue(id, out template) ? template : null;

            return spell;
        }

        public SpellLevelTemplate GetSpellTemplate(uint spellLevelId)
        {
            SpellLevelTemplate template;
            return m_spellsLevels.TryGetValue(spellLevelId, out template) ? template : null;
        }

        public SpellTemplate GetSpellTemplate(string name, bool ignorecase)
        {
            return m_spells.Values.FirstOrDefault(entry => entry.Name.Equals(name, ignorecase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture));
        }

        public SpellBombTemplate GetSpellBombTemplate(int id)
        {
            SpellBombTemplate template;
            return m_spellsBomb.TryGetValue(id, out template) ? template : null;
        }

        public SpellTemplate GetFirstSpellTemplate(Predicate<SpellTemplate> predicate)
        {
            return m_spells.Values.FirstOrDefault(entry => predicate(entry));
        }

        public IEnumerable<SpellTemplate> GetSpellTemplates()
        {
            return m_spells.Values;
        }

        public int GetRealCompanionSpell(int id)
        {
            return m_companionSpell.ContainsKey(id) ? m_companionSpell[id].SpellId : -1;
        }

        public SpellLevelTemplate GetSpellLevel(int id)
        {
            SpellLevelTemplate template;

            return m_spellsLevels.TryGetValue((uint)id, out template) ? template : null;
        }

        public SpellLevelTemplate GetSpellLevel(int templateid, int level)
        {
            var template = GetSpellTemplate(templateid);

            if (template == null)
                return null;

            return template.SpellLevelsIds.Length <= level - 1 ? null : GetSpellLevel((int)template.SpellLevelsIds[level - 1]);
        }

        public IEnumerable<SpellLevelTemplate> GetSpellLevels(SpellTemplate template)
        {
            try
            {
                var teste = template.SpellLevelsIds.Select(x => m_spellsLevels[x]);
                return teste;
            }
            catch
            {
                logger.Info("tempalte id :" + template.Id);
                return null;
            }
        }

        public IEnumerable<SpellLevelTemplate> GetSpellLevels(int id)
        {
            return m_spellsLevels.Values.Where(entry => entry.Spell.Id == id).OrderBy(entry => entry.Id);
        }

        public IEnumerable<SpellLevelTemplate> GetSpellLevels()
        {
            return m_spellsLevels.Values;
        }

        public SpellType GetSpellType(uint id)
        {
            SpellType template;

            return m_spellsTypes.TryGetValue((int)id, out template) ? template : null;
        }

        public SpellState GetSpellState(uint id)
        {
            SpellState state;

            return m_spellsState.TryGetValue((int)id, out state) ? state : null;
        }

        public IEnumerable<SpellState> GetSpellStates()
        {
            return m_spellsState.Values;
        }

        #region variants
        public static BreedSpell GetSpellVariant(int spell)
        {
            return m_spellsVariants.FirstOrDefault(x => x.Spell == spell || x.VariantId == spell);
        }

        public static BreedSpell[] GetSpellVariant(PlayableBreedEnum breed, ushort level)
        {
            return m_spellsVariants.Where(x => x.BreedId == (int)breed && x.ObtainLevel <= level && x.ObtainLevel > 1).ToArray();
        }

        public static BreedSpell[] GetSpellVariant(PlayableBreedEnum breed)
        {
            return m_spellsVariants.Where(x => x.BreedId == (int)breed && x.ObtainLevel == 1).ToArray();
        }

        public int GetSpellPairVariant(int SpellId)
        {
            int id = SpellId;
            var variant = m_variants.FirstOrDefault(x => x.Value.SpellIds.Contains((uint)SpellId)).Value;

            if (variant != null)
            {
                id = (int)variant.SpellIds.FirstOrDefault(x => x != SpellId);
            }

            return id;
        }

        public IEnumerable<SpellLevelTemplate> GetSpellVariants(SpellTemplate template)
        {
            return template.SpellLevelsIds.Select(x => m_spellsLevels[x]);
        }

        public IEnumerable<BreedSpell> GetSpellVariants()
        {
            return m_spellsVariants;
        }
        #endregion

        public void AddSpellCastHandler(Type handler, SpellTemplate spell)
        {
            var ctor = handler.GetConstructor(new[] { typeof(SpellCastInformations) });

            if (ctor == null)
                throw new Exception(string.Format("Handler {0} : No valid constructor found !", handler.Name));

            try
            {
                m_spellsCastHandler.Add(spell.Id, ctor.CreateDelegate<SpellCastConstructor>());
            }
            catch (ArgumentException ex)
            {
                string errorMessage = string.Format("Duplicate key '{0}' found while adding the SpellTemplate.", spell.Id);
                throw new ArgumentException(errorMessage, ex);
            }
        }

        public SpellCastHandler GetSpellCastHandler(SpellCastInformations cast)
        {
            SpellCastConstructor ctor;
            return m_spellsCastHandler.TryGetValue(cast.Spell.Template.Id, out ctor) ? ctor(cast) : new DefaultSpellCastHandler(cast);
        }

        public SpellCastHandler GetSpellCastHandler(FightActor caster, Spell spell, Cell targetedCell, bool critical)
        {
            return GetSpellCastHandler(new SpellCastInformations(caster, spell, targetedCell, critical));
        }

        public FinishMoveTemplate GetFinishMove(int finishMove)
        {
            FinishMoveTemplate template;

            return m_finishMoves.TryGetValue(finishMove, out template) ? template : null;
        }
    }
}