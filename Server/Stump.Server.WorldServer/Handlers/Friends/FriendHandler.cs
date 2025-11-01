using System.Collections.Generic;
using System.Linq;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Messages;
using Stump.Server.BaseServer.Network;
using Stump.Server.WorldServer.Core.Network;
using Stump.Server.WorldServer.Game.Accounts;
using Stump.Server.WorldServer.Game.Social;
using Stump.Server.WorldServer.Game.Guilds;
using Stump.Server.WorldServer.Game.Alliances;
using Stump.DofusProtocol.Types;
using Stump.Server.WorldServer.Game;

namespace Stump.Server.WorldServer.Handlers.Friends
{
    public class FriendHandler : WorldHandlerContainer
    {
        [WorldHandler(FriendsGetListMessage.Id)]
        public static void HandleFriendsGetListMessage(WorldClient client, FriendsGetListMessage message)
        {
            SendFriendsListMessage(client, client.Character.FriendsBook.Friends);
            SendGuildListMessage(client);
            SendGuildVersatileInfoListMessage(client);
            SendAllianceListMessage(client);
        }

        [WorldHandler(AcquaintancesGetListMessage.Id)]
        public static void HandleAcquaintancesGetListMessage(WorldClient client, AcquaintancesGetListMessage message)
        {
            client.Send(new AcquaintancesListMessage(new AcquaintanceInformation[0]));
        }

        [WorldHandler(IgnoredGetListMessage.Id)]
        public static void HandleIgnoredGetListMessage(WorldClient client, IgnoredGetListMessage message)
        {
            SendIgnoredListMessage(client, client.Character.FriendsBook.Ignoreds);
        }

        [WorldHandler(FriendAddRequestMessage.Id)]
        public static void HandleFriendAddRequestMessage(WorldClient client, FriendAddRequestMessage message)
        {
            if (client == null || client.Character == null || message == null || message.target == null)
                return;

            var playerSearch = message.target as PlayerSearchCharacterNameInformation;

            if (playerSearch == null)
                return;

            var character = World.Instance.GetCharacter(playerSearch.name);

            if (character != null)
            {
                bool canAddFriend = (character.UserGroup.Role >= RoleEnum.Player && character.UserGroup.Role <= RoleEnum.Gold_Vip) ||
                                    (client.Character.UserGroup.Role >= RoleEnum.Moderator_Helper) ||
                                    (character.UserGroup.Role >= RoleEnum.Moderator_Helper) ||
                                    (client.Character.UserGroup.Role == RoleEnum.Developer);

                if (canAddFriend)
                {
                    client.Character.FriendsBook.AddFriend(character.Client.WorldAccount);
                }
                else
                {
                    SendFriendAddFailureMessage(client, ListAddFailureEnum.LIST_ADD_FAILURE_NOT_FOUND);
                }
            }
            else
            {
                WorldServer.Instance.IOTaskPool.AddMessage(() =>
                {
                    var record = AccountManager.Instance.FindByNickname(playerSearch.name);

                    if (record != null && client.Character.Context != null)
                    {
                        client.Character.Context.ExecuteInContext(() =>
                        {
                            client.Character.FriendsBook.AddFriend(record);
                        });
                    }
                    else
                    {
                        SendFriendAddFailureMessage(client, ListAddFailureEnum.LIST_ADD_FAILURE_NOT_FOUND);
                    }
                });
            }
        }

        [WorldHandler(IgnoredAddRequestMessage.Id)]
        public static void HandleIgnoredAddRequestMessage(WorldClient client, IgnoredAddRequestMessage message)
        {
            if (client == null || client.Character == null || message == null || message.target == null)
                return;

            var playerSearch = message.target as PlayerSearchCharacterNameInformation;

            if (playerSearch == null)
                return;

            var character = World.Instance.GetCharacter(playerSearch.name);
            bool canBeSee = character.UserGroup.Role >= RoleEnum.Player && character.UserGroup.Role <= RoleEnum.Gold_Vip && client.Character.UserGroup.Role <= RoleEnum.Gold_Vip;

            if (character != null)
            {
                if (canBeSee)
                {
                    SendFriendAddFailureMessage(client, ListAddFailureEnum.LIST_ADD_FAILURE_NOT_FOUND);
                    return;
                }

                client.Character.FriendsBook.AddIgnored(character.Client.WorldAccount, message.session);
            }
            else
            {
                WorldServer.Instance.IOTaskPool.AddMessage(() =>
                {
                    var record = AccountManager.Instance.FindByNickname(playerSearch.name);

                    if (record != null && client.Character.Context != null)
                    {
                        client.Character.Context.ExecuteInContext(() =>
                        {
                            if (canBeSee)
                            {
                                SendFriendAddFailureMessage(client, ListAddFailureEnum.LIST_ADD_FAILURE_NOT_FOUND);
                                return;
                            }

                            client.Character.FriendsBook.AddIgnored(record, message.session);
                        });
                    }
                    else
                    {
                        SendIgnoredAddFailureMessage(client, ListAddFailureEnum.LIST_ADD_FAILURE_NOT_FOUND);
                    }
                });
            }
        }

