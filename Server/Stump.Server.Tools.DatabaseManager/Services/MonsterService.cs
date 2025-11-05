using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Stump.Server.Tools.DatabaseManager.CommandLine;
using Stump.Server.Tools.DatabaseManager.Utilities;
using Stump.Server.WorldServer.Database.Monsters;

namespace Stump.Server.Tools.DatabaseManager.Services
{
    public class MonsterService : ServiceBase
    {
        public MonsterService(Stump.ORM.Database database, OptionDictionary options)
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
                    return ServiceResult.Failed($"Action '{action}' inconnue pour les monstres.");
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

            if (Options.TryGetInt("race", out var race))
                AddCondition("Race = {0}", race);

            var bossOption = GetBoolOption("boss", "isBoss");
            if (bossOption.HasValue)
                AddCondition("IsBoss = {0}", bossOption.Value);

            var miniBossOption = GetBoolOption("miniBoss", "isMiniBoss");
            if (miniBossOption.HasValue)
                AddCondition("IsMiniBoss = {0}", miniBossOption.Value);

            var questOption = GetBoolOption("quest", "isQuestMonster");
            if (questOption.HasValue)
                AddCondition("isQuestMonster = {0}", questOption.Value);

            var summonOption = GetBoolOption("useSummonSlot");
            if (summonOption.HasValue)
                AddCondition("UseSummonSlot = {0}", summonOption.Value);

            var bombOption = GetBoolOption("useBombSlot");
            if (bombOption.HasValue)
                AddCondition("UseBombSlot = {0}", bombOption.Value);

            if (Options.TryGetInt("favoriteSubarea", out var favoriteSubarea))
                AddCondition("FavoriteSubareaId = {0}", favoriteSubarea);

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

            var sql = MonsterTemplateRelator.FetchQuery;
            if (conditions.Count > 0)
                sql += " WHERE " + string.Join(" AND ", conditions);

            sql += $" ORDER BY Id LIMIT {limit}";

            var monsters = Database.Fetch<MonsterTemplate>(sql, parameters.ToArray());
            if (monsters.Count == 0)
            {
                WriteLine("Aucun monstre trouvé.");
                return ServiceResult.Ok();
            }

            var table = new ConsoleTable("Id", "NameId", "Race", "Boss", "MiniBoss", "Quest", "SummonSlot", "Speed");
            foreach (var monster in monsters)
            {
                table.AddRow(monster.Id, monster.NameId, monster.Race, monster.IsBoss ? "oui" : "non",
                    monster.IsMiniBoss ? "oui" : "non", monster.isQuestMonster ? "oui" : "non",
                    monster.UseSummonSlot ? "oui" : "non", monster.SpeedAdjust.ToString(CultureInfo.InvariantCulture));
            }

            table.Write(Console.Out);
            return ServiceResult.Ok();
        }

        private ServiceResult Show()
        {
            if (!Options.TryGetInt("id", out var id))
                return ServiceResult.Failed("L'option --id est obligatoire pour afficher un monstre.");

            var monster = Database.SingleOrDefault<MonsterTemplate>("SELECT * FROM monsters_templates WHERE Id=@0", id);
            if (monster == null)
                return ServiceResult.Failed($"Aucun monstre avec l'identifiant {id}.");

            WriteLine($"Id : {monster.Id}");
            WriteLine($"NameId : {monster.NameId}");
            WriteLine($"Race : {monster.Race}");
            WriteLine($"GfxId : {monster.GfxId}");
            WriteLine($"Look : {monster.LookAsString}");
            WriteLine($"IsBoss : {(monster.IsBoss ? "oui" : "non")}");
            WriteLine($"IsMiniBoss : {(monster.IsMiniBoss ? "oui" : "non")}");
            WriteLine($"QuestMonster : {(monster.isQuestMonster ? "oui" : "non")}");
            WriteLine($"UseSummonSlot : {(monster.UseSummonSlot ? "oui" : "non")}");
            WriteLine($"UseBombSlot : {(monster.UseBombSlot ? "oui" : "non")}");
            WriteLine($"CanPlay : {(monster.CanPlay ? "oui" : "non")}");
            WriteLine($"CanTackle : {(monster.CanTackle ? "oui" : "non")}");
            WriteLine($"CanBePushed : {(monster.CanBePushed ? "oui" : "non")}");
            WriteLine($"CanBeCarried : {(monster.CanBeCarried ? "oui" : "non")}");
            WriteLine($"CanUsePortal : {(monster.CanUsePortal ? "oui" : "non")}");
            WriteLine($"CanSwitchPos : {(monster.CanSwitchPos ? "oui" : "non")}");
            WriteLine($"CanSwitchPosOnTarget : {(monster.CanSwitchPosOnTarget ? "oui" : "non")}");
            WriteLine($"FavoriteSubareaId : {monster.FavoriteSubareaId}");
            WriteLine($"correspondingMiniBossId : {monster.correspondingMiniBossId}");
            WriteLine($"SpeedAdjust : {monster.SpeedAdjust.ToString(CultureInfo.InvariantCulture)}");
            WriteLine($"CreatureBoneId : {monster.CreatureBoneId}");
            WriteLine($"AllIdolsDisabled : {(monster.AllIdolsDisabled ? "oui" : "non")}");
            WriteLine($"IncompatibleChallenges : {(monster.IncompatibleChallenges?.Count > 0 ? string.Join(", ", monster.IncompatibleChallenges) : "(aucune)")}");
            return ServiceResult.Ok();
        }

