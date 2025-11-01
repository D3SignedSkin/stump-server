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
using Stump.Server.WorldServer.Database.Mounts;
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
using Stump.Core.Mathematics;
using Stump.Core.Reflection;
using Stump.Server.BaseServer;
using Stump.Server.WorldServer.Commands.Commands;
using Stump.Server.WorldServer.Database.Companion;
using Stump.Server.WorldServer.Game.Achievements;
using Stump.Server.WorldServer.Game.Alliances;
using Stump.Server.WorldServer.Game.Companions;
using Stump.Server.WorldServer.Game.Dopeul;
using Stump.Server.WorldServer.Game.HavenBags;
using Stump.Server.WorldServer.Game.Idols;
using Stump.Server.WorldServer.Game.Items;
using Stump.Server.WorldServer.Game.Prisms;
using Stump.Server.WorldServer.Handlers.Alliances;
using Stump.Server.WorldServer.Handlers.Prism;
using Stump.Server.WorldServer.Handlers.PvP;

namespace Stump.Server.WorldServer.Game.Actors.RolePlay.Characters {
    public sealed class Character : Humanoid, IStatsOwner, IInventoryOwner, ICommandsUser {
        [Variable]
        public static ushort HonorLimit = 20000;
        public bool battleFieldOn = false;
        public bool followforspouse = false;
        public TimeSpan last;
        readonly CharacterRecord m_record;
        bool m_recordLoaded;
        public DopeulCollection DopeulCollection { get; private set; }
        public event Action<Character> KingOfHill;
        public event Action<Character> KoHRevive;
        public List<CharacterSpellModification> SpellsModifications = new List<CharacterSpellModification> ();
        public string[] test = new string[10];
        public string temptest {

            set {
                test[9] = test[8];
                test[8] = test[7];
                test[7] = test[6];
                test[6] = test[5];
                test[5] = test[4];
                test[4] = test[3];
                test[3] = test[2];
                test[2] = test[1];
                test[1] = test[0];
                test[0] = value;

            }
        }
        [Variable(true)]
        public static int[] SpellsBlock =
       {
            //9631,154,9632,151
        };
        DateTime loginTime;

        public Character (CharacterRecord record, WorldClient client) {
            m_record = record;
            Client = client;
            SaveSync = new object ();
            LoggoutSync = new object ();
            Status = new PlayerStatus ((sbyte) PlayerStatusEnum.PLAYER_STATUS_AVAILABLE);

            //no se cual llame al pc que llama al metodo
            //se ebe crear un metodo que llene la variable Dopeul con los datos necesarios
        }

        #region Events

        public event Action<Character> LoggedIn;

        void OnLoggedIn () {
            // try
            //{
            if (CurrentSpouse != 0) {
                var spouse = World.Instance.GetCharacter (CurrentSpouse);
                try {
                    SpouseHandler.SendSpouseInformationMessage (Client, spouse);
                    switch (spouse.Account.Lang) {
                        case "fr":
                            spouse.DisplayNotification ("Votre compagnon est en ligne!");
                            break;
                        case "es":
                            spouse.DisplayNotification ("�Su compa�ero est� conectado!");
                            break;
                        case "en":
                            spouse.DisplayNotification ("Your mate is online!");
                            break;
                        default:
                            spouse.DisplayNotification ("Seu companheiro est� conectado!");
                            break;
                    }
                    SpouseHandler.SendSpouseInformationMessage (spouse.Client, Client.Character);
                } catch {
                    if (CurrentSpouse != 0)
                        switch (Account.Lang) {
                            case "fr":
                                DisplayNotification ("Votre compagnon est hors ligne, vous serez averti quand il se connecte.");
                                break;
                            case "es":
                                DisplayNotification ("Su compa�ero est� desconectado, se le avisar� cuando se conecte.");
                                break;
                            case "en":
                                DisplayNotification ("Your mate is offline, you will be warned when he connects.");
                                break;
                            default:
                                DisplayNotification ("Seu companheiro est� desconectado, voc� ser� avisado quando ele conectar-se.", NotificationEnum.ERREUR);
                                break;
                        }
                }
            }
            if (GuildMember != null) {
                GuildMember.OnCharacterConnected (this);

                if (Guild.MotdContent != null)
                    GuildHandler.SendGuildMotdMessage (Client, Guild);

                if (Guild.BulletinContent != null)
                    GuildHandler.SendGuildBulletinMessage (Client, Guild);
                if (Guild.Alliance != null) {

                    if (Guild.Alliance.MotdContent != null)
                        AllianceHandler.SendAllianceMotdMessage (Client, Guild.Alliance);
                    if (Guild.Alliance.BulletinContent != null)
                        AllianceHandler.SendAllianceBulletinMessage (Client, Guild.Alliance);

                }
                AddEmote (EmotesEnum.EMOTE_GUILD);
                if (Guild.Level > 200)
                    AddEmote ((EmotesEnum) 155); //guildwineer

            } else {
                RemoveEmote (EmotesEnum.EMOTE_GUILD);
                if (HasEmote ((EmotesEnum) 155))
                    RemoveEmote ((EmotesEnum) 155); //guildwineer
            }
            if (GuildMember != null) {
                if (Guild.Alliance != null) { AddEmote (EmotesEnum.EMOTE_ALLIANCE); } else
                    RemoveEmote (EmotesEnum.EMOTE_ALLIANCE);
            }
            //Arena
            CheckArenaDailyProperties_1vs1 ();
            CheckArenaDailyProperties_3vs3 ();

            if (PrestigeRank > 0 && PrestigeManager.Instance.PrestigeEnabled) {
                var item = GetPrestigeItem ();
                if (item == null)
                    CreatePrestigeItem ();
                else {
                    item.UpdateEffects ();
                    Inventory.RefreshItem (item);
                }
                RefreshStats ();
            } else {
                var item = GetPrestigeItem ();
                if (item != null)
                    Inventory.RemoveItem (item, true);
            }

            OnPlayerLifeStatusChanged (PlayerLifeStatus);

            if (!IsGhost ()) {
                var energyGain = (short) (DateTime.Now - Record.LastUsage.Value).TotalMinutes;

                energyGain = (short) ((Energy + energyGain) > EnergyMax ? (EnergyMax - Energy) : energyGain);

                if (energyGain <= 0) { } else {

                    Energy += energyGain;

                    RefreshStats ();

                    //Vous avez r�cup�r� <b>%1</b> points d'�nergie.
                    SendInformationMessage (TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 7, energyGain);
                }
            }

            Record.LastUsage = DateTime.Now;

            var document = new BsonDocument { { "AcctId", Account.Id }, { "AcctName", Account.Login }, { "CharacterId", Id }, { "CharacterName", Name }, { "IPAddress", Client.IP }, { "Action", "Login" }, { "Date", DateTime.Now.ToString (CultureInfo.InvariantCulture) }
            };

            MongoLogger.Instance.Insert ("characters_connections", document);

            LoggedIn?.Invoke (this);
            //}
            //catch { }

        }

        public event Action<Character> LoggedOut;

        void OnLoggedOut () {
            try {
                EnterMap -= OnFollowedMemberEnterMap;
                EnterMap -= UpdateFollowSpouse;

                if (FriendsBook != null)
                    FriendsBook.CheckDC (); // attempt to resolve leaks

                if (Fight != null && (Fight.State == FightState.Placement || Fight.State == FightState.Fighting))
                    Record.LeftFightId = Fight.Id;
                else
                    Record.LeftFightId = null;

                if (GuildMember != null)
                    GuildMember.OnCharacterDisconnected (this);

                if (TaxCollectorDefendFight != null)
                    TaxCollectorDefendFight.RemoveDefender (this);

                if (ArenaManager.Instance.IsInQueue (this))
                    ArenaManager.Instance.RemoveFromQueue (this);

                if (ArenaPopup != null)
                    ArenaPopup.Deny ();

                if (Jobs != null)
                    foreach (var job in Jobs.Where (x => x.IsIndexed))
                        job.Template.RemoveAvaiableCrafter (this);

                var document = new BsonDocument { { "AcctId", Client.Account.Id }, { "AcctName", Client.Account.Login }, { "CharacterId", Id }, { "CharacterName", Name }, { "IPAddress", Client.IP }, { "Action", "Loggout" }, { "Date", DateTime.Now.ToString (CultureInfo.InvariantCulture) }
                };

                MongoLogger.Instance.Insert ("characters_connections", document);

                Record.LastUsage = DateTime.Now;

                LoggedOut?.Invoke (this);
            } catch { }
        }

        public event Action<Character> Saved;

        public void OnSaved () {
            IsAuthSynced = true;
            UnBlockAccount ();

            Saved?.Invoke (this);
        }

        public event Action<Character, int> LifeRegened;

        private void OnLifeRegened (int regenedLife) {
            LifeRegened?.Invoke (this, regenedLife);
        }

        public event Action<Character> AccountUnblocked;

        private void OnAccountUnblocked () {
            AccountUnblocked?.Invoke (this);
        }

        public event Action<Character> LookRefreshed;

        private void OnLookRefreshed () {
            LookRefreshed?.Invoke (this);
        }

        public event Action<Character> StatsResfreshed;

        private void OnStatsResfreshed () {
            StatsResfreshed?.Invoke (this);
        }

        public event Action<Character, Npc, NpcActionTypeEnum, NpcAction> InteractingWith;

        public void OnInteractingWith (Npc npc, NpcActionTypeEnum actionType, NpcAction action) {
            InteractingWith?.Invoke (this, npc, actionType, action);
        }
        #endregion Events

        #region Properties

        public WorldClient Client {
            get;
        }

        public string CharacterToSeekName {
            get { return Record.CharacterToSeekName; }
            set { Record.CharacterToSeekName = value; }
        }

        public AccountData Account {
            get { return Client.Account; }
        }

        public WorldAccount WorldAccount {
            get { return Client.WorldAccount; }
        }

        public UserGroup UserGroup {
            get {
                return Client.UserGroup;
            }
        }

        public object SaveSync {
            get;
            private set;
        }

        public object LoggoutSync {
            get;
            private set;
        }

        private bool m_inWorld;

        public override bool IsInWorld {
            get {
                return m_inWorld;
            }
        }

        public CharacterMerchantBag MerchantBag {
            get;
            private set;
        }
        #region Battlefield
        public Map MapBattleField {
            get;
            set;
        }

        public Cell CellBattleField {
            get;
            set;
        }
        public void updateBattleFieldPosition () {
            this.MapBattleField = this.Map;
            this.CellBattleField = this.Cell;
        }
        #endregion

        #region Identifier

        public override string Name {
            get {
                return (RoleEnum) Account.UserGroupId >= RoleEnum.GameMaster_Padawan ?
                    ((RoleEnum) Account.UserGroupId == RoleEnum.Non_ADM ? (Vip ? (m_record.NameVip != "" ? "<font color='" + m_record.NameVip + "'><b>" + m_record.Name + "</b></font>" : m_record.Name) : m_record.Name) : (Vip ? $"[{(m_record.NameVip != "" ? "<font color='" + m_record.NameVip + "'><b>" + m_record.Name + "</b></font>" : m_record.Name)}]" : $"[{m_record.Name}]")) :
                    (Vip ? (m_record.NameVip != "" ? "<font color='" + m_record.NameVip + "'><b>" + m_record.Name + "</b></font>" : m_record.Name) : m_record.Name);
            }
            protected set {
                m_record.Name = value;
                base.Name = value;
            }
        }
        public string Namedefault {
            get {
                return (RoleEnum) Account.UserGroupId >= RoleEnum.GameMaster_Padawan ?
                    ((RoleEnum) Account.UserGroupId == RoleEnum.Non_ADM ? m_record.Name : $"[{m_record.Name}]") :
                    m_record.Name;
            }
        }
        public override int Id {
            get { return m_record.Id; }
            protected set {
                m_record.Id = value;
                base.Id = value;
            }
        }

        #endregion Identifier

        #region Achievement
        public PlayerAchievement Achievement { get; private set; }

        #endregion

        #region Inventory

        public Inventory Inventory {
            get;
            private set;
        }

        public int Kamas {
            get { return Record.Kamas; }
            set { Record.Kamas = value; }
        }

        #endregion Inventory

        #region Jobs

        public JobsCollection Jobs {
            get;
            private set;
        }

        #endregion Jobs

        #region Interactives

        public InteractiveObject CurrentUsedInteractive => CurrentUsedSkill?.InteractiveObject;

        public Skill CurrentUsedSkill {
            get;
            private set;
        }

        public void SetCurrentSkill (Skill skill) {
            CurrentUsedSkill = skill;
        }

        public void ResetCurrentSkill () {
            CurrentUsedSkill = null;
        }

        #endregion

        #region Position

        public override ICharacterContainer CharacterContainer {
            get {
                if (IsFighting ())
                    return Fight;

                return Map;
            }
        }

        #endregion Position

        #region Dialog

        private IDialoger m_dialoger;

        public IDialoger Dialoger {
            get { return m_dialoger; }
            private set {
                m_dialoger = value;
                m_dialog = value != null ? m_dialoger.Dialog : null;
            }
        }

        private IDialog m_dialog;

        public IDialog Dialog {
            get { return m_dialog; }
            private set {
                m_dialog = value;
                if (m_dialog == null)
                    m_dialoger = null;
            }
        }

        public NpcShopDialogLogger NpcShopDialog => Dialog as NpcShopDialogLogger;

        public ZaapDialog ZaapDialog => Dialog as ZaapDialog;

        public ZaapiDialog ZaapiDialog => Dialog as ZaapiDialog;

        public MerchantShopDialog MerchantShopDialog => Dialog as MerchantShopDialog;

        public RequestBox RequestBox {
            get;
            private set;
        }

        public void SetDialoger (IDialoger dialoger) {
            if (Dialog != null)
                Dialog.Close ();

            Dialoger = dialoger;
        }

        public void SetDialog (IDialog dialog) {
            if (Dialog != null)
                Dialog.Close ();

            Dialog = dialog;
        }

        public void CloseDialog (IDialog dialog) {
            if (Dialog == dialog)
                Dialoger = null;
        }

        public void ResetDialog () {
            Dialoger = null;
        }

        public void OpenRequestBox (RequestBox request) {
            RequestBox = request;
        }

        public void ResetRequestBox () {
            RequestBox = null;
        }

        public bool IsBusy () => IsInRequest () || IsDialoging ();

        public bool IsDialoging () => Dialog != null;

        public bool IsInRequest () => RequestBox != null;

        public bool IsRequestSource () => IsInRequest () && RequestBox.Source == this;

        public bool IsRequestTarget () => IsInRequest () && RequestBox.Target == this;

        public bool IsTalkingWithNpc () => Dialog is NpcDialog;

        public bool IsInZaapDialog () => Dialog is ZaapDialog;

        public bool IsInZaapiDialog () => Dialog is ZaapiDialog;

        #endregion Dialog

        #region Party

        private readonly Dictionary<int, PartyInvitation> m_partyInvitations = new Dictionary<int, PartyInvitation> ();

        private readonly List<PartyTypeEnum> m_partiesLoyalTo = new List<PartyTypeEnum> ();

        private Character m_followedCharacter;

        private Party m_party;
        private ArenaParty m_arenaParty;

        public Party Party {
            get { return m_party; }
            private set {
                if (m_party != null && value != m_party) SetLoyalToParty (PartyTypeEnum.PARTY_TYPE_CLASSICAL, false);
                m_party = value;
            }
        }

        public ArenaParty ArenaParty {
            get { return m_arenaParty; }
            private set {
                if (m_arenaParty != null && value != m_arenaParty) SetLoyalToParty (PartyTypeEnum.PARTY_TYPE_ARENA, false);
                m_arenaParty = value;
            }
        }

        public Party[] Parties => new [] { Party, ArenaParty }.Where (x => x != null).ToArray ();

        public bool IsInParty () {
            return Party != null || ArenaParty != null;
        }

        public bool IsInParty (int id) {
            return (Party != null && Party.Id == id) || (ArenaParty != null && ArenaParty.Id == id);
        }

        public bool IsInParty (PartyTypeEnum type) {
            return (type == PartyTypeEnum.PARTY_TYPE_CLASSICAL && Party != null) || (type == PartyTypeEnum.PARTY_TYPE_ARENA && ArenaParty != null);
        }

        public bool IsPartyLeader () {
            return Party?.Leader == this;
        }

        public bool IsPartyLeader (int id) {
            return GetParty (id)?.Leader == this;
        }

        public Party GetParty (int id) {
            if (Party != null && Party.Id == id)
                return Party;

            if (ArenaParty != null && ArenaParty.Id == id)
                return ArenaParty;

            return null;
        }

        public bool IsLoyalToParty (PartyTypeEnum type) => m_partiesLoyalTo.Contains (type);
        public void SetLoyalToParty (PartyTypeEnum type, bool loyal) {
            if (loyal) m_partiesLoyalTo.Add (type);
            else m_partiesLoyalTo.Remove (type);

            PartyHandler.SendPartyLoyaltyStatusMessage (Client, GetParty (type), loyal);
        }

        public Party GetParty (PartyTypeEnum type) {
            switch (type) {
                case PartyTypeEnum.PARTY_TYPE_CLASSICAL:
                    return Party;

                case PartyTypeEnum.PARTY_TYPE_ARENA:
                    return ArenaParty;

                default:
                    throw new NotImplementedException (string.Format ("Cannot manage party of type {0}", type));
            }
        }

        public void SetParty (Party party) {
            switch (party.Type) {
                case PartyTypeEnum.PARTY_TYPE_CLASSICAL:
                    Party = party;
                    break;

                case PartyTypeEnum.PARTY_TYPE_ARENA:
                    ArenaParty = (ArenaParty) party;
                    break;

                default:
                    logger.Error ("Cannot manage party of type {0} ({1})", party.GetType (), party.Type);
                    break;
            }
        }

        public void ResetParty (PartyTypeEnum type) {
            switch (type) {
                case PartyTypeEnum.PARTY_TYPE_CLASSICAL:
                    Party = null;
                    break;

                case PartyTypeEnum.PARTY_TYPE_ARENA:
                    ArenaParty = null;
                    break;

                default:
                    logger.Error ("Cannot manage party of type {0}", type);
                    break;
            }

            CompassHandler.SendCompassResetMessage (Client, CompassTypeEnum.COMPASS_TYPE_PARTY);
        }

        #endregion Party
        #region Spouse & Misc

        public int CurrentSpouse { get { return m_record.SpouseID; } set { m_record.SpouseID = value; } }
        public void FollowSpouse (Character Spouse) {
            followforspouse = true;
            Client.Send (new CompassUpdatePartyMemberMessage ((sbyte) CompassTypeEnum.COMPASS_TYPE_SPOUSE, new MapCoordinates ((short) Spouse.Map.Position.X, (short) Spouse.Map.Position.Y), (ulong) Spouse.Id, true)); // idk active ???
            Client.Send (new TextInformationMessage ((sbyte) TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 368, new string[] {
                Spouse.Name
            }));
            Spouse.Client.Send (new TextInformationMessage ((sbyte) TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 52, new string[] {
                Name
            }));
        }
        public void StopFollowSpouse (Character spouse = null) {
            followforspouse = false;
            if (spouse != null)
                spouse.Client.Send (new TextInformationMessage ((sbyte) TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 53, new List<string> {
                    Name
                }));
            Client.Send (new CompassUpdatePartyMemberMessage ((sbyte) CompassTypeEnum.COMPASS_TYPE_SPOUSE, new MapCoordinates ((short) spouse.Map.Position.X, (short) spouse.Map.Position.Y), (ulong) spouse.Id, false)); // maybe dont need this
            Client.Send (new CompassResetMessage ((sbyte) CompassTypeEnum.COMPASS_TYPE_SPOUSE));
        }
        public void UpdateFollowSpouse (RolePlayActor actor, Map map)

        {
            Character spouse = actor as Character;
            if (actor == null)
                return;
            // Character spouse = World.Instance.GetCharacter(SpouseID);
            if (spouse != null && spouse.followforspouse)
                spouse.Client.Send (new CompassUpdatePartyMemberMessage ((sbyte) CompassTypeEnum.COMPASS_TYPE_SPOUSE, new MapCoordinates ((short) map.Position.X, (short) map.Position.Y), (ulong) Id, true));
            else if (spouse != null) {
                spouse.Client.Send (new CompassResetMessage ((sbyte) CompassTypeEnum.COMPASS_TYPE_SPOUSE));
                spouse.followforspouse = false;
            }
        }

        #endregion

        #region Trade

        public IExchange Exchange {
            get { return Dialog as IExchange; }
        }

        public Exchanger Exchanger => Dialoger as Exchanger;

        public ITrade Trade {
            get { return Dialog as ITrade; }
        }

        public PlayerTrade PlayerTrade {
            get { return Trade as PlayerTrade; }
        }

        public Trader Trader {
            get { return Dialoger as Trader; }
        }

        public bool IsInExchange () {
            return Exchanger != null;
        }

        public bool IsTrading () {
            return Trade != null;
        }

        public bool IsTradingWithPlayer () {
            return PlayerTrade != null;
        }

        #endregion Trade

        #region Idols

        public IdolInventory IdolInventory {
            get;
            set;
        }

