using System;
using System.Linq;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Types;
using System.Collections.Generic;
using Stump.Server.WorldServer.Game.Maps;
using Stump.Server.WorldServer.Game.Maps.Cells;
using Stump.Server.WorldServer.Database.Interactives;
using Stump.Server.WorldServer.Handlers.Interactives;
using Stump.Server.WorldServer.Game.Interactives.Skills;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;

namespace Stump.Server.WorldServer.Game.Interactives
{
    public class InteractiveObject : WorldObject
    {
        readonly Dictionary<int, Skill> m_skills = new Dictionary<int, Skill>();

        //Remove Reference Hints 
        private List<int> _removeReferenceHint = new List<int>() { 147, 148, 149, 150, 151, 152, 153, 154, 155, 156, 157, 158, 159, 160, 161, 162, 163, 164, 165, 166, 167, 168, 169, 170, 171, 172, 173, 174, 175, 176, 177, 178, 179, 180, 181, 182, 183, 184, 185, 186, 187, 188, 189, 190, 191, 192, 193, 194, 195, 196, 197, 198, 199, 200, 201, 202, 203, 204, 205, 220, 279, 282, 284, 285 };

        public InteractiveObject(Map map, InteractiveSpawn spawn)
        {
            Spawn = spawn;
            Position = new ObjectPosition(map, spawn.CellId);
            CharacterCanSee = new List<Character>();

            GenerateSkills();
        }

        public InteractiveSpawn Spawn { get; }

        public List<Character> CharacterCanSee;

        public bool Animated => Spawn.Animated || m_skills.Values.Any(x => x is SkillHarvest);

        public InteractiveStateEnum State { get; set; }

        public override int Id
        {
            get { return Spawn.Id; }
            protected set { Spawn.Id = value; }
        }

        public int InstId { get; set; }

        /// <summary>
        /// Can be null
        /// </summary>
        public InteractiveTemplate Template => Spawn.Template;

        public override bool CanBeSee(WorldObject byObj)
        {
            if (!Map.IsInstantiated || CharacterCanSee.Count == 0)
                return base.CanBeSee(byObj);

            return base.CanBeSee(byObj) && CharacterCanSee.Contains(byObj as Character);
        }

        public void SetInteractiveState(InteractiveStateEnum state)
        {
            if (state == InteractiveStateEnum.STATE_ANIMATED && !Animated)
                return;

            State = state;

            InteractiveHandler.SendStatedElementUpdatedMessage(Map.Clients, Id, Cell.Id, (int)State);
        }

        void GenerateSkills()
        {
            foreach (var skillRecord in Spawn.GetSkills())
            {
                try
                {
                    var id = InteractiveManager.Instance.PopSkillId();
                    var skill = skillRecord.GenerateSkill(id, this);

                    m_skills.Add(id, skill);
                }
                catch (Exception ex)
                {
                    //logger.Error($"Cannot generate skills of spawn {Spawn.Id} interactive ({Spawn.Template}) : {ex.Message}");
                }
            }
        }

        public Skill GetSkill(int id)
        {
            Skill result;
            return !m_skills.TryGetValue(id, out result) ? null : result;
        }

        public IEnumerable<Skill> GetSkills() => m_skills.Values;

        public IEnumerable<Skill> GetEnabledSkills(Character character) => m_skills.Values.Where(entry => entry.IsEnabled(character));

        public IEnumerable<Skill> GetDisabledSkills(Character character) => m_skills.Values.Where(entry => entry is SkillHarvest && !entry.IsEnabled(character));

        public StatedElement GetStatedElement() => new StatedElement(Id, (ushort)Cell.Id, (uint)State, true);

        public IEnumerable<InteractiveElementSkill> GetDisabledElementSkills(Character character) => m_skills.Values.Where(entry => entry is SkillHarvest && !entry.IsEnabled(character) && entry.SkillTemplate.ClientDisplay).Select(entry => entry.GetInteractiveElementSkill());

        public IEnumerable<InteractiveElementSkill> GetEnabledElementSkills(Character character)
        {
            var enabledSkills = m_skills.Values.Where(entry => entry.IsEnabled(character) && entry.SkillTemplate.ClientDisplay).Select(entry => entry.GetInteractiveElementSkill());

            if (Template?.Id > 0 && _removeReferenceHint.Contains(Template.Id))
            {
                var additionalSkills = new List<InteractiveElementSkill>
                {
                    new InteractiveElementSkill((int)SkillTemplateEnum.PANNEAU_DIRECTIONNEL_VIA_RÉFÉRENCE_HINT, Id), //[!] Panneau directionnel : via référence hint
                    new InteractiveElementSkill((int)SkillTemplateEnum.BASE_339, Id), //Indicar uma saída
                };

                enabledSkills = enabledSkills.Concat(additionalSkills);
            }

            return enabledSkills;
        }

        public InteractiveElement GetInteractiveElement(Character character)
        {
            return new InteractiveElement(
                elementId: Id,
                elementTypeId: Template?.Id ?? -1,
                enabledSkills: GetEnabledElementSkills(character).ToArray(),
                disabledSkills: GetDisabledElementSkills(character).ToArray(),
                onCurrentMap: true);
        }

        public InteractiveElement GetInteractiveElementWithBonus(Character character)
        {
            if (m_skills.Values.Any(x => x is ISkillWithAgeBonus))
            {
                return new InteractiveElementWithAgeBonus(
                    elementId: Id,
                    elementTypeId: Template?.Id ?? -1,
                    enabledSkills: GetEnabledElementSkills(character).ToArray(),
                    disabledSkills: GetDisabledElementSkills(character).ToArray(),
                    onCurrentMap: true,
                    ageBonus: m_skills.Values.OfType<ISkillWithAgeBonus>().Max(x => x.AgeBonus));
            }

            return new InteractiveElement(
                elementId: Id,
                elementTypeId: Template?.Id ?? -1,
                enabledSkills: GetEnabledElementSkills(character).ToArray(),
                disabledSkills: GetDisabledElementSkills(character).ToArray(),
                onCurrentMap: true);
        }
    }
}