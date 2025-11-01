namespace Stump.Server.WorldServer.Database.Startup
{
    public class StartupActionRecordRelator
    {
        public static string FetchQuery = "SELECT * FROM startup_actions LEFT JOIN characters_startup_actions_items_binds ON characters_startup_actions_items_binds.StartupActionId = startup_actions.Id WHERE characters_startup_actions_items_binds.OwnerId={0} AND characters_startup_actions_items_binds.active = 1";
        public static string FetchQueryItems = "SELECT * FROM startup_actions_items WHERE startup_actions_items.ActionId={0}";
        public static string FecthQuery_presentes = "SELECT * FROM characters_startup_actions_items_binds";
        public static string FecthQuery_console = "SELECT * FROM Command_to_console";
    }
}