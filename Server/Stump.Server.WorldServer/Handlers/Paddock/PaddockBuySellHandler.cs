using Stump.DofusProtocol.Messages;
using Stump.Server.WorldServer.Core.Network;
using Stump.Server.WorldServer.Game.Dialogs.Paddock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stump.Server.WorldServer.Handlers.Paddock
{
    class PaddockBuySellHandler : WorldHandlerContainer
    {
        private PaddockBuySellHandler() { }
        [WorldHandler(PaddockBuyRequestMessage.Id)]
        public static void HandlePaddockBuyRequestMessage(WorldClient client, PaddockBuyRequestMessage message)
        {
            var PaddockBuySellPanel = client.Character.Dialog as PaddockBuySell;
            PaddockBuySellPanel?.BuyPaddock(message.proposedPrice);
        }
        [WorldHandler(PaddockSellRequestMessage.Id)]
        public static void HandlePaddockSellRequestMessage(WorldClient client, PaddockSellRequestMessage message)
        {
    
            var PaddockBuySellPanel = client.Character.Dialog as PaddockBuySell;
            
            PaddockBuySellPanel?.SellPaddock(message.price, message.forSale);
        }
    }
}
