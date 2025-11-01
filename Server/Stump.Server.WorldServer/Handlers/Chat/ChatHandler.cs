using System;
using System.Collections.Generic;
using Stump.Core.Extensions;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Messages;
using Stump.DofusProtocol.Types;
using Stump.Server.BaseServer.Network;
using Stump.Server.WorldServer.Core.Network;
using Stump.Server.WorldServer.Game;
using Stump.Server.WorldServer.Game.Actors.Interfaces;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Social;
using MongoDB.Bson;
using Stump.Server.BaseServer.Logging;
using System.Globalization;
using System.Linq;
using System.Web.UI.WebControls;

namespace Stump.Server.WorldServer.Handlers.Chat
{
    public partial class ChatHandler
    {
        [WorldHandler(ChatClientPrivateMessage.Id)]
        public static void HandleChatClientPrivateMessage(WorldClient client, ChatClientPrivateMessage message)
        {
            if (string.IsNullOrEmpty(message.content))
                return;

            var sender = client.Character;

            if (message.receiver == null)
                return;

            var playerSearch = (PlayerSearchCharacterNameInformation)message.receiver;
            var receiver = World.Instance.GetCharacter(playerSearch.name);

            if (receiver == null)
            {
                SendChatErrorMessage(client, ChatErrorEnum.CHAT_ERROR_RECEIVER_NOT_FOUND);
                return;
            }

            if (sender.IsMuted())
            {
                //Le principe de précaution vous a rendu muet pour %1 seconde(s).
                client.Character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 123, (int)client.Character.GetMuteRemainingTime().TotalSeconds);
                return;
            }

            if (receiver.IsMuted())
            {
                //Message automatique : Le joueur <b>%1</b> a été rendu muet pour ne pas avoir respecté les règles. <b>%1</b> ne pourra pas vous répondre avant <b>%2</b> minutes.
                sender.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 168, receiver.Name, (int)receiver.GetMuteRemainingTime().TotalMinutes);
                return;
            }

            if (sender == receiver)
            {
                //Le joueur %1 était absent et n'a donc pas reçu votre message.
                SendChatErrorMessage(client, ChatErrorEnum.CHAT_ERROR_INTERIOR_MONOLOGUE);
                return;
            }

            var badword = ChatManager.Instance.CanSendMessage(message.content);

            if (badword != string.Empty)
            {
                #region // ----------------- Sistema de Logs MongoDB Mensagens Badwords by: Kenshin ---------------- //
                try
                {
                    var document = new BsonDocument
                        {
                            { "SenderHardwareID", sender.Account.LastHardwareId },
                            { "SenderId", sender.Id },
                            { "SenderName", sender.Name },
                            { "SenderAccountId", sender.Account.Id },
                            { "SenderAccountName", sender.Account.Login },
                            { "ReceiverId", receiver.Id },
                            { "ReceiverName", receiver.Name },
                            { "ReceiverAccountId", receiver.Account.Id },
                            { "ReceiverAccountName", receiver.Account.Login },
                            { "Message", message.content },
                            { "Badword", badword },
                            { "ChannelId", 9 },
                            { "ChannelName", "Channel Private" },
                            { "Date", DateTime.Now.ToString(CultureInfo.CreateSpecificCulture("pt-BR")) }
                        };

                    MongoLogger.Instance.Insert("Player_Badword", document);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Erro no Mongologs das Mensagens : " + e.Message);
                }
                #endregion

                switch (client.Character.Account.Lang)
                {
                    case "fr":
                        client.Character.SendServerMessage($"Message non envoyé. Le terme <b>{badword}</b> est interdit sur le serveur ! Votre Nick est surveillé.");
                        break;
                    case "es":
                        client.Character.SendServerMessage($"Mensaje no enviado ¡El término <b>{badword}</b> está prohibido en el servidor! Tu Nick está siendo monitoreado.");
                        break;
                    case "en":
                        client.Character.SendServerMessage($"Message not sent. The term <b>{badword}</b> is forbidden on the server! Your Nick is being monitored.");
                        break;
                    default:
                        client.Character.SendServerMessage($"Mensagem não enviada. O termo <b>{badword}</b> é proibido no servidor! Seu Nick está salvo e sendo monitorado.");
                        break;
                }

                return;
            }