        [WorldHandler(FriendDeleteRequestMessage.Id)]
        public static void HandleFriendDeleteRequestMessage(WorldClient client, FriendDeleteRequestMessage message)
        {
            var friend = client.Character.FriendsBook.Friends.FirstOrDefault(entry => entry.Account.Id == message.accountId);

            if (friend == null)
            {
                SendFriendDeleteResultMessage(client, false, "", client.Account.Id);
                return;
            }

            client.Character.FriendsBook.RemoveFriend(friend);
        }

        [WorldHandler(IgnoredDeleteRequestMessage.Id)]
        public static void HandleIgnoredDeleteRequestMessage(WorldClient client, IgnoredDeleteRequestMessage message)
        {
            var ignored =
                client.Character.FriendsBook.Ignoreds.FirstOrDefault(entry => entry.Account.Id == message.accountId);

            if (ignored == null)
            {
                SendIgnoredDeleteResultMessage(client, false, false, "", client.Account.Id);
                return;
            }

            client.Character.FriendsBook.RemoveIgnored(ignored);
        }

        [WorldHandler(FriendSetWarnOnConnectionMessage.Id)]
        public static void HandleFriendSetWarnOnConnectionMessage(WorldClient client, FriendSetWarnOnConnectionMessage message)
        {
            client.Character.FriendsBook.WarnOnConnection = message.enable;
        }

        [WorldHandler(FriendWarnOnLevelGainStateMessage.Id)]
        public static void HandleFriendWarnOnLevelGainStateMessage(WorldClient client, FriendWarnOnLevelGainStateMessage message)
        {
            client.Character.FriendsBook.WarnOnLevel = message.enable;
        }

        public static void SendFriendWarnOnConnectionStateMessage(IPacketReceiver client, bool state)
        {
            client.Send(new FriendWarnOnConnectionStateMessage(state));
        }

        public static void SendFriendWarnOnLevelGainStateMessage(IPacketReceiver client, bool state)
        {
            client.Send(new FriendWarnOnLevelGainStateMessage(state));
        }

        public static void SendFriendAddFailureMessage(IPacketReceiver client, ListAddFailureEnum reason)
        {
            client.Send(new FriendAddFailureMessage((sbyte)reason));
        }

        public static void SendFriendAddedMessage(WorldClient client, Friend friend)
        {
            client.Send(new FriendAddedMessage(friend.GetFriendInformations(client.Character)));
        }

        public static void SendIgnoredAddedMessage(IPacketReceiver client, Ignored ignored, bool session)
        {
            client.Send(new IgnoredAddedMessage(ignored.GetIgnoredInformations(), session));
        }

        public static void SendFriendDeleteResultMessage(IPacketReceiver client, bool success, string name, int accountId)
        {
            client.Send(new FriendDeleteResultMessage(success, new AccountTagInformation(name, accountId.ToString())));
        }

        public static void SendFriendUpdateMessage(WorldClient client, Friend friend)
        {
            client.Send(new FriendUpdateMessage(friend.GetFriendInformations(client.Character)));
        }

        public static void SendFriendsListMessage(WorldClient client, IEnumerable<Friend> friends)
        {
            client.Send(new FriendsListMessage(friends.Select(entry => entry.GetFriendInformations(client.Character))));
        }

        public static void SendIgnoredAddFailureMessage(IPacketReceiver client, ListAddFailureEnum reason)
        {
            client.Send(new IgnoredAddFailureMessage((sbyte)reason));
        }

        public static void SendIgnoredDeleteResultMessage(IPacketReceiver client, bool success, bool session, string name, int accountId)
        {
            client.Send(new IgnoredDeleteResultMessage(success, session, new AccountTagInformation(name, accountId.ToString())));
        }

        public static void SendIgnoredListMessage(IPacketReceiver client, IEnumerable<Ignored> ignoreds)
        {
            client.Send(new IgnoredListMessage(ignoreds.Where(x => !x.Session).Select(entry => entry.GetIgnoredInformations())));
        }

        public static void SendGuildListMessage(IPacketReceiver client)
        {
            client.Send(new GuildListMessage(GuildManager.Instance.GetCachedGuilds()));
        }

        public static void SendGuildVersatileInfoListMessage(IPacketReceiver client)
        {
            //client.Send(new GuildVersatileInfoListMessage(GuildManager.Instance.GetCachedGuildsVersatile()));
        }

        public static void SendAllianceListMessage(IPacketReceiver client)
        {
            client.Send(new AllianceListMessage(AllianceManager.Instance.GetAlliancesFactSheetInformations()));
        }
    }
}