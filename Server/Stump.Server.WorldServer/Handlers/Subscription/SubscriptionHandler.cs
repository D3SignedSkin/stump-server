using Stump.DofusProtocol.Messages;
using Stump.Server.WorldServer.Core.Network;

namespace Stump.Server.WorldServer.Handlers.Subscription
{
    public partial class SubscriptionHandler : WorldHandlerContainer
    {
        public static void SubscriptionZoneMessage(WorldClient client, bool Active)
        {
            client.Send(new SubscriptionZoneMessage(Active));
        }

        public static void SubscriptionLimitationMessage(WorldClient client, sbyte Reason) //Use SubscriptionRequiredEnum
        { 
            client.Send(new SubscriptionLimitationMessage(Reason));
        }
    }
}