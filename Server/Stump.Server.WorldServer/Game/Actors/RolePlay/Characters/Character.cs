using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using MongoDB.Bson;
using Stump.Core.Attributes;
using Stump.Core.Collections;
using Stump.Core.Extensions;
using Stump.Core.Threading;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Enums.Custom;
using Stump.DofusProtocol.Messages;
using Stump.DofusProtocol.Types;
using Stump.Server.BaseServer.Commands;
using Stump.Server.BaseServer.IPC.Messages;
using Stump.Server.BaseServer.IPC.Objects;
using Stump.Server.BaseServer.Logging;
using Stump.Server.WorldServer.Core.IPC;
using Stump.Server.WorldServer.Core.Network;
using Stump.Server.WorldServer.Database.Accounts;
using Stump.Server.WorldServer.Database.Breeds;
using Stump.Server.WorldServer.Database.Characters;
using Stump.Server.WorldServer.Database.Npcs.Actions;
using Stump.Server.WorldServer.Database.Quests;
using Stump.Server.WorldServer.Database.Social;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Accounts;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Actors.Interfaces;
using Stump.Server.WorldServer.Game.Actors.Look;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Merchants;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Monsters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Mounts;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Npcs;
using Stump.Server.WorldServer.Game.Actors.RolePlay.TaxCollectors;
using Stump.Server.WorldServer.Game.Actors.Stats;
using Stump.Server.WorldServer.Game.Arena;
using Stump.Server.WorldServer.Game.Breeds;
using Stump.Server.WorldServer.Game.Dialogs;
using Stump.Server.WorldServer.Game.Dialogs.Interactives;
using Stump.Server.WorldServer.Game.Dialogs.Merchants;
using Stump.Server.WorldServer.Game.Dialogs.Npcs;
using Stump.Server.WorldServer.Game.Exchanges;
using Stump.Server.WorldServer.Game.Exchanges.Trades;
using Stump.Server.WorldServer.Game.Exchanges.Trades.Players;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Fights.Teams;
using Stump.Server.WorldServer.Game.Guilds;
using Stump.Server.WorldServer.Game.Interactives;
using Stump.Server.WorldServer.Game.Interactives.Skills;
using Stump.Server.WorldServer.Game.Items.BidHouse;
using Stump.Server.WorldServer.Game.Items.Player;
using Stump.Server.WorldServer.Game.Items.Player.Custom;
using Stump.Server.WorldServer.Game.Jobs;
using Stump.Server.WorldServer.Game.Maps;
using Stump.Server.WorldServer.Game.Maps.Cells;
using Stump.Server.WorldServer.Game.Maps.Pathfinding;
using Stump.Server.WorldServer.Game.Notifications;
using Stump.Server.WorldServer.Game.Parties;
using Stump.Server.WorldServer.Game.Quests;
using Stump.Server.WorldServer.Game.Shortcuts;
using Stump.Server.WorldServer.Game.Social;
using Stump.Server.WorldServer.Game.Spells;
using Stump.Server.WorldServer.Handlers.Basic;
using Stump.Server.WorldServer.Handlers.Characters;
using Stump.Server.WorldServer.Handlers.Chat;
using Stump.Server.WorldServer.Handlers.Compass;
using Stump.Server.WorldServer.Handlers.Context;
using Stump.Server.WorldServer.Handlers.Context.RolePlay;
using Stump.Server.WorldServer.Handlers.Context.RolePlay.Party;
using Stump.Server.WorldServer.Handlers.Guilds;
using Stump.Server.WorldServer.Handlers.Initialization;
using Stump.Server.WorldServer.Handlers.Interactives;
using Stump.Server.WorldServer.Handlers.Inventory;
using Stump.Server.WorldServer.Handlers.Moderation;
using Stump.Server.WorldServer.Handlers.Mounts;
using Stump.Server.WorldServer.Handlers.Titles;
using GuildMember = Stump.Server.WorldServer.Game.Guilds.GuildMember;
using System.Threading.Tasks;
using Handlers.Spouse;
using Stump.Core.Reflection;
using Stump.Server.BaseServer;
using Stump.Server.WorldServer.Database.Companion;
using Stump.Server.WorldServer.Game.Achievements;
using Stump.Server.WorldServer.Game.Companions;
using Stump.Server.WorldServer.Game.HavenBags;
using Stump.Server.WorldServer.Game.Idols;
using Stump.Server.WorldServer.Game.Prisms;
using Stump.Server.WorldServer.Handlers.Alliances;
using Stump.Server.WorldServer.Handlers.PvP;
using Stump.Server.WorldServer.Game.Dungs;
using Stump.Server.WorldServer.Game.MapsReset;
using Stump.Server.WorldServer.Game.Mandatory;
using Stump.Server.WorldServer.Game.Dopple;
using Stump.Server.WorldServer.Database.Items.Templates;
using Stump.Server.WorldServer.Database.Achievements;
using Stump.Server.WorldServer.Database.Arena;
using Newtonsoft.Json;
using Stump.Server.WorldServer.Game.Breach;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Items;
using Stump.Server.WorldServer.Game.Items.Player.Custom.CeremonialRings;
using Stump.Server.WorldServer.Game.Arena.Leagues;
using System.Diagnostics;
using Stump.Server.WorldServer.Game.Misc.AutoEvents;
using Stump.Server.WorldServer.Game.Misc.Notify;
using MySql.Data.MySqlClient.Memcached;

namespace Stump.Server.WorldServer.Game.Actors.RolePlay.Characters
{
    public sealed class Character : Humanoid, IStatsOwner, IInventoryOwner, ICommandsUser
    {
        bool m_recordLoaded;
        public TimeSpan last;
        readonly CharacterRecord m_record;
        public bool battleFieldOn = false;
        public bool followforspouse = false;

        public int ArenaTokensMax = 0;
        public event Action<Character> KingOfHill;
        public event Action<Character> KoHRevive;
        public List<RolePlayActor> following = new List<RolePlayActor>();
        public List<IndexedEntityLook> itemsFollowsLook = new List<IndexedEntityLook>();
        public List<SpellModifierMessage> SpellsModifications = new List<SpellModifierMessage>();

        static Stopwatch CharacterGameTime = new Stopwatch();

        public DoppleCollection DoppleCollection { get; private set; }

        public MandatoryCollection MandatoryCollection { get; private set; }

        [Variable]
        public static ushort HonorLimit = 20000;

        [Variable(true)]
        public static int[] SpellsBlock =
        {
            //9631,154,9632,151
        };

        public int[] SpellsBlock_temp
        {
            get
            {
                return SpellsBlock;
            }
        }

        [Variable(true)]
        public static int[] SpellsBugs =
        {
            //9631,154,9632,151
        };

        public int[] SpellsBugs_temp
        {
            get
            {
                return SpellsBugs;
            }
        }

        [Variable(true)]
        public static long[] DungeonsAdd =
        {
            //9631,154,9632,151
        };

        [Variable(true)]
        public static long[] DungsDiscoverBlock =
        {
            //9631,154,9632,151
        };

        DateTime loginTime;

        public DateTime NextPresetTime { get; set; }

        public Character(Character record)
        {

        }

        public Character(CharacterRecord record, WorldClient client)
        {
            m_record = record;
            Client = client;
            SaveSync = new object();
            LoggoutSync = new object();
            Status = new PlayerStatus((sbyte)PlayerStatusEnum.PLAYER_STATUS_AVAILABLE);
            NextPresetTime = DateTime.Now;
            //no se cual llame al pc que llama al metodo
            //se ebe crear un metodo que llene la variable Dopeul con los datos necesarios
        }

        #region Events

        public event Action<Character> LoggedIn;

        //public bool IsFirstConnection
        //{
        //    get { return Record.FirstConnection; }
        //    set
        //    {
        //        Record.FirstConnection = value;
        //    }
        //}

        public int ChallengesCount
        {
            get { return Record.ChallengesCount; }
            set { Record.ChallengesCount = value; }
        }

        public int ChallengesInDungeonCount
        {
            get { return Record.ChallengesInDungeonCount; }
            set { Record.ChallengesInDungeonCount = value; }
        }

        public sbyte ChallengeMod
        {
            get { return Record.ChallengeMod; }
            set { Record.ChallengeMod = value; }
        }

        public sbyte ChallengeXpOrDrop
        {
            get { return Record.ChallengeXpOrDrop; }
            set { Record.ChallengeXpOrDrop = value; }
        }

        public int OwnedRuneAmount
        {
            get { return Record.OwnedRuneAmount; }
            set { Record.OwnedRuneAmount = value; }
        }

        void OnLoggedIn()
        {
            #region MSGs
            if (CurrentSpouse != 0)
            {
                var spouse = World.Instance.GetCharacter(CurrentSpouse);
                try
                {
                    SpouseHandler.SendSpouseInformationMessage(Client, spouse);
                    switch (spouse.Account.Lang)
                    {
                        case "fr":
                            spouse.DisplayNotification("Votre compagnon est en ligne!");
                            break;
                        case "es":
                            spouse.DisplayNotification("�Su compa�ero est� conectado!");
                            break;
                        case "en":
                            spouse.DisplayNotification("Your mate is online!");
                            break;
                        default:
                            spouse.DisplayNotification("Seu companheiro est� conectado!");
                            break;
                    }
                    SpouseHandler.SendSpouseInformationMessage(spouse.Client, Client.Character);
                }
                catch
                {
                    if (CurrentSpouse != 0)
                        switch (Account.Lang)
                        {
                            case "fr":
                                DisplayNotification("Votre compagnon est hors ligne, vous serez averti quand il se connecte.");
                                break;
                            case "es":
                                DisplayNotification("Su compa�ero est� desconectado, se le avisar� cuando se conecte.");
                                break;
                            case "en":
                                DisplayNotification("Your mate is offline, you will be warned when he connects.");
                                break;
                            default:
                                DisplayNotification("Seu companheiro est� desconectado, voc� ser� avisado quando ele conectar-se.", NotificationEnum.ERREUR);
                                break;
                        }
                }
            }
            //Código de inicio de sesión MSG en la promoción Ogrinas 2x 
            if (Settings.Ogrines2xAnnounce == true && Account.UserGroupId <= 2)
            {
                switch (Account.Lang)
                {
                    case "fr":
                        OpenPopup("\n\nCombien de bons projets ont cessé d'exister par manque d'investissement ?\n\nL'investissement dans le projet est important pour que nous puissions maintenir les serveurs en ligne et aussi pour pouvoir investir dans la technologie afin d'améliorer le gameplay.\n\nNe laissez pas ce projet mourir lui aussi. Faites un don dès maintenant sur le site et recevez 2X les Ogrines.\n\n<a href=\"http://serverhydra.com/fr/boutique/paiement/br/paypal/choix-offre\"> CLIQUEZ ICI !</a>\n(PROMOTION LIMITÉE DANS LE TEMPS)", "<center>Hydra Promotion Ogrines 2x</center>", 5);
                        break;
                    case "es":
                        OpenPopup("\n\nCuántos buenos proyectos han dejado de existir por falta de inversión?\n\nLa inversión en el proyecto es importante para que podamos mantener los servidores en línea y también para poder invertir en tecnología para mejorar el juego.\n\nNo dejes que este proyecto muera también. Haga una donación ahora mismo en el sitio y reciba 2X Ogrinas.\n\n<a href=\"http://serverhydra.com/es/tienda/pago/br/paypal/elegir-oferta\"> PULSE AQUÍ !</a>\n(PROMOCIÓN POR TIEMPO LIMITADO)", "<center>Promoción de la Hydra Ogrinas 2x</center>", 5);
                        break;
                    case "en":
                        OpenPopup("\n\nHow many good projects have ceased to exist for lack of investment ?\n\nThe investment in the project is important so we can keep the servers online and also invest in technology to improve gameplay.\n\nDo not let this project die too. Make a donation right now on the site and receive 2X Ogrines.\n\n<a href=\"http://serverhydra.com/en/shop/payment/br/paypal/choose-offer\"> CLICK HERE !</a>\n(LIMITED TIME PROMOTION)", "<center>Hydra Promotion Ogrines 2x</center>", 5);
                        break;
                    default:
                        OpenPopup("\n\nQuantos projetos bons deixaram de exister por falta de investimento ?\n\nO investimento no projeto e importante para podermos manter os servidores online e também poder investir em tecnologia para melhoria da jogabilidade.\n\nNão deixe esse projeto morrer também. Faça uma doação agora mesmo no site e receba 2X Ogrines.\n\n<a href=\"http://serverhydra.com/pt/loja/pagamento/br/paypal/escolher-oferta\" > CLICK AQUI !</a>\n (PROMOÇÃO POR TEMPO LIMITADO)", "<center>Hydra Promoção Ogrines 2x</center>", 5);
                        break;
                }
            }

            if (Vip)
            {
                World.Instance.SendAnnounceAuto(this);
            }
            #endregion

            if (GuildMember != null)
            {
                GuildMember.OnCharacterConnected(this);

                if (Guild.MotdContent != null)
                    GuildHandler.SendGuildMotdMessage(Client, Guild);

                if (Guild.BulletinContent != null)
                    GuildHandler.SendGuildBulletinMessage(Client, Guild);
                if (Guild.Alliance != null)
                {

                    if (Guild.Alliance.MotdContent != null)
                        AllianceHandler.SendAllianceMotdMessage(Client, Guild.Alliance);
                    if (Guild.Alliance.BulletinContent != null)
                        AllianceHandler.SendAllianceBulletinMessage(Client, Guild.Alliance);

                }

                AddEmote(EmotesEnum.EMOTE_GUILD);

                if (Guild.Level > 200)
                    AddEmote((EmotesEnum)155); //guildwineer
            }
            else
            {
                RemoveEmote(EmotesEnum.EMOTE_GUILD);

                if (HasEmote((EmotesEnum)155))
                    RemoveEmote((EmotesEnum)155); //guildwineer
            }

            if (GuildMember != null)
            {
                if (Guild.Alliance != null)
                    AddEmote(EmotesEnum.EMOTE_ALLIANCE);
                else
                    RemoveEmote(EmotesEnum.EMOTE_ALLIANCE);
            }

            //Arena
            CheckArenaDailyProperties_1vs1();
            CheckArenaDailyProperties_3vs3_Team();
            CheckArenaDailyProperties_3vs3_Solo();

            #region Ornamentos e Titulos
            foreach (var ornament in Settings.VipOrnament)
            {
                if (!HasOrnament(ornament))
                {
                    if (Vip)
                        AddOrnament(ornament);
                }
                else
                {
                    if (!Vip)
                        RemoveOrnament(ornament);
                }
            }

            foreach (var ornamentGold in Settings.GoldVipOrnament)
            {
                if (!HasOrnament(ornamentGold))
                {
                    if (GoldVip)
                        AddOrnament(ornamentGold);
                }
                else
                {
                    if (!GoldVip)
                        RemoveOrnament(ornamentGold);
                }
            }

            foreach (var title in Settings.VipTitle)
            {
                if (!HasTitle(title))
                {
                    if (Vip)
                        AddTitle(title);
                }
                else
                {
                    if (!Vip)
                        RemoveTitle(title);
                }
            }

            foreach (var title in Settings.GoldTitle)
            {
                if (!HasTitle(title))
                {
                    if (GoldVip)
                        AddTitle(title);
                }
                else
                {
                    if (!GoldVip)
                        RemoveTitle(title);
                }
            }

            foreach (var emote in Settings.VipEmote)
            {
                if (!HasEmote((EmotesEnum)emote))
                {
                    if (Vip)
                        AddEmote((EmotesEnum)emote);
                }
                else
                {
                    if (!Vip)
                        RemoveEmote((EmotesEnum)emote);
                }
            }

            foreach (var emote in Settings.GoldEmote)
            {
                if (!HasEmote((EmotesEnum)emote))
                {
                    if (GoldVip)
                        AddEmote((EmotesEnum)emote);
                }
                else
                {
                    if (!GoldVip)
                        RemoveEmote((EmotesEnum)emote);
                }
            }

            #region Titles Staff
            if (IsGameMaster())
            {
                ushort titleId = GetTitleIdForRole(UserGroup.Role);

                if (titleId != 0)
                {
                    if (!HasTitle(titleId))
                    {
                        AddTitle(titleId);
                        SelectTitle(titleId);
                    }
                }
            }

            if (HasTitle(503) && UserGroup.Role != RoleEnum.Moderator_Helper)
                RemoveTitle(503);

            if (HasTitle(502) && UserGroup.Role != RoleEnum.GameMaster_Padawan && UserGroup.Role != RoleEnum.GameMaster)
                RemoveTitle(502);

            if (HasTitle(501) && UserGroup.Role != RoleEnum.Administrator)
                RemoveTitle(501);

            if (HasTitle(500) && UserGroup.Role != RoleEnum.Developer)
                RemoveTitle(500);
            #endregion

            #endregion

            #region Prestigios
            if (PrestigeRank > 0 && PrestigeManager.Instance.PrestigeEnabled)
            {
                var item = GetPrestigeItem();
                if (item == null)
                    CreatePrestigeItem();
                else
                {
                    item.UpdateEffects();
                    Inventory.RefreshItem(item);
                }
                RefreshStats();
            }
            else
            {
                var item = GetPrestigeItem();
                if (item != null)
                    Inventory.RemoveItem(item, true);
            }
            #endregion

            OnPlayerLifeStatusChanged(PlayerLifeStatus);

            if (!IsGhost())
            {
                var energyGain = (short)(DateTime.Now - Record.LastUsage.Value).TotalMinutes;

                energyGain = (short)((Energy + energyGain) > EnergyMax ? (EnergyMax - Energy) : energyGain);

                if (energyGain <= 0) { }
                else
                {

                    Energy += energyGain;

                    RefreshStats();

                    //Vous avez r�cup�r� <b>%1</b> points d'�nergie.
                    SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 7, energyGain);
                }
            }

            Record.LastUsage = DateTime.Now;

            var document = new BsonDocument {
                { "AcctId", Account.Id },
                { "AcctName", Account.Login },
                { "CharacterId", Id },
                { "CharacterName", Name },
                { "CharacterKamas", Kamas },
                { "BankKamas", Bank.Kamas },
                { "CharacterOgrines", Account.Tokens },
                { "IPAddress", Client.IP },
                { "Action", "Login" },
                { "Date", DateTime.Now.ToString(CultureInfo.CreateSpecificCulture("pt-BR")) }
            };

            MongoLogger.Instance.Insert("World_Chars_Connections", document);

            LoggedIn?.Invoke(this);
        }

        public event Action<Character> LoggedOut;

        void OnLoggedOut()
        {
            try
            {
                EnterMap -= OnFollowedMemberEnterMap;
                EnterMap -= UpdateFollowSpouse;

                //Event Ticket
                this.WorldAccount.TotalGameTimeInSeconds += (long)CharacterGameTime.Elapsed.TotalSeconds;

                if (FriendsBook != null)
                    FriendsBook.CheckDC(); // attempt to resolve leaks

                if (Fight != null && (Fight.State == FightState.Placement || Fight.State == FightState.Fighting))
                {
                    Record.LeftFightId = Fight.Id;
                }
                else
                {
                    Record.LeftFightId = null;
                }

                if (GuildMember != null)
                    GuildMember.OnCharacterDisconnected(this);

                if (TaxCollectorDefendFight != null)
                    TaxCollectorDefendFight.RemoveDefender(this);

                if (ArenaManager.Instance.IsInQueue(this))
                    ArenaManager.Instance.RemoveFromQueue(this);

                if (ArenaPopup != null)
                    ArenaPopup.Deny();

                if (Jobs != null)
                {
                    foreach (var job in Jobs.Where(x => x.IsIndexed))
                    {
                        job.Template.RemoveAvaiableCrafter(this);
                    }
                }

                var documentloggout = new BsonDocument
                {
                    { "AcctId", Account.Id },
                    { "AcctName", Account.Login },
                    { "CharacterId", Id },
                    { "CharacterName", Name },
                    { "CharacterKamas", Kamas },
                    { "BankKamas", Bank.Kamas },
                    { "CharacterOgrines", Account.Tokens },
                    { "IPAddress", Client.IP },
                    { "Action", "Loggout" },
                    { "Date", DateTime.Now.ToString(CultureInfo.CreateSpecificCulture("pt-BR")) }
                };

                MongoLogger.Instance.Insert("World_Chars_Connections", documentloggout);

                Record.LastUsage = DateTime.Now;

                LoggedOut?.Invoke(this);
            }
            catch (Exception e)
            {
                Console.WriteLine("Erro no OnLoggedOut: " + e.Message);
            }
        }

        public event Action<Character> Saved;

        public void OnSaved()
        {
            IsAuthSynced = true;
            UnBlockAccount();

            Saved?.Invoke(this);
        }

        public event Action<Character, int> LifeRegened;

        private void OnLifeRegened(int regenedLife)
        {
            LifeRegened?.Invoke(this, regenedLife);
        }

        public event Action<Character> AccountUnblocked;

        private void OnAccountUnblocked()
        {
            AccountUnblocked?.Invoke(this);
        }

        public event Action<Character> LookRefreshed;

        private void OnLookRefreshed()
        {
            LookRefreshed?.Invoke(this);
        }

        public event Action<Character> StatsResfreshed;

        private void OnStatsResfreshed()
        {
            StatsResfreshed?.Invoke(this);
        }

        public event Action<Character, Npc, NpcActionTypeEnum, NpcAction> InteractingWith;

        public void OnInteractingWith(Npc npc, NpcActionTypeEnum actionType, NpcAction action)
        {
            InteractingWith?.Invoke(this, npc, actionType, action);
        }
        #endregion Events

        #region Properties

        public WorldClient Client
        {
            get;
        }

        public string CharacterToSeekName
        {
            get { return Record.CharacterToSeekName; }
            set { Record.CharacterToSeekName = value; }
        }

        public AccountData Account
        {
            get { return Client.Account; }
        }

        public WorldAccount WorldAccount
        {
            get { return Client.WorldAccount; }
        }

        public UserGroup UserGroup
        {
            get
            {
                return Client.UserGroup;
            }
        }

        public object SaveSync
        {
            get;
            private set;
        }

        public object LoggoutSync
        {
            get;
            private set;
        }

        private bool m_inWorld;

        public override bool IsInWorld
        {
            get
            {
                return m_inWorld;
            }
        }

        public CharacterMerchantBag MerchantBag
        {
            get;
            private set;
        }

        #region Battlefield
        public Map MapBattleField
        {
            get;
            set;
        }

        public Cell CellBattleField
        {
            get;
            set;
        }

        public bool ForcePassTurn
        {
            get;
            set;
        }

        public bool isMultiLeadder
        {
            get;
            set;
        }

        public void updateBattleFieldPosition()
        {
            this.MapBattleField = this.Map;
            this.CellBattleField = this.Cell;
        }
        #endregion

        #region Incarnation

        public bool IsInIncarnation
        {
            get
            {
                return Record.IsInIncarnation;
            }
            set
            {
                Record.IsInIncarnation = value;
            }
        }

        public int IncarnationId
        {
            get
            {
                return Record.IncarnationId;
            }
            set
            {
                Record.IncarnationId = value;
            }
        }

        #endregion

        #region Identifier

        public override string Name
        {
            get
            {
                RoleEnum groupId = (RoleEnum)Account.UserGroupId;

                if (groupId == RoleEnum.Vip)
                {
                    return $"[VIP]{m_record.Name}";
                }
                if (groupId == RoleEnum.Gold_Vip)
                {
                    return $"[GOLD]{m_record.Name}";
                }
                else if (groupId == RoleEnum.Moderator_Helper)
                {
                    return $"[MOD]{m_record.Name}";
                }
                else if (groupId == RoleEnum.GameMaster_Padawan || groupId == RoleEnum.GameMaster)
                {
                    return $"[GM]{m_record.Name}";
                }
                else if (groupId == RoleEnum.Administrator)
                {
                    return $"[ADMIN]{m_record.Name}";
                }
                else if (groupId == RoleEnum.Developer)
                {
                    return $"[DEV]{m_record.Name}";
                }
                else
                {
                    return m_record.Name;
                }
            }

            protected set
            {
                m_record.Name = value;
                base.Name = value;
            }
        }

        public string Namedefault
        {
            get
            {
                RoleEnum groupId = (RoleEnum)Account.UserGroupId;

                if (groupId == RoleEnum.Vip)
                {
                    return $"[VIP]{m_record.Name}";
                }
                if (groupId == RoleEnum.Gold_Vip)
                {
                    return $"[GOLD]{m_record.Name}";
                }
                else if (groupId == RoleEnum.Moderator_Helper)
                {
                    return $"[MOD]{m_record.Name}";
                }
                else if (groupId == RoleEnum.GameMaster_Padawan || groupId == RoleEnum.GameMaster)
                {
                    return $"[GM]{m_record.Name}";
                }
                else if (groupId == RoleEnum.Administrator)
                {
                    return $"[ADMIN]{m_record.Name}";
                }
                else if (groupId == RoleEnum.Developer)
                {
                    return $"[DEV]{m_record.Name}";
                }
                else
                {
                    return m_record.Name;
                }
            }
        }

        public string NameClean
        {
            get
            {
                return m_record.Name;
            }
        }

        public override int Id
        {
            get { return m_record.Id; }
            protected set
            {
                m_record.Id = value;
                base.Id = value;
            }
        }

        #endregion Identifier

        #region Achievement
        public PlayerAchievement Achievement { get; private set; }

        #endregion

        #region Inventory

        public Inventory Inventory
        {
            get;
            private set;
        }

        public long Kamas
        {
            get { return Record.Kamas; }
            set { Record.Kamas = value; }
        }

        #region Followers / Wanteds

        public void AddFollow(int monsterId)
        {
            List<BasePlayerItem> items = this.Inventory.GetItems(x => x.Template.Type.ItemType == ItemTypeEnum.PERSONNAGE_SUIVEUR_32).ToList();

            var followerItem = items.Where(x => (x.Template.Effects.FirstOrDefault(y => y.EffectId == EffectsEnum.Effect_148) as EffectDice).Value == monsterId).FirstOrDefault();

            if (followerItem is null)
                return;

            List<BasePlayerItem> itemsEquipped = this.Inventory.GetEquipedItems().Where(entry => entry.Position == CharacterInventoryPositionEnum.INVENTORY_POSITION_FOLLOWER && entry.Template.Id == followerItem.Template.Id).ToList();

            if (itemsEquipped.Count == 0)
            {
                FollowerItem follow = new FollowerItem(this, followerItem.Record);
                this.Inventory.MoveItem(follow, CharacterInventoryPositionEnum.INVENTORY_POSITION_FOLLOWER, true);
            }
        }

        #endregion

        #endregion Inventory

        #region Jobs
        public JobsCollection Jobs
        {
            get;
            private set;
        }

        public event Action<ItemTemplate, int> HarvestItem;

        public void OnHarvestItem(ItemTemplate item, int quantity)
        {
            HarvestItem?.Invoke(item, quantity);
        }

        public event Action<BasePlayerItem, int> CraftItem;

        public void OnCraftItem(BasePlayerItem item, int quantity)
        {
            CraftItem?.Invoke(item, quantity);
        }

        public event Action<ItemTemplate, int> DecraftItem;

        public void OnDecraftItem(ItemTemplate item, int runeQuantity)
        {
            OwnedRuneAmount += runeQuantity;
            DecraftItem?.Invoke(item, runeQuantity);
        }
        #endregion Jobs

        #region Interactives

        public InteractiveObject CurrentUsedInteractive => CurrentUsedSkill?.InteractiveObject;

        public Skill CurrentUsedSkill
        {
            get;
            private set;
        }

        public void SetCurrentSkill(Skill skill)
        {
            CurrentUsedSkill = skill;
        }

        public void ResetCurrentSkill()
        {
            CurrentUsedSkill = null;
        }

        #endregion

        #region Position

        public override ICharacterContainer CharacterContainer
        {
            get
            {
                if (IsFighting())
                    return Fight;

                return Map;
            }
        }

        #endregion Position

        #region Dungeon

        public List<long[]> DungeonReturn
        {
            get { return Record.DungeonReturn; }
            set { Record.DungeonReturn = value; }
        }

        #endregion

        #region Dialog

        private IDialoger m_dialoger;

        public IDialoger Dialoger
        {
            get { return m_dialoger; }
            private set
            {
                m_dialoger = value;
                m_dialog = value != null ? m_dialoger.Dialog : null;
            }
        }

        private IDialog m_dialog;

        public IDialog Dialog
        {
            get { return m_dialog; }
            private set
            {
                m_dialog = value;
                if (m_dialog == null)
                    m_dialoger = null;
            }
        }

        public NpcShopDialogLogger NpcShopDialog => Dialog as NpcShopDialogLogger;

        public ZaapDialog ZaapDialog => Dialog as ZaapDialog;

        public ZaapiDialog ZaapiDialog => Dialog as ZaapiDialog;

        public MerchantShopDialog MerchantShopDialog => Dialog as MerchantShopDialog;

        public RequestBox RequestBox
        {
            get;
            private set;
        }

        public void SetDialoger(IDialoger dialoger)
        {
            if (Dialog != null)
                Dialog.Close();

            Dialoger = dialoger;
        }

        public void SetDialog(IDialog dialog)
        {
            if (Dialog != null)
                Dialog.Close();

            Dialog = dialog;
        }

        public void CloseDialog(IDialog dialog)
        {
            if (Dialog == dialog)
                Dialoger = null;
        }

        public void ResetDialog()
        {
            Dialoger = null;
        }

        public void OpenRequestBox(RequestBox request)
        {
            RequestBox = request;
        }

        public void ResetRequestBox()
        {
            RequestBox = null;
        }

        public bool IsBusy() => IsInRequest() || IsDialoging();

        public bool IsDialoging() => Dialog != null;

        public bool IsInRequest() => RequestBox != null;

        public bool IsRequestSource() => IsInRequest() && RequestBox.Source == this;

        public bool IsRequestTarget() => IsInRequest() && RequestBox.Target == this;

        public bool IsTalkingWithNpc() => Dialog is NpcDialog;

        public bool IsInZaapDialog() => Dialog is ZaapDialog;


        public bool IsInZaapiDialog() => Dialog is ZaapiDialog;

        #endregion Dialog

        #region Party

        private readonly Dictionary<int, PartyInvitation> m_partyInvitations = new Dictionary<int, PartyInvitation>();

        private readonly List<PartyTypeEnum> m_partiesLoyalTo = new List<PartyTypeEnum>();

        private Character m_followedCharacter;

        private Party m_party;
        private ArenaParty m_arenaParty;

        public Party Party
        {
            get { return m_party; }
            private set
            {
                if (m_party != null && value != m_party) SetLoyalToParty(PartyTypeEnum.PARTY_TYPE_CLASSICAL, false);
                m_party = value;
            }
        }

        public ArenaParty ArenaParty
        {
            get { return m_arenaParty; }
            private set
            {
                if (m_arenaParty != null && value != m_arenaParty) SetLoyalToParty(PartyTypeEnum.PARTY_TYPE_ARENA, false);
                m_arenaParty = value;
            }
        }

        public Party[] Parties => new[] { Party, ArenaParty }.Where(x => x != null).ToArray();

        public bool IsInParty()
        {
            return Party != null || ArenaParty != null;
        }

