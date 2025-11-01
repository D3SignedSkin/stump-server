using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NLog;
using Stump.Core.Extensions;
using Stump.Core.Reflection;
using Stump.DofusProtocol.Messages;
using Stump.DofusProtocol.Types;
using Stump.Server.WorldServer.Core.Network;
using Stump.Server.WorldServer.Database.Alliances;
using Stump.Server.WorldServer.Game.Guilds;
using Stump.Server.WorldServer.Game.Prisms;
using Stump.Server.WorldServer.Handlers.Alliances;
using Stump.Server.WorldServer.Handlers.Basic;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Items;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using System.Collections.ObjectModel;
using NetworkGuildEmblem = Stump.DofusProtocol.Types.SocialEmblem;
using System.Text.RegularExpressions;

namespace Stump.Server.WorldServer.Game.Alliances
{
    public class Alliance
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        public Dictionary<int, Guild> m_guilds = new Dictionary<int, Guild>();
        readonly List<PrismNpc> m_taxPrims = new List<PrismNpc>();
        private readonly object m_lock = new object();
        private bool m_isDirty;

        public Alliance(int id, string name, string tag)
        {
            Record = new AllianceRecord();
            Id = id;
            Name = name;
            Tag = tag;
            Record.CreationDate = DateTime.Now;
            Emblem = new AllianceEmblem(Record)
            {
                BackgroundColor = Color.White,
                BackgroundShape = 1,
                SymbolColor = Color.Black,
                SymbolShape = 1
            };
            BulletinContent = "";
            BulletinDate = DateTime.Now;
            LastNotifiedDate = DateTime.Now;
            Prisms = new List<PrismNpc>();
            Record.IsNew = true;
            IsDirty = true;
        }

        public Alliance(AllianceRecord record)
        {
            Record = record;
            Emblem = new AllianceEmblem(Record);
            BulletinDate = DateTime.Now;
            LastNotifiedDate = DateTime.Now;
            Prisms = new List<PrismNpc>();

            foreach (var item in Singleton<GuildManager>.Instance.GetGuildsByAlliance(Record))
            {
                if (item.Id == Record.Owner)
                {
                    item.SetAlliance(this);

                    m_guilds.Add(item.Id, item);

                    foreach (var guild in item.Clients)
                    {
                        if (!Clients.Contains(guild))
                            Clients.Add(guild);
                    }

                    if (Record.Owner == item.Id)
                        SetBoss(item);
                }
            }

            foreach (var item in Singleton<GuildManager>.Instance.GetGuildsByAlliance(Record))
            {
                if (item.Id != Record.Owner)
                {
                    item.SetAlliance(this);

                    m_guilds.Add(item.Id, item);

                    foreach (var guild in item.Clients)
                    {
                        if (!Clients.Contains(guild))
                            Clients.Add(guild);
                    }

                    if (Record.Owner == item.Id)
                        SetBoss(item);
                }
            }

            if (Boss == null)
            {
                if (m_guilds.Count > 0)
                    SetBoss(m_guilds.First().Value);
            }
        }

        public AllianceRecord Record { get; }

        public Guild Boss { get; private set; }

        public AllianceEmblem Emblem { get; protected set; }

        public List<PrismNpc> Prisms { get; set; }

        public ReadOnlyCollection<PrismNpc> TaxPrisms => m_taxPrims.AsReadOnly();

        WorldClientCollection m_clients = new WorldClientCollection();

        private List<Actors.RolePlay.Characters.Character> m_Character = new List<Actors.RolePlay.Characters.Character>();

        #region >> Fields
        public int Id
        {
            get { return Record.Id; }
            private set { Record.Id = value; }
        }

        public string Name
        {
            get { return Record.Name; }
            private set
            {
                Record.Name = value;
                IsDirty = true;
            }
        }

        public string Tag
        {
            get { return Record.Tag; }
            private set
            {
                Record.Tag = value;
                IsDirty = true;
            }
        }

        public WorldClientCollection Clients
        {
            get
            {
                foreach (var a in m_guilds)
                {
                    foreach (var b in a.Value.Clients)
                    {
                        if (!m_Character.Contains(b.Character))
                        {
                            m_Character.Add(b.Character);
                            m_clients.Add(b.Character.Client);
                        }
                    }
                }

                return m_clients;
            }
        }

        public bool IsDirty
        {
            get { return m_isDirty || Emblem.IsDirty; }
            set
            {
                m_isDirty = value;
                if (!value)
                {
                    Emblem.IsDirty = false;
                }
            }
        }

        public void UpdateMotd(Guilds.GuildMember member, string content)
        {
            try
            {
                MotdContent = content;
                MotdMember = member;
                MotdDate = DateTime.Now;

                AllianceHandler.SendAllianceMotdMessage(Clients, this);
            }
            catch (Exception ex)
            {
                logger.Error("UpdateMotd: " + ex);
            }
        }

