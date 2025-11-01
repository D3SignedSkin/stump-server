using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;

namespace Stump.Server.WorldServer.Game.Conditions.Criterions
{
    public class UnknownCriterion : Criterion
    {
        public const string Identifier = "Pc";
        public const string Identifier1 = "Ms";
        public const string Identifier2 = "Mw";
        public const string Identifier3 = "Pm";

        public override bool Eval(Character character)
        {
            return true;
        }

        public override void Build()
        {
        }

        public override string ToString()
        {
            return FormatToString(Identifier);
        }
    }
}
