using System;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using System.Linq;
using System.Collections.Generic;

namespace Stump.Server.WorldServer.Game.Conditions.Criterions
{
    public class QuestNoAnyActiveCriterion : Criterion
    {
        public const string Identifier = "Qaa";
        private static readonly List<int> DoppleQuests = new List<int> { 470, 940, 715, 708, 469, 468, 467, 466, 465, 464, 463, 462, 461, 460, 459, 458, 1843, 1679, 1617 };

        public int QuestId
        {
            get;
            set;
        }

        public override bool Eval(Character character)
        {
            switch (Operator)
            {
                case ComparaisonOperatorEnum.EQUALS:
                    if (DoppleQuests.Contains(QuestId) && character.Quests.Where(x => x.Id == QuestId) == null)
                        return true;
                    else
                        return !character.Quests.Any(x => !x.Finished && x.Id == QuestId);
                default:
                    return false;
            }
        }

        public override void Build()
        {
            int questId;

            if (!int.TryParse(Literal, out questId))
                throw new Exception(string.Format("Cannot build QuestActiveCriterion, {0} is not a valid quest id", Literal));

            QuestId = questId;
        }

        public override string ToString()
        {
            return FormatToString(Identifier);
        }
    }
}