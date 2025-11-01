using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;

namespace Stump.Server.WorldServer.Game.Conditions.Criterions
{
    public class GuildValidCriterion : Criterion
    {
        public const string Identifier = "Pw";

        public int Emote
        {
            get;
            set;
        }
        public override bool Eval(Character character) => character.Guild != null;

        public override void Build()
        {
        }

        public override string ToString() => FormatToString(Identifier);
    }
}