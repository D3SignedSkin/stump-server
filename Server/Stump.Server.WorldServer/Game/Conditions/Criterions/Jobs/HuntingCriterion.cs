using System;
using System.Linq;
using MongoDB.Driver;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;

namespace Stump.Server.WorldServer.Game.Conditions.Criterions
{
    public class HuntingCriterion : Criterion
    {
        public const string Identifier = "CU";

        public int HuntingCount
        {
            get;
            set;
        }

        public override bool Eval(Character character)
        {
            var EffectHunting = character.Inventory.GetEquipedItems().Where(entry => entry.Effects.Exists(x => x.EffectId == EffectsEnum.Effect_795));
            bool result = false;

            switch (Operator)
            {
                case ComparaisonOperatorEnum.EQUALS:
                    result = EffectHunting.Count() == HuntingCount ? true : false;
                    return result;
                case ComparaisonOperatorEnum.SUPERIOR:
                    result = EffectHunting.Count() > HuntingCount ? true : false;
                    return result;
                case ComparaisonOperatorEnum.INFERIOR:
                    result = EffectHunting.Count() < HuntingCount ? true : false;
                    return result;
                default:
                    return false;
            }
        }

        public override void Build()
        {
            int Hunting;

            if (!int.TryParse(Literal, out Hunting))
                throw new Exception(string.Format("Cannot build HuntingCriterion, {0} is not a valid Hunting", Literal));

            HuntingCount = Hunting;
        }

        public override string ToString()
        {
            return FormatToString(Identifier);
        }
    }
}