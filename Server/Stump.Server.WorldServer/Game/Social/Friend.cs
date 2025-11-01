using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Types;
using Stump.Server.WorldServer.Database.Accounts;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.HavenBags;

namespace Stump.Server.WorldServer.Game.Social
{
    public class Friend
    {
        public Friend(AccountRelation relation, WorldAccount account)
        {
            Relation = relation;
            Account = account;
        }

        public Friend(AccountRelation relation, WorldAccount account, Character character)
        {
            Relation = relation;
            Account = account;
            Character = character;
        }

        public WorldAccount Account { get; }

        public Character Character { get; private set; }

        public AccountRelation Relation { get; }

        public void SetOnline(Character character)
        {
            if (character.Client.WorldAccount.Id != Account.Id)
                return;

            Character = character;
        }

        public void SetOffline()
        {
            Character = null;
        }

        public bool IsOnline()
        {
            return Character != null;
        }

        public FriendInformations GetFriendInformations(Character asker)
        {
            var havenbag = HavenBagManager.Instance.GetHavenBagByOwner(Character);
            var accountTag = new AccountTagInformation(Account.Nickname, Account.Id.ToString());

            if (havenbag == null && IsOnline())
            {
                return new FriendOnlineInformations(
                    accountId: Account.Id,
                    accountTag: accountTag,
                    playerState: (sbyte)(Character.FriendsBook.IsFriend(asker.Account.Id) ? Character.IsFighting() ? PlayerStateEnum.GAME_TYPE_FIGHT : PlayerStateEnum.GAME_TYPE_ROLEPLAY : PlayerStateEnum.UNKNOWN_STATE),
                    lastConnection: (ushort)Account.LastConnectionTimeStamp,
                    achievementPoints: Character.Record.AchievementPoints,
                    leagueId: (short)Character.ArenaLeague.LeagueId,
                    ladderPosition: 0,
                    sex: Character.Sex == SexTypeEnum.SEX_FEMALE,
                    havenBagShared: false,
                    playerId: (ulong)Character.Id,
                    playerName: Character.Name,
                    level: Character.FriendsBook.IsFriend(asker.Account.Id) ? (byte)Character.Level : (byte)0,
                    alignmentSide: Character.FriendsBook.IsFriend(asker.Account.Id) ? (sbyte)Character.AlignmentSide : (sbyte)AlignmentSideEnum.ALIGNMENT_UNKNOWN,
                    breed: (sbyte)Character.Breed.Id,
                    guildInfo: Character.GuildMember == null ? new GuildInformations(0, "", 0, new SocialEmblem(0, 0, 0, 0)) : Character.GuildMember.Guild.GetGuildInformations(),
                    moodSmileyId: (ushort)Character.SmileyMoodId,
                    status: Character.Status);
            }

            if (IsOnline())
            {
                return new FriendOnlineInformations(
                    accountId: Account.Id,
                    accountTag: accountTag,
                    playerState: (sbyte)(Character.FriendsBook.IsFriend(asker.Account.Id) ? Character.IsFighting() ? PlayerStateEnum.GAME_TYPE_FIGHT : PlayerStateEnum.GAME_TYPE_ROLEPLAY : PlayerStateEnum.UNKNOWN_STATE),
                    lastConnection: (ushort)Account.LastConnectionTimeStamp,
                    achievementPoints: Character.Record.AchievementPoints,
                    leagueId: (short)Character.ArenaLeague.LeagueId,
                    ladderPosition: 0,
                    sex: Character.Sex == SexTypeEnum.SEX_FEMALE,
                    havenBagShared: havenbag.FriendsAllowed,
                    playerId: (ulong)Character.Id,
                    playerName: Character.Name,
                    level: Character.FriendsBook.IsFriend(asker.Account.Id) ? (byte)Character.Level : (byte)0,
                    alignmentSide: Character.FriendsBook.IsFriend(asker.Account.Id) ? (sbyte)Character.AlignmentSide : (sbyte)AlignmentSideEnum.ALIGNMENT_UNKNOWN,
                    breed: (sbyte)Character.Breed.Id,
                    guildInfo: Character.GuildMember == null ? new GuildInformations(0, "", 0, new SocialEmblem(0, 0, 0, 0)) : Character.GuildMember.Guild.GetGuildInformations(),
                    moodSmileyId: (ushort)Character.SmileyMoodId,
                    status: Character.Status);
            }

            return new FriendInformations(
                accountId: Account.Id,
                accountTag: accountTag,
                playerState: (sbyte)PlayerStateEnum.NOT_CONNECTED,
                lastConnection: (ushort)Account.LastConnectionTimeStamp,
                achievementPoints: 0,
                leagueId: 0,
                ladderPosition: 0);
        }
    }
}