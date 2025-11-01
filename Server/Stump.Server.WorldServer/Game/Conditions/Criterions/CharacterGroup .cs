using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using System;

namespace Stump.Server.WorldServer.Game.Conditions.Criterions
{
    public class CharacterGroupCriterion : Criterion
    {
        public const string Identifier = "CTG";

        public bool CharGroup
        {
            get;
            set;
        }

        public override bool Eval(Character character) => character.IsInParty();

        public override void Build()
        {
            bool Group;

            if (!bool.TryParse(Literal, out Group))
                throw new Exception(string.Format("Cannot build CharacterGroupCriterion, {0} is not a valid Group", Literal));

            CharGroup = Group;
        }

        public override string ToString()
        {
            return FormatToString(Identifier);
        }
    }
}
