using System;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;

namespace Stump.Server.WorldServer.Game.Conditions.Criterions
{
    public class RankCriterion : Criterion
    {
        public const string Identifier = "Pq";

        public RoleEnum Role
        {
            get;
            set;
        }

        public override bool Eval(Character character)
        {
            return Compare((int)character.UserGroup.Role, (int)Role);
        }

        public override void Build()
        {
            int role;

            if (!int.TryParse(Literal, out role))
                throw new Exception(string.Format("Cannot build RankCriterion, {0} is not a valid role", Literal));

            Role = (RoleEnum)role;
        }

        public override string ToString()
        {
            return FormatToString(Identifier);
        }
    }
}