using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;

namespace Stump.Server.WorldServer.Game.Conditions.Criterions
{
    public class VipCriterion : Criterion
    {
        public const string Identifier = "Vip";

        public int Emote
        {
            get;
            set;
        }
        public override bool Eval(Character character) => character.Vip != false;

        public override void Build()
        {
        }

        public override string ToString() => FormatToString(Identifier);
    }
}