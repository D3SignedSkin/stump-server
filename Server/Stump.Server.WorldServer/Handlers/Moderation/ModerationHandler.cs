using Stump.DofusProtocol.Messages;
using Stump.Server.BaseServer.Network;
using Stump.Server.WorldServer.Core.Network;

namespace Stump.Server.WorldServer.Handlers.Moderation
{
    public class ModerationHandler : WorldHandlerContainer
    {
        [WorldHandler(PopupWarningCloseRequestMessage.Id)]
        public static void HandlePopupWarningCloseRequestMessage(WorldClient client, PopupWarningCloseRequestMessage message)
        {
            if (!client.Connected)
                return;

            client.Send(new PopupWarningClosedMessage());
        }

        public static void SendPopupWarningMessage(IPacketReceiver client, string content, string author, sbyte lockDuration)
        {
            client.Send(new PopupWarningMessage((byte)lockDuration, author, content));
        }
    }
}