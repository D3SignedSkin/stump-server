using System;
using System.Linq;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;

namespace Stump.Server.WorldServer.Game.Conditions.Criterions
{
    public class HasItemEquippedCriterion : Criterion
    {
        public const string Identifier = "POE";

        public int Item
        {
            get;
            set;
        }

        public override bool Eval(Character character)
        {
            return Operator == ComparaisonOperatorEnum.EQUALS ? character.Inventory.GetEquipedItems().Any(x => x.Template.Id == Item) : !character.Inventory.GetEquipedItems().Any(x => x.Template.Id == Item);
        }

        public override void Build()
        {
            int item;

            if (!int.TryParse(Literal, out item))
                throw new Exception(string.Format("Cannot build HasEquippedItem, {0} is not a valid item", Literal));

            Item = item;
        }

        public override string ToString()
        {
            return FormatToString(Identifier);
        }
    }
}