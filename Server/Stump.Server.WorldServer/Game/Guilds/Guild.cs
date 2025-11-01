using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using Stump.Core.Attributes;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Messages;
using Stump.DofusProtocol.Types;
using Stump.Server.WorldServer.Core.Network;
using Stump.Server.WorldServer.Database.Guilds;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.TaxCollectors;
using Stump.Server.WorldServer.Game.Items;
using Stump.Server.WorldServer.Game.Maps.Paddocks;
using Stump.Server.WorldServer.Game.Spells;
using Stump.Server.WorldServer.Handlers.Basic;
using Stump.Server.WorldServer.Handlers.TaxCollector;
using Stump.Server.WorldServer.Handlers.Guilds;
using NetworkGuildEmblem = Stump.DofusProtocol.Types.SocialEmblem;
using Stump.Server.WorldServer.Game.Alliances;
using Stump.Core.Reflection;
using Stump.Core.Extensions;
using Stump.Server.WorldServer.Handlers.Alliances;
using Stump.Server.BaseServer.Logging;
using MongoDB.Bson;
using System.Globalization;

namespace Stump.Server.WorldServer.Game.Guilds
{
    public class Guild
    {
        static readonly Logger logger = LogManager.GetCurrentClassLogger();

        static readonly double[][] XP_PER_GAP =
        {
            new double[] {0, 10},
            new double[] {10, 8},
            new double[] {20, 6},
            new double[] {30, 4},
            new double[] {40, 3},
            new double[] {50, 2},
            new double[] {60, 1.5},
            new double[] {70, 1}
        };

        public static readonly short[] TAX_COLLECTOR_SPELLS =
        {
            (short) SpellIdEnum.ROCK_458,
            (short) SpellIdEnum.WAVE_457,
            (short) SpellIdEnum.CYCLONE_456,
            (short) SpellIdEnum.FLAME_455,
            (short) SpellIdEnum.DESTABILISATION_462,
            (short) SpellIdEnum.UNBEWITCHMENT_460,
            (short) SpellIdEnum.WORD_OF_HEALING_459,
            (short) SpellIdEnum.AQUEOUS_SHIELD_451,
            (short) SpellIdEnum.EARTH_SHIELD_453,
            (short) SpellIdEnum.WIND_SHIELD_454,
            (short) SpellIdEnum.GLOWING_SHIELD_452,
            (short) SpellIdEnum.MASS_COMPULSION_461,
        };

        public const int TAX_COLLECTOR_MAX_PODS = 5000;
        public const int TAX_COLLECTOR_MAX_PROSPECTING = 500;
        public const int TAX_COLLECTOR_MAX_TAX = 50;
        public const int TAX_COLLECTOR_MAX_WISDOM = 400;

        [Variable(true)]
        public static int BaseMaxMembers = 30;

        [Variable(true)]
        public static int MaxGuildXP = 500000;

        readonly List<GuildMember> m_members = new List<GuildMember>();
        private readonly List<Paddock> m_paddocks = new List<Paddock>();
        readonly WorldClientCollection m_clients = new WorldClientCollection();
        readonly List<TaxCollectorNpc> m_taxCollectors = new List<TaxCollectorNpc>();
        readonly Spell[] m_spells = new Spell[TAX_COLLECTOR_SPELLS.Length];
        bool m_isDirty;
        private Alliance m_alliance;

        public Guild(GuildRecord record, IEnumerable<GuildMember> members)
        {

            Record = record;
            m_members.AddRange(members);
            Level = ExperienceManager.Instance.GetGuildLevel(Experience);
            ExperienceLevelFloor = ExperienceManager.Instance.GetGuildLevelExperience(Level);
            ExperienceNextLevelFloor = ExperienceManager.Instance.GetGuildNextLevelExperience(Level);
            Emblem = new GuildEmblem(Record);

            BulletinDate = DateTime.Now;
            LastNotifiedDate = DateTime.Now;

            if (m_members.Count == 0 && !record.IsNew)
            {
                logger.Error("Guild {0} ({1}) is empty", Id, Name);
                return;
            }

            foreach (var member in m_members)
            {
                BindMemberEvents(member);
                member.BindGuild(this);
            }

            if (Boss == null && !record.IsNew)
            {
                logger.Error("There is at no boss in guild {0} ({1}) -> Promote new Boss", Id, Name);
                var newBoss = Members.OrderBy(x => x.RankId).FirstOrDefault();
                if (newBoss != null)
                    SetBoss(newBoss);
            }

            // load spells
            for (var i = 0; i < record.Spells.Length && i < TAX_COLLECTOR_SPELLS.Length; i++)
            {
                if (record.Spells[i] == 0)
                    continue;

                m_spells[i] = new Spell(TAX_COLLECTOR_SPELLS[i], (byte)record.Spells[i]);
            }

            //load paddock 
        }

