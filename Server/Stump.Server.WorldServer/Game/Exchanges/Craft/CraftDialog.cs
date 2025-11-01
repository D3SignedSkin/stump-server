using System;
using System.Collections.Generic;
using System.Linq;
using Stump.Core.Extensions;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Messages;
using Stump.Server.WorldServer.Core.Network;
using Stump.Server.WorldServer.Database.Jobs;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Exchanges.Trades;
using Stump.Server.WorldServer.Game.Exchanges.Trades.Players;
using Stump.Server.WorldServer.Game.Interactives;
using Stump.Server.WorldServer.Game.Interactives.Skills;
using Stump.Server.WorldServer.Game.Items;
using Stump.Server.WorldServer.Game.Jobs;
using Stump.Server.WorldServer.Handlers.Inventory;

namespace Stump.Server.WorldServer.Game.Exchanges.Craft
{
    public abstract class CraftDialog : BaseCraftDialog
    {
        protected CraftDialog(InteractiveObject interactive, Skill skill, Job job) : base(interactive, skill, job)
        {
            Amount = 1;
        }

        public PlayerTradeItem SignatureRune
        {
            get;
            private set;
        }

        public bool ChangeRecipe(CraftingActor actor, RecipeRecord recipe)
        {
            if (recipe.JobId != Job.Id)
                return false;

            if (recipe.ResultLevel > Job.Level && Job.Id != (int)JobEnum.BASE)
                return false;

            bool valid = true;
            for (int i = 0; valid && i < recipe.Ingredients.Length; i++)
            {
                var item = actor.Character.Inventory.TryGetItem(recipe.Ingredients[i]);

                valid = item != null && actor.MoveItemToPanel(item, (int)recipe.Quantities[i]);
            }

            if (!valid)
                return false;

            ChangeAmount(1);
            return true;
        }


