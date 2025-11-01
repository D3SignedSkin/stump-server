using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using System;

namespace Stump.Server.WorldServer.Game.Conditions.Criterions
{
    public class OgrinesQuantCriterion : Criterion
    {
        public const string Identifier = "OGG";

        public int Value
        {
            get;
            set;
        }

        public override bool Eval(Character character)
        {
            switch (Operator)
            {
                case ComparaisonOperatorEnum.EQUALS:
                case ComparaisonOperatorEnum.INEQUALS:
                    return Operator == ComparaisonOperatorEnum.EQUALS ? character.Account.Tokens == Value : character.Account.Tokens != Value;
                case ComparaisonOperatorEnum.INFERIOR:
                case ComparaisonOperatorEnum.SUPERIOR:
                    return Compare(character.Account.Tokens, Value);
                default:
                    return false;
            }
        }

        public override void Build()
        {
            int value;

            if (!int.TryParse(Literal, out value))
                throw new Exception(string.Format("Cannot build OgrinesQuantCriterion, {0} is not a value", Literal));

            Value = value;
        }

        public override string ToString()
        {
            return FormatToString(Identifier);
        }
    }
}