using System;
using Stump.ORM;
using Stump.Server.Tools.DatabaseManager.CommandLine;

namespace Stump.Server.Tools.DatabaseManager.Services
{
    public abstract class ServiceBase
    {
        protected ServiceBase(Database database, OptionDictionary options)
        {
            Database = database ?? throw new ArgumentNullException(nameof(database));
            Options = options ?? throw new ArgumentNullException(nameof(options));
        }

        protected Database Database { get; }

        protected OptionDictionary Options { get; }

        protected void WriteLine(string message)
        {
            if (!string.IsNullOrEmpty(message))
                Console.WriteLine(message);
        }

        protected void WriteError(string message)
        {
            if (!string.IsNullOrEmpty(message))
                Console.Error.WriteLine(message);
        }

        protected static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToLowerInvariant();
        }

        protected bool? GetBoolOption(params string[] keys)
        {
            if (keys == null)
                return null;

            foreach (var key in keys)
            {
                if (Options.TryGetBool(key, out var value))
                    return value;
            }

            return null;
        }

        public abstract ServiceResult Execute(string action);
    }
}