        public ReadOnlyCollection<GuildMember> Members => m_members.AsReadOnly();

        public WorldClientCollection Clients => m_clients;

        public ReadOnlyCollection<TaxCollectorNpc> TaxCollectors => m_taxCollectors.AsReadOnly();

        public GuildRecord Record
        {
            get;
            set;
        }

        public int Id
        {
            get { return Record.Id; }
            private set { Record.Id = value; }
        }

        public GuildMember Boss => Members.FirstOrDefault(x => x.RankId == 1);

        public int MaxMembers
        {
            get { return BaseMaxMembers + (Level / 2); }
        }

        public long Experience
        {
            get { return Record.Experience; }
            protected set
            {
                Record.Experience = value;
                IsDirty = true;
            }
        }

        public uint Boost
        {
            get { return Record.Boost; }
            protected set
            {
                Record.Boost = value;
                IsDirty = true;
            }
        }

        public int TaxCollectorProspecting
        {
            get { return Record.Prospecting; }
            protected set
            {
                Record.Prospecting = value;
                IsDirty = true;
            }
        }

        public int TaxCollectorWisdom
        {
            get { return Record.Wisdom; }
            protected set
            {
                Record.Wisdom = value;
                IsDirty = true;
            }
        }

        public int TaxCollectorPods
        {
            get { return Record.Pods; }
            protected set
            {
                Record.Pods = value;
                IsDirty = true;
            }
        }

        public int TaxCollectorHealth => 100 * Level;

        public int TaxCollectorResistance => Level > 50 ? 50 : Level;

        public int TaxCollectorDamageBonuses => Level;

        public int MaxTaxCollectors
        {
            get { return Record.MaxTaxCollectors; }
            protected set
            {
                Record.MaxTaxCollectors = value;
                IsDirty = true;
            }
        }
        public int MaxPaddocks
        {
            get { return (int)Math.Round((Level / 10d), 0); }

        }

        public long ExperienceLevelFloor
        {
            get;
            protected set;
        }

        public long ExperienceNextLevelFloor
        {
            get;
            protected set;
        }

        public DateTime CreationDate => Record.CreationDate;

        public string Name
        {
            get { return Record.Name; }
            protected set
            {
                Record.Name = value;
                IsDirty = true;
            }
        }

        public GuildEmblem Emblem
        {
            get;
            protected set;
        }

        public ushort Level
        {
            get;
            protected set;
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
            protected set
            {
                Record.MotdDate = value;
            }
        }

