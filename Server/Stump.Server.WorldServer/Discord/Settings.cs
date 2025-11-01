using System;
using Stump.Core.Attributes;

namespace Stump.Server.WorldServer
{
    [Serializable]
    public static class DiscordIntegration
    {
        //[Variable(true)]
        public static bool EnableDiscordWebHook = false;

        [Variable(true)]
        public static string DiscordChatKoliseu = "";

        [Variable(true)]
        public static string DiscordChatGlobalUrl = "";

        [Variable(true)]
        public static string DiscordChatVipUrl = "";

        [Variable(true)]
        public static string DiscordChatSallersUrl = "";

        [Variable(true)]
        public static string DiscordChatSeekUrl = "";

        [Variable(true)]
        public static string DiscordChatStaffUrl = "";

        [Variable(true)]
        public static string DiscordChatStaffLogsUrl = "";

        [Variable(true)]
        public static string DiscordWHUsername = "Hydra";

        [Variable(true)]
        public static string DiscordWHAvatarURL = "";
    }
}