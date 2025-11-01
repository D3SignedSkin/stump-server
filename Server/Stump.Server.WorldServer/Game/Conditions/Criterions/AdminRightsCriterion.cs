using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using System;

namespace Stump.Server.WorldServer.Game.Conditions.Criterions
{
    public class AdminRightsCriterion : Criterion
    {
        public const string Identifier = "PX";

        public int Role
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
                    return Operator == ComparaisonOperatorEnum.EQUALS ? character.Account.UserGroupId == Role : character.Account.UserGroupId != Role;
                case ComparaisonOperatorEnum.INFERIOR:
                case ComparaisonOperatorEnum.SUPERIOR:
                    return Compare(character.Account.UserGroupId, Role);
                default:
                    return false;
            }
        }

        public override void Build()
        {
            int role;

            if (!int.TryParse(Literal, out role))
                throw new Exception(string.Format("Cannot build AdminRightsCriterion, {0} is not a valid role", Literal));

            Role = role;
        }

        public override string ToString()
        {
            return FormatToString(Identifier);
        }
    }
}