        public bool Craft()
        {
            var ingredients = GetIngredients().ToArray();
            var recipe = FindMatchingRecipe(ingredients);

            if (recipe == null)
            {
                InventoryHandler.SendExchangeCraftResultMessage(Clients, ExchangeCraftResultEnum.CRAFT_FAILED);
                return false;
            }

            if (recipe.ResultLevel > Job.Level && Job.Id != (int)JobEnum.BASE)
            {
                InventoryHandler.SendExchangeCraftResultWithObjectIdMessage(Clients, ExchangeCraftResultEnum.CRAFT_FAILED, recipe.ItemTemplate);
                return false;
            }

            if (ingredients.Any(x => x.Owner.Inventory[x.Guid]?.Stack < x.Stack * Amount))
            {
                InventoryHandler.SendExchangeCraftResultMessage(Clients, ExchangeCraftResultEnum.CRAFT_FAILED);
                return false;
            }

            foreach (var item in ingredients)
            {
                var playerItem = item.Owner.Inventory[item.Guid];

                item.Owner.Inventory.RemoveItem(playerItem, (int)item.Stack * Amount);

                if (playerItem.Template.Id != (int)ItemIdEnum.RUNE_DE_SIGNATURE_7508)
                {
                    if (item.Owner == Crafter.Character)
                        Crafter.MoveItem(item.Guid, 0);
                    else
                        Receiver.MoveItem(item.Guid, 0);
                }
                else
                {
                    var playerItemTrade = item as PlayerTradeItem;

                    if (item.Template.Id == (int)ItemIdEnum.RUNE_DE_SIGNATURE_7508)
                    {
                        SignatureRune = playerItemTrade.Stack > 0 ? playerItemTrade : null;
                    }
                }
            }

            #region Assinatura de Criação de Items by:Kenshin
            var signature = SignatureRune;

            if (signature != null && signature.Stack != 0)
            {
                signature.Owner.Inventory.RemoveItem(signature.PlayerItem, 1);

                if (signature.Owner.Id == Crafter.Id)
                {
                    Crafter.MoveItem((int)(uint)signature.Guid, -1);
                }
                else
                {
                    if (signature.Stack <= 1)
                    {
                        signature.Stack = 0;
                        signature.Owner.Inventory.RemoveItem(signature.PlayerItem);
                    }
                    else
                    {
                        signature.Stack -= 1;
                    }
                }

                if (recipe.ItemTemplate.Effects.Exists(x => x.EffectId == EffectsEnum.Effect_988))
                {
                    recipe.ItemTemplate.Effects.RemoveAll(x => x.EffectId == EffectsEnum.Effect_988);
                    recipe.ItemTemplate.Effects.Add(new EffectString(EffectsEnum.Effect_988, Crafter.Character.Namedefault));
                }
                else
                {
                    recipe.ItemTemplate.Effects.Add(new EffectString(EffectsEnum.Effect_988, Crafter.Character.Namedefault));
                }
            }
            #endregion

            var xp = Job.GetCraftXp(recipe, Amount);

            if (xp > 0)
                Job.Experience += xp;

            if (!ItemManager.Instance.HasToBeGenerated(recipe.ItemTemplate))
            {
                var createdItem = Receiver.Character.Inventory.AddItem(recipe.ItemTemplate, Amount);

                Receiver.Character.OnCraftItem(createdItem, Amount);
                InventoryHandler.SendExchangeCraftResultWithObjectDescMessage(Clients, ExchangeCraftResultEnum.CRAFT_SUCCESS, createdItem, Amount);
                InventoryHandler.SendExchangeCraftInformationObjectMessage(Crafter.Character.Map.Clients, createdItem, ExchangeCraftResultEnum.CRAFT_SUCCESS);

                // MISC
                JobManager.Instance.RegisterCraft(recipe.ItemTemplate.Id, Amount, Crafter.Character.Id);
            }
            else
            {
                var dict = new Dictionary<List<EffectBase>, int>(new EffectsListComparer());

                for (int i = 0; i < Amount; i++)
                {
                    //Crafts Items Full Players by: Kenshin
                    Random rnd = new Random();
                    var percent = rnd.Next(0, 100);
                    int percentWin = 0;

                    if (Crafter.Character.UserGroup.Role <= RoleEnum.Player)
                        percentWin = 5;
                    else if (Crafter.Character.UserGroup.Role == RoleEnum.Vip)
                        percentWin = 15;
                    else if (Crafter.Character.UserGroup.Role >= RoleEnum.Gold_Vip)
                        percentWin = 25;
                    else
                        percentWin = 0;

                    List<EffectBase> effects;

                    if (percent <= percentWin)
                        effects = ItemManager.Instance.GenerateItemEffects(recipe.ItemTemplate, true);
                    else
                        effects = ItemManager.Instance.GenerateItemEffects(recipe.ItemTemplate);


                    if (dict.ContainsKey(effects))
                        dict[effects] += 1;
                    else
                        dict.Add(effects, 1);
                }

                foreach (var keyPair in dict)
                {
                    var createdItem = Receiver.Character.Inventory.AddItem(recipe.ItemTemplate, keyPair.Key, keyPair.Value);

                    Receiver.Character.OnCraftItem(createdItem, keyPair.Value);
                    InventoryHandler.SendExchangeCraftResultWithObjectDescMessage(Clients, ExchangeCraftResultEnum.CRAFT_SUCCESS, createdItem, keyPair.Value);
                    InventoryHandler.SendExchangeCraftInformationObjectMessage(Crafter.Character.Map.Clients, createdItem, ExchangeCraftResultEnum.CRAFT_SUCCESS);

                    // MISC
                    JobManager.Instance.RegisterCraft(recipe.ItemTemplate.Id, Amount, Crafter.Character.Id);
                }
            }

            var timecraft = DateTime.Now.AddSeconds(1).GetUnixTimeStampLong();

            Crafter.Character.Map.Clients.Send(new GameRolePlayDelayedObjectUseMessage(Crafter.Character.Id, (sbyte)DelayedActionTypeEnum.DELAYED_ACTION_OBJECT_USE, timecraft, (ushort)recipe.ItemTemplate.Id));
            Crafter.Character.Map.Clients.Send(new GameRolePlayDelayedActionFinishedMessage(Crafter.Character.Id, (sbyte)DelayedActionTypeEnum.DELAYED_ACTION_OBJECT_USE));

            ChangeAmount(1);

            return true;
        }

        protected virtual RecipeRecord FindMatchingRecipe(PlayerTradeItem[] ingredients)
        {
            var combinedIngredients = new Dictionary<int, uint>();

            foreach (var ingredient in ingredients)
            {
                if (ingredient.Template.Id != (int)ItemIdEnum.RUNE_DE_SIGNATURE_7508)
                {
                    if (combinedIngredients.ContainsKey(ingredient.Template.Id))
                        combinedIngredients[ingredient.Template.Id] += ingredient.Stack;
                    else
                        combinedIngredients.Add(ingredient.Template.Id, ingredient.Stack);
                }
            }

            return (from recipe in Skill.SkillTemplate.Recipes
                    where recipe.IngredientIds.Length == combinedIngredients.Count
                    let valid = !(from item in combinedIngredients
                                  let index = Array.IndexOf(recipe.IngredientIds, item.Key)
                                  where index < 0 || recipe.Quantities[index] != item.Value
                                  select item).Any()
                    where valid
                    select recipe).FirstOrDefault();
        }

        protected virtual IEnumerable<PlayerTradeItem> GetIngredients()
        {
            return Receiver.Items.Concat(Crafter.Items).OfType<PlayerTradeItem>();
        }

    }
}