        #endregion
        #region Spells Modifications
        public void SpellAddDamage (short spellId, short amount) {
            if (Spells == null) return;
            if (Spells.Where (x => x.Id == spellId) == null || Spells.Where (x => x.Id == spellId).Count () < 1) return;
            var spell = Spells.Where (x => x.Id == spellId).First ();
            if (spell != null) {
                var su = SpellsModifications.Where (x => x.spellId == spellId && x.modificationType == (sbyte) CharacterSpellModificationTypeEnum.DAMAGE);
                var Damage = new Fights.Damage (amount) {
                    Source = Fighter,
                    Spell = spell,
                };
                var damage = Fighter.CalculateDamageBonuses (Damage);
                if (su.Count () > 0) {
                    // SpellsModifications.FirstOrDefault(x => x.spellId == su.FirstOrDefault(xx => xx == x).spellId && x.modificationType == su.FirstOrDefault(xx => xx == x).modificationType).value.additionnal += amount;
                    //SpellsModifications.FirstOrDefault(x => x.spellId == su.FirstOrDefault(xx => xx == x).spellId && x.modificationType == su.FirstOrDefault(xx => xx == x).modificationType).value.alignGiftBonus += amount;
                    // SpellsModifications.FirstOrDefault(x => x.spellId == su.FirstOrDefault(xx => xx == x).spellId && x.modificationType == su.FirstOrDefault(xx => xx == x).modificationType).value.contextModif += amount;
                    SpellsModifications.FirstOrDefault (x => x.spellId == su.FirstOrDefault (xx => xx == x).spellId && x.modificationType == su.FirstOrDefault (xx => xx == x).modificationType).value.objectsAndMountBonus += (short) damage.Amount;
                    // SpellsModifications.FirstOrDefault(x => x.spellId == su.FirstOrDefault(xx => xx == x).spellId && x.modificationType == su.FirstOrDefault(xx => xx == x).modificationType).value.@base += amount;//(x => x == su)

                } else {
                    CharacterSpellModification s = new CharacterSpellModification ((sbyte) CharacterSpellModificationTypeEnum.DAMAGE, (ushort) spellId, new CharacterBaseCharacteristic (0, 0, (short) damage.Amount, 0, 0));
                    SpellsModifications.Add (s);
                }
                //ContextHandler.SendGameFightSynchronizeMessage(Client, Fight);
                //Fight.ForEach(entry => ContextHandler.SendGameFightSynchronizeMessage(entry.Client, Fight), true);
                //Fight.ForEach(entry => entry.RefreshStats());
                RefreshStats ();
                // Fight.GetAllCharacters((this as CharacterFighter));
                //  Fight.RejoinFightFromDisconnection(Fighter);
                /*foreach (var test in Fight.GetAllFighters())
                {

                   

                }
                ContextHandler.SendFighterStatsListMessage(Client,this);*/

            }
        }

        public void SpellAddDamageDisable (short spellId, short amount) {
            if (Spells == null) return;
            if (Spells.Where (x => x.Id == spellId) == null || Spells.Where (x => x.Id == spellId).Count () < 1) return;
            var spell = Spells.Where (x => x.Id == spellId).First ();
            if (spell != null) {
                var s = SpellsModifications.Where (x => x.spellId == spellId && x.modificationType == (sbyte) CharacterSpellModificationTypeEnum.DAMAGE);
                var Damage = new Fights.Damage (amount) {
                    Source = Fighter,
                    Spell = spell,
                };
                var damage = Fighter.CalculateDamageBonuses (Damage);

                if (s.Count () > 0) {
                    SpellsModifications.FirstOrDefault (x => x.spellId == s.FirstOrDefault (xx => xx == x).spellId && x.modificationType == s.FirstOrDefault (xx => xx == x).modificationType).value.objectsAndMountBonus -= (short) damage.Amount;
                    //SpellsModifications.FirstOrDefault(x => x == s.FirstOrDefault()).value.objectsAndMountBonus -= amount;
                    //ESTAVA ASSIM MAS A MSM COISA DO DE CIMA SpellsModifications.FirstOrDefault(x => x == s).value.contextModif -= amount;
                    RefreshStats ();
                }
            }
        }
        #endregion

        #region Titles & Ornaments

        public ReadOnlyCollection<ushort> Titles => Record.Titles.AsReadOnly ();

        public ReadOnlyCollection<ushort> Ornaments => Record.Ornaments.AsReadOnly ();

        public ushort? SelectedTitle {
            get { return Record.TitleId; }
            private set { Record.TitleId = value; }
        }

        public bool HasTitle (ushort title) => Record.Titles.Contains (title);

        public void AddTitle (ushort title) {
            if (HasTitle (title))
                return;

            Record.Titles.Add (title);
            TitleHandler.SendTitleGainedMessage (Client, title);
        }

        public bool RemoveTitle (ushort title) {
            var result = Record.Titles.Remove (title);

            if (result)
                TitleHandler.SendTitleLostMessage (Client, title);

            if (title == SelectedTitle)
                ResetTitle ();

            return result;
        }

        public bool SelectTitle (ushort title) {
            if (!HasTitle (title))
                return false;

            SelectedTitle = title;
            TitleHandler.SendTitleSelectedMessage (Client, title);
            RefreshActor ();
            return true;
        }

        public void ResetTitle () {
            SelectedTitle = null;
            TitleHandler.SendTitleSelectedMessage (Client, 0);
            RefreshActor ();
        }

        public ushort? SelectedOrnament {
            get { return Record.Ornament; }
            private set { Record.Ornament = value; }
        }

        public bool HasOrnament (ushort ornament) {
            return Record.Ornaments.Contains (ornament);
        }

        public void AddOrnament (ushort ornament) {
            if (!HasOrnament (ornament)) {
                Record.Ornaments.Add (ornament);
            }
            TitleHandler.SendOrnamentGainedMessage (Client, (ushort) ornament);
        }

        public bool RemoveOrnament (ushort ornament) {
            bool result;
            if (result = Record.Ornaments.Remove (ornament)) {
                TitleHandler.SendTitlesAndOrnamentsListMessage (Client, this);
            }
            return result;
        }

        public void RemoveAllOrnament () {
            Record.Ornaments.Clear ();
            TitleHandler.SendTitlesAndOrnamentsListMessage (Client, this);
        }

        public bool SelectOrnament (ushort ornament) {
            bool result;
            if (!HasOrnament (ornament)) {
                result = false;
            } else {
                SelectedOrnament = ornament;
                TitleHandler.SendOrnamentSelectedMessage (Client, (short) ornament);
                RefreshActor ();
                result = true;
            }
            return result;
        }

        public void ResetOrnament () {
            SelectedOrnament = null;
            TitleHandler.SendOrnamentSelectedMessage (Client, 0);
            RefreshActor ();
        }

        #endregion Titles & Ornaments

        #region Apparence

        public bool CustomLookActivated {
            get { return m_record.CustomLookActivated; }
            set { m_record.CustomLookActivated = value; }
        }

        public ActorLook CustomLook {
            get { return m_record.CustomEntityLook; }
            set { m_record.CustomEntityLook = value; }
        }

        public ActorLook DefaultLook {
            get { return m_record.DefaultLook; }
            set {
                m_record.DefaultLook = value;

                UpdateLook ();
            }
        }

        public override ActorLook Look {
            get { return CustomLookActivated ? CustomLook : m_look; }
            set {
                m_look = value;
                m_record.LastLook = value;
            }
        }

        public override SexTypeEnum Sex {
            get { return m_record.Sex; }
            protected set { m_record.Sex = value; }
        }

        public PlayableBreedEnum BreedId {
            get { return m_record.Breed; }
            private set {
                m_record.Breed = value;
                Breed = BreedManager.Instance.GetBreed (value);
            }
        }

        public Breed Breed {
            get;
            private set;
        }

        public Head Head {
            get;
            set;
        }

        public bool Invisible {
            get;
            private set;
        }

        public PlayerStatus Status {
            get;
            private set;
        }

        public void SetStatus (PlayerStatusEnum status) {
            if (Status.statusId == (sbyte) status)
                return;

            Status = new PlayerStatus ((sbyte) status);
            CharacterStatusHandler.SendPlayerStatusUpdateMessage (Client, Status);
        }

        public bool IsAvailable (Character character, bool msg) {
            if (Status.statusId == (sbyte) PlayerStatusEnum.PLAYER_STATUS_SOLO)
                return false;

            if (Status.statusId == (sbyte) PlayerStatusEnum.PLAYER_STATUS_PRIVATE && !FriendsBook.IsFriend (character.Account.Id))
                return false;

            if (Status.statusId == (sbyte) PlayerStatusEnum.PLAYER_STATUS_AFK && !msg)
                return false;

            return true;
        }

        public bool ToggleInvisibility (bool toggle) {
            Invisible = toggle;

            if (!IsInFight ())
                Map.Refresh (this);

            SendInformationMessage (TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, toggle ? (short) 236 : (short) 237);

            return Invisible;
        }

        public bool ToggleInvisibility () => ToggleInvisibility (!Invisible);

        public void ResetDefaultLook () {
            var look = Breed.GetLook (Sex, true);
            look.SetColors (DefaultLook.Colors);

            Head = BreedManager.Instance.GetHead (x => x.Breed == (uint) BreedId && x.Gender == (uint) Sex && x.Order == Head.Order);

            foreach (var skin in Head.Skins)
                look.AddSkin (skin);

            DefaultLook = look;
        }

        public void UpdateLook (bool send = true) {
            var look = DefaultLook.Clone ();

            look = Inventory.Where (x => x.IsEquiped ()).Aggregate (look, (current, item) => item.UpdateItemSkin (current));

            switch (PlayerLifeStatus) {
                case PlayerLifeStatusEnum.STATUS_PHANTOM:
                    look.BonesID = 3;
                    look.AddSkin (Sex == SexTypeEnum.SEX_FEMALE ? (short) 323 : (short) 322);
                    look.AddSkin (Sex == SexTypeEnum.SEX_FEMALE ? Breed.FemaleGhostBonesId : Breed.MaleGhostBonesId);
                    break;
                case PlayerLifeStatusEnum.STATUS_TOMBSTONE:
                    look.BonesID = Breed.TombBonesId;
                    break;
            }

            if (IsRiding) {
                var mountLook = EquippedMount.Look.Clone ();
                look.BonesID = 2;
                mountLook.SetRiderLook (look);

                look = mountLook;
            }
            var currentEmote = GetCurrentEmote ();

            if (currentEmote != null) {
                look = currentEmote.UpdateEmoteLook (this, look, true);
            }

            Look = look;

            if (send)
                SendLookUpdated ();
        }

        public void UpdateLook (Emote emote, bool apply, bool send = true) {
            Look = emote.UpdateEmoteLook (this, Look, apply);

            if (send)
                SendLookUpdated ();
        }

        public void UpdateLook (BasePlayerItem item, bool send = true) {
            Look = item.UpdateItemSkin (Look);

            if (send)
                SendLookUpdated ();
        }

        private void SendLookUpdated () {
            if (Fight != null) {
                Fighter.Look = Look.Clone ();
                Fighter.Look.RemoveAuras ();

                if (Fighter.IsDead () || Fighter.HasLeft ())
                    return;

                ContextHandler.SendGameContextRefreshEntityLookMessage (CharacterContainer.Clients, Fighter);
            } else {
                ContextHandler.SendGameContextRefreshEntityLookMessage (CharacterContainer.Clients, this);
            }
        }

        public void RefreshActor () {
            if (Fight != null) {
                Fighter.Look = Look.Clone ();
                Fighter.Look.RemoveAuras ();

                Fight.Map.Area.ExecuteInContext (() =>
                    Fight.RefreshActor (Fighter));
            } else if (Map != null) {
                Map.Area.ExecuteInContext (() =>
                    Map.Refresh (this));
            }

            OnLookRefreshed ();
        }

        #endregion Apparence

        #region Stats

        #region Delegates

        public delegate void LevelChangedHandler (Character character, ushort currentLevel, int difference);

        public delegate void GradeChangedHandler (Character character, sbyte currentGrade, int difference);

        #endregion Delegates

        #region Levels

        public ushort Level {
            get;
            private set;
        }

        public long Experience {
            get { return RealExperience - PrestigeRank * ExperienceManager.Instance.HighestCharacterExperience; }
            private set {
                RealExperience = PrestigeRank * ExperienceManager.Instance.HighestCharacterExperience + value;
                if ((value < UpperBoundExperience || Level >= ExperienceManager.Instance.HighestCharacterLevel) &&
                    value >= LowerBoundExperience) return;
                var lastLevel = Level;

                Level = ExperienceManager.Instance.GetCharacterLevel (value);

                LowerBoundExperience = ExperienceManager.Instance.GetCharacterLevelExperience (Level);
                UpperBoundExperience = ExperienceManager.Instance.GetCharacterNextLevelExperience (Level);

                var difference = Level - lastLevel;

                OnLevelChanged (Level, difference);
            }
        }

        public void LevelUp (ushort levelAdded) {
            ushort level;

            if (levelAdded + Level > ExperienceManager.Instance.HighestCharacterLevel)
                level = ExperienceManager.Instance.HighestCharacterLevel;
            else
                level = (ushort) (levelAdded + Level);

            var experience = ExperienceManager.Instance.GetCharacterLevelExperience (level);

            Experience = experience;
        }

        public void LevelDown (ushort levelRemoved) {
            ushort level;

            if (Level - levelRemoved < 1)
                level = 1;
            else
                level = (ushort) (Level - levelRemoved);

            var experience = ExperienceManager.Instance.GetCharacterLevelExperience (level);

            Experience = experience;
        }

        public void AddExperience (int amount) {
            Experience += amount;
        }

        public void AddExperience (long amount) {
            Experience += amount;
        }

        public void AddExperience (double amount) {
            Experience += (long) amount;
        }

        #endregion Levels

        public long LowerBoundExperience {
            get;
            private set;
        }

        public long UpperBoundExperience {
            get;
            private set;
        }

        public ushort StatsPoints {
            get { return m_record.StatsPoints; }
            set { m_record.StatsPoints = value; }
        }

        public ushort SpellsPoints {
            get { return m_record.SpellsPoints; }
            set { m_record.SpellsPoints = value; }
        }

        public short EnergyMax {
            get { return m_record.EnergyMax; }
            set { m_record.EnergyMax = value; }
        }

        public short Energy {
            get { return m_record.Energy; }
            set {
                var energy = (short) (value < 0 ? 0 : value);
                var diff = (short) (energy - m_record.Energy);

                m_record.Energy = energy;
                OnEnergyChanged (energy, diff);
            }
        }

        public PlayerLifeStatusEnum PlayerLifeStatus {
            get { return m_record.PlayerLifeStatus; }
            set {
                m_record.PlayerLifeStatus = value;
                OnPlayerLifeStatusChanged (value);
            }
        }

        public int LifePoints {
            get { return Stats.Health.Total; }
        }

        public int MaxLifePoints {
            get { return Stats.Health.TotalMax; }
        }

        public SpellInventory Spells {
            get;
            private set;
        }

        public StatsFields Stats {
            get;
            private set;
        }

        public bool GodMode {
            get;
            private set;
        }

        public bool CriticalMode {
            get;
            private set;
        }

        private void OnEnergyChanged (short energy, short diff) {
            if (diff < 0)
                SendInformationMessage (TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 34, Math.Abs (diff)); //Vous avez perdu <b>%1</b> points d'�nergie.

            if (energy > 0 && energy <= (Level * 10) && diff < 0)
                SendSystemMessage (11, false, energy);

            PlayerLifeStatus = energy > 0 ? PlayerLifeStatusEnum.STATUS_ALIVE_AND_KICKING : PlayerLifeStatusEnum.STATUS_TOMBSTONE;
        }

        private void OnPlayerLifeStatusChanged (PlayerLifeStatusEnum status) {
            if (status != PlayerLifeStatusEnum.STATUS_ALIVE_AND_KICKING)
                ForceDismount ();

            var phoenixMapId = 0;

            if (status == PlayerLifeStatusEnum.STATUS_PHANTOM) {
                phoenixMapId = World.Instance.GetNearestGraveyard (Map).PhoenixMapId;
                StartRegen ();
            }

            CharacterHandler.SendGameRolePlayPlayerLifeStatusMessage (Client, status, phoenixMapId);
            InitializationHandler.SendSetCharacterRestrictionsMessage (Client, this);

            UpdateLook ();
        }

        public void FreeSoul () {
            if (PlayerLifeStatus != PlayerLifeStatusEnum.STATUS_TOMBSTONE)
                return;

            var graveyard = World.Instance.GetNearestGraveyard (Map);
            Teleport (graveyard.Map, graveyard.Map.GetCell (graveyard.CellId));

            PlayerLifeStatus = PlayerLifeStatusEnum.STATUS_PHANTOM;
        }

        public event LevelChangedHandler LevelChanged;

        private void OnLevelChanged (ushort currentLevel, int difference) {
            if (difference > 0 && currentLevel <= 200) {
                SpellsPoints += (ushort) difference;
                StatsPoints += (ushort) (difference * 5);
            }
            if ( currentLevel <= 200)
             Stats.Health.Base += (short) (difference * 5);
            Stats.Health.DamageTaken = 0;

            if (currentLevel >= 100 && currentLevel - difference < 100) {
                Stats.AP.Base++;
                AddOrnament ((ushort) OrnamentEnum.NIVEAU_100);
                AddEmote (EmotesEnum.EMOTE_AURA_DE_PUISSANCE);
            } else if (currentLevel < 100 && currentLevel - difference >= 100) {
                Stats.AP.Base--;
                RemoveOrnament ((ushort) OrnamentEnum.NIVEAU_100);
                RemoveEmote (EmotesEnum.EMOTE_AURA_DE_PUISSANCE);
            }

            if (currentLevel >= 160 && currentLevel - difference < 160)
                AddOrnament ((ushort) OrnamentEnum.NIVEAU_160);
            else if (currentLevel < 160 && currentLevel - difference >= 160)
                RemoveOrnament ((ushort) OrnamentEnum.NIVEAU_160);

            if (currentLevel >= 200 && currentLevel - difference < 200)
                AddOrnament ((ushort) OrnamentEnum.NIVEAU_200);
            else if (currentLevel < 200 && currentLevel - difference >= 200)
                RemoveOrnament ((ushort) OrnamentEnum.NIVEAU_200);

            var shortcuts = Shortcuts.SpellsShortcuts;
            foreach (var spell in Breed.Spells) {
                if (spell.ObtainLevel > currentLevel) {
                    foreach (var shortcut in shortcuts.Where (x => x.Value.SpellId == spell.Spell).ToArray ())
                        Shortcuts.RemoveShortcut (ShortcutBarEnum.SPELL_SHORTCUT_BAR, shortcut.Key);

                    if (Spells.HasSpell (spell.Spell)) {
                        Spells.UnLearnSpell (spell.Spell);
                    }
                } else if (spell.ObtainLevel <= currentLevel && !Spells.HasSpell (spell.Spell)) {
                    if (!SpellsBlock.Contains(spell.Spell))
                    {
                        Spells.LearnSpell(spell.Spell);
                        Shortcuts.AddSpellShortcut(Shortcuts.GetNextFreeSlot(ShortcutBarEnum.SPELL_SHORTCUT_BAR),
                            (short)spell.Spell);
                    }
                }

                if (spell.VariantLevel > currentLevel) {
                    foreach (var shortcut in shortcuts.Where (x => x.Value.SpellId == spell.VariantId).ToArray ())
                        Shortcuts.RemoveShortcut (ShortcutBarEnum.SPELL_SHORTCUT_BAR, shortcut.Key);

                    if (Spells.HasSpell (spell.VariantId)) {
                        Spells.UnLearnSpell (spell.VariantId);
                    }
                } else if (spell.VariantLevel <= currentLevel && !Spells.HasSpell (spell.VariantId)) {
                    if (!SpellsBlock.Contains(spell.VariantId))
                    {
                        Spells.LearnSpell(spell.VariantId);
                        //var breedSpell = SpellManager.GetSpellVariant(spell.VariantId);
                        //if (breedSpell == null)
                            //return;
                        //Task.Factory.StartNewDelayed(1250, () => Client.Send(new SpellVariantActivationMessage((ushort)breedSpell.Spell, true)));

                    }
                }

                if (spell.Spell != 0)
                {
                    try
                    {
                        var currentSpell = Spells.GetSpell(spell.Spell);
                       
                        if(currentSpell != null)
                        {
                            if (currentSpell.ByLevel[2].MinPlayerLevel <= Level && currentSpell.Record.Level == 1)
                                currentSpell.BoostSpell();
                            if (currentSpell.ByLevel[3].MinPlayerLevel <= Level && currentSpell.Record.Level == 2)
                                currentSpell.BoostSpell();
                        }
                    }
                    catch (Exception ex) { }
                }
            }

            RefreshStats ();

            if (currentLevel > 1) {
                if (difference > 0)
                {
                    CharacterHandler.SendCharacterLevelUpMessage(Client, (ushort)currentLevel);
                    foreach (var spell in Client.Character.Shortcuts.GetShortcuts(ShortcutBarEnum.SPELL_SHORTCUT_BAR))
                    {
                        if (spell is Database.Shortcuts.SpellShortcut)
                            Client.Send(new SpellVariantActivationMessage(((ushort)(spell as Database.Shortcuts.SpellShortcut).SpellId), true));
                    }
                }
                CharacterHandler.SendCharacterLevelUpInformationMessage (Map.Clients, this, (ushort) currentLevel);
            }

            LevelChanged?.Invoke (this, currentLevel, difference);
        }

