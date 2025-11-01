using NLog;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Types;
using Stump.Server.WorldServer.Database.Guilds;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using System;

namespace Stump.Server.WorldServer.Game.Guilds
{
    public class GuildMember
    {
        static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public GuildMember(GuildMemberRecord record)
        {
            Record = record;
        }

        public GuildMember(Guild guild, Character character)
        {
            Record = new GuildMemberRecord
            {
                CharacterId = character.Id,
                AccountId = character.Account.Id,
                Character = character.Record,
                GivenExperience = 0,
                GivenPercent = 0,
                RankId = 0,
                GuildId = guild.Id,
                Rights = GuildRightsBitEnum.GUILD_RIGHT_NONE,
            };

            Guild = guild;
            Character = character;
            IsDirty = true;
            IsNew = true;
        }

        public GuildMemberRecord Record
        {
            get;
        }

        public int Id => Record.CharacterId;

        /// <summary>
        ///     Null if the character isn't connected.
        /// </summary>
        public Character Character
        {
            get;
            private set;
        }

        public bool IsConnected => Character != null;

        public Guild Guild
        {
            get;
            private set;
        }

        public long GivenExperience
        {
            get { return Record.GivenExperience; }
            set
            {
                Record.GivenExperience = value;
                IsDirty = true;
            }
        }

        public byte GivenPercent
        {
            get { return Record.GivenPercent; }
            set
            {
                Record.GivenPercent = value;
                IsDirty = true;
            }
        }

        public GuildRightsBitEnum Rights
        {
            get { return Record.Rights; }
            set
            {
                Record.Rights = value;
                IsDirty = true;
            }
        }

        public short RankId
        {
            get { return Record.RankId >= 0 && Record.RankId <= 35 ? Record.RankId : (short)0; }
            set
            {
                Record.RankId = value;
                IsDirty = true;
            }
        }

        public bool IsBoss => RankId == 1;

        public string Name => Record.Name;

        public long Experience => Record.Experience;

        public int PrestigeRank => Record.PrestigeRank;

        public PlayableBreedEnum Breed => Record.Breed;

        public SexTypeEnum Sex => Record.Sex;

        public AlignmentSideEnum AlignementSide => Record.AlignementSide;

        public DateTime? LastConnection => Record.LastConnection;

        /// <summary>
        /// True if must be saved
        /// </summary>
        public bool IsDirty
        {
            get;
            protected set;
        }

        public bool IsNew
        {
            get;
            protected set;
        }

        public CharacterMinimalSocialPublicInformations GetCharacterMinimalSocialPublicInformations()
        {
            return new CharacterMinimalSocialPublicInformations(
                id: (ulong)Id,
                name: Name,
                level: ExperienceManager.Instance.GetCharacterLevel(Experience),
                rank: null);
        }