            if (receiver.FriendsBook.IsIgnored(sender.Account.Id))
            {
                //<b>%1</b> is ignoring you and no longer receives your messages.
                sender.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 381, receiver.Name);
                return;
            }

            if (!receiver.IsAvailable(sender, true))
            {
                //<b>%1</b> is ignoring you and no longer receives your messages.
                sender.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 381, receiver.Name);
                return;
            }

            if (sender.Status.statusId != (sbyte)PlayerStatusEnum.PLAYER_STATUS_AVAILABLE && sender.Status.statusId != (sbyte)PlayerStatusEnum.PLAYER_STATUS_PRIVATE
                || !sender.FriendsBook.IsFriend(receiver.Account.Id)) sender.SetStatus(PlayerStatusEnum.PLAYER_STATUS_AVAILABLE);

            #region // ----------------- Sistema de Logs MongoDB Mensagens by: Kenshin ---------------- //
            try
            {
                var document = new BsonDocument
                    {
                        { "SenderHardwareID", sender.Account.LastHardwareId },
                        { "SenderId", sender.Id },
                        { "SenderName", sender.Name },
                        { "SenderAccountId", sender.Account.Id },
                        { "SenderAccountName", sender.Account.Login },
                        { "ReceiverId", receiver.Id },
                        { "ReceiverName", receiver.Name },
                        { "ReceiverAccountId", receiver.Account.Id },
                        { "ReceiverAccountName", receiver.Account.Login },
                        { "Message", message.content },
                        { "ChannelId", 9 },
                        { "ChannelName", "Channel Private" },
                        { "Date", DateTime.Now.ToString(CultureInfo.CreateSpecificCulture("pt-BR")) }
                    };

                MongoLogger.Instance.Insert("Player_Chats", document);
            }
            catch (Exception e)
            {
                Console.WriteLine("Erro no Mongologs das Mensagens : " + e.Message);
            }
            #endregion

            //Send to receiver
            SendChatServerMessage(receiver.Client, sender, ChatActivableChannelsEnum.PSEUDO_CHANNEL_PRIVATE, message.content);

            //Send a copy to sender
            SendChatServerCopyMessage(client, sender, receiver, ChatActivableChannelsEnum.PSEUDO_CHANNEL_PRIVATE, message.content);

            if (receiver.Status.statusId == (sbyte)PlayerStatusEnum.PLAYER_STATUS_AFK && receiver.Status is PlayerStatusExtended)
                SendChatServerMessage(sender.Client, receiver, ChatActivableChannelsEnum.PSEUDO_CHANNEL_PRIVATE, $"Réponse automatique:{((PlayerStatusExtended)receiver.Status).message}");
        }

        [WorldHandler(ChatClientPrivateWithObjectMessage.Id)]
        public static void HandleChatClientPrivateWithObjectMessage(WorldClient client, ChatClientPrivateWithObjectMessage message)
        {
            if (string.IsNullOrEmpty(message.content))
                return;

            var sender = client.Character;

            if (message.receiver == null)
                return;

            var playerSearch = (PlayerSearchCharacterNameInformation)message.receiver;
            var receiver = World.Instance.GetCharacter(playerSearch.name);

            if (receiver == null)
            {
                SendChatErrorMessage(client, ChatErrorEnum.CHAT_ERROR_RECEIVER_NOT_FOUND);
                return;
            }

            if (sender.IsMuted())
            {
                //Le principe de précaution vous a rendu muet pour %1 seconde(s).
                sender.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 123, (int)sender.GetMuteRemainingTime().TotalSeconds);
                return;
            }

            if (receiver.IsMuted())
            {
                //Message automatique : Le joueur <b>%1</b> a été rendu muet pour ne pas avoir respecté les règles. <b>%1</b> ne pourra pas vous répondre avant <b>%2</b> minutes.
                sender.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 168, receiver.Name, receiver.Name, (int)receiver.GetMuteRemainingTime().TotalMinutes);
                return;
            }

            if (sender == receiver)
            {
                SendChatErrorMessage(client, ChatErrorEnum.CHAT_ERROR_INTERIOR_MONOLOGUE);
                return;
            }

            var badword = ChatManager.Instance.CanSendMessage(message.content);


            if (badword != string.Empty)
            {
                #region // ----------------- Sistema de Logs MongoDB Mensagens Badwords by: Kenshin ---------------- //
                try
                {
                    var document = new BsonDocument
                    {
                            { "SenderHardwareID", sender.Account.LastHardwareId },
                            { "SenderId", sender.Id },
                            { "SenderName", sender.Name },
                            { "SenderAccountId", sender.Account.Id },
                            { "SenderAccountName", sender.Account.Login },
                            { "ReceiverId", receiver.Id },
                            { "ReceiverName", receiver.Name },
                            { "ReceiverAccountId", receiver.Account.Id },
                            { "ReceiverAccountName", receiver.Account.Login },
                            { "Message", message.content },
                            { "Badword", badword },
                            { "ChannelId", 9 },
                            { "ChannelName", "Channel Private" },
                            { "Date", DateTime.Now.ToString(CultureInfo.CreateSpecificCulture("pt-BR")) }
                    };

                    MongoLogger.Instance.Insert("Player_Badword", document);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Erro no Mongologs das Mensagens : " + e.Message);
                }
                #endregion

                switch (client.Character.Account.Lang)
                {
                    case "fr":
                        client.Character.SendServerMessage($"Message non envoyé. Le terme <b>{badword}</b> est interdit sur le serveur ! Votre Nick est surveillé.");
                        break;
                    case "es":
                        client.Character.SendServerMessage($"Mensaje no enviado ¡El término <b>{badword}</b> está prohibido en el servidor! Tu Nick está siendo monitoreado.");
                        break;
                    case "en":
                        client.Character.SendServerMessage($"Message not sent. The term <b>{badword}</b> is forbidden on the server! Your Nick is being monitored.");
                        break;
                    default:
                        client.Character.SendServerMessage($"Mensagem não enviada. O termo <b>{badword}</b> é proibido no servidor! Seu Nick está salvo e sendo monitorado.");
                        break;
                }
                return;
            }

            if (receiver.FriendsBook.IsIgnored(sender.Account.Id))
            {
                //Le joueur %1 était absent et n'a donc pas reçu votre message.
                sender.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 14, receiver.Name);
                return;
            }

            if (!receiver.IsAvailable(sender, true))
            {
                //Le joueur %1 était absent et n'a donc pas reçu votre message.
                sender.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 14, receiver.Name);
                return;
            }

            if (sender.Status.statusId != (sbyte)PlayerStatusEnum.PLAYER_STATUS_AVAILABLE && sender.Status.statusId != (sbyte)PlayerStatusEnum.PLAYER_STATUS_PRIVATE || !sender.FriendsBook.IsFriend(receiver.Account.Id))
                sender.SetStatus(PlayerStatusEnum.PLAYER_STATUS_AVAILABLE);

            #region // ----------------- Sistema de Logs MongoDB Mensagens by: Kenshin ---------------- //
            try
            {
                var document = new BsonDocument
                {
                   { "SenderHardwareID", sender.Account.LastHardwareId },
                   { "SenderId", sender.Id },
                   { "SenderName", sender.Name },
                   { "SenderAccountId", sender.Account.Id },
                   { "SenderAccountName", sender.Account.Login },
                   { "ReceiverId", receiver.Id },
                   { "ReceiverName", receiver.Name },
                   { "ReceiverAccountId", receiver.Account.Id },
                   { "ReceiverAccountName", receiver.Account.Login },
                   { "Message", message.content },
                   { "ChannelId", 9 },
                   { "ChannelName", "Channel Private" },
                   { "Date", DateTime.Now.ToString(CultureInfo.CreateSpecificCulture("pt-BR")) }
                };

                MongoLogger.Instance.Insert("Player_Chats", document);
            }
            catch (Exception e)
            {
                Console.WriteLine("Erro no Mongologs das Mensagens : " + e.Message);
            }
            #endregion      

            //Send to receiver
            SendChatServerWithObjectMessage(receiver.Client, sender, ChatActivableChannelsEnum.PSEUDO_CHANNEL_PRIVATE, message.content, "", message.objects);

            //Send a copy to sender
            SendChatServerCopyWithObjectMessage(client, sender, receiver, ChatActivableChannelsEnum.PSEUDO_CHANNEL_PRIVATE, message.content, message.objects);

            if (receiver.Status.statusId == (sbyte)PlayerStatusEnum.PLAYER_STATUS_AFK && receiver.Status is PlayerStatusExtended)
                SendChatServerMessage(sender.Client, receiver, ChatActivableChannelsEnum.PSEUDO_CHANNEL_PRIVATE, $"Réponse automatique:{((PlayerStatusExtended)receiver.Status).message}");
        }

        [WorldHandler(ChatClientMultiMessage.Id)]
        public static void HandleChatClientMultiMessage(WorldClient client, ChatClientMultiMessage message)
        {
            ChatManager.Instance.HandleChat(client, (ChatActivableChannelsEnum)message.channel, message.content);
        }

        [WorldHandler(ChatClientMultiWithObjectMessage.Id)]
        public static void HandleChatClientMultiWithObjectMessage(WorldClient client, ChatClientMultiWithObjectMessage message)
        {
            ChatManager.Instance.HandleChat(client, (ChatActivableChannelsEnum)message.channel, message.content, message.objects);
        }

        [WorldHandler(ChatCommunityChannelSetCommunityRequestMessage.Id)]
        public static void HandleChatCommunityChannelSetCommunityRequestMessage(WorldClient client, ChatCommunityChannelSetCommunityRequestMessage message)
        {
            client.Send(new ChatCommunityChannelCommunityMessage(message.communityId));
        }

        //Version 2.61 by Kenshin
        public static void SendChatServerWithObjectMessage(IPacketReceiver client, INamedActor sender, ChatActivableChannelsEnum channel, string content, string fingerprint, IEnumerable<ObjectItem> objectItems)
        {
            client.Send(new ChatServerWithObjectMessage(channel: (sbyte)channel, content: content, timestamp: DateTime.Now.GetUnixTimeStamp(), fingerprint: fingerprint, senderId: sender.Id, senderName: sender.Name, prefix: "", senderAccountId: 0, objects: objectItems.ToArray()));
        }

        public static void SendChatServerMessage(IPacketReceiver client, string message)
        {
            SendChatServerMessage(client, ChatActivableChannelsEnum.PSEUDO_CHANNEL_INFO, message, DateTime.Now.GetUnixTimeStamp(), "", 0, "", 0);
        }

        public static void SendChatServerMessage(IPacketReceiver client, INamedActor sender, ChatActivableChannelsEnum channel, string message)
        {
            SendChatServerMessage(client, sender, channel, message, DateTime.Now.GetUnixTimeStamp(), "");
        }

        public static void SendChatServerMessage(IPacketReceiver client, INamedActor sender, ChatActivableChannelsEnum channel, string message, int timestamp, string fingerprint)
        {
            client.Send(new ChatServerMessage(
                            channel: (sbyte)channel,
                            content: message,
                            timestamp: timestamp,
                            fingerprint: fingerprint,
                            senderId: sender.Id,
                            senderName: sender.Name,
                            prefix: "",
                            senderAccountId: 0));
        }

        public static void SendChatServerMessage(IPacketReceiver client, Character sender, ChatActivableChannelsEnum channel, string message)
        {
            SendChatServerMessage(client, sender, channel, message, DateTime.Now.GetUnixTimeStamp(), "");
        }

        public static void SendChatServerMessage(IPacketReceiver client, Character sender, ChatActivableChannelsEnum channel, string message, int timestamp, string fingerprint)
        {
            if (string.IsNullOrEmpty(message))
                return;

            if (sender.UserGroup.Role <= RoleEnum.GameMaster_Padawan)
                message = message.HtmlEntities();

            client.Send(new ChatServerMessage(
                channel: (sbyte)channel,
                content: message,
                timestamp: timestamp,
                fingerprint: fingerprint,
                senderId: sender.Id,
                senderName: sender.Name,
                prefix: "",
                senderAccountId: sender.Account.Id));
        }

        public static void SendChatServerMessage(IPacketReceiver client, ChatActivableChannelsEnum channel, string message, int timestamp, string fingerprint, int senderId, string senderName, int accountId)
        {
            if (!string.IsNullOrEmpty(message))
            {
                client.Send(new ChatServerMessage(
                    channel: (sbyte)channel,
                    content: message,
                    timestamp: timestamp,
                    fingerprint: fingerprint,
                    senderId: senderId,
                    senderName: senderName,
                    prefix: "",
                    senderAccountId: accountId));
            }
        }

        public static void SendChatAdminServerMessage(IPacketReceiver client, Character sender, ChatActivableChannelsEnum channel, string message)
        {
            SendChatAdminServerMessage(client, sender, channel, message, DateTime.Now.GetUnixTimeStamp(), "");
        }

        public static void SendChatAdminServerMessage(IPacketReceiver client, Character sender, ChatActivableChannelsEnum channel, string message, int timestamp, string fingerprint)
        {
            SendChatAdminServerMessage(client, channel,
                                       message,
                                       timestamp,
                                       fingerprint,
                                       sender.Id,
                                       sender.Name,
                                       sender.Account.Id);
        }

        public static void SendChatAdminServerMessage(IPacketReceiver client, ChatActivableChannelsEnum channel, string message, int timestamp, string fingerprint, int senderId, string senderName, int accountId)
        {
            if (!string.IsNullOrEmpty(message))
            {
                //client.Send(new ChatAdminServerMessage((sbyte)channel,
                //                                       message,
                //                                       timestamp,
                //                                       fingerprint,
                //                                       senderId,
                //                                       senderName,
                //                                       accountId));
            }
        }

        public static void SendChatServerCopyMessage(IPacketReceiver client, Character sender, Character receiver, ChatActivableChannelsEnum channel, string message)
        {
            SendChatServerCopyMessage(client, sender, receiver, channel, message, DateTime.Now.GetUnixTimeStamp(), "");
        }

        public static void SendChatServerCopyMessage(IPacketReceiver client, Character sender, Character receiver, ChatActivableChannelsEnum channel, string message, int timestamp, string fingerprint)
        {
            if (!sender.UserGroup.IsGameMaster)
                message = message.HtmlEntities();

            client.Send(new ChatServerCopyMessage(
                            (sbyte)channel,
                            message,
                            timestamp,
                            fingerprint,
                            (ulong)receiver.Id,
                            receiver.Name));
        }

        public static void SendChatServerCopyWithObjectMessage(IPacketReceiver client, Character sender, Character receiver, ChatActivableChannelsEnum channel, string message, IEnumerable<ObjectItem> objectItems)
        {
            SendChatServerCopyWithObjectMessage(client, sender, receiver, channel, message, DateTime.Now.GetUnixTimeStamp(), "", objectItems);
        }

        public static void SendChatServerCopyWithObjectMessage(IPacketReceiver client, Character sender, Character receiver, ChatActivableChannelsEnum channel, string message, int timestamp, string fingerprint, IEnumerable<ObjectItem> objectItems)
        {
            if (!sender.UserGroup.IsGameMaster)
                message = message.HtmlEntities();

            client.Send(new ChatServerCopyWithObjectMessage(
                            (sbyte)channel,
                            message,
                            timestamp,
                            fingerprint,
                            (ulong)receiver.Id,
                            receiver.Name,
                            objectItems));
        }

        public static void SendChatErrorMessage(IPacketReceiver client, ChatErrorEnum error)
        {
            client.Send(new ChatErrorMessage((sbyte)error));
        }
    }
}