        public void ResetStats (bool additional = false) {
            Stats.Agility.Base = 0;
            Stats.Strength.Base = 0;
            Stats.Vitality.Base = 0;
            Stats.Wisdom.Base = 0;
            Stats.Intelligence.Base = 0;
            Stats.Chance.Base = 0;

            if (additional) {
                Stats.Agility.Additional = 0;
                Stats.Strength.Additional = 0;
                Stats.Vitality.Additional = 0;
                Stats.Wisdom.Additional = 0;
                Stats.Intelligence.Additional = 0;
                Stats.Chance.Additional = 0;
            }

            var newPoints = (Level - 1) * 5;
            StatsPoints = (ushort) newPoints;

            RefreshStats ();
            Inventory.CheckItemsCriterias ();

            //Caract�ristiques (de base et additionnelles) r�initialis�es.(469)
            //Caract�ristiques de base r�initialis�es.(470)
            SendInformationMessage (TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, (short) (additional ? 469 : 470));
        }

        public void RefreshStats () {
            if (IsRegenActive ())
                UpdateRegenedLife ();

            CharacterHandler.SendCharacterStatsListMessage (Client);

            OnStatsResfreshed ();
        }

        public void ToggleGodMode (bool state) {
            GodMode = state;
        }

        public void ToggleCriticalMode (bool state) {
            CriticalMode = state;
        }

        public bool IsGameMaster () {
            return UserGroup.IsGameMaster;
        }

        public void SetBreed (PlayableBreedEnum breed) {
            BreedId = breed;
            ResetDefaultLook ();
        }

        #endregion Stats

        #region Mount

        private List<Mount> m_stabledMounts = new List<Mount> ();
        private List<Mount> m_publicPaddockedMounts = new List<Mount> ();
        private Queue<Mount> m_releaseMounts = new Queue<Mount> ();

        public Mount EquippedMount {
            get { return m_equippedMount; }
            private set {
                m_equippedMount = value;
                Record.EquippedMount = value?.Id;

                if (value == null)
                    IsRiding = false;
            }
        }

        public bool IsRiding {
            get { return EquippedMount != null && Record.IsRiding; }
            private set { Record.IsRiding = value; }
        }

        public ReadOnlyCollection<Mount> PublicPaddockedMounts => m_publicPaddockedMounts.AsReadOnly ();
        public ReadOnlyCollection<Mount> StabledMounts => m_stabledMounts.AsReadOnly ();

        public Mount GetStabledMount (int mountId) {
            return m_stabledMounts.FirstOrDefault (x => x.Id == mountId);
        }

        public Mount GetPublicPaddockedMount (int mountId) {
            return m_publicPaddockedMounts.FirstOrDefault (x => x.Id == mountId);
        }

        private void LoadMounts () {
            var database = MountManager.Instance.Database;

            m_stabledMounts = database.Query<MountRecord> (string.Format (MountRecordRelator.FindByOwnerStabled, Id)).Select (x => new Mount (this, x)).ToList ();
            m_publicPaddockedMounts = database.Query<MountRecord> (string.Format (MountRecordRelator.FindByOwnerPublicPaddocked, Id)).Select (x => new Mount (this, x)).ToList ();

            if (Record.EquippedMount.HasValue) {
                EquippedMount = new Mount (this, database.Single<MountRecord> (string.Format (MountRecordRelator.FindById, Record.EquippedMount.Value)));

                if (IsRiding)
                    EquippedMount.ApplyMountEffects (false);
            }
        }

        private void SaveMounts () {
            var database = MountManager.Instance.Database;
            if (EquippedMount != null && (EquippedMount.IsDirty || EquippedMount.Record.IsNew))
                EquippedMount.Save (database);

            foreach (var mount in m_publicPaddockedMounts.Where (x => x.IsDirty || x.Record.IsNew))
                mount.Save (database);

            foreach (var mount in m_stabledMounts.Where (x => x.IsDirty || x.Record.IsNew))
                mount.Save (database);

            while (m_releaseMounts.Count > 0) {
                var deletedMount = m_releaseMounts.Dequeue ();
                MountManager.Instance.DeleteMount (deletedMount.Record);
            }
        }

        public void AddStabledMount (Mount mount) {
            mount.Owner = this;
            m_stabledMounts.Add (mount);
        }

        public void RemoveStabledMount (Mount mount) {
            m_stabledMounts.Remove (mount);
        }

        public void AddPublicPaddockedMount (Mount mount) {
            m_publicPaddockedMounts.Add (mount);
        }

        public void RemovePublicPaddockedMount (Mount mount) {
            m_publicPaddockedMounts.Remove (mount);
        }

        public void SetOwnedMount (Mount mount) {
            mount.Owner = this;
        }

        public bool HasEquippedMount () {
            return EquippedMount != null;
        }

        public bool EquipMount (Mount mount) {
            if (mount.Owner != this)
                return false;

            EquippedMount = mount;

            MountHandler.SendMountSetMessage (Client, mount.GetMountClientData ());
            MountHandler.SendMountXpRatioMessage (Client, mount.GivenExperience);
            return true;
        }

        public void UnEquipMount () {
            if (EquippedMount == null)
                return;

            ForceDismount ();

            if (EquippedMount.Harness != null) {
                Inventory.MoveItem (EquippedMount.Harness, CharacterInventoryPositionEnum.INVENTORY_POSITION_NOT_EQUIPED, true);

                // Votre harnachement est d�pos� dans votre inventaire.
                BasicHandler.SendTextInformationMessage (Client, TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 661);
            }

            EquippedMount.Save (MountManager.Instance.Database);
            EquippedMount = null;

            MountHandler.SendMountUnSetMessage (Client);
        }

        public bool ReleaseMount () {
            if (EquippedMount == null)
                return false;

            var mount = EquippedMount;
            UnEquipMount ();

            MountHandler.SendMountReleaseMessage (Client, mount.Id);
            m_releaseMounts.Enqueue (mount);
            return true;
        }

        public bool RideMount () {
            return !IsRiding && ToggleRiding ();
        }

        public bool Dismount () {
            return IsRiding && ToggleRiding ();
        }

        public void ForceDismount () {
            IsRiding = false;

            if (EquippedMount == null)
                return;

            EquippedMount.UnApplyMountEffects ();
            UpdateLook ();

            //Vous descendez de votre monture.
            BasicHandler.SendTextInformationMessage (Client, TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 273);
            MountHandler.SendMountRidingMessage (Client, IsRiding);
        }

        public bool ToggleRiding () {
            if (EquippedMount == null)
                return false;

            if (!IsRiding && Level < Mount.RequiredLevel) {
                SendInformationMessage (TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 227, Mount.RequiredLevel);
                return false;
            }

            if (IsBusy () || (IsInFight () && Fight.State != FightState.Placement)) {
                //Une action est d�j� en cours. Impossible de monter ou de descendre de votre monture.
                BasicHandler.SendTextInformationMessage (Client, TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 355);
                return false;
            }

            IsRiding = !IsRiding;

            if (IsRiding) {
                var pet = Inventory.TryGetItem (CharacterInventoryPositionEnum.ACCESSORY_POSITION_PETS);
                if (pet != null)
                    Inventory.MoveItem (pet, CharacterInventoryPositionEnum.INVENTORY_POSITION_NOT_EQUIPED);

                EquippedMount.ApplyMountEffects ();
            } else {
                //Vous descendez de votre monture.
                BasicHandler.SendTextInformationMessage (Client, TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 273);

                EquippedMount.UnApplyMountEffects ();
            }

            UpdateLook ();

            MountHandler.SendMountRidingMessage (Client, IsRiding);

            return true;
        }

        #endregion Mount

        #region Guild

        public GuildMember GuildMember {
            get;
            set;
        }

        public Guild Guild {
            get { return GuildMember != null ? GuildMember.Guild : null; }
        }

        public bool WarnOnGuildConnection {
            get {
                var result = false;
                try {
                    result = Record.WarnOnGuildConnection;
                } catch { }
                return result;
            }
            set {
                try {
                    Record.WarnOnGuildConnection = value;
                    GuildHandler.SendGuildMemberWarnOnConnectionStateMessage (Client, value);
                } catch { }
            }
        }

        #endregion Guild

        #region Alignment

        public AlignmentSideEnum AlignmentSide {
            get { return m_record.AlignmentSide; }
            private set {
                m_record.AlignmentSide = value;
            }
        }

        public sbyte AlignmentGrade {
            get;
            private set;
        }

        public sbyte AlignmentValue {
            get { return m_record.AlignmentValue; }
            private set { m_record.AlignmentValue = value; }
        }

        public ushort Honor {
            get { return m_record.Honor; }
            set {
                m_record.Honor = value;
                if ((value > LowerBoundHonor && value < UpperBoundHonor))
                    return;

                var lastGrade = AlignmentGrade;

                AlignmentGrade = (sbyte) ExperienceManager.Instance.GetAlignementGrade (m_record.Honor);

                LowerBoundHonor = ExperienceManager.Instance.GetAlignementGradeHonor ((byte) AlignmentGrade);
                UpperBoundHonor = ExperienceManager.Instance.GetAlignementNextGradeHonor ((byte) AlignmentGrade);

                var difference = AlignmentGrade - lastGrade;

                if (difference != 0)
                    OnGradeChanged (AlignmentGrade, difference);
            }
        }

        public ushort LowerBoundHonor {
            get;
            private set;
        }

        public ushort UpperBoundHonor {
            get;
            private set;
        }

        public ushort Dishonor {
            get { return m_record.Dishonor; }
            private set { m_record.Dishonor = value; }
        }

        public int CharacterPower {
            get { return Id + Level; }
        }

        public int CharacterRankId {
            get { return m_record.RankId; }
            set {
                m_record.RankId = value;
            }
        }

        public int CharacterRankExp {
            get { return m_record.RankExp; }
            set {
                int before = m_record.RankExp;
                m_record.RankExp = value;
                if (m_record.RankExp < 0) {
                    m_record.RankExp = 0;
                    m_record.RankId = 1;
                }
                this.checkRank (before, m_record.RankExp);
            }
        }

        public int CharacterRankWin {
            get { return m_record.RankWin; }
            set {
                m_record.RankWin = value;
            }
        }

        public int CharacterRankLose {
            get { return m_record.RankLose; }
            set {
                m_record.RankLose = value;
            }
        }

        public DateTime CharacterRankReward {
            get { return m_record.RankReward; }
            set { m_record.RankReward = value; }
        }

        public void checkRank (int expBefore, int expAfter) {
            if (this.CharacterRankId == 1)
                return;
            var ranks = RankManager.Instance.getRanks ();
            foreach (var rank in ranks) {
                if (expBefore < rank.Value.RankExp && expAfter >= rank.Value.RankExp) {
                    this.CharacterRankId = rank.Value.RankId;

                    switch (this.Account.Lang) {
                        case "fr":
                            this.SendServerMessage ("F�licitations, vous avez gagn� un nouveau rang dans le mode Duelliste '<b>" + this.GetCharacterRankName () + "</b>', �a va parfaitement!", Color.Chartreuse);
                            break;
                        case "es":
                            this.SendServerMessage ("Felicitaciones has ganado un nuevo rango en modo Duelista '<b>" + this.GetCharacterRankName () + "</b>', te queda perfectamente!", Color.Chartreuse);
                            break;
                        case "en":
                            this.SendServerMessage ("Congratulations you have won a new rank in Duelist mode '<b>" + this.GetCharacterRankName () + "</b>', it fits perfectly!", Color.Chartreuse);
                            break;
                        default:
                            this.SendServerMessage ("Parab�ns, voc� ganhou uma nova classifica��o no modo Duelista '<b>" + this.GetCharacterRankName () + "</b>', ela se encaixa perfeitamente!", Color.Chartreuse);
                            break;
                    }
                    break;
                }
                if (expBefore >= rank.Value.RankExp && expAfter < rank.Value.RankExp) {
                    if (rank.Value.RankId == 1 || rank.Value.RankId == 0) {
                        this.CharacterRankId = 1;

                        switch (this.Account.Lang) {
                            case "fr":
                                this.SendServerMessage ("Vous n'avez pas plus de rang � perdre en mode Dueliste, n'abandonnez pas '<b>" + this.GetCharacterRankName () + "</b>'...", Color.Chartreuse);
                                break;
                            case "es":
                                this.SendServerMessage ("Usted no tiene m�s rango para perder en el modo Duelista, no desista a '<b>" + this.GetCharacterRankName () + "</b>'...", Color.Chartreuse);
                                break;
                            case "en":
                                this.SendServerMessage ("You no longer have rank to lose in Duelist mode, do not give up '<b>" + this.GetCharacterRankName () + "</b>'...", Color.Chartreuse);
                                break;
                            default:
                                this.SendServerMessage ("Voc� n�o tem mais rank para perder no modo Duelista, n�o desista '<b>" + this.GetCharacterRankName () + "</b>'...", Color.Chartreuse);
                                break;
                        }
                    } else {
                        this.CharacterRankId = rank.Value.RankId - 1;

                        switch (this.Account.Lang) {
                            case "fr":
                                this.SendServerMessage ("Vous avez perdu un rang en mode Duelist, maintenant vous �tes rang'<b>" + this.GetCharacterRankName () + "</b>'...", Color.Chartreuse);
                                break;
                            case "es":
                                this.SendServerMessage ("Has perdido un rango en el modo Duelista, ahora eres rango '<b>" + this.GetCharacterRankName () + "</b>'...", Color.Chartreuse);
                                break;
                            case "en":
                                this.SendServerMessage ("You have lost a rank in Duelist mode, now you are rank '<b>" + this.GetCharacterRankName () + "</b>'...", Color.Chartreuse);
                                break;
                            default:
                                this.SendServerMessage ("Voc� perdeu um rank no modo Duelist, agora voc� � rank '<b>" + this.GetCharacterRankName () + "</b>'...", Color.Chartreuse);
                                break;
                        }
                    }
                    break;
                }
            }
        }
        public bool PvPEnabled {
            get { return m_record.PvPEnabled; }
            private set {
                m_record.PvPEnabled = value;
                OnPvPToggled ();
            }
        }

        public string GetCharacterRankName () {
            return RankManager.Instance.getRecordById (this.CharacterRankId, this).RankName;
        }

        public int GetCharacterRankBonus () {
            return RankManager.Instance.getRecordById (this.CharacterRankId, this).RankBonus;
        }
        public void ChangeAlignementSide (AlignmentSideEnum side) {
            AlignmentSide = side;

            OnAligmenentSideChanged ();
            if (side == AlignmentSideEnum.ALIGNMENT_ANGEL) {

                switch (Account.Lang) {
                    case "fr":
                        SendServerMessage ("F�licitations, maintenant vous �tes <b>Bontariano</b> !");
                        break;
                    case "es":
                        SendServerMessage ("Felicitaciones, ahora eres <b>Bontariano</b> !");
                        break;
                    case "en":
                        SendServerMessage ("Congratulations, now you are <b>Bontariano</b> !");
                        break;
                    default:
                        SendServerMessage ("Parab�ns, agora voc� � <b>Bontariano</b> !");
                        break;
                }
            } else if (side == AlignmentSideEnum.ALIGNMENT_EVIL) {
                switch (Account.Lang) {
                    case "fr":
                        SendServerMessage ("F�licitations, maintenant vous �tes <b>Brakmariano</b> !");
                        break;
                    case "es":
                        SendServerMessage ("Felicitaciones, ahora eres <b>Brakmariano</b> !");
                        break;
                    case "en":
                        SendServerMessage ("Congratulations, now you are <b>Brakmariano</b> !");
                        break;
                    default:
                        SendServerMessage ("Parab�ns, agora voc� � <b>Brakmariano</b> !");
                        break;
                }
            } else if (side == AlignmentSideEnum.ALIGNMENT_MERCENARY) {
                switch (Account.Lang) {
                    case "fr":
                        SendServerMessage ("F�licitations, maintenant vous �tes <b>Mercenaire</b> !");
                        break;
                    case "es":
                        SendServerMessage ("Felicitaciones, ahora eres <b>Mercenario</b> !");
                        break;
                    case "en":
                        SendServerMessage ("Congratulations, now you are <b>Mercenary</b> !");
                        break;
                    default:
                        SendServerMessage ("Parab�ns, agora voc� � <b>Mercen�rio</b> !");
                        break;
                }
            }
        }

        public void AddHonor (ushort amount) {
            Honor += (Honor + amount) >= HonorLimit ? (ushort) (HonorLimit - Honor) : amount;
        }

        public void SubHonor (ushort amount) {
            if (Honor - amount < 0)
                Honor = 0;
            else
                Honor -= amount;
        }

        public void AddDishonor (ushort amount) {
            Dishonor += amount;
        }

        public void SubDishonor (ushort amount) {
            if (Dishonor - amount < 0)
                Dishonor = 0;
            else
                Dishonor -= amount;
        }

        public void TogglePvPMode (bool state) {
            if (IsInFight ())
                return;

            PvPEnabled = state;
        }

        public event GradeChangedHandler GradeChanged;

        private void OnGradeChanged (sbyte currentLevel, int difference) {
            Map.Refresh (this);
            RefreshStats ();

            GradeChanged?.Invoke (this, currentLevel, difference);
        }

        public event Action<Character, bool> PvPToggled;

        private void OnPvPToggled () {
            foreach (var item in Inventory.GetItems (x => x.Position == CharacterInventoryPositionEnum.ACCESSORY_POSITION_SHIELD && x.AreConditionFilled (this)))
                Inventory.MoveItem (item, CharacterInventoryPositionEnum.INVENTORY_POSITION_NOT_EQUIPED);

            if (!PvPEnabled) {
                var amount = (ushort) Math.Round (Honor * 0.05);
                SubHonor (amount);
                switch (Account.Lang) {
                    case "fr":
                        SendServerMessage ($"Vous avez perdu <b>{amount}</b> points d'honneur.");
                        break;
                    case "es":
                        SendServerMessage ($"Has perdido <b>{amount}</b> puntos de honor.");
                        break;
                    case "en":
                        SendServerMessage ($"You have lost <b>{amount}</b> honor points.");
                        break;
                    default:
                        SendServerMessage ($"Voc� perdeu <b>{amount}</b> pontos de honra.");
                        break;
                }
            }

            Map.Refresh (this);
            RefreshStats ();

            PvPToggled?.Invoke (this, PvPEnabled);
        }

        public event Action<Character, AlignmentSideEnum> AlignmnentSideChanged;

        private void OnAligmenentSideChanged () {
            TogglePvPMode (true);
            Honor = 0;
            Dishonor = 0;
            if (AlignmentSide != AlignmentSideEnum.ALIGNMENT_NEUTRAL) {
                this.AlignmentGrade = 1;
            } else {
                this.AlignmentSide = 0;
            }
            Map.Refresh (this);
            RefreshStats ();
            RefreshActor ();
            PvPHandler.SendAlignmentRankUpdateMessage (Client, this);

            AlignmnentSideChanged?.Invoke (this, AlignmentSide);
            Client.Character.Map.Refresh (Client.Character);
        }

        #endregion Alignment

        #region Fight

        public CharacterFighter Fighter {
            get;
            private set;
        }
        public CompanionActor Companion {
            get;
            set;
        }

        public FightSpectator Spectator {
            get;
            private set;
        }

        public FightPvT TaxCollectorDefendFight {
            get;
            private set;
        }
        public FightPvMr PrismDefendFight { get; set; }
        public IFight Fight {
            get { return Fighter == null ? (Spectator != null ? Spectator.Fight : null) : Fighter.Fight; }
        }

        public FightTeam Team {
            get { return Fighter != null ? Fighter.Team : null; }
        }

        public bool IsGhost () {
            return PlayerLifeStatus != PlayerLifeStatusEnum.STATUS_ALIVE_AND_KICKING;
        }

        public bool IsSpectator () {
            return Spectator != null;
        }

        public bool IsInFight () {
            return IsSpectator () || IsFighting ();
        }

        public bool IsFighting () {
            return Fighter != null;
        }

        public void SetDefender (FightPvT fight) {
            TaxCollectorDefendFight = fight;
        }

        public void ResetDefender () {
            TaxCollectorDefendFight = null;
        }
        public void SetDefender (FightPvMr fight) {
            PrismDefendFight = fight;
        }
        public void ResetPrismDefender () {
            PrismDefendFight = null;
        }
        #endregion Fight

        #region Shortcuts

        public ShortcutBar Shortcuts {
            get;
            private set;
        }

        #endregion Shortcuts

        #region Regen

        public byte RegenSpeed {
            get;
            private set;
        }

        public DateTime? RegenStartTime {
            get;
            private set;
        }

        #endregion Regen

        #region Chat

        public ChatHistory ChatHistory {
            get;
            private set;
        }

        public DateTime? MuteUntil {
            get { return m_record.MuteUntil; }
            private set { m_record.MuteUntil = value; }
        }

        public void Mute (TimeSpan time, Character from) {
            MuteUntil = DateTime.Now + time;

            // %1 vous a rendu muet pour %2 minute(s).
            SendInformationMessage (TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 17, from.Name,
                (int) time.TotalMinutes);
        }

        public void Mute (TimeSpan time) {
            MuteUntil = DateTime.Now + time;
            // Le principe de pr�caution vous a rendu muet pour %1 seconde(s).
            SendInformationMessage (TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 123, (int) time.TotalSeconds);
        }

        public void UnMute () {
            MuteUntil = null;

            switch (Account.Lang) {
                case "fr":
                    SendServerMessage ("Vous avez �t� d�mut�.", Color.Red);
                    break;
                case "es":
                    SendServerMessage ("Usted ha sido desmutado.", Color.Red);
                    break;
                case "en":
                    SendServerMessage ("You have been unmuted.", Color.Red);
                    break;
                default:
                    SendServerMessage ("Voc� foi desmutado.", Color.Red);
                    break;
            }
        }

