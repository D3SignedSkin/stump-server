using NLog;
using Stump.DofusProtocol.Messages;
using Stump.DofusProtocol.Types;
using Stump.Server.WorldServer.Core.Network;
using System.Collections.Generic;

namespace Stump.Server.WorldServer.Handlers.Basic
{
    public class AnomalyHandler : WorldHandlerContainer
    {
        protected static readonly Logger logger = LogManager.GetCurrentClassLogger();

        [WorldHandler(AnomalySubareaInformationRequestMessage.Id)]
        public static void HandleAnomalySubareaInformationRequestMessage(WorldClient client, AnomalySubareaInformationRequestMessage message)
        {
            //List<AnomalySubareaInformation> subareas = new List<AnomalySubareaInformation>();

            //subareas.Add(new AnomalySubareaInformation
            //{
            //    subAreaId = 10,
            //    rewardRate = 100,
            //    hasAnomaly = true,
            //    anomalyClosingTime = 222
            //});

            //client.Send(new AnomalySubareaInformationResponseMessage(subareas));
        }

        public static void SendAnomalyStateMessage(WorldClient client)
        {
            client.Send(new AnomalyStateMessage(subAreaId: 10, open: true, closingTime: 222));
        }
    }
}