using System;
using System.Linq;
using System.Collections.Generic;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;

namespace Stump.Server.WorldServer.Game.Conditions.Criterions
{
    public class QuestDoneCriterion : Criterion
    {
        public const string Identifier = "Qf";
        private static readonly List<int> DoppleQuests = new List<int> { 470, 940, 715, 708, 469, 468, 467, 466, 465, 464, 463, 462, 461, 460, 459, 458, 1843, 1679, 1617 };

        public int QuestId { get; set; }

        public override bool Eval(Character character)
        {
            if (DoppleQuests.Contains(QuestId) && !character.Quests.Any(x => x.Id == QuestId))
                return true;

            switch (Operator)
            {
                case ComparaisonOperatorEnum.EQUALS:
                case ComparaisonOperatorEnum.SUPERIOR:
                    return character.Quests.Where(x => x.Finished && x.Id == QuestId).Count() > 0;
                default:
                    return false;
            }
        }

        public override void Build()
        {
            int questId;

            if (!int.TryParse(Literal, out questId))
                throw new Exception(string.Format("Cannot build QuestDoneCriterion, {0} is not a valid quest id", Literal));

            QuestId = questId;
        }

        public override string ToString()
        {
            return FormatToString(Identifier);
        }
    }
}