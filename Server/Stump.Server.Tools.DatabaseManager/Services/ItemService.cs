using System;
using System.Collections.Generic;
using System.Globalization;
using Stump.DofusProtocol.Enums;
using Stump.Server.Tools.DatabaseManager.CommandLine;
using Stump.Server.Tools.DatabaseManager.Utilities;
using Stump.Server.WorldServer.Database.Items.Templates;

namespace Stump.Server.Tools.DatabaseManager.Services
{
    public class ItemService : ServiceBase
    {
        public ItemService(Stump.ORM.Database database, OptionDictionary options)
            : base(database, options)
        {
        }

        public override ServiceResult Execute(string action)
        {
            switch (Normalize(action))
            {
                case "list":
                case "ls":
                    return List();
                case "show":
                case "get":
                    return Show();
                case "create":
                case "add":
                    return Create();
                case "update":
                case "edit":
                    return Update();
                case "delete":
                case "remove":
                    return Delete();
                default:
                    return ServiceResult.Failed($"Action '{action}' inconnue pour les items.");
            }
        }

        private ServiceResult List()
        {
            var limit = 50;
            if (Options.TryGetInt("limit", out var limitOption) && limitOption > 0)
                limit = limitOption;

            var conditions = new List<string>();
            var parameters = new List<object>();

            void AddCondition(string template, object value)
            {
                var placeholder = $"@{parameters.Count}";
                conditions.Add(template.Replace("{0}", placeholder));
                parameters.Add(value);
            }

            if (Options.TryGetInt("id", out var id))
                AddCondition("Id = {0}", id);

            if (Options.TryGetUInt("nameId", out var nameId))
                AddCondition("NameId = {0}", nameId);

            if (Options.TryGetUInt("typeId", out var typeId))
                AddCondition("TypeId = {0}", typeId);
            else if (Options.TryGetEnum<ItemTypeEnum>("type", out var typeEnum))
                AddCondition("TypeId = {0}", (uint)typeEnum);
            else if (Options.TryGetUInt("type", out var typeValue))
                AddCondition("TypeId = {0}", typeValue);

            if (Options.TryGetUInt("level-min", out var levelMin))
                AddCondition("Level >= {0}", levelMin);

            if (Options.TryGetUInt("level-max", out var levelMax))
                AddCondition("Level <= {0}", levelMax);

            if (Options.TryGetUIntArray("ids", out var ids) && ids.Length > 0)
            {
                var placeholders = new List<string>();
                foreach (var entry in ids)
                {
                    placeholders.Add($"@{parameters.Count}");
                    parameters.Add(entry);
                }

                conditions.Add($"Id IN ({string.Join(", ", placeholders)})");
            }

            var sql = ItemTemplateRelator.FetchQuery;
            if (conditions.Count > 0)
            {
                sql += " WHERE " + string.Join(" AND ", conditions);
            }

            sql += $" ORDER BY Id LIMIT {limit}";

            var items = Database.Fetch<ItemTemplate>(sql, parameters.ToArray());

            if (items.Count == 0)
            {
                WriteLine("Aucun item trouvé.");
                return ServiceResult.Ok();
            }

            var table = new ConsoleTable("Id", "NameId", "TypeId", "Level", "Price", "Exchangeable", "Usable");
            foreach (var item in items)
            {
                table.AddRow(item.Id, item.NameId, item.TypeId, item.Level, item.Price.ToString(CultureInfo.InvariantCulture),
                    item.Exchangeable ? "oui" : "non", item.Usable ? "oui" : "non");
            }

            table.Write(Console.Out);
            return ServiceResult.Ok();
        }

