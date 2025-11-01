using System;
using System.Linq;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;

namespace Stump.Server.WorldServer.Game.Conditions.Criterions
{
    public class QuestObjectiveStepsCriterion : Criterion
    {
        public const string Identifier = "Qs";

        public int QuestStepId { get; set; }

        public override bool Eval(Character character)
        {
            switch (Operator)
            {
                case ComparaisonOperatorEnum.EQUALS:
                    return character.Quests.Where(x => x.CurrentStep.Objectives.Any(y => y.StepTemplate.Id == QuestStepId && !y.Finished)).Count() > 0;
                case ComparaisonOperatorEnum.INEQUALS:
                    return character.Quests.Where(x => x.CurrentStep.Objectives.Any(y => y.StepTemplate.Id == QuestStepId && y.Finished)).Count() == 0;
                default:
                    return false;
            }
        }

        public override void Build()
        {
            int questStepId;

            if (!int.TryParse(Literal, out questStepId))
                throw new Exception(string.Format("Cannot build QuestObjectiveStepsCriterion, {0} is not a valid quest step id", Literal));

            QuestStepId = questStepId;
        }

        public override string ToString()
        {
            return FormatToString(Identifier);
        }
    }
}