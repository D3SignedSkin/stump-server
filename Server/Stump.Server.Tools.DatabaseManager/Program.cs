using System;
using Stump.ORM;
using Stump.Server.BaseServer.Database;
using Stump.Server.Tools.DatabaseManager.CommandLine;
using Stump.Server.Tools.DatabaseManager.Services;
using Stump.Server.WorldServer.Database.Items.Templates;

namespace Stump.Server.Tools.DatabaseManager
{
    internal static class Program
    {
        private static readonly string[] ConfigurationKeys = { "host", "port", "user", "password", "database", "provider" };

        private static int Main(string[] args)
        {
            try
            {
                var input = OptionReader.Parse(args);

                if (input.ShowHelp)
                {
                    PrintHelp();
                    return 0;
                }

                if (string.IsNullOrWhiteSpace(input.Entity) || string.IsNullOrWhiteSpace(input.Action))
                {
                    PrintHelp();
                    return 1;
                }

                var configuration = BuildConfiguration(input.Options);
                RemoveConfigurationOptions(input.Options);

                DatabaseAccessor accessor = null;
                try
                {
                    accessor = CreateAccessor(configuration);

                    var service = CreateService(input.Entity, accessor.Database, input.Options);
                    if (service == null)
                    {
                        Console.Error.WriteLine($"Entité '{input.Entity}' inconnue. Utilisez items, monsters ou challenges.");
                        return 1;
                    }

                    var result = service.Execute(input.Action);
                    if (!result.Success)
                    {
                        if (!string.IsNullOrEmpty(result.Message))
                            Console.Error.WriteLine(result.Message);
                        return 1;
                    }

                    if (!string.IsNullOrEmpty(result.Message))
                        Console.WriteLine(result.Message);

                    return 0;
                }
                finally
                {
                    accessor?.CloseConnection();
                }
            }
            catch (OptionParsingException ex)
            {
                Console.Error.WriteLine(ex.Message);
                PrintHelp();
                return 1;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Erreur inattendue : " + ex.Message);
                return 1;
            }
        }

        private static DatabaseAccessor CreateAccessor(DatabaseConfiguration configuration)
        {
            var accessor = new DatabaseAccessor(configuration);
            accessor.RegisterMappingAssembly(typeof(ItemTemplate).Assembly);
            accessor.Initialize();
            accessor.OpenConnection();
            DataManager.DefaultDatabase = accessor.Database;
            return accessor;
        }

        private static DatabaseConfiguration BuildConfiguration(OptionDictionary options)
        {
            return new DatabaseConfiguration
            {
                Host = options.GetString("host", "localhost"),
                Port = options.GetString("port", "3306"),
                User = options.GetString("user", "root"),
                Password = options.GetString("password", string.Empty),
                DbName = options.GetString("database", "stump_world"),
                ProviderName = options.GetString("provider", "MySql.Data.MySqlClient")
            };
        }

        private static void RemoveConfigurationOptions(OptionDictionary options)
        {
            foreach (var key in ConfigurationKeys)
                options.Remove(key);
        }

        private static ServiceBase CreateService(string entity, Database database, OptionDictionary options)
        {
            switch (entity.ToLowerInvariant())
            {
                case "item":
                case "items":
                    return new ItemService(database, options);
                case "monster":
                case "monsters":
                    return new MonsterService(database, options);
                case "challenge":
                case "challenges":
                    return new ChallengeService(database, options);
                default:
                    return null;
            }
        }

        private static void PrintHelp()
        {
            Console.WriteLine("Outil de gestion de la base de données Stump");
            Console.WriteLine("Usage : stump-dbtool <entité> <action> [options]");
            Console.WriteLine("Entités : items, monsters, challenges");
            Console.WriteLine("Actions : list, show, create, update, delete");
            Console.WriteLine();
            Console.WriteLine("Options de connexion :");
            Console.WriteLine("  --host=<hôte>            (défaut : localhost)");
            Console.WriteLine("  --port=<port>            (défaut : 3306)");
            Console.WriteLine("  --user=<utilisateur>     (défaut : root)");
            Console.WriteLine("  --password=<mot de passe>");
            Console.WriteLine("  --database=<nom>         (défaut : stump_world)");
            Console.WriteLine("  --provider=<provider>    (défaut : MySql.Data.MySqlClient)");
            Console.WriteLine();
            Console.WriteLine("Exemples :");
            Console.WriteLine("  stump-dbtool items list --limit=20");
            Console.WriteLine("  stump-dbtool monsters update --id=123 --boss=true");
            Console.WriteLine("  stump-dbtool challenges create --id=10 --nameId=200 --descriptionId=201 --minBonus=20 --maxBonus=60");
            Console.WriteLine();
            Console.WriteLine("Ajoutez --help pour afficher cette aide.");
        }
    }
}
