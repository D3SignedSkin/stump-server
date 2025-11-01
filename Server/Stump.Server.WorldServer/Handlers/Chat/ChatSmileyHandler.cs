using Stump.DofusProtocol.Enums.Custom;
using Stump.DofusProtocol.Messages;
using Stump.Server.BaseServer.Network;
using Stump.Server.WorldServer.Core.Network;
using Stump.Server.WorldServer.Game.Actors;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using System.Linq;

namespace Stump.Server.WorldServer.Handlers.Chat
{
    public partial class ChatHandler
    {
        [WorldHandler(ChatSmileyRequestMessage.Id)]
        public static void HandleChatSmileyRequestMessage(WorldClient client, ChatSmileyRequestMessage message)
        {
            client.Character.DisplaySmiley((short)message.smileyId);
        }

        [WorldHandler(MoodSmileyRequestMessage.Id)]
        public static void HandleMoodSmileyRequestMessage(WorldClient client, MoodSmileyRequestMessage message)
        {
            client.Character.SetMood((short)message.smileyId);
        }

        public static void SendChatSmileyMessage(IPacketReceiver client, Character character, short smileyId)
        {
            client.Send(new ChatSmileyMessage(
                            character.Id,
                            (ushort)smileyId,
                            character.Account.Id));
        }

        public static void SendChatSmileyMessage(IPacketReceiver client, ContextActor entity, short smileyId)
        {
            client.Send(new ChatSmileyMessage(
                            entity.Id,
                            (ushort)smileyId,
                            0));
        }

        public static void SendChatSmileyExtraPackListMessage(IPacketReceiver client, SmileyPacksEnum[] smileyPacks)
        {
            client.Send(new ChatSmileyExtraPackListMessage(smileyPacks.Select(x => (sbyte)x)));
        }

        public static void SendMoodSmileyResultMessage(IPacketReceiver client, sbyte result, short smileyId)
        {
            client.Send(new MoodSmileyResultMessage(result, (ushort)smileyId));
        }
    }
}