using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using System;

namespace Stump.Server.WorldServer.Game.Conditions.Criterions
{
    public class CreateDaysCriterion : Criterion
    {
        public const string Identifier = "CDA";

        public int Days
        {
            get;
            set;
        }

        public override bool Eval(Character character)
        {
            int CreateDate = DateTime.Now.Subtract(character.Account.CreationDate).Days;

            return Compare((int)CreateDate, (int)Days);
        }

        public override void Build()
        {
            int day;

            if (!int.TryParse(Literal, out day))
                throw new Exception(string.Format("Cannot build CreateDaysCriterion, {0} is not a valid days", Literal));

            Days = (int)day;
        }

        public override string ToString()
        {
            return FormatToString(Identifier);
        }
    }
}