        public void UpdateBulletin(GuildMember member, string content, bool notify = true)
        {
            try
            {
                BulletinContent = content;
                BulletinMember = member;
                BulletinDate = DateTime.Now;

                if (notify)
                    LastNotifiedDate = DateTime.Now;

                AllianceHandler.SendAllianceBulletinMessage(Clients, this);
            }
            catch (Exception ex)
            {
                logger.Error("UpdateBulletin: " + ex);
            }
        }

        public string MotdContent
        {
            get { return Record.MotdContent; }
            protected set
            {
                Record.MotdContent = value;
            }
        }

        public DateTime MotdDate
        {
            get { return Record.MotdDate; }
            set
            {
                Record.MotdDate = value;
            }
        }

        public Guilds.GuildMember MotdMember
        {
            get { return MembersTryGetId(Record.MotdMemberId); }
            protected set
            {
                Record.MotdMemberId = value.Id;
            }
        }

        public string BulletinContent
        {
            get { return Record.BulletinContent; }
            protected set
            {
                Record.BulletinContent = value;
            }
        }

        public DateTime BulletinDate
        {
            get { return Record.BulletinDate; }
            protected set
            {
                Record.BulletinDate = value;
            }
        }

        public DateTime LastNotifiedDate
        {
            get { return Record.LastNotifiedDate; }
            protected set
            {
                Record.LastNotifiedDate = value;
            }
        }

        public Guilds.GuildMember BulletinMember
        {
            get { return MembersTryGetId(Record.BulletinMemberId); }
            protected set
            {
                Record.BulletinMemberId = value.Id;
            }
        }

        public ushort Members
        {
            get { return (ushort)m_guilds.Sum(entry => entry.Value.Members.Count); }
        }
        #endregion

        #region >> Handlers 
        public Guilds.GuildMember MembersTryGetId(int ID)
        {
            foreach (var c in m_guilds)
            {
                foreach (var d in c.Value.Members)
                {
                    if (d.Id == ID)
                        return d;
                }
            }

            return null;
        }

        public void AddPrism(PrismNpc prism)
        {
            Prisms.Add(prism);
        }

        public bool TryAddGuild(Guild guild)
        {
            bool result;

            lock (m_lock)
            {
                if (guild.Alliance != null)
                {
                    result = false;
                }
                else
                {
                    m_guilds.Add(guild.Id, guild);
                    guild.SetAlliance(this);

                    foreach (var client in guild.Clients)
                    {
                        if (!Clients.Contains(client))
                            Clients.Add(client);
                    }

                    if (m_guilds.Count == 1)
                        SetBoss(guild);

                    OnGuildAdded(guild);
                    result = true;
                }
            }

            return result;
        }

        public Guild GetGuildById(uint id)
        {
            return m_guilds.TryGetValue((int)id, out var guild) ? guild : null;
        }

        public AllianceInformation GetAllianceInformations()
        {
            return new AllianceInformation((uint)Id, Tag, Name, Emblem.GetNetworkGuildEmblem());
        }

        public BasicNamedAllianceInformations GetBasicNamedAllianceInformations()
        {
            return new BasicNamedAllianceInformations((uint)Id, Tag, Name);
        }

        public AllianceFactSheetInformation GetAllianceFactSheetInformations()
        {
            AllianceFactSheetInformation _factSheet = new AllianceFactSheetInformation(
                allianceId: (uint)Id,
                allianceTag: Tag,
                allianceName: Name,
                allianceEmblem: Emblem.GetNetworkGuildEmblem(),
                creationDate: Record.CreationDate.GetUnixTimeStamp(),
                nbMembers: Members,
                nbSubarea: 0,
                nbTaxCollectors: 0,
                recruitment: new AllianceRecruitmentInformation());

            return _factSheet;
        }

        public IEnumerable<GuildInsiderFactSheetInformations> GetGuildsInformations()
        {
            var _insiderFactSheet = from guild in m_guilds
                                    select new GuildInsiderFactSheetInformations(
                guildId: (uint)guild.Value.Id,
                guildName: guild.Value.Level > 200 ? "° " + guild.Value.Name + " °" : guild.Value.Name,
                guildLevel: guild.Value.Level > 200 ? (byte)(guild.Value.Level - 200) : (byte)guild.Value.Level,
                guildEmblem: guild.Value.Emblem.GetNetworkGuildEmblem(),
                leaderId: (uint)guild.Value.Boss.Id,
                nbMembers: (ushort)guild.Value.Members.Count,
                lastActivityDay: 0,
                recruitment: null,
                nbPendingApply: 0,
                leaderName: guild.Value.Boss.Name);

            return _insiderFactSheet;
        }

