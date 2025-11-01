using System;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;

namespace Stump.Server.WorldServer.Game.Conditions.Criterions
{
    public class HasSpellCriterion : Criterion
    {
        public const string Identifier = "SPELL";

        public short Spellid
        {
            get;
            set;
        }

        public override bool Eval(Character character)
            => Operator == ComparaisonOperatorEnum.EQUALS ? character.Spells.HasSpell(Spellid) : !character.Spells.HasSpell(Spellid);

        public override void Build()
        {
            short spells;

            if (!short.TryParse(Literal, out spells))
                throw new Exception(string.Format("Cannot build HasSpellsCriterion, {0} is not a valid Spells", Literal));

            Spellid = spells;
        }

        public override string ToString()
        {
            return FormatToString(Identifier);
        }
    }
}
