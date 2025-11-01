using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using System;

namespace Stump.Server.WorldServer.Game.Conditions.Criterions
{
    public class CharacterAscensionCriterion : Criterion
    {
        public const string Identifier = "CASC";

        public int AscensionBasicStair
        {
            get;
            set;
        }

        public override bool Eval(Character character)
        {
            return Compare((int)character.AscensionBasicStair, (int)AscensionBasicStair);
        }

        public override void Build()
        {
            int AscensionBasicStairCASC;

            if (!int.TryParse(Literal, out AscensionBasicStairCASC))
                throw new Exception(string.Format("Cannot build CharacterAscensionCriterion, {0} is not a valid Ascension", Literal));

            AscensionBasicStair = AscensionBasicStairCASC;
        }

        public override string ToString()
        {
            return FormatToString(Identifier);
        }
    }
}