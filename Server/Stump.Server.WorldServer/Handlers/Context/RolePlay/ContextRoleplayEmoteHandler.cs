using System;
using System.Collections.Generic;
using System.Linq;
using Stump.Core.Extensions;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Messages;
using Stump.Server.BaseServer.Network;
using Stump.Server.WorldServer.Core.Network;
using Stump.Server.WorldServer.Game.Actors;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;

namespace Stump.Server.WorldServer.Handlers.Context.RolePlay
{
    public partial class ContextRoleplayHandler
    {
        [WorldHandler(EmotePlayRequestMessage.Id)]
        public static void HandleEmotePlayRequestMessage(WorldClient client, EmotePlayRequestMessage message)
        {
            if (!client.Character.HasEmote((EmotesEnum)message.emoteId))
                return;

            if (client.Character.IsMoving())
                return;

            EmotesEnum emotePlay = (EmotesEnum)message.emoteId;

            client.Character.PlayEmote(emotePlay);
        }

        public static void SendEmotePlayMessage(IPacketReceiver client, Character character, EmotesEnum emote)
        {
            client.Send(new EmotePlayMessage((ushort)emote, DateTime.Now.GetUnixTimeStampLong(), character.Id, character.Account.Id));
        }

        public static void SendEmoteListMessage(IPacketReceiver client, IEnumerable<ushort> emoteList)
        {
            var emotes = emoteList.Select(x => x).ToArray();

            client.Send(new EmoteListMessage(emotes));
        }

        public static void SendEmoteAddMessage(IPacketReceiver client, ushort emote)
        {
            client.Send(new EmoteAddMessage(emote));
        }

        public static void SendEmoteRemoveMessage(IPacketReceiver client, ushort emote)
        {
            client.Send(new EmoteRemoveMessage(emote));
        }

        public static void SendEmotePlayErrorMessage(IPacketReceiver client, ushort emote)
        {
            client.Send(new EmotePlayErrorMessage(emote));
        }
    }
}