        private ServiceResult Show()
        {
            if (!Options.TryGetInt("id", out var id))
                return ServiceResult.Failed("L'option --id est obligatoire pour afficher un item.");

            var item = Database.SingleOrDefault<ItemTemplate>("SELECT * FROM items_templates WHERE Id=@0", id);
            if (item == null)
                return ServiceResult.Failed($"Aucun item avec l'identifiant {id}.");

            WriteLine($"Id : {item.Id}");
            WriteLine($"NameId : {item.NameId}");
            WriteLine($"DescriptionId : {item.DescriptionId}");
            WriteLine($"TypeId : {item.TypeId}");
            WriteLine($"Level : {item.Level}");
            WriteLine($"Price : {item.Price.ToString(CultureInfo.InvariantCulture)}");
            WriteLine($"RealWeight : {item.RealWeight}");
            WriteLine($"Exchangeable : {(item.Exchangeable ? "oui" : "non")}");
            WriteLine($"Usable : {(item.Usable ? "oui" : "non")}");
            WriteLine($"Targetable : {(item.Targetable ? "oui" : "non")}");
            WriteLine($"TwoHanded : {(item.TwoHanded ? "oui" : "non")}");
            WriteLine($"Criteria : {item.Criteria ?? "(aucun)"}");
            WriteLine($"CriteriaTarget : {item.CriteriaTarget ?? "(aucun)"}");
            WriteLine($"ItemSetId : {item.ItemSetId}");
            WriteLine($"AppearanceId : {item.AppearanceId}");
            WriteLine($"RecipeSlots : {item.RecipeSlots}");
            WriteLine($"SecretRecipe : {(item.SecretRecipe ? "oui" : "non")}");
            WriteLine($"BonusIsSecret : {(item.BonusIsSecret ? "oui" : "non")}");
            WriteLine($"ObjectIsDisplayOnWeb : {(item.ObjectIsDisplayOnWeb ? "oui" : "non")}");
            WriteLine($"NonUsableOnAnother : {(item.NonUsableOnAnother ? "oui" : "non")}");
            WriteLine($"Enhanceable : {(item.Enhanceable ? "oui" : "non")}");
            WriteLine($"HideEffects : {(item.HideEffects ? "oui" : "non")}");
            return ServiceResult.Ok();
        }

        private ServiceResult Create()
        {
            if (!Options.TryGetInt("id", out var id))
                return ServiceResult.Failed("L'option --id est obligatoire pour créer un item.");

            var existing = Database.SingleOrDefault<ItemTemplate>("SELECT * FROM items_templates WHERE Id=@0", id);
            if (existing != null)
                return ServiceResult.Failed($"Un item avec l'identifiant {id} existe déjà.");

            if (!Options.TryGetUInt("nameId", out var nameId))
                return ServiceResult.Failed("L'option --nameId est obligatoire pour créer un item.");

            if (!Options.TryGetUInt("descriptionId", out var descriptionId))
                return ServiceResult.Failed("L'option --descriptionId est obligatoire pour créer un item.");

            if (!TryParseType(out var typeId))
                return ServiceResult.Failed("Impossible de déterminer le type (--type ou --typeId).");

            if (!Options.TryGetUInt("level", out var level))
                return ServiceResult.Failed("L'option --level est obligatoire pour créer un item.");

            var template = new ItemTemplate
            {
                Id = id,
                NameId = nameId,
                DescriptionId = descriptionId,
                TypeId = typeId,
                Level = level,
                RealWeight = Options.TryGetUInt("weight", out var weight) ? weight : 0,
                IconId = Options.TryGetInt("iconId", out var iconId) ? iconId : 0,
                UseAnimationId = Options.TryGetInt("useAnimation", out var useAnimation) ? useAnimation : 0,
                Usable = Options.TryGetBool("usable", out var usable) && usable,
                Targetable = Options.TryGetBool("targetable", out var targetable) && targetable,
                Exchangeable = Options.TryGetBool("exchangeable", out var exchangeable) ? exchangeable : true,
                TwoHanded = Options.TryGetBool("twoHanded", out var twoHanded) && twoHanded,
                Etheral = Options.TryGetBool("etheral", out var etheral) && etheral,
                Cursed = Options.TryGetBool("cursed", out var cursed) && cursed,
                Price = Options.TryGetDouble("price", out var price) ? price : 0d,
                ItemSetId = Options.TryGetInt("itemSetId", out var itemSetId) ? itemSetId : 0,
                Criteria = Options.GetString("criteria"),
                CriteriaTarget = Options.GetString("criteriaTarget"),
                HideEffects = Options.TryGetBool("hideEffects", out var hideEffects) && hideEffects,
                Enhanceable = Options.TryGetBool("enhanceable", out var enhanceable) && enhanceable,
                NonUsableOnAnother = Options.TryGetBool("nonUsableOnAnother", out var nonUsableOnAnother) && nonUsableOnAnother,
                AppearanceId = Options.TryGetUInt("appearanceId", out var appearanceId) ? appearanceId : 0,
                SecretRecipe = Options.TryGetBool("secretRecipe", out var secretRecipe) && secretRecipe,
                RecipeSlots = Options.TryGetUInt("recipeSlots", out var recipeSlots) ? recipeSlots : 0,
                BonusIsSecret = Options.TryGetBool("bonusSecret", out var bonusSecret) && bonusSecret
            };

            var displayOption = GetBoolOption("displayOnWeb", "displayWeb");
            if (displayOption.HasValue)
                template.ObjectIsDisplayOnWeb = displayOption.Value;

            Database.Insert(template);
            return ServiceResult.Ok($"Item {id} créé avec succès.");
        }

