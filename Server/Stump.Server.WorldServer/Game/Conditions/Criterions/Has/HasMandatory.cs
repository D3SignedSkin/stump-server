using System;
using System.Linq;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;

namespace Stump.Server.WorldServer.Game.Conditions.Criterions
{
    public class HasMandatory : Criterion
    {
        public const string Identifier = "MDC";

        public int MandValue
        {
            get;
            set;
        }

        public override bool Eval(Character character)
        {
            int MandatoryId = 0;

            foreach (var Mandatory in character.MandatoryCollection.Mandatory.Where(Mandatory => Mandatory.MandatoryId == MandValue && Mandatory.OwnerId == character.Id && DateTime.Now <= Mandatory.Time))
            {
                if (Mandatory != null)
                    MandatoryId = Mandatory.MandatoryId;
                break;
            }

            return Operator == ComparaisonOperatorEnum.EQUALS ? MandatoryId == MandValue : MandatoryId != MandValue;
        }

        public override void Build()
        {
            short mandatory;

            if (!short.TryParse(Literal, out mandatory))
                throw new Exception(string.Format("Cannot build HasMandatory, {0} is not a valid Mandatory", Literal));

            MandValue = mandatory;
        }

        public override string ToString()
        {
            return FormatToString(Identifier);
        }
    }
}