        public GuildMember MotdMember
        {
            get { return TryGetMember(Record.MotdMemberId); }
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

        public GuildMember BulletinMember
        {
            get { return TryGetMember(Record.BulletinMemberId); }
            protected set
            {
                Record.BulletinMemberId = value.Id;
            }
        }

        public ReadOnlyCollection<Paddock> Paddocks => m_paddocks.AsReadOnly();

        public bool IsDirty
        {
            get { return m_isDirty || Emblem.IsDirty || Members.Any(x => x.IsDirty || x.IsNew); }
            set
            {
                m_isDirty = value;

                if (!value)
                    Emblem.IsDirty = false;
            }
        }

        public Alliance Alliance
        {
            get
            {
                return this.m_alliance;
            }
            set
            {
                this.m_alliance = value;
                this.Record.AllianceId = (this.m_alliance == null ? new int?() : this.m_alliance.Id);
                this.IsDirty = true;
            }
        }

        public long PercsKills
        {
            get { return Record.PercsKills; }
            protected set
            {
                Record.PercsKills = value;
                IsDirty = true;
            }
        }

        public long PercsDefenders
        {
            get { return Record.PercsDefenders; }
            protected set
            {
                Record.PercsDefenders = value;
                IsDirty = true;
            }
        }

        public void UpdateKills()
        {
            PercsKills += 1;
        }

        public void UpdateDefenders()
        {
            PercsDefenders += 1;
        }

        public void UpdateMotd(GuildMember member, string content)
        {
            MotdContent = content;
            MotdMember = member;
            MotdDate = DateTime.Now;
            GuildHandler.SendGuildMotdMessage(m_clients, this);
        }

        public void UpdateBulletin(GuildMember member, string content, bool notify = true)
        {
            BulletinContent = content;
            BulletinMember = member;
            BulletinDate = DateTime.Now;

            if (notify)
                LastNotifiedDate = DateTime.Now;

            GuildHandler.SendGuildBulletinMessage(m_clients, this);
        }

        public void AddPaddock(Paddock paddock)
        {
            m_paddocks.Add(paddock);
            //GuildHandler.SendGuildInformationsPaddocksMessage(this.Clients, this);
            GuildHandler.SendGuildInformationsPaddocksMessage(paddock.Guild.Clients, paddock.Guild);
        }

        public void RemovePaddock(Paddock paddock)
        {
            m_paddocks.Remove(paddock);
            GuildHandler.SendGuildInformationsPaddocksMessage(paddock.Guild.Clients, paddock.Guild);
        }

        public void AddTaxCollector(TaxCollectorNpc taxCollector)
        {
            m_taxCollectors.Add(taxCollector);
        }

        public void RemoveTaxCollector(TaxCollectorNpc taxCollector)
        {
            m_taxCollectors.Remove(taxCollector);
            TaxCollectorManager.Instance.RemoveTaxCollectorSpawn(taxCollector);
            TaxCollectorHandler.SendTaxCollectorMovementRemoveMessage(taxCollector.Guild.Clients, taxCollector);
        }

        public void RemoveTaxCollectors()
        {
            foreach (var taxCollector in m_taxCollectors.ToArray())
            {
                RemoveTaxCollector(taxCollector);
            }
        }

        public void RemoveGuildMembers()
        {
            foreach (var member in m_members.ToArray())
            {
                RemoveMember(member);
            }
        }

        public float AdjustGivenExperience(Character giver, float amount)
        {
            var gap = giver.Level - Level;

            for (var i = XP_PER_GAP.Length - 1; i >= 0; i--)
            {
                if (gap > XP_PER_GAP[i][0])
                    return (float)(amount * XP_PER_GAP[i][1] * 0.01);
            }

            return (float)(amount * XP_PER_GAP[0][1] * 0.01);
        }

        public void AddXP(long experience)
        {
            Experience += experience;

            var level = ExperienceManager.Instance.GetGuildLevel(Experience);

            if (level == Level)
                return;

            if (level > Level)
                Boost += (uint)((level - Level) * 5);

            Level = level;
            OnLevelChanged();
        }

        public void SetXP(long experience)
        {
            Experience = experience;

            var level = ExperienceManager.Instance.GetGuildLevel(Experience);

            if (level == Level) return;

            Level = level;
            OnLevelChanged();
        }

        public bool UpgradeTaxCollectorPods()
        {
            if (TaxCollectorPods >= TAX_COLLECTOR_MAX_PODS)
                return false;

            if (Boost <= 0)
                return false;

            Boost -= 1;
            TaxCollectorPods += 20;

            if (TaxCollectorPods > TAX_COLLECTOR_MAX_PODS)
                TaxCollectorPods = TAX_COLLECTOR_MAX_PODS;

            return true;
        }

        public bool UpgradeTaxCollectorProspecting()
        {
            if (TaxCollectorProspecting >= TAX_COLLECTOR_MAX_PROSPECTING)
                return false;

            if (Boost <= 0)
                return false;

            Boost -= 1;
            TaxCollectorProspecting += 1;

            if (TaxCollectorProspecting > TAX_COLLECTOR_MAX_PROSPECTING)
                TaxCollectorProspecting = TAX_COLLECTOR_MAX_PROSPECTING;

            return true;
        }

        public bool UpgradeTaxCollectorWisdom()
        {
            if (TaxCollectorWisdom >= TAX_COLLECTOR_MAX_WISDOM)
                return false;

            if (Boost <= 0)
                return false;

            Boost -= 1;
            TaxCollectorWisdom += 1;

            if (TaxCollectorWisdom > TAX_COLLECTOR_MAX_WISDOM)
                TaxCollectorWisdom = TAX_COLLECTOR_MAX_WISDOM;

            return true;
        }

        public bool UpgradeMaxTaxCollectors()
        {
            if (MaxTaxCollectors >= TAX_COLLECTOR_MAX_TAX)
                return false;

            if (Boost < 10)
                return false;

            Boost -= 10;
            MaxTaxCollectors += 1;

            if (MaxTaxCollectors > TAX_COLLECTOR_MAX_TAX)
                MaxTaxCollectors = TAX_COLLECTOR_MAX_TAX;

            return true;
        }

        public bool UpgradeSpell(int spellId)
        {
            var spellIndex = Array.IndexOf(TAX_COLLECTOR_SPELLS, (short)spellId);

            if (spellIndex == -1)
                return false;

            if (Boost < 5)
                return false;

            var spell = m_spells[spellIndex];

            if (spell == null)
            {
                var template = SpellManager.Instance.GetSpellTemplate(spellId);

                if (template == null)
                {
                    logger.Error("Cannot boost tax collector spell {0}, template not found", spellId);
                    return false;
                }

                m_spells[spellIndex] = new Spell(template, 1);
            }
            else
            {
                if (!spell.BoostSpell())
                    return false;
            }

            Boost -= 5;
            return true;
        }

        public bool UnBoostSpell(int spellId)
        {
            var spellIndex = Array.IndexOf(TAX_COLLECTOR_SPELLS, (short)spellId);

            if (spellIndex == -1)
                return false;

            var spell = m_spells[spellIndex];

            if (spell == null)
                return false;

            if (!spell.UnBoostSpell())
                return false;

            Boost += 5;
            return true;
        }

        public ReadOnlyCollection<Spell> GetTaxCollectorSpells() => m_spells.Where(x => x != null).ToList().AsReadOnly();

        public int[] GetTaxCollectorSpellsLevels() => m_spells.Select(x => x == null ? 0 : x.CurrentLevel).ToArray();

        public SocialGroupCreationResultEnum SetGuildName(Character character, string name)
        {
            var potion = character.Inventory.TryGetItem(ItemManager.Instance.TryGetTemplate(ItemIdEnum.POTION_DE_RENOMMAGE_DE_GUILDE_13273));
            if (potion == null)
                return SocialGroupCreationResultEnum.SOCIAL_GROUP_CREATE_ERROR_REQUIREMENT_UNMET;

            if (!Regex.IsMatch(name, "^([A-Z][a-z\u00E0-\u00FC']{2,14}(\\s|-)?)([A-Z]?[a-z\u00E0-\u00FC']{1,15}(\\s|-)?){0,2}([A-Z]?[a-z\u00E0-\u00FC']{1,15})?$", RegexOptions.Compiled))
            {
                return SocialGroupCreationResultEnum.SOCIAL_GROUP_CREATE_ERROR_NAME_INVALID;
            }

            if (GuildManager.Instance.DoesNameExist(name))
                return SocialGroupCreationResultEnum.SOCIAL_GROUP_CREATE_ERROR_NAME_ALREADY_EXISTS;

            character.Inventory.RemoveItem(potion, 1);

            Name = name;

            foreach (var taxCollector in TaxCollectors)
            {
                taxCollector.RefreshLook();
                taxCollector.Map.Refresh(taxCollector);
            }

            foreach (var member in Members)
            {
                member.Character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 383);
                GuildHandler.SendGuildMembershipMessage(member.Character.Client, member.Character.GuildMember);

                member.Character.RefreshActor();
            }

            return SocialGroupCreationResultEnum.SOCIAL_GROUP_CREATE_OK;
        }

