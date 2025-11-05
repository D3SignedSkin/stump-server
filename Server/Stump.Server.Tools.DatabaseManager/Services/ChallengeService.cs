using System;
using System.Collections.Generic;
using Stump.Server.Tools.DatabaseManager.CommandLine;
using Stump.Server.Tools.DatabaseManager.Utilities;
using Stump.Server.WorldServer.Database.Challenges;

namespace Stump.Server.Tools.DatabaseManager.Services
{
    public class ChallengeService : ServiceBase
    {
        public ChallengeService(Stump.ORM.Database database, OptionDictionary options)
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
                    return ServiceResult.Failed($"Action '{action}' inconnue pour les challenges.");
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

            if (Options.TryGetUInt("descriptionId", out var descriptionId))
                AddCondition("DescriptionId = {0}", descriptionId);

            if (Options.TryGetInt("minBonus", out var minBonus))
                AddCondition("MinBonus >= {0}", minBonus);

            if (Options.TryGetInt("maxBonus", out var maxBonus))
                AddCondition("MaxBonus <= {0}", maxBonus);

            var hiddenOpt = GetBoolOption("hidden");
            if (hiddenOpt.HasValue)
                AddCondition("Hidden = {0}", hiddenOpt.Value);

            var dareOpt = GetBoolOption("dareAvailable", "dare");
            if (dareOpt.HasValue)
                AddCondition("DareAvailable = {0}", dareOpt.Value);

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

            var sql = ChallengeRelator.FetchQuery;
            if (conditions.Count > 0)
                sql += " WHERE " + string.Join(" AND ", conditions);

            sql += $" ORDER BY Id LIMIT {limit}";

            var challenges = Database.Fetch<ChallengeRecord>(sql, parameters.ToArray());
            if (challenges.Count == 0)
            {
                WriteLine("Aucun challenge trouvé.");
                return ServiceResult.Ok();
            }

            var table = new ConsoleTable("Id", "NameId", "MinBonus", "MaxBonus", "Hidden", "Dare");
            foreach (var challenge in challenges)
            {
                table.AddRow(challenge.Id, challenge.NameId, challenge.MinBonus, challenge.MaxBonus,
                    challenge.Hidden ? "oui" : "non", challenge.DareAvailable ? "oui" : "non");
            }

            table.Write(Console.Out);
            return ServiceResult.Ok();
        }

        private ServiceResult Show()
        {
            if (!Options.TryGetInt("id", out var id))
                return ServiceResult.Failed("L'option --id est obligatoire pour afficher un challenge.");

            var challenge = Database.SingleOrDefault<ChallengeRecord>("SELECT * FROM challenges WHERE Id=@0", id);
            if (challenge == null)
                return ServiceResult.Failed($"Aucun challenge avec l'identifiant {id}.");

            WriteLine($"Id : {challenge.Id}");
            WriteLine($"NameId : {challenge.NameId}");
            WriteLine($"DescriptionId : {challenge.DescriptionId}");
            WriteLine($"MinBonus : {challenge.MinBonus}");
            WriteLine($"MaxBonus : {challenge.MaxBonus}");
            WriteLine($"Hidden : {(challenge.Hidden ? "oui" : "non")}");
            WriteLine($"DareAvailable : {(challenge.DareAvailable ? "oui" : "non")}");
            WriteLine($"IncompatibleChallenges : {(challenge.IncompatibleChallenges != null && challenge.IncompatibleChallenges.Length > 0 ? string.Join(", ", challenge.IncompatibleChallenges) : "(aucune)")}");
            return ServiceResult.Ok();
        }

        private ServiceResult Create()
        {
            if (!Options.TryGetInt("id", out var id))
                return ServiceResult.Failed("L'option --id est obligatoire pour créer un challenge.");

            var existing = Database.SingleOrDefault<ChallengeRecord>("SELECT * FROM challenges WHERE Id=@0", id);
            if (existing != null)
                return ServiceResult.Failed($"Un challenge avec l'identifiant {id} existe déjà.");

            if (!Options.TryGetUInt("nameId", out var nameId))
                return ServiceResult.Failed("L'option --nameId est obligatoire pour créer un challenge.");

            if (!Options.TryGetUInt("descriptionId", out var descriptionId))
                return ServiceResult.Failed("L'option --descriptionId est obligatoire pour créer un challenge.");

            if (!Options.TryGetInt("minBonus", out var minBonus))
                return ServiceResult.Failed("L'option --minBonus est obligatoire pour créer un challenge.");

            if (!Options.TryGetInt("maxBonus", out var maxBonus))
                return ServiceResult.Failed("L'option --maxBonus est obligatoire pour créer un challenge.");

            var challenge = new ChallengeRecord
            {
                Id = id,
                NameId = nameId,
                DescriptionId = descriptionId,
                MinBonus = minBonus,
                MaxBonus = maxBonus,
                Hidden = GetBoolOption("hidden") ?? false,
                DareAvailable = GetBoolOption("dareAvailable", "dare") ?? false
            };

            ApplyIncompatibleChallenges(challenge);

            Database.Insert(challenge);
            return ServiceResult.Ok($"Challenge {id} créé avec succès.");
        }

        private ServiceResult Update()
        {
            if (!Options.TryGetInt("id", out var id))
                return ServiceResult.Failed("L'option --id est obligatoire pour modifier un challenge.");

            var challenge = Database.SingleOrDefault<ChallengeRecord>("SELECT * FROM challenges WHERE Id=@0", id);
            if (challenge == null)
                return ServiceResult.Failed($"Aucun challenge avec l'identifiant {id}.");

            if (Options.TryGetUInt("nameId", out var nameId))
                challenge.NameId = nameId;

            if (Options.TryGetUInt("descriptionId", out var descriptionId))
                challenge.DescriptionId = descriptionId;

            if (Options.TryGetInt("minBonus", out var minBonus))
                challenge.MinBonus = minBonus;

            if (Options.TryGetInt("maxBonus", out var maxBonus))
                challenge.MaxBonus = maxBonus;

            var hiddenOpt = GetBoolOption("hidden");
            if (hiddenOpt.HasValue)
                challenge.Hidden = hiddenOpt.Value;

            var dareOpt = GetBoolOption("dareAvailable", "dare");
            if (dareOpt.HasValue)
                challenge.DareAvailable = dareOpt.Value;

            ApplyIncompatibleChallenges(challenge);

            Database.Update(challenge);
            return ServiceResult.Ok($"Challenge {id} mis à jour.");
        }

        private ServiceResult Delete()
        {
            if (!Options.TryGetInt("id", out var id))
                return ServiceResult.Failed("L'option --id est obligatoire pour supprimer un challenge.");

            var challenge = Database.SingleOrDefault<ChallengeRecord>("SELECT * FROM challenges WHERE Id=@0", id);
            if (challenge == null)
                return ServiceResult.Failed($"Aucun challenge avec l'identifiant {id}.");

            Database.Delete(challenge);
            return ServiceResult.Ok($"Challenge {id} supprimé.");
        }

        private void ApplyIncompatibleChallenges(ChallengeRecord challenge)
        {
            if (Options.TryGetUIntArray("incompatibleChallenges", out var challenges))
                challenge.IncompatibleChallenges = challenges;
        }
    }
}
