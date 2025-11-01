using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using System;
using System.Linq;

namespace Stump.Server.WorldServer.Game.Conditions.Criterions
{
    public class DungeonReturnCriterion : Criterion
    {
        public const string Identifier = "DGR";

        public int Value { get; set; }

        public override bool Eval(Character character)
        {
            switch (Operator)
            {
                case ComparaisonOperatorEnum.EQUALS:
                    bool result = character != null && character.DungeonReturn != null && character.DungeonReturn.Any(x => x != null && x[0] == Value);
                    return result;
                default:
                    return false;
            }
        }

        public override void Build()
        {
            int value;

            if (!int.TryParse(Literal, out value))
                throw new Exception(string.Format("Cannot build DungeonReturnCriterion, {0} is not a value", Literal));

            Value = value;
        }

        public override string ToString()
        {
            return FormatToString(Identifier);
        }
    }
}