        public bool IsInParty(int id)
        {
            return (Party != null && Party.Id == id) || (ArenaParty != null && ArenaParty.Id == id);
        }

        public bool IsInParty(PartyTypeEnum type)
        {
            return (type == PartyTypeEnum.PARTY_TYPE_CLASSICAL && Party != null) || (type == PartyTypeEnum.PARTY_TYPE_ARENA && ArenaParty != null);
        }

        public bool IsPartyLeader()
        {
            return Party?.Leader == this;
        }

        public bool IsPartyLeader(int id)
        {
            return GetParty(id)?.Leader == this;
        }

        public Party GetParty(int id)
        {
            if (Party != null && Party.Id == id)
                return Party;

            if (ArenaParty != null && ArenaParty.Id == id)
                return ArenaParty;

            return null;
        }

        public bool IsLoyalToParty(PartyTypeEnum type) => m_partiesLoyalTo.Contains(type);
        public void SetLoyalToParty(PartyTypeEnum type, bool loyal)
        {
            if (loyal) m_partiesLoyalTo.Add(type);
            else m_partiesLoyalTo.Remove(type);

            PartyHandler.SendPartyLoyaltyStatusMessage(Client, GetParty(type), loyal);
        }

        public Party GetParty(PartyTypeEnum type)
        {
            switch (type)
            {
                case PartyTypeEnum.PARTY_TYPE_CLASSICAL:
                    return Party;

                case PartyTypeEnum.PARTY_TYPE_ARENA:
                    return ArenaParty;

                default:
                    throw new NotImplementedException(string.Format("Cannot manage party of type {0}", type));
            }
        }

        public void SetParty(Party party)
        {
            switch (party.Type)
            {
                case PartyTypeEnum.PARTY_TYPE_CLASSICAL:
                    Party = party;
                    break;

                case PartyTypeEnum.PARTY_TYPE_ARENA:
                    ArenaParty = (ArenaParty)party;
                    break;

                default:
                    logger.Error("Cannot manage party of type {0} ({1})", party.GetType(), party.Type);
                    break;
            }
        }

        public void ResetParty(PartyTypeEnum type)
        {
            switch (type)
            {
                case PartyTypeEnum.PARTY_TYPE_CLASSICAL:
                    Party = null;
                    break;

                case PartyTypeEnum.PARTY_TYPE_ARENA:
                    ArenaParty = null;
                    break;

                default:
                    logger.Error("Cannot manage party of type {0}", type);
                    break;
            }

            CompassHandler.SendCompassResetMessage(Client, CompassTypeEnum.COMPASS_TYPE_PARTY);
        }

        #endregion Party

        #region Spouse & Misc

        public int CurrentSpouse { get { return m_record.SpouseID; } set { m_record.SpouseID = value; } }
        public void FollowSpouse(Character Spouse)
        {
            followforspouse = true;
            Client.Send(new CompassUpdatePartyMemberMessage((sbyte)CompassTypeEnum.COMPASS_TYPE_SPOUSE, new MapCoordinates((short)Spouse.Map.Position.X, (short)Spouse.Map.Position.Y), (ulong)Spouse.Id, true)); // idk active ???
            Client.Send(new TextInformationMessage((sbyte)TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 368, new string[] {
                Spouse.Name
            }));
            Spouse.Client.Send(new TextInformationMessage((sbyte)TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 52, new string[] {
                Name
            }));
        }
        public void StopFollowSpouse(Character spouse = null)
        {
            followforspouse = false;
            if (spouse != null)
                spouse.Client.Send(new TextInformationMessage((sbyte)TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 53, new List<string> {
                    Name
                }));
            Client.Send(new CompassUpdatePartyMemberMessage((sbyte)CompassTypeEnum.COMPASS_TYPE_SPOUSE, new MapCoordinates((short)spouse.Map.Position.X, (short)spouse.Map.Position.Y), (ulong)spouse.Id, false)); // maybe dont need this
            Client.Send(new CompassResetMessage((sbyte)CompassTypeEnum.COMPASS_TYPE_SPOUSE));
        }
        public void UpdateFollowSpouse(RolePlayActor actor, Map map)

        {
            Character spouse = actor as Character;
            if (actor == null)
                return;
            // Character spouse = World.Instance.GetCharacter(SpouseID);
            if (spouse != null && spouse.followforspouse)
                spouse.Client.Send(new CompassUpdatePartyMemberMessage((sbyte)CompassTypeEnum.COMPASS_TYPE_SPOUSE, new MapCoordinates((short)map.Position.X, (short)map.Position.Y), (ulong)Id, true));
            else if (spouse != null)
            {
                spouse.Client.Send(new CompassResetMessage((sbyte)CompassTypeEnum.COMPASS_TYPE_SPOUSE));
                spouse.followforspouse = false;
            }
        }

        #endregion

        #region Trade

        public IExchange Exchange
        {
            get { return Dialog as IExchange; }
        }

        public Exchanger Exchanger => Dialoger as Exchanger;

        public ITrade Trade
        {
            get { return Dialog as ITrade; }
        }

        public PlayerTrade PlayerTrade
        {
            get { return Trade as PlayerTrade; }
        }

        public Trader Trader
        {
            get { return Dialoger as Trader; }
        }

        public bool IsInExchange()
        {
            return Exchanger != null;
        }

        public bool IsTrading()
        {
            return Trade != null;
        }

        public bool IsTradingWithPlayer()
        {
            return PlayerTrade != null;
        }

        #endregion Trade

        //#region Spells Modifications
        //public void SpellRangeableEnable(short spellId)
        //{
        //    if (Spells == null)
        //        return;

        //    if (Spells.Where(x => x.Id == spellId) == null || Spells.Where(x => x.Id == spellId).Count() < 1)
        //        return;

        //    var spell = Spells.Where(x => x.Id == spellId).First();

        //    if (spell != null)
        //    {
        //        //spell.RangeableEnable = true;

        //        //CharacterSpellModification s = new CharacterSpellModification(
        //        //    (sbyte)CharacterSpellModificationTypeEnum.RANGEABLE,
        //        //    (ushort)spellId,
        //        //    new CharacterCharacteristicDetailed(1, 0,0,0,0, 1));

        //        SpellsModifications.Add(s);
        //        RefreshStats();
        //    }
        //}

        //public void SpellRangeableDisable(short spellId)
        //{
        //    if (Spells == null)
        //        return;

        //    if (Spells.Where(x => x.Id == spellId) == null || Spells.Where(x => x.Id == spellId).Count() < 1)
        //        return;

        //    var spell = Spells.Where(x => x.Id == spellId).First();

        //    if (spell != null)
        //    {
        //        try
        //        {
        //            spell.RangeableEnable = false;

        //            var s = SpellsModifications.Where(x => x.spellId == spellId && x.modificationType == (byte)CharacterSpellModificationTypeEnum.RANGEABLE);

        //            if (s != null)
        //                SpellsModifications.Remove(s.FirstOrDefault());

        //            RefreshStats();
        //        }
        //        catch (Exception ex)
        //        {
        //            logger.Error("Error - " + ex);
        //        }
        //    }
        //}

        //public void SpellObstaclesDisable(int spellId)
        //{
        //    if (Spells == null)
        //        return;

        //    if (Spells.Where(x => x.Id == spellId) == null || Spells.Where(x => x.Id == spellId).Count() < 1)
        //        return;

        //    var spell = Spells.Where(x => x.Id == spellId).First();

        //    if (spell != null)
        //    {
        //        spell.SpellObstaclesDisable = true;

        //        CharacterCharacteristicDetailed characteristicDetailed = new CharacterCharacteristicDetailed(1, 0,0,0,0, 1);
        //        //CharacterSpellModification spellModification = new CharacterSpellModification((sbyte)CharacterSpellModificationTypeEnum.LOS, (ushort)spellId, characteristicDetailed);
        //        //this.Map.ForEach(x => new UpdateSpellModifierMessage(this.Id, spellModification));

        //        SpellsModifications.Add(spellModification);
        //        RefreshStats();
        //    }
        //}

        //public void SpellObstaclesDisable(Spell spell)
        //{
        //    if (spell != null)
        //    {
        //        spell.SpellObstaclesDisable = true;

        //        CharacterCharacteristicDetailed characteristicDetailed = new CharacterCharacteristicDetailed(1, 0,0,0,0, 1);
        //        //CharacterSpellModification spellModification = new CharacterSpellModification((sbyte)CharacterSpellModificationTypeEnum.LOS, (ushort)spell.Template.Id, characteristicDetailed);
        //        //this.Map.ForEach(x => new UpdateSpellModifierMessage(this.Id, spellModification));

        //        SpellsModifications.Add(spellModification);
        //        RefreshStats();
        //    }
        //}

        //public void SpellObstaclesEnable(Spell spell)
        //{
        //    if (spell != null)
        //    {
        //        spell.SpellObstaclesDisable = false;

        //        var s = SpellsModifications.FirstOrDefault(x => x.spellId == spell.Template.Id && x.modificationType == (byte)CharacterSpellModificationTypeEnum.LOS);

        //        if (s != null)
        //            SpellsModifications.Remove(s);
        //    }
        //}

        //public void SpellObstaclesEnable(short spellId)
        //{
        //    if (Spells == null)
        //        return;

        //    if (Spells.Where(x => x.Id == spellId) == null || Spells.Where(x => x.Id == spellId).Count() < 1)
        //        return;

        //    var spell = Spells.Where(x => x.Id == spellId).First();

        //    if (spell != null)
        //    {
        //        try
        //        {
        //            spell.SpellObstaclesDisable = false;

        //            var s = SpellsModifications.Where(x => x.spellId == spellId && x.modificationType == (byte)CharacterSpellModificationTypeEnum.LOS);

        //            if (s != null)
        //                SpellsModifications.Remove(s.FirstOrDefault());

        //            RefreshStats();
        //        }
        //        catch (Exception ex)
        //        {
        //            logger.Error("Error - " + ex);
        //        }
        //    }
        //}

        //public void LineCastDisable(short spellId)
        //{
        //    if (Spells == null)
        //        return;

        //    if (Spells.Where(x => x.Id == spellId) == null || Spells.Where(x => x.Id == spellId).Count() < 1)
        //        return;

        //    var spell = Spells.Where(x => x.Id == spellId).First();

        //    if (spell != null)
        //    {
        //        spell.LineCastDisable = true;

        //        CharacterSpellModification s = new CharacterSpellModification(
        //            (sbyte)CharacterSpellModificationTypeEnum.CAST_LINE,
        //            (ushort)spellId,
        //            new CharacterCharacteristicDetailed(1, 0,0,0,0, 1));

        //        SpellsModifications.Add(s);
        //        RefreshStats();
        //    }
        //}

        //public void LineCastEnable(short spellId)
        //{
        //    if (Spells == null)
        //        return;

        //    if (Spells.Where(x => x.Id == spellId) == null || Spells.Where(x => x.Id == spellId).Count() < 1)
        //        return;

        //    var spell = Spells.Where(x => x.Id == spellId).First();

        //    if (spell != null)
        //    {
        //        try
        //        {
        //            spell.LineCastDisable = false;

        //            var s = SpellsModifications.Where(x => x.spellId == spellId && x.modificationType == (byte)CharacterSpellModificationTypeEnum.CAST_LINE);

        //            if (s != null)
        //                SpellsModifications.Remove(s.FirstOrDefault());

        //            RefreshStats();
        //        }
        //        catch (Exception ex)
        //        {
        //            logger.Error("Error - " + ex);
        //        }
        //    }
        //}

        //public void ReduceSpellCost(short spellId, uint amount)
        //{
        //    if (Spells == null)
        //        return;

        //    if (Spells.Where(x => x.Id == spellId) == null || Spells.Where(x => x.Id == spellId).Count() < 1)
        //        return;

        //    var spell = Spells.Where(x => x.Id == spellId).First();

        //    if (spell != null)
        //    {
        //        var v = SpellsModifications.FirstOrDefault(x => x.spellId == spellId && x.modificationType == (byte)CharacterSpellModificationTypeEnum.AP_COST);

        //        if (v != null)
        //        {
        //            v.value.additional += (short)amount;
        //            spell.ApCostReduction += amount;
        //            RefreshStats();
        //            return;
        //        }

        //        CharacterSpellModification s = new CharacterSpellModification(
        //            (sbyte)CharacterSpellModificationTypeEnum.AP_COST,
        //            (ushort)spellId,
        //            new CharacterCharacteristicDetailed(1, 0,0,0,0, 1));

        //        SpellsModifications.Add(s);
        //        spell.ApCostReduction += amount;

        //        RefreshStats();
        //    }
        //}

        //public void SpellCostDisable(short spellId, short boost)
        //{
        //    if (Spells == null)
        //        return;

        //    if (Spells.Where(x => x.Id == spellId) == null || Spells.Where(x => x.Id == spellId).Count() < 1)
        //        return;

        //    var spell = Spells.Where(x => x.Id == spellId).First();

        //    if (spell != null)
        //    {
        //        var s = SpellsModifications.Where(x => x.spellId == spellId && x.modificationType == (byte)CharacterSpellModificationTypeEnum.AP_COST).FirstOrDefault();

        //        try
        //        {
        //            if (s != null)
        //            {
        //                s.value.additional -= boost;
        //                spell.ApCostReduction -= (uint)boost;

        //                if (s.value.additional <= 0)
        //                {
        //                    SpellsModifications.Remove(s);
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            logger.Error("Error - " + ex);
        //        }

        //        RefreshStats();
        //    }
        //}

        //public void IncreaseRange(short spellId, uint amount)
        //{
        //    if (Spells == null)
        //        return;

        //    if (Spells.Where(x => x.Id == spellId) == null || Spells.Where(x => x.Id == spellId).Count() < 1)
        //        return;

        //    var spell = Spells.Where(x => x.Id == spellId).First();

        //    if (spell != null)
        //    {
        //        spell.AdditionalRange += (int)amount;

        //        CharacterSpellModification s = new CharacterSpellModification(
        //            (sbyte)CharacterSpellModificationTypeEnum.RANGE_MAX,
        //            (ushort)spellId,
        //            new CharacterCharacteristicDetailed((short)amount, 0,0,0,0, 0));

        //        SpellsModifications.Add(s);
        //        RefreshStats();
        //    }
        //}

        //public void IncreaseRangeDisable(short spellId, short boost)
        //{
        //    if (Spells == null)
        //        return;

        //    if (Spells.Where(x => x.Id == spellId) == null || Spells.Where(x => x.Id == spellId).Count() < 1)
        //        return;

        //    var spell = Spells.Where(x => x.Id == spellId).First();

        //    if (spell != null)
        //    {
        //        var s = SpellsModifications.Where(x => x.spellId == spellId && x.modificationType == (byte)CharacterSpellModificationTypeEnum.RANGE_MAX).FirstOrDefault();

        //        try
        //        {
        //            if (s != null)
        //            {
        //                s.value.contextModif -= boost;
        //                spell.AdditionalRange -= boost;

        //                if (s.value.contextModif <= 0)
        //                {
        //                    SpellsModifications.Remove(s);
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            logger.Error("Error - " + ex);
        //        }

        //        RefreshStats();
        //    }
        //}

        //public void SpellRangeHandler(short spellId, short amount)
        //{
        //    if (Spells == null)
        //        return;

        //    if (Spells.Where(x => x.Id == spellId) == null || Spells.Where(x => x.Id == spellId).Count() < 1)
        //        return;

        //    var spell = Spells.Where(x => x.Id == spellId).First();

        //    if (spell != null)
        //    {
        //        var v = SpellsModifications.FirstOrDefault(x => x.spellId == spellId && x.modificationType == (byte)CharacterSpellModificationTypeEnum.RANGE_MAX);

        //        if (v != null)
        //        {
        //            v.value.contextModif += amount;
        //            spell.AdditionalRange += amount;

        //            RefreshStats();
        //            return;
        //        }

        //        CharacterSpellModification spellModification = new CharacterSpellModification((sbyte)CharacterSpellModificationTypeEnum.RANGE_MAX, (ushort)spellId,
        //            new CharacterCharacteristicDetailed(amount, 0,0,0,0, 0));

        //        SpellsModifications.Add(spellModification);
        //        spell.AdditionalRange += amount;
        //        RefreshStats();
        //    }
        //}

        //public void ReduceDelay(short spellId, uint amount)
        //{
        //    if (Spells == null)
        //        return;

        //    if (Spells.Where(x => x.Id == spellId) == null || Spells.Where(x => x.Id == spellId).Count() < 1)
        //        return;

        //    var spell = Spells.Where(x => x.Id == spellId).First();

        //    if (spell != null)
        //    {
        //        spell.DelayReduction += amount;

        //        CharacterSpellModification s = new CharacterSpellModification(
        //            (sbyte)CharacterSpellModificationTypeEnum.CAST_INTERVAL,
        //            (ushort)spellId,
        //            new CharacterCharacteristicDetailed(0, 0,0,0,0, (short)amount));

        //        SpellsModifications.Add(s);

        //        RefreshStats();
        //    }
        //}

        //public void ReduceDelayDisable(short spellId)
        //{
        //    if (Spells == null)
        //        return;

        //    if (Spells.Where(x => x.Id == spellId) == null || Spells.Where(x => x.Id == spellId).Count() < 1)
        //        return;

        //    var spell = Spells.Where(x => x.Id == spellId).First();

        //    if (spell != null)
        //    {
        //        try
        //        {
        //            var s = SpellsModifications.Where(x => x.spellId == spellId && x.modificationType == (byte)CharacterSpellModificationTypeEnum.CAST_INTERVAL).FirstOrDefault();
        //            spell.DelayReduction -= (uint)s.value.additional;

        //            if (s != null)
        //                SpellsModifications.Remove(s);
        //        }
        //        catch (Exception ex)
        //        {
        //            logger.Error("Error - " + ex);
        //        }

        //        RefreshStats();
        //    }
        //}

        //public void AddMaxCastPerTurn(short spellId, uint amount)
        //{
        //    if (Spells == null)
        //        return;

        //    if (Spells.Where(x => x.Id == spellId) == null || Spells.Where(x => x.Id == spellId).Count() < 1)
        //        return;

        //    var spell = Spells.Where(x => x.Id == spellId).First();

        //    if (spell != null)
        //    {
        //        spell.AdditionalCastPerTurn += amount;

        //        CharacterSpellModification s = new CharacterSpellModification(
        //            (sbyte)CharacterSpellModificationTypeEnum.MAX_CAST_PER_TURN,
        //            (ushort)spellId,
        //            new CharacterCharacteristicDetailed((short)amount, 0,0,0,0, (short)amount));

        //        SpellsModifications.Add(s);
        //        RefreshStats();
        //    }
        //}

        //public void AddMaxCastPerTurnDisable(short spellId)
        //{
        //    if (Spells == null)
        //        return;

        //    if (Spells.Where(x => x.Id == spellId) == null || Spells.Where(x => x.Id == spellId).Count() < 1)
        //        return;

        //    var spell = Spells.Where(x => x.Id == spellId).First();

        //    if (spell != null)
        //    {
        //        try
        //        {
        //            var s = SpellsModifications.Where(x => x.spellId == spellId && x.modificationType == (byte)CharacterSpellModificationTypeEnum.MAX_CAST_PER_TURN).FirstOrDefault();
        //            spell.AdditionalCastPerTurn -= (uint)s.value.additional;

        //            if (s != null)
        //                SpellsModifications.Remove(s);
        //        }
        //        catch (Exception ex)
        //        {
        //            logger.Error("Error - " + ex);
        //        }

        //        RefreshStats();
        //    }
        //}

        //public void AddMaxCastPerTarget(short spellId, uint amount)
        //{
        //    if (Spells == null)
        //        return;

        //    if (Spells.Where(x => x.Id == spellId) == null || Spells.Where(x => x.Id == spellId).Count() < 1)
        //        return;

        //    var spell = Spells.Where(x => x.Id == spellId).First();

        //    if (spell != null)
        //    {
        //        spell.AdditionalCastPerTarget += amount;

        //        CharacterSpellModification s = new CharacterSpellModification(
        //            (sbyte)CharacterSpellModificationTypeEnum.MAX_CAST_PER_TARGET,
        //            (ushort)spellId,
        //            new CharacterCharacteristicDetailed((short)amount, 0,0,0,0, (short)amount));

        //        SpellsModifications.Add(s);
        //        RefreshStats();
        //    }
        //}

        //public void AddMaxCastPerTargetDisable(short spellId)
        //{
        //    if (Spells == null)
        //        return;

        //    if (Spells.Where(x => x.Id == spellId) == null || Spells.Where(x => x.Id == spellId).Count() < 1)
        //        return;

        //    var spell = Spells.Where(x => x.Id == spellId).First();

        //    if (spell != null)
        //    {
        //        try
        //        {
        //            var s = SpellsModifications.Where(x => x.spellId == spellId && x.modificationType == (byte)CharacterSpellModificationTypeEnum.MAX_CAST_PER_TARGET).FirstOrDefault();
        //            spell.AdditionalCastPerTarget -= (uint)s.value.additional;

        //            if (s != null)
        //                SpellsModifications.Remove(s);
        //        }
        //        catch (Exception ex)
        //        {
        //            logger.Error("Error - " + ex);
        //        }

        //        RefreshStats();
        //    }
        //}

        //public void AddDamage(short spellId, uint amount)
        //{
        //    if (Spells == null)
        //        return;

        //    if (Spells.Where(x => x.Id == spellId) == null || Spells.Where(x => x.Id == spellId).Count() < 1)
        //        return;

        //    var spell = Spells.Where(x => x.Id == spellId).First();

        //    if (spell != null)
        //    {
        //        spell.AdditionalDamage += amount;

        //        CharacterSpellModification s = new CharacterSpellModification(
        //            (sbyte)CharacterSpellModificationTypeEnum.DAMAGE,
        //            (ushort)spellId,
        //            new CharacterCharacteristicDetailed((short)amount, 0,0,0,0, (short)amount));

        //        SpellsModifications.Add(s);

        //        RefreshStats();
        //    }
        //}

        //public void AddDamageDisable(short spellId)
        //{
        //    if (Spells == null)
        //        return;

        //    if (Spells.Where(x => x.Id == spellId) == null || Spells.Where(x => x.Id == spellId).Count() < 1)
        //        return;

        //    var spell = Spells.Where(x => x.Id == spellId).First();

        //    if (spell != null)
        //    {
        //        try
        //        {
        //            var s = SpellsModifications.Where(x => x.spellId == spellId && x.modificationType == (byte)CharacterSpellModificationTypeEnum.DAMAGE).FirstOrDefault();
        //            spell.AdditionalDamage -= (uint)s.value.additional;

        //            if (s != null)
        //                SpellsModifications.Remove(s);
        //        }
        //        catch (Exception ex)
        //        {
        //            logger.Error("Error - " + ex);
        //        }

        //        RefreshStats();
        //    }
        //}

        //public void SpellAddDamage(short spellId, uint amount)
        //{
        //    if (Spells == null)
        //        return;

        //    if (Spells.Where(x => x.Id == spellId) == null || Spells.Where(x => x.Id == spellId).Count() < 1)
        //        return;

        //    var spell = Spells.Where(x => x.Id == spellId).First();

        //    if (spell != null)
        //    {
        //        CharacterSpellModification s = new CharacterSpellModification(
        //            (sbyte)CharacterSpellModificationTypeEnum.DAMAGE,
        //            (ushort)spellId,
        //            new CharacterCharacteristicDetailed((short)amount, 0,0,0,0, (short)amount));

        //        SpellsModifications.Add(s);

        //        RefreshStats();
        //    }
        //}

        //public void SpellAddDamageDisable(short spellId)
        //{
        //    if (Spells == null)
        //        return;

        //    if (Spells.Where(x => x.Id == spellId) == null || Spells.Where(x => x.Id == spellId).Count() < 1)
        //        return;

        //    var spell = Spells.Where(x => x.Id == spellId).First();

        //    if (spell != null)
        //    {
        //        try
        //        {
        //            var s = SpellsModifications.Where(x => x.spellId == spellId && x.modificationType == (byte)CharacterSpellModificationTypeEnum.DAMAGE);

        //            if (s != null)
        //                SpellsModifications.Remove(s.FirstOrDefault());
        //        }
        //        catch (Exception ex)
        //        {
        //            logger.Error("Error - " + ex);
        //        }

        //        RefreshStats();
        //    }
        //}

        //public void AddHeal(short spellId, uint amount)
        //{
        //    if (Spells == null)
        //        return;

        //    if (Spells.Where(x => x.Id == spellId) == null || Spells.Where(x => x.Id == spellId).Count() < 1)
        //        return;

        //    var spell = Spells.Where(x => x.Id == spellId).First();

        //    if (spell != null)
        //    {
        //        spell.AdditionalHeal += amount;

        //        CharacterSpellModification s = new CharacterSpellModification(
        //            (sbyte)CharacterSpellModificationTypeEnum.HEAL_BONUS,
        //            (ushort)spellId,
        //            new CharacterCharacteristicDetailed((short)amount, 0,0,0,0, (short)amount));

        //        SpellsModifications.Add(s);

        //        RefreshStats();
        //    }
        //}

        //public void AddHealDisable(short spellId)
        //{
        //    if (Spells == null)
        //        return;

        //    if (Spells.Where(x => x.Id == spellId) == null || Spells.Where(x => x.Id == spellId).Count() < 1)
        //        return;

        //    var spell = Spells.Where(x => x.Id == spellId).First();

        //    if (spell != null)
        //    {
        //        try
        //        {
        //            var s = SpellsModifications.Where(x => x.spellId == spellId && x.modificationType == (byte)CharacterSpellModificationTypeEnum.HEAL_BONUS).FirstOrDefault();
        //            spell.AdditionalHeal -= (uint)s.value.additional;

        //            if (s != null)
        //                SpellsModifications.Remove(s);
        //        }
        //        catch (Exception ex)
        //        {
        //            logger.Error("Error - " + ex);
        //        }

        //        RefreshStats();
        //    }
        //}

        //public void AddCritical(short spellId, uint amount)
        //{
        //    if (Spells == null)
        //        return;

        //    if (Spells.Where(x => x.Id == spellId) == null || Spells.Where(x => x.Id == spellId).Count() < 1)
        //        return;

        //    var spell = Spells.Where(x => x.Id == spellId).First();

        //    if (spell != null)
        //    {
        //        spell.AdditionalCriticalPercent += amount;

        //        CharacterSpellModification s = new CharacterSpellModification(
        //            (sbyte)CharacterSpellModificationTypeEnum.CRITICAL_HIT_BONUS,
        //            (ushort)spellId,
        //            new CharacterCharacteristicDetailed(0, 0,0,0,0, (short)amount));

        //        SpellsModifications.Add(s);

        //        RefreshStats();
        //    }
        //}

        //public void AddCriticalDisable(short spellId)
        //{
        //    if (Spells == null)
        //        return;

        //    if (Spells.Where(x => x.Id == spellId) == null || Spells.Where(x => x.Id == spellId).Count() < 1)
        //        return;

        //    var spell = Spells.Where(x => x.Id == spellId).First();

        //    if (spell != null)
        //    {
        //        try
        //        {
        //            var s = SpellsModifications.Where(x => x.spellId == spellId && x.modificationType == (byte)CharacterSpellModificationTypeEnum.CRITICAL_HIT_BONUS).FirstOrDefault();
        //            spell.AdditionalCriticalPercent -= (uint)s.value.additional;

        //            if (s != null)
        //                SpellsModifications.Remove(s);
        //        }
        //        catch (Exception ex)
        //        {
        //            logger.Error("Error - " + ex);
        //        }

        //        RefreshStats();
        //    }
        //}
        //#endregion

        #region Titles & Ornaments

        public ReadOnlyCollection<ushort> Titles => Record.Titles.AsReadOnly();

        public ReadOnlyCollection<ushort> Ornaments => Record.Ornaments.AsReadOnly();

        public ushort? SelectedTitle
        {
            get { return Record.TitleId; }
            private set { Record.TitleId = value; }
        }

        public bool HasTitle(ushort title) => Record.Titles.Contains(title);

        public void AddTitle(ushort title)
        {
            if (HasTitle(title))
                return;

            Record.Titles.Add(title);
            TitleHandler.SendTitleGainedMessage(Client, (short)title);
        }

        public bool RemoveTitle(ushort title)
        {
            var result = Record.Titles.Remove(title);

            if (result)
                TitleHandler.SendTitleLostMessage(Client, (short)title);

            if (title == SelectedTitle)
                ResetTitle();

            return result;
        }

        public bool SelectTitle(ushort title)
        {
            if (!HasTitle(title))
                return false;

            SelectedTitle = title;
            TitleHandler.SendTitleSelectedMessage(Client, title);
            RefreshActor();
            return true;
        }

        public void ResetTitle()
        {
            SelectedTitle = null;
            TitleHandler.SendTitleSelectedMessage(Client, 0);
            RefreshActor();
        }

        public ushort? SelectedOrnament
        {
            get { return Record.Ornament; }
            private set { Record.Ornament = value; }
        }

        public bool HasOrnament(ushort ornament) => Record.Ornaments.Contains(ornament);

        public void AddOrnament(ushort ornament)
        {
            if (!HasOrnament(ornament))
            {
                Record.Ornaments.Add(ornament);
            }
            TitleHandler.SendOrnamentGainedMessage(Client, (ushort)ornament);
        }

        public bool RemoveOrnament(ushort ornament)
        {
            bool result;
            if (result = Record.Ornaments.Remove(ornament))
            {
                TitleHandler.SendTitlesAndOrnamentsListMessage(Client, this);
            }
            return result;
        }

        public void RemoveAllOrnament()
        {
            Record.Ornaments.Clear();
            TitleHandler.SendTitlesAndOrnamentsListMessage(Client, this);
        }

        public bool SelectOrnament(ushort ornament)
        {
            bool result;
            if (!HasOrnament(ornament))
            {
                result = false;
            }
            else
            {
                SelectedOrnament = ornament;
                TitleHandler.SendOrnamentSelectedMessage(Client, (short)ornament);
                RefreshActor();
                result = true;
            }
            return result;
        }

        public void ResetOrnament()
        {
            SelectedOrnament = null;
            TitleHandler.SendOrnamentSelectedMessage(Client, 0);
            RefreshActor();
        }

        #endregion Titles & Ornaments

        #region Apparence

        public bool CustomLookActivated
        {
            get { return m_record.CustomLookActivated; }
            set { m_record.CustomLookActivated = value; }
        }

        public ActorLook CustomLook
        {
            get { return m_record.CustomEntityLook; }
            set { m_record.CustomEntityLook = value; }
        }

        public ActorLook DefaultLook
        {
            get { return m_record.DefaultLook; }
            set
            {
                m_record.DefaultLook = value;

                UpdateLook();
            }
        }

        public override ActorLook Look
        {
            get { return CustomLookActivated ? CustomLook : m_look; }
            set
            {
                m_look = value;
                m_record.LastLook = value;
            }
        }

        public override SexTypeEnum Sex
        {
            get { return m_record.Sex; }
            protected set { m_record.Sex = value; }
        }

