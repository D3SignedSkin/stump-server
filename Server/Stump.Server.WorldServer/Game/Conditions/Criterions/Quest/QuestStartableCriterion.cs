using System;
using System.Linq;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;

namespace Stump.Server.WorldServer.Game.Conditions.Criterions
{
    public class QuestStartableCriterion : Criterion
    {
        public const string Identifier = "Qc";

        public int QuestId { get; set; }

        public override bool Eval(Character character)
        {
            switch (Operator)
            {
                case ComparaisonOperatorEnum.EQUALS:
                case ComparaisonOperatorEnum.SUPERIOR:
                    return character.Quests.Where(x => x.Id == QuestId).Count() > 0;
                case ComparaisonOperatorEnum.INEQUALS:
                    return character.Quests.Where(x => x.Id == QuestId).Count() == 0;
                default:
                    return false;
            }
        }

        public override void Build()
        {
            int questId;

            if (!int.TryParse(Literal, out questId))
                throw new Exception(string.Format("Cannot build QuestStartableCriterion, {0} is not a valid quest id", Literal));

            QuestId = questId;
        }

        public override string ToString()
        {
            return FormatToString(Identifier);
        }
    }
}