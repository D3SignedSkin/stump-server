using System;
using NLog;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;

namespace Stump.Server.WorldServer.Game.Conditions.Criterions
{
    public class DayCriterion : Criterion
    {
        public const string Identifier = "DC";

        public int Day
        {
            get;
            set;
        }

        public override bool Eval(Character character)
        {
                return Compare((int)DateTime.Now.DayOfWeek, Day);
        }

        public override void Build()
        {
            int day;
            if (!int.TryParse(Literal, out day))
                throw new Exception(string.Format("Cannot build DayCriterion, {0} is not a valid day", Literal));
            Day = day;
        }

        public override string ToString()
        {
            return FormatToString(Identifier);
        }
    }
}