namespace Stump.Server.Tools.DatabaseManager.CommandLine
{
    public class CommandLineInput
    {
        public CommandLineInput(string entity, string action, OptionDictionary options, bool showHelp)
        {
            Entity = entity;
            Action = action;
            Options = options;
            ShowHelp = showHelp;
        }

        public string Entity { get; }

        public string Action { get; }

        public OptionDictionary Options { get; }

        public bool ShowHelp { get; }

        public bool HasCommand => !string.IsNullOrWhiteSpace(Entity) && !string.IsNullOrWhiteSpace(Action);
    }
}