        public bool IsMuted () {
            return MuteUntil.HasValue && MuteUntil > DateTime.Now;
        }

        public TimeSpan GetMuteRemainingTime () {
            if (!MuteUntil.HasValue)
                return TimeSpan.MaxValue;

            return MuteUntil.Value - DateTime.Now;
        }

        #endregion Chat

        #region Smiley

        public event Action<Character, int> MoodChanged;

        private void OnMoodChanged () {
            try {
                Guild?.UpdateMember (Guild.TryGetMember (Id));
                MoodChanged?.Invoke (this, SmileyMoodId);
            } catch { }

        }

        public ReadOnlyCollection<SmileyPacksEnum> SmileyPacks => Record.SmileyPacks.AsReadOnly ();

        public int SmileyMoodId {
            get { return Record.SmileyMoodId; }
            set { Record.SmileyMoodId = value; }
        }

        public DateTime LastMoodChange {
            get;
            private set;
        }

        public bool HasSmileyPack (SmileyPacksEnum pack) => SmileyPacks.Contains (pack);

        public void AddSmileyPack (SmileyPacksEnum pack) {
            if (HasSmileyPack (pack))
                return;

            Record.SmileyPacks.Add (pack);
            ChatHandler.SendChatSmileyExtraPackListMessage (Client, SmileyPacks.ToArray ());
        }

