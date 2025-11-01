using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;

namespace Stump.Server.WorldServer.Game.Conditions.Criterions
{
    public class UnknownFalseCriterion : Criterion
    {
        public const string Identifier = "Mt";
        public const string Identifier1 = "DD";
        public const string Identifier2 = "Sv";
        public const string Identifier3 = "ST";

        public override bool Eval(Character character)
        {
            return false;
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
