namespace Game.Mandatory
{
    public class MandatoryRelator
    {
        public static string FetchByOwner = "SELECT * FROM characters_mandatory WHERE OwnerId={0}";
    }
}