        public IEnumerable<AllianceMemberInfo> GetAllianceMemberInfo()
        {
            var allianceMemberInfo = m_guilds.Values
                .SelectMany(guild => guild.Members)
                .Select(member => new AllianceMemberInfo
                {
                    id = (ulong)member.Id,
                    name = member.Name,
                    level = Singleton<ExperienceManager>.Instance.GetCharacterLevel(member.Experience),
                    breed = (sbyte)member.Breed,
                    sex = member.Sex == SexTypeEnum.SEX_MALE,
                    connected = (sbyte)(member.IsConnected ? 1 : 0),
                    hoursSinceLastConnection = (ushort)Math.Truncate(DateTime.Now.Subtract((DateTime)member.LastConnection).TotalHours),
                    accountId = member.Record.AccountId,
                    status = new PlayerStatus(),
                    rankId = member.RankId,
                    enrollmentDate = ConvertToTimestamp(DateTime.Now),
                    avaRoleId = 0
                });

            return allianceMemberInfo;
        }

        private double ConvertToTimestamp(DateTime date)
        {
            return (date - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        }

        public IEnumerable<PrismGeolocalizedInformation> GetPrismsInformations()
        {
            if (Prisms != null)
            {
                var list =
                    Prisms.Select(
                        prismRecord =>
                            new PrismGeolocalizedInformation((ushort)prismRecord.SubArea.Id,
                                (uint)prismRecord.Alliance.Id, //TODO Send by original
                                (short)prismRecord.Map.Position.X, (short)prismRecord.Map.Position.Y,
                                prismRecord.Map.Id,
                                prismRecord.GetAllianceInsiderPrismInformation()))
                        .ToList();

                return list;
            }

            return new PrismGeolocalizedInformation[0];
        }

        public IEnumerable<CharacterMinimalSocialPublicInformations> GetCharacterMinimalSocialPublicInformations()
        {
            return from guild in m_guilds
                   select new CharacterMinimalSocialPublicInformations(
                id: (uint)guild.Value.Id,
                name: guild.Value.Level > 200 ? "° " + guild.Value.Name + " °" : guild.Value.Name,
                level: guild.Value.Level > 200 ? (byte)(guild.Value.Level - 200) : (byte)guild.Value.Level,
                rank: new RankPublicInformation());
        }

        protected virtual void OnGuildAdded(Guild guild)
        {
            foreach (var member in guild.Members)
            {
                if (member.IsConnected)
                {
                    AllianceHandler.SendAllianceJoinedMessage(member.Character.Client, this);

                    if (this.BulletinContent != null)
                        AllianceHandler.SendAllianceBulletinMessage(member.Character.Client, this);

                    if (this.MotdContent != null)
                        AllianceHandler.SendAllianceMotdMessage(member.Character.Client, this);

                    member.Character.RefreshActor();
                    member.Character.AddEmote(EmotesEnum.EMOTE_ALLIANCE);
                }
            }
        }

        public bool KickGuild(Guild guild)
        {
            if (guild == null || !m_guilds.Values.Contains(guild))
                return false;

            m_guilds.Remove(guild.Id);
            IsDirty = true;
            guild.IsDirty = true;

            if (m_guilds.Count == 0)
            {
                AllianceManager.Instance.DeleteAlliance(this);
            }
            else if (guild == guild.Alliance.Boss)
            {
                var newBoss = m_guilds.OrderByDescending(x => x.Value.Experience).FirstOrDefault();

                if (newBoss.Value != null)
                {
                    SetBoss(newBoss.Value);

                    BasicHandler.SendTextInformationMessage(Clients, TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 199, newBoss.Value.Name, guild.Name, Name);
                }
            }

            var allianceold = guild.Alliance;
            guild.Alliance = null;

            foreach (var member in guild.Members)
            {
                if (member.IsConnected)
                {
                    member.Character.RemoveEmote(EmotesEnum.EMOTE_ALLIANCE);
                    member.Character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 410, allianceold.Name);
                    member.Character.Client.Send(new AllianceLeftMessage());
                    member.Character.RefreshActor();
                }
            }

            return true;
        }