        private ServiceResult Update()
        {
            if (!Options.TryGetInt("id", out var id))
                return ServiceResult.Failed("L'option --id est obligatoire pour modifier un item.");

            var item = Database.SingleOrDefault<ItemTemplate>("SELECT * FROM items_templates WHERE Id=@0", id);
            if (item == null)
                return ServiceResult.Failed($"Aucun item avec l'identifiant {id}.");

            if (Options.TryGetUInt("nameId", out var nameId))
                item.NameId = nameId;

            if (Options.TryGetUInt("descriptionId", out var descriptionId))
                item.DescriptionId = descriptionId;

            if (TryParseType(out var typeId))
                item.TypeId = typeId;

            if (Options.TryGetUInt("level", out var level))
                item.Level = level;

            if (Options.TryGetUInt("weight", out var weight))
                item.RealWeight = weight;

            if (Options.TryGetInt("iconId", out var iconId))
                item.IconId = iconId;

            if (Options.TryGetDouble("price", out var price))
                item.Price = price;

            if (Options.TryGetInt("useAnimation", out var useAnimation))
                item.UseAnimationId = useAnimation;

            if (Options.TryGetBool("usable", out var usable))
                item.Usable = usable;

            if (Options.TryGetBool("targetable", out var targetable))
                item.Targetable = targetable;

            if (Options.TryGetBool("exchangeable", out var exchangeable))
                item.Exchangeable = exchangeable;

            if (Options.TryGetBool("twoHanded", out var twoHanded))
                item.TwoHanded = twoHanded;

            if (Options.TryGetBool("etheral", out var etheral))
                item.Etheral = etheral;

            if (Options.TryGetBool("cursed", out var cursed))
                item.Cursed = cursed;

            if (Options.TryGetInt("itemSetId", out var itemSetId))
                item.ItemSetId = itemSetId;

            if (Options.TryGetBool("hideEffects", out var hideEffects))
                item.HideEffects = hideEffects;

            if (Options.TryGetBool("enhanceable", out var enhanceable))
                item.Enhanceable = enhanceable;

            if (Options.TryGetBool("nonUsableOnAnother", out var nonUsableOnAnother))
                item.NonUsableOnAnother = nonUsableOnAnother;

            if (Options.TryGetBool("secretRecipe", out var secretRecipe))
                item.SecretRecipe = secretRecipe;

            if (Options.TryGetUInt("recipeSlots", out var recipeSlots))
                item.RecipeSlots = recipeSlots;

            var displayOpt = GetBoolOption("displayOnWeb", "displayWeb");
            if (displayOpt.HasValue)
                item.ObjectIsDisplayOnWeb = displayOpt.Value;

            if (Options.TryGetBool("bonusSecret", out var bonusSecret))
                item.BonusIsSecret = bonusSecret;

            var criteria = Options.GetString("criteria");
            if (criteria != null)
                item.Criteria = string.IsNullOrWhiteSpace(criteria) ? null : criteria;

            var criteriaTarget = Options.GetString("criteriaTarget");
            if (criteriaTarget != null)
                item.CriteriaTarget = string.IsNullOrWhiteSpace(criteriaTarget) ? null : criteriaTarget;

            if (Options.TryGetUInt("appearanceId", out var appearanceId))
                item.AppearanceId = appearanceId;

            Database.Update(item);
            return ServiceResult.Ok($"Item {id} mis à jour.");
        }

        private ServiceResult Delete()
        {
            if (!Options.TryGetInt("id", out var id))
                return ServiceResult.Failed("L'option --id est obligatoire pour supprimer un item.");

            var item = Database.SingleOrDefault<ItemTemplate>("SELECT * FROM items_templates WHERE Id=@0", id);
            if (item == null)
                return ServiceResult.Failed($"Aucun item avec l'identifiant {id}.");

            Database.Delete(item);
            return ServiceResult.Ok($"Item {id} supprimé.");
        }

        private bool TryParseType(out uint typeId)
        {
            typeId = 0;
            if (Options.TryGetUInt("typeId", out var explicitType))
            {
                typeId = explicitType;
                return true;
            }

            if (Options.TryGetEnum<ItemTypeEnum>("type", out var enumType))
            {
                typeId = (uint)enumType;
                return true;
            }

            if (Options.TryGetUInt("type", out var numericType))
            {
                typeId = numericType;
                return true;
            }

            return false;
        }

    }
}