        public PlayableBreedEnum BreedId
        {
            get { return m_record.Breed; }
            private set
            {
                m_record.Breed = value;
                Breed = BreedManager.Instance.GetBreed(value);
            }
        }

        public Breed Breed
        {
            get;
            private set;
        }

        public Head Head
        {
            get;
            set;
        }

        public bool Invisible
        {
            get;
            private set;
        }

        public PlayerStatus Status
        {
            get;
            private set;
        }

        public void SetStatus(PlayerStatusEnum status)
        {
            if (Status.statusId == (sbyte)status)
                return;

            Status = new PlayerStatus((sbyte)status);
            CharacterStatusHandler.SendPlayerStatusUpdateMessage(Client, Status);
        }

        public bool IsAvailable(Character character, bool msg)
        {
            if (Status.statusId == (sbyte)PlayerStatusEnum.PLAYER_STATUS_SOLO)
                return false;

            if (Status.statusId == (sbyte)PlayerStatusEnum.PLAYER_STATUS_PRIVATE && !FriendsBook.IsFriend(character.Account.Id))
                return false;

            if (Status.statusId == (sbyte)PlayerStatusEnum.PLAYER_STATUS_AFK && !msg)
                return false;

            return true;
        }

        public bool ToggleInvisibility(bool toggle)
        {
            Invisible = toggle;

            if (!IsInFight())
                Map.Refresh(this);

            SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, toggle ? (short)236 : (short)237);