        public SocialGroupCreationResultEnum SetGuildEmblem(Character character, NetworkGuildEmblem emblem)
        {
            var potion = character.Inventory.TryGetItem(ItemManager.Instance.TryGetTemplate(ItemIdEnum.POTION_DE_CHANGEMENT_DE_BLASON_DE_GUILDE_13270));

            if (potion == null)
                return SocialGroupCreationResultEnum.SOCIAL_GROUP_CREATE_ERROR_REQUIREMENT_UNMET;

            if (GuildManager.Instance.DoesEmblemExist(emblem))
                return SocialGroupCreationResultEnum.SOCIAL_GROUP_CREATE_ERROR_EMBLEM_ALREADY_EXISTS;

            character.Inventory.RemoveItem(potion, 1);

            Emblem.ChangeEmblem(emblem);

            foreach (var taxCollector in TaxCollectors)
            {
                taxCollector.RefreshLook();
                taxCollector.Map.Refresh(taxCollector);
            }

            foreach (var member in Members.Where(x => x.IsConnected))
            {
                member.Character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 382);
                GuildHandler.SendGuildMembershipMessage(member.Character.Client, member.Character.GuildMember);

                member.Character.RefreshActor();
            }

            return SocialGroupCreationResultEnum.SOCIAL_GROUP_CREATE_OK;
        }

