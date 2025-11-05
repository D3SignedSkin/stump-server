using System;

namespace Stump.Server.Tools.DatabaseManager.CommandLine
{
    public static class OptionReader
    {
        public static CommandLineInput Parse(string[] args)
        {
            if (args == null)
                args = Array.Empty<string>();

            var options = new OptionDictionary();
            string entity = null;
            string action = null;
            bool help = false;

            for (int i = 0; i < args.Length; i++)
            {
                var token = args[i];

                if (string.IsNullOrWhiteSpace(token))
                    continue;

                if (IsHelpToken(token))
                {
                    help = true;
                    continue;
                }

                if (IsOptionToken(token))
                {
                    ParseOption(options, token, args, ref i);
                    continue;
                }

                if (entity == null)
                {
                    entity = token;
                }
                else if (action == null)
                {
                    action = token;
                }
                else
                {
                    throw new OptionParsingException($"Unexpected argument '{token}'.");
                }
            }

            return new CommandLineInput(entity, action, options, help);
        }

        private static bool IsHelpToken(string token)
        {
            return string.Equals(token, "--help", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(token, "-h", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsOptionToken(string token)
        {
            return token.StartsWith("--", StringComparison.Ordinal) ||
                   (token.StartsWith("-", StringComparison.Ordinal) && token.Length > 1 && !string.Equals(token, "-", StringComparison.Ordinal));
        }

        private static void ParseOption(OptionDictionary options, string token, string[] args, ref int index)
        {
            string key;
            string value = null;

            if (token.StartsWith("--", StringComparison.Ordinal))
            {
                key = token.Substring(2);
            }
            else
            {
                key = token.Substring(1);
                if (string.Equals(key, "h", StringComparison.OrdinalIgnoreCase))
                {
                    // treat -h as help
                    return;
                }
            }

            if (string.IsNullOrEmpty(key))
                throw new OptionParsingException("Encountered option without a name.");

            var equalIndex = key.IndexOf('=');
            if (equalIndex >= 0)
            {
                value = key.Substring(equalIndex + 1);
                key = key.Substring(0, equalIndex);
            }
            else if (index + 1 < args.Length && !IsOptionToken(args[index + 1]))
            {
                value = args[++index];
            }
            else
            {
                value = "true";
            }

            options.Set(key, value);
        }
    }
}