        private ServiceResult Create()
        {
            if (!Options.TryGetInt("id", out var id))
                return ServiceResult.Failed("L'option --id est obligatoire pour créer un monstre.");

            var existing = Database.SingleOrDefault<MonsterTemplate>("SELECT * FROM monsters_templates WHERE Id=@0", id);
            if (existing != null)
                return ServiceResult.Failed($"Un monstre avec l'identifiant {id} existe déjà.");

            if (!Options.TryGetUInt("nameId", out var nameId))
                return ServiceResult.Failed("L'option --nameId est obligatoire pour créer un monstre.");

            if (!Options.TryGetUInt("gfxId", out var gfxId))
                return ServiceResult.Failed("L'option --gfxId est obligatoire pour créer un monstre.");

            if (!Options.TryGetInt("race", out var race))
                return ServiceResult.Failed("L'option --race est obligatoire pour créer un monstre.");

            var monster = new MonsterTemplate
            {
                Id = id,
                NameId = nameId,
                GfxId = gfxId,
                Race = race,
                FavoriteSubareaId = Options.TryGetInt("favoriteSubarea", out var favorite) ? favorite : 0,
                correspondingMiniBossId = Options.TryGetUInt("correspondingMiniBossId", out var miniBossId) ? miniBossId : 0,
                SpeedAdjust = Options.TryGetDouble("speed", out var speed) ? speed : 0d,
                CreatureBoneId = Options.TryGetInt("creatureBone", out var bone) ? bone : 0
            };

            var look = Options.GetString("look");
            if (look != null)
                monster.LookAsString = look;

            ApplyMonsterBoolOptions(monster);
            ApplyIncompatibleChallenges(monster);

            Database.Insert(monster);
            return ServiceResult.Ok($"Monstre {id} créé avec succès.");
        }

        private ServiceResult Update()
        {
            if (!Options.TryGetInt("id", out var id))
                return ServiceResult.Failed("L'option --id est obligatoire pour modifier un monstre.");

            var monster = Database.SingleOrDefault<MonsterTemplate>("SELECT * FROM monsters_templates WHERE Id=@0", id);
            if (monster == null)
                return ServiceResult.Failed($"Aucun monstre avec l'identifiant {id}.");

            if (Options.TryGetUInt("nameId", out var nameId))
                monster.NameId = nameId;

            if (Options.TryGetUInt("gfxId", out var gfxId))
                monster.GfxId = gfxId;

            if (Options.TryGetInt("race", out var race))
                monster.Race = race;

            if (Options.TryGetInt("favoriteSubarea", out var favorite))
                monster.FavoriteSubareaId = favorite;

            if (Options.TryGetUInt("correspondingMiniBossId", out var miniBossId))
                monster.correspondingMiniBossId = miniBossId;

            if (Options.TryGetDouble("speed", out var speed))
                monster.SpeedAdjust = speed;

            if (Options.TryGetInt("creatureBone", out var bone))
                monster.CreatureBoneId = bone;

            var look = Options.GetString("look");
            if (look != null)
                monster.LookAsString = look;

            ApplyMonsterBoolOptions(monster);
            ApplyIncompatibleChallenges(monster);

            Database.Update(monster);
            return ServiceResult.Ok($"Monstre {id} mis à jour.");
        }

        private ServiceResult Delete()
        {
            if (!Options.TryGetInt("id", out var id))
                return ServiceResult.Failed("L'option --id est obligatoire pour supprimer un monstre.");

            var monster = Database.SingleOrDefault<MonsterTemplate>("SELECT * FROM monsters_templates WHERE Id=@0", id);
            if (monster == null)
                return ServiceResult.Failed($"Aucun monstre avec l'identifiant {id}.");

            Database.Delete(monster);
            return ServiceResult.Ok($"Monstre {id} supprimé.");
        }

        private void ApplyMonsterBoolOptions(MonsterTemplate monster)
        {
            var boolMappings = new Dictionary<string[], Action<bool>>
            {
                { new[] { "boss", "isBoss" }, value => monster.IsBoss = value },
                { new[] { "miniBoss", "isMiniBoss" }, value => monster.IsMiniBoss = value },
                { new[] { "quest", "isQuestMonster" }, value => monster.isQuestMonster = value },
                { new[] { "useSummonSlot" }, value => monster.UseSummonSlot = value },
                { new[] { "useBombSlot" }, value => monster.UseBombSlot = value },
                { new[] { "canPlay" }, value => monster.CanPlay = value },
                { new[] { "canTackle" }, value => monster.CanTackle = value },
                { new[] { "canBePushed" }, value => monster.CanBePushed = value },
                { new[] { "canBeCarried" }, value => monster.CanBeCarried = value },
                { new[] { "canUsePortal" }, value => monster.CanUsePortal = value },
                { new[] { "canSwitchPos" }, value => monster.CanSwitchPos = value },
                { new[] { "canSwitchPosOnTarget" }, value => monster.CanSwitchPosOnTarget = value },
                { new[] { "allIdolsDisabled" }, value => monster.AllIdolsDisabled = value }
            };

            foreach (var mapping in boolMappings)
            {
                var option = GetBoolOption(mapping.Key);
                if (option.HasValue)
                    mapping.Value(option.Value);
            }
        }

        private void ApplyIncompatibleChallenges(MonsterTemplate monster)
        {
            if (Options.TryGetUIntArray("incompatibleChallenges", out var challenges))
                monster.IncompatibleChallenges = challenges.ToList();
        }
    }
}