        public void SetBoss(GuildMember guildMember)
        {
            if (guildMember.Guild != this)
                return;

            if (Boss != null)
            {
                if (Boss == guildMember)
                    return;

                var oldBoss = Boss;

                oldBoss.RankId = 2;
                oldBoss.Rights = GuildRightsBitEnum.GUILD_RIGHT_MANAGE_RIGHTS;

                BasicHandler.SendTextInformationMessage(m_clients, TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 199, guildMember.Name, oldBoss.Name, Name); // <b>%1</b> a remplacé <b>%2</b>  au poste de meneur de la guilde <b>%3</b>
                UpdateMember(oldBoss);
            }

            guildMember.RankId = 1;
            guildMember.Rights = GuildRightsBitEnum.GUILD_RIGHT_BOSS;

            UpdateMember(guildMember);
        }

        public void SetAlliance(Alliance alliance)
        {
            m_alliance = alliance;
            Alliance = alliance;
        }

        public bool KickMember(GuildMember from, GuildMember kickedMember)
        {
            var leave = from.Id == kickedMember.Id;

            if (!from.HasRight(GuildRightsBitEnum.GUILD_RIGHT_BAN_MEMBERS) && !leave)
                return false;

            if (kickedMember.IsBoss && (!leave || Members.Count > 1))
                return false;

            if (!RemoveMember(kickedMember))
                return false;

            if (!leave)
                from.Character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 177, kickedMember.Name);  // Vous avez banni <b>%1</b> de votre guilde.

            GuildHandler.SendGuildMemberLeavingMessage(m_clients, kickedMember, !leave);

