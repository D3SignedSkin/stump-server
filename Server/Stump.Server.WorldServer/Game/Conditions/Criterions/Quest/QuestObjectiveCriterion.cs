using System;
using System.Linq;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;

namespace Stump.Server.WorldServer.Game.Conditions.Criterions
{
    public class QuestObjectiveCriterion : Criterion
    {
        public const string Identifier = "Qo";

        public int QuestObjectiveId { get; set; }

        public override bool Eval(Character character)
        {
            switch (Operator)
            {
                case ComparaisonOperatorEnum.EQUALS:
                    return character.Quests.Where(x => x.CurrentStep.Objectives.Any(y => y.ObjectiveRecord.ObjectiveId == QuestObjectiveId && y.Finished)).Count() == 0;
                case ComparaisonOperatorEnum.SUPERIOR:
                    return character.Quests.Where(x => x.CurrentStep.Objectives.Any(y => y.ObjectiveRecord.ObjectiveId == QuestObjectiveId && y.Finished)).Count() > 0;
                default:
                    return false;
            }
        }

        public override void Build()
        {
            int questObjectiveId;

            if (!int.TryParse(Literal, out questObjectiveId))
                throw new Exception(string.Format("Cannot build QuestActiveCriterion, {0} is not a valid quest objective id", Literal));

            QuestObjectiveId = questObjectiveId;
        }

        public override string ToString()
        {
            return FormatToString(Identifier);
        }
    }
}