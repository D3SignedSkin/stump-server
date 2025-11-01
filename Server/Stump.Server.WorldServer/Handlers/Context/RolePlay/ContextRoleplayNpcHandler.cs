using System.Collections.Generic;
using System.Linq;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Messages;
using Stump.Server.BaseServer.Network;
using Stump.Server.WorldServer.Core.Network;
using Stump.Server.WorldServer.Database.Npcs;
using Stump.Server.WorldServer.Game.Actors.Interfaces;
using Stump.Server.WorldServer.Game.Actors.RolePlay;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Npcs;
using Stump.Server.WorldServer.Game.Exchanges.BidHouse;

namespace Stump.Server.WorldServer.Handlers.Context.RolePlay
{
    public partial class ContextRoleplayHandler
    {
        [WorldHandler(NpcGenericActionRequestMessage.Id)]
        public static void HandleNpcGenericActionRequestMessage(WorldClient client, NpcGenericActionRequestMessage message)
        {
            if (client.Character.Dialog != null && (client.Character.Dialog is BidHouseExchange bidHouseExchange) && bidHouseExchange.Npc == null)
            {
                var bidbuy = new BidHouseExchange(client.Character, bidHouseExchange.Types, bidHouseExchange.MaxItemLevel, !bidHouseExchange.Buy);

                if (bidbuy != null)
                    bidbuy.Open();
            }
            else
            {
                var npc = client.Character.Map.GetActor<RolePlayActor>(message.npcId) as IInteractNpc;

                if (npc != null)
                    npc.InteractWith((NpcActionTypeEnum)message.npcActionId, client.Character);
            }
        }

        [WorldHandler(NpcDialogReplyMessage.Id)]
        public static void HandleNpcDialogReplyMessage(WorldClient client, NpcDialogReplyMessage message)
        {
            client.Character.ReplyToNpc(message.replyId);
        }

        public static void SendNpcDialogCreationMessage(IPacketReceiver client, Npc npc)
        {
            client.Send(new NpcDialogCreationMessage(npc.Position.Map.Id, npc.Id));
        }

        public static void SendNpcDialogQuestionMessage(IPacketReceiver client, NpcMessage message, IEnumerable<int> replies, params string[] parameters)
        {
            client.Send(new NpcDialogQuestionMessage((ushort)message.Id, parameters, replies.Select(x => (uint)x)));
        }
    }
}