            return true;
        }

        public bool ChangeParameters(Character from, GuildMember member, short rankId, byte xpPercent, uint rights)
        {
            if (from.GuildMember.HasRight(GuildRightsBitEnum.GUILD_RIGHT_MANAGE_RANKS) ||
                from.GuildMember.HasRight(GuildRightsBitEnum.GUILD_RIGHT_MANAGE_RIGHTS) ||
                from.GuildMember.HasRight(GuildRightsBitEnum.GUILD_RIGHT_MANAGE_XP_CONTRIBUTION) ||
                from.GuildMember.HasRight(GuildRightsBitEnum.GUILD_RIGHT_MANAGE_MY_XP_CONTRIBUTION))
            {
                if (rankId < 0 || rankId > 35)
                    return false;

                if (xpPercent < 0 || xpPercent > 90)
                    return false;

                if (from.GuildMember.HasRight(GuildRightsBitEnum.GUILD_RIGHT_MANAGE_RANKS) && !member.IsBoss)
                {
                    if (rankId == 1)
                    {
                        if (from.GuildMember.IsBoss)
                            SetBoss(member);
                    }
                    else
                    {
                        member.RankId = rankId;
                    }
                }

                if (from.GuildMember.HasRight(GuildRightsBitEnum.GUILD_RIGHT_MANAGE_RIGHTS) && !member.IsBoss && from.GuildMember.Id != member.Id)
                    member.Rights = (GuildRightsBitEnum)rights;

                if (from.GuildMember.HasRight(GuildRightsBitEnum.GUILD_RIGHT_MANAGE_XP_CONTRIBUTION) || (from.GuildMember == member &&
                    from.GuildMember.HasRight(GuildRightsBitEnum.GUILD_RIGHT_MANAGE_MY_XP_CONTRIBUTION)))
                {
                    member.GivenPercent = xpPercent;
                }
            }

            UpdateMember(member);

            if (member.IsConnected)
            {
                GuildHandler.SendGuildMembershipMessage(member.Character.Client, member);
                GuildHandler.SendGuildInformationsGeneralMessage(member.Character.Client, this);
            }

            return true;
        }

        public void UpdateMember(GuildMember member)
        {
            GuildHandler.SendGuildInformationsMemberUpdateMessage(m_clients, member);
        }

        public bool CanAddMember() => m_members.Count < MaxMembers;

        public GuildMember TryGetMember(int id) => m_members.FirstOrDefault(x => x.Id == id);

        public bool TryAddMember(Character character)
        {
            GuildMember dummy;
            return TryAddMember(character, out dummy);
        }

        public bool TryAddMember(Character character, out GuildMember member)
        {
            if (!CanAddMember())
            {
                member = null;
                return false;
            }

            member = new GuildMember(this, character);
            m_members.Add(member);
            character.GuildMember = member;

            m_clients.Add(character.Client);

            if (m_members.Count == 1)
                SetBoss(member);

            OnMemberAdded(member);

            return true;
        }

        //public bool TryAddPaddock(Character character, out Paddock paddock)
        //{
        //    // if (!CanAddMember())
        //    //{
        //    //  member = null;
        //    // return false;
        //    //}
        //    WorldMapPaddockRecord paddock_temp = new WorldMapPaddockRecord();
        //    paddock_temp.GuildId = this.Id;
        //    paddock_temp.Locked = false;
        //    paddock_temp.MapId = 0;
        //    paddock_temp.
        //    paddock = new Paddock(this, character);
        //    m_members.Add(member);
        //    character.GuildMember = member;

        //    m_clients.Add(character.Client);

        //    if (m_members.Count == 1)
        //        SetBoss(member);

        //    OnMemberAdded(member);

        //    return true;
        //}

        public bool RemoveMember(GuildMember member)
        {
            if (member == null || !m_members.Contains(member))
                return false;

            m_members.Remove(member);

            if (member.IsConnected)
                m_clients.Remove(member.Character.Client);

            OnMemberRemoved(member);

            return true;
        }

        protected virtual void OnMemberAdded(GuildMember member)
        {
            IsDirty = true;

            BindMemberEvents(member);
            GuildManager.Instance.RegisterGuildMember(member);

            if (member.IsConnected)
            {
                GuildHandler.SendGuildJoinedMessage(member.Character.Client, member);
                GuildHandler.SendGuildInformationsMembersMessage(member.Character.Client, this);
                GuildHandler.SendGuildInformationsGeneralMessage(member.Character.Client, this);

                if (BulletinContent != null)
                    GuildHandler.SendGuildBulletinMessage(member.Character.Client, this);

                if (MotdContent != null)
                    GuildHandler.SendGuildMotdMessage(member.Character.Client, this);

                if (Alliance != null)
                {
                    AllianceHandler.SendAllianceInsiderInfoMessage(member.Character.Client, Alliance);
                    if (Alliance.BulletinContent != null)
                        AllianceHandler.SendAllianceBulletinMessage(member.Character.Client, Alliance);

                    if (Alliance.MotdContent != null)
                        AllianceHandler.SendAllianceMotdMessage(member.Character.Client, Alliance);

                    member.Character.AddEmote(EmotesEnum.EMOTE_ALLIANCE);

                }

                member.Character.RefreshActor();
                member.Character.AddEmote(EmotesEnum.EMOTE_GUILD);

                if (Level > 200)
                    member.Character.AddEmote((EmotesEnum)155);//guildwineer
            }

            UpdateMember(member);

            var document = new BsonDocument
            {
                { "memberId", member.Id },
                { "memberName", member.Name },
                { "guildId", Id },
                { "guildName", Name },
                { "Date", DateTime.Now.ToString(CultureInfo.CreateSpecificCulture("pt-BR")) }
            };

            MongoLogger.Instance.Insert("MembersGuildAdd", document);
        }

        protected virtual void OnMemberRemoved(GuildMember member)
        {
            IsDirty = true;

            GuildManager.Instance.DeleteGuildMember(member);
            UnBindMemberEvents(member);

            if (Members.Count == 0)
            {
                GuildManager.Instance.DeleteGuild(this);
            }
            else if (member.IsBoss)
            {
                var newBoss = Members.OrderBy(x => x.RankId).FirstOrDefault();

                if (newBoss != null)
                {
                    SetBoss(newBoss);
                    BasicHandler.SendTextInformationMessage(m_clients, TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 199, newBoss.Name, member.Name, Name); // <b>%1</b> a remplacé <b>%2</b>  au poste de meneur de la guilde <b>%3</b>
                }
            }

            if (!member.IsConnected)
                return;

            member.Character.RemoveEmote(EmotesEnum.EMOTE_GUILD);

            if (member.Character.HasEmote((EmotesEnum)155))
                member.Character.RemoveEmote((EmotesEnum)155);//guildwineer

            member.Character.GuildMember = null;

            if (Alliance != null)
            {
                member.Character.RemoveEmote(EmotesEnum.EMOTE_ALLIANCE);
                member.Character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 410, Alliance.Name);
                member.Character.Client.Send(new AllianceLeftMessage());
            }

            member.Character.RefreshActor();
            member.Character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 176); // Vous avez quitté la guilde.
            GuildHandler.SendGuildLeftMessage(member.Character.Client);
        }

        protected virtual void OnLevelChanged()
        {
            ExperienceLevelFloor = ExperienceManager.Instance.GetGuildLevelExperience(Level);
            ExperienceNextLevelFloor = ExperienceManager.Instance.GetGuildNextLevelExperience(Level);

            BasicHandler.SendTextInformationMessage(m_clients, TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 208, Level); //Votre guilde passe niveau %1

            if (Level > 200)
                m_clients.Send(new GuildLevelUpMessage((byte)(Level - 200)));
            else
                m_clients.Send(new GuildLevelUpMessage((byte)Level));
        }

        void OnMemberConnected(GuildMember member)
        {
            BasicHandler.SendTextInformationMessage(m_clients, TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 224, member.Character.Name); //Un membre de votre guilde, {player,%1,%2}, est en ligne.
            m_clients.Add(member.Character.Client);

            UpdateMember(member);
            m_clients.Send(new GuildMemberOnlineStatusMessage((ulong)member.Id, true));
        }

        void OnMemberDisconnected(GuildMember member, Character character)
        {
            m_clients.Remove(character.Client);

            UpdateMember(member);

            m_clients.Send(new GuildMemberOnlineStatusMessage((ulong)member.Id, false));
            m_clients.Send(new GuildMemberLeavingMessage(false, (ulong)member.Id));
        }

        void BindMemberEvents(GuildMember member)
        {

            member.Connected += OnMemberConnected;
            member.Disconnected += OnMemberDisconnected;

        }


        void UnBindMemberEvents(GuildMember member)
        {
            member.Connected -= OnMemberConnected;
            member.Disconnected -= OnMemberDisconnected;
        }

        public GuildInformations GetGuildInformations()
        {
            if (Level > 200)
                return new GuildInformations((uint)Id, "° " + Name + " °", (byte)(Level - 200), Emblem.GetNetworkGuildEmblem());
            else
                return new GuildInformations((uint)Id, Name, (byte)Level, Emblem.GetNetworkGuildEmblem());
        }

        public BasicGuildInformations GetGuildVersatileInformations()
        {
            if (Level > 200)
                return new BasicGuildInformations((uint)Id, Name, (byte)(Level - 200));
            else
                return new BasicGuildInformations((uint)Id, Name, (byte)Level);
        }

        public GuildFactsMessage GetGuildFactsMessage()
        {
            if (Alliance != null)
            {
                if (Level > 200)
                {
                    GuildFactSheetInformations factSheetInformations = new GuildFactSheetInformations(
                        guildId: (uint)Id,
                        guildName: "° " + Name + " °",
                        guildLevel: (byte)(Level - 200),
                        guildEmblem: Emblem.GetNetworkGuildEmblem(),
                        leaderId: (uint)Boss.Id,
                        nbMembers: (ushort)Members.Count,
                        lastActivityDay: 0,
                        recruitment: new GuildRecruitmentInformation(),
                        nbPendingApply: 0);

                    GuildFactsMessage _guildFactsMessage = new GuildFactsMessage(
                        infos: factSheetInformations,
                        creationDate: Record.CreationDate.GetUnixTimeStamp(),
                        members: from x in Members select x.IsConnected ? x.Character.GuildMember.GetCharacterMinimalSocialPublicInformations() : new CharacterMinimalSocialPublicInformations((uint)x.Id, x.Name, Singleton<ExperienceManager>.Instance.GetCharacterLevel(x.Experience), null));

                    return _guildFactsMessage;
                }
                else
                {
                    var factSheetInformations = new GuildFactSheetInformations(
                        guildId: (uint)Id,
                        guildName: Name,
                        guildLevel: (byte)Level,
                        guildEmblem: Emblem.GetNetworkGuildEmblem(),
                        leaderId: (uint)Boss.Id,
                        nbMembers: (ushort)Members.Count,
                        lastActivityDay: 0,
                        recruitment: new GuildRecruitmentInformation(),
                        nbPendingApply: 0);

                    GuildFactsMessage _guildFactsMessage = new GuildFactsMessage(
                        infos: factSheetInformations,
                        creationDate: Record.CreationDate.GetUnixTimeStamp(),
                        members: from x in Members select x.IsConnected ? x.Character.GuildMember.GetCharacterMinimalSocialPublicInformations() : new CharacterMinimalSocialPublicInformations((uint)x.Id, x.Name, Singleton<ExperienceManager>.Instance.GetCharacterLevel(x.Experience), null));

                    return _guildFactsMessage;
                }
            }

            if (Level > 200)
            {
                var factSheetInformations = new GuildFactSheetInformations(
                        guildId: (uint)Id,
                        guildName: "° " + Name + " °",
                        guildLevel: (byte)(Level - 200),
                        guildEmblem: Emblem.GetNetworkGuildEmblem(),
                        leaderId: (uint)Boss.Id,
                        nbMembers: (ushort)Members.Count,
                        lastActivityDay: 0,
                        recruitment: new GuildRecruitmentInformation(),
                        nbPendingApply: 0);

                GuildFactsMessage _guildFactsMessage = new GuildFactsMessage(
                    infos: factSheetInformations,
                    creationDate: Record.CreationDate.GetUnixTimeStamp(),
                    members: from x in Members select x.IsConnected ? x.Character.GuildMember.GetCharacterMinimalSocialPublicInformations() : new CharacterMinimalSocialPublicInformations((uint)x.Id, x.Name, Singleton<ExperienceManager>.Instance.GetCharacterLevel(x.Experience), null));

                return _guildFactsMessage;
            }
            else
            {
                var factSheetInformations = new GuildFactSheetInformations(
                        guildId: (uint)Id,
                        guildName: Name,
                        guildLevel: (byte)Level,
                        guildEmblem: Emblem.GetNetworkGuildEmblem(),
                        leaderId: (uint)Boss.Id,
                        nbMembers: (ushort)Members.Count,
                        lastActivityDay: 0,
                        recruitment: new GuildRecruitmentInformation(),
                        nbPendingApply: 0);

                GuildFactsMessage _guildFactsMessage = new GuildFactsMessage(
                    infos: factSheetInformations,
                    creationDate: Record.CreationDate.GetUnixTimeStamp(),
                    members: from x in Members select x.IsConnected ? x.Character.GuildMember.GetCharacterMinimalSocialPublicInformations() : new CharacterMinimalSocialPublicInformations((uint)x.Id, x.Name, Singleton<ExperienceManager>.Instance.GetCharacterLevel(x.Experience), null));

                return _guildFactsMessage;
            }
        }

        public BasicGuildInformations GetBasicGuildInformations()
        {
            if (Level > 200)
                return new BasicGuildInformations((uint)Id, "° " + Name + " °", (byte)(Level - 200));
            else
                return new BasicGuildInformations((uint)Id, Name, (byte)Level);
        }

        public GuildFactSheetInformations GetGuildFactSheetInformations()
        {
            if (Level > 200)
                return new GuildFactSheetInformations((uint)Id, "° " + Name + " °", (byte)(Level - 200), Emblem.GetNetworkGuildEmblem(), (ulong)Boss.Id, (ushort)Members.Count, lastActivityDay: 0, recruitment: new GuildRecruitmentInformation(), nbPendingApply: 0);
            else
                return new GuildFactSheetInformations((uint)Id, Name, (byte)Level, Emblem.GetNetworkGuildEmblem(), (ulong)Boss.Id, (ushort)Members.Count, lastActivityDay: 0, recruitment: new GuildRecruitmentInformation(), nbPendingApply: 0);
        }

        #region >> World Save
        public void Save(ORM.Database database)
        {
            Record.Spells = GetTaxCollectorSpellsLevels();

            WorldServer.Instance.IOTaskPool.AddMessage(() =>
            {
                try
                {
                    database.BeginTransaction();
                    var temp = Record.IsNew;

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

                    foreach (var member in Members.Where(x => x.IsDirty || x.IsNew))
                    {
                        member.Save(database);
                    }

                    foreach (var paddock in Paddocks)
                    {
                        paddock.Save(database);
                    }

                    database.CompleteTransaction();
                }
                catch (Exception ex)
                {
                    logger.Error($"Error saving Guild: {ex.Message}");
                }
            });
        }
        #endregion
    }
}