        public GuildMemberInfo GetNetworkGuildMember()
        {
            var hvb = HavenBags.HavenBagManager.Instance.GetHavenBagByOwner(Character);

            if (hvb == null && IsConnected)
            {
                GuildMemberInfo _guildMember = new GuildMemberInfo()
                {
                    id = (ulong)Id,
                    name = Character.Name,
                    level = Character.Level,
                    sex = Character.Sex == SexTypeEnum.SEX_FEMALE,
                    havenBagShared = false,
                    breed = (sbyte)Character.Breed.Id,
                    rankId = (ushort)RankId,
                    enrollmentDate = 0, //TODO - v2.66
                    givenExperience = (ulong)GivenExperience,
                    experienceGivenPercent = (sbyte)GivenPercent,
                    connected = Character.IsInFight() ? (sbyte)2 : (sbyte)1,
                    alignmentSide = (sbyte)Character.AlignmentSide,
                    hoursSinceLastConnection = (ushort)DateTime.Now.Hour,
                    moodSmileyId = (ushort)Character.SmileyMoodId,
                    accountId = Record.AccountId,
                    achievementPoints = Record.Character.AchievementPoints,
                    status = Character.Status,
                    note = null //TODO - v2.66
                };

                return _guildMember;
            }
            else if (IsConnected)
            {
                GuildMemberInfo _guildMember = new GuildMemberInfo()
                {
                    id = (ulong)Id,
                    name = Character.Name,
                    level = Character.Level,
                    sex = Character.Sex == SexTypeEnum.SEX_FEMALE,
                    havenBagShared = hvb.GuildAllowed,
                    breed = (sbyte)Character.Breed.Id,
                    rankId = (ushort)RankId,
                    enrollmentDate = 0, //TODO - v2.66
                    givenExperience = (ulong)GivenExperience,
                    experienceGivenPercent = (sbyte)GivenPercent,
                    connected = Character.IsInFight() ? (sbyte)2 : (sbyte)1,
                    alignmentSide = (sbyte)Character.AlignmentSide,
                    hoursSinceLastConnection = (ushort)DateTime.Now.Hour,
                    moodSmileyId = (ushort)Character.SmileyMoodId,
                    accountId = Record.AccountId,
                    achievementPoints = Record.Character.AchievementPoints,
                    status = Character.Status,
                    note = null //TODO - v2.66
                };

                return _guildMember;
            }

            GuildMemberInfo _guildFinalMember = new GuildMemberInfo()
            {
                id = (ulong)Id,
                name = Name,
                level = ExperienceManager.Instance.GetCharacterLevel(Experience, PrestigeRank),
                sex = Character.Sex == SexTypeEnum.SEX_FEMALE,
                havenBagShared = false,
                breed = (sbyte)Breed,
                rankId = (ushort)RankId,
                enrollmentDate = 0, //TODO - v2.66
                givenExperience = (ulong)GivenExperience,
                experienceGivenPercent = (sbyte)GivenPercent,
                connected = 0,
                alignmentSide = (sbyte)AlignementSide,
                hoursSinceLastConnection = LastConnection != null ? (ushort)(DateTime.Now - LastConnection.Value).TotalHours : (ushort)0,
                moodSmileyId = 0,
                accountId = Record.AccountId,
                achievementPoints = Record.Character.AchievementPoints,
                status = new PlayerStatus((sbyte)PlayerStatusEnum.PLAYER_STATUS_OFFLINE),
                note = null //TODO - v2.66
            };

            return _guildFinalMember;
        }

        public bool HasRight(GuildRightsBitEnum right) => Rights.HasFlag(GuildRightsBitEnum.GUILD_RIGHT_BOSS) || (Rights.HasFlag(GuildRightsBitEnum.GUILD_RIGHT_MANAGE_RIGHTS) && right != GuildRightsBitEnum.GUILD_RIGHT_BOSS) || Rights.HasFlag(right);

        public event Action<GuildMember> Connected;

        public event Action<GuildMember, Character> Disconnected;

        public void OnCharacterConnected(Character character)
        {
            if (character.Id != Record.CharacterId)
            {
                throw new Exception(string.Format("GuildMember.CharacterId ({0}) != characterid ({1})", Record.CharacterId, character.Id));
            }

            Character = character;

            var evnt = Connected;

            if (evnt != null)
                evnt(this);
        }

        public void OnCharacterDisconnected(Character character)
        {
            IsDirty = true;
            Character = null;

            var evnt = Disconnected;

            if (evnt != null)
                evnt(this, character);
        }

        public void AddXP(long experience)
        {
            GivenExperience += experience;
            Guild.AddXP(experience);
        }

        public void BindGuild(Guild guild)
        {
            if (Guild != null)
                throw new Exception(string.Format("Guild already bound to GuildMember {0}", Id));

            Guild = guild;
        }

        #region >> World Save
        public void Save(ORM.Database database)
        {
            WorldServer.Instance.IOTaskPool.AddMessage(() =>
            {
                try
                {
                    if (IsNew)
                    {
                        database.Insert(Record);
                    }
                    else
                    {
                        database.Update(Record);
                    }

                    IsDirty = false;
                    IsNew = false;
                }
                catch (Exception ex)
                {
                    logger.Error($"Error saving GuildMember: {ex.Message}");
                }
            });
        }
        #endregion
    }
}