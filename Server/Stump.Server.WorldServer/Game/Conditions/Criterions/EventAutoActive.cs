using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using System;

namespace Stump.Server.WorldServer.Game.Conditions.Criterions
{
    public class EventAutoActiveCriterion : Criterion
    {
        public const string Identifier = "EAOA";

        public bool Active
        {
            get;
            set;
        }
        public override bool Eval(Character character)
        {
            return Compare(Settings.AutoMapEvent, Active);
        }
        public override void Build()
        {
            bool PassActive;

            if (!bool.TryParse(Literal, out PassActive))
                throw new Exception(string.Format("Cannot build EventActiveCriterion, {0} is not a valid bool", Literal));

            Active = PassActive;
        }

        public override string ToString()
        {
            return FormatToString(Identifier);
        }
    }
}