            return Invisible;
        }

        public bool ToggleInvisibility() => ToggleInvisibility(!Invisible);

        public void ResetDefaultLook()
        {
            var look = Breed.GetLook(Sex, true);
            look.SetColors(DefaultLook.Colors);

            Head = BreedManager.Instance.GetHead(x => x.Breed == (uint)BreedId && x.Gender == (uint)Sex && x.Order == Head.Order);

            foreach (var skin in Head.Skins)
                look.AddSkin(skin);

            DefaultLook = look;
        }

        public void UpdateLook(bool send = true)
        {
            var look = DefaultLook.Clone();

            look = Inventory.Where(x => x.IsEquiped()).Aggregate(look, (current, item) => item.UpdateItemSkin(current));

            switch (PlayerLifeStatus)
            {
                case PlayerLifeStatusEnum.STATUS_PHANTOM:
                    look.BonesID = 3;
                    look.AddSkin(Sex == SexTypeEnum.SEX_FEMALE ? (short)323 : (short)322);
                    look.AddSkin(Sex == SexTypeEnum.SEX_FEMALE ? Breed.FemaleGhostBonesId : Breed.MaleGhostBonesId);
                    break;
                case PlayerLifeStatusEnum.STATUS_TOMBSTONE:
                    look.BonesID = Breed.TombBonesId;
                    break;
            }

            if (IsRiding)
            {
                var mountLook = EquippedMount.Look.Clone();
                look.BonesID = 2;
                mountLook.SetRiderLook(look);

                look = mountLook;
            }
            var currentEmote = GetCurrentEmote();

            if (currentEmote != null)
            {
                look = currentEmote.UpdateEmoteLook(this, look, true);
            }

            Look = look;

            if (send)
                SendLookUpdated();
        }

        public void UpdateLook(Emote emote, bool apply, bool send = true)
        {
            Look = emote.UpdateEmoteLook(this, Look, apply);

            if (send)
                SendLookUpdated();
        }

        public void UpdateLook(BasePlayerItem item, bool send = true)
        {
            Look = item.UpdateItemSkin(Look);

            if (send)
                SendLookUpdated();
        }

        private void SendLookUpdated()
        {
            if (Fight != null)
            {
                Fighter.Look = Look.Clone();
                Fighter.Look.RemoveAuras();

                if (Fighter.IsDead() || Fighter.HasLeft())
                    return;

                ContextHandler.SendGameContextRefreshEntityLookMessage(CharacterContainer.Clients, Fighter);
            }
            else
            {
                ContextHandler.SendGameContextRefreshEntityLookMessage(CharacterContainer.Clients, this);
            }
        }

        public void RefreshActor()
        {
            if (Fight != null)
            {
                Fighter.Look = Look.Clone();
                Fighter.Look.RemoveAuras();

                Fight.Map.Area.ExecuteInContext(() =>
                   Fight.RefreshActor(Fighter));
            }
            else if (Map != null)
            {
                Map.Area.ExecuteInContext(() =>
                   Map.Refresh(this));
            }

            OnLookRefreshed();
        }

        #endregion Apparence

        #region Stats

        #region Delegates

        public delegate void LevelChangedHandler(Character character, ushort currentLevel, int difference);

        public delegate void GradeChangedHandler(Character character, sbyte currentGrade, int difference);

        #endregion Delegates

        #region Levels

        public ushort Level
        {
            get;
            private set;
        }

        public long Experience
        {
            get { return RealExperience - PrestigeRank * ExperienceManager.Instance.HighestCharacterExperience; }
            private set
            {
                RealExperience = PrestigeRank * ExperienceManager.Instance.HighestCharacterExperience + value;
                if ((value < UpperBoundExperience || Level >= ExperienceManager.Instance.HighestCharacterLevel) &&
                    value >= LowerBoundExperience) return;
                var lastLevel = Level;

                Level = ExperienceManager.Instance.GetCharacterLevel(value);

                LowerBoundExperience = ExperienceManager.Instance.GetCharacterLevelExperience(Level);
                UpperBoundExperience = ExperienceManager.Instance.GetCharacterNextLevelExperience(Level);

                var difference = Level - lastLevel;

                OnLevelChanged(Level, difference);
            }
        }

        public void LevelUp(ushort levelAdded)
        {
            ushort level;

            if (levelAdded + Level > ExperienceManager.Instance.HighestCharacterLevel)
                level = ExperienceManager.Instance.HighestCharacterLevel;
            else
                level = (ushort)(levelAdded + Level);

            var experience = ExperienceManager.Instance.GetCharacterLevelExperience(level);

            Experience = experience;
        }

        public void LevelDown(ushort levelRemoved)
        {
            ushort level;

            if (Level - levelRemoved < 1)
                level = 1;
            else
                level = (ushort)(Level - levelRemoved);

            var experience = ExperienceManager.Instance.GetCharacterLevelExperience(level);

            Experience = experience;
        }

        public void AddExperience(int amount)
        {
            Experience += amount;
        }

        public void AddExperience(long amount)
        {
            Experience += amount;
        }

        public void AddExperience(double amount)
        {
            Experience += (long)amount;
        }

        #endregion Levels

        public long LowerBoundExperience
        {
            get;
            private set;
        }

        public long UpperBoundExperience
        {
            get;
            private set;
        }

        public ushort StatsPoints
        {
            get { return m_record.StatsPoints; }
            set { m_record.StatsPoints = value; }
        }

        public ushort SpellsPoints
        {
            get { return m_record.SpellsPoints; }
            set { m_record.SpellsPoints = value; }
        }

        public short EnergyMax
        {
            get { return m_record.EnergyMax; }
            set { m_record.EnergyMax = value; }
        }

        public short Energy
        {
            get { return m_record.Energy; }
            set
            {
                var energy = (short)(value < 0 ? 0 : value);
                var diff = (short)(energy - m_record.Energy);

                m_record.Energy = energy;
                OnEnergyChanged(energy, diff);
            }
        }

        public PlayerLifeStatusEnum PlayerLifeStatus
        {
            get { return m_record.PlayerLifeStatus; }
            set
            {
                m_record.PlayerLifeStatus = value;
                OnPlayerLifeStatusChanged(value);
            }
        }

        public int LifePoints
        {
            get { return Stats.Health.Total; }
        }

        public int MaxLifePoints
        {
            get { return Stats.Health.TotalMax; }
        }

        public SpellInventory Spells
        {
            get;
            private set;
        }
        public StatsFields PrivateStats
        {
            get;
            private set;
        }
        public StatsFields Stats
        {
            get
            {
                if (CustomStatsActivated) return CustomStats;
                return PrivateStats;
            }
            set
            {
                if (CustomStatsActivated) CustomStats = value;
                else
                {
                    PrivateStats = value;
                }
            }
        }
        public StatsFields CustomStats
        {
            get;
            private set;
        }
        public bool CustomStatsActivated = false;

        public bool GodMode
        {
            get;
            private set;
        }

        public bool CriticalMode
        {
            get;
            private set;
        }

        private void OnEnergyChanged(short energy, short diff)
        {
            if (diff < 0)
                SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 34, Math.Abs(diff)); //Vous avez perdu <b>%1</b> points d'�nergie.

            if (energy > 0 && energy <= (Level * 10) && diff < 0)
                SendSystemMessage(11, false, energy);

            PlayerLifeStatus = energy > 0 ? PlayerLifeStatusEnum.STATUS_ALIVE_AND_KICKING : PlayerLifeStatusEnum.STATUS_TOMBSTONE;
        }

        private void OnPlayerLifeStatusChanged(PlayerLifeStatusEnum status)
        {
            if (status != PlayerLifeStatusEnum.STATUS_ALIVE_AND_KICKING)
                ForceDismount();

            var phoenixMapId = 0;

            if (status == PlayerLifeStatusEnum.STATUS_PHANTOM)
            {
                phoenixMapId = World.Instance.GetNearestGraveyard(Map).PhoenixMapId;
                StartRegen();
            }

            CharacterHandler.SendGameRolePlayPlayerLifeStatusMessage(Client, status, phoenixMapId);
            InitializationHandler.SendSetCharacterRestrictionsMessage(Client, this);

            UpdateLook();
        }

        public void FreeSoul()
        {
            if (PlayerLifeStatus != PlayerLifeStatusEnum.STATUS_TOMBSTONE)
                return;

            var graveyard = World.Instance.GetNearestGraveyard(Map);
            Teleport(graveyard.Map, graveyard.Map.GetCell(graveyard.CellId));

            PlayerLifeStatus = PlayerLifeStatusEnum.STATUS_PHANTOM;
        }

        public event LevelChangedHandler LevelChanged;

        private void OnLevelChanged(ushort currentLevel, int difference)
        {
            if (difference > 0 && currentLevel - difference <= 200)
            {
                if (currentLevel > 200) difference -= currentLevel - 200;
                SpellsPoints += (ushort)difference;
                StatsPoints += (ushort)(difference * 5);
                PrivateStats.Health.Base += (short)(difference * 5);
                PrivateStats.Health.DamageTaken = 0;
            }
            else if (difference > 0)
            {
                PrivateStats.Health.DamageTaken = 0;
            }

            #region > Auras
            // Primeira Aura de level 100
            if (currentLevel >= 100 && currentLevel - difference < 100)
            {
                PrivateStats.AP.Base++;
                AddOrnament((ushort)OrnamentEnum.NIVEAU_100);
                AddEmote(EmotesEnum.EMOTE_AURA_DE_PUISSANCE);
            }
            else if (currentLevel < 100 && currentLevel - difference >= 100)
            {
                PrivateStats.AP.Base--;
                RemoveOrnament((ushort)OrnamentEnum.NIVEAU_100);
                RemoveEmote(EmotesEnum.EMOTE_AURA_DE_PUISSANCE);
            }

            // Primeira Aura de level 160
            if (currentLevel >= 160 && currentLevel - difference < 160)
            {
                AddOrnament((ushort)OrnamentEnum.NIVEAU_160);
                AddEmote(EmotesEnum.EMOTE_AURA_DE_PUISSANCE);
            }
            else if (currentLevel < 160 && currentLevel - difference >= 160)
            {
                RemoveOrnament((ushort)OrnamentEnum.NIVEAU_160);
                RemoveEmote(EmotesEnum.EMOTE_AURA_DE_PUISSANCE);
            }

            // Primeira Aura de level 200
            if (currentLevel >= 200 && currentLevel - difference < 200)
            {
                AddOrnament((ushort)OrnamentEnum.NIVEAU_200);
                AddEmote(EmotesEnum.EMOTE_AURA_DE_PUISSANCE);
            }
            else if (currentLevel < 200 && currentLevel - difference >= 200)
            {
                RemoveOrnament((ushort)OrnamentEnum.NIVEAU_200);
                RemoveEmote(EmotesEnum.EMOTE_AURA_DE_PUISSANCE);
            }

            // ADIÇÃO DOS ORNAMENTOS DE LEVEIS OMEGAS
            // Primeira Ornamento de level 225
            if (currentLevel >= 225 && currentLevel - difference < 225)
            {
                AddOrnament((ushort)OrnamentEnum.OMEGA_25);
            }
            else if (currentLevel < 225 && currentLevel - difference >= 225)
            {
                RemoveOrnament((ushort)OrnamentEnum.OMEGA_25);
            }
            if (currentLevel >= 250 && currentLevel - difference < 250)
            {
                AddOrnament((ushort)OrnamentEnum.OMEGA_50);
            }
            else if (currentLevel < 250 && currentLevel - difference >= 250)
            {
                RemoveOrnament((ushort)OrnamentEnum.OMEGA_50);
            }
            if (currentLevel >= 275 && currentLevel - difference < 275)
            {
                AddOrnament((ushort)OrnamentEnum.OMEGA_75);
            }
            else if (currentLevel < 275 && currentLevel - difference >= 275)
            {
                RemoveOrnament((ushort)OrnamentEnum.OMEGA_75);
            }

            // Auras Revisão Omega
            // Primeira Aura de Omega level 300
            if (currentLevel >= 300 && currentLevel - difference < 300)
            {
                AddEmote((EmotesEnum)171);
                PlayEmote(EmotesEnum.OMEGA_100, true);
            }
            else if (currentLevel < 300 && currentLevel - difference >= 300)
            {
                RemoveEmote((EmotesEnum)171);
            }
            if (currentLevel >= 325 && currentLevel - difference < 325)
            {
                AddOrnament((ushort)OrnamentEnum.OMEGA_125);
            }
            else if (currentLevel < 325 && currentLevel - difference >= 325)
            {
                RemoveOrnament((ushort)OrnamentEnum.OMEGA_125);
            }
            if (currentLevel >= 350 && currentLevel - difference < 350)
            {
                AddOrnament((ushort)OrnamentEnum.OMEGA_150);
            }
            else if (currentLevel < 350 && currentLevel - difference >= 350)
            {
                RemoveOrnament((ushort)OrnamentEnum.OMEGA_150);
            }
            if (currentLevel >= 375 && currentLevel - difference < 375)
            {
                AddOrnament((ushort)OrnamentEnum.OMEGA_175);
            }
            else if (currentLevel < 375 && currentLevel - difference >= 375)
            {
                RemoveOrnament((ushort)OrnamentEnum.OMEGA_175);
            }

            // Primeira Aura de Omega level 400
            if (currentLevel >= 400 && currentLevel - difference < 400)
            {
                AddEmote((EmotesEnum)172);
                PlayEmote(EmotesEnum.OMEGA_200, true);
            }
            else if (currentLevel < 400 && currentLevel - difference >= 400)
            {
                RemoveEmote((EmotesEnum)172);
            }
            if (currentLevel >= 425 && currentLevel - difference < 425)
            {
                AddOrnament((ushort)OrnamentEnum.OMEGA_225);
            }
            else if (currentLevel < 425 && currentLevel - difference >= 425)
            {
                RemoveOrnament((ushort)OrnamentEnum.OMEGA_225);
            }
            if (currentLevel >= 450 && currentLevel - difference < 450)
            {
                AddOrnament((ushort)OrnamentEnum.OMEGA_250);
            }
            else if (currentLevel < 450 && currentLevel - difference >= 450)
            {
                RemoveOrnament((ushort)OrnamentEnum.OMEGA_250);
            }
            if (currentLevel >= 475 && currentLevel - difference < 475)
            {
                AddOrnament((ushort)OrnamentEnum.OMEGA_275);
            }
            else if (currentLevel < 475 && currentLevel - difference >= 475)
            {
                RemoveOrnament((ushort)OrnamentEnum.OMEGA_275);
            }

            // Primeira Aura de Omega level 500
            if (currentLevel >= 500 && currentLevel - difference < 500)
            {
                AddEmote((EmotesEnum)173);
                PlayEmote(EmotesEnum.OMEGA_300, true);
            }
            else if (currentLevel < 500 && currentLevel - difference >= 500)
            {
                RemoveEmote((EmotesEnum)173);
            }
            if (currentLevel >= 525 && currentLevel - difference < 525)
            {
                AddOrnament((ushort)OrnamentEnum.OMEGA_325);
            }
            else if (currentLevel < 525 && currentLevel - difference >= 525)
            {
                RemoveOrnament((ushort)OrnamentEnum.OMEGA_325);
            }
            if (currentLevel >= 550 && currentLevel - difference < 550)
            {
                AddOrnament((ushort)OrnamentEnum.OMEGA_350);
            }
            else if (currentLevel < 550 && currentLevel - difference >= 550)
            {
                RemoveOrnament((ushort)OrnamentEnum.OMEGA_350);
            }
            if (currentLevel >= 575 && currentLevel - difference < 575)
            {
                AddOrnament((ushort)OrnamentEnum.OMEGA_375);
            }
            else if (currentLevel < 575 && currentLevel - difference >= 575)
            {
                RemoveOrnament((ushort)OrnamentEnum.OMEGA_375);
            }

            // Primeira Aura de Omega level 600
            if (currentLevel >= 600 && currentLevel - difference < 600)
            {
                AddEmote((EmotesEnum)174);
            }
            else if (currentLevel < 600 && currentLevel - difference >= 600)
            {
                RemoveEmote((EmotesEnum)174);
            }
            if (currentLevel >= 625 && currentLevel - difference < 625)
            {
                AddOrnament((ushort)OrnamentEnum.OMEGA_425);
            }
            else if (currentLevel < 625 && currentLevel - difference >= 625)
            {
                RemoveOrnament((ushort)OrnamentEnum.OMEGA_425);
            }
            if (currentLevel >= 650 && currentLevel - difference < 650)
            {
                AddOrnament((ushort)OrnamentEnum.OMEGA_450);
            }
            else if (currentLevel < 650 && currentLevel - difference >= 650)
            {
                RemoveOrnament((ushort)OrnamentEnum.OMEGA_450);
            }
            if (currentLevel >= 675 && currentLevel - difference < 675)
            {
                AddOrnament((ushort)OrnamentEnum.OMEGA_475);
            }
            else if (currentLevel < 675 && currentLevel - difference >= 675)
            {
                RemoveOrnament((ushort)OrnamentEnum.OMEGA_475);
            }

            // Primeira Aura de Omega level 700
            if (currentLevel >= 700 && currentLevel - difference < 700)
            {
                AddEmote((EmotesEnum)175);
                PlayEmote(EmotesEnum.OMEGA_500, true);
            }
            else if (currentLevel < 700 && currentLevel - difference >= 700)
            {
                RemoveEmote((EmotesEnum)175);
            }
            // Final da Atribuição de Ornamentos e Emotes para Leveis Omegas
            #endregion

            #region > Criando Shorts e aprendendo as Spells
            var shortcuts = Shortcuts.SpellsShortcuts;

            foreach (var spell in Breed.Spells)
            {
                if (spell.ObtainLevel > currentLevel)
                {
                    var spellShortcuts = shortcuts.Where(x => x.Value.SpellId == spell.Spell).ToArray();

                    foreach (var shortcut in spellShortcuts)
                    {
                        Shortcuts.RemoveShortcut(ShortcutBarEnum.SPELL_SHORTCUT_BAR, shortcut.Key);
                    }

                    if (Spells.HasSpell(spell.Spell, true))
                    {
                        Spells.UnLearnSpell(spell.Spell);
                    }
                }
                else if (spell.ObtainLevel <= currentLevel && !Spells.HasSpell(spell.Spell, true) && !SpellsBlock.Contains(spell.Spell))
                {
                    Spells.LearnSpell(spell.Spell);

                    if (IsInIncarnation)
                    {
                        Shortcuts.AddSpellShortcut(Shortcuts.GetNextFreeSlot(ShortcutBarEnum.SPELL_SHORTCUT_BAR), (short)spell.Spell, false, false);
                    }
                    else
                    {
                        Shortcuts.AddSpellShortcut(Shortcuts.GetNextFreeSlot(ShortcutBarEnum.SPELL_SHORTCUT_BAR), (short)spell.Spell);
                    }
                }

                if (spell.VariantLevel > currentLevel)
                {
                    var variantShortcuts = shortcuts.Where(x => x.Value.SpellId == spell.VariantId).ToArray();
                    foreach (var shortcut in variantShortcuts)
                    {
                        Shortcuts.RemoveShortcut(ShortcutBarEnum.SPELL_SHORTCUT_BAR, shortcut.Key);
                    }

                    if (Spells.HasSpell(spell.VariantId, true))
                    {
                        Spells.UnLearnSpell(spell.VariantId);
                    }
                }
                else if (spell.VariantLevel <= currentLevel && !Spells.HasSpell(spell.VariantId, true) && !SpellsBlock.Contains(spell.VariantId))
                {
                    Spells.LearnSpell(spell.VariantId);
                }

                if (spell.Spell != 0)
                {
                    try
                    {
                        var currentSpell = Spells.GetSpell(spell.Spell);

                        if (currentSpell != null)
                        {
                            var level2 = currentSpell.ByLevel[2];

                            if (level2 != null && level2.MinPlayerLevel <= Level && currentSpell.Record.Level == 1)
                            {
                                currentSpell.BoostSpell();
                            }

                            var level3 = currentSpell.ByLevel[3];

                            if (level3 != null && level3.MinPlayerLevel <= Level && currentSpell.Record.Level == 2)
                            {
                                currentSpell.BoostSpell();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Trate a exceção apropriadamente ou remova o bloco catch vazio
                    }
                }
            }
            #endregion

            RefreshStats();

            if (currentLevel > 1)
            {
                if (difference > 0)
                    CharacterHandler.SendCharacterLevelUpMessage(Client, (ushort)currentLevel);

                CharacterHandler.SendCharacterLevelUpInformationMessage(Map.Clients, this, (ushort)currentLevel);
            }

            LevelChanged?.Invoke(this, currentLevel, difference);
        }

        public void ResetStats(bool additional = false)
        {
            if (IsInIncarnation)
            {
                return;
            }

            Stats.Agility.Base = 0;
            Stats.Strength.Base = 0;
            Stats.Vitality.Base = 0;
            Stats.Wisdom.Base = 0;
            Stats.Intelligence.Base = 0;
            Stats.Chance.Base = 0;

            if (additional)
            {
                Stats.Agility.Additional = 0;
                Stats.Strength.Additional = 0;
                Stats.Vitality.Additional = 0;
                Stats.Wisdom.Additional = 0;
                Stats.Intelligence.Additional = 0;
                Stats.Chance.Additional = 0;
            }

            var newPoints = ((Level >= (ushort)200 ? (ushort)200 : Level) - 1) * 5;
            StatsPoints = (ushort)newPoints;

            RefreshStats();
            Inventory.CheckItemsCriterias();

            //Caract�ristiques (de base et additionnelles) r�initialis�es.(469)
            //Caract�ristiques de base r�initialis�es.(470)
            SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, (short)(additional ? 469 : 470));
        }

        public void RefreshStats()
        {
            if (IsRegenActive())
                UpdateRegenedLife();

            CharacterHandler.SendCharacterStatsListMessage(Client);

            OnStatsResfreshed();
        }

        public void ToggleGodMode(bool state)
        {
            GodMode = state;
        }

        public void ToggleCriticalMode(bool state)
        {
            CriticalMode = state;
        }

        public bool IsGameMaster()
        {
            return UserGroup.IsGameMaster;
        }

        public void SetBreed(PlayableBreedEnum breed)
        {
            BreedId = breed;
            ResetDefaultLook();
        }

        #endregion Stats

        #region Mount

        public Mount EquippedMount
        {
            get { return m_equippedMount; }
            private set
            {
                m_equippedMount = value;
                Record.EquippedMount = value?.Id;

                if (value == null)
                {
                    IsRiding = false;
                }
            }
        }

        public bool IsRiding
        {
            get { return EquippedMount != null && Record.IsRiding; }
            private set { Record.IsRiding = value; }
        }

        public bool HasEquippedMount()
        {
            return EquippedMount != null;
        }

        public bool EquipMount(Mount mount)
        {
            mount.setMountOwner(this);

            EquippedMount = mount;
            EquippedMount.SetMountEquipped();
            EquippedMount.Inventory = new MountInventory(this);

            MountHandler.SendMountSetMessage(Client, mount.GetMountClientData());
            MountHandler.SendMountXpRatioMessage(Client, mount.GivenExperience);

            return true;
        }

        public Boolean UnEquipMount()
        {
            if (EquippedMount == null)
                return false;

            ForceDismount();

            if (EquippedMount.Harness != null)
            {
                Inventory.MoveItem(EquippedMount.Harness, CharacterInventoryPositionEnum.INVENTORY_POSITION_NOT_EQUIPED, true);

                // Votre harnachement est d�pos� dans votre inventaire.
                BasicHandler.SendTextInformationMessage(Client, TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 661);
            }

            EquippedMount.RemoveMountEquipped();
            EquippedMount.Save(MountManager.Instance.Database);
            EquippedMount = null;

            MountHandler.SendMountUnSetMessage(Client);

            return true;
        }

        public bool ReleaseMount()
        {
            if (EquippedMount == null || !IsMountInventoryEmpty())
                return false;

            Mount mount = EquippedMount;

            UnEquipMount();

            MountManager.Instance.DeleteMount(mount.Record);
            MountHandler.SendMountReleaseMessage(Client, mount.Id);

            return true;
        }

        public bool IsMountInventoryEmpty()
        {
            if (HasEquippedMount() && EquippedMount.Inventory.Count != 0)
            {
                this.SendServerMessageLang(
                    "Você deve primeiro esvaziar o inventário da sua montaria, antes de desequipá-la.",
                    "You must first empty your mount's inventory before unequipping it.",
                    "Primero debes vaciar el inventario de tu montura antes de desequiparla.",
                    "Vous devez d'abord vider l'inventaire de votre monture avant de la déséquiper.");

                return false;
            }
            else
            {
                return true;
            }
        }

        public bool RideMount()
        {
            return !IsRiding && ToggleRiding();
        }

        public bool Dismount()
        {
            return IsRiding && ToggleRiding();
        }

        public void ForceDismount()
        {
            IsRiding = false;

            if (EquippedMount == null)
                return;

            EquippedMount.UnApplyMountEffects();
            UpdateLook();

            //Vous descendez de votre monture.
            BasicHandler.SendTextInformationMessage(Client, TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 273);
            MountHandler.SendMountRidingMessage(Client, IsRiding);
        }

        public bool ToggleRiding()
        {
            if (EquippedMount == null)
                return false;

            if (!IsRiding && Level < Mount.RequiredLevel)
            {
                SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 227, Mount.RequiredLevel);
                return false;
            }

            if (IsBusy() || (IsInFight() && Fight.State != FightState.Placement))
            {
                //Une action est d�j� en cours. Impossible de monter ou de descendre de votre monture.
                BasicHandler.SendTextInformationMessage(Client, TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 355);
                return false;
            }

            IsRiding = !IsRiding;

            if (IsRiding)
            {
                BasePlayerItem pet = Inventory.TryGetItem(CharacterInventoryPositionEnum.ACCESSORY_POSITION_PETS);

                if (pet != null)
                    Inventory.MoveItem(pet, CharacterInventoryPositionEnum.INVENTORY_POSITION_NOT_EQUIPED);

                EquippedMount.ApplyMountEffects();
            }
            else
            {
                //Vous descendez de votre monture.
                BasicHandler.SendTextInformationMessage(Client, TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 273);

                EquippedMount.UnApplyMountEffects();
            }

            UpdateLook();

            MountHandler.SendMountRidingMessage(Client, IsRiding);

            return true;
        }

        private void LoadMounts()
        {
            if (Record.EquippedMount.HasValue)
            {
                EquippedMount = MountManager.Instance.GetEquippedMountByMountId((int)Record.EquippedMount);

                if (EquippedMount is null)
                {
                    logger.Error($"Failed to load equipped mount. Character ID: {this.Id}");
                }
                else
                {
                    EquippedMount.setMountOwner(this);
                    EquippedMount.Inventory = new MountInventory(this);
                    EquippedMount.Inventory.LoadRecord();

                    if (IsRiding)
                    {
                        EquippedMount.ApplyMountEffects(false);
                    }
                }
            }
        }

        #endregion Mount

        #region Guild

        public GuildMember GuildMember
        {
            get;
            set;
        }

        public Guild Guild
        {
            get { return GuildMember != null ? GuildMember.Guild : null; }
        }

        public bool WarnOnGuildConnection
        {
            get
            {
                var result = false;
                try
                {
                    result = Record.WarnOnGuildConnection;
                }
                catch { }
                return result;
            }
            set
            {
                try
                {
                    Record.WarnOnGuildConnection = value;
                    GuildHandler.SendGuildMemberWarnOnConnectionStateMessage(Client, value);
                }
                catch { }
            }
        }

        #endregion Guild

        #region Alignment

        public AlignmentSideEnum AlignmentSide
        {
            get { return m_record.AlignmentSide; }
            private set
            {
                m_record.AlignmentSide = value;
            }
        }

        public sbyte AlignmentGrade
        {
            get;
            private set;
        }

        public sbyte AlignmentValue
        {
            get { return m_record.AlignmentValue; }
            private set { m_record.AlignmentValue = value; }
        }

        public ushort Honor
        {
            get { return m_record.Honor; }
            set
            {
                m_record.Honor = value;
                if ((value > LowerBoundHonor && value < UpperBoundHonor))
                    return;

                var lastGrade = AlignmentGrade;

                AlignmentGrade = (sbyte)ExperienceManager.Instance.GetAlignementGrade(m_record.Honor);

                LowerBoundHonor = ExperienceManager.Instance.GetAlignementGradeHonor((byte)AlignmentGrade);
                UpperBoundHonor = ExperienceManager.Instance.GetAlignementNextGradeHonor((byte)AlignmentGrade);

                var difference = AlignmentGrade - lastGrade;

                if (difference != 0)
                    OnGradeChanged(AlignmentGrade, difference);
            }
        }

        public ushort LowerBoundHonor
        {
            get;
            private set;
        }

        public ushort UpperBoundHonor
        {
            get;
            private set;
        }

        public ushort Dishonor
        {
            get { return m_record.Dishonor; }
            private set { m_record.Dishonor = value; }
        }

        public int CharacterPower
        {
            get { return Id + Level; }
        }

        public int CharacterRankId
        {
            get { return m_record.RankId; }
            set
            {
                m_record.RankId = value;
            }
        }

        public int CharacterRankExp
        {
            get { return m_record.RankExp; }
            set
            {
                int before = m_record.RankExp;
                m_record.RankExp = value;
                if (m_record.RankExp < 0)
                {
                    m_record.RankExp = 0;
                    m_record.RankId = 1;
                }
                this.checkRank(before, m_record.RankExp);
            }
        }

        public int CharacterRankWin
        {
            get { return m_record.RankWin; }
            set
            {
                m_record.RankWin = value;
            }
        }

        public int CharacterRankLose
        {
            get { return m_record.RankLose; }
            set
            {
                m_record.RankLose = value;
            }
        }

        public DateTime CharacterRankReward
        {
            get { return m_record.RankReward; }
            set { m_record.RankReward = value; }
        }

        public void checkRank(int expBefore, int expAfter)
        {
            if (this.CharacterRankId == 1)
                return;
            var ranks = RankManager.Instance.getRanks();
            foreach (var rank in ranks)
            {
                if (expBefore < rank.Value.RankExp && expAfter >= rank.Value.RankExp)
                {
                    this.CharacterRankId = rank.Value.RankId;

                    switch (this.Account.Lang)
                    {
                        case "fr":
                            this.SendServerMessage("F�licitations, vous avez gagn� un nouveau rang dans le mode Duelliste '<b>" + this.GetCharacterRankName() + "</b>', �a va parfaitement!", Color.Chartreuse);
                            break;
                        case "es":
                            this.SendServerMessage("Felicitaciones has ganado un nuevo rango en modo Duelista '<b>" + this.GetCharacterRankName() + "</b>', te queda perfectamente!", Color.Chartreuse);
                            break;
                        case "en":
                            this.SendServerMessage("Congratulations you have won a new rank in Duelist mode '<b>" + this.GetCharacterRankName() + "</b>', it fits perfectly!", Color.Chartreuse);
                            break;
                        default:
                            this.SendServerMessage("Parab�ns, voc� ganhou uma nova classifica��o no modo Duelista '<b>" + this.GetCharacterRankName() + "</b>', ela se encaixa perfeitamente!", Color.Chartreuse);
                            break;
                    }
                    break;
                }
                if (expBefore >= rank.Value.RankExp && expAfter < rank.Value.RankExp)
                {
                    if (rank.Value.RankId == 1 || rank.Value.RankId == 0)
                    {
                        this.CharacterRankId = 1;

                        switch (this.Account.Lang)
                        {
                            case "fr":
                                this.SendServerMessage("Vous n'avez pas plus de rang � perdre en mode Dueliste, n'abandonnez pas '<b>" + this.GetCharacterRankName() + "</b>'...", Color.Chartreuse);
                                break;
                            case "es":
                                this.SendServerMessage("Usted no tiene m�s rango para perder en el modo Duelista, no desista a '<b>" + this.GetCharacterRankName() + "</b>'...", Color.Chartreuse);
                                break;
                            case "en":
                                this.SendServerMessage("You no longer have rank to lose in Duelist mode, do not give up '<b>" + this.GetCharacterRankName() + "</b>'...", Color.Chartreuse);
                                break;
                            default:
                                this.SendServerMessage("Voc� n�o tem mais rank para perder no modo Duelista, n�o desista '<b>" + this.GetCharacterRankName() + "</b>'...", Color.Chartreuse);
                                break;
                        }
                    }
                    else
                    {
                        this.CharacterRankId = rank.Value.RankId - 1;

                        switch (this.Account.Lang)
                        {
                            case "fr":
                                this.SendServerMessage("Vous avez perdu un rang en mode Duelist, maintenant vous �tes rang'<b>" + this.GetCharacterRankName() + "</b>'...", Color.Chartreuse);
                                break;
                            case "es":
                                this.SendServerMessage("Has perdido un rango en el modo Duelista, ahora eres rango '<b>" + this.GetCharacterRankName() + "</b>'...", Color.Chartreuse);
                                break;
                            case "en":
                                this.SendServerMessage("You have lost a rank in Duelist mode, now you are rank '<b>" + this.GetCharacterRankName() + "</b>'...", Color.Chartreuse);
                                break;
                            default:
                                this.SendServerMessage("Voc� perdeu um rank no modo Duelist, agora voc� � rank '<b>" + this.GetCharacterRankName() + "</b>'...", Color.Chartreuse);
                                break;
                        }
                    }
                    break;
                }
            }
        }
        public bool PvPEnabled
        {
            get { return m_record.PvPEnabled; }
            private set
            {
                m_record.PvPEnabled = value;
                OnPvPToggled();
            }
        }

        public string GetCharacterRankName()
        {
            return RankManager.Instance.getRecordById(this.CharacterRankId, this).RankName;
        }

        public int GetCharacterRankBonus()
        {
            return RankManager.Instance.getRecordById(this.CharacterRankId, this).RankBonus;
        }
        public void ChangeAlignementSide(AlignmentSideEnum side)
        {
            AlignmentSide = side;


            OnAligmenentSideChanged();
            if (side == AlignmentSideEnum.ALIGNMENT_ANGEL)
            {

                switch (Account.Lang)
                {
                    case "fr":
                        SendServerMessage("F�licitations, maintenant vous �tes <b>Bontariano</b> !");
                        break;
                    case "es":
                        SendServerMessage("Felicitaciones, ahora eres <b>Bontariano</b> !");
                        break;
                    case "en":
                        SendServerMessage("Congratulations, now you are <b>Bontariano</b> !");
                        break;
                    default:
                        SendServerMessage("Parab�ns, agora voc� � <b>Bontariano</b> !");
                        break;
                }
            }
            else if (side == AlignmentSideEnum.ALIGNMENT_EVIL)
            {
                switch (Account.Lang)
                {
                    case "fr":
                        SendServerMessage("F�licitations, maintenant vous �tes <b>Brakmariano</b> !");
                        break;
                    case "es":
                        SendServerMessage("Felicitaciones, ahora eres <b>Brakmariano</b> !");
                        break;
                    case "en":
                        SendServerMessage("Congratulations, now you are <b>Brakmariano</b> !");
                        break;
                    default:
                        SendServerMessage("Parab�ns, agora voc� � <b>Brakmariano</b> !");
                        break;
                }
            }
            else if (side == AlignmentSideEnum.ALIGNMENT_MERCENARY)
            {
                switch (Account.Lang)
                {
                    case "fr":
                        SendServerMessage("F�licitations, maintenant vous �tes <b>Mercenaire</b> !");
                        break;
                    case "es":
                        SendServerMessage("Felicitaciones, ahora eres <b>Mercenario</b> !");
                        break;
                    case "en":
                        SendServerMessage("Congratulations, now you are <b>Mercenary</b> !");
                        break;
                    default:
                        SendServerMessage("Parab�ns, agora voc� � <b>Mercen�rio</b> !");
                        break;
                }
            }
        }

        public void AddArrangements(ushort amount)
        {
            SlotsArrangements = (SlotsArrangements + amount) > 18 ? 18 : SlotsArrangements + amount;
        }

        public void AddHonor(ushort amount)
        {
            Honor += (Honor + amount) >= HonorLimit ? (ushort)(HonorLimit - Honor) : amount;
        }

        public void SubHonor(ushort amount)
        {
            if (Honor - amount < 0)
                Honor = 0;
            else
                Honor -= amount;
        }

        public void AddDishonor(ushort amount)
        {
            Dishonor += amount;
        }

        public void SubDishonor(ushort amount)
        {
            if (Dishonor - amount < 0)
                Dishonor = 0;
            else
                Dishonor -= amount;
        }

        public void TogglePvPMode(bool state)
        {
            if (IsInFight())
                return;

            PvPEnabled = state;
        }

        public event GradeChangedHandler GradeChanged;

        private void OnGradeChanged(sbyte currentLevel, int difference)
        {
            Map.Refresh(this);
            RefreshStats();

            GradeChanged?.Invoke(this, currentLevel, difference);
        }

        public event Action<Character, bool> PvPToggled;

        private void OnPvPToggled()
        {
            foreach (var item in Inventory.GetItems(x => x.Position == CharacterInventoryPositionEnum.ACCESSORY_POSITION_SHIELD && x.AreConditionFilled(this)))
                Inventory.MoveItem(item, CharacterInventoryPositionEnum.INVENTORY_POSITION_NOT_EQUIPED);

            if (!PvPEnabled)
            {
                var amount = (ushort)Math.Round(Honor * 0.05);

                SubHonor(amount);
                switch (Account.Lang)
                {
                    case "fr":
                        SendServerMessage($"Vous avez perdu <b>{amount}</b> points d'honneur.");
                        break;
                    case "es":
                        SendServerMessage($"Has perdido <b>{amount}</b> puntos de honor.");
                        break;
                    case "en":
                        SendServerMessage($"You have lost <b>{amount}</b> honor points.");
                        break;
                    default:
                        SendServerMessage($"Voc� perdeu <b>{amount}</b> pontos de honra.");
                        break;
                }
            }

            Map.Refresh(this);
            RefreshStats();

            PvPToggled?.Invoke(this, PvPEnabled);
        }

        public event Action<Character, AlignmentSideEnum> AlignmnentSideChanged;

        private void OnAligmenentSideChanged()
        {
            TogglePvPMode(true);
            Honor = 0;
            Dishonor = 0;
            if (AlignmentSide != AlignmentSideEnum.ALIGNMENT_NEUTRAL)
            {
                this.AlignmentGrade = 1;
            }
            else
            {
                this.AlignmentSide = 0;
            }
            Map.Refresh(this);
            RefreshStats();
            RefreshActor();
            PvPHandler.SendAlignmentRankUpdateMessage(Client, this);

            AlignmnentSideChanged?.Invoke(this, AlignmentSide);
            Client.Character.Map.Refresh(Client.Character);
        }

        #endregion Alignment

        #region Fight

        public CharacterFighter Fighter
        {
            get;
            private set;
        }
        public CompanionActor Companion
        {
            get;
            set;
        }

        public FightSpectator Spectator
        {
            get;
            private set;
        }

        public FightPvT TaxCollectorDefendFight
        {
            get;
            private set;
        }
        public FightPvMr PrismDefendFight { get; set; }
        public IFight Fight
        {
            get { return Fighter == null ? (Spectator != null ? Spectator.Fight : null) : Fighter.Fight; }
        }

        public FightTeam Team
        {
            get { return Fighter != null ? Fighter.Team : null; }
        }

        public bool IsGhost()
        {
            return PlayerLifeStatus != PlayerLifeStatusEnum.STATUS_ALIVE_AND_KICKING;
        }

        public bool IsSpectator()
        {
            return Spectator != null;
        }

        public bool IsInFight()
        {
            return IsSpectator() || IsFighting();
        }

        public bool IsFighting()
        {
            return Fighter != null;
        }

        public void SetDefender(FightPvT fight)
        {
            TaxCollectorDefendFight = fight;
        }

        public void ResetDefender()
        {
            TaxCollectorDefendFight = null;
        }
        public void SetDefender(FightPvMr fight)
        {
            PrismDefendFight = fight;
        }
        public void ResetPrismDefender()
        {
            PrismDefendFight = null;
        }
        #endregion Fight

        #region Shortcuts

        public ShortcutBar Shortcuts
        {
            get;
            private set;
        }

        #endregion Shortcuts

        #region Regen

        public byte RegenSpeed
        {
            get;
            private set;
        }

        public DateTime? RegenStartTime
        {
            get;
            private set;
        }

        #endregion Regen

        #region Chat

        public ChatHistory ChatHistory
        {
            get;
            private set;
        }

        public DateTime? MuteUntil
        {
            get { return m_record.MuteUntil; }
            private set { m_record.MuteUntil = value; }
        }

        public void Mute(TimeSpan time, Character from)
        {
            MuteUntil = DateTime.Now + time;

            // %1 vous a rendu muet pour %2 minute(s).
            SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 17, from.Name,
                (int)time.TotalMinutes);
        }

        public void Mute(TimeSpan time)
        {
            MuteUntil = DateTime.Now + time;
            // Le principe de pr�caution vous a rendu muet pour %1 seconde(s).
            SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 123, (int)time.TotalSeconds);
        }

        public void UnMute()
        {
            MuteUntil = null;

            switch (Account.Lang)
            {
                case "fr":
                    SendServerMessage("Vous avez �t� d�mut�.", Color.Red);
                    break;
                case "es":
                    SendServerMessage("Usted ha sido desmutado.", Color.Red);
                    break;
                case "en":
                    SendServerMessage("You have been unmuted.", Color.Red);
                    break;
                default:
                    SendServerMessage("Voc� foi desmutado.", Color.Red);
                    break;
            }
        }

        public bool IsMuted()
        {
            return MuteUntil.HasValue && MuteUntil > DateTime.Now;
        }

        public TimeSpan GetMuteRemainingTime()
        {
            if (!MuteUntil.HasValue)
                return TimeSpan.MaxValue;

            return MuteUntil.Value - DateTime.Now;
        }

        #endregion Chat

        #region Smiley

        public event Action<Character, int> MoodChanged;

        private void OnMoodChanged()
        {
            try
            {
                Guild?.UpdateMember(Guild.TryGetMember(Id));
                MoodChanged?.Invoke(this, SmileyMoodId);
            }
            catch { }

        }

        public ReadOnlyCollection<SmileyPacksEnum> SmileyPacks => Record.SmileyPacks.AsReadOnly();

        public int SmileyMoodId
        {
            get { return Record.SmileyMoodId; }
            set { Record.SmileyMoodId = value; }
        }

        public DateTime LastMoodChange
        {
            get;
            private set;
        }

        public bool HasSmileyPack(SmileyPacksEnum pack) => SmileyPacks.Contains(pack);

        public void AddSmileyPack(SmileyPacksEnum pack)
        {
            if (HasSmileyPack(pack))
                return;

            Record.SmileyPacks.Add(pack);
            ChatHandler.SendChatSmileyExtraPackListMessage(Client, SmileyPacks.ToArray());
        }

        public bool RemoveSmileyPack(SmileyPacksEnum pack)
        {
            var result = Record.SmileyPacks.Remove(pack);
            int[] smileys = null;
            if (result)
            {
                switch ((int)pack)
                {
                    case 1:
                        smileys = new int[] { 128, 129, 130, 131, 132, 133, 134, 135, 136, 137, 138, 139, 140, 141, 142, 143, 144, 145, 146, 147 }; //CHACHA_PACK
                        break;
                    case 2:
                        smileys = new int[] { 148, 149, 150, 151, 152, 153, 154, 155, 156, 157, 158, 159, 160, 161, 162, 163, 164, 165, 166, 167 }; //CHIENCHIEN_PACK
                        break;
                    case 3:
                        smileys = new int[] { 168, 169, 170, 171, 172, 173, 174, 175, 176, 177, 178, 179, 180, 181, 182, 183, 184, 185, 186, 187 }; //BILBY_PACK
                        break;
                    case 4:
                        smileys = new int[] { 188, 189, 190, 191, 192, 193, 194, 195, 196, 197, 198, 199, 200, 201, 202, 203, 204, 205, 206, 207 }; //HALOUINE_PACK
                        break;
                    case 5:
                        smileys = new int[] { 208, 209, 210, 211, 212, 213, 214, 215, 216, 217, 218, 219, 220, 221, 222, 223, 224, 225, 226, 227 }; //JORIS_PACK
                        break;
                    default:
                        break;
                }

                if (smileys != null)
                {
                    foreach (var a in smileys)
                    {
                        foreach (var b in Shortcuts.SmileyShortcuts.Where(x => x.Value.SmileyId == (short)a).ToList())
                        {
                            //var shortcut = Shortcuts.SmileyShortcuts.FirstOrDefault(x => x.Value.SmileyId == a);
                            if (b.Value != null)
                                Shortcuts.RemoveShortcut(ShortcutBarEnum.GENERAL_SHORTCUT_BAR, b.Key);
                        }
                    }
                }

                ChatHandler.SendChatSmileyExtraPackListMessage(Client, SmileyPacks.ToArray());
            }

            return result;
        }

        public override void DisplaySmiley(short smileyId)
        {
            ChatHandler.SendChatSmileyMessage(CharacterContainer.Clients, this, smileyId);
        }

        public void SetMood(short smileyId)
        {
            if (DateTime.Now - LastMoodChange < TimeSpan.FromSeconds(5))
                ChatHandler.SendMoodSmileyResultMessage(Client, 2, smileyId);
            else
            {
                SmileyMoodId = smileyId;
                LastMoodChange = DateTime.Now;

                ChatHandler.SendMoodSmileyResultMessage(Client, 0, smileyId);
                OnMoodChanged();
            }
        }

        #endregion Smiley

        #region Prestige

        public int PrestigeRank
        {
            get { return m_record.PrestigeRank; }
            set { m_record.PrestigeRank = value; }
        }

        public long RealExperience
        {
            get { return m_record.Experience; }
            private set { m_record.Experience = value; }
        }

        public bool IsPrestigeMax() => PrestigeRank == PrestigeManager.PrestigeTitles.Length;

        public PrestigeItem GetPrestigeItem()
        {
            if (!PrestigeManager.Instance.PrestigeEnabled)
                return null;

            return Inventory.TryGetItem(PrestigeManager.BonusItem) as PrestigeItem;
        }

        public PrestigeItem CreatePrestigeItem() => (PrestigeItem)Inventory.AddItem(PrestigeManager.BonusItem);

        public bool IncrementPrestige()
        {
            if (Level < 200 || IsPrestigeMax() && PrestigeManager.Instance.PrestigeEnabled)
                return false;

            PrestigeRank++;
            AddTitle((ushort)PrestigeManager.Instance.GetPrestigeTitle(PrestigeRank));

            switch (PrestigeRank)
            {
                case 1:
                    AddOrnament(16);
                    break;
                case 3:
                    AddOrnament(17);
                    break;
                case 5:
                    AddOrnament(18);
                    break;
                case 7:
                    AddOrnament(60);
                    break;
                case 9:
                    AddOrnament(92);
                    break;
                case 10:
                    AddOrnament(93);
                    break;
            }

            //Inventory.AddItem(atomItem, 10000 * PrestigeRank);

            var item = GetPrestigeItem();

            if (item == null)
                item = CreatePrestigeItem();
            else
            {
                item.UpdateEffects();
                Inventory.RefreshItem(item);
            }

            OpenPopup($"Tu es pass� prestige {PrestigeRank} ! \r\nTu repasses donc niveau 1. \r\nTu dois te d�connecter et te reconnecter pour voir ton prestige et ton niveau !");

            foreach (var equippedItem in Inventory.ToArray())
                Inventory.MoveItem(equippedItem, CharacterInventoryPositionEnum.INVENTORY_POSITION_NOT_EQUIPED);

            var points = (Spells.CountSpentBoostPoint() + SpellsPoints) - (Level - 1);

            Dismount();

            Experience = 0;
            Spells.ForgetAllSpells();
            SpellsPoints = (ushort)(points >= 0 ? points : 0);
            ResetStats();

            return true;
        }

        public bool DecrementPrestige()
        {
            RemoveTitle((ushort)PrestigeManager.Instance.GetPrestigeTitle(PrestigeRank));
            PrestigeRank--;

            var item = GetPrestigeItem();

            if (item != null)
            {
                if (PrestigeRank > 0)
                {
                    item.UpdateEffects();
                    Inventory.RefreshItem(item);
                }
                else Inventory.RemoveItem(item);
            }

            OpenPopup(
                string.Format(
                    "Vous venez de passer au rang prestige {0}. Vous repassez niveau 1 et vous avez acquis des bonus permanents visible sur l'objet '{1}' de votre inventaire, ",
                    PrestigeRank + 1, item.Template.Name) +
                "les bonus s'appliquent sans �quipper l'objet. Vous devez vous reconnecter pour actualiser votre niveau.");

            return true;
        }

        public void ResetPrestige()
        {
            foreach (var title in PrestigeManager.PrestigeTitles)
            {
                RemoveTitle((ushort)title);
            }
            PrestigeRank = 0;

            var item = GetPrestigeItem();

            if (item != null)
            {
                Inventory.RemoveItem(item);
            }
        }

        #endregion Prestige

        #region // ----------------- DG AscensionBasic By:Kenshin ---------------- //
        public int AscensionBasicStair
        {
            get { return m_record.AscensionBasicStair; }
            private set { m_record.AscensionBasicStair = value; }
        }

        public int GetAscensionBasicStair()
        {
            return AscensionBasicStair;
        }

        public bool IncrementAscensionBasicStair()
        {
            AddAscensionBasicStair(1);
            RefreshActor();
            return true;
        }

        public void AddAscensionBasicStair(int amount)
        {
            AscensionBasicStair += amount;
        }

        public void SetAscensionBasicStair(int amount)
        {
            AscensionBasicStair = amount;
        }

        public void SubAscensionStair(int amount)
        {
            if (AscensionBasicStair - amount < 0)
                AscensionBasicStair = 0;
            else
                AscensionBasicStair -= amount;
        }
        #endregion

        #region Arena
        public ArenaLeague ArenaLeague => LeaguesManager.Instance.GetLeague(6); //LeagueID

        public bool CanEnterArena(bool send = true)
        {
            #region Restrições
            if (Level < ArenaManager.ArenaMinLevel)
            {
                if (send)
                {
                    SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 326);//Você deve ter pelo menos nível 50 para lutar em Koliseu.
                    return false;
                }
            }
            if (ArenaPenality >= DateTime.Now)
            {
                if (send)
                {
                    SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 323);//Você foi banido do Koliseu por um tempo porque abandonou uma partida do Koliseu.
                    return false;
                }
            }
            if (IsInJail())
            {
                if (send)
                {
                    SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 339);//Você não pode participar do Koliseu de uma prisão.
                    return false;
                }
            }
            if (IsGhost())
            {
                if (send)
                {
                    SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 373);//Nenhuma luta de Koliseu será oferecida a você enquanto você estiver em uma tumba ou em um fantasma.
                    return false;
                }
            }
            if (Fight is ArenaFight)
            {
                if (send)
                {
                    SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 334);//Você já está em combate Koliseu.
                    return false;
                }
            }

            if (Fight is FightAgression || Fight is FightPvT || Fight is FightDuel)
                return false;
            #endregion

            return true;
        }

        #region Check Arena
        public void CheckArenaDailyProperties_1vs1()
        {
            if (m_record.ArenaDailyDate_1vs1.Day == DateTime.Now.Day || ArenaDailyMaxRank_1vs1 <= 0)
                return;

            var amountToken = (int)Math.Floor(ArenaDailyMaxRank_1vs1 / 10d);
            var amountKamas = (ArenaDailyMaxRank_1vs1 * 1000);

            if (amountToken > 6)
                amountToken = 6;

            m_record.ArenaDailyDate_1vs1 = DateTime.Now;
            /*ArenaDailyMaxRank_1vs1 = 0;
            ArenaDailyMatchsCount_1vs1 = 0;
            ArenaDailyMatchsWon_1vs1 = 0;*/

            Inventory.AddItem(ArenaManager.Instance.TokenItemTemplate, amountToken);
            Inventory.AddKamas(amountKamas);

            DisplayNotification(NotificationEnum.KOLIZÉUM, amountKamas, amountToken);
        }

        public void CheckArenaDailyProperties_3vs3_Team()
        {
            if (m_record.ArenaDailyDate_3vs3_Team.Day == DateTime.Now.Day || ArenaDailyMaxRank_3vs3_Team <= 0)
                return;

            var amountToken = (int)Math.Floor(ArenaDailyMaxRank_3vs3_Team / 10d);
            var amountKamas = (ArenaDailyMaxRank_3vs3_Team * 1000);

            if (amountToken > 6)
                amountToken = 6;

            m_record.ArenaDailyDate_3vs3_Team = DateTime.Now;
            /*ArenaDailyMaxRank_3vs3 = 0;
            ArenaDailyMatchsCount_3vs3 = 0;
            ArenaDailyMatchsWon_3vs3 = 0;*/

            Inventory.AddItem(ArenaManager.Instance.TokenItemTemplate, amountToken);
            Inventory.AddKamas(amountKamas);

            DisplayNotification(NotificationEnum.KOLIZÉUM, amountKamas, amountToken);
        }

        public void CheckArenaDailyProperties_3vs3_Solo()
        {
            if (m_record.ArenaDailyDate_3vs3_Solo.Day == DateTime.Now.Day || ArenaDailyMaxRank_3vs3_Solo <= 0)
                return;

            var amountToken = (int)Math.Floor(ArenaDailyMaxRank_3vs3_Solo / 10d);
            var amountKamas = (ArenaDailyMaxRank_3vs3_Solo * 1000);

            if (amountToken > 6)
                amountToken = 6;

            m_record.ArenaDailyDate_3vs3_Solo = DateTime.Now;
            /*ArenaDailyMaxRank_3vs3_Solo = 0;
            ArenaDailyMatchsCount_3vs3_Solo = 0;
            ArenaDailyMatchsWon_3vs3_Solo = 0;*/

            Inventory.AddItem(ArenaManager.Instance.TokenItemTemplate, amountToken);
            Inventory.AddKamas(amountKamas);

            DisplayNotification(NotificationEnum.KOLIZÉUM, amountKamas, amountToken);
        }
        #endregion

        #region Computer Arena
        public int ComputeWonArenaKolifichas(int rank)
        {
            if (rank > 500)
                rank = 500;

            double levelMultiplier = (this.Level > 200) ? this.Level / 40.0 : 5.0;
            double firstPart = rank * 0.2 / 100 * (0.20 * levelMultiplier);
            double secondPart = levelMultiplier * 10;

            return (int)Math.Floor(firstPart + secondPart);
        }


        public int ComputeWonArenaTokens(int rank)
        {
            Random random = new Random();
            int maxTokens = random.Next(50, 61);
            const int rankCap = 100;
            const double tokenMultiplier = 1.5;

            rank = Math.Min(rank, rankCap);
            double calculatedTokens = rank / (double)rankCap * maxTokens * tokenMultiplier;
            int adjustedValue = (int)Math.Min(Math.Floor(calculatedTokens), maxTokens);

            return adjustedValue;
        }


        public ulong ComputeWonArenaKamas()
        {
            return (ulong)Math.Floor(500 * (Level * (Level / 200d)));
        }

        public int ComputeWonExperience()
        {
            return (int)Math.Floor(1800 * (Level * (Level / 200d)));
        }
        #endregion

        public void UpdateArenaProperties(int rank, bool win, int mode)
        {
            if (mode == 1)
                CheckArenaDailyProperties_1vs1();
            else if (mode == 2)
                CheckArenaDailyProperties_3vs3_Solo();
            else
                CheckArenaDailyProperties_3vs3_Team();

            #region 1vs1
            if (mode == 1)
            {
                ArenaPointsRank_1vs1 = rank;

                if (rank > ArenaMaxPointsRank_1vs1)
                    ArenaMaxPointsRank_1vs1 = rank;

                if (rank > ArenaDailyMaxRank_1vs1)
                    ArenaDailyMaxRank_1vs1 = rank;

                ArenaDayFightCount_1vs1++;

                if (win)
                    ArenaDayVictoryCount_1vs1++;

                m_record.ArenaDailyDate_1vs1 = DateTime.Now;

            }
            #endregion

            #region 3vs3 Solo
            else if (mode == 2)
            {
                ArenaPointsRank_3vs3_Solo = rank;

                if (rank > ArenaMaxPointsRank_3vs3_Solo)
                    ArenaMaxPointsRank_3vs3_Solo = rank;

                if (rank > ArenaDailyMaxRank_3vs3_Solo)
                    ArenaDailyMaxRank_3vs3_Solo = rank;

                ArenaDayFightCount_3vs3_Solo++;

                if (win)
                    ArenaDayVictoryCount_3vs3_Solo++;

                m_record.ArenaDailyDate_3vs3_Solo = DateTime.Now;

            }
            #endregion

            #region 3vs3 Team
            else
            {
                ArenaPointsRank_3vs3_Team = rank;

                if (rank > ArenaMaxPointsRank_3vs3_Team)
                    ArenaMaxPointsRank_3vs3_Team = rank;

                if (rank > ArenaDailyMaxRank_3vs3_Team)
                    ArenaDailyMaxRank_3vs3_Team = rank;

                ArenaDayFightCount_3vs3_Team++;

                if (win)
                    ArenaDayVictoryCount_3vs3_Team++;

                m_record.ArenaDailyDate_3vs3_Team = DateTime.Now;
            }
            #endregion

            ContextRoleplayHandler.SendGameRolePlayArenaUpdatePlayerInfosMessage(Client, this);
        }

        public void SetArenaPenality(TimeSpan time)
        {
            ArenaPenality = DateTime.Now + time;
            SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 323);//Você foi banido do Koliseu por um tempo porque abandonou uma partida do Kolissium.
        }

        public void ToggleArenaPenality()
        {
            SetArenaPenality(TimeSpan.FromMinutes(ArenaManager.ArenaPenalityTime));
        }

        public void SetAgressionPenality(TimeSpan time)
        {
            AgressionPenality = DateTime.Now + time;

            #region MSG
            switch (Account.Lang)
            {
                case "fr":
                    SendServerMessage("Vous avez �t� interdit pour une p�riode de " + ArenaManager.ArenaPenalityTime + " ans pour avoir quitt� un match...", Color.DarkOrange);
                    break;
                case "es":
                    SendServerMessage("Usted ha sido prohibido por un per�odo de " + ArenaManager.ArenaPenalityTime + " por abandonar una partida...", Color.DarkOrange);
                    break;
                case "en":
                    SendServerMessage("You were banned for a period of " + ArenaManager.ArenaPenalityTime + " for leaving a match...", Color.DarkOrange);
                    break;
                default:
                    SendServerMessage("Voc� foi banido por um per�odo de " + ArenaManager.ArenaPenalityTime + " por abandonar uma partida...", Color.DarkOrange);
                    break;
            }
            #endregion

            this.battleFieldOn = false;
        }

        public void ToggleAgressionPenality()
        {
            SetAgressionPenality(TimeSpan.FromMinutes(ArenaManager.ArenaPenalityTime));
        }

        public void ToggleArenaWaitTime()
        {
            SetArenaPenality(TimeSpan.FromMinutes(ArenaManager.ArenaWaitTime));
        }

        #region Arena (1vs1)
        public int ArenaPointsRank_1vs1
        {
            get { return m_record.ArenaPointsRank_1vs1; }
            set { m_record.ArenaPointsRank_1vs1 = value; }
        }

        public int ArenaMaxPointsRank_1vs1
        {
            get { return m_record.ArenaMaxPointsRank_1vs1; }
            set { m_record.ArenaMaxPointsRank_1vs1 = value; }
        }

        public int ArenaDailyMaxRank_1vs1
        {
            get { return m_record.ArenaDailyMaxRank_1vs1; }
            set { m_record.ArenaDailyMaxRank_1vs1 = value; }
        }

        public int ArenaDayVictoryCount_1vs1
        {
            get { return m_record.ArenaDayVictoryCount_1vs1; }
            set { m_record.ArenaDayVictoryCount_1vs1 = value; }
        }

        public int ArenaDayFightCount_1vs1
        {
            get { return m_record.ArenaDayFightCount_1vs1; }
            set { m_record.ArenaDayFightCount_1vs1 = value; }
        }

        public DateTime ArenaDailyDate_1vs1
        {
            get { return m_record.ArenaDailyDate_1vs1; }
            set { m_record.ArenaDailyDate_1vs1 = value; }
        }
        #endregion

        #region Arena (3vs3)solo
        public int ArenaPointsRank_3vs3_Solo
        {
            get { return m_record.ArenaPointsRank_3vs3_Solo; }
            set { m_record.ArenaPointsRank_3vs3_Solo = value; }
        }

        public int ArenaMaxPointsRank_3vs3_Solo
        {
            get { return m_record.ArenaMaxPointsRank_3vs3_Solo; }
            set { m_record.ArenaMaxPointsRank_3vs3_Solo = value; }
        }

        public int ArenaDailyMaxRank_3vs3_Solo
        {
            get { return m_record.ArenaDailyMaxRank_3vs3_Solo; }
            set { m_record.ArenaDailyMaxRank_3vs3_Solo = value; }
        }

        public int ArenaDayVictoryCount_3vs3_Solo
        {
            get { return m_record.ArenaDayVictoryCount_3vs3_Solo; }
            set { m_record.ArenaDayVictoryCount_3vs3_Solo = value; }
        }

        public int ArenaDayFightCount_3vs3_Solo
        {
            get { return m_record.ArenaDayFightCount_3vs3_Solo; }
            set { m_record.ArenaDayFightCount_3vs3_Solo = value; }
        }

        public DateTime ArenaDailyDate_3vs3_Solo
        {
            get { return m_record.ArenaDailyDate_1vs1; }
            set { m_record.ArenaDailyDate_1vs1 = value; }
        }
        #endregion

        #region Arena (3vs3)team
        public int ArenaPointsRank_3vs3_Team
        {
            get { return m_record.ArenaPointsRank_3vs3_Team; }
            set { m_record.ArenaPointsRank_3vs3_Team = value; }
        }

        public int ArenaMaxPointsRank_3vs3_Team
        {
            get { return m_record.ArenaMaxPointsRank_3vs3_Team; }
            set { m_record.ArenaMaxPointsRank_3vs3_Team = value; }
        }

        public int ArenaDailyMaxRank_3vs3_Team
        {
            get { return m_record.ArenaDailyMaxRank_3vs3_Team; }
            set { m_record.ArenaDailyMaxRank_3vs3_Team = value; }
        }

        public int ArenaDayVictoryCount_3vs3_Team
        {
            get { return m_record.ArenaDayVictoryCount_3vs3_Team; }
            set { m_record.ArenaDayVictoryCount_3vs3_Team = value; }
        }

        public int ArenaDayFightCount_3vs3_Team
        {
            get { return m_record.ArenaDayFightCount_3vs3_Team; }
            set { m_record.ArenaDayFightCount_3vs3_Team = value; }
        }

        public DateTime ArenaDailyDate_3vs3_Team
        {
            get { return m_record.ArenaDailyDate_3vs3_Team; }
            set { m_record.ArenaDailyDate_3vs3_Team = value; }
        }
        #endregion

        public DateTime ArenaPenality
        {
            get { return m_record.ArenaPenalityDate; }
            set { m_record.ArenaPenalityDate = value; }
        }

        public DateTime AgressionPenality
        {
            get { return m_record.AgressionPenalityDate; }
            set { m_record.AgressionPenalityDate = value; }
        }

        public ArenaPopup ArenaPopup
        {
            get;
            set;
        }

        public int ArenaMode
        {
            get;
            set;
        }
        #endregion Arena

        #region Dung
        public DungInvitation DungPopup
        {
            get;
            set;
        }
        #endregion Dung

        #region VIP / Gold VIP
        public bool Vip
        {
            get
            {
                return Account.IsSubscribe == true;
            }
        }

        public bool GoldVip
        {
            get
            {
                return Account.IsSubscribe == true && Account.UserGroupId >= (int)RoleEnum.Gold_Vip;
            }
        }
        #endregion VIP

        #region Arrangements
        public int SlotsArrangements
        {
            get { return m_record.SlotsArrangements; }
            private set { m_record.SlotsArrangements = value; }
        }
        #endregion

        #endregion Properties

        #region Actions

        #region Chat

        public bool AdminMessagesEnabled
        {
            get;
            set;
        }

        public void SendConnectionMessages()
        {
            SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 89);

            if (Account.LastConnection != null && Account.LastConnectionIp != Client.IP)
            {
                var date = Account.LastConnection.Value;

                SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 152,
                    date.Year,
                    date.Month,
                    date.Day,
                    date.Hour,
                    date.Minute.ToString("00"),
                    Account.LastConnectionIp);

                SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 153, Client.IP);
            }

            #region Merchant OnLogin
            ulong kamasMerchant = 0;
            var merchantSoldItems = new List<ObjectItemQuantityPriceDateEffects>();

            foreach (var item in MerchantBag.ToArray())
            {
                var objectEffects = new List<ObjectEffect>();

                item.Effects.ForEach((effect) =>
                {
                    var objectEffect = new ObjectEffect();
                    objectEffect.actionId = (ushort)effect.Id;
                    objectEffects.Add(objectEffect);
                });

                if (item.StackSold <= 0)
                    continue;

                var price = item.Price * item.StackSold;
                kamasMerchant += price;

                merchantSoldItems.Add(new ObjectItemQuantityPriceDateEffects((ushort)item.Template.Id, (uint)item.StackSold, (ulong)price, new ObjectEffects(objectEffects), 0));

                item.StackSold = 0;

                if (item.Stack == 0)
                    MerchantBag.RemoveItem(item, true);
            }

            Inventory.AddKamas((long)kamasMerchant);
            #endregion

            #region Bidhouse OnLogin
            var soldItems = BidHouseManager.Instance.GetSoldBidHouseItems(Account.Id);
            var bidhouseSoldItems = new List<ObjectItemQuantityPriceDateEffects>();
            long AfterTokens = Client.Account.Tokens;
            var TokensBidHouse = 0;
            long ItemsSold = 0;

            foreach (var item in soldItems)
            {
                var objectEffects = new List<ObjectEffect>();

                item.Effects.ForEach((effect) =>
                {
                    var objectEffect = new ObjectEffect();
                    objectEffect.actionId = (ushort)effect.Id;
                    objectEffects.Add(objectEffect);
                });

                TokensBidHouse += (int)item.Price;
                ItemsSold += item.Stack;
                BidHouseManager.Instance.RemoveBidHouseItem(item, true);

                bidhouseSoldItems.Add(new ObjectItemQuantityPriceDateEffects((ushort)item.Template.Id, (uint)item.Stack, (ulong)item.Price, new ObjectEffects(objectEffects), 0));
            }

            if (bidhouseSoldItems.Count() > 0)
                Client.CreateAccountToken(TokensBidHouse, "BidHouseReceiver: " + NameClean.ToString(), "Token Ogrines", Id, NameClean);
            #endregion

            if (merchantSoldItems.Any())
            {
                InventoryHandler.SendExchangeOfflineSoldItemsMessage(Client, merchantSoldItems.ToArray(), bidhouseSoldItems.ToArray());
            }

            if (bidhouseSoldItems.Any())
            {
                #region MongoDB Merchant
                var document = new BsonDocument
                    {
                        { "CharacterId", Id },
                        { "CharacterName", NameClean },
                        { "AccountEmail", Client.Account.Email },
                        { "AccountItensSolds", ItemsSold },
                        { "TotalTokensSell", TokensBidHouse },
                        { "AccountAfterTokens", AfterTokens },
                        { "AccountBeforeTokens", Client.Account.Tokens},
                        { "Date", DateTime.Now.ToString(CultureInfo.CreateSpecificCulture("pt-BR")) }
                    };

                MongoLogger.Instance.Insert("Player_BidHouseReceiver", document);
                #endregion

                SendServerMessageLang
                    (
                    "<b>" + merchantSoldItems.Count() + "</b> lotes vendidos. Valor total: <b>" + merchantSoldItems.Sum(item => (long)item.price) + "</b> Ogrines",
                    "<b>" + merchantSoldItems.Count() + "</b> lots sold. Total amount: <b>" + merchantSoldItems.Sum(item => (long)item.price) + "</b> Ogrines",
                    "<b>" + merchantSoldItems.Count() + "</b> lotes vendidos. Importe total: <b>" + merchantSoldItems.Sum(item => (long)item.price) + "</b> Ogrines",
                    "<b>" + merchantSoldItems.Count() + "</b> lots vendus. Montant total : <b>" + merchantSoldItems.Sum(item => (long)item.price) + "</b> Ogrines"
                     );
            }
        }

        public void SendServerMessage(string message)
        {
            BasicHandler.SendTextInformationMessage(Client, TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 0, message);
        }

        public void SendServerMessage(string message, Color color)
        {
            SendServerMessage(string.Format("<font color=\"#{0}\">{1}</font>", color.ToArgb().ToString("X"), message));
        }

        //public void SendServerMessageLang(string message, string messageen, string messagees, string messagefr, Color color)
        public void SendServerMessageLang(string message, string messageen, string messagees, string messagefr)
        {
            switch (Client.Character.Account.Lang)
            {
                case "fr":
                    Client.Character.SendServerMessage(messagefr);
                    break;
                case "es":
                    Client.Character.SendServerMessage(messagees);
                    break;
                case "en":
                    Client.Character.SendServerMessage(messageen);
                    break;
                default:
                    Client.Character.SendServerMessage(message);
                    break;
            }
        }

        public void SendServerMessageLangColor(string message, string messageen, string messagees, string messagefr, Color color)
        {
            switch (Client.Character.Account.Lang)
            {
                case "fr":
                    Client.Character.SendServerMessage(messagefr, color);
                    break;
                case "es":
                    Client.Character.SendServerMessage(messagees, color);
                    break;
                case "en":
                    Client.Character.SendServerMessage(messageen, color);
                    break;
                default:
                    Client.Character.SendServerMessage(message, color);
                    break;
            }
        }

        public void SendServerDisplayLang(string message, string messageen, string messagees, string messagefr)
        {
            switch (Client.Character.Account.Lang)
            {
                case "fr":
                    Client.Character.DisplayNotification(messagefr);
                    break;
                case "es":
                    Client.Character.DisplayNotification(messagees);
                    break;
                case "en":
                    Client.Character.DisplayNotification(messageen);
                    break;
                default:
                    Client.Character.DisplayNotification(message);
                    break;
            }
        }

        public void SendInformationMessage(TextInformationTypeEnum msgType, short msgId, params object[] parameters)
        {
            BasicHandler.SendTextInformationMessage(Client, msgType, msgId, parameters);
        }

        public void SendSystemMessage(short msgId, bool hangUp, params object[] parameters)
        {
            BasicHandler.SendSystemMessageDisplayMessage(Client, hangUp, msgId, parameters);
        }

        public void OpenPopup(string message)
        {
            OpenPopup(message, "Servidor", 1);
        }

        public void OpenPopup(string message, string sender)
        {
            OpenPopup(message, sender, 1);
        }

        public void OpenPopup(string message, string sender, sbyte lockDuration)
        {
            if (lockDuration == 0)
                lockDuration = 1;

            ModerationHandler.SendPopupWarningMessage(Client, message, sender, lockDuration);
        }

        public void OpenPopupLang(string message, string messageen, string messagees, string messagefr, string sender, sbyte lockDuration)
        {
            if (lockDuration == 0)
                lockDuration = 1;

            switch (Client.Character.Account.Lang)
            {
                case "fr":
                    ModerationHandler.SendPopupWarningMessage(Client, messagefr, sender, lockDuration);
                    break;
                case "es":
                    ModerationHandler.SendPopupWarningMessage(Client, messagees, sender, lockDuration);
                    break;
                case "en":
                    ModerationHandler.SendPopupWarningMessage(Client, messageen, sender, lockDuration);
                    break;
                default:
                    ModerationHandler.SendPopupWarningMessage(Client, message, sender, lockDuration);
                    break;
            }
        }
        #endregion Chat

        #region Move
        public override void OnEnterMap(Map map)
        {
            ContextRoleplayHandler.SendCurrentMapMessage(Client, map.Id);

            foreach (var actor in map.Actors)
            {
                if (!actor.IsMoving())
                    continue;

                var moveKeys = actor.MovementPath.GetServerPathKeys();
                var actorMoving = actor;

                if (actor.MovementPath.Walk)
                {
                    ContextHandler.SendGameCautiousMapMovementMessage(map.Clients, moveKeys, actorMoving);
                }
                else
                {
                    ContextHandler.SendGameMapMovementMessage(map.Clients, moveKeys, actorMoving);
                }

                BasicHandler.SendBasicNoOperationMessage(map.Clients);
            }

            //if (map.Prism != null)
            //{
            //    PrismHandler.SendPrismsListUpdateMessage(Client, map.Prism, map.Prism.Alliance.Id == Guild?.Alliance?.Id);
            //}

            //TODO - Verificar sua utilidade.. para personagens sem guild ao logar em um mapa com coletor buga o login;
            //if (map.TaxCollector != null)
            //{
            //    TaxCollectorHandler.SendTaxCollectorListMessage(Client, map.TaxCollector.Guild);
            //}

            //if (IsInIncarnation)
            //{
            //    //IncarnationManager.Instance.CheckArea(this, map);
            //}

            if (map.Zaap != null && !KnownZaaps.Contains(map))
            {
                DiscoverZaap(map);
            }

            if (MustBeJailed() && !IsInJail())
            {
                TeleportToJail();
            }
            else if (!MustBeJailed() && IsInJail() && !IsGameMaster())
            {
                Teleport(Breed.GetStartPosition());
            }

            ResetCurrentSkill();

            foreach (var job in Jobs.Where(x => x.IsIndexed))
            {
                job.Template.RefreshCrafter(this);
            }

            #region >> Tutorial Animation
            if (Map.Id == 153092354 && Quests.FirstOrDefault(x => x.Id == 1629 && !x.Finished) != null && !Quests.FirstOrDefault(x => x.Id == 1629).CurrentStep.Objectives.FirstOrDefault(x => x.ObjectiveRecord.ObjectiveId == 10015).Finished)
            {
                Map.ActivateInteractiveObjectForASpecificPlayer(this, 489593);
            }
            else if (Map.Id == 153092354 && ((Quests.FirstOrDefault(x => x.Id == 1629 && !x.Finished) != null && Quests.FirstOrDefault(x => x.Id == 1629).CurrentStep.Objectives.FirstOrDefault(x => x.ObjectiveRecord.ObjectiveId == 10015).Finished) || Quests.FirstOrDefault(x => x.Id == 1629 && x.Finished) != null))
            {
                Map.DisableActivateStateInteractiveObjectForASpecificPlayer(this, 489593);
            }

            if (Map.Id == 153092354 && Quests.FirstOrDefault(x => x.Id == 1630 && !x.Finished) != null && !Quests.FirstOrDefault(x => x.Id == 1630).CurrentStep.Objectives.FirstOrDefault(x => x.ObjectiveRecord.ObjectiveId == 10016).Finished)
            {
                Map.ActivateInteractiveObjectForASpecificPlayer(this, 489541);
            }
            else if (Map.Id == 153092354 && ((Quests.FirstOrDefault(x => x.Id == 1630 && !x.Finished) != null && Quests.FirstOrDefault(x => x.Id == 1630).CurrentStep.Objectives.FirstOrDefault(x => x.ObjectiveRecord.ObjectiveId == 10016).Finished) || Quests.FirstOrDefault(x => x.Id == 1630 && x.Finished) != null))
            {
                Map.DisableActivateStateInteractiveObjectForASpecificPlayer(this, 489541);
            }
            #endregion

            base.OnEnterMap(map);
            SaveLater();
        }

        public override bool CanMove()
        {
            if (PlayerLifeStatus == PlayerLifeStatusEnum.STATUS_TOMBSTONE)
                return false;

            if (Fight?.State == FightState.Placement || Fight?.State == FightState.NotStarted)
                return false;

            return base.CanMove() && !IsDialoging();
        }

        public override bool IsGonnaChangeZone() => base.IsGonnaChangeZone() || !IsLoggedIn;

        public override bool StartMove(Path movementPath)
        {
            LeaveDialog(); //Close Dialog && RequestBox when moving

            if (Inventory.IsFull())
                movementPath.SetWalk();

            if (!IsFighting() && !MustBeJailed() && IsInJail())
            {
                Teleport(Breed.GetStartPosition());
                return false;
            }

            if (IsFighting())
                if (Fighter.IsSlaveTurn())
                    return Fighter.GetSlave().StartMove(movementPath);
                else return Fighter.StartMove(movementPath);

            CancelEmote(false);
            Look.RemoveAuras();

            return base.StartMove(movementPath);
        }

        public override bool StopMove() => IsFighting() ? Fighter.StopMove() : base.StopMove();

        public override bool MoveInstant(ObjectPosition destination) => IsFighting() ? Fighter.MoveInstant(destination) : base.MoveInstant(destination);

        public override bool StopMove(ObjectPosition currentObjectPosition) => IsFighting() ? Fighter.StopMove(currentObjectPosition) : base.StopMove(currentObjectPosition);

        public override bool Teleport(MapNeighbour mapNeighbour)
        {
            var success = base.Teleport(mapNeighbour);

            if (!success)
            {
                if (this.Account.UserGroupId >= (int)RoleEnum.GameMaster)
                {
                    switch (Account.Lang)
                    {
                        case "fr":
                            SendServerMessage("Transition de carte inconnue");
                            break;
                        case "es":
                            SendServerMessage("Transición de mapa desconocida");
                            break;
                        case "en":
                            SendServerMessage("Unknown map transition");
                            break;
                        default:
                            SendServerMessage("Transição de mapa desconhecido");
                            break;
                    }
                }
            }

            return success;
        }

        public override bool Teleport(Map mapScroll, MapNeighbour mapNeighbour)
        {
            var success = base.Teleport(mapScroll, mapNeighbour);

            if (!success)
            {
                if (this.Account.UserGroupId >= (int)RoleEnum.GameMaster)
                {
                    switch (Account.Lang)
                    {
                        case "fr":
                            SendServerMessage("Transition de carte inconnue");
                            break;
                        case "es":
                            SendServerMessage("Transición de mapa desconocida");
                            break;
                        case "en":
                            SendServerMessage("Unknown map transition");
                            break;
                        default:
                            SendServerMessage("Transição de mapa desconhecido");
                            break;
                    }
                }
            }

            return success;
        }

        #region Jail

        private readonly long[] JAILS_MAPS = { 105121026, 105119744, 105120002 };
        private readonly int[][] JAILS_CELLS = { new[] { 179, 445, 184, 435 }, new[] { 314 }, new[] { 300 } };

        public bool TeleportToJail()
        {
            var random = new AsyncRandom();

            var mapIndex = random.Next(0, JAILS_MAPS.Length);
            var cellIndex = random.Next(0, JAILS_CELLS[mapIndex].Length);

            var map = World.Instance.GetMap(JAILS_MAPS[mapIndex]);

            if (map == null)
            {
                logger.Error("Cannot find jail map {0}", JAILS_MAPS[mapIndex]);
                return false;
            }

            var cell = map.Cells[JAILS_CELLS[mapIndex][cellIndex]];

            Teleport(new ObjectPosition(map, cell), false);

            return true;
        }

        public bool MustBeJailed()
        {
            return Client.Account.IsJailed && (Client.Account.BanEndDate == null || Client.Account.BanEndDate > DateTime.Now);
        }

        public bool IsInJail()
        {
            return JAILS_MAPS.Contains(Map.Id);
        }

        #endregion Jail

        protected override void OnTeleported(ObjectPosition position)
        {
            base.OnTeleported(position);

            UpdateRegenedLife();

            if (Dialog != null)
                Dialog.Close();
        }

        public override bool CanChangeMap() => base.CanChangeMap() && !IsFighting() && !Account.IsJailed;

        #endregion Move

        public void OnKoh()
        {
            KingOfHill?.Invoke(this);
        }

        public void OnKoHRevive()
        {
            KoHRevive?.Invoke(this);
        }

        #region Dialog

        public void DisplayNotification(string text, NotificationEnum notification = NotificationEnum.INFORMATION)
        {
            Client.Send(new NotificationByServerMessage((ushort)notification, new[] { text }, true));
        }

        public void DisplayNotification(NotificationEnum notification, params object[] parameters)
        {
            Client.Send(new NotificationByServerMessage((ushort)notification, parameters.Select(entry => entry.ToString()), true));
        }

        public void DisplayNotification(Notification notification)
        {
            notification.Display();
        }

        public void LeaveDialog()
        {
            if (IsInRequest())
                CancelRequest();

            if (IsDialoging())
                Dialog.Close();
        }

        public void ReplyToNpc(uint replyId)
        {
            if (!IsTalkingWithNpc())
                return;

            ((NpcDialog)Dialog).Reply(replyId);
        }

        public void AcceptRequest()
        {
            if (!IsInRequest())
                return;

            if (RequestBox.Target == this)
                RequestBox.Accept();
        }

        public void DenyRequest()
        {
            if (!IsInRequest())
                return;

            if (RequestBox.Target == this)
                RequestBox.Deny();
        }

        public void CancelRequest()
        {
            if (!IsInRequest())
                return;

            if (IsRequestSource())
                RequestBox.Cancel();
            else if (IsRequestTarget())
                DenyRequest();
        }

        #endregion Dialog

        #region Party

        public void Invite(Character target, PartyTypeEnum type, bool force = false)
        {
            var created = false;
            Party party;
            if (!IsInParty(type))
            {
                party = PartyManager.Instance.Create(type);

                if (!EnterParty(party))
                    return;

                created = true;
            }
            else party = GetParty(type);
            PartyJoinErrorEnum error;
            if (!party.CanInvite(target, out error, this))
            {
                PartyHandler.SendPartyCannotJoinErrorMessage(target.Client, party, error);
                if (created)
                    LeaveParty(party);

                return;
            }

            if (target.m_partyInvitations.ContainsKey(party.Id))
            {
                if (created)
                    LeaveParty(party);

                return; // already invited
            }

            var invitation = new PartyInvitation(party, this, target);
            target.m_partyInvitations.Add(party.Id, invitation);

            party.AddGuest(target);

            if (force)
                invitation.Accept();
            else
                invitation.Display();
        }

        public PartyInvitation GetInvitation(int id)
        {
            return m_partyInvitations.ContainsKey(id) ? m_partyInvitations[id] : null;
        }

        public bool RemoveInvitation(PartyInvitation invitation)
        {
            return m_partyInvitations.Remove(invitation.Party.Id);
        }

        public void DenyAllInvitations()
        {
            foreach (var partyInvitation in m_partyInvitations.ToArray())
            {
                partyInvitation.Value.Deny();
            }
        }

        public void DenyAllInvitations(PartyTypeEnum type)
        {
            foreach (var partyInvitation in m_partyInvitations.Where(x => x.Value.Party.Type == type).ToArray())
            {
                partyInvitation.Value.Deny();
            }
        }

        public void DenyAllInvitations(Party party)
        {
            foreach (var partyInvitation in m_partyInvitations.Where(x => x.Value.Party == party).ToArray())
            {
                partyInvitation.Value.Deny();
            }
        }

        public bool EnterParty(Party party)
        {
            if (IsInParty(party.Type))
                LeaveParty(GetParty(party.Type));

            if (m_partyInvitations.ContainsKey(party.Id))
                m_partyInvitations.Remove(party.Id);

            DenyAllInvitations(party.Type);
            UpdateRegenedLife();

            if (party.Disbanded)
                return false;

            SetParty(party);
            party.MemberRemoved += OnPartyMemberRemoved;
            party.PartyDeleted += OnPartyDeleted;

            if (party.IsMember(this))
                return false;

            if (party.PromoteGuestToMember(this))
                return true;

            // if fails to enter
            party.MemberRemoved -= OnPartyMemberRemoved;
            party.PartyDeleted -= OnPartyDeleted;
            ResetParty(party.Type);

            return false;
        }

        public void LeaveParty(Party party)
        {
            if (!IsInParty(party.Id) || !party.CanLeaveParty(this))
                return;

            party.MemberRemoved -= OnPartyMemberRemoved;
            party.PartyDeleted -= OnPartyDeleted;
            party.RemoveMember(this);
            ResetParty(party.Type);
        }

        private void OnPartyMemberRemoved(Party party, Character member, bool kicked)
        {
            if (m_followedCharacter == member)
                UnfollowMember();

            if (member != this)
                return;

            party.MemberRemoved -= OnPartyMemberRemoved;
            party.PartyDeleted -= OnPartyDeleted;

            ResetParty(party.Type);
        }

        private void OnPartyDeleted(Party party)
        {
            party.MemberRemoved -= OnPartyMemberRemoved;
            party.PartyDeleted -= OnPartyDeleted;

            ResetParty(party.Type);
        }

        public void FollowMember(Character character)
        {
            if (m_followedCharacter != null)
                UnfollowMember();

            m_followedCharacter = character;
            character.EnterMap += OnFollowedMemberEnterMap;

            PartyHandler.SendPartyFollowStatusUpdateMessage(Client, Party, true, character.Id);
            CompassHandler.SendCompassUpdatePartyMemberMessage(Client, character, true);
        }
        public void FollowSpousee(Character character)
        {
            if (followforspouse == true)
                StopFollowSpouse();

            followforspouse = true;
            FollowSpouse(character);
            character.EnterMap += UpdateFollowSpouse;

            //PartyHandler.SendPartyFollowStatusUpdateMessage(Client, Party, true, character.Id);
            //CompassHandler.SendCompassUpdatePartyMemberMessage(Client, character, true);
            character.Client.Send(new CompassUpdatePartyMemberMessage((sbyte)CompassTypeEnum.COMPASS_TYPE_SPOUSE, new MapCoordinates((short)Map.Position.X, (short)Map.Position.Y), (ulong)Id, true));

        }
        public void StopFollowSpouse()
        {
            if (followforspouse == false)
                return;
            var spouse = World.Instance.GetCharacter(CurrentSpouse);
            if (spouse != null)
                StopFollowSpouse(spouse);
            EnterMap -= UpdateFollowSpouse;

            //PartyHandler.SendPartyFollowStatusUpdateMessage(Client, Party, true, 0);
            //CompassHandler.SendCompassUpdatePartyMemberMessage(Client, m_followedCharacter, false);

            // m_followedCharacter = null;

            if (spouse != null)
                spouse.Client.Send(new CompassUpdatePartyMemberMessage((sbyte)CompassTypeEnum.COMPASS_TYPE_SPOUSE, new MapCoordinates((short)Map.Position.X, (short)Map.Position.Y), (ulong)Id, false));
            followforspouse = false;
        }

        public void UnfollowMember()
        {
            if (m_followedCharacter == null)
                return;

            m_followedCharacter.EnterMap -= OnFollowedMemberEnterMap;

            PartyHandler.SendPartyFollowStatusUpdateMessage(Client, Party, true, 0);
            CompassHandler.SendCompassUpdatePartyMemberMessage(Client, m_followedCharacter, false);

            m_followedCharacter = null;
        }

        private void OnFollowedMemberEnterMap(RolePlayActor actor, Map map)
        {
            Character character = actor as Character;
            if (actor == null)
                return;

            CompassHandler.SendCompassUpdatePartyMemberMessage(Client, character, true);
        }

        #endregion Party

        #region Quest
        private List<Quest> m_quests = new List<Quest>();

        public List<QuestRecord> m_questsRecord = new List<QuestRecord>();

        public List<QuestObjectiveStatus> m_questsObjectiveRecord = new List<QuestObjectiveStatus>();

        public event Action<Character, Quest> OnQuestFinished;

        public ReadOnlyCollection<Quest> Quests => m_quests.AsReadOnly();

        private List<int> QuestsRepeats = new List<int> { 470, 469, 468, 467, 466, 465, 464, 463, 462, 461, 460, 459, 458, 708, 715, 940, 1617, 1679, 1843 };

        public void QuestCompleted(Quest quest)
        {
            OnQuestFinished?.Invoke(this, quest);
        }

        public void LoadQuests()
        {
            m_questsRecord = QuestManager.Instance.Database.Query<QuestRecord>(string.Format(QuestRecordRelator.FetchByOwner, Id)).ToList();
            m_questsObjectiveRecord = QuestManager.Instance.Database.Query<QuestObjectiveStatus>(string.Format(QuestRecordRelator.FetchObjectiveByOwner, Id)).ToList();

            foreach (var record in m_questsRecord)
            {
                record.Objectives = m_questsObjectiveRecord.Where(x => x.QuestId == record.QuestId && x.OwnerId == record.OwnerId).Select(y => y).ToList();
            }

            m_quests = m_questsRecord.Select(x => new Quest(this, x)).ToList();
        }

        public void StartQuest(int questStepId)
        {
            var step = QuestManager.Instance.GetQuestStep(questStepId);

            if (step == null)
                throw new Exception($"Step {questStepId} not found");

            StartQuest(step);
        }

        public void StartQuest(QuestStepTemplate questStep)
        {
            var quest = m_quests.FirstOrDefault(x => x.Template.Steps.Contains(questStep));

            if (quest == null)
            {
                quest = new Quest(this, questStep, false);
                m_quests.Add(quest);
                SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 54, quest.Id);
            }
            else if (quest != null && QuestsRepeats.Contains(quest.Id))
            {
                int index = m_quests.IndexOf(quest);

                if (index >= 0)
                    m_quests[index] = new Quest(this, questStep, true);

                SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 54, quest.Id);
            }
            else
            {
                quest.ChangeQuestStep(questStep);
            }

            #region >> ActivateInteractiveObjectForASpecificPlayer
            if (quest.Id == 1629 && Map.Id == 153092354)
            {
                Map.ActivateInteractiveObjectForASpecificPlayer(this, 489593);
            }

            if (quest.Id == 1630 && Map.Id == 153092354)
            {
                Map.ActivateInteractiveObjectForASpecificPlayer(this, 489541);
            }
            #endregion

            #region >> Att Player/Map
            foreach (var interac in Map.GetInteractiveObjects())
            {
                Map.Refresh(interac);
            }

            foreach (var npc in Map.Actors.OfType<Npc>())
            {
                npc.Refresh();
            }

            RefreshActor();
            #endregion

            SaveLater();
        }

        private void SaveQuests()
        {
            var database = QuestManager.Instance.Database;

            foreach (var quest in m_quests)
            {
                quest.Save(database);
            }
        }

        public bool HasQuestById(int questId)
        {
            return m_questsRecord.Any(x => x.QuestId == questId);
        }
        #endregion

        #region Fight

        public delegate void CharacterContextChangedHandler(Character character, bool inFight);

        public event CharacterContextChangedHandler ContextChanged;

        public delegate void CharacterEnterFightChangedHandler(CharacterFighter fighter);

        public event CharacterEnterFightChangedHandler EnterFight;

        public delegate void CharacterFightReadyStatusChanged(CharacterFighter fighter);

        public event CharacterFightReadyStatusChanged ReadyStatusChanged;

        public delegate void CharacterFightEndedHandler(Character character, CharacterFighter fighter);

        public event CharacterFightEndedHandler FightEnded;

        public delegate void CharacterFightStartedHandler(Character character, CharacterFighter fighter);

        public event CharacterFightStartedHandler FightStarted;

        public delegate void CharacterDiedHandler(Character character);

        public event CharacterDiedHandler Died;

        private void OnDied()
        {
            /*  var energylost = (short)(10 * Level);

              if (SuperArea.Id == 5) //Dimensions divines
                  energylost *= 2;

              Energy -= energylost;
              */
            if (!IsGhost())
            {
                var dest = GetSpawnPoint() ?? Breed.GetStartPosition();

                NextMap = dest.Map;
                Cell = dest.Cell ?? dest.Map.GetRandomFreeCell();
                Direction = dest.Direction;
            }

            Stats.Health.DamageTaken = (Stats.Health.TotalMax - 1);

            Died?.Invoke(this);
        }
        private void OnFightEnded(CharacterFighter fighter)
        {
            FightEnded?.Invoke(this, fighter);
        }
        public void OnFightStarted(CharacterFighter fighter)
        {
            FightStarted?.Invoke(this, fighter);
        }
        private void OnCharacterContextChanged(bool inFight)
        {
            ContextChanged?.Invoke(this, inFight);
        }
        public void OnCharacterContextReady(int mapId)
        {

        }
        public void OnEnterFight(CharacterFighter fighter)
        {
            EnterFight?.Invoke(fighter);
        }

        public ushort GetGrave()
        {
            ushort result;
            switch ((int)BreedId)
            {
                //bonesId of the grave for each breeds
                case 1:
                    result = 2384;
                    break;
                case 2:
                    result = 2380;
                    break;
                case 3:
                    result = 2373;
                    break;
                case 4:
                    result = 2376;
                    break;
                case 5:
                    result = 2386;
                    break;
                case 6:
                    result = 2378;
                    break;
                case 7:
                    result = 2383;
                    break;
                case 8:
                    result = 2374;
                    break;
                case 9:
                    result = 2372;
                    break;
                case 10:
                    result = 2381;
                    break;
                case 11:
                    result = 2379;
                    break;
                case 12:
                    result = 2375;
                    break;
                case 13:
                    result = 2382;
                    break;
                case 14:
                    result = 2377;
                    break;
                case 15:
                    result = 2385;
                    break;
                case 16:
                    result = 3091;
                    break;
                default:
                    result = 3091; //don't know the hupper grave :p
                    break;
            }
            return result;
        }

        public FighterRefusedReasonEnum CanRequestFight(Character target)
        {

            if (!target.IsInWorld || target.IsFighting() || target.IsSpectator() || target.IsBusy() || !target.IsAvailable(this, false))
                return FighterRefusedReasonEnum.OPPONENT_OCCUPIED;

            if (!IsInWorld || IsFighting() || IsSpectator() || IsBusy())
                return FighterRefusedReasonEnum.IM_OCCUPIED;

            if (target == this)
                return FighterRefusedReasonEnum.FIGHT_MYSELF;

            if (target.Map != Map || !Map.AllowChallenge)
                return FighterRefusedReasonEnum.WRONG_MAP;

            if (IsGhost())
                return FighterRefusedReasonEnum.GHOST_REFUSED;

            return FighterRefusedReasonEnum.FIGHTER_ACCEPTED;
        }

        public FighterRefusedReasonEnum CanAgressAvA(Character target)
        {
            FighterRefusedReasonEnum result;
            if (target == this)
            {
                result = FighterRefusedReasonEnum.FIGHT_MYSELF;
            }
            else
            {
                if (!target.AvAActived || !AvAActived)
                {

                    result = FighterRefusedReasonEnum.INSUFFICIENT_RIGHTS;
                }
                else
                {
                    if (!target.IsInWorld || target.IsFighting() || target.IsSpectator()) //|| target.IsBusy())
                    {
                        result = FighterRefusedReasonEnum.OPPONENT_OCCUPIED;
                    }
                    else
                    {
                        if (!IsInWorld || IsFighting() || IsSpectator() || IsBusy())
                        {
                            result = FighterRefusedReasonEnum.IM_OCCUPIED;
                        }
                        else
                        {
                            if (target.AvaState != AggressableStatusEnum.AvA_ENABLED_AGGRESSABLE ||
                                AvaState != AggressableStatusEnum.AvA_ENABLED_AGGRESSABLE)
                            {
                                result = FighterRefusedReasonEnum.INSUFFICIENT_RIGHTS;
                            }
                            else
                            {
                                if (target.Guild?.Alliance?.Id == Guild?.Alliance?.Id && target.Guild?.Alliance?.Id != null && Guild?.Alliance?.Id != null)
                                {
                                    result = FighterRefusedReasonEnum.WRONG_ALLIANCE;
                                }
                                else
                                {
                                    if (!SubArea.HasPrism || SubArea.Prism.State != PrismStateEnum.PRISM_STATE_VULNERABLE)
                                    {
                                        result = FighterRefusedReasonEnum.WRONG_MAP;
                                    }
                                    else
                                    {
                                        if (target.Client.IP == Client.IP)
                                        {
                                            result = FighterRefusedReasonEnum.MULTIACCOUNT_NOT_ALLOWED;
                                        }
                                        else
                                        {
                                            if (target.Client.Account.Email == Client.Account.Email)
                                            {
                                                return FighterRefusedReasonEnum.MULTIACCOUNT_NOT_ALLOWED;
                                            }
                                            result = target.Level < 50 || Level < 50 ? FighterRefusedReasonEnum.INSUFFICIENT_RIGHTS : FighterRefusedReasonEnum.FIGHTER_ACCEPTED;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }
        public bool CanBattlefield(Character target)
        {
            if (target == this)
                return false;

            if (!target.battleFieldOn)
                return false;

            if (!target.IsInWorld || target.IsFighting() || target.IsSpectator() || target.IsBusy() || !target.IsAvailable(this, false))
                return false;

            if (string.Equals(target.Client.IP, Client.IP) && !IsGameMaster())
                return false;

            if (Math.Abs(Level - target.Level) > 40)
                return false;

            if (IsGhost() || target.IsGhost())
                return false;
            return true;
        }

        public FighterRefusedReasonEnum CanAgress(Character target, bool bypassCheck = false)
        {
            if (target.Client.IP == Client.IP)
                return FighterRefusedReasonEnum.MULTIACCOUNT_NOT_ALLOWED;

            if (target.Client.Account.Email == Client.Account.Email)
                return FighterRefusedReasonEnum.MULTIACCOUNT_NOT_ALLOWED;

            if (target.Client.Account.LastHardwareId == Client.Account.LastHardwareId)
                return FighterRefusedReasonEnum.MULTIACCOUNT_NOT_ALLOWED;

            if (target == this)
                return FighterRefusedReasonEnum.FIGHT_MYSELF;

            if (!target.PvPEnabled || !PvPEnabled)
                return FighterRefusedReasonEnum.INSUFFICIENT_RIGHTS;

            if (!target.IsInWorld || target.IsFighting() || target.IsSpectator()) //|| target.IsBusy())
                return FighterRefusedReasonEnum.OPPONENT_OCCUPIED;

            if (!bypassCheck && (!IsInWorld || IsFighting() || IsSpectator() || IsBusy()))
                return FighterRefusedReasonEnum.IM_OCCUPIED;

            if (AlignmentSide <= AlignmentSideEnum.ALIGNMENT_NEUTRAL || target.AlignmentSide <= AlignmentSideEnum.ALIGNMENT_NEUTRAL)
                return FighterRefusedReasonEnum.WRONG_ALIGNMENT;

            if (target.AlignmentSide == AlignmentSide)
                return FighterRefusedReasonEnum.WRONG_ALIGNMENT;

            if (AvAActived && SubArea.HasPrism)
            {
                if (SubArea.Prism.State == PrismStateEnum.PRISM_STATE_VULNERABLE)
                    return (FighterRefusedReasonEnum)AvaState;
                //AvA_ENABLED_NON_AGGRESSABLE When full of alliances so can see but can't be aggresed!
            }

            if (!bypassCheck && (target.Map != Map || !Map.AllowAggression))
                return FighterRefusedReasonEnum.WRONG_MAP;

            if (string.Equals(target.Client.IP, Client.IP) && !IsGameMaster())
                return FighterRefusedReasonEnum.MULTIACCOUNT_NOT_ALLOWED;

            if (string.Equals(target.Client.Account.Email, Client.Account.Email))
                return FighterRefusedReasonEnum.MULTIACCOUNT_NOT_ALLOWED;

            if (string.Equals(target.Client.Account.LastHardwareId, Client.Account.LastHardwareId))
                return FighterRefusedReasonEnum.MULTIACCOUNT_NOT_ALLOWED;

            if (IsGhost() || target.IsGhost())
                return FighterRefusedReasonEnum.GHOST_REFUSED;

            #region Bloqueios de Niveis
            int LevelMath = Level > 200 ? 200 : Level;
            int TargetLevelMath = target.Level > 200 ? 200 : target.Level;

            if (Math.Abs(LevelMath) <= 200 && Math.Abs(TargetLevelMath) <= 200)
            {
                if (Math.Abs(LevelMath - TargetLevelMath) > 30)
                    return FighterRefusedReasonEnum.INSUFFICIENT_RIGHTS;
            }
            #endregion

            return FighterRefusedReasonEnum.FIGHTER_ACCEPTED;
        }

        public FighterRefusedReasonEnum CanAttack(TaxCollectorNpc target)
        {
            if (GuildMember != null && target.IsTaxCollectorOwner(GuildMember))
            {
                return FighterRefusedReasonEnum.WRONG_GUILD;
            }

            if (IsBusy() || IsFighting() || IsSpectator() || !IsInWorld)
            {
                return FighterRefusedReasonEnum.IM_OCCUPIED;
            }

            if (target.IsBusy() || target.IsFighting || !target.IsInWorld)
            {
                return FighterRefusedReasonEnum.OPPONENT_OCCUPIED;
            }

            if (target.Map != Map)
            {
                return FighterRefusedReasonEnum.WRONG_MAP;
            }

            if (IsGhost())
            {
                return FighterRefusedReasonEnum.GHOST_REFUSED;
            }

            return FighterRefusedReasonEnum.FIGHTER_ACCEPTED;
        }

        public FighterRefusedReasonEnum CanAttack(PrismNpc target)
        {

            FighterRefusedReasonEnum result;
            if (Guild?.Alliance != null && target.IsPrismOwner(Guild))
                result = FighterRefusedReasonEnum.WRONG_ALLIANCE;

            else
            {

                if (target.IsBusy())
                    result = FighterRefusedReasonEnum.OPPONENT_OCCUPIED;
                else
                {

                    result = target.Map != Map ?
                        FighterRefusedReasonEnum.WRONG_MAP :
                        FighterRefusedReasonEnum.FIGHTER_ACCEPTED;
                }
            }
            return result;
        }
        public FighterRefusedReasonEnum CanAttack(MonsterGroup group)
        {
            if (IsFighting() || IsSpectator() || !IsInWorld)
                return FighterRefusedReasonEnum.IM_OCCUPIED;

            if (!group.IsInWorld)
                return FighterRefusedReasonEnum.OPPONENT_OCCUPIED;

            if (group.Map != Map)
                return FighterRefusedReasonEnum.WRONG_MAP;

            if (IsGhost())
                return FighterRefusedReasonEnum.GHOST_REFUSED;

            return FighterRefusedReasonEnum.FIGHTER_ACCEPTED;
        }

        public CharacterFighter CreateFighter(FightTeam team)
        {
            if (IsFighting() || IsSpectator() || !IsInWorld)
                throw new Exception(string.Format("{0} is already in a fight", this));

            NextMap = Map; // we do not leave the map
            Map.Leave(this);

            foreach (var following in following)
            {
                following.Map.Leave(following);
            }

            StopRegen();

            if (IsInMovement)
                StopMove();

            ContextHandler.SendGameContextDestroyMessage(Client);
            ContextHandler.SendGameContextCreateMessage(Client, 2);
            RefreshActor();
            Fighter = new CharacterFighter(this, team);

            if (ContextHandler.GetFightAllowsChallenge(Fight))
            {
                int _challengeAmount = ContextHandler.GetChallengeCount(Fight);

                ContextHandler.SendChallengeNumberMessage(Client, _challengeAmount);
                ContextHandler.SendChallengeModSelectedMessage(Client, Client.Character.ChallengeMod);
                ContextHandler.SendChallengeBonusChoiceSelectedMessage(Client, Client.Character.ChallengeXpOrDrop);
            }

            ContextHandler.SendGameFightStartingMessage(Client, team.Fight.FightType, Fight.DefendersTeam.Leader == null ? 0 : Fight.DefendersTeam.Leader.Id, team.Leader == null ? 0 : team.Leader.Id, team.Fight);

            if (IsPartyLeader() && Party.RestrictFightToParty && team.Fighters.Count == 0 && !team.IsRestrictedToParty)
            {
                team.ToggleOption(FightOptionsEnum.FIGHT_OPTION_SET_TO_PARTY_ONLY);
            }

            OnCharacterContextChanged(true);

            return Fighter;
        }

        public CompanionActor CreateCompanion(FightTeam team)
        {
            CompanionActor result;

            CompanionRecord companion = null;
            var Listcompanion = Singleton<CompanionsManager>.Instance.GetCompanionById(Inventory.GetItems(CharacterInventoryPositionEnum.INVENTORY_POSITION_COMPANION).First().Template.Id);
            if (Listcompanion.Count() != 0)
            {
                companion = Listcompanion.First();
            }
            if (companion != null && (team.Fight is FightPvM || team.Fight is FightDuel))
            {
                List<Spell> spellsCompanion = new List<Spell>();
                foreach (var spell in companion.SpellsId)
                {

                    spellsCompanion.Add(new Spell(Singleton<SpellManager>.Instance.GetSpellTemplate(spell), 1));
                }

                Companion = new CompanionActor(this, team, ActorLook.Parse(companion.Look), spellsCompanion, (byte)companion.Id, Fight.GetNextContextualId());
                Companion.NextMap = Map;
                result = Companion;
            }
            else
            {
                result = null;
            }
            OnCharacterContextChanged(true);
            return result;
        }

        public FightSpectator CreateSpectator(IFight fight)
        {
            if (IsFighting() || IsSpectator() || !IsInWorld)
                throw new Exception(string.Format("{0} is already in a fight", this));

            if (!fight.CanSpectatorJoin(this))
                throw new Exception(string.Format("{0} cannot join fight in spectator", this));

            NextMap = Map; // we do not leave the map
            Map.Leave(this);
            foreach (var following in following)
                following.Map.Leave(following);
            StopRegen();

            if (IsInMovement)
                StopMove();

            ContextHandler.SendGameContextDestroyMessage(Client);
            ContextHandler.SendGameContextCreateMessage(Client, 2);
            ContextHandler.SendGameFightStartingMessage(Client, fight.FightType, fight.ChallengersTeam.Leader.Id, fight.DefendersTeam.Leader.Id, fight);

            Spectator = new FightSpectator(this, fight);

            OnCharacterContextChanged(true);

            return Spectator;
        }

        private CharacterFighter RejoinFightAfterDisconnection(CharacterFighter oldFighter)
        {
            Map.Leave(this);

            foreach (var following in following)
            {
                following.Map.Leave(following);
            }

            Map = oldFighter.Map;
            NextMap = oldFighter.Character.NextMap;

            StopRegen();

            ContextHandler.SendGameContextDestroyMessage(Client);
            ContextHandler.SendGameContextCreateMessage(Client, 2);
            ContextRoleplayHandler.SendCurrentMapMessage(Client, Map.Id);
            ContextRoleplayHandler.SendMapComplementaryInformationsDataMessage(Client);

            oldFighter.RestoreFighterFromDisconnection(this);
            Fighter = oldFighter;

            ContextHandler.SendGameFightStartingMessage(Client, Fighter.Fight.FightType, Fighter.Fight.ChallengersTeam.Leader.Id, Fighter.Fight.DefendersTeam.Leader.Id, Fighter.Fight);

            Fighter.Fight.RejoinFightFromDisconnection(Fighter);
            OnCharacterContextChanged(true);

            ContextHandler.SendChallengeListMessage(this.Client, this.Fighter.Fight);

            return Fighter;
        }

        /// <summary>
        /// Rejoin the map after a fight
        /// </summary>
        public void RejoinMap()
        {
            if (!IsFighting() && !IsSpectator())
                return;

            if (Fighter != null)
                OnFightEnded(Fighter);

            var defenders = Fight.DefendersTeam;

            if (IsGodOrPoutchCondition(defenders))
            {
                Stats.Health.DamageTaken = 0;
            }
            else if (Fighter != null && (Fighter.HasLeft() && !Fighter.IsDisconnected || Fight.Losers == Fighter.Team) && !Fight.IsDeathTemporarily)
            {
                OnDied();
            }

            if (!Client.Connected)
                return;

            Fighter = null;
            Spectator = null;

            ContextHandler.SendGameContextDestroyMessage(Client);
            ContextHandler.SendGameContextCreateMessage(Client, 1);
            RefreshStats();

            OnCharacterContextChanged(false);
            StartRegen();

            if (Map == null)
                return;

            if (IsLoggedIn)
            {
                if (!NextMap.Area.IsRunning)
                    NextMap.Area.Start();

                NextMap.Area.ExecuteInContext(() =>
                {
                    if (IsLoggedIn)
                    {
                        LastMap = Map;
                        Map = NextMap;
                        Map.Enter(this);
                        NextMap = null;

                        foreach (var following in following)
                        {
                            var position_char = Position.Clone();
                            following.Map = position_char.Map;

                            var excludedCells = position_char.Map.GetActors<RolePlayActor>().Select(entry => entry.Cell.Id);
                            following.Cell = position_char.Map.Cells[position_char.Point.GetAdjacentCells(true).Where(x => x.IsInMap()).OrderBy(x => x.ManhattanDistanceTo(position_char.Point)).Where(x => position_char.Map.Cells[x.CellId].Walkable && !excludedCells.Contains(x.CellId)).FirstOrDefault().CellId];

                            if (following is FollowerActor)
                                (following as FollowerActor).m_contextId = Map.GetNextContextualId();

                            Map.Enter(following);
                        }
                    }
                });

                RefreshActor();
                Map.Refresh(this);
            }
            else
            {
                SaveLater(); //if disconnected in fight we must save the change at the end of the fight 
            }
        }

        private bool IsGodOrPoutchCondition(FightTeam defenders)
        {
            List<int> poutchsId = new List<int>
            {
                (int)MonsterIdEnum.POUTCH_INGBALL_494,
                (int)MonsterIdEnum.POUTCH_INGBALL_5499,
                (int)MonsterIdEnum.POUTCH_OMBRE_3589,
                (int)MonsterIdEnum.POUTCH_CRNE_ROSE_3592,
                (int)MonsterIdEnum.POUTCH_HYPERSCAMPE_3590,
                (int)MonsterIdEnum.POUTCH_SYLARGH_3591,
                (int)MonsterIdEnum.POUTCH_VIL_SMISSE_3588,
            };

            return GodMode || defenders.GetAllFighters().OfType<MonsterFighter>().Any(m => poutchsId.Contains(m.Monster.Template.Id));
        }
        #endregion Fight

        #region Breach
        public BreachGroupInvitation BreachGroupInvitation
        {
            get;
            set;
        }

        public Character BreachOwner
        {
            get;
            set;
        }

        public long[] BreachGroup
        {
            get;
            set;
        }

        public int BreachBudget
        {
            get { return Record.BreachBudget; }
            set { Record.BreachBudget = value; }
        }

        public int BreachStep
        {
            get { return Record.BreachStep; }
            set { Record.BreachStep = value; }
        }

        public ExtendedBreachBranch CurrentBreachRoom
        {
            get;
            set;
        }

        public ObjectEffectInteger[] BreachBoosts
        {
            get
            {
                if (Record.BreachBoosts != null)
                {
                    return JsonConvert.DeserializeObject<List<ObjectEffectInteger>>(Record.BreachBoosts).ToArray();
                }
                else
                {
                    return new ObjectEffectInteger[] { };
                }
            }
            set { Record.BreachBoosts = JsonConvert.SerializeObject(value); }
        }

        public BreachReward[] BreachBuyables
        {
            get
            {
                if (Record.BreachBuyables != null)
                {
                    return JsonConvert.DeserializeObject<List<BreachReward>>(Record.BreachBuyables).ToArray();
                }
                else
                {
                    return new BreachReward[] { };
                }
            }
            set { Record.BreachBuyables = JsonConvert.SerializeObject(value); }
        }

        public ExtendedBreachBranch[] BreachBranches
        {
            get
            {
                if (Record.BreachBranches != null)
                {
                    return JsonConvert.DeserializeObject<List<ExtendedBreachBranch>>(Record.BreachBranches).ToArray();
                }
                else
                {
                    ExtendedBreachBranch[] extendedBreachBranches = BreachBranche.generateBreachBranches(this);
                    this.BreachBranches = extendedBreachBranches;
                    return extendedBreachBranches;
                }
            }
            set { Record.BreachBranches = JsonConvert.SerializeObject(value); }
        }
        #endregion

        #region Regen

        public bool IsRegenActive()
        {
            return RegenStartTime.HasValue;
        }

        public void StartRegen()
        {
            float Time = Rates.RegenRate;

            if (Client.UserGroup.Role == RoleEnum.Vip)
                Time = Rates.VipRegenRate;
            else if (Client.UserGroup.Role >= RoleEnum.Gold_Vip)
                Time = Rates.GoldRegenRate;

            StartRegen((byte)(10f / Time));
        }

        public void StartRegen(byte timePerHp)
        {
            if (IsRegenActive())
                StopRegen();

            if (PlayerLifeStatus == PlayerLifeStatusEnum.STATUS_TOMBSTONE)
                return;

            RegenStartTime = DateTime.Now;
            RegenSpeed = timePerHp;

            CharacterHandler.SendLifePointsRegenBeginMessage(Client, (byte)RegenSpeed);
        }

        public void StopRegen()
        {
            if (!IsRegenActive())
                return;

            var regainedLife = (int)Math.Floor((DateTime.Now - RegenStartTime).Value.TotalSeconds / (RegenSpeed / 10f));

            if (LifePoints + regainedLife > MaxLifePoints)
                regainedLife = MaxLifePoints - LifePoints;

            if (regainedLife > 0)
            {
                Stats.Health.DamageTaken -= regainedLife;
            }

            CharacterHandler.SendLifePointsRegenEndMessage(Client, regainedLife);

            RegenStartTime = null;
            RegenSpeed = 0;
            OnLifeRegened(regainedLife);
        }

        public void UpdateRegenedLife()
        {
            if (!IsRegenActive())
                return;

            var regainedLife = (int)Math.Floor((DateTime.Now - RegenStartTime).Value.TotalSeconds / (RegenSpeed / 10f));

            if (LifePoints + regainedLife > MaxLifePoints)
                regainedLife = MaxLifePoints - LifePoints;

            if (regainedLife > 0)
            {
                Stats.Health.DamageTaken -= regainedLife;
                CharacterHandler.SendUpdateLifePointsMessage(Client);
            }

            RegenStartTime = DateTime.Now;

            OnLifeRegened(regainedLife);
        }

        #endregion Regen

        #region Zaaps

        private ObjectPosition m_spawnPoint;

        public List<Map> KnownZaaps
        {
            get { return Record.KnownZaaps; }
        }

        public List<Map> KnownDungeon
        {
            get { return Record.KnownDungeons; }
        }

        //Aqui efetuar o cadastro do mapa ID em KnownDungeonsBin utilizar para criar o retorno de sala DG
        public void DiscoverDungeon(Map map)
        {
            if (!KnownDungeon.Contains(map) && !DungsDiscoverBlock.Contains(map.Id))
            {
                KnownDungeon.Add(map);
                //BasicHandler.SendTextInformationMessage(Client, TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 24);
            }
        }

        public void DiscoverZaap(Map map)
        {
            if (!KnownZaaps.Contains(map))
                KnownZaaps.Add(map);

            BasicHandler.SendTextInformationMessage(Client, TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 24);
            // new zaap
        }

        public void SetSpawnPoint(Map map)
        {
            Record.SpawnMap = map;
            m_spawnPoint = null;

            BasicHandler.SendTextInformationMessage(Client, TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 6);
            // pos saved

            InteractiveHandler.SendZaapRespawnUpdatedMessage(Client);
        }

        public ObjectPosition GetSpawnPoint()
        {
            if (Record.SpawnMap == null)
                return Breed.GetStartPosition();

            if (m_spawnPoint != null)
                return m_spawnPoint;

            var map = Record.SpawnMap;

            if (map.Zaap == null)
                return new ObjectPosition(map, map.GetRandomFreeCell(), Direction);

            var cell = map.GetRandomAdjacentFreeCell(map.Zaap.Position.Point);
            var direction = map.Zaap.Position.Point.OrientationTo(new MapPoint(cell));

            return new ObjectPosition(map, cell, direction);
        }

        #endregion Zaaps

        #region Emotes

        private LimitedStack<Pair<Emote, DateTime>> m_playedEmotes = new LimitedStack<Pair<Emote, DateTime>>(5);
        private bool m_cancelEmote;

        public ReadOnlyCollection<EmotesEnum> Emotes => Record.Emotes.AsReadOnly();

        public override Pair<Emote, DateTime> LastEmoteUsed => !m_cancelEmote && m_playedEmotes.Count > 0 ? m_playedEmotes.Peek() : null;

        private Pair<Emote, DateTime> GetCurrentEmotePair() => LastEmoteUsed != null &&
            (LastEmoteUsed.First.Duration == 0 || LastEmoteUsed.First.Persistancy || (DateTime.Now - LastEmoteUsed.Second) < TimeSpan.FromMilliseconds(LastEmoteUsed.First.Duration)) ? LastEmoteUsed : null;

        public Emote GetCurrentEmote() => GetCurrentEmotePair()?.First;

        public bool CancelEmote(bool send = true)
        {
            var emote = GetCurrentEmote();

            if (emote == null)
                return false;

            m_cancelEmote = true;
            UpdateLook(emote, false, false);

            if (send)
                ContextRoleplayHandler.SendEmotePlayMessage(CharacterContainer.Clients, this, 0);

            RefreshActor();

            return true;
        }

        public bool HasEmote(EmotesEnum emote) => Emotes.Contains(emote);

        public void AddEmote(EmotesEnum emote)
        {
            if (HasEmote(emote))
                return;

            Record.Emotes.Add(emote);
            ContextRoleplayHandler.SendEmoteAddMessage(Client, (ushort)emote);
        }

        public bool RemoveEmote(EmotesEnum emote)
        {
            var result = Record.Emotes.Remove(emote);

            if (result)
            {
                //var shortcut = Shortcuts.EmoteShortcuts.FirstOrDefault(x => x.Value.EmoteId == (int)emote);
                foreach (var emo in Shortcuts.EmoteShortcuts.Where(x => x.Value.EmoteId == (int)emote).ToList())
                {
                    if (emo.Value != null)
                        Shortcuts.RemoveShortcut(ShortcutBarEnum.GENERAL_SHORTCUT_BAR, emo.Key);
                }

                ContextRoleplayHandler.SendEmoteRemoveMessage(Client, (ushort)emote);
            }

            return result;
        }

        public void PlayEmote(EmotesEnum emoteId, bool force = false)
        {
            var emote = ChatManager.Instance.GetEmote((uint)emoteId);

            if (emote == null)
            {
                ContextRoleplayHandler.SendEmotePlayErrorMessage(Client, (ushort)emoteId);
                return;
            }

            if (!HasEmote(emoteId) && !force)
            {
                ContextRoleplayHandler.SendEmotePlayErrorMessage(Client, (ushort)emoteId);
                return;
            }

            var currentEmote = GetCurrentEmote();

            if (currentEmote != null)
            {
                CancelEmote();

                if (currentEmote == emote)
                {
                    return;
                }
            }

            m_cancelEmote = false;
            m_playedEmotes.Push(new Pair<Emote, DateTime>(emote, DateTime.Now));
            UpdateLook(emote, true, false);

            RefreshActor();

            ContextRoleplayHandler.SendEmotePlayMessage(CharacterContainer.Clients, this, emoteId);
        }

        #endregion Emotes

        #region FinishMove

        public ReadOnlyCollection<FinishMove> FinishMoves => Record.FinishMoves.AsReadOnly();

        public bool HasFinishMove(int finishMove) => FinishMoves.Any(x => x.Id == finishMove);

        public void AddFinishMove(int finishMove)
        {
            if (HasFinishMove(finishMove))
                return;

            Record.FinishMoves.Add(new FinishMove(finishMove, false));
        }

        public bool RemoveFinishMove(int finishMove)
        {
            if (HasFinishMove(finishMove))
                return false;

            return Record.FinishMoves.Remove(GetFinishMove(finishMove));
        }

        public FinishMove GetFinishMove(int finishMove)
        {
            return Record.FinishMoves.FirstOrDefault(x => x.Id == finishMove);
        }

        public FinishMoveInformations[] GetFinishMovesInformations()
        {
            return FinishMoves.Select(x => x.GetInformations()).ToArray();
        }

        #endregion FinishMove

        #region Friend & Ennemies

        public FriendsBook FriendsBook
        {
            get;
            private set;
        }

        #endregion Friend & Ennemies

        #region Merchant

        private Merchant m_merchantToSpawn;

        public bool CanEnableMerchantMode(bool sendError = true)
        {
            if (MerchantBag.Count == 0)
            {
                if (sendError)
                    SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 23);
                return false;
            }

            if (!Map.AllowHumanVendor)
            {
                if (sendError)
                    SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 237);

                return false;
            }

            if (Map.MerchantsDisabled)
            {
                if (sendError)
                    SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 237);

                return false;
            }

            if (Map.IsMerchantLimitReached())
            {
                if (sendError)
                    SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 25, Map.MaxMerchantsPerMap);
                return false;
            }

            if (MerchantManager.Instance.FindMerchantByMapCellId(Map.Id, Cell.Id).Count() > 0)
            {
                if (sendError)
                    SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 24);
                return false;
            }

            if (!Map.IsCellFree(Cell.Id, this))
            {
                if (sendError)
                    SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 24);
                return false;
            }

            //Restricción de Hydra del personal Por:Kenshin
            if (Client.Character.UserGroup.Role >= RoleEnum.Moderator_Helper && Client.Character.UserGroup.Role <= RoleEnum.Administrator || Client.Character.Invisible)
            {
                #region Menssagem Infor
                switch (Client.Character.Account.Lang)
                {
                    case "fr":
                        Client.Character.SendServerMessage("Vous n'êtes pas autorisé à activer le mode marchand. Vérifiez vos droits avec STAFF", System.Drawing.Color.Red);
                        break;
                    case "es":
                        Client.Character.SendServerMessage("No se le permite activar el modo comerciante. Verifique sus derechos con STAFF", System.Drawing.Color.Red);
                        break;
                    case "en":
                        Client.Character.SendServerMessage("You are not allowed to activate Merchant Mode. Check your rights with STAFF", System.Drawing.Color.Red);
                        break;
                    default:
                        Client.Character.SendServerMessage("Você não tem permissão para ativar o Modo Mercador. Consulte seus direitos com a STAFF", System.Drawing.Color.Red);
                        break;
                }
                #endregion

                #region MongoDB Logs Staff
                var document = new BsonDocument
                    {
                        { "AccountId", Client.Account.Id },
                        { "AccountName", Client.Account.Login },
                        { "CharacterId", Id },
                        { "CharacterName", Namedefault},
                        { "AbuseReason", "Enable Merchant"},
                        { "IPAddress", Client.IP },
                        { "ClientKey", Client.Account.LastHardwareId },
                        { "Date", DateTime.Now.ToString(CultureInfo.CreateSpecificCulture("pt-BR")) }
                    };

                MongoLogger.Instance.Insert("Staff_AbuseRights", document);
                #endregion
                return false;
            }

            if (Kamas >= MerchantBag.GetMerchantTax())
                return true;

            if (sendError)
                SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 76);

            return false;
        }

        public bool EnableMerchantMode()
        {
            var MercMap = Client.Character.Map;
            var MercCell = Client.Character.Cell;

            if (!CanEnableMerchantMode())
                return false;

            if (Client.Character.Direction >= DirectionsEnum.DIRECTION_WEST || Client.Character.Direction == DirectionsEnum.DIRECTION_SOUTH)
            {
                Client.Character.Teleport(new ObjectPosition(MercMap, MercCell, DirectionsEnum.DIRECTION_SOUTH_WEST));
            }

            Client.Character.Look.RemoveAuras();
            m_merchantToSpawn = new Merchant(this);
            Inventory.SubKamas(MerchantBag.GetMerchantTax());
            MerchantManager.Instance.AddMerchantSpawn(m_merchantToSpawn.Record);
            MerchantManager.Instance.ActiveMerchant(m_merchantToSpawn);
            Client.Disconnect();

            return true;
        }

        private void CheckMerchantModeReconnection()
        {
            foreach (var merchant in MerchantManager.Instance.UnActiveMerchantFromAccount(Client.WorldAccount))
            {
                try
                {
                    merchant.Save(WorldServer.Instance.DBAccessor.Database);

                    Console.WriteLine("Save successful. Merchant data has been successfully saved.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error during database save: {ex.Message}. The merchant data could not be saved.");
                }

                if (merchant.Record.CharacterId != Id)
                    continue;

                MerchantBag.LoadMerchantBag(merchant.Bag);

                MerchantManager.Instance.RemoveMerchantSpawn(merchant.Record);
            }

            // if the merchant wasn't active
            var record = MerchantManager.Instance.GetMerchantSpawn(Id);
            if (record == null)
                return;

            MerchantManager.Instance.RemoveMerchantSpawn(record);
        }

        #endregion Merchant

        #region Bank

        public Bank Bank
        {
            get;
            private set;
        }

        #endregion Bank

        #region Drop Items

        public void GetDroppedItem(WorldObjectItem objectItem)
        {
            if (Inventory.IsFull(objectItem.Item, objectItem.Quantity))
            {
                //Vous ne pouvez pas porter autant d'objets.
                SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 285);

                //Le nombre maximum d'objets pour cet inventaire est d�j� atteint.
                SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_POPUP, 6);
                return;
            }

            objectItem.Map.Leave(objectItem);
            Inventory.AddItem(objectItem.Item, objectItem.Effects, objectItem.Quantity);
        }

        public void DropItem(int itemId, int quantity)
        {
            if (quantity <= 0)
                return;

            var cell = Position.Point.GetAdjacentCells(x => Map.Cells[x].Walkable && Map.IsCellFree(x) && !Map.IsObjectItemOnCell(x)).FirstOrDefault();

            if (cell == null)
            {
                //Il n'y a pas assez de place ici.
                SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 145);
                return;
            }

            var item = Inventory.TryGetItem(itemId);

            if (item == null)
                return;

            if (item.IsLinkedToAccount() || item.IsLinkedToPlayer() || item.Template.Id == 20000) //Temporary block orb drop
                return;

            //Bloqueio de Venda de Itens comprados em NPCs Shops
            if (item.Effects.Exists(x => x.EffectId == EffectsEnum.Effect_BlockItemNpcShop) && Account.UserGroupId <= 3)
                return;

            if (item.Stack < quantity)
            {
                //Vous ne poss�dez pas l'objet en quantit� suffisante.
                SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 252);
                return;
            }

            Inventory.RemoveItem(item, quantity);

            var objectItem = new WorldObjectItem(item.Guid, Map, Map.Cells[cell.CellId], item.Template, item.Effects.Clone(), quantity);

            Map.Enter(objectItem);
        }

        #endregion Drop Items

        #region Debug

        public void ClearHighlight()
        {
            Client.Send(new DebugClearHighlightCellsMessage());
        }

        public Color HighlightCell(Cell cell)
        {
            var rand = new Random();
            var color = Color.FromArgb(0xFF << 24 | rand.Next(0xFFFFFF));
            HighlightCell(cell, color);

            return color;
        }

        public void HighlightCell(Cell cell, Color color)
        {
            Client.Send(new DebugHighlightCellsMessage(color.ToArgb() & 16777215, new[] {
                (ushort) cell.Id }));
        }

        public Color HighlightCells(IEnumerable<Cell> cells)
        {
            var rand = new Random();
            var color = Color.FromArgb(0xFF << 24 | rand.Next(0xFFFFFF));

            HighlightCells(cells, color);
            return color;
        }

        public void HighlightCells(IEnumerable<Cell> cells, Color color)
        {
            Client.Send(new DebugHighlightCellsMessage(color.ToArgb() & 16777215, cells.Select(x => (ushort)x.Id)));
        }

        #endregion Debug

        #endregion Actions

        #region Save & Load
        public bool IsLoggedIn
        {
            get;
            private set;
        }

        public bool IsAccountBlocked
        {
            get;
            private set;
        }

        public bool IsAuthSynced
        {
            get;
            set;
        }

        /// <summary>
        ///   Spawn the character on the map. It can be called once.
        /// </summary>
        public void LogIn()
        {
            #region Remover Informações
            //Checar se existe no inventario os itens que estão bugados no shortcuts
            foreach (var ItemsShort in Shortcuts.ItemsShortcuts.ToList())
            {
                if (!Inventory.GetItems().Any(x => x.Template.Id == ItemsShort.Value.ItemTemplateId))
                {
                    Shortcuts.RemoveShortcut(ShortcutBarEnum.GENERAL_SHORTCUT_BAR, ItemsShort.Key);
                }
            }
            #endregion

            #region Verificações
            //Checar Items com Status Bugados
            var accessoryPositions = new[]
            {
                CharacterInventoryPositionEnum.ACCESSORY_POSITION_AMULET,
                CharacterInventoryPositionEnum.ACCESSORY_POSITION_BELT,
                CharacterInventoryPositionEnum.ACCESSORY_POSITION_BOOTS,
                CharacterInventoryPositionEnum.ACCESSORY_POSITION_CAPE,
                CharacterInventoryPositionEnum.ACCESSORY_POSITION_HAT,
                CharacterInventoryPositionEnum.ACCESSORY_POSITION_SHIELD,
                CharacterInventoryPositionEnum.ACCESSORY_POSITION_WEAPON,
                CharacterInventoryPositionEnum.INVENTORY_POSITION_RING_LEFT,
                CharacterInventoryPositionEnum.INVENTORY_POSITION_RING_RIGHT
            };

            foreach (var itemCheck in Inventory.GetItems(x => accessoryPositions.Contains(x.Position)))
            {
                Inventory.CheckItemBug(itemCheck, true);
            }

            var mimisymbic = Inventory.TryGetItem(ItemIdEnum.MIMIBIOTE_14485);

            if (mimisymbic != null)
            {
                foreach (var mimi in Inventory.GetItems(x => x.Template.Id == (int)ItemIdEnum.MIMIBIOTE_14485))
                {
                    if (mimi.Effects.Exists(y => y.EffectId == EffectsEnum.Effect_BlockItemNpcShop))
                        Inventory.RemoveItem(mimi, 1, true);
                }
            }

            CeremonialRingsHandlers.RefreshCeremonialFollows(this);

            if (NotifyManager.Instance.HasNotifyMessage(this.Account.Id))
            {
                var notify = NotifyManager.Instance.GetNotifyMemberByAccountId(this.Account.Id);

                this.OpenPopup(notify.NotifyMessage, notify.ByAdmin, 60);

                NotifyManager.Instance.SetNotifyMemberByAccountIdUpdate(notify);
            }
            #endregion

            #region Bloqueios
            // TODO - Bloqueio de companheiros
            foreach (var item in Inventory.GetItems(x => x.Position == CharacterInventoryPositionEnum.INVENTORY_POSITION_COMPANION))
                Inventory.MoveItem(item, CharacterInventoryPositionEnum.INVENTORY_POSITION_NOT_EQUIPED);
            #endregion

            if ((DateTime.Now - loginTime).TotalSeconds <= 15)
                return;

            loginTime = DateTime.Now;

            if (IsInWorld)
                return;

            #region Troca de Classe Obrigatoria (Desativado e usado em casso de erro).
            //if (BreedId == (PlayableBreedEnum)18)
            //{
            //    Record.MandatoryChanges |= (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_BREED;
            //    Record.PossibleChanges |= (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_BREED;
            //    Record.PossibleChanges |= (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_GENDER;
            //    Record.PossibleChanges |= (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_COSMETIC;
            //    Record.PossibleChanges |= (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_COLORS;
            //    SendSystemMessage(63, false);
            //    Client.Disconnect();
            //}
            #endregion

            #region Sistema de Record de Players Server
            if (IPCAccessor.Instance.IsConnected)
            {
                IPCAccessor.Instance.SendRequest(new ServerUpdateMessage(WorldServer.Instance.ClientManager.Count, ServerStatusEnum.ONLINE), delegate (CommonOKMessage message)
                {
                    Game.Misc.ServerInfoManager.Instance.AddRecord(WorldServer.Instance.ClientManager.Count);
                });
            }

            if (Account.UserGroupId >= 4)
            {
                #region MSG
                switch (Account.Lang)
                {
                    case "fr":
                        SendServerMessage(string.Format("Serveur Record Players Online: " + Game.Misc.ServerInfoManager.Instance.GetRecord()));
                        break;
                    case "es":
                        SendServerMessage(string.Format("Servidor Record Players Online: " + Game.Misc.ServerInfoManager.Instance.GetRecord()));
                        break;
                    case "en":
                        SendServerMessage(string.Format("Server Record Players Online: " + Game.Misc.ServerInfoManager.Instance.GetRecord()));
                        break;
                    default:
                        SendServerMessage(string.Format("Server Record Players Online: " + Game.Misc.ServerInfoManager.Instance.GetRecord()));
                        break;
                }
                #endregion
            }
            #endregion

            if (Settings.Weekend && DateTime.Now.DayOfWeek == DayOfWeek.Saturday || DateTime.Now.DayOfWeek == DayOfWeek.Sunday && !IsInFight())
            {
                int Percent = UserGroup.Role >= RoleEnum.Gold_Vip ? Settings.WeekGoldVipValue : UserGroup.Role == RoleEnum.Vip ? Settings.WeekVipValue : Settings.WeekPlayerValue;

                SendServerDisplayLang
                    (
                    "[WEEK-END] Bônus de " + Percent + "% adicionado ao DROP de Kamas e XP nesse final de semana.",
                    "[WEEK-END] Bonus of " + Percent + "% added to Kamas and XP DROP this weekend.",
                    "[WEEK-END] Bonificación de " + Percent + "% añadido a Kamas y XP DROP este fin de semana.",
                    "[WEEK-END] Bonus de " + Percent + " % ajoutés aux Kamas et XP DROP ce week-end."
                    );
            }

            if (!IsInFight())
            {
                Task.Factory.StartNewDelayed(1250, () =>
                {
                    HavenBagManager.Instance.ExitHavenBag(Client);
                    MapsResetManager.Instance.ExitPlayerMap(Client);
                });
            }

            Task.Factory.StartNewDelayed(1250, () => Client.Send(Client.Character.Map.GetMapComplementaryInformationsDataMessage(Client.Character)));

            #region > Relogin em Luta ou Login
            CharacterFighter fighter = null; // Variável que armazena o lutador desconectado em uma luta em andamento
            CharacterFighter fightertwo = null; // Variável que armazena o lutador desconectado em uma nova luta

            if (Record.LeftFightId != null) // Verifica se o personagem está em uma luta e se está desconectado
            {
                var fight = FightManager.Instance.GetFight(Record.LeftFightId.Value);// Busca o objeto de luta correspondente
                fighter = fight?.GetLeaver(Id);// Busca o lutador desconectado na luta
            }
            else // Se não estiver em uma luta, verifica se há uma luta em andamento que o personagem participa
            {
                var fight = Map.Fights.FirstOrDefault(f => f.Fighters.Any(x => x.Id == Id));// Busca a primeira luta em que o personagem participa

                if (fight != null) // Se encontrou uma luta, armazena o ID da luta em Record.LeftFightId e busca o lutador desconectado na luta
                {
                    Record.LeftFightId = fight.Id;
                    fightertwo = FightManager.Instance.GetFight(Record.LeftFightId.Value)?.GetLeaver(Id);
                }
            }


            if (fighter?.IsDisconnected == true)// Se encontrou um lutador desconectado em uma luta, tenta reconectá-lo
            {
                Map.Area.AddMessage(() => RejoinFightAfterDisconnection(fighter));
                RefreshStats();
            }
            else if (fightertwo != null) // Se encontrou um lutador desconectado em uma nova luta, desconecta o personagem para evitar bugs
            {
                fightertwo.Fight.EndFight();
                Client.Disconnect();
            }
            else // Se não encontrou nenhum lutador desconectado, cria um novo contexto para o personagem
            {
                ContextHandler.SendGameContextDestroyMessage(Client);
                ContextHandler.SendGameContextCreateMessage(Client, 1);

                RefreshStats();

                Map.Area.AddMessage(() =>
                {
                    Map.Enter(this);
                    StartRegen();
                });
            }
            #endregion > Relogin em Luta ou Login

            World.Instance.Enter(this);
            m_inWorld = true;
            Inventory.CheckItemsCriterias();
            //Inventory.CheckICoinsPVM();
            Startupactions.StartupManager.Instance.confirmacao.Remove(Account.Id);
            CharacterGameTime.Start();

            IsLoggedIn = true;
            OnLoggedIn();

            foreach (var Dungeon in DungeonsAdd)
            {
                var map = World.Instance.GetMap(Dungeon);

                if (!KnownDungeon.Contains(map))
                    DiscoverDungeon(map);
            }

            if (Record.UnAcceptedAchievementsCSV != null)
            {
                foreach (var AchievementToUnlock in Record.m_unacceptedAchievements.ToArray())
                {
                    AchievementTemplate achievementTemplate = Singleton<AchievementManager>.Instance.TryGetAchievement(AchievementToUnlock);
                    Achievement.UnLockUnCompletedAchievement(achievementTemplate);
                }
            }

            if (Jobs.Any(x => x.Level >= 50) && UserGroup.Role <= RoleEnum.Gold_Vip) //Agrega el nombre a la lista de profesiones al iniciar sesión.
            {
                var subscriptions = new List<JobBookSubscription>();

                foreach (var job in Jobs.Where(x => x.Level >= 50))
                {
                    var addedOrRemoved = job.Template.AddOrRemoveAvailableCrafter(this);
                    job.IsIndexed = addedOrRemoved;

                    subscriptions.Add(new JobBookSubscription((sbyte)job.Template.Id, addedOrRemoved));
                }

                ContextRoleplayHandler.SendJobBookSubscriptionMessage(Client, subscriptions.ToArray());
            }

            int eventTimeTicket = AutoEventTicketManager.Instance.HasPlayerRewardRecord(this.Account.Id, this.WorldAccount.TotalGameTimeInSeconds);

            if (eventTimeTicket > 0)
            {
                AutoEventTicketManager.Instance.SetPlayerRecord(this.Account.Id, this.Account.Nickname, this.WorldAccount.TotalGameTimeInSeconds, eventTimeTicket);

                if (eventTimeTicket > 7)
                    eventTimeTicket = 7;

                int hoursDiference = eventTimeTicket * 2;

                this.Inventory.AddItem(ItemManager.Instance.TryGetTemplate(30018), eventTimeTicket);
                AutoEventTicketManager.Instance.SendMessageWebHook(this, eventTimeTicket, hoursDiference);

                #region // ----------------- Sistema de Logs MongoDB Entrega Ticket by: Kenshin ---------------- //
                try
                {
                    var document = new BsonDocument
                        {
                          { "AccountId", this.Account.Id },
                          { "AccountName", this.Account.Login },
                          { "CharacterId", this.Id },
                          { "CharacterName", this.Name },
                          { "TicketQuantity", eventTimeTicket },
                          { "Date", DateTime.Now.ToString(CultureInfo.CreateSpecificCulture("pt-BR")) }
                        };

                    MongoLogger.Instance.Insert("Player_EventTicket", document);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Erro no Mongologs do NPC de criação do Ticket Event : " + e.Message);
                }
                #endregion
            }
        }

        public void LogOut()
        {
            if (Area == null)
            {
                WorldServer.Instance.IOTaskPool.AddMessage(PerformLoggout);
            }
            else
            {
                Area.AddMessage(PerformLoggout);
            }
        }

        private void PerformLoggout()
        {
            lock (LoggoutSync)
            {
                IsLoggedIn = false;

                try
                {
                    OnLoggedOut();

                    if (!IsInWorld)
                        return;

                    DenyAllInvitations();

                    if (IsInRequest())
                        CancelRequest();

                    if (IsDialoging())
                        Dialog.Close();

                    if (ArenaParty != null)
                        LeaveParty(ArenaParty);

                    if (Party != null)
                        LeaveParty(Party);

                    CharacterGameTime.Stop();
                    CharacterGameTime.Reset();

                    if (Map != null && Map.IsActor(this))
                    {
                        Map.Leave(this);

                        foreach (var following in following)
                        {
                            following.Map.Leave(following);
                        }
                    }
                    else if (Area != null)
                    {
                        Area.Leave(this);
                    }

                    if (Map != null && m_merchantToSpawn != null)
                        Map.Enter(m_merchantToSpawn);

                    World.Instance.Leave(this);

                    m_inWorld = false;
                }
                catch (Exception ex)
                {
                    logger.Error("Cannot perfom OnLoggout actions, but trying to Save character : {0}", ex);
                }
                finally
                {
                    BlockAccount();
                    WorldServer.Instance.IOTaskPool.ExecuteInContext(() =>
                    {
                        try
                        {
                            SaveNow();
                            UnLoadRecord();
                        }
                        finally
                        {
                            Delete();
                        }
                    });
                }
            }
        }

        public void SaveLater()
        {
            BlockAccount();
            WorldServer.Instance.IOTaskPool.AddMessage(SaveNow);
        }

        internal void SaveNow()
        {
            try
            {
                WorldServer.Instance.IOTaskPool.EnsureContext();
                var database = WorldServer.Instance.DBAccessor.Database;

                lock (SaveSync)
                {
                    using (var transaction = database.GetTransaction())
                    {
                        Inventory.Save(database, false);
                        Bank.Save(database);
                        MerchantBag.Save(database);
                        Spells.Save();
                        Shortcuts.Save();
                        FriendsBook.Save();
                        Jobs.Save(database);
                        SaveQuests();
                        DoppleCollection.Save(ServerBase<WorldServer>.Instance.DBAccessor.Database);
                        MandatoryCollection.Save(ServerBase<WorldServer>.Instance.DBAccessor.Database);

                        m_record.MapId = NextMap != null ? NextMap.Id : Map.Id;
                        m_record.CellId = Cell.Id;
                        m_record.Direction = Direction;

                        if (!CustomStatsActivated)
                        {
                            m_record.AP = PrivateStats[PlayerFields.AP].Base < 0 ? 0 : PrivateStats[PlayerFields.AP].Base;
                            m_record.MP = PrivateStats[PlayerFields.MP].Base;
                            m_record.Strength = PrivateStats[PlayerFields.Strength].Base;
                            m_record.Agility = PrivateStats[PlayerFields.Agility].Base;
                            m_record.Chance = PrivateStats[PlayerFields.Chance].Base;
                            m_record.Intelligence = PrivateStats[PlayerFields.Intelligence].Base;
                            m_record.Wisdom = PrivateStats[PlayerFields.Wisdom].Base;
                            m_record.Vitality = PrivateStats[PlayerFields.Vitality].Base;

                            m_record.PermanentAddedStrength = (short)PrivateStats[PlayerFields.Strength].Additional;
                            m_record.PermanentAddedAgility = (short)PrivateStats[PlayerFields.Agility].Additional;
                            m_record.PermanentAddedChance = (short)PrivateStats[PlayerFields.Chance].Additional;
                            m_record.PermanentAddedIntelligence = (short)PrivateStats[PlayerFields.Intelligence].Additional;
                            m_record.PermanentAddedWisdom = (short)PrivateStats[PlayerFields.Wisdom].Additional;
                            m_record.PermanentAddedVitality = (short)PrivateStats[PlayerFields.Vitality].Additional;

                            m_record.BaseHealth = Stats.Health.Base;
                            m_record.DamageTaken = Stats.Health.DamageTaken;
                        }

                        database.Update(m_record);
                        database.Update(Client.WorldAccount);
                        transaction.Complete();
                    }
                }

                if (IsAuthSynced)
                    OnSaved();
                else
                {
                    IPCAccessor.Instance.SendRequest<CommonOKMessage>(new UpdateAccountMessage(Account), msg => { OnSaved(); });
                }
            }
            catch (Exception e)
            {
                UnBlockAccount();
                logger.Error(e);
                throw e;
            }
        }

        public void LoadRecord()
        {
            Breed = BreedManager.Instance.GetBreed(BreedId);
            Head = BreedManager.Instance.GetHead(Record.Head);
            var map = World.Instance.GetMap(m_record.MapId);

            if (map == null)
            {
                map = World.Instance.GetMap(Breed.StartMap);
                m_record.CellId = Breed.StartCell;
                m_record.Direction = Breed.StartDirection;
            }

            Position = new ObjectPosition(
                map,
                map.Cells[m_record.CellId],
                m_record.Direction);

            Stats = new StatsFields(this);
            Stats.Initialize(m_record);

            Level = ExperienceManager.Instance.GetCharacterLevel(Experience);
            LowerBoundExperience = ExperienceManager.Instance.GetCharacterLevelExperience(Level);
            UpperBoundExperience = ExperienceManager.Instance.GetCharacterNextLevelExperience(Level);

            AlignmentGrade = (sbyte)ExperienceManager.Instance.GetAlignementGrade(m_record.Honor);
            LowerBoundHonor = ExperienceManager.Instance.GetAlignementGradeHonor((byte)AlignmentGrade);
            UpperBoundHonor = ExperienceManager.Instance.GetAlignementNextGradeHonor((byte)AlignmentGrade);

            DoppleCollection = new DoppleCollection();
            DoppleCollection.Load(Id);

            MandatoryCollection = new MandatoryCollection();
            MandatoryCollection.Load(Id);

            Inventory = new Inventory(this);
            Inventory.LoadInventory();
            Inventory.LoadPresets();

            Bank = new Bank(this);
            Bank.LoadRecord();

            Jobs = new JobsCollection(this);
            Jobs.LoadJobs();

            MerchantBag = new CharacterMerchantBag(this);
            CheckMerchantModeReconnection();
            MerchantBag.LoadMerchantBag();

            GuildMember = GuildManager.Instance.TryGetGuildMember(Id);

            UpdateLook(false);

            LoadMounts();

            Spells = new SpellInventory(this);
            Spells.LoadSpells();

            Shortcuts = new ShortcutBar(this);
            Shortcuts.Load();

            FriendsBook = new FriendsBook(this);
            FriendsBook.Load();

            ChatHistory = new ChatHistory(this);

            LoadQuests();

            try
            {
                Achievement = new PlayerAchievement(this);
                Achievement.LoadAchievements();
            }
            catch { }

            m_recordLoaded = true;
        }

        public bool AvAActived { get; internal set; }

        public AggressableStatusEnum AvaState { get; internal set; }

        private void UnLoadRecord()
        {
            if (!m_recordLoaded)
                return;

            m_recordLoaded = false;
        }

        private void BlockAccount()
        {
            AccountManager.Instance.BlockAccount(Client.WorldAccount, this);
            IsAccountBlocked = true;
        }

        private void UnBlockAccount()
        {
            if (!IsAccountBlocked)
                return;

            AccountManager.Instance.UnBlockAccount(Client.WorldAccount);
            IsAccountBlocked = false;

            OnAccountUnblocked();
        }
        #endregion Save & Load

        #region Exceptions

        private readonly List<KeyValuePair<string, Exception>> m_commandsError = new List<KeyValuePair<string, Exception>>();
        private Mount m_equippedMount;
        private ActorLook m_look;

        public List<KeyValuePair<string, Exception>> CommandsErrors => m_commandsError;

        #endregion Exceptions

        #region Network

        #region GameRolePlayCharacterInformations

        public override GameContextActorInformations GetGameContextActorInformations(Character character)
        {
            return new GameRolePlayCharacterInformations(
                Id,
                GetEntityDispositionInformations(),
                Look.GetEntityLook(),
                Name,
                GetHumanInformations(),
                Account.Id,
                GetActorAlignmentInformations());
        }

        #endregion GameRolePlayCharacterInformations

        #region ActorAlignmentInformations

        public ActorAlignmentInformations GetActorAlignmentInformations()
        {
            return new ActorAlignmentInformations(
                (sbyte)AlignmentSide,
                AlignmentValue,
                PvPEnabled ? AlignmentGrade : (sbyte)0,
                CharacterPower);
        }

        #endregion ActorAlignmentInformations

        #region ActorExtendedAlignmentInformations
        public byte GetAgressablestatus()
        {
            if (AvAActived && SubArea.HasPrism)
            {
                if (SubArea.Prism.State == PrismStateEnum.PRISM_STATE_VULNERABLE)
                    return (byte)AvaState;

                //AvA_ENABLED_NON_AGGRESSABLE When full of alliances so can see but can't be aggresed!
            }

            if (PvPEnabled)
            {
                if (!Map.AllowAggression)
                {
                    return (byte)AggressableStatusEnum.PvP_ENABLED_NON_AGGRESSABLE;
                }

                return (byte)AggressableStatusEnum.PvP_ENABLED_AGGRESSABLE;
            }

            return (byte)AggressableStatusEnum.NON_AGGRESSABLE;
        }

        public ActorExtendedAlignmentInformations GetActorAlignmentExtendInformations()
        {
            return new ActorExtendedAlignmentInformations(
                (sbyte)AlignmentSide,
                AlignmentValue,
                PvPEnabled ? AlignmentGrade : (sbyte)0,
                CharacterPower,
                Honor,
                LowerBoundHonor,
                UpperBoundHonor,
                PvPEnabled ? (sbyte)AggressableStatusEnum.PvP_ENABLED_AGGRESSABLE : (sbyte)AggressableStatusEnum.NON_AGGRESSABLE);
        }
        #endregion ActorExtendedAlignmentInformations

        #region CharacterBaseInformations

        public CharacterBaseInformations GetCharacterBaseInformations()
        {
            return new CharacterBaseInformations(
                (ulong)Id,
                Namedefault,
                Level,
                Look.GetEntityLook(),
                (sbyte)BreedId,
                Sex == SexTypeEnum.SEX_FEMALE,
                1);
        }

        public CharacterMinimalInformations GetCharacterMinimalInformations() => new CharacterMinimalInformations((ulong)Id, Name, Level);

        public CharacterMinimalPlusLookInformations GetCharacterMinimalPlusLookInformations()
        {
            return new CharacterMinimalPlusLookInformations(
            (ulong)Id,
            Name,
            Level,
            Look.GetEntityLook(),
            (sbyte)Breed.Id);
        }

        /*
        *  1 - contextModif 2 - @base 3 - alignGiftBonus 4 - characteristicId 5 - objectsAndMountBonus 6 - additional
        *  
        *  contextModif : Stats Context
        *  @base        : Stats Base
        *  alignGiftBonus : Bônus de Alinhamento
        *  characteristicId : ID da Characteristic (Podendo usar Enums)
        *  objectsAndMountBonus : Stats de Todos Equipes do personagem
        *  additional : Stats Adicional
        */

        public List<CharacterCharacteristic> GetCharacterCharacteristic()
        {
            var characteristic = new List<CharacterCharacteristic>()
            {
                new CharacterCharacteristicDetailed(contextModif: (short)Stats.Health.Context, @base: (short)Stats.Health.Base, alignGiftBonus: 0, characteristicId: 0, objectsAndMountBonus: (short)Stats.Health.Equiped, additional: (short)Stats.Health.Additional),
                new CharacterCharacteristicDetailed(contextModif: (short)Stats[PlayerFields.AP].Context, @base: (short)Stats[PlayerFields.AP].Base, alignGiftBonus: 0, characteristicId: (short)CharacteristicEnum.ACTION_POINTS_AP, objectsAndMountBonus: (short)Stats[PlayerFields.AP].Equiped, (short)Stats[PlayerFields.AP].Additional),
                new CharacterCharacteristicDetailed(contextModif: (short)Stats.Strength.Context, @base: (short)Stats.Strength.Base, alignGiftBonus: 0, characteristicId: (short)CharacteristicEnum.STRENGTH, objectsAndMountBonus: (short)Stats.Strength.Equiped, additional: (short) Stats.Strength.Additional),
                new CharacterCharacteristicDetailed(contextModif: (short)Stats.Vitality.Context, @base: (short)Stats.Vitality.Base, alignGiftBonus: 0, characteristicId: (short)CharacteristicEnum.VITALITY, objectsAndMountBonus: (short)Stats.Vitality.Equiped, additional: (short)Stats.Vitality.Additional),
                new CharacterCharacteristicDetailed(contextModif: (short)Stats.Wisdom.Context, @base: (short)Stats.Wisdom.Base, alignGiftBonus: 0, characteristicId: (short)CharacteristicEnum.WISDOM, objectsAndMountBonus: (short)Stats.Wisdom.Equiped, additional: (short)Stats.Wisdom.Additional),
                new CharacterCharacteristicDetailed(contextModif: (short)Stats.Chance.Context, @base: (short)Stats.Chance.Base, alignGiftBonus: 0, characteristicId: (short)CharacteristicEnum.CHANCE, objectsAndMountBonus: (short)Stats.Chance.Equiped, additional: (short)Stats.Chance.Additional),
                new CharacterCharacteristicDetailed(contextModif: (short)Stats.Agility.Context, @base: (short)Stats.Agility.Base, alignGiftBonus: 0, characteristicId: (short)CharacteristicEnum.AGILITY, objectsAndMountBonus: (short)Stats.Agility.Equiped, additional: (short)Stats.Agility.Additional),
                new CharacterCharacteristicDetailed(contextModif: (short)Stats.Intelligence.Context, @base: (short)Stats.Intelligence.Base, alignGiftBonus: 0, characteristicId: (short)CharacteristicEnum.INTELLIGENCE, objectsAndMountBonus: (short)Stats.Intelligence.Equiped, additional: (short)Stats.Intelligence.Additional),
                new CharacterCharacteristicDetailed(contextModif: (short)Stats[PlayerFields.DamageBonus].Context, @base: (short)Stats[PlayerFields.DamageBonus].Base, alignGiftBonus: 0, characteristicId: (short)CharacteristicEnum.DAMAGE, objectsAndMountBonus: (short)Stats[PlayerFields.DamageBonus].Equiped, additional: (short)Stats[PlayerFields.DamageBonus].Additional),
                new CharacterCharacteristicDetailed(contextModif: (short)Stats[PlayerFields.CriticalHit].Context, @base: (short)Stats[PlayerFields.CriticalHit].Base, alignGiftBonus: 0, characteristicId: (short)CharacteristicEnum.CRITICAL, objectsAndMountBonus: (short)Stats[PlayerFields.CriticalHit].Equiped, additional: (short)Stats[PlayerFields.CriticalHit].Additional),
                new CharacterCharacteristicDetailed(contextModif: (short)Stats[PlayerFields.Range].Context, @base: (short)Stats[PlayerFields.Range].Base, alignGiftBonus: 0, characteristicId: (short)CharacteristicEnum.RANGE, objectsAndMountBonus: (short)Stats[PlayerFields.Range].Equiped, additional: (short)Stats[PlayerFields.Range].Additional),
                new CharacterCharacteristicDetailed(contextModif: (short)Stats[PlayerFields.MP].Context, @base: (short)Stats[PlayerFields.MP].Base, alignGiftBonus: 0, characteristicId: (short)CharacteristicEnum.MOVEMENT_POINTS_MP, objectsAndMountBonus: (short)Stats[PlayerFields.MP].Equiped, additional: (short)Stats[PlayerFields.MP].Additional),
                new CharacterCharacteristicDetailed(contextModif: (short)Stats[PlayerFields.DamageBonusPercent].Context, @base: (short)Stats[PlayerFields.DamageBonusPercent].Base, alignGiftBonus: 0, characteristicId: (short)CharacteristicEnum.POWER, objectsAndMountBonus: (short)Stats[PlayerFields.DamageBonusPercent].Equiped, additional: (short)Stats[PlayerFields.DamageBonusPercent].Additional),
                new CharacterCharacteristicDetailed(contextModif: (short)Stats[PlayerFields.SummonLimit].Context, @base: (short)Stats[PlayerFields.SummonLimit].Base, alignGiftBonus: 0, characteristicId: (short)CharacteristicEnum.SUMMONS, objectsAndMountBonus: (short)Stats[PlayerFields.SummonLimit].Equiped, additional: (short)Stats[PlayerFields.SummonLimit].Additional),
                new CharacterCharacteristicDetailed(contextModif: (short)Stats[PlayerFields.DodgeAPProbability].Context, @base: (short)Stats[PlayerFields.DodgeAPProbability].Base, alignGiftBonus: 0, characteristicId: (short)CharacteristicEnum.AP_LOSS_RES, objectsAndMountBonus : (short)Stats[PlayerFields.DodgeAPProbability].Equiped, additional: (short)Stats[PlayerFields.DodgeAPProbability].Additional),
                new CharacterCharacteristicDetailed(contextModif: (short)Stats[PlayerFields.DodgeMPProbability].Context, @base: (short)Stats[PlayerFields.DodgeMPProbability].Base, alignGiftBonus: 0, characteristicId: (short)CharacteristicEnum.MP_LOSS_RES, objectsAndMountBonus: (short)Stats[PlayerFields.DodgeMPProbability].Equiped, additional: (short)Stats[PlayerFields.DodgeMPProbability].Additional),
                new CharacterCharacteristicDetailed(contextModif: (short)Stats[PlayerFields.EarthResistPercent].Context, @base: (short)Stats[PlayerFields.EarthResistPercent].Base, alignGiftBonus: 0, characteristicId: (short)CharacteristicEnum.EARTH_REDUCTION_PERCENT, objectsAndMountBonus: (short)Stats[PlayerFields.EarthResistPercent].Equiped, additional: (short)Stats[PlayerFields.EarthResistPercent].Additional),
                new CharacterCharacteristicDetailed(contextModif: (short)Stats[PlayerFields.FireResistPercent].Context, @base: (short)Stats[PlayerFields.FireResistPercent].Base, alignGiftBonus: 0, characteristicId: (short)CharacteristicEnum.FIRE_REDUCTION_PERCENT, objectsAndMountBonus: (short)Stats[PlayerFields.FireResistPercent].Equiped, additional: (short)Stats[PlayerFields.FireResistPercent].Additional),
                new CharacterCharacteristicDetailed(contextModif: (short)Stats[PlayerFields.WaterResistPercent].Context, @base: (short)Stats[PlayerFields.WaterResistPercent].Base, alignGiftBonus: 0, characteristicId: (short)CharacteristicEnum.WATER_REDUCTION_PERCENT, objectsAndMountBonus: (short)Stats[PlayerFields.WaterResistPercent].Equiped, additional: (short)Stats[PlayerFields.WaterResistPercent].Additional),
                new CharacterCharacteristicDetailed(contextModif: (short)Stats[PlayerFields.AirResistPercent].Context, @base: (short)Stats[PlayerFields.AirResistPercent].Base, alignGiftBonus: 0, characteristicId: (short)CharacteristicEnum.AIR_REDUCTION_PERCENT, objectsAndMountBonus: (short)Stats[PlayerFields.AirResistPercent].Equiped, additional: (short)Stats[PlayerFields.AirResistPercent].Additional),
                new CharacterCharacteristicDetailed(contextModif: (short)Stats[PlayerFields.NeutralResistPercent].Context, @base: (short)Stats[PlayerFields.NeutralResistPercent].Base, alignGiftBonus: 0, characteristicId: (short)CharacteristicEnum.NEUTRAL_REDUCTION_PERCENT, objectsAndMountBonus: (short)Stats[PlayerFields.NeutralResistPercent].Equiped, additional: (short)Stats[PlayerFields.NeutralResistPercent].Additional),
                new CharacterCharacteristicDetailed(contextModif: 0, @base: 0, alignGiftBonus: 0, characteristicId: (short)CharacteristicEnum.CRITICAL_FAILURE, objectsAndMountBonus: 0, additional: 0),
                new CharacterCharacteristicDetailed(contextModif: (short)Stats.Initiative.Context, @base: (short)Stats.Initiative.Base, alignGiftBonus: 0, characteristicId: (short)CharacteristicEnum.INITIATIVE, objectsAndMountBonus: (short)Stats.Initiative.Equiped, additional: (short)Stats.Initiative.Additional),
                new CharacterCharacteristicDetailed(contextModif: (short)Stats[PlayerFields.Prospecting].Context, @base: (short)Stats[PlayerFields.Prospecting].Base, alignGiftBonus: 0, characteristicId: (short)CharacteristicEnum.PROSPECTING, objectsAndMountBonus: (short)Stats[PlayerFields.Prospecting].Equiped, additional: (short)Stats[PlayerFields.Prospecting].Additional),
                new CharacterCharacteristicDetailed(contextModif: (short)Stats[PlayerFields.HealBonus].Context, @base: (short)Stats[PlayerFields.HealBonus].Base, alignGiftBonus: 0, characteristicId: (short)CharacteristicEnum.HEALS, objectsAndMountBonus: (short)Stats[PlayerFields.HealBonus].Equiped, additional: (short)Stats[PlayerFields.HealBonus].Additional),
                new CharacterCharacteristicDetailed(contextModif: (short)Stats[PlayerFields.DamageReflection].Context, @base: (short)Stats[PlayerFields.DamageReflection].Base, alignGiftBonus: 0, characteristicId: (short)CharacteristicEnum.REFLECT, objectsAndMountBonus: (short)Stats[PlayerFields.DamageReflection].Equiped, additional: (short)Stats[PlayerFields.DamageReflection].Additional),
                new CharacterCharacteristicDetailed(contextModif: (short)Stats[PlayerFields.EarthElementReduction].Context, @base: (short)Stats[PlayerFields.EarthElementReduction].Base, alignGiftBonus: 0, characteristicId: (short)CharacteristicEnum.EARTH_REDUCTION_FIXED, objectsAndMountBonus: (short)Stats[PlayerFields.EarthElementReduction].Equiped, additional: (short)Stats[PlayerFields.EarthElementReduction].Additional),
                new CharacterCharacteristicDetailed(contextModif: (short)Stats[PlayerFields.FireElementReduction].Context, @base: (short)Stats[PlayerFields.FireElementReduction].Base, alignGiftBonus: 0, characteristicId: (short)CharacteristicEnum.FIRE_REDUCTION_FIXED, objectsAndMountBonus: (short)Stats[PlayerFields.FireElementReduction].Equiped, additional: (short)Stats[PlayerFields.FireElementReduction].Additional),
                new CharacterCharacteristicDetailed(contextModif: (short)Stats[PlayerFields.WaterElementReduction].Context, @base: (short)Stats[PlayerFields.WaterElementReduction].Base, alignGiftBonus: 0, characteristicId: (short)CharacteristicEnum.WATER_REDUCTION_FIXED, objectsAndMountBonus: (short)Stats[PlayerFields.WaterElementReduction].Equiped, additional: (short)Stats[PlayerFields.WaterElementReduction].Additional),
                new CharacterCharacteristicDetailed(contextModif: (short)Stats[PlayerFields.AirElementReduction].Context, @base: (short)Stats[PlayerFields.AirElementReduction].Base, alignGiftBonus: 0, characteristicId: (short)CharacteristicEnum.AIR_REDUCTION_FIXED, objectsAndMountBonus: (short)Stats[PlayerFields.AirElementReduction].Equiped, additional: (short)Stats[PlayerFields.AirElementReduction].Additional),
                new CharacterCharacteristicDetailed(contextModif: (short)Stats[PlayerFields.NeutralElementReduction].Context, @base: (short)Stats[PlayerFields.NeutralElementReduction].Base, alignGiftBonus: 0, characteristicId: (short)CharacteristicEnum.NEUTRAL_REDUCTION_FIXED, objectsAndMountBonus: (short)Stats[PlayerFields.NeutralElementReduction].Equiped, additional: (short)Stats[PlayerFields.NeutralElementReduction].Additional),
                new CharacterCharacteristicDetailed(contextModif: (short)Stats[PlayerFields.TrapBonusPercent].Context, @base: (short)Stats[PlayerFields.TrapBonusPercent].Base, alignGiftBonus: 0, characteristicId: (short)CharacteristicEnum.TRAPS_BONUS_PERCENT, objectsAndMountBonus: (short)Stats[PlayerFields.TrapBonusPercent].Equiped, additional: (short)Stats[PlayerFields.TrapBonusPercent].Additional),
                new CharacterCharacteristicDetailed(contextModif: (short)Stats[PlayerFields.TrapBonus].Context, @base: (short)Stats[PlayerFields.TrapBonus].Base, alignGiftBonus: 0, characteristicId: (short)CharacteristicEnum.TRAPS_BONUS_FIXED, objectsAndMountBonus: (short)Stats[PlayerFields.TrapBonus].Equiped, additional: (short)Stats[PlayerFields.TrapBonus].Additional),
                new CharacterCharacteristicDetailed(contextModif: (short)Stats[PlayerFields.PermanentDamagePercent].Context, @base: (short)Stats[PlayerFields.PermanentDamagePercent].Base, alignGiftBonus: 0, characteristicId: (short)CharacteristicEnum.EROSION, objectsAndMountBonus: (short)Stats[PlayerFields.PermanentDamagePercent].Equiped, additional: 0),
                new CharacterCharacteristicDetailed(contextModif: (short)Stats[PlayerFields.TackleEvade].Context, @base: (short)Stats[PlayerFields.TackleEvade].Base, alignGiftBonus: 0, characteristicId: (short)CharacteristicEnum.DODGE, objectsAndMountBonus: (short)Stats[PlayerFields.TackleEvade].Equiped, additional: 0),
                new CharacterCharacteristicDetailed(contextModif: (short)Stats[PlayerFields.TackleBlock].Context, @base: (short)Stats[PlayerFields.TackleBlock].Base, alignGiftBonus: 0, characteristicId: (short)CharacteristicEnum.LOCK, objectsAndMountBonus: (short)Stats[PlayerFields.TackleBlock].Equiped, additional: 0),
                new CharacterCharacteristicDetailed(contextModif: (short)Stats[PlayerFields.APAttack].Context, @base: (short)Stats[PlayerFields.APAttack].Base, alignGiftBonus: 0, characteristicId: (short)CharacteristicEnum.AP_REDUCTION, objectsAndMountBonus: (short)Stats[PlayerFields.APAttack].Equiped, additional: 0),
                new CharacterCharacteristicDetailed(contextModif: (short)Stats[PlayerFields.MPAttack].Context, @base: (short)Stats[PlayerFields.MPAttack].Base, alignGiftBonus: 0, characteristicId: (short)CharacteristicEnum.MP_REDUCTION, objectsAndMountBonus: (short)Stats[PlayerFields.MPAttack].Equiped, additional: 0),
                new CharacterCharacteristicDetailed(contextModif: (short)Stats[PlayerFields.PushDamageBonus].Context, @base: (short)Stats[PlayerFields.PushDamageBonus].Base, alignGiftBonus: 0, characteristicId: (short)CharacteristicEnum.PUSHBACK_REDUCTION, objectsAndMountBonus: (short)Stats[PlayerFields.PushDamageBonus].Equiped, additional: 0),
                new CharacterCharacteristicDetailed(contextModif: (short)Stats[PlayerFields.PushDamageReduction].Context, @base: (short)Stats[PlayerFields.PushDamageReduction].Base, alignGiftBonus: 0, characteristicId: (short)CharacteristicEnum.PUSHBACK_DAMAGE_FIXED, objectsAndMountBonus: (short)Stats[PlayerFields.PushDamageReduction].Equiped, additional: 0),
                new CharacterCharacteristicDetailed(contextModif: (short)Stats[PlayerFields.CriticalDamageBonus].Context, @base: (short)Stats[PlayerFields.CriticalDamageBonus].Base, alignGiftBonus: 0, characteristicId: (short)CharacteristicEnum.CRITICAL_DAMAGE, objectsAndMountBonus: (short)Stats[PlayerFields.CriticalDamageBonus].Equiped, additional: 0),
                new CharacterCharacteristicDetailed(contextModif: (short)Stats[PlayerFields.CriticalDamageReduction].Context, @base: (short)Stats[PlayerFields.CriticalDamageReduction].Base, alignGiftBonus: 0, characteristicId: (short)CharacteristicEnum.CRITICAL_REDUCTION_FIXED, objectsAndMountBonus: (short)Stats[PlayerFields.CriticalDamageReduction].Equiped, additional: 0),
                new CharacterCharacteristicDetailed(contextModif: (short)Stats[PlayerFields.EarthDamageBonus].Context, @base: (short)Stats[PlayerFields.EarthDamageBonus].Base, alignGiftBonus: 0, characteristicId: (short)CharacteristicEnum.EARTH_DAMAGE_FIXED, objectsAndMountBonus: (short)Stats[PlayerFields.EarthDamageBonus].Equiped, additional: 0),
                new CharacterCharacteristicDetailed(contextModif: (short)Stats[PlayerFields.FireDamageBonus].Context, @base: (short)Stats[PlayerFields.FireDamageBonus].Base, alignGiftBonus: 0, characteristicId: (short)CharacteristicEnum.FIRE_DAMAGE_FIXED, objectsAndMountBonus: (short)Stats[PlayerFields.FireDamageBonus].Equiped, additional: 0),
                new CharacterCharacteristicDetailed(contextModif: (short)Stats[PlayerFields.WaterDamageBonus].Context, @base: (short)Stats[PlayerFields.WaterDamageBonus].Base, alignGiftBonus: 0, characteristicId: (short)CharacteristicEnum.WATER_DAMAGE_FIXED, objectsAndMountBonus: (short)Stats[PlayerFields.WaterDamageBonus].Equiped, additional: 0),
                new CharacterCharacteristicDetailed(contextModif: (short)Stats[PlayerFields.AirDamageBonus].Context, @base: (short)Stats[PlayerFields.AirDamageBonus].Base, alignGiftBonus: 0, characteristicId: (short)CharacteristicEnum.AIR_DAMAGE_FIXED, objectsAndMountBonus: (short)Stats[PlayerFields.AirDamageBonus].Equiped, additional: 0),
                new CharacterCharacteristicDetailed(contextModif: (short)Stats[PlayerFields.NeutralDamageBonus].Context, @base: (short)Stats[PlayerFields.NeutralDamageBonus].Base, alignGiftBonus: 0, characteristicId: (short)CharacteristicEnum.NEUTRAL_DAMAGE_FIXED, objectsAndMountBonus: (short)Stats[PlayerFields.NeutralDamageBonus].Equiped, additional: 0),
                new CharacterCharacteristicDetailed(contextModif: 0, @base: (short)Stats.Health.TotalMax, alignGiftBonus: 0, characteristicId: (short)CharacteristicEnum.VITALITY_BONUS_DOES_NOT_REMOVE_HP_WHEN_LIFTED, objectsAndMountBonus: 0, additional: 0),
                new CharacterCharacteristicDetailed(contextModif: 0, @base: (short)(Stats.Health.DamageTaken * -1), alignGiftBonus: 0, characteristicId: (short)CharacteristicEnum.TEMPORARY_HEALTH_POINT_PENALTY, objectsAndMountBonus: 0, additional: 0),
                new CharacterCharacteristicDetailed(0, 0, 0, (short)CharacteristicEnum.DAMAGE_RESISTANCE_PERCENTAGE, 0, 0),
                new CharacterCharacteristicDetailed(0, 0, 0, (short)CharacteristicEnum.POWER_BONUS_FOR_GLYPHS, 0, 0),
                new CharacterCharacteristicDetailed(0, 100, 0,(short)CharacteristicEnum.DAMAGE_MULTIPLIER, 0, 0),
                new CharacterCharacteristicDetailed(0, 0, 0, (short)CharacteristicEnum.POWER_BONUS_FOR_RUNES, 0, 0),
                new CharacterCharacteristicDetailed(contextModif: 0, @base: 100, alignGiftBonus: 0, characteristicId: (short)CharacteristicEnum.DISTANCE_120, objectsAndMountBonus: (short)Stats[PlayerFields.RangedDamageDonePercent].Equiped, additional: 0),
                new CharacterCharacteristicDetailed(contextModif: 0, @base: 100, alignGiftBonus: 0, characteristicId: (short)CharacteristicEnum.DISTANCE_121, objectsAndMountBonus: (short)Stats[PlayerFields.RangedDamageReceivedPercent].Equiped, additional: 0),
                new CharacterCharacteristicDetailed(contextModif: 0, @base: 100, alignGiftBonus: 0, characteristicId: (short)CharacteristicEnum.WEAPON_122, objectsAndMountBonus: (short)Stats[PlayerFields.WeaponDamageDonePercent].Equiped, additional: 0),
                new CharacterCharacteristicDetailed(contextModif: 0, @base: 100, alignGiftBonus: 0, characteristicId: (short)CharacteristicEnum.SPELLS_123, objectsAndMountBonus: (short)Stats[PlayerFields.SpellDamageDonePercent].Equiped, additional: 0),
                new CharacterCharacteristicDetailed(contextModif: 0, @base: 100, alignGiftBonus: 0, characteristicId: (short)CharacteristicEnum.MELEE_124, objectsAndMountBonus: (short)Stats[PlayerFields.MeleeDamageReceivedPercent].Equiped, additional: 0),
                new CharacterCharacteristicDetailed(contextModif: 0, @base: 100, alignGiftBonus: 0, characteristicId: (short)CharacteristicEnum.MELEE_125, objectsAndMountBonus: (short)Stats[PlayerFields.MeleeDamageDonePercent].Equiped, additional: 0),
                new CharacterCharacteristicDetailed(contextModif: 0, @base: 100, alignGiftBonus: 0, characteristicId: (short)CharacteristicEnum.SPELLS_141, objectsAndMountBonus: (short)Stats[PlayerFields.SpellDamageReceivedPercent].Equiped, additional: 0),
                new CharacterCharacteristicDetailed(contextModif: 0, @base: 100, alignGiftBonus: 0, characteristicId: (short)CharacteristicEnum.WEAPON_142, objectsAndMountBonus: (short)Stats[PlayerFields.WeaponDamageReceivedPercent].Equiped, additional: 0),
                new CharacterCharacteristicValue(total: Stats.Shield.TotalSafe, characteristicId: (short)CharacteristicEnum.SHIELD),
                new CharacterCharacteristicValue(total: Stats.AP.Total, characteristicId: (short)CharacteristicEnum.MAX_ACTION_POINTS_AP),
                new CharacterCharacteristicValue(total: Stats.MP.Total, characteristicId: (short)CharacteristicEnum.MAX_MOVEMENT_POINTS_MP),
                new CharacterCharacteristicValue(total: Honor, characteristicId: (short)CharacteristicEnum.HONOUR_POINTS),
                new CharacterCharacteristicValue(total: Dishonor, characteristicId: (short)CharacteristicEnum.DISGRACE_POINTS),
                new CharacterCharacteristicValue(total: Energy, characteristicId: (short)CharacteristicEnum.ENERGY_POINTS),
                new CharacterCharacteristicValue(total: EnergyMax, characteristicId: (short)CharacteristicEnum.MAX_ENERGY_POINTS),
                new CharacterCharacteristicDetailed(contextModif: 0, @base: (short)StatsPoints, alignGiftBonus: 0, characteristicId: (short)CharacteristicEnum.CHARACTERISTIC_POINTS, objectsAndMountBonus: 0, additional: 0),
            };

            return characteristic;
        }

        public CharacterCharacteristicsInformations GetCharacterCharacteristicsInformations()
        {
            return new CharacterCharacteristicsInformations(
                experience: (ulong)Experience,
                experienceLevelFloor: (ulong)LowerBoundExperience,
                experienceNextLevelFloor: (ulong)UpperBoundExperience,
                experienceBonusLimit: (ulong)UpperBoundExperience,
                kamas: (ulong)this.Inventory.Kamas,
                alignmentInfos: GetActorAlignmentExtendInformations(),
                criticalHitWeapon: (ushort)Inventory.WeaponCriticalHit,
                characteristics: GetCharacterCharacteristic(),
                spellModifiers: new List<SpellModifierMessage>(), // TODO - 2.71
                probationTime: 0);
        }

        #endregion CharacterBaseInformations

        #region PartyMemberInformations

        public PartyInvitationMemberInformations GetPartyInvitationMemberInformations()
        {
            return new PartyInvitationMemberInformations(
                id: (ulong)Id,
                name: Name,
                level: Level,
                entityLook: Look.GetEntityLook(),
                breed: (sbyte)BreedId,
                sex: Sex == SexTypeEnum.SEX_FEMALE,
                prestige: 0,
                worldX: (short)Map.Position.X,
                worldY: (short)Map.Position.Y,
                mapId: Map.Id,
                subAreaId: (ushort)Map.SubArea.Id,
                entities: Companion == null ? new PartyEntityMemberInformation[0] : new PartyEntityMemberInformation[] { Companion.GetPartyCompanionMemberInformations() });
        }

        public PartyMemberInformations GetPartyMemberInformations()
        {
            return new PartyMemberInformations(
                id: (ulong)Id,
                name: Name,
                level: Level,
                entityLook: Look.GetEntityLook(),
                breed: (sbyte)BreedId,
                sex: Sex == SexTypeEnum.SEX_FEMALE,
                prestige: 0,
                lifePoints: (uint)LifePoints,
                maxLifePoints: (uint)MaxLifePoints,
                prospecting: (ushort)Stats[PlayerFields.Prospecting].Total,
                regenRate: RegenSpeed,
                initiative: (ushort)Stats[PlayerFields.Initiative].Total,
                alignmentSide: (sbyte)AlignmentSide,
                worldX: (short)Map.Position.X,
                worldY: (short)Map.Position.Y,
                mapId: Map.Id,
                subAreaId: (ushort)SubArea.Id,
                status: Status,
                entities: Companion == null ? new PartyEntityMemberInformation[0] : new PartyEntityMemberInformation[] { Companion.GetPartyCompanionMemberInformations() });
        }

        public PartyMemberArenaInformations GetPartyMemberArenaInformations()
        {
            return new PartyMemberArenaInformations(
                id: (ulong)Id,
                name: Name,
                level: (byte)Level,
                entityLook: Look.GetEntityLook(),
                breed: (sbyte)BreedId,
                sex: Sex == SexTypeEnum.SEX_FEMALE,
                prestige: 0,
                lifePoints: (uint)LifePoints,
                maxLifePoints: (uint)MaxLifePoints,
                prospecting: (ushort)Stats[PlayerFields.Prospecting].Total,
                regenRate: RegenSpeed,
                initiative: (ushort)Stats[PlayerFields.Initiative].Total,
                alignmentSide: (sbyte)AlignmentSide,
                worldX: (short)Map.Position.X,
                worldY: (short)Map.Position.Y,
                mapId: Map.Id,
                subAreaId: (ushort)SubArea.Id,
                status: Status,
                entities: Companion == null ? new PartyEntityMemberInformation[0] : new PartyEntityMemberInformation[] { Companion.GetPartyCompanionMemberInformations() },
                rank: (ushort)ArenaPointsRank_3vs3_Solo);
        }

        public PartyGuestInformations GetPartyGuestInformations(Party party)
        {
            if (!m_partyInvitations.ContainsKey(party.Id))
                return new PartyGuestInformations();

            var invitation = m_partyInvitations[party.Id];

            return new PartyGuestInformations(
                (ulong)Id,
                (ulong)invitation.Source.Id,
                Name,
                Look.GetEntityLook(),
                (sbyte)BreedId,
                Sex == SexTypeEnum.SEX_FEMALE,
                Status,
                Companion == null ? new PartyEntityMemberInformation[0] : new PartyEntityMemberInformation[] { Companion.GetPartyCompanionMemberInformations() });
        }

        #endregion PartyMemberInformations

        public override ActorRestrictionsInformations GetActorRestrictionsInformations()
        {
            return new ActorRestrictionsInformations(
                cantBeAggressed: !Map.AllowAggression || IsGhost(),
                cantBeChallenged: !Map.AllowChallenge || IsGhost(),
                cantTrade: !Map.AllowExchangesBetweenPlayers || IsGhost(),
                cantBeAttackedByMutant: IsGhost(),
                cantRun: false,
                forceSlowWalk: false,
                cantMinimize: false,
                cantMove: PlayerLifeStatus == PlayerLifeStatusEnum.STATUS_TOMBSTONE,
                cantAggress: !Map.AllowAggression || IsGhost(),
                cantChallenge: IsGhost(),
                cantExchange: IsGhost(),
                cantAttack: IsGhost(),
                cantChat: false,
                //cantBeMerchant: IsGhost(),
                cantUseObject: IsGhost(),
                cantUseTaxCollector: IsGhost(),
                cantUseInteractive: IsGhost(),
                cantSpeakToNPC: IsGhost(),
                cantChangeZone: false,
                cantAttackMonster: IsGhost());
        }

        public override HumanInformations GetHumanInformations()
        {
            var human = base.GetHumanInformations();
            var options = new List<HumanOption>();

            try
            {
                if (this.Inventory.GetItems(CharacterInventoryPositionEnum.INVENTORY_POSITION_FOLLOWER).Count() > 0)
                {
                    sbyte index = 0;
                    List<IndexedEntityLook> followers = new List<IndexedEntityLook>();

                    foreach (var item in this.Inventory.GetItems(CharacterInventoryPositionEnum.INVENTORY_POSITION_FOLLOWER))
                    {
                        if (item.Effects.Any(effect => effect.EffectId == EffectsEnum.Effect_RateXP || effect.EffectId == EffectsEnum.Effect_RateDrop || effect.EffectId == EffectsEnum.Effect_RateKamas))
                            continue;

                        if (index > sbyte.MaxValue)
                            break;

                        index++;
                        int type = (item.Effects.FirstOrDefault(x => x.EffectId == EffectsEnum.Effect_148) as EffectInteger).Value;
                        int followerId = (item.Template.Effects.FirstOrDefault(x => x.EffectId == EffectsEnum.Effect_148) as EffectDice).Value;
                        EntityLook monsterLook = MonsterManager.Instance.GetTemplate(followerId).EntityLook.GetEntityLook();

                        followers.Add(new IndexedEntityLook(monsterLook, index));
                    }

                    options.Add(new HumanOptionFollowers(followers));
                }

                if (Guild != null)
                {
                    options.Add(new HumanOptionGuild(Guild.GetGuildInformations()));

                    if (Guild.Alliance != null)
                    {
                        options.Add(new HumanOptionAlliance(Guild.Alliance.GetAllianceInformations(), (sbyte)GetAgressablestatus()));
                    }
                }

                if (SelectedTitle != null)
                    options.Add(new HumanOptionTitle(SelectedTitle.Value, string.Empty));

                if (SelectedOrnament != null)
                    options.Add(new HumanOptionOrnament(SelectedOrnament.Value, Level, LeaguesManager.Instance.GetLeagueCharacter(), 0));

                if (LastEmoteUsed != null)
                    options.Add(new HumanOptionEmote((byte)LastEmoteUsed.First.Id, LastEmoteUsed.Second.GetUnixTimeStampLong()));

                if (LastSkillUsed != null)
                    options.Add(new HumanOptionSkillUse((uint)LastSkillUsed.InteractiveObject.Id, (ushort)LastSkillUsed.SkillTemplate.Id, LastSkillUsed.SkillEndTime.GetUnixTimeStampLong()));

                if (itemsFollowsLook != null && itemsFollowsLook.Count > 0)
                {
                    options.Add(new HumanOptionFollowers(itemsFollowsLook));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ocorreu um erro ao obter as informações do personagem: " + ex.Message);
            }

            human.options = options.ToArray();
            return human;
        }
        #endregion Network

        public CharacterRecord Record => m_record;

        public Commands.Commands.Players.ZaapDialog CustomZaapDialog => Dialog as Commands.Commands.Players.ZaapDialog;
        public Misc.DonjonZaapDialog DonjonZaapDialog => Dialog as Misc.DonjonZaapDialog;
        public Misc.DopplesZaapDialog DopplesZaapDialog => Dialog as Misc.DopplesZaapDialog;
        public Misc.StartZaapDialog StartZaapDialog => Dialog as Misc.StartZaapDialog;

        public override bool CanBeSee(WorldObject byObj) => base.CanBeSee(byObj) && (byObj == this || !Invisible) && Game.HavenBags.HavenBagManager.Instance.CanBeSeenInHavenBag(byObj, this) && byObj is Character && (byObj == this || !Map.IsInstantiated);

        protected override void OnDisposed()
        {
            if (FriendsBook != null)
                FriendsBook.Dispose();

            if (Inventory != null)
                Inventory.Dispose();

            base.OnDisposed();
        }
        public WorldServerData GetWorldServerData()
        {
            //var characterWorld = Client.Account.Characters.First(x => x.CharacterId == Id);
            return WorldServer.ServerInformation;
        }

        public override string ToString() => string.Format("{0} ({1})", Name, Id);

        public bool IsInCustomZaapDialog() => Dialog is Commands.Commands.Players.ZaapDialog;
        public bool IsInDonjonZaapDialog() => Dialog is Misc.DonjonZaapDialog;
        public bool IsInDoplonDialog() => Dialog is Misc.DopplesZaapDialog;
        public bool IsInStartDialog() => Dialog is Misc.StartZaapDialog;

        // Função para obter o ID do título com base no papel
        private ushort GetTitleIdForRole(RoleEnum role)
        {
            Dictionary<RoleEnum, ushort> titleMap = new Dictionary<RoleEnum, ushort>()
            {
                { RoleEnum.Moderator_Helper, 503 },
                { RoleEnum.GameMaster_Padawan, 502 },
                { RoleEnum.GameMaster, 502 },
                { RoleEnum.Administrator, 501 },
                { RoleEnum.Developer, 500 }
            };

            if (titleMap.ContainsKey(role))
                return titleMap[role];

            return 0;
        }
    }
}