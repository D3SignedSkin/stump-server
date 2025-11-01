using System;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using System.Linq;

namespace Stump.Server.WorldServer.Game.Conditions.Criterions
{
    public class TimeDopplesCriterion : Criterion
    {
        public const string Identifier = "Dt";

        public int MonsterId
        {
            get;
            set;
        }

        public override bool Eval(Character character)
        {
            var compareTime = DateTime.Now;
            var matchingDopeul = character.DoppleCollection.Dopeul.LastOrDefault(dopeul => dopeul.DopeulId == MonsterId);

            if (matchingDopeul != null)
            {
                compareTime = matchingDopeul.Time;
            }
            else
            {
                return true;
            }

            switch (Operator)
            {
                case ComparaisonOperatorEnum.EQUALS:
                    return compareTime > DateTime.Now ? true : false;

                case ComparaisonOperatorEnum.INEQUALS:
                    return compareTime <= DateTime.Now ? true : false;

                default:
                    return true;
            }
        }

        public override void Build()
        {
            int monsterId;

            if (!int.TryParse(Literal, out monsterId))
                throw new Exception(string.Format("Cannot build TimeDopplesCriterion, {0} is not a valid monster id", Literal));

            MonsterId = monsterId;
        }

        public override string ToString()
        {
            return FormatToString(Identifier);
        }
    }
}