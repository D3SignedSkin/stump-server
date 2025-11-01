using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using System;

namespace Stump.Server.WorldServer.Game.Conditions.Criterions
{
    public class EventNatawActiveCriterion : Criterion
    {
        public const string Identifier = "NATAW";

        public bool Active
        {
            get;
            set;
        }

        public override bool Eval(Character character)
        {
            bool Result = false;

            if (DateTime.Now >= Settings.StartNataw && DateTime.Now <= Settings.EndNataw)
                Result = true;

            return Compare(Result, Active);
        }
        public override void Build()
        {
            bool PassActive;

            if (!bool.TryParse(Literal, out PassActive))
                throw new Exception(string.Format("Cannot build EventNatawActiveCriterion, {0} is not a valid bool", Literal));

            Active = PassActive;
        }

        public override string ToString()
        {
            return FormatToString(Identifier);
        }
    }
}
