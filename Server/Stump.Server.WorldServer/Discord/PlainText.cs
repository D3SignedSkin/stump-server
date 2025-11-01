using NLog;
using System;
using System.Collections.Specialized;
using System.Net;

namespace Stump.Server.WorldServer.Discord
{
    internal class PlainText
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static void SendWebHook(string Url, string msg, string Username)
        {
            //if (Url is null)
            //    return;

            //if (WorldServer.Host.Equals("127.0.0.1"))
            //{
            //    Console.WriteLine("SendWebHook Error: Sending webhook on a local machine is not allowed.");
            //    return;
            //}

            //Post(Url, new NameValueCollection()
            //{
            //    {
            //        "username",
            //        Username
            //    },
            //    {
            //        "avatar_url",
            //        DiscordIntegration.DiscordWHAvatarURL
            //    },
            //    {
            //        "content",
            //        msg
            //    }
            //});
        }

        public static byte[] Post(string url, NameValueCollection pairs)
        {
            try
            {
                if (url is null)
                    return null;

                using (WebClient webClient = new WebClient())
                {
                    return webClient.UploadValues(url, pairs);
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Erro ao enviar webhook para '{url}': {ex.Message}");
                return null;
            }
        }
    }
}