        public SocialGroupCreationResultEnum SetAllianceName(Character character, string name, string tag)
        {
            var potion = character.Inventory.TryGetItem(ItemManager.Instance.TryGetTemplate(ItemIdEnum.POTION_DE_RENOMMAGE_DALLIANCE_14291));

            if (potion == null)
                return SocialGroupCreationResultEnum.SOCIAL_GROUP_CREATE_ERROR_REQUIREMENT_UNMET;

            if (!Regex.IsMatch(name, "^([A-Z][a-z\u00E0-\u00FC']{2,14}(\\s|-)?)([A-Z]?[a-z\u00E0-\u00FC']{1,15}(\\s|-)?){0,2}([A-Z]?[a-z\u00E0-\u00FC']{1,15})?$", RegexOptions.Compiled))
            {
                return SocialGroupCreationResultEnum.SOCIAL_GROUP_CREATE_ERROR_NAME_INVALID;
            }

            if (AllianceManager.Instance.DoesNameExist(name))
                return SocialGroupCreationResultEnum.SOCIAL_GROUP_CREATE_ERROR_NAME_ALREADY_EXISTS;

            if (AllianceManager.Instance.DoesTagExist(tag))
                return SocialGroupCreationResultEnum.SOCIAL_GROUP_CREATE_ERROR_NAME_ALREADY_EXISTS;

            character.Inventory.RemoveItem(potion, 1);

            Name = name;
            Tag = tag;

            foreach (var guild in m_guilds)
            {
                foreach (var taxCollector in guild.Value.TaxCollectors)
                {
                    taxCollector.RefreshLook();
                    taxCollector.Map.Refresh(taxCollector);
                }
            }

            foreach (var taxPrisms in TaxPrisms)
            {
                taxPrisms.RefreshLook();
                taxPrisms.Map.Refresh(taxPrisms);
            }

            foreach (var guild in m_guilds)
            {
                foreach (var member in guild.Value.Members.Where(x => x.IsConnected))
                {
                    member.Character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 408);
                    AllianceHandler.SendAllianceMembershipMessage(member.Character.Client, member.Character.Guild?.Alliance);

                    member.Character.RefreshActor();
                }
            }

            return SocialGroupCreationResultEnum.SOCIAL_GROUP_CREATE_OK;
        }

        public SocialGroupCreationResultEnum SetAllianceEmblem(Character character, NetworkGuildEmblem emblem)
        {
            var potion = character.Inventory.TryGetItem(ItemManager.Instance.TryGetTemplate(ItemIdEnum.POTION_DE_CHANGEMENT_DE_BLASON_DALLIANCE_14292));

            if (potion == null)
                return SocialGroupCreationResultEnum.SOCIAL_GROUP_CREATE_ERROR_REQUIREMENT_UNMET;

            if (AllianceManager.Instance.DoesEmblemExist(emblem))
                return SocialGroupCreationResultEnum.SOCIAL_GROUP_CREATE_ERROR_EMBLEM_ALREADY_EXISTS;

            character.Inventory.RemoveItem(potion, 1);

            Emblem.ChangeEmblem(emblem);

            foreach (var guild in m_guilds)
            {
                foreach (var taxCollector in guild.Value.TaxCollectors)
                {
                    taxCollector.RefreshLook();
                    taxCollector.Map.Refresh(taxCollector);
                }
            }

            foreach (var taxPrisms in TaxPrisms)
            {
                taxPrisms.RefreshLook();
                taxPrisms.Map.Refresh(taxPrisms);
            }

            foreach (var guild in m_guilds)
            {
                foreach (var member in guild.Value.Members.Where(x => x.IsConnected))
                {
                    member.Character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 409);
                    AllianceHandler.SendAllianceMembershipMessage(member.Character.Client, member.Character.Guild?.Alliance);

                    member.Character.RefreshActor();
                }
            }

            return SocialGroupCreationResultEnum.SOCIAL_GROUP_CREATE_OK;
        }

        public void SetBoss(Guild guild)
        {
            Dictionary<int, Guild> temp = new Dictionary<int, Guild>();

            temp.Add(guild.Id, guild);

            foreach (var a in m_guilds.ToList())
            {
                if (a.Value.Id != guild.Id)
                {
                    temp.Add(a.Value.Id, a.Value);
                }
            }

            m_guilds = temp;
            Boss = guild;

            if (Record.Owner != Boss.Id)
            {
                Record.Owner = Boss.Id;
                IsDirty = true;
            }
        }

        public BasicAllianceInformations GetBasicAllianceInformations()
        {
            return new BasicAllianceInformations(allianceId: (uint)Id, allianceTag: this.Tag);
        }

        public void SendPrismsInfoValidMessage()
        {
            //Clients.Send(new PrismsInfoValidMessage(Prisms.Where(x => x.IsFighting).Select(x => x.Fighter.GetPrismFightersInformation())));
        }

        public void SendInformationMessage(TextInformationTypeEnum msgType, short msgId, params object[] parameters)
        {
            BasicHandler.SendTextInformationMessage(Clients, msgType, msgId, parameters);
        }
        #endregion

        #region >> World Save
        public void Save(ORM.Database database)
        {
            WorldServer.Instance.IOTaskPool.AddMessage(() =>
            {
                try
                {
                    if (Record.IsNew)
                    {
                        database.Insert(Record);
                    }
                    else
                    {
                        database.Update(Record);
                    }

                    IsDirty = false;
                    Record.IsNew = false;
                }
                catch (Exception ex)
                {
                    logger.Error($"Error saving Alliance: {ex.Message}");
                }
            });
        }
        #endregion
    }
}