        public bool RemoveSmileyPack (SmileyPacksEnum pack) {
            var result = Record.SmileyPacks.Remove (pack);
            int[] smileys = null;
            if (result) {
                switch ((int) pack) {
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

                if (smileys != null) {
                    foreach (var a in smileys) {
                        foreach (var b in Shortcuts.SmileyShortcuts.Where (x => x.Value.SmileyId == (short) a).ToList ()) {
                            //var shortcut = Shortcuts.SmileyShortcuts.FirstOrDefault(x => x.Value.SmileyId == a);
                            if (b.Value != null)
                                Shortcuts.RemoveShortcut (ShortcutBarEnum.GENERAL_SHORTCUT_BAR, b.Key);
                        }
                    }
                }

                ChatHandler.SendChatSmileyExtraPackListMessage (Client, SmileyPacks.ToArray ());
            }

            return result;
        }

        public override void DisplaySmiley (short smileyId) {
            ChatHandler.SendChatSmileyMessage (CharacterContainer.Clients, this, smileyId);
        }

        public void SetMood (short smileyId) {
            if (DateTime.Now - LastMoodChange < TimeSpan.FromSeconds (5))
                ChatHandler.SendMoodSmileyResultMessage (Client, 2, smileyId);
            else {
                SmileyMoodId = smileyId;
                LastMoodChange = DateTime.Now;

                ChatHandler.SendMoodSmileyResultMessage (Client, 0, smileyId);
                OnMoodChanged ();
            }
        }

        #endregion Smiley

        #region Prestige

        public int PrestigeRank {
            get { return m_record.PrestigeRank; }
            set { m_record.PrestigeRank = value; }
        }

        public long RealExperience {
            get { return m_record.Experience; }
            private set { m_record.Experience = value; }
        }

        public bool IsPrestigeMax () => PrestigeRank == PrestigeManager.PrestigeTitles.Length;

        public PrestigeItem GetPrestigeItem () {
            if (!PrestigeManager.Instance.PrestigeEnabled)
                return null;

            return Inventory.TryGetItem (PrestigeManager.BonusItem) as PrestigeItem;
        }

        public PrestigeItem CreatePrestigeItem () => (PrestigeItem) Inventory.AddItem (PrestigeManager.BonusItem);

        public bool IncrementPrestige () {
            if (Level < 200 || IsPrestigeMax () && PrestigeManager.Instance.PrestigeEnabled)
                return false;

            PrestigeRank++;
            AddTitle ((ushort) PrestigeManager.Instance.GetPrestigeTitle (PrestigeRank));

            switch (PrestigeRank) {
                case 1:
                    AddOrnament (16);
                    break;
                case 3:
                    AddOrnament (17);
                    break;
                case 5:
                    AddOrnament (18);
                    break;
                case 7:
                    AddOrnament (60);
                    break;
                case 9:
                    AddOrnament (92);
                    break;
                case 10:
                    AddOrnament (93);
                    break;
            }

            //Inventory.AddItem(atomItem, 10000 * PrestigeRank);

            var item = GetPrestigeItem ();

            if (item == null)
                item = CreatePrestigeItem ();
            else {
                item.UpdateEffects ();
                Inventory.RefreshItem (item);
            }

            OpenPopup ($"Tu es pass� prestige {PrestigeRank} ! \r\nTu repasses donc niveau 1. \r\nTu dois te d�connecter et te reconnecter pour voir ton prestige et ton niveau !");

            foreach (var equippedItem in Inventory.ToArray ())
                Inventory.MoveItem (equippedItem, CharacterInventoryPositionEnum.INVENTORY_POSITION_NOT_EQUIPED);

            var points = (Spells.CountSpentBoostPoint () + SpellsPoints) - (Level - 1);

            Dismount ();

            Experience = 0;
            Spells.ForgetAllSpells ();
            SpellsPoints = (ushort) (points >= 0 ? points : 0);
            ResetStats ();

            return true;
        }

        public bool DecrementPrestige () {
            RemoveTitle ((ushort) PrestigeManager.Instance.GetPrestigeTitle (PrestigeRank));
            PrestigeRank--;

            var item = GetPrestigeItem ();

            if (item != null) {
                if (PrestigeRank > 0) {
                    item.UpdateEffects ();
                    Inventory.RefreshItem (item);
                } else Inventory.RemoveItem (item);
            }

            OpenPopup (
                string.Format (
                    "Vous venez de passer au rang prestige {0}. Vous repassez niveau 1 et vous avez acquis des bonus permanents visible sur l'objet '{1}' de votre inventaire, ",
                    PrestigeRank + 1, item.Template.Name) +
                "les bonus s'appliquent sans �quipper l'objet. Vous devez vous reconnecter pour actualiser votre niveau.");

            return true;
        }

        public void ResetPrestige () {
            foreach (var title in PrestigeManager.PrestigeTitles) {
                RemoveTitle ((ushort) title);
            }
            PrestigeRank = 0;

            var item = GetPrestigeItem ();

            if (item != null) {
                Inventory.RemoveItem (item);
            }
        }

        #endregion Prestige

        #region Arena

        public bool CanEnterArena (bool send = true) {
            if (Level < ArenaManager.ArenaMinLevel) {
                if (send)
                    // Vous devez �tre au moins niveau 50 pour faire des combats en Koliz�um.
                    SendInformationMessage (TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 326);
                return false;
            }

            if (ArenaPenality >= DateTime.Now) {
                if (send)
                    // Vous �tes interdit de Koliz�um pour un certain temps car vous avez abandonn� un match de Koliz�um.
                    SendInformationMessage (TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 323);

                return false;
            }

            if (IsInJail ()) {
                if (send)
                    // Vous ne pouvez pas participer au Koliz�um depuis une prison.
                    SendInformationMessage (TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 339);

                return false;
            }

            if (IsGhost ()) {
                if (send)
                    // Aucun combat de koliz�um ne vous sera propos� tant que vous serez en tombe ou en fant�me.
                    SendInformationMessage (TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 373);

                return false;
            }

            if (Fight is ArenaFight) {
                if (send)
                    //Vous �tes d�j� en combat de Koliz�um.
                    SendInformationMessage (TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 334);

                return false;
            }

            if (Fight is FightAgression || Fight is FightPvT || Fight is FightDuel)
                return false;

            return true;
        }

        public void CheckArenaDailyProperties_1vs1 () {
            if (m_record.ArenaDailyDate_1vs1.Day == DateTime.Now.Day || ArenaDailyMaxRank_1vs1 <= 0)
                return;

            var amountToken = (int) Math.Floor (ArenaDailyMaxRank_1vs1 / 10d);
            var amountKamas = (ArenaDailyMaxRank_1vs1 * 1000);

            if (amountToken > 1)
                amountToken = 1;

            m_record.ArenaDailyDate_1vs1 = DateTime.Now;
            ArenaDailyMaxRank_1vs1 = 0;
            ArenaDailyMatchsCount_1vs1 = 0;
            ArenaDailyMatchsWon_1vs1 = 0;

            Inventory.AddItem (ArenaManager.Instance.TokenItemTemplate, amountToken);
            Inventory.AddKamas (amountKamas);

            DisplayNotification (NotificationEnum.KOLIZÉUM, amountKamas, amountToken);
        }

        public void CheckArenaDailyProperties_3vs3 () {
            if (m_record.ArenaDailyDate_3vs3.Day == DateTime.Now.Day || ArenaDailyMaxRank_3vs3 <= 0)
                return;

            var amountToken = (int) Math.Floor (ArenaDailyMaxRank_3vs3 / 10d);
            var amountKamas = (ArenaDailyMaxRank_3vs3 * 1000);

            if (amountToken > 3)
                amountToken = 3;

            m_record.ArenaDailyDate_3vs3 = DateTime.Now;
            ArenaDailyMaxRank_3vs3 = 0;
            ArenaDailyMatchsCount_3vs3 = 0;
            ArenaDailyMatchsWon_3vs3 = 0;
            Inventory.AddItem (ArenaManager.Instance.TokenItemTemplate, amountToken);
            Inventory.AddKamas (amountKamas);

            DisplayNotification (NotificationEnum.KOLIZÉUM, amountKamas, amountToken);
        }
        public int ComputeWonArenaCaliston () {
            return (this.Level > 1) ? this.Level / 43 : 1;
        }
        public int ComputeWonArenaTokens (int rank) {
            if (Level >= 20 && Level <= 150) {
                return 2;
            }

            return 3;
        }

        public int ComputeWonArenaKamas () {
            return (int) Math.Floor ((500 * (Level * (Level / 255d))));
        }

        public int ComputeWonExperience () {
            return (int) Math.Floor ((1800 * (Level * (Level / 255d))));
        }
        public void UpdateArenaProperties (int rank, bool win, int mode) {
            if (mode == 1)
                CheckArenaDailyProperties_1vs1 ();

            else
                CheckArenaDailyProperties_3vs3 ();

            #region 1vs1
            if (mode == 1) {
                ArenaRank_1vs1 = rank;

                if (rank > ArenaMaxRank_1vs1)
                    ArenaMaxRank_1vs1 = rank;

                if (rank > ArenaDailyMaxRank_1vs1)
                    ArenaDailyMaxRank_1vs1 = rank;

                ArenaDailyMatchsCount_1vs1++;

                if (win)
                    ArenaDailyMatchsWon_1vs1++;

                m_record.ArenaDailyDate_1vs1 = DateTime.Now;

            }
            #endregion

            #region 3vs3
            else {
                ArenaRank_3vs3 = rank;

                if (rank > ArenaMaxRank_3vs3)
                    ArenaMaxRank_3vs3 = rank;

                if (rank > ArenaDailyMaxRank_3vs3)
                    ArenaDailyMaxRank_3vs3 = rank;

                ArenaDailyMatchsCount_3vs3++;

                if (win)
                    ArenaDailyMatchsWon_3vs3++;

                m_record.ArenaDailyDate_3vs3 = DateTime.Now;

            }

            ContextRoleplayHandler.SendGameRolePlayArenaUpdatePlayerInfosMessage (Client, this);
            #endregion

            ContextRoleplayHandler.SendGameRolePlayArenaUpdatePlayerInfosMessage (Client, this);

            if (!win)
                return;

            if (this == null)
                return;

            if (this.Fighter != null) {
                Random rand = new Random ();

                var m_breeds = (from fighter in this.Fighter.OpposedTeam.GetAllFightersWithLeavers ().OfType<CharacterFighter> () where (this.Fight.Losers.Fighters.Contains (fighter) || this.Fight.Leavers.Contains (fighter)) select fighter.Character.Breed.Id).ToList ();

                Inventory.AddItem (ArenaManager.Instance.TokenItemTemplate, ComputeWonArenaTokens (rank));
                Inventory.AddKamas (ComputeWonArenaKamas ());
                AddExperience (ComputeWonExperience ());
            }

        }

        public void SetArenaPenality (TimeSpan time) {
            ArenaPenality = DateTime.Now + time;

            // Vous �tes interdit de Koliz�um pour un certain temps car vous avez abandonn� un match de Koliz�um.
            SendInformationMessage (TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 323);
        }

        public void ToggleArenaPenality () {
            SetArenaPenality (TimeSpan.FromMinutes (ArenaManager.ArenaPenalityTime));
        }

        public void SetAgressionPenality (TimeSpan time) {
            AgressionPenality = DateTime.Now + time;

            switch (Account.Lang) {
                case "fr":
                    SendServerMessage ("Vous avez �t� interdit pour une p�riode de " + ArenaManager.ArenaPenalityTime + " ans pour avoir quitt� un match...", Color.DarkOrange);
                    break;
                case "es":
                    SendServerMessage ("Usted ha sido prohibido por un per�odo de " + ArenaManager.ArenaPenalityTime + " por abandonar una partida...", Color.DarkOrange);
                    break;
                case "en":
                    SendServerMessage ("You were banned for a period of " + ArenaManager.ArenaPenalityTime + " for leaving a match...", Color.DarkOrange);
                    break;
                default:
                    SendServerMessage ("Voc� foi banido por um per�odo de " + ArenaManager.ArenaPenalityTime + " por abandonar uma partida...", Color.DarkOrange);
                    break;
            }
            this.battleFieldOn = false;
        }

        public void ToggleAgressionPenality () {
            SetAgressionPenality (TimeSpan.FromMinutes (ArenaManager.ArenaPenalityTime));
        }
        public void ToggleArenaWaitTime () {
            SetArenaPenality (TimeSpan.FromMinutes (ArenaManager.ArenaWaitTime));
        }

        #region Arena (3vs3)
        public int ArenaRank_3vs3 {
            get { return m_record.ArenaRank_3vs3; }
            set { m_record.ArenaRank_3vs3 = value; }
        }

        public int ArenaMaxRank_3vs3 {
            get { return m_record.ArenaMaxRank_3vs3; }
            set { m_record.ArenaMaxRank_3vs3 = value; }
        }

        public int ArenaDailyMaxRank_3vs3 {
            get { return m_record.ArenaDailyMaxRank_3vs3; }
            set { m_record.ArenaDailyMaxRank_3vs3 = value; }
        }

        public int ArenaDailyMatchsWon_3vs3 {
            get { return m_record.ArenaDailyMatchsWon_3vs3; }
            set { m_record.ArenaDailyMatchsWon_3vs3 = value; }
        }

        public int ArenaDailyMatchsCount_3vs3 {
            get { return m_record.ArenaDailyMatchsCount_3vs3; }
            set { m_record.ArenaDailyMatchsCount_3vs3 = value; }
        }
        #endregion

        #region Arena (1vs1)
        public int ArenaRank_1vs1 {
            get { return m_record.ArenaRank_1vs1; }
            set { m_record.ArenaRank_1vs1 = value; }
        }

        public int ArenaMaxRank_1vs1 {
            get { return m_record.ArenaMaxRank_1vs1; }
            set { m_record.ArenaMaxRank_1vs1 = value; }
        }

        public int ArenaDailyMaxRank_1vs1 {
            get { return m_record.ArenaDailyMaxRank_1vs1; }
            set { m_record.ArenaDailyMaxRank_1vs1 = value; }
        }

        public int ArenaDailyMatchsWon_1vs1 {
            get { return m_record.ArenaDailyMatchsWon_1vs1; }
            set { m_record.ArenaDailyMatchsWon_1vs1 = value; }
        }

        public int ArenaDailyMatchsCount_1vs1 {
            get { return m_record.ArenaDailyMatchsCount_1vs1; }
            set { m_record.ArenaDailyMatchsCount_1vs1 = value; }
        }
        #endregion
        public DateTime ArenaPenality {
            get { return m_record.ArenaPenalityDate; }
            set { m_record.ArenaPenalityDate = value; }
        }

        public DateTime AgressionPenality {
            get { return m_record.AgressionPenalityDate; }
            set { m_record.AgressionPenalityDate = value; }
        }
        public ArenaPopup ArenaPopup {
            get;
            set;
        }
        public int ArenaMode {
            get;
            set;
        }
        #endregion Arena

        #region VIP
        public bool Vip {
            get {

                return (Account.IsSubscribe == true);
            }
        }

        #endregion VIP

        #endregion Properties

        #region Actions

        #region Chat

        public bool AdminMessagesEnabled {
            get;
            set;
        }

        public void SendConnectionMessages () {
            SendInformationMessage (TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 89);
            if (Account.LastConnection != null) {
                var date = Account.LastConnection.Value;

                SendInformationMessage (TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 152,
                    date.Year,
                    date.Month,
                    date.Day,
                    date.Hour,
                    date.Minute.ToString ("00"),
                    Account.LastConnectionIp);

                SendInformationMessage (TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 153, Client.IP);
            }

            var kamasMerchant = 0;
            var merchantSoldItems = new List<ObjectItemGenericQuantityPrice> ();

            foreach (var item in MerchantBag.ToArray ()) {
                if (item.StackSold <= 0)
                    continue;

                var price = (int) (item.Price * item.StackSold);
                kamasMerchant += price;

                merchantSoldItems.Add (new ObjectItemGenericQuantityPrice ((ushort) item.Template.Id, (uint) item.StackSold, (ulong) price));

                item.StackSold = 0;

                if (item.Stack == 0)
                    MerchantBag.RemoveItem (item, true);
            }

            Inventory.AddKamas (kamasMerchant);

            var soldItems = BidHouseManager.Instance.GetSoldBidHouseItems (Account.Id);
            var bidhouseSoldItems = new List<ObjectItemGenericQuantityPrice> ();
            var kamasBidHouse = 0;

            foreach (var item in soldItems) {
                kamasBidHouse += (int) item.Price;
                BidHouseManager.Instance.RemoveBidHouseItem (item, true);

                bidhouseSoldItems.Add (new ObjectItemGenericQuantityPrice ((ushort) item.Template.Id, (uint) item.Stack, (uint) item.Price));
            }

            Bank.AddKamas (kamasBidHouse);

            if (merchantSoldItems.Any () || bidhouseSoldItems.Any ())
                InventoryHandler.SendExchangeOfflineSoldItemsMessage (Client, merchantSoldItems.ToArray (), bidhouseSoldItems.ToArray ());
        }

        public void SendServerMessage (string message) {
            BasicHandler.SendTextInformationMessage (Client, TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 0, message);
        }

        public void SendServerMessage (string message, Color color) {
            SendServerMessage (string.Format ("<font color=\"#{0}\">{1}</font>", color.ToArgb ().ToString ("X"), message));
        }

        public void SendInformationMessage (TextInformationTypeEnum msgType, short msgId, params object[] parameters) {
            BasicHandler.SendTextInformationMessage (Client, msgType, msgId, parameters);
        }

        public void SendSystemMessage (short msgId, bool hangUp, params object[] parameters) {
            BasicHandler.SendSystemMessageDisplayMessage (Client, hangUp, msgId, parameters);
        }

        public void OpenPopup (string message) {
            OpenPopup (message, "Server", 0);
        }

        public void OpenPopup (string message, string sender, sbyte lockDuration) {
            ModerationHandler.SendPopupWarningMessage (Client, message, sender, lockDuration);
        }

        #endregion Chat

        #region Move

        public override void OnEnterMap (Map map) {
            ContextRoleplayHandler.SendCurrentMapMessage (Client, map.Id);

            // send actor actions
            foreach (var actor in map.Actors) {
                if (!actor.IsMoving ())
                    continue;

                var moveKeys = actor.MovementPath.GetServerPathKeys ();
                var actorMoving = actor;

                if (actor.MovementPath.Walk)
                    ContextHandler.SendGameCautiousMapMovementMessage (Client, moveKeys, actorMoving);
                else
                    ContextHandler.SendGameMapMovementMessage (Client, moveKeys, actorMoving);

                BasicHandler.SendBasicNoOperationMessage (Client);
            }
            if (map.Prism != null) {
                PrismHandler.SendPrismsListUpdateMessage (Client, map.Prism, map.Prism.Alliance.Id == Guild?.Alliance?.Id);
            }
            //if (map.Zaap != null && !KnownZaaps.Contains(map))
            //  DiscoverZaap(map);

            if (MustBeJailed () && !IsInJail ())
                TeleportToJail ();

            else if (!MustBeJailed () && IsInJail () && !IsGameMaster ())
                Teleport (Breed.GetStartPosition ());

            /*if (IsRiding && !map.Outdoor && ArenaManager.Instance.Arenas.All(x => x.Value.MapId != map.Id))
                Dismount();*/

            ResetCurrentSkill ();

            foreach (var job in Jobs.Where (x => x.IsIndexed)) {
                job.Template.RefreshCrafter (this);
            }

            base.OnEnterMap (map);
        }

        public override bool CanMove () {

            if (PlayerLifeStatus == PlayerLifeStatusEnum.STATUS_TOMBSTONE)
                return false;

            if (Fight?.State == FightState.Placement || Fight?.State == FightState.NotStarted)
                return false;

            return base.CanMove () && !IsDialoging ();
        }

        public override bool IsGonnaChangeZone () => base.IsGonnaChangeZone () || !IsLoggedIn;

        public override bool StartMove (Path movementPath) {
            LeaveDialog (); //Close Dialog && RequestBox when moving

            if (Inventory.IsFull ())
                movementPath.SetWalk ();

            if (!IsFighting () && !MustBeJailed () && IsInJail ()) {
                Teleport (Breed.GetStartPosition ());
                return false;
            }

            if (IsFighting ())
                if (Fighter.IsSlaveTurn ())
                    return Fighter.GetSlave ().StartMove (movementPath);
                else return Fighter.StartMove (movementPath);

            CancelEmote (false);

            return base.StartMove (movementPath);
        }

        public override bool StopMove () => IsFighting () ? Fighter.StopMove () : base.StopMove ();

        public override bool MoveInstant (ObjectPosition destination) => IsFighting () ? Fighter.MoveInstant (destination) : base.MoveInstant (destination);

        public override bool StopMove (ObjectPosition currentObjectPosition) => IsFighting () ? Fighter.StopMove (currentObjectPosition) : base.StopMove (currentObjectPosition);

        public override bool Teleport (MapNeighbour mapNeighbour) {
            var success = base.Teleport (mapNeighbour);

            if (!success)
                switch (Account.Lang) {
                    case "fr":
                        SendServerMessage ("Transition de carte inconnue");
                        break;
                    case "es":
                        SendServerMessage ("Transici�n de mapa desconocida");
                        break;
                    case "en":
                        SendServerMessage ("Unknown map transition");
                        break;
                    default:
                        SendServerMessage ("Transi��o de mapa desconhecido");
                        break;
                }

            return success;
        }

        #region Jail

        private readonly int[] JAILS_MAPS = { 105121026, 105119744, 105120002 };
        private readonly int[][] JAILS_CELLS = { new [] { 179, 445, 184, 435 }, new [] { 314 }, new [] { 300 } };

        public bool TeleportToJail () {
            var random = new AsyncRandom ();

            var mapIndex = random.Next (0, JAILS_MAPS.Length);
            var cellIndex = random.Next (0, JAILS_CELLS[mapIndex].Length);

            var map = World.Instance.GetMap (JAILS_MAPS[mapIndex]);

            if (map == null) {
                logger.Error ("Cannot find jail map {0}", JAILS_MAPS[mapIndex]);
                return false;
            }

            var cell = map.Cells[JAILS_CELLS[mapIndex][cellIndex]];

            Teleport (new ObjectPosition (map, cell), false);

            return true;
        }

        public bool MustBeJailed () {
            return Client.Account.IsJailed && (Client.Account.BanEndDate == null || Client.Account.BanEndDate > DateTime.Now);
        }

        public bool IsInJail () {
            return JAILS_MAPS.Contains (Map.Id);
        }

        #endregion Jail

        protected override void OnTeleported (ObjectPosition position) {
            base.OnTeleported (position);

            UpdateRegenedLife ();

            if (Dialog != null)
                Dialog.Close ();
        }

        public override bool CanChangeMap () => base.CanChangeMap () && !IsFighting () && !Account.IsJailed;

        #endregion Move
        public void OnKoh () {
            KingOfHill?.Invoke (this);
        }

        public void OnKoHRevive () {
            KoHRevive?.Invoke (this);
        }
        #region Dialog

        public void DisplayNotification (string text, NotificationEnum notification = NotificationEnum.INFORMATION) {
            Client.Send (new NotificationByServerMessage ((ushort) notification, new [] { text }, true));
        }

        public void DisplayNotification (NotificationEnum notification, params object[] parameters) {
            Client.Send (new NotificationByServerMessage ((ushort) notification, parameters.Select (entry => entry.ToString ()), true));
        }

        public void DisplayNotification (Notification notification) {
            notification.Display ();
        }

        public void LeaveDialog () {
            if (IsInRequest ())
                CancelRequest ();

            if (IsDialoging ())
                Dialog.Close ();
        }

        public void ReplyToNpc (short replyId) {
            if (!IsTalkingWithNpc ())
                return;

            ((NpcDialog) Dialog).Reply (replyId);
        }

        public void AcceptRequest () {
            if (!IsInRequest ())
                return;

            if (RequestBox.Target == this)
                RequestBox.Accept ();
        }

        public void DenyRequest () {
            if (!IsInRequest ())
                return;

            if (RequestBox.Target == this)
                RequestBox.Deny ();
        }

        public void CancelRequest () {
            if (!IsInRequest ())
                return;

            if (IsRequestSource ())
                RequestBox.Cancel ();
            else if (IsRequestTarget ())
                DenyRequest ();
        }

        #endregion Dialog

        #region Party

        public void Invite (Character target, PartyTypeEnum type, bool force = false) {
            var created = false;
            Party party;
            if (!IsInParty (type)) {
                party = PartyManager.Instance.Create (type);

                if (!EnterParty (party))
                    return;

                created = true;
            } else party = GetParty (type);
            PartyJoinErrorEnum error;
            if (!party.CanInvite (target, out error, this)) {
                PartyHandler.SendPartyCannotJoinErrorMessage (target.Client, party, error);
                if (created)
                    LeaveParty (party);

                return;
            }

            if (target.m_partyInvitations.ContainsKey (party.Id)) {
                if (created)
                    LeaveParty (party);

                return; // already invited
            }

            var invitation = new PartyInvitation (party, this, target);
            target.m_partyInvitations.Add (party.Id, invitation);

            party.AddGuest (target);

            if (force)
                invitation.Accept ();
            else
                invitation.Display ();
        }

        public PartyInvitation GetInvitation (int id) {
            return m_partyInvitations.ContainsKey (id) ? m_partyInvitations[id] : null;
        }

        public bool RemoveInvitation (PartyInvitation invitation) {
            return m_partyInvitations.Remove (invitation.Party.Id);
        }

        public void DenyAllInvitations () {
            foreach (var partyInvitation in m_partyInvitations.ToArray ()) {
                partyInvitation.Value.Deny ();
            }
        }

        public void DenyAllInvitations (PartyTypeEnum type) {
            foreach (var partyInvitation in m_partyInvitations.Where (x => x.Value.Party.Type == type).ToArray ()) {
                partyInvitation.Value.Deny ();
            }
        }

        public void DenyAllInvitations (Party party) {
            foreach (var partyInvitation in m_partyInvitations.Where (x => x.Value.Party == party).ToArray ()) {
                partyInvitation.Value.Deny ();
            }
        }

        public bool EnterParty (Party party) {
            if (IsInParty (party.Type))
                LeaveParty (GetParty (party.Type));

            if (m_partyInvitations.ContainsKey (party.Id))
                m_partyInvitations.Remove (party.Id);

            DenyAllInvitations (party.Type);
            UpdateRegenedLife ();

            if (party.Disbanded)
                return false;

            SetParty (party);
            party.MemberRemoved += OnPartyMemberRemoved;
            party.PartyDeleted += OnPartyDeleted;

            if (party.IsMember (this))
                return false;

            if (party.PromoteGuestToMember (this))
                return true;

            // if fails to enter
            party.MemberRemoved -= OnPartyMemberRemoved;
            party.PartyDeleted -= OnPartyDeleted;
            ResetParty (party.Type);

            return false;
        }

        public void LeaveParty (Party party) {
            if (!IsInParty (party.Id) || !party.CanLeaveParty (this))
                return;

            party.MemberRemoved -= OnPartyMemberRemoved;
            party.PartyDeleted -= OnPartyDeleted;
            party.RemoveMember (this);
            ResetParty (party.Type);
        }

        private void OnPartyMemberRemoved (Party party, Character member, bool kicked) {
            if (m_followedCharacter == member)
                UnfollowMember ();

            if (member != this)
                return;

            party.MemberRemoved -= OnPartyMemberRemoved;
            party.PartyDeleted -= OnPartyDeleted;

            ResetParty (party.Type);
        }

        private void OnPartyDeleted (Party party) {
            party.MemberRemoved -= OnPartyMemberRemoved;
            party.PartyDeleted -= OnPartyDeleted;

            ResetParty (party.Type);
        }

        public void FollowMember (Character character) {
            if (m_followedCharacter != null)
                UnfollowMember ();

            m_followedCharacter = character;
            character.EnterMap += OnFollowedMemberEnterMap;

            PartyHandler.SendPartyFollowStatusUpdateMessage (Client, Party, true, character.Id);
            CompassHandler.SendCompassUpdatePartyMemberMessage (Client, character, true);
        }
        public void FollowSpousee (Character character) {
            if (followforspouse == true)
                StopFollowSpouse ();

            followforspouse = true;
            FollowSpouse (character);
            character.EnterMap += UpdateFollowSpouse;

            //PartyHandler.SendPartyFollowStatusUpdateMessage(Client, Party, true, character.Id);
            //CompassHandler.SendCompassUpdatePartyMemberMessage(Client, character, true);
            character.Client.Send (new CompassUpdatePartyMemberMessage ((sbyte) CompassTypeEnum.COMPASS_TYPE_SPOUSE, new MapCoordinates ((short) Map.Position.X, (short) Map.Position.Y), (ulong) Id, true));

        }
        public void StopFollowSpouse () {
            if (followforspouse == false)
                return;
            var spouse = World.Instance.GetCharacter (CurrentSpouse);
            if (spouse != null)
                StopFollowSpouse (spouse);
            EnterMap -= UpdateFollowSpouse;

            //PartyHandler.SendPartyFollowStatusUpdateMessage(Client, Party, true, 0);
            //CompassHandler.SendCompassUpdatePartyMemberMessage(Client, m_followedCharacter, false);

            // m_followedCharacter = null;

            if (spouse != null)
                spouse.Client.Send (new CompassUpdatePartyMemberMessage ((sbyte) CompassTypeEnum.COMPASS_TYPE_SPOUSE, new MapCoordinates ((short) Map.Position.X, (short) Map.Position.Y), (ulong) Id, false));
            followforspouse = false;
        }

        public void UnfollowMember () {
            if (m_followedCharacter == null)
                return;

            m_followedCharacter.EnterMap -= OnFollowedMemberEnterMap;

            PartyHandler.SendPartyFollowStatusUpdateMessage (Client, Party, true, 0);
            CompassHandler.SendCompassUpdatePartyMemberMessage (Client, m_followedCharacter, false);

            m_followedCharacter = null;
        }

        private void OnFollowedMemberEnterMap (RolePlayActor actor, Map map) {
            Character character = actor as Character;
            if (actor == null)
                return;

            CompassHandler.SendCompassUpdatePartyMemberMessage (Client, character, true);
        }

        #endregion Party

        #region Quest

        private List<Quest> m_quests = new List<Quest> ();

        public ReadOnlyCollection<Quest> Quests => m_quests.AsReadOnly ();

        public void LoadQuests () {
            var database = QuestManager.Instance.Database;

            m_quests = database.Query<QuestRecord> (string.Format (QuestRecordRelator.FetchByOwner, Id)).Select (x => new Quest (this, x)).ToList ();
        }

        public void StartQuest (int questStepId) {
            var step = QuestManager.Instance.GetQuestStep (questStepId);

            if (step == null)
                throw new Exception ($"Step {questStepId} not found");

            StartQuest (step);
        }

        public void StartQuest (QuestStepTemplate questStep) {
            //var quest = m_quests.FirstOrDefault(x => x.Template.Steps.Contains(questStep));
            var quest = m_quests?.FirstOrDefault (x => x.Template.StepIds.Contains (questStep.Id));
            if (quest == null) {
                quest = new Quest (this, questStep);
                m_quests.Add (quest);
            } else {
                quest.ChangeQuestStep (questStep);
            }
        }
        public Quest HaveQuest (QuestStepTemplate questStep) {
            //var quest = m_quests.FirstOrDefault(x => x.Template.Steps.Contains(questStep));
            return m_quests.FirstOrDefault (x => x.Finished? x.Template.StepIds.Contains (questStep.Id) : x.CurrentStep.Id == questStep.Id);

        }

        public List<short> getFinishedQuests () {
            List<short> finishedQuests = new List<short> ();

            var listOfFinishedQuests = m_quests.Where (m => m.Finished == true).ToList ();

            if (listOfFinishedQuests.Count > 0) {
                finishedQuests = listOfFinishedQuests.Select (x => (short) x.Id).ToList ();
            }

            return finishedQuests;
        }

        //public List<short> getFinishedQuestsRepetitions()
        //{
        //    List<short> finishedQuests = new List<short>();

        //    var listOfFinishedQuests = m_quests.Where(m => m.Finished == true).ToList();

        //    if (listOfFinishedQuests.Count > 0)
        //    {
        //        finishedQuests = listOfFinishedQuests.Select(x => (short)x.Id).Count().ToList();
        //    }

        //    return finishedQuests;
        //}

        #endregion

        #region Fight

        public delegate void CharacterContextChangedHandler (Character character, bool inFight);

        public event CharacterContextChangedHandler ContextChanged;

        public delegate void CharacterFightEndedHandler (Character character, CharacterFighter fighter);

        public event CharacterFightEndedHandler FightEnded;

        public delegate void CharacterDiedHandler (Character character);

        public event CharacterDiedHandler Died;

        private void OnDied () {
            /*  var energylost = (short)(10 * Level);

              if (SuperArea.Id == 5) //Dimensions divines
                  energylost *= 2;

              Energy -= energylost;
              */
            if (!IsGhost ()) {
                var dest = GetSpawnPoint () ?? Breed.GetStartPosition ();

                NextMap = dest.Map;
                Cell = dest.Cell ?? dest.Map.GetRandomFreeCell ();
                Direction = dest.Direction;
            }

            Stats.Health.DamageTaken = (Stats.Health.TotalMax - 1);

            Died?.Invoke (this);
        }

        private void OnFightEnded (CharacterFighter fighter) {
            FightEnded?.Invoke (this, fighter);

        }
        public ushort GetGrave () {
            ushort result;
            switch ((int) BreedId) {
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
        private void OnCharacterContextChanged (bool inFight) {
            ContextChanged?.Invoke (this, inFight);
        }

        public void OnCharacterContextReady (int mapId) {

        }

        public FighterRefusedReasonEnum CanRequestFight (Character target) {

            if (!target.IsInWorld || target.IsFighting () || target.IsSpectator () || target.IsBusy () || !target.IsAvailable (this, false))
                return FighterRefusedReasonEnum.OPPONENT_OCCUPIED;

            if (!IsInWorld || IsFighting () || IsSpectator () || IsBusy ())
                return FighterRefusedReasonEnum.IM_OCCUPIED;

            if (target == this)
                return FighterRefusedReasonEnum.FIGHT_MYSELF;

            if (target.Map != Map || !Map.AllowChallenge)
                return FighterRefusedReasonEnum.WRONG_MAP;

            if (IsGhost ())
                return FighterRefusedReasonEnum.GHOST_REFUSED;

            return FighterRefusedReasonEnum.FIGHTER_ACCEPTED;
        }
        public FighterRefusedReasonEnum CanAgressAvA (Character target) {
            FighterRefusedReasonEnum result;
            if (target == this) {
                result = FighterRefusedReasonEnum.FIGHT_MYSELF;
            } else {
                if (!target.AvAActived || !AvAActived) {

                    result = FighterRefusedReasonEnum.INSUFFICIENT_RIGHTS;
                } else {
                    if (!target.IsInWorld || target.IsFighting () || target.IsSpectator ()) //|| target.IsBusy())
                    {
                        result = FighterRefusedReasonEnum.OPPONENT_OCCUPIED;
                    } else {
                        if (!IsInWorld || IsFighting () || IsSpectator () || IsBusy ()) {
                            result = FighterRefusedReasonEnum.IM_OCCUPIED;
                        } else {
                            if (target.AvaState != AggressableStatusEnum.AvA_ENABLED_AGGRESSABLE ||
                                AvaState != AggressableStatusEnum.AvA_ENABLED_AGGRESSABLE) {
                                result = FighterRefusedReasonEnum.INSUFFICIENT_RIGHTS;
                            } else {
                                if (target.Guild?.Alliance?.Id == Guild?.Alliance?.Id && target.Guild?.Alliance?.Id != null && Guild?.Alliance?.Id != null) {
                                    result = FighterRefusedReasonEnum.WRONG_ALLIANCE;
                                } else {
                                    if (!SubArea.HasPrism || SubArea.Prism.State != PrismStateEnum.PRISM_STATE_VULNERABLE) {
                                        result = FighterRefusedReasonEnum.WRONG_MAP;
                                    } else {
                                        if (target.Client.IP == Client.IP) {
                                            result = FighterRefusedReasonEnum.MULTIACCOUNT_NOT_ALLOWED;
                                        } else {
                                            if (target.Client.Account.Email == Client.Account.Email) {
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
        public bool CanBattlefield (Character target) {
            if (target == this)
                return false;

            if (!target.battleFieldOn)
                return false;

            if (!target.IsInWorld || target.IsFighting () || target.IsSpectator () || target.IsBusy () || !target.IsAvailable (this, false))
                return false;

            if (string.Equals (target.Client.IP, Client.IP) && !IsGameMaster ())
                return false;

            if (Math.Abs (Level - target.Level) > 40)
                return false;

            if (IsGhost () || target.IsGhost ())
                return false;
            return true;
        }

        public FighterRefusedReasonEnum CanAgress (Character target, bool bypassCheck = false) {
            if (target.Client.IP == Client.IP)
                return FighterRefusedReasonEnum.MULTIACCOUNT_NOT_ALLOWED;

            if (target == this)
                return FighterRefusedReasonEnum.FIGHT_MYSELF;

            if (!target.PvPEnabled || !PvPEnabled)
                return FighterRefusedReasonEnum.INSUFFICIENT_RIGHTS;

            if (!target.IsInWorld || target.IsFighting () || target.IsSpectator ()) //|| target.IsBusy())
                return FighterRefusedReasonEnum.OPPONENT_OCCUPIED;

            if (!bypassCheck && (!IsInWorld || IsFighting () || IsSpectator () || IsBusy ()))
                return FighterRefusedReasonEnum.IM_OCCUPIED;

            if (AlignmentSide <= AlignmentSideEnum.ALIGNMENT_NEUTRAL || target.AlignmentSide <= AlignmentSideEnum.ALIGNMENT_NEUTRAL)
                return FighterRefusedReasonEnum.WRONG_ALIGNMENT;

            if (target.AlignmentSide == AlignmentSide)
                return FighterRefusedReasonEnum.WRONG_ALIGNMENT;
            if (AvAActived && SubArea.HasPrism) {
                if (SubArea.Prism.State == PrismStateEnum.PRISM_STATE_VULNERABLE)
                    return (FighterRefusedReasonEnum) AvaState;
                //AvA_ENABLED_NON_AGGRESSABLE When full of alliances so can see but can't be aggresed!
            }
            if (!bypassCheck && (target.Map != Map || !Map.AllowAggression))
                return FighterRefusedReasonEnum.WRONG_MAP;

            if (string.Equals (target.Client.IP, Client.IP) && !IsGameMaster ())
                return FighterRefusedReasonEnum.MULTIACCOUNT_NOT_ALLOWED;

            if (Math.Abs (Level) < 200 && Math.Abs (target.Level) < 200) {
                if (Math.Abs (Level - target.Level) > 20)
                    return FighterRefusedReasonEnum.INSUFFICIENT_RIGHTS;
            } else if (Math.Abs (Level) < 200 || Math.Abs (target.Level) < 200) {
                if (Math.Abs (Level - target.Level) > 20)
                    return FighterRefusedReasonEnum.INSUFFICIENT_RIGHTS;
            } else {
                if (Math.Abs (Level - target.Level) > 55)
                    return FighterRefusedReasonEnum.INSUFFICIENT_RIGHTS;
            }
            if (IsGhost () || target.IsGhost ())
                return FighterRefusedReasonEnum.GHOST_REFUSED;

            return FighterRefusedReasonEnum.FIGHTER_ACCEPTED;
        }

        public FighterRefusedReasonEnum CanAttack (TaxCollectorNpc target) {
            if (GuildMember != null && target.IsTaxCollectorOwner (GuildMember))
                return FighterRefusedReasonEnum.WRONG_GUILD;

            if (IsBusy () || IsFighting () || IsSpectator () || !IsInWorld)
                return FighterRefusedReasonEnum.IM_OCCUPIED;

            if (target.IsBusy () || target.IsFighting || !target.IsInWorld)
                return FighterRefusedReasonEnum.OPPONENT_OCCUPIED;

            if (target.Map != Map)
                return FighterRefusedReasonEnum.WRONG_MAP;

            if (IsGhost ())
                return FighterRefusedReasonEnum.GHOST_REFUSED;

            return FighterRefusedReasonEnum.FIGHTER_ACCEPTED;
        }
        public FighterRefusedReasonEnum CanAttack (PrismNpc target) {

            FighterRefusedReasonEnum result;
            if (Guild?.Alliance != null && target.IsPrismOwner (Guild))
                result = FighterRefusedReasonEnum.WRONG_ALLIANCE;

            else {

                if (target.IsBusy ())
                    result = FighterRefusedReasonEnum.OPPONENT_OCCUPIED;
                else {

                    result = target.Map != Map ?
                        FighterRefusedReasonEnum.WRONG_MAP :
                        FighterRefusedReasonEnum.FIGHTER_ACCEPTED;
                }
            }
            return result;
        }
        public FighterRefusedReasonEnum CanAttack (MonsterGroup group) {
            if (IsFighting () || IsSpectator () || !IsInWorld)
                return FighterRefusedReasonEnum.IM_OCCUPIED;

            if (!group.IsInWorld)
                return FighterRefusedReasonEnum.OPPONENT_OCCUPIED;

            if (group.Map != Map)
                return FighterRefusedReasonEnum.WRONG_MAP;

            if (IsGhost ())
                return FighterRefusedReasonEnum.GHOST_REFUSED;

            return FighterRefusedReasonEnum.FIGHTER_ACCEPTED;
        }

        public CharacterFighter CreateFighter (FightTeam team) {
            if (IsFighting () || IsSpectator () || !IsInWorld)
                throw new Exception (string.Format ("{0} is already in a fight", this));

            NextMap = Map; // we do not leave the map
            Map.Leave (this);
            StopRegen ();

            if (IsInMovement)
                StopMove ();

            ContextHandler.SendGameContextDestroyMessage (Client);
            ContextHandler.SendGameContextCreateMessage (Client, 2);
            RefreshActor ();
            Fighter = new CharacterFighter (this, team);

            ContextHandler.SendGameFightStartingMessage (Client, team.Fight.FightType, Fight.DefendersTeam.Leader == null ? 0 : Fight.DefendersTeam.Leader.Id, team.Leader == null ? 0 : team.Leader.Id);

            if (IsPartyLeader () && Party.RestrictFightToParty && team.Fighters.Count == 0 && !team.IsRestrictedToParty)
                team.ToggleOption (FightOptionsEnum.FIGHT_OPTION_SET_TO_PARTY_ONLY);

            OnCharacterContextChanged (true);

            return Fighter;
        }
        public CompanionActor CreateCompanion (FightTeam team) {
            CompanionActor result;

            CompanionRecord companion = null;
            var Listcompanion = Singleton<CompanionsManager>.Instance.GetCompanionById (Inventory.GetItems (CharacterInventoryPositionEnum.INVENTORY_POSITION_COMPANION).First ().Template.Id);
            if (Listcompanion.Count () != 0) {
                companion = Listcompanion.First ();
            }
            if (companion != null && (team.Fight is FightPvM || team.Fight is FightDuel)) {
                List<Spell> spellsCompanion = new List<Spell> ();
                foreach (var spell in companion.SpellsId) {

                    spellsCompanion.Add (new Spell (Singleton<SpellManager>.Instance.GetSpellTemplate (spell), 1));
                }

                Companion = new CompanionActor (this, team, ActorLook.Parse (companion.Look), spellsCompanion, (byte) companion.Id, Fight.GetNextContextualId ());
                Companion.NextMap = Map;
                result = Companion;
            } else {
                result = null;
            }
            OnCharacterContextChanged (true);
            return result;
        }
        public FightSpectator CreateSpectator (IFight fight) {
            if (IsFighting () || IsSpectator () || !IsInWorld)
                throw new Exception (string.Format ("{0} is already in a fight", this));

            if (!fight.CanSpectatorJoin (this))
                throw new Exception (string.Format ("{0} cannot join fight in spectator", this));

            NextMap = Map; // we do not leave the map
            Map.Leave (this);
            StopRegen ();

            if (IsInMovement)
                StopMove ();

            ContextHandler.SendGameContextDestroyMessage (Client);
            ContextHandler.SendGameContextCreateMessage (Client, 2);

            ContextHandler.SendGameFightStartingMessage (Client, fight.FightType, fight.ChallengersTeam.Leader.Id, fight.DefendersTeam.Leader.Id);

            Spectator = new FightSpectator (this, fight);

            OnCharacterContextChanged (true);

            return Spectator;
        }

        private CharacterFighter RejoinFightAfterDisconnection (CharacterFighter oldFighter) {
            Map.Leave (this);
            Map = oldFighter.Map;
            NextMap = oldFighter.Character.NextMap;

            StopRegen ();

            ContextHandler.SendGameContextDestroyMessage (Client);
            ContextHandler.SendGameContextCreateMessage (Client, 2);
            ContextRoleplayHandler.SendCurrentMapMessage (Client, Map.Id);
            ContextRoleplayHandler.SendMapComplementaryInformationsDataMessage (Client);

            oldFighter.RestoreFighterFromDisconnection (this);
            Fighter = oldFighter;

            ContextHandler.SendGameFightStartingMessage (Client, Fighter.Fight.FightType, Fighter.Fight.ChallengersTeam.Leader.Id,
                Fighter.Fight.DefendersTeam.Leader.Id);

            Fighter.Fight.RejoinFightFromDisconnection (Fighter);
            OnCharacterContextChanged (true);

            foreach (var challenge in Fight.Challenges) {
                ContextHandler.SendChallengeInfoMessage (Client, challenge);

                if (challenge.Status != ChallengeStatusEnum.RUNNING)
                    ContextHandler.SendChallengeResultMessage (Client, challenge);
            }

            return Fighter;
        }

        /// <summary>
        /// Rejoin the map after a fight
        /// </summary>
        public void RejoinMap () {
            if (!IsFighting () && !IsSpectator ())
                return;

            if (Fighter != null)
                OnFightEnded (Fighter);

            if (GodMode)
                Stats.Health.DamageTaken = 0;
            else if (Fighter != null && (Fighter.HasLeft () && !Fighter.IsDisconnected || Fight.Losers == Fighter.Team) && !Fight.IsDeathTemporarily)
                OnDied ();

            if (!Client.Connected)
                return;

            Fighter = null;
            Spectator = null;

            ContextHandler.SendGameContextDestroyMessage (Client);
            ContextHandler.SendGameContextCreateMessage (Client, 1);
            RefreshStats ();

            OnCharacterContextChanged (false);
            StartRegen ();

            if (Map == null)
                return;

            if (IsLoggedIn) {
                if (!NextMap.Area.IsRunning)
                    NextMap.Area.Start ();

                NextMap.Area.ExecuteInContext (() => {
                    if (IsLoggedIn) {
                        LastMap = Map;
                        Map = NextMap;
                        Map.Enter (this);
                        NextMap = null;
                    }
                });

                RefreshActor ();
                Map.Refresh (this);
            } else
                SaveLater (); // if disconnected in fight we must save the change at the end of the fight 

        }

        #endregion Fight

        #region Regen

        public bool IsRegenActive () {
            return RegenStartTime.HasValue;
        }

        public void StartRegen () {
            StartRegen ((byte) (10f / Rates.RegenRate));
        }

        public void StartRegen (byte timePerHp) {
            if (IsRegenActive ())
                StopRegen ();

            if (PlayerLifeStatus == PlayerLifeStatusEnum.STATUS_TOMBSTONE)
                return;

            RegenStartTime = DateTime.Now;
            RegenSpeed = timePerHp;

            CharacterHandler.SendLifePointsRegenBeginMessage (Client, (byte) RegenSpeed);
        }

        public void StopRegen () {
            if (!IsRegenActive ())
                return;

            var regainedLife = (int) Math.Floor ((DateTime.Now - RegenStartTime).Value.TotalSeconds / (RegenSpeed / 10f));

            if (LifePoints + regainedLife > MaxLifePoints)
                regainedLife = MaxLifePoints - LifePoints;

            if (regainedLife > 0) {
                Stats.Health.DamageTaken -= regainedLife;
            }

            CharacterHandler.SendLifePointsRegenEndMessage (Client, regainedLife);

            RegenStartTime = null;
            RegenSpeed = 0;
            OnLifeRegened (regainedLife);
        }

        public void UpdateRegenedLife () {
            if (!IsRegenActive ())
                return;

            var regainedLife = (int) Math.Floor ((DateTime.Now - RegenStartTime).Value.TotalSeconds / (RegenSpeed / 10f));

            if (LifePoints + regainedLife > MaxLifePoints)
                regainedLife = MaxLifePoints - LifePoints;

            if (regainedLife > 0) {
                Stats.Health.DamageTaken -= regainedLife;
                CharacterHandler.SendUpdateLifePointsMessage (Client);
            }

            RegenStartTime = DateTime.Now;

            OnLifeRegened (regainedLife);
        }

        #endregion Regen

        #region Zaaps

        private ObjectPosition m_spawnPoint;

        public List<Map> KnownZaaps {
            get { return Record.KnownZaaps; }
        }

        public void DiscoverZaap (Map map) {
            if (!KnownZaaps.Contains (map))
                KnownZaaps.Add (map);

            BasicHandler.SendTextInformationMessage (Client, TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 24);
            // new zaap
        }

        public void SetSpawnPoint (Map map) {
            Record.SpawnMap = map;
            m_spawnPoint = null;

            BasicHandler.SendTextInformationMessage (Client, TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 6);
            // pos saved

            InteractiveHandler.SendZaapRespawnUpdatedMessage (Client);
        }

        public ObjectPosition GetSpawnPoint () {
            if (Record.SpawnMap == null)
                return Breed.GetStartPosition ();

            if (m_spawnPoint != null)
                return m_spawnPoint;

            var map = Record.SpawnMap;

            if (map.Zaap == null)
                return new ObjectPosition (map, map.GetRandomFreeCell (), Direction);

            var cell = map.GetRandomAdjacentFreeCell (map.Zaap.Position.Point);
            var direction = map.Zaap.Position.Point.OrientationTo (new MapPoint (cell));

            return new ObjectPosition (map, cell, direction);
        }

        #endregion Zaaps

        #region Emotes

        private LimitedStack<Pair<Emote, DateTime>> m_playedEmotes = new LimitedStack<Pair<Emote, DateTime>> (5);
        private bool m_cancelEmote;

        public ReadOnlyCollection<EmotesEnum> Emotes => Record.Emotes.AsReadOnly ();

        public override Pair<Emote, DateTime> LastEmoteUsed => !m_cancelEmote && m_playedEmotes.Count > 0 ? m_playedEmotes.Peek () : null;

        private Pair<Emote, DateTime> GetCurrentEmotePair () => LastEmoteUsed != null && (LastEmoteUsed.First.Duration == 0 || LastEmoteUsed.First.Persistancy || (DateTime.Now - LastEmoteUsed.Second) < TimeSpan.FromMilliseconds (LastEmoteUsed.First.Duration)) ?
            LastEmoteUsed :
            null;

        public Emote GetCurrentEmote () => GetCurrentEmotePair ()?.First;

        public bool CancelEmote (bool send = true) {
            var emote = GetCurrentEmote ();

            if (emote == null)
                return false;

            m_cancelEmote = true;
            UpdateLook (emote, false, false);

            if (send)
                ContextRoleplayHandler.SendEmotePlayMessage (CharacterContainer.Clients, this, 0);

            RefreshActor ();

            return true;
        }

        public bool HasEmote (EmotesEnum emote) => Emotes.Contains (emote);

        public void AddEmote (EmotesEnum emote) {
            if (HasEmote (emote))
                return;

            Record.Emotes.Add (emote);
            ContextRoleplayHandler.SendEmoteAddMessage (Client, (sbyte) emote);
        }

        public bool RemoveEmote (EmotesEnum emote) {
            var result = Record.Emotes.Remove (emote);

            if (result) {

                //var shortcut = Shortcuts.EmoteShortcuts.FirstOrDefault(x => x.Value.EmoteId == (int)emote);
                foreach (var b in Shortcuts.EmoteShortcuts.Where (x => x.Value.EmoteId == (sbyte) emote).ToList ()) {
                    if (b.Value != null)
                        Shortcuts.RemoveShortcut (ShortcutBarEnum.GENERAL_SHORTCUT_BAR, b.Key);
                }

                ContextRoleplayHandler.SendEmoteRemoveMessage (Client, (sbyte) emote);
            }

            return result;
        }

        public void PlayEmote (EmotesEnum emoteId, bool force = false) {
            var emote = ChatManager.Instance.GetEmote ((int) emoteId);

            if (emote == null) {
                ContextRoleplayHandler.SendEmotePlayErrorMessage (Client, (sbyte) emoteId);
                return;
            }

            if (!HasEmote (emoteId) && !force) {
                ContextRoleplayHandler.SendEmotePlayErrorMessage (Client, (sbyte) emoteId);
                return;
            }

            var currentEmote = GetCurrentEmote ();

            if (currentEmote != null) {
                CancelEmote ();

                if (currentEmote == emote) {
                    return;
                }
            }

            m_cancelEmote = false;
            m_playedEmotes.Push (new Pair<Emote, DateTime> (emote, DateTime.Now));
            UpdateLook (emote, true, false);

            RefreshActor ();

            ContextRoleplayHandler.SendEmotePlayMessage (CharacterContainer.Clients, this, emoteId);
        }

        #endregion Emotes

        #region FinishMove

        public ReadOnlyCollection<FinishMove> FinishMoves => Record.FinishMoves.AsReadOnly ();

        public bool HasFinishMove (int finishMove) => FinishMoves.Any (x => x.Id == finishMove);

        public void AddFinishMove (int finishMove) {
            if (HasFinishMove (finishMove))
                return;

            Record.FinishMoves.Add (new FinishMove (finishMove, false));
        }

        public bool RemoveFinishMove (int finishMove) {
            if (HasFinishMove (finishMove))
                return false;

            return Record.FinishMoves.Remove (GetFinishMove (finishMove));
        }

        public FinishMove GetFinishMove (int finishMove) {
            return Record.FinishMoves.FirstOrDefault (x => x.Id == finishMove);
        }

        public FinishMoveInformations[] GetFinishMovesInformations () {
            return FinishMoves.Select (x => x.GetInformations ()).ToArray ();
        }

        #endregion FinishMove

        #region Friend & Ennemies

        public FriendsBook FriendsBook {
            get;
            private set;
        }

        #endregion Friend & Ennemies

        #region Merchant

        private Merchant m_merchantToSpawn;

        public bool CanEnableMerchantMode (bool sendError = true) {
            if (MerchantBag.Count == 0) {
                if (sendError)
                    SendInformationMessage (TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 23);
                return false;
            }

            if (!Map.AllowHumanVendor) {
                if (sendError)
                    SendInformationMessage (TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 237);

                return false;
            }

            if (Map.IsMerchantLimitReached ()) {
                if (sendError)
                    SendInformationMessage (TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 25, Map.MaxMerchantsPerMap);
                return false;
            }

            if (!Map.IsCellFree (Cell.Id, this)) {
                if (sendError)
                    SendInformationMessage (TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 24);
                return false;
            }

            if (Kamas >= MerchantBag.GetMerchantTax ())
                return true;

            if (sendError)
                SendInformationMessage (TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 76);

            return false;
        }

        public bool EnableMerchantMode () {
            if (!CanEnableMerchantMode ())
                return false;

            m_merchantToSpawn = new Merchant (this);

            Inventory.SubKamas (MerchantBag.GetMerchantTax ());
            MerchantManager.Instance.AddMerchantSpawn (m_merchantToSpawn.Record);
            MerchantManager.Instance.ActiveMerchant (m_merchantToSpawn);
            Client.Disconnect ();

            return true;
        }

        private void CheckMerchantModeReconnection () {
            foreach (var merchant in MerchantManager.Instance.UnActiveMerchantFromAccount (Client.WorldAccount)) {
                merchant.Save (WorldServer.Instance.DBAccessor.Database);

                if (merchant.Record.CharacterId != Id)
                    continue;

                MerchantBag.LoadMerchantBag (merchant.Bag);

                MerchantManager.Instance.RemoveMerchantSpawn (merchant.Record);
            }

            // if the merchant wasn't active
            var record = MerchantManager.Instance.GetMerchantSpawn (Id);
            if (record == null)
                return;

            MerchantManager.Instance.RemoveMerchantSpawn (record);
        }

        #endregion Merchant

        #region Bank

        public Bank Bank {
            get;
            private set;
        }

        #endregion Bank

        #region Drop Items

        public void GetDroppedItem (WorldObjectItem objectItem) {
            if (Inventory.IsFull (objectItem.Item, objectItem.Quantity)) {
                //Vous ne pouvez pas porter autant d'objets.
                SendInformationMessage (TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 285);

                //Le nombre maximum d'objets pour cet inventaire est d�j� atteint.
                SendInformationMessage (TextInformationTypeEnum.TEXT_INFORMATION_POPUP, 6);
                return;
            }

            objectItem.Map.Leave (objectItem);
            Inventory.AddItem (objectItem.Item, objectItem.Effects, objectItem.Quantity);
        }

        public void DropItem (int itemId, int quantity) {
            if (quantity <= 0)
                return;

            var cell = Position.Point.GetAdjacentCells (x => Map.Cells[x].Walkable && Map.IsCellFree (x) && !Map.IsObjectItemOnCell (x)).FirstOrDefault ();
            if (cell == null) {
                //Il n'y a pas assez de place ici.
                SendInformationMessage (TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 145);
                return;
            }

            var item = Inventory.TryGetItem (itemId);
            if (item == null)
                return;

            if (item.IsLinkedToAccount () || item.IsLinkedToPlayer () || item.Template.Id == 20000) //Temporary block orb drop
                return;

            if (item.Stack < quantity) {
                //Vous ne poss�dez pas l'objet en quantit� suffisante.
                SendInformationMessage (TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 252);
                return;
            }

            Inventory.RemoveItem (item, quantity);

            var objectItem = new WorldObjectItem (item.Guid, Map, Map.Cells[cell.CellId], item.Template, item.Effects.Clone (), quantity);

            Map.Enter (objectItem);
        }

        #endregion Drop Items

        #region Debug

        public void ClearHighlight () {
            Client.Send (new DebugClearHighlightCellsMessage ());
        }

        public Color HighlightCell (Cell cell) {
            var rand = new Random ();
            var color = Color.FromArgb (0xFF << 24 | rand.Next (0xFFFFFF));
            HighlightCell (cell, color);

            return color;
        }

        public void HighlightCell (Cell cell, Color color) {
            Client.Send (new DebugHighlightCellsMessage (color.ToArgb () & 16777215, new [] {
                (ushort) cell.Id }));
        }

        public Color HighlightCells (IEnumerable<Cell> cells) {
            var rand = new Random ();
            var color = Color.FromArgb (0xFF << 24 | rand.Next (0xFFFFFF));

            HighlightCells (cells, color);
            return color;
        }

        public void HighlightCells (IEnumerable<Cell> cells, Color color) {
            Client.Send (new DebugHighlightCellsMessage (color.ToArgb () & 16777215, cells.Select (x => (ushort) x.Id)));
        }

        #endregion Debug

        #endregion Actions

        #region Save & Load

        public bool IsLoggedIn {
            get;
            private set;
        }

        public bool IsAccountBlocked {
            get;
            private set;
        }

        public bool IsAuthSynced {
            get;
            set;
        }

        /// <summary>
        ///   Spawn the character on the map. It can be called once.
        /// </summary>
        public void LogIn() {
            Console.WriteLine("Comecou");
            if ((DateTime.Now - loginTime).TotalSeconds <= 15)
                return;
            loginTime = DateTime.Now;
            if (IsInWorld)
                return;
            Console.WriteLine("1");
            /* if (BreedId == (PlayableBreedEnum)18)
             {
                 Record.MandatoryChanges |= (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_BREED;
                 Record.PossibleChanges |= (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_BREED;
                 Record.PossibleChanges |= (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_GENDER;
                 Record.PossibleChanges |= (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_COSMETIC;
                 Record.PossibleChanges |= (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_COLORS;
                 SendSystemMessage(63, false);
                 Client.Disconnect();
             }*/
            if (!IsInFight())
                Task.Factory.StartNewDelayed(1250, () => HavenBagManager.Instance.ExitHavenBag(Client));
            Console.WriteLine("2");
            Task.Factory.StartNewDelayed(1250, () => Client.Send(Client.Character.Map.GetMapComplementaryInformationsDataMessage(Client.Character)));
            CharacterFighter fighter = null;
            Console.WriteLine("3");
            if (Record.LeftFightId != null) {

                var fight = FightManager.Instance.GetFight(Record.LeftFightId.Value);
                Console.WriteLine("4");
                if (fight != null)
                    fighter = fight.GetLeaver(Id);
                Console.WriteLine("5");
            }
            Console.WriteLine("6");
            if (fighter != null && fighter.IsDisconnected) {

                Map.Area.AddMessage(() => {
                    RejoinFightAfterDisconnection(fighter);
                });
                Console.WriteLine("7");
            } else {

                ContextHandler.SendGameContextDestroyMessage(Client);
                Console.WriteLine("8");
                ContextHandler.SendGameContextCreateMessage(Client, 1);
                Console.WriteLine("9");

                RefreshStats();
                Console.WriteLine("10");
                //Map.Area.AddMessage(() =>
                //{
                Map.Enter(this);
                Console.WriteLine("11");
                StartRegen();
                Console.WriteLine("12");

                //});
            }

            World.Instance.Enter(this);
            Console.WriteLine("13");
            m_inWorld = true;
            // SendServerMessage("Liberamos um pacote de atualiza��es acumulativa no site, caso a sua deu erro ou algum problema no launcher, tente baixar e extrair na pasta do Dofus Wolf!");
            //DisplayNotification("Ps: quem estiver com item bugado da forja magica(ex 800 de potencia ou 5k de vit ) e n�o der para algum moderador, aconta ser� banida!");
            Console.WriteLine("14");
            Inventory.CheckItemsCriterias();
            Console.WriteLine("15");
            Inventory.CheckICoinsPVM();
            Console.WriteLine("16");

            Startupactions.StartupManager.Instance.confirmacao.Remove(Account.Id);
            Console.WriteLine("17");
            Task.Factory.StartNewDelayed(1250, () =>
            {
                foreach (var spell in Client.Character.Shortcuts.GetShortcuts(ShortcutBarEnum.SPELL_SHORTCUT_BAR))
                {
                    if (spell is Database.Shortcuts.SpellShortcut)
                        Client.Send(new SpellVariantActivationMessage(((ushort)(spell as Database.Shortcuts.SpellShortcut).SpellId), true));
                }
            });
            Console.WriteLine("17");
            /* switch (Account.Lang) {
                 case "fr":
                     SendServerMessage (Settings.MOTD_fr, Settings.MOTDColor);
                     SendServerMessage ("<center>" + "<font color=\"#FF4040\"><a href=\"https://dfwolf.online/fr/article/40-super-promotion-of-50-off\"><b><u>Promotion des 50% OFF (POUR TEMPS LIMIT�)</u></b></a></font><br><br>" + "<font color=\"#00BFFF\"><a href=\"https://dfwolf.online/fr/article/38-the-wolfpack-prevails\"><b><u>La meute de loups pr�vaut!</u></b></a></font><br><b><u>Mises � jour</b></u>:<font color=\"#FF9700\"><a href=\"https://dfwolf.online/fr/article/39-march-updates\"><b><u>Mises � jour de Mars!</u></b></a></font><br><b><u>Mises � jour</b></u>:<font color=\"#FF9700\"><a href=\"https://dfwolf.online/forum/index.php?/topic/68-atualiza%C3%A7%C3%B5es-11032019\"><b><u>Derni�re mise � jour dans le forum! (Seulement en portugais)</u></b></a></font></center>");
                     DisplayNotification ("<center>" + "<font color=\"#FF4040\"><a href=\"https://dfwolf.online/fr/article/40-super-promotion-of-50-off\"><b><u>Promotion des 50% OFF (POUR TEMPS LIMIT�)</u></b></a></font><br><br>" + "<font color=\"#00BFFF\"><a href=\"https://dfwolf.online/fr/article/38-the-wolfpack-prevails\"><b><u>La meute de loups pr�vaut!</u></b></a></font><br><b><u>Mises � jour</b></u>:<font color=\"#FF9700\"><a href=\"https://dfwolf.online/fr/article/39-march-updates\"><b><u>Mises � jour de Mars!</u></b></a></font><br><b><u>Mises � jour</b></u>:<font color=\"#FF9700\"><a href=\"https://dfwolf.online/forum/index.php?/topic/68-atualiza%C3%A7%C3%B5es-11032019\"><b><u>Derni�re mise � jour dans le forum! (Seulement en portugais)</u></b></a></font></center>");
                     OpenPopup ("<center>" + "<font color=\"#FF4040\"><a href=\"https://dfwolf.online/fr/article/40-super-promotion-of-50-off\"><b><u>Promotion des 50% OFF (POUR TEMPS LIMIT�)</u></b></a></font><br><br>" + "<font color=\"#00BFFF\"><a href=\"https://dfwolf.online/fr/article/38-the-wolfpack-prevails\"><b><u>La meute de loups pr�vaut!</u></b></a></font><br><b><u>Mises � jour</b></u>:<font color=\"#FF9700\"><a href=\"https://dfwolf.online/fr/article/39-march-updates\"><b><u>Mises � jour de Mars!</u></b></a></font><br><b><u>Mises � jour</b></u>:<font color=\"#FF9700\"><a href=\"https://dfwolf.online/forum/index.php?/topic/68-atualiza%C3%A7%C3%B5es-11032019\"><b><u>Derni�re mise � jour dans le forum! (Seulement en portugais)</u></b></a></font></center>", "Wolf Server", 3);
                     break;
                 case "es":
                     SendServerMessage (Settings.MOTD_es, Settings.MOTDColor);
                     SendServerMessage ("<center>" + "<font color=\"#FF4040\"><a href=\"https://dfwolf.online/es/articulo/40-super-promotion-of-50-off\"><b><u>Promoci�n de 50% OFF (POR TIEMPO LIMITADO)</u></b></a></font><br><br>" + "<font color=\"#00BFFF\"><a href=\"https://dfwolf.online/es/articulo/38-the-wolfpack-prevails\"><b><u>�La jaur�a prevalece!</u></b></a></font><br><b><u>Actualizaciones</b></u>:<font color=\"#FF9700\"><a href=\"https://dfwolf.online/es/articulo/39-march-updates\"><b><u>�Actualizaciones de Marzo!</u></b></a></font><br><b><u>Actualizaciones</b></u>:<font color=\"#FF9700\"><a href=\"https://dfwolf.online/forum/index.php?/topic/68-atualiza%C3%A7%C3%B5es-11032019\"><b><u>�Ultima actualizaci�n en el foro! (S�lo en portugu�s)</u></b></a></font></center>");
                     DisplayNotification ("<center>" + "<font color=\"#FF4040\"><a href=\"https://dfwolf.online/es/articulo/40-super-promotion-of-50-off\"><b><u>Promoci�n de 50% OFF (POR TIEMPO LIMITADO)</u></b></a></font><br><br>" + "<font color=\"#00BFFF\"><a href=\"https://dfwolf.online/es/articulo/38-the-wolfpack-prevails\"><b><u>�La jaur�a prevalece!</u></b></a></font><br><b><u>Actualizaciones</b></u>:<font color=\"#FF9700\"><a href=\"https://dfwolf.online/es/articulo/39-march-updates\"><b><u>�Actualizaciones de Marzo!</u></b></a></font><br><b><u>Actualizaciones</b></u>:<font color=\"#FF9700\"><a href=\"https://dfwolf.online/forum/index.php?/topic/68-atualiza%C3%A7%C3%B5es-11032019\"><b><u>�Ultima actualizaci�n en el foro! (S�lo en portugu�s)</u></b></a></font></center>");
                     OpenPopup ("<center>" + "<font color=\"#FF4040\"><a href=\"https://dfwolf.online/es/articulo/40-super-promotion-of-50-off\"><b><u>Promoci�n de 50% OFF (POR TIEMPO LIMITADO)</u></b></a></font><br><br>" + "<font color=\"#00BFFF\"><a href=\"https://dfwolf.online/es/articulo/38-the-wolfpack-prevails\"><b><u>�La jaur�a prevalece!</u></b></a></font><br><b><u>Actualizaciones</b></u>:<font color=\"#FF9700\"><a href=\"https://dfwolf.online/es/articulo/39-march-updates\"><b><u>�Actualizaciones de Marzo!</u></b></a></font><br><b><u>Actualizaciones</b></u>:<font color=\"#FF9700\"><a href=\"https://dfwolf.online/forum/index.php?/topic/68-atualiza%C3%A7%C3%B5es-11032019\"><b><u>�Ultima actualizaci�n en el foro! (S�lo en portugu�s)</u></b></a></font></center>", "Wolf Server", 3);
                     break;
                 case "en":
                     SendServerMessage (Settings.MOTD_en, Settings.MOTDColor);
                     SendServerMessage ("<center>" + "<font color=\"#FF4040\"><a href=\"https://dfwolf.online/en/article/40-super-promotion-of-50-off\"><b><u>Promotion of 50% OFF (FOR LIMITED TIME)</u></b></a></font><br><br>" + "<font color=\"#00BFFF\"><a href=\"https://dfwolf.online/en/article/38-the-wolfpack-prevails\"><b><u>The wolfpack prevails!</u></b></a></font><br><b><u>Updates</b></u>:<font color=\"#FF9700\"><a href=\"https://dfwolf.online/en/article/39-march-updates\"><b><u>March Updates!</u></b></a></font><br><b><u>Updates</b></u>:<font color=\"#FF9700\"><a href=\"https://dfwolf.online/forum/index.php?/topic/68-atualiza%C3%A7%C3%B5es-11032019\"><b><u>Last update in the Forum! (Only in Portuguese)</u></b></a></font></center>");
                     DisplayNotification ("<center>" + "<font color=\"#FF4040\"><a href=\"https://dfwolf.online/en/article/40-super-promotion-of-50-off\"><b><u>Promotion of 50% OFF (FOR LIMITED TIME)</u></b></a></font><br><br>" + "<font color=\"#00BFFF\"><a href=\"https://dfwolf.online/en/article/38-the-wolfpack-prevails\"><b><u>The wolfpack prevails!</u></b></a></font><br><b><u>Updates</b></u>:<font color=\"#FF9700\"><a href=\"https://dfwolf.online/en/article/39-march-updates\"><b><u>March Updates!</u></b></a></font><br><b><u>Updates</b></u>:<font color=\"#FF9700\"><a href=\"https://dfwolf.online/forum/index.php?/topic/68-atualiza%C3%A7%C3%B5es-11032019\"><b><u>Last update in the Forum! (Only in Portuguese)</u></b></a></font></center>");
                     OpenPopup ("<center>" + "<font color=\"#FF4040\"><a href=\"https://dfwolf.online/en/article/40-super-promotion-of-50-off\"><b><u>Promotion of 50% OFF (FOR LIMITED TIME)</u></b></a></font><br><br>" + "<font color=\"#00BFFF\"><a href=\"https://dfwolf.online/en/article/38-the-wolfpack-prevails\"><b><u>The wolfpack prevails!</u></b></a></font><br><b><u>Updates</b></u>:<font color=\"#FF9700\"><a href=\"https://dfwolf.online/en/article/39-march-updates\"><b><u>March Updates!</u></b></a></font><br><b><u>Updates</b></u>:<font color=\"#FF9700\"><a href=\"https://dfwolf.online/forum/index.php?/topic/68-atualiza%C3%A7%C3%B5es-11032019\"><b><u>Last update in the Forum! (Only in Portuguese)</u></b></a></font></center>", "Wolf Server", 3);
                     break;
                 default:
                     SendServerMessage (Settings.MOTD, Settings.MOTDColor);
                     SendServerMessage ("<center>" + "<font color=\"#FF4040\"><a href=\"https://dfwolf.online/pt/artigo/40-super-promotion-of-50-off\"><b><u>Promo��o de 50% OFF (POR TEMPO LIMITADO)</u></b></a></font><br><br>" + "<font color=\"#00BFFF\"><a href=\"https://dfwolf.online/pt/artigo/38-the-wolfpack-prevails\"><b><u>A alcateia prevalece!</u></b></a></font><br><b><u>Atualiza��es</b></u>:<font color=\"#FF9700\"><a href=\"https://dfwolf.online/pt/artigo/39-march-updates\"><b><u>Atualiza��es de Mar�o!</u></b></a></font><br><b><u>Atualiza��es</b></u>:<font color=\"#FF9700\"><a href=\"https://dfwolf.online/forum/index.php?/topic/68-atualiza%C3%A7%C3%B5es-11032019\"><b><u>Ultima atualiza��o no Forum!</u></b></a></font></center>");
                     DisplayNotification ("<center>" + "<font color=\"#FF4040\"><a href=\"https://dfwolf.online/pt/artigo/40-super-promotion-of-50-off\"><b><u>Promo��o de 50% OFF (POR TEMPO LIMITADO)</u></b></a></font><br><br>" + "<font color=\"#00BFFF\"><a href=\"https://dfwolf.online/pt/artigo/38-the-wolfpack-prevails\"><b><u>A alcateia prevalece!</u></b></a></font><br><b><u>Atualiza��es</b></u>:<font color=\"#FF9700\"><a href=\"https://dfwolf.online/pt/artigo/39-march-updates\"><b><u>Atualiza��es de Mar�o!</u></b></a></font><br><b><u>Atualiza��es</b></u>:<font color=\"#FF9700\"><a href=\"https://dfwolf.online/forum/index.php?/topic/68-atualiza%C3%A7%C3%B5es-11032019\"><b><u>Ultima atualiza��o no Forum!</u></b></a></font></center>");
                     OpenPopup ("<center>" + "<font color=\"#FF4040\"><a href=\"https://dfwolf.online/pt/artigo/40-super-promotion-of-50-off\"><b><u>Promo��o de 50% OFF (POR TEMPO LIMITADO)</u></b></a></font><br><br>" + "<font color=\"#00BFFF\"><a href=\"https://dfwolf.online/pt/artigo/38-the-wolfpack-prevails\"><b><u>A alcateia prevalece!</u></b></a></font><br><b><u>Atualiza��es</b></u>:<font color=\"#FF9700\"><a href=\"https://dfwolf.online/pt/artigo/39-march-updates\"><b><u>Atualiza��es de Mar�o!</u></b></a></font><br><b><u>Atualiza��es</b></u>:<font color=\"#FF9700\"><a href=\"https://dfwolf.online/forum/index.php?/topic/68-atualiza%C3%A7%C3%B5es-11032019\"><b><u>Ultima atualiza��o no Forum!</u></b></a></font></center>", "Wolf Server", 3);
                     break;
             }*/

            string text_resume = "";
            string text = "";
            switch (Account.Lang) {
                 case "fr":
                    SendServerMessage(Settings.MOTD_fr, Settings.MOTDColor);
                    break;
                 case "es":
                    SendServerMessage(Settings.MOTD_es, Settings.MOTDColor);
                    break;
                 case "en":
                     SendServerMessage (Settings.MOTD_en, Settings.MOTDColor);
                           break;
                 default:
                     SendServerMessage (Settings.MOTD, Settings.MOTDColor);

                   text_resume = "O Wolf irar passar por diversas mudanças neste Beta! O Beta está repleto de <b>Novidades</b>,para ficar saber o que mudou acesse o nosso <a href=\"https://dfwolf.online/forum/index.php?/topic/112-atualiza%C3%A7%C3%A3o-247-ptbr\"><u>Fórum</u></a> e acesse o <a href=\"https://dfwolf.online/pt\"><u>Site</u></a> para saber oque irar mudar.<br>E participe de nosso <a href=\"https://discord.gg/bwYwr4Y\"><u>Discord</u></a> e Grupo de <a href=\"https://chat.whatsapp.com/EFTMtTjegW77N53VqnjlYe\"><u>Whatsapp</u></a>!";
                    text =
                     "<center><b>Bem Vindo ao Wolf 2.47<b></center>" +
                     "<hr/><br>" +
                     text_resume +
                      "<br><hr/>" +
                     "Jogadores Online: " + WorldServer.Clients.Count + " há "+ WorldServer.Instance.UpTime.ToString(@"dd\.hh\:mm\:ss") + " Online" +
                     "<br>Recorde Online: Indeterminado" +
                     "<br>Habilidades Desativada: Nenhuma";
                    break;

             }
            SendServerMessage(text_resume);
            DisplayNotification(text_resume);
            OpenPopup(text);
            Console.WriteLine("18");

            //  OpenPopup("Bem vindo a Wolf Server! O servidor pensado para os jogadores hardcore de Dofus, esperamos que tenha uma agrad�vel boas-vindas, Para come�ar, voc� pode ver todos os comandos com .help, para a loja .tp shop, e para voltar aqui .tp start. Um guia completo voc� pode encontr�-lo no <u><a href=\"https://www.facebook.com/DofusWolf/ \" >Wolf Face </a></u>", "Wolf Server", 5);
            // OpenPopup("Calabou�os Desativados, se mesmo assim o erro do 50% persistir contate alguem da staff indicando o seu mapa","Note Fix",3);
            IsLoggedIn = true;
            OnLoggedIn ();
            Console.WriteLine("19");
            foreach (var title in Settings.VipTitle) {
                if (!HasTitle (title)) {
                    if (Vip) {
                        AddTitle (title);
                    }
                } else {
                    if (!Vip) {
                        RemoveTitle (title);
                    }
                }
            }
            Console.WriteLine("20");
            foreach (var ornament in Settings.VipOrnament) {

                if (!HasOrnament (ornament)) {
                    if (Vip) {
                        AddOrnament (ornament);
                    }
                } else {
                    if (!Vip) {
                        RemoveOrnament (ornament);
                    }
                }
            }
            Console.WriteLine("21");
            foreach (var emote in Settings.VipEmote) {

                if (!HasEmote ((EmotesEnum) emote)) {
                    if (Vip) {
                        AddEmote ((EmotesEnum) emote);
                    }
                } else {
                    if (!Vip) {
                        RemoveEmote ((EmotesEnum) emote);
                    }
                }
            }
            Console.WriteLine("22");
            /*  if (!HasSmileyPack((SmileyPacksEnum)2))
                  if (Vip)
                  {
                      AddSmileyPack((SmileyPacksEnum)2);
                      AddSmileyPack((SmileyPacksEnum)3);
                      AddSmileyPack((SmileyPacksEnum)4);
                      AddSmileyPack((SmileyPacksEnum)5);
                  }
                  else
                  {
                      if (!Vip)
                      {
                          RemoveSmileyPack((SmileyPacksEnum)2);
                          RemoveSmileyPack((SmileyPacksEnum)3);
                          RemoveSmileyPack((SmileyPacksEnum)4);
                          RemoveSmileyPack((SmileyPacksEnum)5);
                      }
                  }*/
            foreach (var spellid in SpellsBlock)
            {
                //Spells.ForgetSpell(spellid);
                if (Spells.HasSpell(spellid))
                {
                    foreach (var shortcut in Shortcuts.SpellsShortcuts.Where(x => x.Value.SpellId == spellid).ToArray())
                        Shortcuts.RemoveShortcut(ShortcutBarEnum.SPELL_SHORTCUT_BAR, shortcut.Key);
                    Spells.UnLearnSpell(spellid);
                }
            }
            Console.WriteLine("23");
        }

        public void LogOut () {
            if (Area == null) {
                WorldServer.Instance.IOTaskPool.AddMessage (PerformLoggout);
            } else {
                Area.AddMessage (PerformLoggout);
            }
        }

        private void PerformLoggout () {
            lock (LoggoutSync) {
                IsLoggedIn = false;

                try {
                    OnLoggedOut ();

                    if (!IsInWorld)
                        return;

                    DenyAllInvitations ();

                    if (IsInRequest ())
                        CancelRequest ();

                    if (IsDialoging ())
                        Dialog.Close ();

                    if (ArenaParty != null)
                        LeaveParty (ArenaParty);

                    if (Party != null)
                        LeaveParty (Party);

                    if (Map != null && Map.IsActor (this))
                        Map.Leave (this);
                    else if (Area != null)
                        Area.Leave (this);

                    if (Map != null && m_merchantToSpawn != null)
                        Map.Enter (m_merchantToSpawn);

                    World.Instance.Leave (this);

                    m_inWorld = false;
                } catch (Exception ex) {
                    logger.Error ("Cannot perfom OnLoggout actions, but trying to Save character : {0}", ex);
                } finally {
                    BlockAccount ();
                    WorldServer.Instance.IOTaskPool.ExecuteInContext (
                        () => {
                            try {
                                SaveNow ();
                                UnLoadRecord ();
                            } finally {
                                Delete ();
                            }
                        });

                }
            }
        }

        public void SaveLater () {
            BlockAccount ();
            WorldServer.Instance.IOTaskPool.AddMessage (SaveNow);
        }
        public void SaveDopeul () {
            try {
                DopeulCollection.Save (ServerBase<WorldServer>.Instance.DBAccessor.Database);
            } catch (Exception ex) {
                logger.Error ($"Dopeul Save Error: {ex.Message} | Trace: {ex.StackTrace}");
            }
        }
        internal void SaveNow () {
            try {
                WorldServer.Instance.IOTaskPool.EnsureContext ();
                var database = WorldServer.Instance.DBAccessor.Database;
                var text1 = "";
                var text2 = "";
                switch (Account.Lang) {
                    case "fr":
                        text1 = "L'";
                        text2 = " n'a pas �t� sauv� !!!";
                        break;
                    case "es":
                        text1 = "�El ";
                        text2 = " no fue salvo !!!";
                        break;
                    case "en":
                        text1 = "The ";
                        text2 = " was not saved !!!";
                        break;
                    default:
                        text1 = "O ";
                        text2 = " n�o foi salvo!!!";
                        break;
                }
                lock (SaveSync) {
                    try { Inventory.Save (database, false); } catch (Exception ex) {
                        logger.Error (ex.Message);
                        this.SendServerMessage (text1 + "Inventory" + text2);
                        this.OpenPopup (text1 + "Inventory" + text2);
                    }
                    try { Bank.Save (database); } catch (Exception ex) {
                        logger.Error (ex.Message);
                        this.SendServerMessage (text1 + "Bank" + text2);
                        this.OpenPopup (text1 + "Bank" + text2);
                    }
                    try { MerchantBag.Save (database); } catch (Exception ex) {
                        logger.Error (ex.Message);
                        this.SendServerMessage (text1 + "MerchantBag" + text2);
                        this.OpenPopup (text1 + "MerchantBag" + text2);
                    }
                    try { Spells.Save (); } catch (Exception ex) {
                        logger.Error (ex.Message);
                        this.SendServerMessage (text1 + "Spells" + text2);
                        this.OpenPopup (text1 + "Spells" + text2);
                    }
                    try { Shortcuts.Save (); } catch (Exception ex) {
                        logger.Error (ex.Message);
                        this.SendServerMessage (text1 + "Shortcuts" + text2);
                        this.OpenPopup (text1 + "Shortcuts" + text2);
                    }
                    try { FriendsBook.Save (); } catch (Exception ex) {
                        logger.Error (ex.Message);
                        this.SendServerMessage (text1 + "FriendsBook" + text2);
                        this.OpenPopup (text1 + "FriendsBook" + text2);
                    }
                    try { Jobs.Save (database); } catch (Exception ex) {
                        logger.Error (ex.Message);
                        this.SendServerMessage (text1 + "Jobs" + text2);
                        this.OpenPopup (text1 + "Jobs" + text2);
                    }
                    try { IdolInventory.Save (); } catch (Exception ex) {
                        logger.Error (ex.Message);
                        this.SendServerMessage (text1 + "IdolInventory" + text2);
                        this.OpenPopup (text1 + "IdolInventory" + text2);
                    }
                    try { Guild?.Save (database); } catch (Exception ex) {
                        logger.Error (ex.Message);
                        this.SendServerMessage (text1 + "Guild" + text2);
                        this.OpenPopup (text1 + "Guild" + text2);
                    }
                    // try { Alliance?.Save(database); }
                    //  catch (Exception ex)
                    {
                        //  logger.Error(ex.Message);
                        //  this.SendServerMessage(text1 + "Alliance"+text2);
                        //  this.SendServerMessage("Salvamento da alian�a indisponivel.");
                        // this.OpenPopup(text1 + "Alliance"+text2);
                    }

                    try { SaveMounts (); } catch (Exception ex) {
                        logger.Error (ex.Message);
                        this.SendServerMessage (text1 + "Mounts" + text2);
                        this.OpenPopup (text1 + "Mounts" + text2);
                    }
                    try { SaveDopeul (); } catch (Exception ex) {
                        logger.Error (ex.Message);
                        this.SendServerMessage (text1 + "Dopeul" + text2);
                        this.OpenPopup (text1 + "Dopeul" + text2);
                    }
                    try {
                        m_record.MapId = NextMap != null ? NextMap.Id : Map.Id;
                        m_record.CellId = Cell.Id;
                        m_record.Direction = Direction;

                        m_record.AP = Stats[PlayerFields.AP].Base;
                        m_record.MP = Stats[PlayerFields.MP].Base;
                        m_record.Strength = Stats[PlayerFields.Strength].Base;
                        m_record.Agility = Stats[PlayerFields.Agility].Base;
                        m_record.Chance = Stats[PlayerFields.Chance].Base;
                        m_record.Intelligence = Stats[PlayerFields.Intelligence].Base;
                        m_record.Wisdom = Stats[PlayerFields.Wisdom].Base;
                        m_record.Vitality = Stats[PlayerFields.Vitality].Base;

                        m_record.PermanentAddedStrength = (short) Stats[PlayerFields.Strength].Additional;
                        m_record.PermanentAddedAgility = (short) Stats[PlayerFields.Agility].Additional;
                        m_record.PermanentAddedChance = (short) Stats[PlayerFields.Chance].Additional;
                        m_record.PermanentAddedIntelligence = (short) Stats[PlayerFields.Intelligence].Additional;
                        m_record.PermanentAddedWisdom = (short) Stats[PlayerFields.Wisdom].Additional;
                        m_record.PermanentAddedVitality = (short) Stats[PlayerFields.Vitality].Additional;

                        m_record.BaseHealth = Stats.Health.Base;
                        m_record.DamageTaken = Stats.Health.DamageTaken;

                    } catch (Exception ex) {
                        logger.Error (ex.Message);
                        this.SendServerMessage (text1 + "'Save'" + text2);
                        this.OpenPopup (text1 + "'Save'" + text2);
                    }
                    try { database.Update (m_record); } catch (Exception ex) {
                        logger.Error (ex.Message);
                        this.SendServerMessage (text1 + "m_record" + text2);
                        this.OpenPopup (text1 + "m_record" + text2);
                    }
                    try { database.Update (Client.WorldAccount); } catch (Exception ex) {
                        logger.Error (ex.Message);
                        this.SendServerMessage (text1 + "Client.WorldAccount" + text2);
                        this.OpenPopup (text1 + "Client.WorldAccount" + text2);
                    }

                }

                if (IsAuthSynced)
                    OnSaved ();
                else {
                    IPCAccessor.Instance.SendRequest<CommonOKMessage> (new UpdateAccountMessage (Account),
                        msg => {
                            OnSaved ();
                        });
                }
            } catch (Exception e) {
                UnBlockAccount ();
                throw e;
            }
        }

        public void LoadRecord () {
            Breed = BreedManager.Instance.GetBreed (BreedId);
            Head = BreedManager.Instance.GetHead (Record.Head);
            var map = World.Instance.GetMap (m_record.MapId);

            if (map == null) {
                map = World.Instance.GetMap (Breed.StartMap);
                m_record.CellId = Breed.StartCell;
                m_record.Direction = Breed.StartDirection;
            }

            Position = new ObjectPosition (
                map,
                map.Cells[m_record.CellId],
                m_record.Direction);

            Stats = new StatsFields (this);
            Stats.Initialize (m_record);
            Level = ExperienceManager.Instance.GetCharacterLevel (Experience);
            LowerBoundExperience = ExperienceManager.Instance.GetCharacterLevelExperience (Level);
            UpperBoundExperience = ExperienceManager.Instance.GetCharacterNextLevelExperience (Level);

            AlignmentGrade = (sbyte) ExperienceManager.Instance.GetAlignementGrade (m_record.Honor);
            LowerBoundHonor = ExperienceManager.Instance.GetAlignementGradeHonor ((byte) AlignmentGrade);
            UpperBoundHonor = ExperienceManager.Instance.GetAlignementNextGradeHonor ((byte) AlignmentGrade);

            Inventory = new Inventory (this);
            Inventory.LoadInventory ();
            Inventory.LoadPresets ();
            try {
                Achievement = new PlayerAchievement (this);
                Achievement.LoadAchievements ();
            } catch { }

            IdolInventory = new IdolInventory (this);

            Bank = new Bank (this);
            Bank.LoadRecord ();

            MerchantBag = new CharacterMerchantBag (this);
            CheckMerchantModeReconnection ();
            MerchantBag.LoadMerchantBag ();
            try {
                GuildMember = GuildManager.Instance.TryGetGuildMember (Id);
            } catch { }

            // try
            //{
            //if (this.Guild != null)
            //{
            //Guild.SetAlliance(Singleton<AllianceManager>.Instance.TryGetAlliance(GuildMember.Guild.Record.AllianceId.HasValue ? GuildMember.Guild.Record.AllianceId.Value : 0));
            //  }
            //}
            //catch { }

            UpdateLook (false);
            try { LoadMounts (); } catch { }

            Spells = new SpellInventory (this);
            Spells.LoadSpells ();

            DopeulCollection = new DopeulCollection ();
            DopeulCollection.Load (this);

            Shortcuts = new ShortcutBar (this);
            Shortcuts.Load ();

            FriendsBook = new FriendsBook (this);
            FriendsBook.Load ();

            ChatHistory = new ChatHistory (this);

            LoadQuests ();

            Jobs = new JobsCollection (this);
            Jobs.LoadJobs ();

            m_recordLoaded = true;
        }
        public bool AvAActived { get; internal set; }
        public AggressableStatusEnum AvaState { get; internal set; }
        private void UnLoadRecord () {
            if (!m_recordLoaded)
                return;

            m_recordLoaded = false;
        }

        private void BlockAccount () {
            AccountManager.Instance.BlockAccount (Client.WorldAccount, this);
            IsAccountBlocked = true;
        }

        private void UnBlockAccount () {
            if (!IsAccountBlocked)
                return;

            AccountManager.Instance.UnBlockAccount (Client.WorldAccount);
            IsAccountBlocked = false;

            OnAccountUnblocked ();
        }

        #endregion Save & Load

        #region Exceptions

        private readonly List<KeyValuePair<string, Exception>> m_commandsError = new List<KeyValuePair<string, Exception>> ();
        private Mount m_equippedMount;
        private ActorLook m_look;

        public List<KeyValuePair<string, Exception>> CommandsErrors => m_commandsError;

        #endregion Exceptions

        #region Network

        #region GameRolePlayCharacterInformations

        public override GameContextActorInformations GetGameContextActorInformations (Character character) {
            return new GameRolePlayCharacterInformations (
                Id,
                Look.GetEntityLook (),
                GetEntityDispositionInformations (),
                Name,
                GetHumanInformations (),
                Account.Id,
                GetActorAlignmentInformations ());
        }

        #endregion GameRolePlayCharacterInformations

        #region ActorAlignmentInformations

        public ActorAlignmentInformations GetActorAlignmentInformations () {
            return new ActorAlignmentInformations (
                (sbyte) AlignmentSide,
                AlignmentValue,
                PvPEnabled ? AlignmentGrade : (sbyte) 0,
                CharacterPower);
        }

        #endregion ActorAlignmentInformations

        #region ActorExtendedAlignmentInformations
        public byte GetAgressablestatus () {
            if (AvAActived && SubArea.HasPrism) {
                if (SubArea.Prism.State == PrismStateEnum.PRISM_STATE_VULNERABLE)
                    return (byte) AvaState;
                //AvA_ENABLED_NON_AGGRESSABLE When full of alliances so can see but can't be aggresed!
            }
            if (PvPEnabled) {
                if (!Map.AllowAggression) {
                    return (byte) AggressableStatusEnum.PvP_ENABLED_NON_AGGRESSABLE;
                }
                return (byte) AggressableStatusEnum.PvP_ENABLED_AGGRESSABLE;
            }
            return (byte) AggressableStatusEnum.NON_AGGRESSABLE;
        }
        public ActorExtendedAlignmentInformations GetActorAlignmentExtendInformations () {
            return new ActorExtendedAlignmentInformations (
                (sbyte) AlignmentSide,
                AlignmentValue,
                PvPEnabled ? AlignmentGrade : (sbyte) 0,
                CharacterPower,
                (ushort) Honor,
                (ushort) LowerBoundHonor,
                (ushort) UpperBoundHonor,
                (PvPEnabled ? (sbyte) AggressableStatusEnum.PvP_ENABLED_AGGRESSABLE : (sbyte) AggressableStatusEnum.NON_AGGRESSABLE));
        }

        #endregion ActorExtendedAlignmentInformations

        #region CharacterBaseInformations

        public CharacterBaseInformations GetCharacterBaseInformations () => new CharacterBaseInformations (
            (ulong) Id,
            Namedefault, //Name
            (ushort) Level,
            Look.GetEntityLook (),
            (sbyte) BreedId,
            Sex == SexTypeEnum.SEX_FEMALE);
        public CharacterMinimalInformations GetCharacterMinimalInformations () => new CharacterMinimalInformations ((ulong) Id, Name, (ushort) Level);
        public CharacterMinimalPlusLookInformations GetCharacterMinimalPlusLookInformations () => new CharacterMinimalPlusLookInformations (
            (ulong) Id,
            Name,
            (ushort) Level,
            Look.GetEntityLook (),
            (sbyte) Breed.Id);

        public CharacterCharacteristicsInformations GetCharacterCharacteristicsInformations () =>

            new CharacterCharacteristicsInformations (
                Experience, // EXPERIENCE
                LowerBoundExperience, // EXPERIENCE level floor
                UpperBoundExperience, // EXPERIENCE nextlevel floor
                UpperBoundExperience, // TODO: EXPERIENCE bonus limit

                Kamas, // Amount of kamas.

                (short) StatsPoints, // Stats points
                0, // Additionnal points
                (short) SpellsPoints, // Spell points

                // Alignment
                GetActorAlignmentExtendInformations (),
                Stats.Health.Total, // Life points
                Stats.Health.TotalMax, // Max Life points

                Energy, // Energy points
                EnergyMax, // maxEnergyPoints

                (short) Stats[PlayerFields.AP]
                .Total, // actionPointsCurrent
                (short) Stats[PlayerFields.MP]
                .Total, // movementPointsCurrent

                Stats[PlayerFields.Initiative],
                Stats[PlayerFields.Prospecting],
                Stats[PlayerFields.AP],
                Stats[PlayerFields.MP],
                Stats[PlayerFields.Strength],
                Stats[PlayerFields.Vitality],
                Stats[PlayerFields.Wisdom],
                Stats[PlayerFields.Chance],
                Stats[PlayerFields.Agility],
                Stats[PlayerFields.Intelligence],
                Stats[PlayerFields.Range],
                Stats[PlayerFields.SummonLimit],
                Stats[PlayerFields.DamageReflection],
                Stats[PlayerFields.CriticalHit],
                (short) Inventory.WeaponCriticalHit,
                Stats[PlayerFields.CriticalMiss],
                Stats[PlayerFields.HealBonus],
                Stats[PlayerFields.DamageBonus],
                Stats[PlayerFields.WeaponDamageBonus],
                Stats[PlayerFields.DamageBonusPercent],
                Stats[PlayerFields.TrapBonus],
                Stats[PlayerFields.TrapBonusPercent],
                Stats[PlayerFields.GlyphBonusPercent],
                Stats[PlayerFields.RuneBonusPercent],
                Stats[PlayerFields.PermanentDamagePercent],
                Stats[PlayerFields.TackleBlock],
                Stats[PlayerFields.TackleEvade],
                Stats[PlayerFields.APAttack],
                Stats[PlayerFields.MPAttack],
                Stats[PlayerFields.PushDamageBonus],
                Stats[PlayerFields.CriticalDamageBonus],
                Stats[PlayerFields.NeutralDamageBonus],
                Stats[PlayerFields.EarthDamageBonus],
                Stats[PlayerFields.WaterDamageBonus],
                Stats[PlayerFields.AirDamageBonus],
                Stats[PlayerFields.FireDamageBonus],
                Stats[PlayerFields.DodgeAPProbability],
                Stats[PlayerFields.DodgeMPProbability],
                Stats[PlayerFields.NeutralResistPercent],
                Stats[PlayerFields.EarthResistPercent],
                Stats[PlayerFields.WaterResistPercent],
                Stats[PlayerFields.AirResistPercent],
                Stats[PlayerFields.FireResistPercent],
                Stats[PlayerFields.NeutralElementReduction],
                Stats[PlayerFields.EarthElementReduction],
                Stats[PlayerFields.WaterElementReduction],
                Stats[PlayerFields.AirElementReduction],
                Stats[PlayerFields.FireElementReduction],
                Stats[PlayerFields.PushDamageReduction],
                Stats[PlayerFields.CriticalDamageReduction],
                Stats[PlayerFields.PvpNeutralResistPercent],
                Stats[PlayerFields.PvpEarthResistPercent],
                Stats[PlayerFields.PvpWaterResistPercent],
                Stats[PlayerFields.PvpAirResistPercent],
                Stats[PlayerFields.PvpFireResistPercent],
                Stats[PlayerFields.PvpNeutralElementReduction],
                Stats[PlayerFields.PvpEarthElementReduction],
                Stats[PlayerFields.PvpWaterElementReduction],
                Stats[PlayerFields.PvpAirElementReduction],
                Stats[PlayerFields.PvpFireElementReduction],
                Stats[PlayerFields.MeleeDamageDonePercent],
                Stats[PlayerFields.MeleeDamageReceivedPercent],
                Stats[PlayerFields.RangedDamageDonePercent],
                Stats[PlayerFields.RangedDamageReceivedPercent],
                Stats[PlayerFields.WeaponDamageDonePercent],
                Stats[PlayerFields.WeaponDamageReceivedPercent],
                Stats[PlayerFields.SpellDamageDonePercent],
                Stats[PlayerFields.SpellDamageReceivedPercent],
                SpellsModifications,
                0);

        #endregion CharacterBaseInformations

        #region PartyMemberInformations

        public PartyInvitationMemberInformations GetPartyInvitationMemberInformations () => new PartyInvitationMemberInformations (
            (ulong) Id,
            Name,
            (ushort) Level,
            Look.GetEntityLook (),
            (sbyte) BreedId,
            Sex == SexTypeEnum.SEX_FEMALE,
            (short) Map.Position.X,
            (short) Map.Position.Y,
            Map.Id,
            (ushort) Map.SubArea.Id,
            Companion == null ? new PartyCompanionMemberInformations[0] : new [] { Companion.GetPartyCompanionMemberInformations () });

        public PartyMemberInformations GetPartyMemberInformations () => new PartyMemberInformations (
            (ulong) Id,
            Name,
            (ushort) Level,
            Look.GetEntityLook (),
            (sbyte) BreedId,
            Sex == SexTypeEnum.SEX_FEMALE,
            LifePoints,
            MaxLifePoints,
            (ushort) Stats[PlayerFields.Prospecting].Total,
            (byte) RegenSpeed,
            (short) Stats[PlayerFields.Initiative].Total,
            (sbyte) AlignmentSide,
            (short) Map.Position.X,
            (short) Map.Position.Y,
            Map.Id,
            (ushort) SubArea.Id,
            Status,
            Companion == null ? new PartyCompanionMemberInformations[0] : new [] { Companion.GetPartyCompanionMemberInformations () });

        public PartyGuestInformations GetPartyGuestInformations (Party party) {
            if (!m_partyInvitations.ContainsKey (party.Id))
                return new PartyGuestInformations ();

            var invitation = m_partyInvitations[party.Id];

            return new PartyGuestInformations (
                (ulong) Id,
                (ulong) invitation.Source.Id,
                Name,
                Look.GetEntityLook (),
                (sbyte) BreedId,
                Sex == SexTypeEnum.SEX_FEMALE,
                Status,
                Companion == null ? new PartyCompanionMemberInformations[0] : new [] { Companion.GetPartyCompanionMemberInformations () });
        }

        public PartyMemberArenaInformations GetPartyMemberArenaInformations () => new PartyMemberArenaInformations (
            (ulong) Id,
            Name,
            (ushort) Level,
            Look.GetEntityLook (),
            (sbyte) BreedId,
            Sex == SexTypeEnum.SEX_FEMALE,
            LifePoints,
            MaxLifePoints,
            (ushort) Stats[PlayerFields.Prospecting].Total,
            (byte) RegenSpeed,
            (short) Stats[PlayerFields.Initiative].Total,
            (sbyte) AlignmentSide,
            (short) Map.Position.X,
            (short) Map.Position.Y,
            Map.Id,
            (ushort) SubArea.Id,
            Status,
            Companion == null ? new PartyCompanionMemberInformations[0] : new [] { Companion.GetPartyCompanionMemberInformations () },
            (ushort) ArenaRank_3vs3);

        #endregion PartyMemberInformations

        public override ActorRestrictionsInformations GetActorRestrictionsInformations () {
            return new ActorRestrictionsInformations (!Map.AllowAggression || IsGhost (), // cantBeAgressed
                !Map.AllowChallenge || IsGhost (), // cantBeChallenged
                !Map.AllowExchangesBetweenPlayers || IsGhost (), // cantTrade
                IsGhost (), // cantBeAttackedByMutant
                false, // cantRun
                false, // forceSlowWalk
                false, // cantMinimize
                PlayerLifeStatus == PlayerLifeStatusEnum.STATUS_TOMBSTONE, // cantMove

                !Map.AllowAggression || IsGhost (), // cantAggress
                IsGhost (), // cantChallenge
                IsGhost (), // cantExchange
                IsGhost (), // cantAttack
                false, // cantChat
                IsGhost (), // cantBeMerchant
                IsGhost (), // cantUseObject
                IsGhost (), // cantUseTaxCollector

                IsGhost (), // cantUseInteractive
                IsGhost (), // cantSpeakToNPC
                false, // cantChangeZone
                IsGhost (), // cantAttackMonster
                false // cantWalk8Directions
            );
        }

        public override HumanInformations GetHumanInformations () {
            var human = base.GetHumanInformations ();

            var options = new List<HumanOption> ();

            try {
                if (Guild != null) {
                    options.Add (new HumanOptionGuild (Guild.GetGuildInformations ()));

                    if (Guild.Alliance != null) {
                        options.Add (new HumanOptionAlliance (Guild.Alliance.GetAllianceInformations (),
                            (sbyte) GetAgressablestatus ()));
                    }

                }
            } catch { }

            if (SelectedTitle != null)
                options.Add (new HumanOptionTitle (SelectedTitle.Value, string.Empty));

            if (SelectedOrnament != null)
                options.Add (new HumanOptionOrnament ((ushort) SelectedOrnament.Value, Level));

            if (LastEmoteUsed != null)
                options.Add (new HumanOptionEmote ((byte) LastEmoteUsed.First.Id, LastEmoteUsed.Second.GetUnixTimeStampLong ()));

            if (LastSkillUsed != null)
                options.Add (new HumanOptionSkillUse ((uint) LastSkillUsed.InteractiveObject.Id, (ushort) LastSkillUsed.SkillTemplate.Id, LastSkillUsed.SkillEndTime.GetUnixTimeStampLong ()));
            human.options = options;
            return human;
        }

        #endregion Network

        public CharacterRecord Record => m_record;

        public Commands.Commands.Teleport.ZaapDialog CustomZaapDialog => Dialog as Commands.Commands.Teleport.ZaapDialog;
        public Commands.Commands.Teleport.DonjonZaapDialog DonjonZaapDialog => Dialog as Commands.Commands.Teleport.DonjonZaapDialog;

        public override bool CanBeSee (WorldObject byObj) => base.CanBeSee (byObj) && (byObj == this || !Invisible) && Game.HavenBags.HavenBagManager.Instance.CanBeSeenInHavenBag (byObj, this);

        protected override void OnDisposed () {
            if (FriendsBook != null)
                FriendsBook.Dispose ();

            if (Inventory != null)
                Inventory.Dispose ();

            base.OnDisposed ();
        }

        public override string ToString () => string.Format ("{0} ({1})", Name, Id);

        public bool IsInCustomZaapDialog() => Dialog is Commands.Commands.Teleport.ZaapDialog;

        public bool IsInDonjonZaapDialog() => Dialog is Commands.Commands.Teleport.DonjonZaapDialog;
    }
}