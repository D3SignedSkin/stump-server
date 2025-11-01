using Stump.DofusProtocol.Messages;
using Stump.Server.WorldServer.Core.Network;

namespace Stump.Server.WorldServer.Handlers.Progression
{
    public class HaapiShopHandler : WorldHandlerContainer
    {
        [WorldHandler(HaapiShopApiKeyRequestMessage.Id)]
        public static void HandleHaapiShopApiKeyRequestMessage(WorldClient client, HaapiShopApiKeyRequestMessage message)
        {
            //client.Send(new HaapiShopApiKeyMessage("aaaa"));
        }
    }
}