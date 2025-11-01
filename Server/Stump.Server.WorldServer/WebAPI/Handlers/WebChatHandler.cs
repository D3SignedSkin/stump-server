namespace Stump.Server.WorldServer.WebAPI.Handlers
{
    public class WebChatHandler
    {
        public string OwnerName { get; set; }

        public int OwnerRole { get; set; }

        public int ChatChannelId { get; set; }

        public string Message { get; set; }

        public string WebTypeMessage => "WebChat";
    }
}