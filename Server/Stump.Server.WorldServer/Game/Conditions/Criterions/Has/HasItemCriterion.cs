using NLog;
using System;
using System.Linq;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;

namespace Stump.Server.WorldServer.Game.Conditions.Criterions
{
    public class HasItemCriterion : Criterion
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        public const string Identifier = "PO";

        public int Item { get; set; }

        public int Quantity { get; set; }

        public override bool Eval(Character character)
        {
            if (Operator == ComparaisonOperatorEnum.EQUALS)
            {
                return Literal.Contains(",") ? character.Inventory.Any(entry => entry.Template.Id == Item && entry.Stack >= Quantity) : character.Inventory.Any(entry => entry.Template.Id == Item);
            }
            else if (Operator == ComparaisonOperatorEnum.INEQUALS)
            {
                return Literal.Contains(",") ? !character.Inventory.Any(entry => entry.Template.Id == Item && entry.Stack >= Quantity) : !character.Inventory.Any(entry => entry.Template.Id == Item);
            }
            else if (Operator == ComparaisonOperatorEnum.SUPERIOR)
            {
                return character.Inventory.Any(entry => entry.Template.Id == Item && entry.Stack > Quantity);
            }
            else if (Operator == ComparaisonOperatorEnum.INFERIOR)
            {
                return character.Inventory.Any(entry => entry.Template.Id == Item && entry.Stack < Quantity);
            }
            else
            {
                return false;
            }
        }

        public override void Build()
        {
            try
            {
                int itemId;
                int itemQuanty = 1;

                if (Literal.Contains(","))
                {
                    var split = Literal.Split(',');

                    if (split.Length != 2 || !int.TryParse(split[0], out itemId) || !int.TryParse(split[1], out itemQuanty))
                        throw new Exception(string.Format("Cannot build HasItemCriterion, {0} is not a valid item (format 'id, itemQuanty')", Literal));
                }
                else if (!int.TryParse(Literal, out itemId))
                {
                    throw new Exception(string.Format("Cannot build HasItemCriterion, {0} is not a valid item id", Literal));
                }

                Item = itemId;
                Quantity = itemQuanty;
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while building: " + ex.Message);
            }
        }

        public override string ToString()
        {
            return FormatToString(Identifier);
        }
    }
}