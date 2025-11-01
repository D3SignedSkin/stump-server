namespace Stump.Server.BaseServer.Initialization
{
    public enum InitializationPass : byte
    {
        Database,
        /// <summary>
        ///     AuthServer, WorldServer, MongoLogger
        /// </summary>
        Any,
        /// <summary>
        ///     ItemsFairyManager, LotteryManager, MapsResetMAnager, AutoEventsManager, AutoManager, ServerInfo, NpcsEffectsManager, StaffPaymentsManager, StartupManager, OptionalFeaturesManager
        /// </summary>
        CoreReserved,
        /// <summary>
        ///     DiscriminatorManager
        /// </summary>
        First,
        /// <summary>
        ///     ConnectionHandler, TextManager, MonsterStarLoop, VoteNotification, ChatManager, ApproachHandler, WebServer
        /// </summary>
        Second,
        /// <summary>
        ///     
        /// </summary>
        Third,
        /// <summary>
        ///     BreedManager, EffectManager
        /// </summary>
        Fourth,
        /// <summary>
        ///     BrainManager, TinselManager, ExperienceManager, RankManeger, RankRewardManeger, ChallengeManager, IdolManager, InteractiveManager, ChasseurManager, HunterManager, CellTriggerManager
        ///     PlacementManager, WorldMapScrollActionManager, CaptureDragodindeManager, CaptureMuldoManager, CaptureVulkManager, PresetsManager, IncarnationManager, ActivitySuggestionsManager, AutoEventTicketManager
        /// </summary>
        Fifth,
        /// <summary>
        ///     NpcManager, ArenaManager, LeaguesManager, JobManager, MonsterNaniManager, WatendManager
        /// </summary>
        Sixth,
        /// <summary>
        ///     AchievementManager, MerchantManager, MonsterManager, MountManager, GuildManager
        /// </summary>
        Seventh,
        /// <summary>
        ///     AllianceManager, ArenaChampsManager, HavenBagManager, KoliLogsManager, MandatoryManager, SeekLogManager, WorldDungeons, World
        /// </summary>
        Eighth,
        /// <summary>
        ///     TaxCollectorManager, PaddockManager, PrismManager
        /// </summary>
        Ninth,
        /// <summary>
        ///     
        /// </summary>
        Tenth,
        /// <summary>
        ///     QuestManager
        /// </summary>
        Last,
        /// <summary>
        ///     KohManager
        /// </summary>
    }
}