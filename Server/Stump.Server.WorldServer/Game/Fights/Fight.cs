using NLog;
using ServiceStack.Text;
using Stump.Core.Attributes;
using Stump.Core.Extensions;
using Stump.Core.Mathematics;
using Stump.Core.Pool;
using Stump.Core.Timers;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Enums.Custom;
using Stump.DofusProtocol.Types;
using Stump.ORM.SubSonic.Extensions;
using Stump.Server.WorldServer.Core.Network;
using Stump.Server.WorldServer.Database.Items.Templates;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Actors;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Effects;
using Stump.Server.WorldServer.Game.Fights.Buffs;
using Stump.Server.WorldServer.Game.Fights.Challenges;
using Stump.Server.WorldServer.Game.Fights.Results;
using Stump.Server.WorldServer.Game.Fights.Sequences;
using Stump.Server.WorldServer.Game.Fights.Teams;
using Stump.Server.WorldServer.Game.Fights.Triggers;
using Stump.Server.WorldServer.Game.Maps;
using Stump.Server.WorldServer.Game.Maps.Cells;
using Stump.Server.WorldServer.Game.Maps.Pathfinding;
using Stump.Server.WorldServer.Game.Spells;
using Stump.Server.WorldServer.Game.Spells.Casts;
using Stump.Server.WorldServer.Handlers.Actions;
using Stump.Server.WorldServer.Handlers.Basic;
using Stump.Server.WorldServer.Handlers.Characters;
using Stump.Server.WorldServer.Handlers.Context;
using Stump.Server.WorldServer.Handlers.Context.RolePlay;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using Stump.Core.Reflection;
using Stump.Server.WorldServer.Game.Items;
using System.Threading.Tasks;
using Stump.Server.WorldServer.Game.Effects.Instances;

namespace Stump.Server.WorldServer.Game.Fights
{
    public delegate void FightWinnersDelegate(IFight fight, FightTeam winners, FightTeam losers, bool draw);

    public delegate void ResultsGeneratedDelegate(IFight fight, List<IFightResult> results);

    public interface IFight : ICharacterContainer
    {
        int Id
        {
            get;
        }

        Guid UniqueId
        {
            get;
        }

        Map Map
        {
            get;
        }

        Cell[] Cells
        {
            get;
        }

        FightTypeEnum FightType
        {
            get;
        }

        bool IsPvP
        {
            get;
        }

        bool IsMultiAccountRestricted
        {
            get;
        }

        FightState State
        {
            get;
        }

        bool IsStarted
        {
            get;
        }


        DateTime CreationTime
        {
            get;
        }

        DateTime StartTime
        {
            get;
        }

        short AgeBonus
        {
            get;
        }

        FightTeam ChallengersTeam
        {
            get;
        }

        FightTeam DefendersTeam
        {
            get;
        }

        FightTeam[] Teams
        {
            get;
        }

        FightTeam Winners
        {
            get;
        }

        FightTeam Losers
        {
            get;
        }

        bool Draw
        {
            get;
        }

        TimeLine TimeLine
        {
            get;
        }

        ReadOnlyCollection<FightActor> Fighters
        {
            get;
        }

        ReadOnlyCollection<FightActor> Leavers
        {
            get;
        }

        ReadOnlyCollection<FightSpectator> Spectators
        {
            get;
        }

        FightActor FighterPlaying
        {
            get;
        }

        List<DefaultChallenge> Challenges
        {
            get;
        }

        sbyte ChallengeDropOrXp
        {
            get;
            set;
        }

        DateTime TurnStartTime
        {
            get;
        }

        ReadyChecker ReadyChecker
        {
            get;
        }

        bool SpectatorClosed
        {
            get;
        }

        bool BladesVisible
        {
            get;
        }

        FightPortalsManager PortalsManager
        {
            get;
        }

        FightSequence CurrentSequence
        {
            get;
        }

        FightSequence CurrentRootSequence
        {
            get;
        }

        DateTime LastSequenceEndTime { get; }

        bool IsSequencing
        {
            get;
        }

        bool IsDeathTemporarily
        {
            get;
        }

        bool CanKickPlayer
        {
            get;
        }

        /// <summary>
        /// Do not modify, just read
        /// </summary>
        WorldClientCollection Clients
        {
            get;
        }

        /// <summary>
        /// Do not modify, just read
        /// </summary>
        WorldClientCollection SpectatorClients
        {
            get;
        }

        bool AIDebugMode
        {
            get;
            set;
        }

        bool Freezed
        {
            get;
            set;
        }

        event Action<IFight> FightStarted;

        event Action<IFight> FightEnded;

        void Initialize();

        void StartFighting();

        bool CheckFightEnd(bool endFight = true);

        void CancelFight();

        void EndFight();

        event FightWinnersDelegate WinnersDetermined;

        event ResultsGeneratedDelegate ResultsGenerated;

        event Action<IFight> GeneratingResults;

        void StartPlacement();

        void ShowBlades();

        void HideBlades();

        void UpdateBlades(FightTeam team);

        bool FindRandomFreeCell(FightActor fighter, out Cell cell, bool placement = true);

        bool RandomnizePosition(FightActor fighter);

        void RandomnizePositions(FightTeam team);

        DirectionsEnum FindPlacementDirection(FightActor fighter);

        bool KickFighter(FightActor kicker, FightActor fighter);

        /// <summary>
        ///   Check if a character can change position (before the fight is started).
        /// </summary>
        /// <param name = "fighter"></param>
        /// <param name="cell"></param>
        /// <returns>If change is possible</returns>
        bool CanChangePosition(FightActor fighter, Cell cell);

        void ToggleSpectatorClosed(Character character, bool state);

        bool CanSpectatorJoin(Character spectator);

        bool AddSpectator(FightSpectator spectator);

        void RemoveSpectator(FightSpectator spectator);

        void RemoveAllSpectators();

        void StartTurn();

        event Action<IFight, FightActor> TurnStarted;
        event Action<IFight, FightActor> SwitchCompanion;
        void StopTurn();

        event Action<IFight, FightActor> BeforeTurnStopped;

        event Action<IFight, FightActor> TurnStopped;

        event Action<FightActor, int, int> Tackled;

        IEnumerable<Buff> GetBuffs();

        void UpdateBuff(Buff buff, bool updateAction = true);

        FightSequence StartMoveSequence(FightPath path);

        FightSequence StartSequence(SequenceTypeEnum sequenceType);

        void OnSequenceEnded(FightSequence fightSequence);

        void EndAllSequences();

        bool AcknowledgeAction(CharacterFighter fighter, int sequenceId);

        IEnumerable<MarkTrigger> GetTriggers();

        bool ShouldTriggerOnMove(Cell cell, FightActor actor);

        MarkTrigger[] GetTriggersByCell(Cell cell);

        MarkTrigger[] GetTriggers(Cell cell);

        void AddTriger(MarkTrigger trigger);

        void RemoveTrigger(MarkTrigger trigger);

        void TriggerMarks(Cell cell, FightActor trigger, TriggerType triggerType);

        void DecrementGlyphDuration(FightActor caster);

        int PopNextTriggerId();

        void FreeTriggerId(int id);

        void AddChallenge(DefaultChallenge challenge);

        int GetChallengesBonus();

        IEnumerable<Character> GetAllCharacters();

        IEnumerable<Character> GetAllCharacters(bool withSpectators = false);

        void ForEach(Action<Character> action);

        void ForEach(Action<Character> action, bool withSpectators = false);

        void ForEach(Action<Character> action, Character except, bool withSpectators = false);

        bool IsCellFree(Cell cell);

        TimeSpan GetFightDuration();

        TimeSpan GetTurnTimeLeft();

        TimeSpan GetPlacementTimeLeft();

        sbyte GetNextContextualId();

        void DeadWaveSpawn();

        void FreeContextualId(sbyte id);

        FightActor GetOneFighter(int id);

        FightActor GetOneFighter(Cell cell);

        FightActor GetOneFighter(Predicate<FightActor> predicate);

        T GetRandomFighter<T>() where T : FightActor;

        T GetOneFighter<T>(int id) where T : FightActor;

        T GetOneFighter<T>(Cell cell) where T : FightActor;

        T GetOneFighter<T>(Predicate<T> predicate) where T : FightActor;

        T GetFirstFighter<T>(int id) where T : FightActor;

        T GetFirstFighter<T>(Cell cell) where T : FightActor;

        T GetFirstFighter<T>(Predicate<T> predicate) where T : FightActor;

        List<FightActor> GetAllFightersInLine(MapPoint startCell, int range, DirectionsEnum direction);

        List<FightActor> GetAllFightersInLineToSacrier(MapPoint startCell, int range, DirectionsEnum direction);

        ReadOnlyCollection<FightActor> GetAllFighters();

        ReadOnlyCollection<FightActor> GetLeavers();

        CharacterFighter GetLeaver(int characterId);

        ReadOnlyCollection<FightSpectator> GetSpectators();

        IEnumerable<Character> GetCharactersAndSpectators();

        IEnumerable<FightActor> GetFightersAndLeavers();

        IEnumerable<FightActor> GetAllFighters(Cell[] cells);

        IEnumerable<FightActor> GetAllFighters(IEnumerable<Cell> cells);

        IEnumerable<FightActor> GetAllFighters(Predicate<FightActor> predicate);

        IEnumerable<T> GetAllFighters<T>() where T : FightActor;

        IEnumerable<T> GetAllFighters<T>(Predicate<T> predicate) where T : FightActor;

        IEnumerable<int> GetDeadFightersIds();

        IEnumerable<int> GetAliveFightersIds();

        FightCommonInformations GetFightCommonInformations();

        FightExternalInformations GetFightExternalInformations(Character character);

        IEnumerable<NamedPartyTeam> GetPartiesName();
        IEnumerable<NamedPartyTeamWithOutcome> GetPartiesNameWithOutcome();

        bool CanBeSeen(MapPoint from, MapPoint to, bool throughEntities = false, WorldObject except = null);

        void RejoinFightFromDisconnection(CharacterFighter character);

        void RefreshActor(FightActor actor);
    }

    public static class FightConfiguration // this is necessary since we can't read static field dynamically in a generic class
    {
        [Variable]
        public static int PlacementPhaseTime = 30000;

        /// <summary>
        ///   Delay for player's turn
        /// </summary>
        [Variable]
        public static int TurnTime = 15000;

        /// <summary>
        ///   Max Delay for player's turn
        /// </summary>
        [Variable]
        public static int MaxTurnTime = 60000;

        /// <summary>
        ///   Delay before force turn to end
        /// </summary>
        [Variable]
        public static int TurnEndTimeOut = 5000;

        /// <summary>
        ///   Delay before force turn to end
        /// </summary>
        [Variable]
        public static int EndFightTimeOut = 10000;

        [Variable]
        public static int TurnsBeforeDisconnection = 20;
    }

    public abstract class Fight<TBlueTeam, TRedTeam> : WorldObjectsContext, IFight where TRedTeam : FightTeam where TBlueTeam : FightTeam
    {
        protected readonly Logger logger = LogManager.GetCurrentClassLogger();

        #region Constructor

        protected Fight(int id, Map fightMap, TBlueTeam defendersTeam, TRedTeam challengersTeam)
        {
            Id = id;
            UniqueId = Guid.NewGuid();
            Map = fightMap;
            DefendersTeam = defendersTeam;
            DefendersTeam.Fight = this;
            ChallengersTeam = challengersTeam;
            ChallengersTeam.Fight = this;
            m_teams = new[] { (FightTeam)ChallengersTeam, DefendersTeam };

            TimeLine = new TimeLine(this);
            m_leavers = new List<FightActor>();
            m_spectators = new List<FightSpectator>();

            Challenges = new List<DefaultChallenge>();
            ChallengesProposal = new List<DefaultChallenge>();

            DefendersTeam.FighterAdded += OnFighterAdded;
            DefendersTeam.FighterRemoved += OnFighterRemoved;
            ChallengersTeam.FighterAdded += OnFighterAdded;
            ChallengersTeam.FighterRemoved += OnFighterRemoved;
            PortalsManager = new FightPortalsManager(this);
            CreationTime = DateTime.Now;
        }

        public Fight(int id, Map fightMap, FightTeam blueTeam, FightTeam redTeam)
        {
            Id = id;
            this.fightMap = fightMap;
            this.blueTeam = blueTeam;
            this.redTeam = redTeam;
        }

        #endregion Constructor

        #region Properties

        public FightPortalsManager PortalsManager
        {
            get;
            protected set;
        }

        protected readonly ReversedUniqueIdProvider m_contextualIdProvider = new ReversedUniqueIdProvider(0);
        protected readonly UniqueIdProvider m_triggerIdProvider = new UniqueIdProvider();

        protected readonly List<Buff> m_buffs = new List<Buff>();

        protected TimedTimerEntry m_placementTimer;
        protected TimedTimerEntry m_turnTimer;

        private bool m_isInitialized;
        private bool m_disposed;

        protected FightTeam[] m_teams;

        public int Id
        {
            get;
            private set;
        }

        public Guid UniqueId
        {
            get;
        }

        public Map Map
        {
            get;
            private set;
        }

        public override Cell[] Cells
        {
            get
            {
                return Map.Cells;
            }
        }

        protected override IReadOnlyCollection<WorldObject> Objects
        {
            get
            {
                return Fighters;
            }
        }

        public abstract FightTypeEnum FightType
        {
            get;
        }

        public abstract bool IsPvP
        {
            get;
        }

        public virtual bool IsMultiAccountRestricted
        {
            get { return false; }
        }

        public FightState State
        {
            get;
            private set;
        }

        public bool IsStarted
        {
            get;
            set;
        }

        public DateTime CreationTime
        {
            get;
            private set;
        }

        public DateTime StartTime
        {
            get;
            private set;
        }

        public short AgeBonus
        {
            get;
            protected set;
        }

        FightTeam IFight.ChallengersTeam
        {
            get { return ChallengersTeam; }
        }

        FightTeam IFight.DefendersTeam
        {
            get { return DefendersTeam; }
        }

        public TRedTeam ChallengersTeam
        {
            get;
            private set;
        }

        public TBlueTeam DefendersTeam
        {
            get;
            private set;
        }

        public FightTeam[] Teams => new FightTeam[] { ChallengersTeam, DefendersTeam };

        public FightTeam Winners
        {
            get;
            protected set;
        }

        public FightTeam Losers
        {
            get;
            protected set;
        }

        public bool Draw
        {
            get;
            protected set;
        }

        public TimeLine TimeLine
        {
            get;
            private set;
        }

        public FightActor FighterPlaying => TimeLine.Current;

        public List<DefaultChallenge> Challenges
        {
            get;
            private set;
        }

        public List<DefaultChallenge> ChallengesProposal
        {
            get;
            private set;
        }

        public DateTime TurnStartTime
        {
            get;
            protected set;
        }

        public ReadyChecker ReadyChecker
        {
            get;
            protected set;
        }

        public ReadOnlyCollection<FightActor> Fighters => TimeLine.Fighters.AsReadOnly();

        public ReadOnlyCollection<FightActor> Leavers
        {
            get { return m_leavers.AsReadOnly(); }
        }

        public ReadOnlyCollection<FightSpectator> Spectators
        {
            get { return m_spectators.AsReadOnly(); }
        }

        public bool SpectatorClosed
        {
            get;
            private set;
        }

        public bool BladesVisible
        {
            get;
            private set;
        }

        public virtual bool IsDeathTemporarily
        {
            get { return false; }
        }

        public virtual bool CanKickPlayer
        {
            get { return true; }
        }

        public bool AIDebugMode
        {
            get;
            set;
        }

        public sbyte ChallengeDropOrXp { get; set; }

        #endregion Properties

        #region Phases

        protected void SetFightState(FightState state)
        {
            UnBindFightersEvents();
            State = state;
            BindFightersEvents();

            //OnStateChanged();
        }

        //protected virtual void OnStateChanged()
        //{
        //    if (State != FightState.Placement && BladesVisible)
        //        HideBlades();
        //}

        public void Initialize()
        {
            if (m_isInitialized)
                return;

            ProcessInitialization();

            m_isInitialized = true;
        }

        protected virtual void ProcessInitialization()
        { }

        public virtual void StartFighting()
        {
            if (State != FightState.Placement && State != FightState.NotStarted) // we can imagine a fight without placement phase
                return;

            SetFightState(FightState.Fighting);
            StartTime = DateTime.Now;
            IsStarted = true;

            HideBlades();
            TimeLine.OrderLine();

            ContextHandler.SendGameEntitiesDispositionMessage(Clients, GetAllFighters());
            ContextHandler.SendGameFightStartMessage(Clients);
            ContextHandler.SendGameFightTurnListMessage(Clients, this);

            ForEach(entry => ContextHandler.SendGameFightSynchronizeMessage(entry.Client, this), true);

            OnFightStarted();
            StartTurn();
        }

        #region EndFight

        public bool CheckFightEnd(bool endFight = true)
        {
            if (DefendersTeam.AreAllDead())
            {
                var wavefighters = DefendersTeam.m_wavesFighters.Where(x => x.WaveNumber == NextWaveNumber && x.MinPlayerLevel <= ChallengersTeam.Fighters.OfType<CharacterFighter>().Count());

                if (wavefighters.Count() > 0)
                {
                    return false;
                }
            }

            if (!ChallengersTeam.AreAllDead() && !DefendersTeam.AreAllDead())
                return false;

            if (endFight)
                EndFight();

            return true;
        }

        public void CancelFight()
        {
            if (CanCancelFight())
            {
                if (State != FightState.Placement)
                {
                    EndFight();
                }
                else
                {
                    SetFightState(FightState.Ended);
                    ContextHandler.SendGameFightEndMessage(Clients, this);

                    foreach (var current in GetCharactersAndSpectators())
                    {
                        current.RejoinMap();
                        current.Companion?.LeaveFight();
                    }

                    Dispose();
                }
            }
        }

        public void EndFight()
        {
            if (State == FightState.Placement)
            {
                CancelFight();
            }

            if (State != FightState.Ended)
            {
                SetFightState(FightState.Ended);

                EndAllSequences();
                ReadyChecker?.Cancel();
                ReadyChecker = ReadyChecker.RequestCheck(this, OnFightEnded, delegate { OnFightEnded(); });
            }
        }

        public event Action<IFight> FightStarted;

        protected virtual void OnFightStarted()
        {
            foreach (var fighter in Fighters)
            {
                fighter.FightStartPosition = fighter.Position.Clone();
                fighter.MovementHistory.RegisterEntry(fighter.FightStartPosition.Cell);
                if (!(fighter is CharacterFighter)) continue;
                var characterFighter = (fighter as CharacterFighter).Character;

                    if (characterFighter.Breed?.BreedInitSpellsIds == null) continue;

                    foreach (var bredInitSpell in characterFighter.Breed.BreedInitSpellsIds)
                    {
                        (fighter as CharacterFighter).CastAutoSpell(new Spell((int)bredInitSpell, 1), (fighter as CharacterFighter).Cell);
                    }
               


            }

            FightStarted?.Invoke(this);
        }

        public event Action<IFight> FightEnded;

        protected virtual void OnFightEnded()
        {
            if (m_turnTimer != null)
                m_turnTimer.Dispose();

            if (ReadyChecker != null)
            {
                ReadyChecker.Cancel();
                ReadyChecker = null;
            }

            DeterminsWinners();

            foreach (var fighter in Fighters.OfType<CharacterFighter>().Where(x => x.IsDisconnected).ToArray())
            {
                if (Winners == fighter.Team)
                    fighter.Team.RemoveLeaver(fighter);
                else
                    fighter.Team.RemoveFighter(fighter);
            }
            foreach (var fighter in GethDcFighters())
            {
                if (this.FightType == FightTypeEnum.FIGHT_TYPE_AGRESSION)
                {
                    FightAgression fight = this as FightAgression;
                    (fighter as CharacterFighter).Character.ToggleAgressionPenality();
                }
            }

            GenerateResults();
            ApplyResults();

            #region Evento Pega-Pega
            Character CharEventWin = null;
            Character CharEventLoser = null;

            //Trabalhando a conclusão do evento pega pega
            if (Settings.PegaPega == true && Map.SubArea.Id == 84 && FightType == FightTypeEnum.FIGHT_TYPE_CHALLENGE)
            {
                try
                {
                    int CountCharacter = 0;
                    Map MapExit = World.Instance.GetMap(165153537);

                    foreach (var Map in Map.SubArea.GetMaps()) //Efetua a contagem dos jogadores na subarea
                    {
                        CountCharacter += Map.GetAllCharacters().Where(x => !x.IsInFight()).Count();
                    }

                    if (CountCharacter == 0) //O Jogador da Luta e o Vencendor
                    {
                        foreach (var FightResults in Results)
                        {
                            foreach (var team in m_teams.Where(entry => entry.Fighters.Any(x => x.Id == (FightResults as FightPlayerResult).Character.Id && !(FightResults as FightPlayerResult).Character.IsGameMaster())))
                            {
                                if (team == Winners || team == Losers && FightResults is FightPlayerResult)
                                {
                                    Character FighterEvent = (FightResults as FightPlayerResult).Character;
                                    CharEventWin = FighterEvent;

                                    if (!FighterEvent.IsGameMaster())
                                    {
                                        World.Instance.ForEachCharacter(character =>
                                        {
                                            character.SendServerMessageLangColor("<b>[Servidor]</b> O jogador <b>" + FighterEvent.Namedefault + "</b> acaba de ganhar o evento Pega-Pega.", "<b>[Server]</b> The player <b>" + FighterEvent.Namedefault + "</b> has just won the Pega-Pega event.", "<b>[Servidor]</b> El jugador <b>" + FighterEvent.Namedefault + "</b> acaba de ganar el evento Pega-Pega.", "<b>[Servidor]</b> Le joueur <b>" + FighterEvent.Namedefault + "</b> vient de remporter l'événement Pega-Pega.", Color.Yellow);
                                        });
                                    }
                                }
                            }
                        }
                    }
                    else if (CountCharacter == 1) //El jugador en el mapa es el ganador.
                    {
                        foreach (var FightResults in Results)
                        {
                            foreach (var team in m_teams.Where(entry => entry.Fighters.Any(x => x.Id == (FightResults as FightPlayerResult).Character.Id && !(FightResults as FightPlayerResult).Character.IsGameMaster())))
                            {
                                if (team == Winners || team == Losers && FightResults is FightPlayerResult)
                                {
                                    Character FighterEvent = (FightResults as FightPlayerResult).Character;
                                    CharEventLoser = FighterEvent;
                                }
                            }
                        }

                        World.Instance.ForEachCharacter(character =>
                        {
                            if (!character.IsGameMaster() && !character.IsInFight() && character.Map.SubArea.Id == 84)
                                CharEventWin = character;
                        });

                        World.Instance.ForEachCharacter(character =>
                        {
                            character.SendServerMessageLangColor("<b>[Servidor]</b> O jogador <b>" + CharEventWin.Namedefault + "</b> acaba de ganhar o evento Pega-Pega.", "<b>[Servidor]</b> The player <b>" + CharEventWin.Namedefault + "</b> has just won the Pega-Pega event.", "<b>[Servidor]</b> El jugador <b>" + CharEventWin.Namedefault + "</b> acaba de ganar el evento Pega-Pega.", "<b>[Servidor]</b> Le joueur <b>" + CharEventWin.Namedefault + "</b> vient de remporter l'événement Pega-Pega.", Color.Yellow);
                        });
                    }
                    else
                    {
                        foreach (var FightResults in Results)
                        {
                            foreach (var team in m_teams.Where(entry => entry.Fighters.Any(x => x.Id == (FightResults as FightPlayerResult).Character.Id && !(FightResults as FightPlayerResult).Character.IsGameMaster())))
                            {
                                if (team == Winners || team == Losers && FightResults is FightPlayerResult)
                                {
                                    Character FighterEvent = (FightResults as FightPlayerResult).Character;

                                    CharEventLoser = FighterEvent;
                                }
                            }
                        }
                    }
                }
                catch
                { }
            }
            #endregion

            foreach (var fightResult in Results.Where(x => (x is FightPlayerResult) && ((FightPlayerResult)x).Fighter.IsDisconnected))
            {
                ((FightPlayerResult)fightResult).Fighter.Character.SpellsModifications = new List<SpellModifierMessage>();
                WorldServer.Instance.IOTaskPool.ExecuteInContext(new Action(((FightPlayerResult)fightResult).Fighter.Character.SaveNow));
            }

            ContextHandler.SendGameFightEndMessage(Clients, this, Results.Select(entry => entry.GetFightResultListEntry()));

            ResetFightersProperties();

            foreach (var character in GetCharactersAndSpectators())
            {
                character.RejoinMap();
            }

            Dispose();

            FightEnded?.Invoke(this);

            foreach (var c in Map.GetAllCharacters())
            {
                Map.Refresh(c);
            }

            if (Settings.PegaPega == true)
            {
                if (Map.SubArea.Id == 84)
                {
                    //Retirando o ganhador do Pega-Pega do Mapa
                    if (CharEventWin != null)
                    {
                        if (EventExitMap(CharEventWin))
                        {
                            var msg = "Pega-Pega";
                            var itemtemplate = ItemManager.Instance.TryGetTemplate(Settings.PrimaryItemIdReturn);
                            Gifts.Gifts.Instance.SetGift(CharEventWin.Account.Id, msg, itemtemplate, 1, false, false, false, true);
                        }
                    }

                    //Retirando o Perdedor do Pega-Pega do Mapa
                    if (CharEventLoser != null)
                    {
                        EventExitMap(CharEventLoser);
                    }
                }
            }
        }

        private bool EventExitMap(Character character)
        {
            return character.Teleport(new ObjectPosition(Singleton<World>.Instance.GetMap(165153537), (short)271, DirectionsEnum.DIRECTION_SOUTH_WEST));
        }

        public event FightWinnersDelegate WinnersDetermined;

        protected virtual void OnWinnersDetermined(FightTeam winners, FightTeam losers, bool draw)
        {
            WinnersDetermined?.Invoke(this, winners, losers, draw);
        }

        protected virtual void DeterminsWinners()
        {
            if (DefendersTeam.AreAllDead() && !ChallengersTeam.AreAllDead())
            {
                Winners = ChallengersTeam;
                Losers = DefendersTeam;
                Draw = false;
            }
            else if (!DefendersTeam.AreAllDead() && ChallengersTeam.AreAllDead())
            {
                Winners = DefendersTeam;
                Losers = ChallengersTeam;
                Draw = false;
            }
            else Draw = true;

            OnWinnersDetermined(Winners, Losers, Draw);
        }

        protected void ResetFightersProperties()
        {
            foreach (var fighter in Fighters)
            {
                fighter.ResetFightProperties();
            }
        }

        public event Action<IFight> GeneratingResults;

        public event ResultsGeneratedDelegate ResultsGenerated;

        protected List<IFightResult> Results
        {
            get;
            set;
        }

        protected void GenerateResults()
        {
            GeneratingResults?.Invoke(this);

            var results = GetResults();

            ResultsGenerated?.Invoke(this, results);

            Results = results;
        }

        protected virtual List<IFightResult> GetResults()
        {
            return new List<IFightResult>();
        }

        protected virtual IEnumerable<IFightResult> GenerateLeaverResults(CharacterFighter leaver, out IFightResult leaverResult)
        {
            leaverResult = null;
            var list = new List<IFightResult>();

            foreach (var fighter in GetFightersAndLeavers().Where(entry => entry.HasResult))
            {
                var result = fighter.GetFightResult(fighter.Team == leaver.Team ? FightOutcomeEnum.RESULT_LOST : FightOutcomeEnum.RESULT_VICTORY);

                if (fighter == leaver)
                    leaverResult = result;

                list.Add(result);
            }

            return list;
        }

        protected virtual void ApplyResults()
        {
            foreach (var fightResult in Results.Where(fightResult => !fightResult.HasLeft ||
                !(fightResult is FightPlayerResult) ||
                (((FightPlayerResult)fightResult).Fighter.IsDisconnected)))
            {
                fightResult.Apply();
            }
        }

        protected void Dispose()
        {
            if (m_disposed)
                return;

            m_disposed = true;

            foreach (var fighter in Fighters)
            {
                fighter.Delete();
            }

            OnDisposed();

            UnBindFightersEvents();

            Map.RemoveFight(this);
            FightManager.Instance.Remove(this);
            GC.SuppressFinalize(this);
        }

        protected virtual void OnDisposed()
        {
            Clients.Dispose();

            if (ReadyChecker != null)
                ReadyChecker.Cancel();

            if (m_placementTimer != null)
                m_placementTimer.Dispose();

            if (m_turnTimer != null)
                m_turnTimer.Dispose();
        }

        #endregion EndFight

        #region Placement

        public virtual void StartPlacement()
        {
            if (State != FightState.NotStarted)
                return;

            SetFightState(FightState.Placement);

            RandomnizePositions(ChallengersTeam);
            RandomnizePositions(DefendersTeam);

            ShowBlades();
            Map.AddFight(this);
        }

        public void RefreshActor(FightActor actor)
        {
            if (actor.HasLeft())
                return;

            ForEach(entry => ContextHandler.SendGameFightShowFighterMessage(entry.Client, actor), true);
        }

        private void OnStatsRefreshed(Character character)
        {
            ForEach(entry => ContextHandler.SendGameFightShowFighterMessage(entry.Client, character.Fighter), true);
        }

        #region Blades

        private void FindBladesPlacement()
        {
            if (ChallengersTeam.Leader.MapPosition.Cell.Id != DefendersTeam.Leader.MapPosition.Cell.Id)
            {
                ChallengersTeam.BladePosition = ChallengersTeam.Leader.MapPosition.Clone();
                DefendersTeam.BladePosition = DefendersTeam.Leader.MapPosition.Clone();
            }
            else
            {
                var cell = Map.GetRandomAdjacentFreeCell(ChallengersTeam.Leader.MapPosition.Point);

                // if cell not found we superpose both blades
                if (cell == null)
                {
                    ChallengersTeam.BladePosition = ChallengersTeam.Leader.MapPosition.Clone();
                }
                else // else we take an adjacent cell
                {
                    var pos = ChallengersTeam.Leader.MapPosition.Clone();
                    pos.Cell = cell;
                    ChallengersTeam.BladePosition = pos;
                }

                DefendersTeam.BladePosition = DefendersTeam.Leader.MapPosition.Clone();
            }
        }

        public void ShowBlades()
        {
            if (BladesVisible || State != FightState.Placement)
                return;

            if (ChallengersTeam.BladePosition == null ||
                DefendersTeam.BladePosition == null)
                FindBladesPlacement();

            //        ContextHandler.SendGameRolePlayShowChallengeMessage(Map.Clients, this);
            var CharactersOnMap = Map.GetAllCharacters().Where(x => x.IsFighting() == false).ToClients();

            ContextHandler.SendGameRolePlayShowChallengeMessage(CharactersOnMap, this);
            var Characters = Map.GetAllCharacters().Where(x => x.IsFighting() == false).ToList();
            foreach (var c in Characters)
            {
                this.Map.Refresh(c);
            }
            ChallengersTeam.TeamOptionsChanged += OnTeamOptionsChanged;
            DefendersTeam.TeamOptionsChanged += OnTeamOptionsChanged;

            BladesVisible = true;
        }

        public void HideBlades()
        {
            if (!BladesVisible)
                return;
            var CharactersOnMap = Map.GetAllCharacters().Where(x => x.IsFighting() == false).ToClients();
            ContextHandler.SendGameRolePlayRemoveChallengeMessage(CharactersOnMap, this);
            var Characters = Map.GetAllCharacters().Where(x => x.IsFighting() == false).ToList();
            foreach (var c in Characters)
            {
                this.Map.Refresh(c);
            }
            ChallengersTeam.TeamOptionsChanged -= OnTeamOptionsChanged;
            DefendersTeam.TeamOptionsChanged -= OnTeamOptionsChanged;

            BladesVisible = false;
        }

        public void UpdateBlades(FightTeam team)
        {
            if (!BladesVisible)
                return;

            ContextHandler.SendGameFightUpdateTeamMessage(Map.Clients, this, team);
        }

        private void OnTeamOptionsChanged(FightTeam team, FightOptionsEnum option)
        {
            ContextHandler.SendGameFightOptionStateUpdateMessage(Clients, team, option, team.GetOptionState(option));
            ContextHandler.SendGameFightOptionStateUpdateMessage(Map.Clients, team, option, team.GetOptionState(option));
        }

        #endregion Blades

        public virtual TimeSpan GetPlacementTimeLeft() => TimeSpan.Zero;

        #region Placement methods

        public bool FindRandomFreeCell(FightActor fighter, out Cell cell, bool placement = true)
        {
            var availableCells = fighter.Team.PlacementCells.Where(entry => GetOneFighter(entry) == null || GetOneFighter(entry) == fighter).ToArray();

            var random = new Random();

            if (availableCells.Length == 0 && placement)
            {
                cell = null;
                return false;
            }

            // if not in placement phase, get a random free cell on the map
            if (availableCells.Length == 0 && !placement)
            {
                var cells = Enumerable.Range(0, (int)MapPoint.MapSize).ToList();
                foreach (var actor in GetAllFighters(actor => cells.Contains(actor.Cell.Id)))
                {
                    cells.Remove(actor.Cell.Id);
                }

                foreach (var a in Map.Cells)
                {
                    if (!a.Walkable)
                        cells.Remove(a.Id);
                }
                cell = Map.Cells[cells[random.Next(cells.Count)]];

                return true;
            }

            cell = availableCells[random.Next(availableCells.Length)];

            return true;
        }

        public bool RandomnizePosition(FightActor fighter)
        {
            if (State != FightState.Placement)
                throw new Exception("State != Placement, cannot random placement position");
            Cell cell;
            if (!FindRandomFreeCell(fighter, out cell))
            {
                if (fighter is CharacterFighter)
                    ((CharacterFighter)fighter).LeaveFight(); // no place more than we kick the actor to avoid bugs
                else
                    fighter.Team.RemoveFighter(fighter);
                return false;
            }

            fighter.ChangePrePlacement(cell);
            return true;
        }

        public void RandomnizePositions(FightTeam team)
        {
            if (State != FightState.Placement)
                throw new Exception("State != Placement, cannot random placement position");
            var shuffledCells = team.PlacementCells.Shuffle().ToList();
            if (shuffledCells.Count() < team.GetAllFighters().Count())
            {
                foreach (var fighter in team.GetAllFighters())
                {
                    Cell cell;
                    FindRandomFreeCell(fighter, out cell);
                    fighter.ChangePrePlacement(cell);
                }




            }
            else
                foreach (var fighter in team.GetAllFighters())
                {
                    fighter.ChangePrePlacement(shuffledCells.FirstOrDefault());
                    shuffledCells.RemoveAt(0);
                    shuffledCells = shuffledCells.Shuffle().ToList();
                }
        }

        public DirectionsEnum FindPlacementDirection(FightActor fighter)
        {
            if (State != FightState.Placement)
                throw new Exception("State != Placement, cannot give placement direction");

            var team = fighter.OpposedTeam;

            Tuple<Cell, uint> closerCell = null;
            foreach (var opposant in team.GetAllFighters())
            {
                var point = opposant.Position.Point;

                if (closerCell == null)
                    closerCell = Tuple.Create(opposant.Cell,
                                              fighter.Position.Point.ManhattanDistanceTo(point));
                else
                {
                    if (fighter.Position.Point.ManhattanDistanceTo(point) < closerCell.Item2)
                        closerCell = Tuple.Create(opposant.Cell,
                                                  fighter.Position.Point.ManhattanDistanceTo(point));
                }
            }

            return closerCell == null ? fighter.Position.Direction : fighter.Position.Point.OrientationTo(new MapPoint(closerCell.Item1), false);
        }

        protected virtual bool CanKickFighter(FightActor kicker, FightActor kicked)
        {
            return State == FightState.Placement && kicker.IsTeamLeader() && kicked.Team == kicker.Team;
        }

        public bool KickFighter(FightActor kicker, FightActor fighter)
        {
            if (!Fighters.Contains(fighter))
                return false;

            if (!CanKickFighter(kicker, fighter))
                return false;

            fighter.Team.RemoveFighter(fighter);
            CharacterFighter characterFighter = fighter as CharacterFighter;
            if (characterFighter != null)
            {
                characterFighter.Character.RejoinMap();
            }

            characterFighter.Character.Companion?.LeaveFight();

            CheckFightEnd();

            return true;
        }

        /// <summary>
        ///   Set the ready state of a character
        /// </summary>
        protected virtual void OnSetReady(FightActor fighter, bool isReady)
        {
            if (State != FightState.Placement)
                return;

            ContextHandler.SendGameFightHumanReadyStateMessage(Clients, fighter);

            if (ChallengersTeam.AreAllReady() && DefendersTeam.AreAllReady())
                StartFighting();
        }

        /// <summary>
        ///   Check if a character can change position (before the fight is started).
        /// </summary>
        /// <param name = "fighter"></param>
        /// <param name="cell"></param>
        /// <returns>If change is possible</returns>
        public virtual bool CanChangePosition(FightActor fighter, Cell cell)
        {
            var figtherOnCell = GetOneFighter(cell);

            return State == FightState.Placement &&
                   fighter.Team.PlacementCells.Contains(cell) &&
                   (figtherOnCell == fighter || figtherOnCell == null);
        }

        protected virtual void OnSwapPreplacementPosition(FightActor fighter, FightActor actor)
        {
            UpdateFightersPlacementDirection();
            ContextRoleplayHandler.SendGameFightPlacementSwapPositionsMessage(Clients, new[] { fighter, actor });
        }

        protected virtual void OnChangePreplacementPosition(FightActor fighter, ObjectPosition objectPosition)
        {
            UpdateFightersPlacementDirection();
            ContextHandler.SendGameEntitiesDispositionMessage(Clients, GetAllFighters());
        }

        protected void UpdateFightersPlacementDirection()
        {
            foreach (FightActor fighter in Fighters)
            {
                fighter.Position.Direction = FindPlacementDirection(fighter);
            }
        }

        #endregion Placement methods

        #endregion Placement

        #endregion Phases

        #region Add/Remove Fighter

        protected virtual void OnFighterAdded(FightTeam team, FightActor actor)
        {
            if (State == FightState.Ended)
            {
                return;
            }

            if (actor is SummonedFighter)
            {
                OnSummonAdded(actor as SummonedFighter);
                return;
            }
            else if (actor is SummonedBomb)
            {
                OnBombAdded(actor as SummonedBomb);
                return;
            }

            TimeLine.Fighters.Add(actor);
            BindFighterEvents(actor);

            if (State == FightState.Placement)
            {
                if (!RandomnizePosition(actor))
                    return;
            }

            if (actor is CompanionActor)
            {
                var companion = actor as CompanionActor;

                if (companion != null)
                {
                    OnCompanionAdded(companion);
                }
            }

            if (actor is CharacterFighter)
            {
                OnCharacterAdded(actor as CharacterFighter);
            }

            ForEach(entry => ContextHandler.SendGameFightShowFighterMessage(entry.Client, actor), true);
            UpdateBlades(team);

            ContextHandler.SendGameFightTurnListMessage(Clients, this);
        }

        protected virtual void OnSummonAdded(SummonedFighter fighter)
        {
            TimeLine.InsertFighter(fighter, TimeLine.Fighters.IndexOf(fighter.Summoner) + 1);
            BindFighterEvents(fighter);
            fighter.OnGetAlive();

            ContextHandler.SendGameFightTurnListMessage(Clients, this);
        }

        protected virtual void OnBombAdded(SummonedBomb bomb)
        {
            TimeLine.InsertFighter(bomb, TimeLine.Fighters.IndexOf(bomb.Summoner) + 1);
            BindFighterEvents(bomb);

            ContextHandler.SendGameFightTurnListMessage(Clients, this);
        }

        protected virtual void OnCharacterAdded(CharacterFighter fighter)
        {
            var character = fighter.Character;

            character.RefreshActor();

            if (character.ArenaPopup != null)
                character.ArenaPopup.Deny();

            Clients.Add(character.Client);

            SendGameFightJoinMessage(fighter);

            if (State == FightState.Placement || State == FightState.NotStarted)
            {
                ContextHandler.SendGameFightPlacementPossiblePositionsMessage(character.Client, this, (sbyte)fighter.Team.Id);
            }

            foreach (var fightMember in GetAllFighters())
                ContextHandler.SendGameFightShowFighterMessage(character.Client, fightMember);

            ContextHandler.SendGameEntitiesDispositionMessage(character.Client, GetAllFighters());

            ContextHandler.SendGameFightUpdateTeamMessage(character.Client, this, ChallengersTeam);
            ContextHandler.SendGameFightUpdateTeamMessage(character.Client, this, DefendersTeam);

            ContextHandler.SendGameFightUpdateTeamMessage(Clients, this, fighter.Team);
        }

        protected virtual void OnCompanionAdded(CompanionActor fighter)
        {

            var character = fighter.Master;

            character.RefreshActor();

            if (character.ArenaPopup != null)
                character.ArenaPopup.Deny();

            //Clients.Add(character.Client);

            //SendGameFightJoinMessage(fighter);

            if (State == FightState.Placement || State == FightState.NotStarted)
            {
                ContextHandler.SendGameFightPlacementPossiblePositionsMessage(character.Client, this, (sbyte)fighter.Team.Id);
            }

            foreach (var fightMember in GetAllFighters())
                ContextHandler.SendGameFightShowFighterMessage(character.Client, fightMember);

            ContextHandler.SendGameEntitiesDispositionMessage(character.Client, GetAllFighters());

            ContextHandler.SendGameFightUpdateTeamMessage(character.Client, this, ChallengersTeam);
            ContextHandler.SendGameFightUpdateTeamMessage(character.Client, this, DefendersTeam);

            ContextHandler.SendGameFightUpdateTeamMessage(Clients, this, fighter.Team);
        }


        protected virtual void OnFighterRemoved(FightTeam team, FightActor actor)
        {
            if (actor is SummonedFighter)
            {
                OnSummonRemoved(actor as SummonedFighter);
                return;
            }

            if (actor is SummonedBomb)
            {
                OnBombRemoved(actor as SummonedBomb);
                return;
            }

            TimeLine.RemoveFighter(actor);
            UnBindFighterEvents(actor);

            if (actor is CharacterFighter)
                OnCharacterRemoved(actor as CharacterFighter);

            switch (State)
            {
                case FightState.Placement:
                    ContextHandler.SendGameFightRemoveTeamMemberMessage(Clients, actor);
                    ContextHandler.SendGameFightRemoveTeamMemberMessage(Map.Clients, actor);
                    break;

                case FightState.Fighting:
                    ContextHandler.SendGameContextRemoveElementMessage(Clients, actor);
                    break;
            }

            UpdateBlades(team);
            //if(team.Fight is FightPvT)
            //UpdateBlades(team.OpposedTeam);
        }

        protected virtual void OnSummonRemoved(SummonedFighter fighter)
        {
            TimeLine.RemoveFighter(fighter);
            UnBindFighterEvents(fighter);

            ContextHandler.SendGameFightTurnListMessage(Clients, this);
        }

        protected virtual void OnBombRemoved(SummonedBomb bomb)
        {
            TimeLine.RemoveFighter(bomb);
            UnBindFighterEvents(bomb);

            ContextHandler.SendGameFightTurnListMessage(Clients, this);
        }

        protected virtual void OnCharacterRemoved(CharacterFighter fighter)
        {
            fighter.Character.Companion?.LeaveFight();
            Clients.Remove(fighter.Character.Client);

        }

        #endregion Add/Remove Fighter

        #region Spectators

        public void ToggleSpectatorClosed(Character character, bool state)
        {
            SpectatorClosed = state;

            // Spectator mode Activated/Disabled
            BasicHandler.SendTextInformationMessage(Clients, TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, (short)(SpectatorClosed ? 40 : 39), character.Name);

            if (state)
                RemoveAllSpectators();

            OnTeamOptionsChanged(ChallengersTeam, FightOptionsEnum.FIGHT_OPTION_SET_SECRET);
            OnTeamOptionsChanged(DefendersTeam, FightOptionsEnum.FIGHT_OPTION_SET_SECRET);
        }

        public virtual bool CanSpectatorJoin(Character spectator) => (!SpectatorClosed && (State == FightState.Placement || State == FightState.Fighting)) || spectator.IsGameMaster();

        public bool AddSpectator(FightSpectator spectator)
        {
            if (!CanSpectatorJoin(spectator.Character))
                return false;

            m_spectators.Add(spectator);
            spectator.JoinTime = DateTime.Now;
            spectator.Left += OnSpectectorLeft;
            spectator.Character.LoggedOut += OnSpectatorLoggedOut;

            Clients.Add(spectator.Client);
            SpectatorClients.Add(spectator.Client);

            OnSpectatorAdded(spectator);

            return true;
        }

        protected virtual void OnSpectatorAdded(FightSpectator spectator)
        {
            SendGameFightSpectatorJoinMessage(spectator);

            if (State == FightState.Placement || State == FightState.NotStarted)
            {
                ContextHandler.SendGameFightPlacementPossiblePositionsMessage(spectator.Client, this, 0);
            }

            foreach (var fighter in GetAllFighters())
            {
                ContextHandler.SendGameFightShowFighterMessage(spectator.Client, fighter);
            }

            ContextHandler.SendGameFightTurnListMessage(spectator.Client, this);
            ContextHandler.SendGameFightSpectateMessage(spectator.Client, this);
            ContextHandler.SendGameFightNewRoundMessage(spectator.Client, TimeLine.RoundNumber);
            CharacterHandler.SendCharacterStatsListMessage(spectator.Client);
            ContextHandler.SendChallengeListMessage(spectator.Client, spectator.Client.Character.Fight);

            if (!spectator.Character.Invisible)
            {
                // Spectator 'X' joined
                BasicHandler.SendTextInformationMessage(Clients, TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 36, spectator.Character.Name);
            }

            using (StartSequence(SequenceTypeEnum.SEQUENCE_TURN_START))
            {
                PortalsManager.RefreshClientPortals(spectator.Client);
            }

            if (TimeLine.Current != null)
            {
                ContextHandler.SendGameFightTurnResumeMessage(spectator.Client, FighterPlaying);
            }
        }

        protected virtual void OnSpectatorLoggedOut(Character character)
        {
            if (!character.IsSpectator())
                return;

            OnSpectectorLeft(character.Spectator);
        }

        protected virtual void OnSpectectorLeft(FightSpectator spectator)
        {
            RemoveSpectator(spectator);
        }

        public void RemoveSpectator(FightSpectator spectator)
        {
            m_spectators.Remove(spectator);

            Clients.Remove(spectator.Character.Client);
            SpectatorClients.Remove(spectator.Client);

            spectator.Left -= OnSpectectorLeft;
            spectator.Character.LoggedOut -= OnSpectatorLoggedOut;

            OnSpectatorRemoved(spectator);
        }

        protected virtual void OnSpectatorRemoved(FightSpectator spectator)
        {
            spectator.Character.RejoinMap();
        }

        public void RemoveAllSpectators()
        {
            foreach (var spectator in m_spectators.GetRange(0, Spectators.Count))
            {
                RemoveSpectator(spectator);
            }
        }

        #endregion Spectators

        #region Turn Management

        public void StartTurn()
        {
            if (State != FightState.Fighting)
                return;

            if (!CheckFightEnd())
            {
                OnTurnStarted();
            }
        }

        public bool PrematuredWave = false;
        public int NextWaveNumber = 1;
        public int[] WaveTurns = new int[] { 3, 6, 8, 10, 13, 20, 25, 31, 36, 41, 46, 51, 56, 61, 66, 71, 76, 81, 86, 91, 96, 105, 110, 115 };

        public void DeadWaveSpawn()
        {
            if (DefendersTeam.AreAllDead())
            {
                var MobsToSPawnNumber = ChallengersTeam.Fighters.OfType<CharacterFighter>().Count();

                if ((ChallengersTeam.Fighters.OfType<CharacterFighter>().Count() < 5))
                    MobsToSPawnNumber = 1;

                var wavefighters = DefendersTeam.m_wavesFighters.Where(x => x.WaveNumber == NextWaveNumber && x.MinPlayerLevel == MobsToSPawnNumber).ToList();

                if (wavefighters.Count() > 0)
                {
                    PrematuredWave = true;

                    var wavefight = wavefighters.Take(ChallengersTeam.Fighters.OfType<CharacterFighter>().Count()).ToList();

                    if (ChallengersTeam.Fighters.OfType<CharacterFighter>().Count() < 4)
                        wavefight = wavefighters.Take(4).ToList();

                    SpawnWaveFighters(wavefight);
                }
            }
        }

        public void SpawnWaveFighters(List<MonsterFighter> fighters)
        {
            var freeplacements = DefendersTeam.PlacementCells.Where(x => GetOneFighter(x) is null).ToList();
            var usedCells = new HashSet<Cell>(freeplacements);

            foreach (var fighter in fighters)
            {
                Cell cell = null;

                if (freeplacements is null || freeplacements.Count > 0)
                {
                    cell = freeplacements[0];
                    freeplacements.RemoveAt(0);
                }
                else
                {
                    cell = Map.Cells.Where(x => GetOneFighter(x) is null && x.Walkable && !usedCells.Contains(x)).OrderBy(v => new MapPoint(DefendersTeam.PlacementCells[0]).ManhattanDistanceTo(v)).FirstOrDefault();

                    if (cell != null)
                    {
                        usedCells.Add(cell);
                    }
                }

                if (cell != null)
                {
                    fighter.Cell = cell;
                }

                DefendersTeam.AddFighter(fighter);
            }

            NextWaveNumber++;
        }

        public event Action<IFight, FightActor> TurnStarted;

        protected virtual void OnTurnStarted()
        {
            PrematuredStartWaveOccured = false;
            ResetSequences();

            #region >> Waves

            if (TimeLine.NewRound)
            {
                if (DefendersTeam.AreAllDead())
                {
                    if (PrematuredWave)
                    {
                        PrematuredWave = false;
                    }
                    else
                    {
                        var MobsToSPawnNumber = ChallengersTeam.Fighters.OfType<CharacterFighter>().Count();

                        if ((ChallengersTeam.Fighters.OfType<CharacterFighter>().Count() < 5))
                            MobsToSPawnNumber = 1;

                        var wavefighters = DefendersTeam.m_wavesFighters.Where(x => x.WaveNumber == NextWaveNumber && x.MinPlayerLevel == MobsToSPawnNumber).ToList();

                        if (wavefighters.Count() > 0)
                        {
                            var wavefight = wavefighters.ToList().Take(ChallengersTeam.Fighters.OfType<CharacterFighter>().Count()).ToList();

                            if (ChallengersTeam.Fighters.OfType<CharacterFighter>().Count() < 4)
                                wavefight = wavefighters.Take(4).ToList();

                            SpawnWaveFighters(wavefight);
                        }
                    }
                }

                ContextHandler.SendGameFightNewRoundMessage(Clients, TimeLine.RoundNumber);
            }

            #endregion

            #region >> Actor Companion

            var actor = FighterPlaying as CompanionActor;

            if (actor != null)
            {
                var companion = actor;
                ContextHandler.SendSlaveSwitchContextMessage(companion.Master.Client, companion);
            }

            ContextHandler.SendGameFightTurnStartMessage(Clients, FighterPlaying.Id, FighterPlaying.TurnTime / 100);

            #endregion

            using (StartSequence(SequenceTypeEnum.SEQUENCE_TURN_START))
            {
                if (!FighterPlaying.IsSummoned())
                {
                    FighterPlaying.DecrementAllCastedBuffsDuration();
                    FighterPlaying.DecrementSummonsCastedBuffsDuration();
                }

                foreach (var passedFighter in TimeLine.PassedActors)
                {
                    passedFighter.DecrementAllCastedBuffsDuration();
                    passedFighter.DecrementSummonsCastedBuffsDuration();
                }

                //Start Turn Character
                //Debug Sacrier by Kenshin v2.61.10
                if (FighterPlaying is CharacterFighter && (FighterPlaying as CharacterFighter).Owner != null)
                {
                    var playing = FighterPlaying as CharacterFighter;

                    //Sacrieur
                    if (playing.Owner.BreedId == PlayableBreedEnum.Sacrieur && playing.GetBuffs().Count() > 0)
                    {
                        if (playing.GetBuffs(x => x.Spell.Id == (int)SpellIdEnum.MUTILATION_12737).Any())
                        {
                            playing.Owner.Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.MUTILATION_13996, 4), playing.Owner.Fighter.Cell);
                            playing.Owner.Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.SUFFERING_EVOLUTION_TRIGGERED_01_14087, 1), playing.Owner.Fighter.Cell);
                        }
                    }

                    // Osamodas
                    if (playing.Owner.BreedId == PlayableBreedEnum.Osamodas)
                    {
                        var playerStates = playing.GetStates();

                        List<int> states = new List<int>
                        {
                            // Tofu
                            (int)SpellStatesEnum.TOFUCHARGE_14_1213,
                            (int)SpellStatesEnum.TOFUCHARGE_24_1214,
                            (int)SpellStatesEnum.TOFUCHARGE_34_1215,
                            (int)SpellStatesEnum.TOFUCHARGE_44_1216,

                            // Gobbal
                            (int)SpellStatesEnum.GOBBACHARGE_14_1217,
                            (int)SpellStatesEnum.GOBBACHARGE_24_1218,
                            (int)SpellStatesEnum.GOBBACHARGE_34_1219,
                            (int)SpellStatesEnum.GOBBACHARGE_44_1220,

                            // Sapo
                            (int)SpellStatesEnum.TOACHARGE_14_1221,
                            (int)SpellStatesEnum.TOACHARGE_24_1222,
                            (int)SpellStatesEnum.TOACHARGE_34_1223,
                            (int)SpellStatesEnum.TOACHARGE_44_1224,

                            // Dragão
                            (int)SpellStatesEnum.WYRMLICHARGE_14_1225,
                            (int)SpellStatesEnum.WYRMLICHARGE_24_1226,
                            (int)SpellStatesEnum.WYRMLICHARGE_34_1227,
                            (int)SpellStatesEnum.WYRMLICHARGE_44_1228
                        };

                        bool hasState = playerStates.Any(playerState => states.Contains(playerState.State.Id) && !playerState.IsDisabled);

                        //Console.WriteLine($"Player states: {string.Join(", ", playerStates.Select(s => s.State.Id))}");
                        //Console.WriteLine($"Has valid state: {hasState}");

                        if (!hasState && !playing.HasState((int)SpellStatesEnum.UNCHARGED_1212))
                        {
                            EffectDice dice = new EffectDice(EffectsEnum.Effect_AddState, (int)SpellStatesEnum.UNCHARGED_1212, 0, 0);
                            dice.ZoneShape = SpellShapeEnum.P;
                            dice.ZoneSize = 1;
                            dice.IsDirty = true;
                            dice.Triggers = "I";
                            dice.Duration = -1;
                            dice.TargetMask = "C";

                            var handler = EffectManager.Instance.GetSpellEffectHandler(dice, playing, new DefaultSpellCastHandler(new SpellCastInformations(playing, new Spell((int)SpellIdEnum.OSAMODASS_WHIP_14155, 1), playing.Cell)), playing.Cell, false);
                            handler.Apply();
                        }
                    }
                }

                PortalsManager.RefreshGamePortals();

                DecrementGlyphDuration(FighterPlaying);
                TriggerMarks(FighterPlaying.Cell, FighterPlaying, TriggerType.OnTurnBegin);
                FighterPlaying.TriggerBuffs(FighterPlaying, BuffTriggerType.OnTurnBegin);
            }

            // can die with triggers
            if (CheckFightEnd())
                return;

            if (FighterPlaying.MustSkipTurn())
            {
                FighterPlaying.ResetUsedPoints();
                PassTurn();

                return;
            }

            ForEach(entry => ContextHandler.SendGameFightSynchronizeMessage(entry.Client, this), true);
            ForEach(entry => entry.RefreshStats());

            if (FighterPlaying is CharacterFighter)
            {
                var characterFighter = FighterPlaying as CharacterFighter;

                ContextHandler.SendGameFightTurnStartPlayingMessage(characterFighter.Character.Client);
                ContextHandler.SendFighterStatsListMessage(characterFighter.Character.Client, characterFighter.Character);

            }
            else if (FighterPlaying is SummonedFighter && (FighterPlaying as SummonedFighter).IsControlled())
            {
                ContextHandler.SendGameFightTurnStartPlayingMessage((FighterPlaying as SummonedFighter).Controller.Character.Client);
            }

            FighterPlaying.TurnStartPosition = FighterPlaying.Position.Clone();

            TurnStartTime = DateTime.Now;

            if (!Freezed)
            {
                if (!Map.Area.IsRunning)
                    Map.Area.Start();

                m_turnTimer = Map.Area.CallDelayed(FighterPlaying.TurnTime, StopTurn);
            }

            TurnStarted?.Invoke(this, FighterPlaying);
        }

        public void StopTurn()
        {
            if (State != FightState.Fighting)
                return;

            if (m_turnTimer != null)
                m_turnTimer.Dispose();

            if (ReadyChecker != null)
            {

                logger.Debug("Last ReadyChecker was not disposed. (Stop Turn) Ator :" + FighterPlaying);

                /*  try
                  {
                      (FighterPlaying as CharacterFighter).Character.OpenPopup("Ocorreu um erro, se persistir, porfavor contacte os administradores!");
                      (FighterPlaying as CharacterFighter).Character.SendServerMessage("Ocorreu um erro, se persistir, porfavor contacte os administradores!");
                  }
                  catch { }*/

                ReadyChecker.Cancel();
                ReadyChecker = null;


                if (CheckFightEnd())
                    return;
                ReadyChecker = ReadyChecker.RequestCheck(this, PassTurnAndCheck, LagAndPassTurn);
            }
            else
            {

                if (CheckFightEnd())
                    return;

                OnTurnStopped();
                ReadyChecker = ReadyChecker.RequestCheck(this, PassTurnAndCheck, LagAndPassTurn);
            }
        }

        public event Action<IFight, FightActor> BeforeTurnStopped;

        public event Action<IFight, FightActor> TurnStopped;

        protected virtual void OnTurnStopped()
        {
            BeforeTurnStopped?.Invoke(this, FighterPlaying);

            using (StartSequence(SequenceTypeEnum.SEQUENCE_TURN_END))
            {

                if (FighterPlaying.IsAlive())
                {
                    FighterPlaying.TriggerBuffs(FighterPlaying, BuffTriggerType.OnTurnEnd);
                    FighterPlaying.TriggerBuffsRemovedOnTurnEnd();
                    TriggerMarks(FighterPlaying.Cell, FighterPlaying, TriggerType.OnTurnEnd);
                }

                var time = (int)Math.Floor(GetTurnTimeLeft().TotalSeconds / 2);
                if (TimeLine.RoundNumber > 1)
                    FighterPlaying.TurnTimeReport = time > 0 ? time : 0;
            }

            // can die with triggers
            if (CheckFightEnd())
                return;

            if (IsSequencing)
                EndAllSequences();

            TurnStopped?.Invoke(this, FighterPlaying);

            ContextHandler.SendGameFightTurnEndMessage(Clients, FighterPlaying);
        }

        protected void LagAndPassTurn(NamedFighter[] laggers)
        {
            if (ReadyChecker == null)
                return;

            // some guys are lagging !
            OnLaggersSpotted(laggers);

            PassTurnAndCheck();
        }

        protected void PassTurnAndCheck()
        {
            if (ReadyChecker == null)
                return;

            ReadyChecker = null;

            FighterPlaying.ResetUsedPoints();
            PassTurn();
        }

        protected void PassTurn()
        {
            if (State != FightState.Fighting)
                return;

            if (CheckFightEnd())
                return;

            redo:
            if (!TimeLine.SelectNextFighter())
            {
                if (!CheckFightEnd())
                {
                    logger.Error("Something goes wrong : no more actors are available to play but the fight is not ended");
                }

                return;
            }

            // player left but is disconnected
            // pass turn is there are others players
            if (FighterPlaying.HasLeft() && FighterPlaying is CharacterFighter)
            {
                var leaver = (CharacterFighter)FighterPlaying;
                if (leaver.IsDisconnected &&
                    leaver.LeftRound + FightConfiguration.TurnsBeforeDisconnection <= TimeLine.RoundNumber)
                {
                    leaver.Die();

                    if (CheckFightEnd())
                        return;
                    IFightResult leaverResult;
                    var results = GenerateLeaverResults(leaver, out leaverResult);

                    leaverResult.Apply();

                    ContextHandler.SendGameFightLeaveMessage(Clients, leaver);

                    leaver.ResetFightProperties();

                    leaver.Team.AddLeaver(leaver);
                    m_leavers.Add(leaver);
                    leaver.Team.RemoveFighter(leaver);

                    leaver.LeaveDisconnectedState(false);

                    leaver.Character.RejoinMap();

                    // logger.Info("save from in queue {0}","fight" );
                    leaver.Character.SaveLater();

                    goto redo;
                }

                // <b>%1</b> vient d'être déconnecté, il quittera la partie dans <b>%2</b> tour(s) s'il ne se reconnecte pas avant.
                BasicHandler.SendTextInformationMessage(Clients, TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 182,
                    FighterPlaying.GetMapRunningFighterName(), leaver.LeftRound + FightConfiguration.TurnsBeforeDisconnection - TimeLine.RoundNumber);
            }

            OnTurnPassed();

            StartTurn();
        }

        protected virtual void OnTurnPassed()
        {
            if (IsSequencing)
                EndAllSequences();
        }

        #endregion Turn Management

        #region Events Binders

        private void UnBindFightersEvents()
        {
            foreach (var fighter in Fighters)
            {
                UnBindFighterEvents(fighter);
            }
        }

        private void UnBindFighterEvents(FightActor actor)
        {
            actor.ReadyStateChanged -= OnSetReady;
            actor.PrePlacementChanged -= OnChangePreplacementPosition;
            actor.PrePlacementSwapped -= OnSwapPreplacementPosition;
            actor.FighterLeft -= OnPlayerLeft;

            actor.StartMoving -= OnStartMoving;
            actor.StopMoving -= OnStopMoving;
            actor.PositionChanged -= OnPositionChanged;
            actor.FightPointsVariation -= OnFightPointsVariation;
            actor.LifePointsChanged -= OnLifePointsChanged;
            actor.DamageReducted -= OnDamageReducted;
            actor.SpellCasting -= OnSpellCasting;
            actor.SpellCasted -= OnSpellCasted;
            actor.WeaponUsed -= OnCloseCombat;
            actor.BuffAdded -= OnBuffAdded;
            actor.BuffRemoved -= OnBuffRemoved;
            actor.Dead -= OnDead;

            var fighter = actor as CharacterFighter;

            if (fighter != null)
            {
                fighter.Character.LoggedOut -= OnPlayerLoggout;
                fighter.Character.StatsResfreshed -= OnStatsRefreshed;
            }
        }

        private void BindFightersEvents()
        {
            foreach (var fighter in Fighters)
            {
                BindFighterEvents(fighter);
            }
        }

        private void BindFighterEvents(FightActor actor)
        {
            if (State == FightState.Placement)
            {
                actor.FighterLeft += OnPlayerLeft;

                actor.ReadyStateChanged += OnSetReady;
                actor.PrePlacementChanged += OnChangePreplacementPosition;
                actor.PrePlacementSwapped += OnSwapPreplacementPosition;
            }

            if (State == FightState.Fighting)
            {
                actor.FighterLeft += OnPlayerLeft;

                actor.StartMoving += OnStartMoving;
                actor.StopMoving += OnStopMoving;
                actor.PositionChanged += OnPositionChanged;
                actor.FightPointsVariation += OnFightPointsVariation;
                actor.LifePointsChanged += OnLifePointsChanged;
                actor.DamageReducted += OnDamageReducted;

                actor.SpellCasting += OnSpellCasting;
                actor.SpellCasted += OnSpellCasted;
                actor.SpellCastFailed += OnSpellCastFailed;
                actor.WeaponUsed += OnCloseCombat;

                actor.BuffAdded += OnBuffAdded;
                actor.BuffRemoved += OnBuffRemoved;

                actor.Dead += OnDead;
            }

            var fighter = actor as CharacterFighter;

            if (fighter != null)
            {
                fighter.Character.LoggedOut += OnPlayerLoggout;

                if (State == FightState.Placement)
                {
                    fighter.Character.StatsResfreshed += OnStatsRefreshed;
                }
            }
        }

        #endregion Events Binders

        #region Turn Actions

        #region Death
        public bool PrematuredStartWaveOccured = false;

        public void DisablePrematuredStartWaveOccured()
        {
            PrematuredStartWaveOccured = false;
        }

        protected async virtual void OnDead(FightActor fighter, FightActor killedBy)
        {
            using (StartSequence(SequenceTypeEnum.SEQUENCE_CHARACTER_DEATH))
            {
                fighter.KillAllSummons();
                fighter.RemoveAndDispellAllBuffs(FightDispellableEnum.DISPELLABLE_BY_DEATH);
                fighter.RemoveAllCastedBuffs(FightDispellableEnum.DISPELLABLE_BY_DEATH);

                ActionsHandler.SendGameActionFightDeathMessage(Clients, fighter, killedBy);
            }

            foreach (var trigger in m_triggers.Where(trigger => trigger.Caster == fighter).ToArray())
            {
                RemoveTrigger(trigger);
            }

            if (DefendersTeam.AreAllDead() && !PrematuredStartWaveOccured)
            {
                PrematuredStartWaveOccured = true;
                Task t = new Task(() => DeadWaveSpawn());
                await Task.Delay(2000);
                t.Start();

                Task td = new Task(() => DisablePrematuredStartWaveOccured());
                await Task.Delay(5000);
                td.Start();
            }
        }

        #endregion Death

        #region Movement

        protected virtual void OnStartMoving(ContextActor actor, Path path)
        {
            var fighter = actor as FightActor;
            var character = actor is CharacterFighter ? (actor as CharacterFighter).Character : null;

            if (fighter != null && !fighter.IsFighterTurn())
                return;

            if (!(path is FightPath))
                return;

            var fightPath = (FightPath)path;

            using (StartMoveSequence(fightPath))
            {
                int index = 0;

                foreach (var tackle in fightPath.Tackles)
                {
                    if (tackle.PathIndex - index > 0)
                    {
                        ForEach(entry =>
                        {
                            if (entry.CanSee(fighter))
                                ContextHandler.SendGameMapMovementMessage(entry.Client, fightPath.Cells.Skip(index).Take(tackle.PathIndex - index + 1).Select(x => x.Id), fighter);

                        }, true);
                    }

                    OnTackled(fighter, tackle);
                    index = tackle.PathIndex;
                }

                if (path.Cells.Length - index > 0)
                {
                    ForEach(entry =>
                    {
                        if (entry.CanSee(fighter))
                            ContextHandler.SendGameMapMovementMessage(entry.Client, fightPath.Cells.Skip(index).Take(path.Cells.Length - index + 1).Select(x => x.Id), fighter);

                    }, true);
                }

                if (fightPath.BlockedByObstacle)
                {
                    character?.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 276); // "Impossible d'emprunter ce chemin : un obstacle bloque le passage !"
                }

                actor.StopMove();
            }
        }

        public event Action<FightActor, int, int> Tackled;
        public event Action<IFight, FightActor> SwitchCompanion;

        protected virtual void OnTackled(FightActor actor, FightTackle tackle)
        {
            if (actor.MP - tackle.TackledMP < 0)
            {
                logger.Error("Cannot apply tackle : mp tackled ({0}) > available mp ({1})", tackle.TackledMP, actor.MP);
                return;
            }

            var summonersToCastSpell = tackle.Tacklers.Where(x => x is SummonedMonster summonedMonster && summonedMonster.Monster.MonsterId == (int)MonsterIdEnum.MASQUE_GRIMAANT_5152).Cast<SummonedMonster>();//FIX MASQUE_GRIMAANT

            if (summonersToCastSpell != null && summonersToCastSpell.Any())
            {
                foreach (var summoner in summonersToCastSpell)
                {
                    summoner.CastAutoSpell(new Spell((int)SpellIdEnum.GRIMACE_9942, 2), summoner.Cell);
                }
            }

            ActionsHandler.SendGameActionFightTackledMessage(Clients, actor, tackle.Tacklers);
            actor.LostAP((short)tackle.TackledAP, actor);
            actor.LostMP((short)tackle.TackledMP, actor);

            actor.TriggerBuffs(actor, BuffTriggerType.OnTackled);

            foreach (var tackler in tackle.Tacklers)
            {
                tackler.TriggerBuffs(tackler, BuffTriggerType.OnTackle);
            }

            Tackled?.Invoke(actor, tackle.TackledAP, tackle.TackledMP);
        }

        protected virtual void OnStopMoving(ContextActor actor, Path path, bool canceled)
        {
            var fighter = actor as FightActor;

            if (fighter != null && !fighter.IsFighterTurn())
                return;

            if (canceled)
                return; // error, mouvement shouldn't be canceled in a fight.

            if (fighter == null)
                return;

            fighter.UseMP((short)path.MPCost);
            PortalsManager.RefreshClientsPortals();
        }

        protected virtual void OnPositionChanged(ContextActor actor, ObjectPosition objectPosition)
        {
            var fighter = actor as FightActor;

            if (fighter == null)
                return;

            TriggerMarks(fighter.Cell, fighter, TriggerType.MOVE);
            PortalsManager.RefreshClientsPortals();
        }

        #endregion Movement

        #region Health & Actions points

        protected virtual void OnLifePointsChanged(FightActor actor, int delta, int shieldDamages, int permanentDamages, FightActor from, EffectSchoolEnum school, Damage dam)
        {
            var loss = (short)-delta;

            if (delta == 0 && shieldDamages == 0 && permanentDamages == 0)
                return;

            var action = ActionsEnum.ACTION_CHARACTER_LIFE_POINTS_LOST;

            switch (school)
            {
                case EffectSchoolEnum.Air:
                    action = ActionsEnum.ACTION_CHARACTER_LIFE_POINTS_LOST_FROM_AIR;
                    break;

                case EffectSchoolEnum.Earth:
                    action = ActionsEnum.ACTION_CHARACTER_LIFE_POINTS_LOST_FROM_EARTH;
                    break;

                case EffectSchoolEnum.Fire:
                    action = ActionsEnum.ACTION_CHARACTER_LIFE_POINTS_LOST_FROM_FIRE;
                    break;

                case EffectSchoolEnum.Water:
                    action = ActionsEnum.ACTION_CHARACTER_LIFE_POINTS_LOST_FROM_WATER;
                    break;

                case EffectSchoolEnum.Pushback:
                    action = ActionsEnum.ACTION_CHARACTER_LIFE_POINTS_LOST_FROM_PUSH;
                    break;

                default:
                    action = ActionsEnum.ACTION_CHARACTER_LIFE_POINTS_LOST;
                    break;
            }

            var shieldBuffs = actor.GetBuffs(x => x is StatBuff && ((StatBuff)x).Caracteristic == PlayerFields.Shield).Select(x => x as StatBuff).ToArray();

            if (!shieldBuffs.Any() && shieldDamages == 0)
            {
                ActionsHandler.SendGameActionFightLifePointsLostMessage(Clients, action, from ?? actor, actor, loss, (short)permanentDamages, dam.ElementId);
            }
            else
            {
                ActionsHandler.SendGameActionFightLifeAndShieldPointsLostMessage(Clients, action, from ?? actor, actor, loss, (short)permanentDamages, dam.ElementId, (short)shieldDamages);

                foreach (var shieldBuff in shieldBuffs)
                {
                    if (shieldDamages <= 0)
                        continue;

                    var diff = Math.Max(0, shieldBuff.Value - shieldDamages);

                    shieldDamages -= (shieldBuff.Value - diff);
                    shieldBuff.Value = (short)diff;

                    if (shieldBuff.Value <= 0)
                        actor.RemoveBuff(shieldBuff);
                    else
                        ContextHandler.SendGameActionFightDispellableEffectMessage(Clients, shieldBuff, true);
                }
            }
        }

        protected virtual void OnFightPointsVariation(FightActor actor, ActionsEnum action, FightActor source, FightActor target, short delta)
        {
            if (delta == 0)
                return;

            ActionsHandler.SendGameActionFightPointsVariationMessage(Clients, action, source, target, delta);
        }

        protected virtual void OnDamageReducted(FightActor fighter, FightActor source, int reduction)
        {
            if (reduction == 0)
                return;

            ActionsHandler.SendGameActionFightReduceDamagesMessage(Clients, source, fighter, reduction);
        }

        #endregion Health & Actions points

        #region Spells

        protected virtual void OnCloseCombat(FightActor caster, WeaponTemplate weapon, Cell targetCell, FightSpellCastCriticalEnum critical, bool silentCast)
        {
            var target = GetOneFighter(targetCell);

            ForEach(entry => ActionsHandler.SendGameActionFightCloseCombatMessage(entry.Client, caster, target, targetCell, critical, !caster.IsVisibleFor(entry) || silentCast, weapon), true);
        }

        protected virtual void OnSpellCasting(FightActor caster, SpellCastHandler castHandler)
        {
            var target = GetOneFighter(castHandler.Informations.TargetedCell);

            if (castHandler.Spell.Id == 0)
            {
                // Envia mensagem de combate corpo a corpo
                ForEach(clientEntry =>
                {
                    var targetCell = castHandler.SeeCast(clientEntry) ? castHandler.Informations.TargetedCell : GetInvisibleSpellCastCell(caster.Cell, castHandler.Informations.TargetedCell);
                    var isSilent = !caster.IsVisibleFor(clientEntry) || castHandler.Informations.Silent;

                    ActionsHandler.SendGameActionFightCloseCombatMessage(clientEntry.Client, caster, target, targetCell, castHandler.Informations.Critical, isSilent, 0);
                }, true);
            }
            else
            {
                var portals = PortalsManager.PortalAnimation(castHandler);

                // Envia mensagem de lançamento de feitiço
                ForEach(clientEntry =>
                {
                    var targetCell = castHandler.SeeCast(clientEntry) ? castHandler.Informations.TargetedCell : GetInvisibleSpellCastCell(caster.Cell, castHandler.Informations.TargetedCell);
                    var isSilent = !caster.IsVisibleFor(clientEntry) || castHandler.Informations.Silent;

                    ContextHandler.SendGameActionFightSpellCastMessage(clientEntry.Client, ActionsEnum.ACTION_FIGHT_CAST_SPELL, caster, target, targetCell, castHandler.Informations.Critical, isSilent, castHandler.Spell, portals);
                }, true);
            }

            //ActionsHandler.SendGameActionUpdateSpellModifierMessage(client: Fight.Clients, modificationType: CharacterSpellModificationTypeEnum.AP_COST, Caster, (ushort)bombSpell.InstantSpellId);
        }


        private Cell GetInvisibleSpellCastCell(Cell casterCell, Cell targetedCell) => Cells[((MapPoint)casterCell).GetCellInDirection(((MapPoint)casterCell).OrientationTo(targetedCell), 1).CellId];

        protected virtual void OnSpellCasted(FightActor caster, SpellCastHandler castHandler)
        {
            CheckFightEnd();
        }

        protected virtual void OnSpellCastFailed(FightActor caster, SpellCastInformations cast)
        {
            ContextHandler.SendGameActionFightNoSpellCastMessage(Clients, cast.Spell);
        }

        #endregion Spells

        #region Buffs

        public IEnumerable<Buff> GetBuffs()
        {
            return m_buffs;
        }

        public void UpdateBuff(Buff buff, bool updateAction = true)
        {
            ContextHandler.SendGameActionFightDispellableEffectMessage(Clients, buff, updateAction);
        }

        protected virtual void OnBuffAdded(FightActor target, Buff buff)
        {
            m_buffs.Add(buff);
            ContextHandler.SendGameActionFightDispellableEffectMessage(Clients, buff);
        }

        protected virtual void OnBuffRemoved(FightActor target, Buff buff)
        {
            m_buffs.Remove(buff);

            // regular debuffing is done automatically
            ActionsHandler.SendGameActionFightDispellEffectMessage(Clients, target, target, buff);
        }

        #endregion Buffs

        #region Sequences

        private int m_nextSequenceId = 1;
        private readonly List<FightSequence> m_sequencesRoot = new List<FightSequence>();

        public FightSequence CurrentSequence
        {
            get;
            private set;
        }

        public FightSequence CurrentRootSequence
        {
            get;
            private set;
        }

        public bool IsSequencing => CurrentRootSequence != null && !CurrentRootSequence.Ended;

        public DateTime LastSequenceEndTime => m_sequencesRoot.Count > 0 ? m_sequencesRoot.Max(x => x.EndTime) : DateTime.Now;

        public FightSequence StartMoveSequence(FightPath path)
        {
            var id = m_nextSequenceId++;
            var sequence = new FightMoveSequence(id, TimeLine.Current, path);
            StartSequence(sequence);
            return sequence;
        }

        public FightSequence StartSequence(SequenceTypeEnum type)
        {
            var id = m_nextSequenceId++;
            var sequence = new FightSequence(id, type, TimeLine.Current);
            StartSequence(sequence);
            return sequence;
        }

        private void StartSequence(FightSequence sequence)
        {
            if (!IsSequencing)
            {
                if (sequence.Parent != null)
                {
                    logger.Error($"Sequence {sequence.Type} is a root and cannot have a parent");
                }

                m_sequencesRoot.Add(sequence);
                CurrentRootSequence = sequence;
            }
            else
                CurrentSequence.AddChildren(sequence);


            CurrentSequence = sequence;

            // just send the root sequence
            if (sequence.Parent == null)
                ActionsHandler.SendSequenceStartMessage(Clients, sequence);
        }

        public void OnSequenceEnded(FightSequence sequence)
        {
            if (CurrentSequence != sequence)
            {
                logger.Error($"Sequence incoherence {sequence} instead of {CurrentSequence}");
            }

            CurrentSequence = sequence.Parent;
        }


        public void EndAllSequences()
        {
            foreach (var sequence in m_sequencesRoot)
                sequence.EndSequence();
        }

        public void ResetSequences()
        {
            m_sequencesRoot.Clear();
            CurrentSequence = null;
            m_nextSequenceId = 1;
        }

        public virtual bool AcknowledgeAction(CharacterFighter fighter, int sequenceId)
        {
            return m_sequencesRoot.Any(x => x.Acknowledge(sequenceId, fighter));
        }

        #endregion Sequences

        #endregion Turn Actions

        #region Non Turn Actions

        protected virtual void OnPlayerLeft(FightActor fighter)
        {
            if (!(fighter is CharacterFighter) && !(fighter is CompanionActor))
            {
                logger.Error("Only characters or Companion can leave a fight");
                return;
            }

            var characterFighter = (fighter as CharacterFighter);
            if (characterFighter != null)
            {
                if (characterFighter.Character.IsLoggedIn)
                {
                    if (State == FightState.Placement)
                    {
                        characterFighter.ResetFightProperties();

                        if (CheckFightEnd())
                            return;

                        fighter.Team.RemoveFighter(fighter);
                        characterFighter.Character.RejoinMap();
                    }
                    else
                    {
                        fighter.Die();

                        // wait the character to be ready
                        var readyChecker = new ReadyChecker(this, new[] { characterFighter });
                        readyChecker.Success += obj => OnPlayerReadyToLeave(characterFighter);
                        readyChecker.Timeout += (obj, laggers) => OnPlayerReadyToLeave(characterFighter);

                        characterFighter.PersonalReadyChecker = readyChecker;
                        readyChecker.Start();
                    }
                    characterFighter.Character.Companion?.Die();
                }
                else
                {
                    var isfighterTurn = fighter.IsFighterTurn();
                    characterFighter.EnterDisconnectedState();

                    if (!CheckFightEnd() && isfighterTurn && characterFighter.MustSkipTurn())
                        StopTurn();

                    fighter.Team.AddLeaver(fighter);
                    m_leavers.Add(fighter);

                    // <b>%1</b> vient d'être déconnecté, il quittera la partie dans <b>%2</b> tour(s) s'il ne se reconnecte pas avant.
                    BasicHandler.SendTextInformationMessage(Clients, TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 182,
                        fighter.GetMapRunningFighterName(), FightConfiguration.TurnsBeforeDisconnection);
                }
            }
            else
            {
                fighter.Team.RemoveFighter(fighter);
            }
        }

        protected virtual void OnPlayerReadyToLeave(CharacterFighter fighter)
        {
            if (fighter.Fight != fighter.Character.Fight)
                return;

            fighter.PersonalReadyChecker = null;
            var isfighterTurn = fighter.IsFighterTurn();
            IFightResult leaverResult;
            var results = GenerateLeaverResults(fighter, out leaverResult);

            leaverResult.Apply();

            ContextHandler.SendGameFightLeaveMessage(Clients, fighter);
            ContextHandler.SendGameFightEndMessage(fighter.Character.Client, this,
                results.Select(x => x.GetFightResultListEntry()));

            var fightend = CheckFightEnd();

            if (!fightend && isfighterTurn)
                StopTurn();

            fighter.ResetFightProperties();

            fighter.Team.AddLeaver(fighter);
            m_leavers.Add(fighter);

            fighter.Team.RemoveFighter(fighter);
            fighter.Character.RejoinMap();

            fighter.Character.Companion?.Die();
        }

        protected virtual void OnPlayerLoggout(Character character)
        {
            character.LoggedOut -= OnPlayerLoggout;
            if (!character.IsFighting() || character.Fight != this)
                return;

            character.Fighter.LeaveFight();
        }

        public void RejoinFightFromDisconnection(CharacterFighter fighter)
        {
            fighter.Character.LoggedOut += OnPlayerLoggout;
            fighter.Team.RemoveLeaver(fighter);
            m_leavers.Remove(fighter);
            fighter.LeaveDisconnectedState();

            var client = fighter.Character.Client;

            Clients.Add(client);

            SendGameFightJoinMessage(fighter);

            foreach (var fightMember in GetAllFighters())
            {
                ContextHandler.SendGameFightShowFighterMessage(client, fightMember);
            }

            fighter.Character.RefreshStats();
            fighter.Character.Stats.Health.PermanentDamages = fighter.ErosionDamageBeforeLeft;

            if (State == FightState.Placement || State == FightState.NotStarted)
            {
                ContextHandler.SendGameFightPlacementPossiblePositionsMessage(client, this, (sbyte)fighter.Team.Id);
            }

            ContextHandler.SendGameEntitiesDispositionMessage(client, GetAllFighters());
            ContextHandler.SendGameFightResumeMessage(client, fighter);
            ContextHandler.SendGameFightStartMessage(client);
            ContextHandler.SendGameFightTurnListMessage(client, this);
            ContextHandler.SendGameFightSynchronizeMessage(client, this);

            if (fighter.IsSlaveTurn())
            {
                ContextHandler.SendSlaveSwitchContextMessage(client, fighter.GetSlave());
            }

            ContextHandler.SendGameFightTurnResumeMessage(client, FighterPlaying);
            ContextHandler.SendGameFightNewRoundMessage(client, TimeLine.RoundNumber);
            ContextHandler.SendGameFightUpdateTeamMessage(client, this, ChallengersTeam);
            ContextHandler.SendGameFightUpdateTeamMessage(client, this, DefendersTeam);

            ContextHandler.SendGameFightOptionStateUpdateMessage(client, fighter.Team, FightOptionsEnum.FIGHT_OPTION_ASK_FOR_HELP, fighter.Team.GetOptionState(FightOptionsEnum.FIGHT_OPTION_ASK_FOR_HELP));
            ContextHandler.SendGameFightOptionStateUpdateMessage(client, fighter.Team, FightOptionsEnum.FIGHT_OPTION_SET_CLOSED, fighter.Team.GetOptionState(FightOptionsEnum.FIGHT_OPTION_SET_CLOSED));
            ContextHandler.SendGameFightOptionStateUpdateMessage(client, fighter.Team, FightOptionsEnum.FIGHT_OPTION_SET_SECRET, fighter.Team.GetOptionState(FightOptionsEnum.FIGHT_OPTION_SET_SECRET));
            ContextHandler.SendGameFightOptionStateUpdateMessage(client, fighter.Team, FightOptionsEnum.FIGHT_OPTION_SET_TO_PARTY_ONLY, fighter.Team.GetOptionState(FightOptionsEnum.FIGHT_OPTION_SET_TO_PARTY_ONLY));

            using (StartSequence(SequenceTypeEnum.SEQUENCE_TURN_END))
            {
                PortalsManager.RefreshTriggers(client);
                PortalsManager.RefreshClientPortals(client);
            }

            // <b>%1</b> vient de se reconnecter en combat.
            BasicHandler.SendTextInformationMessage(Clients, TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 184, fighter.GetMapRunningFighterName());
        }

        #endregion Non Turn Actions

        #region Challenges

        public void AddChallenge(DefaultChallenge challenge)
        {
            challenge.Initialize();
            Challenges.Add(challenge);
        }

        public int GetChallengesBonus() => (int)Challenges.Sum(x => x.Status == ChallengeStatusEnum.COMPLETED_0 ? x.Bonus : 0);

        #endregion Challenges

        #region Triggers

        private readonly List<MarkTrigger> m_triggers = new List<MarkTrigger>();

        public IEnumerable<MarkTrigger> GetTriggers() => m_triggers;

        public bool ShouldTriggerOnMove(Cell cell, FightActor actor)
            => m_triggers.Any(entry => entry.TriggerType.HasFlag(TriggerType.MOVE) && entry.StopMovement && entry.ContainsCell(cell) && entry.CanTrigger(actor));

        public MarkTrigger[] GetTriggersByCell(Cell cell) => m_triggers.Where(entry => entry.ContainsCell(cell)).ToArray();

        public MarkTrigger[] GetTriggers(Cell cell) => m_triggers.Where(entry => entry.CenterCell.Id == cell.Id).ToArray();

        public void AddTriger(MarkTrigger trigger)
        {
            trigger.Triggered += OnMarkTriggered;
            m_triggers.Add(trigger);

            foreach (var character in GetCharactersAndSpectators())
            {
                ContextHandler.SendGameActionFightMarkCellsMessage(character.Client, trigger, character.Fighter != null && trigger.DoesSeeTrigger(character.Fighter));
            }

            if (!trigger.TriggerType.HasFlag(TriggerType.CREATION))
                return;

            var fighters = GetAllFighters(trigger.GetCells());
            foreach (var fighter in fighters)
                trigger.Trigger(fighter);
        }

        public void RemoveTrigger(MarkTrigger trigger)
        {
            trigger.Triggered -= OnMarkTriggered;
            m_triggers.Remove(trigger);
            trigger.NotifyRemoved();

            ContextHandler.SendGameActionFightUnmarkCellsMessage(Clients, trigger);
        }

        public void TriggerMarks(Cell cell, FightActor trigger, TriggerType triggerType)
        {
            var triggers = m_triggers.Where(markTrigger => markTrigger.TriggerType.HasFlag(triggerType) && markTrigger.ContainsCell(cell) && markTrigger.CanTrigger(trigger)).OrderByDescending(x => x.Priority).ToArray();

            foreach (var markTrigger in triggers.OfType<Trap>())
            {
                markTrigger.WillBeTriggered = true;
            }

            // we use a copy 'cause a trigger can be deleted when a fighter die with it
            foreach (var markTrigger in triggers)
            {
                if (!trigger.CanPlay() && (triggerType == TriggerType.OnTurnBegin || triggerType == TriggerType.OnTurnEnd) && (markTrigger is Wall || markTrigger is Glyph))
                    continue;

                using (StartSequence(SequenceTypeEnum.SEQUENCE_GLYPH_TRAP))
                {
                    markTrigger.Trigger(trigger, cell);
                }
            }
        }

        public void DecrementGlyphDuration(FightActor caster)
        {
            var triggersToRemove = m_triggers.Where(trigger => trigger.Caster == caster).Where(trigger => trigger.DecrementDuration()).ToList();

            if (triggersToRemove.Count == 0)
                return;

            using (StartSequence(SequenceTypeEnum.SEQUENCE_GLYPH_TRAP))
            {
                foreach (var trigger in triggersToRemove)
                {
                    RemoveTrigger(trigger);
                }
            }
        }

        public int PopNextTriggerId() => m_triggerIdProvider.Pop();

        public void FreeTriggerId(int id)
        {
            m_triggerIdProvider.Push(id);
        }

        private void OnMarkTriggered(MarkTrigger markTrigger, FightActor trigger, Spell triggerSpell)
        {
            ContextHandler.SendGameActionFightTriggerGlyphTrapMessage(Clients, markTrigger, trigger, triggerSpell);
        }

        #endregion Triggers

        #region Ready Checker

        protected virtual void OnLaggersSpotted(NamedFighter[] laggers)
        {
            if (laggers.Length == 1)
            {
                BasicHandler.SendTextInformationMessage(Clients, TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 28, laggers[0].Name);
            }
            else if (laggers.Length > 1)
            {
                BasicHandler.SendTextInformationMessage(Clients, TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 29, string.Join(",", laggers.Select(entry => entry.Name)));
            }
        }

        #endregion Ready Checker

        #region Freeze

        public bool Freezed
        {
            get { return m_freezed; }
            set
            {
                m_freezed = value;
                OnFreezed();
            }
        }

        private void OnFreezed()
        {
            if (State == FightState.Fighting)
            {
                if (Freezed)
                    m_turnTimer.Stop();
                else
                    m_turnTimer = Map.Area.CallDelayed(FightConfiguration.TurnTime, StopTurn);
            }
        }

        #endregion Freeze

        #region Send Methods

        protected virtual void SendGameFightJoinMessage(CharacterFighter fighter)
        {
            ContextHandler.SendGameFightJoinMessage(fighter.Character.Client, CanCancelFight(), true, IsStarted, IsStarted ? 0 : (int)GetPlacementTimeLeft().TotalMilliseconds / 100, FightType);
        }

        protected virtual void SendGameFightSpectatorJoinMessage(FightSpectator spectator)
        {
            ContextHandler.SendGameFightSpectatorJoinMessage(spectator.Character.Client, this);
        }

        #endregion Send Methods

        #region Get Methods

        private readonly WorldClientCollection m_clients = new WorldClientCollection();
        private readonly WorldClientCollection m_spectatorClients = new WorldClientCollection();
        private readonly List<FightActor> m_leavers;
        private readonly List<FightSpectator> m_spectators;
        private bool m_freezed;
        private Map fightMap;
        private FightTeam blueTeam;
        private FightTeam redTeam;

        /// <summary>
        /// Do not modify, just read
        /// </summary>
        public WorldClientCollection Clients
        {
            get { return m_clients; }
        }

        /// <summary>
        /// Do not modify, just read
        /// </summary>
        public WorldClientCollection SpectatorClients
        {
            get
            {
                return m_spectatorClients;
            }
        }

        public IEnumerable<Character> GetAllCharacters()
        {
            return GetAllCharacters(false);
        }

        public IEnumerable<Character> GetAllCharacters(bool withSpectators = false)
        {
            return withSpectators ? Fighters.OfType<CharacterFighter>().Select(entry => entry.Character).Concat(Spectators.Select(entry => entry.Character)) : Fighters.OfType<CharacterFighter>().Select(entry => entry.Character);
        }

        public void ForEach(Action<Character> action)
        {
            var characters = GetAllCharacters().ToList();

            foreach (var character in characters)
            {
                action(character);
            }
        }

        public void ForEach(Action<Character> action, bool withSpectators = false)
        {
            var characters = GetAllCharacters(withSpectators).ToList();

            foreach (var character in characters)
            {
                action(character);
            }
        }

        public void ForEach(Action<Character> action, Character except, bool withSpectators = false)
        {
            var characters = GetAllCharacters(withSpectators).Where(character => character != except).ToList();

            foreach (var character in characters)
            {
                action(character);
            }
        }

        protected abstract bool CanCancelFight();

        public bool IsCellFree(Cell cell)
        {
            return cell.Walkable && !cell.NonWalkableDuringFight && GetOneFighter(cell) == null;
        }

        public TimeSpan GetFightDuration()
        {
            return TimeSpan.FromMilliseconds(!IsStarted ? 0 : (int)(DateTime.Now - StartTime).TotalMilliseconds);
        }

        public TimeSpan GetTurnTimeLeft()
        {
            if (TimeLine.Current == null)
                return TimeSpan.Zero;

            var time = (DateTime.Now - TurnStartTime).TotalMilliseconds;

            return TimeSpan.FromMilliseconds(time > 0 ? (FighterPlaying.TurnTime - (int)time) : 0);
        }

        public sbyte GetNextContextualId()
        {
            return (sbyte)m_contextualIdProvider.Pop();
        }

        public void FreeContextualId(sbyte id)
        {
            m_contextualIdProvider.Push(id);
        }

        public T GetRandomFighter<T>() where T : FightActor
        {
            var fighters = Fighters.Where(x => x is T && x.IsAlive()).ToArray();

            if (!fighters.Any())
                return null;

            var random = new CryptoRandom().Next(0, fighters.Count());

            return fighters[random] as T;
        }

        public FightActor GetOneFighter(int id)
        {
            return Fighters.FirstOrDefault(entry => entry.Id == id);
        }

        public FightActor GetOneFighter(Cell cell)
        {
            if (Fighters is null)
            {
                logger.Error("Error GetOneFighter : Fighters is null.");
                return null;
            }

            FightActor actor = Fighters.FirstOrDefault(entry => entry.IsAlive() && entry.Cell.Id == cell.Id && entry != null);

            return actor;
        }

        public FightActor GetOneFighter(Predicate<FightActor> predicate)
        {
            var entries = Fighters.Where(entry => predicate(entry));

            var fightActors = entries as FightActor[] ?? entries.ToArray();

            return fightActors.Count() != 0 ? null : fightActors.FirstOrDefault();
        }

        public T GetOneFighter<T>(int id) where T : FightActor
        {
            return Fighters.OfType<T>().FirstOrDefault(entry => entry.Id == id);
        }

        public T GetOneFighter<T>(Cell cell) where T : FightActor
        {
            return Fighters.OfType<T>().FirstOrDefault(entry => entry.IsAlive() && Equals(entry.Position.Cell, cell));
        }

        public T GetOneFighter<T>(Predicate<T> predicate) where T : FightActor
        {
            return Fighters.OfType<T>().FirstOrDefault(entry => predicate(entry));
        }

        public T GetFirstFighter<T>(int id) where T : FightActor
        {
            return Fighters.OfType<T>().FirstOrDefault(entry => entry.Id == id);
        }

        public T GetFirstFighter<T>(Cell cell) where T : FightActor
        {
            return Fighters.OfType<T>().FirstOrDefault(entry => entry.IsAlive() && Equals(entry.Position.Cell, cell));
        }

        public T GetFirstFighter<T>(Predicate<T> predicate) where T : FightActor
        {
            return Fighters.OfType<T>().FirstOrDefault(entry => predicate(entry));
        }

        public List<FightActor> GetAllFightersInLine(MapPoint startCell, int range, DirectionsEnum direction)
        {
            var fighters = new List<FightActor>();
            var nextCell = startCell.GetNearestCellInDirection(direction);
            var i = 0;

            while (nextCell != null && Map.Cells[nextCell.CellId].Walkable && !Map.Cells[nextCell.CellId].NonWalkableDuringFight && i < range)
            {
                var fighter = GetOneFighter(Map.Cells[nextCell.CellId]);

                if (fighter == null && fighters.Any())
                    break;

                if (fighter != null)
                    fighters.Add(fighter);

                nextCell = nextCell.GetNearestCellInDirection(direction);
                i++;
            }

            return fighters;
        }

        public List<FightActor> GetAllFightersInLineToSacrier(MapPoint startCell, int range, DirectionsEnum direction)
        {
            var fighters = new List<FightActor>();
            var nextCell = startCell.GetNearestCellInDirection(direction);
            var i = 0;

            while (nextCell != null && Map.Cells[nextCell.CellId].Walkable && i < range)
            {
                var fighter = GetOneFighter(Map.Cells[nextCell.CellId]);

                if (fighter != null)
                    fighters.Add(fighter);

                nextCell = nextCell.GetNearestCellInDirection(direction);
                i++;
            }

            return fighters;
        }

        public ReadOnlyCollection<FightActor> GetAllFighters()
        {
            return Fighters;
        }

        public ReadOnlyCollection<FightActor> GetLeavers()
        {
            return Leavers;
        }

        public CharacterFighter GetLeaver(int characterId)
        {
            return Leavers.OfType<CharacterFighter>().FirstOrDefault(x => x.Id == characterId);
        }

        public ReadOnlyCollection<FightSpectator> GetSpectators()
        {
            return Spectators;
        }

        public IEnumerable<Character> GetCharactersAndSpectators()
        {
            return GetAllCharacters().Concat(GetSpectators().Select(entry => entry.Character));
        }

        public IEnumerable<FightActor> GetFightersAndLeavers()
        {
            return Fighters.Concat(Leavers.Where(x => !(x is CharacterFighter) || ((Winners != ((CharacterFighter)x).Team) && ((CharacterFighter)x).IsDisconnected) || !((CharacterFighter)x).IsDisconnected));
        }
        public IEnumerable<FightActor> GethDcFighters()
        {
            return Leavers.Where(x => ((CharacterFighter)x).IsDisconnected);
        }
        public IEnumerable<FightActor> GetLosersAndLeaversWithDc()
        {
            return Losers.Fighters.OfType<CharacterFighter>().Concat(Leavers.Where(x => (x is CharacterFighter)));
        }

        public IEnumerable<FightActor> GetAllFighters(Cell[] cells)
        {
            return GetAllFighters<FightActor>(entry => entry.IsAlive() && cells.Contains(entry.Position.Cell));
        }

        public IEnumerable<FightActor> GetAllFighters(IEnumerable<Cell> cells)
        {
            return GetAllFighters(cells.ToArray());
        }

        public IEnumerable<FightActor> GetAllFighters(Predicate<FightActor> predicate)
        {
            return Fighters.Where(entry => predicate(entry));
        }

        public IEnumerable<T> GetAllFighters<T>() where T : FightActor
        {
            return Fighters.OfType<T>();
        }

        public IEnumerable<T> GetAllFighters<T>(Predicate<T> predicate) where T : FightActor
        {
            return Fighters.OfType<T>().Where(entry => predicate(entry));
        }

        public IEnumerable<int> GetDeadFightersIds()
        {
            return GetFightersAndLeavers().Where(entry => entry.IsDead() && entry.IsVisibleInTimeline).Select(entry => entry.Id);
        }

        public IEnumerable<int> GetAliveFightersIds()
        {
            return GetAllFighters<FightActor>(entry => entry.IsAlive() && entry.IsVisibleInTimeline).Select(entry => entry.Id);
        }

        public FightCommonInformations GetFightCommonInformations()
        {
            return new FightCommonInformations((ushort)Id,
                                               (sbyte)FightType,
                                               m_teams.Select(entry => entry.GetFightTeamInformations()).ToArray(),
                                               m_teams.Select(entry => (ushort)entry.BladePosition.Cell.Id).ToArray(),
                                               m_teams.Select(entry => entry.GetFightOptionsInformations()).ToArray());
        }

        public FightExternalInformations GetFightExternalInformations(Character character)
        {
            return new FightExternalInformations((ushort)Id, (sbyte)FightType, !IsStarted ? 0 : StartTime.GetUnixTimeStamp(), SpectatorClosed,
                m_teams.Select(entry => entry.GetFightTeamLightInformations(character)).ToArray(), m_teams.Select(entry => entry.GetFightOptionsInformations()).ToArray());
        }

        public IEnumerable<NamedPartyTeam> GetPartiesName()
        {
            var redParty = ChallengersTeam.GetTeamParty();
            var blueParty = DefendersTeam.GetTeamParty();

            var parties = new[] { redParty, blueParty };
            return parties.Select((x, i) => Tuple.Create(i, x?.Name)).Where(x => x.Item2 != null).Select(x => new NamedPartyTeam((sbyte)x.Item1, x.Item2));
        }

        public IEnumerable<NamedPartyTeamWithOutcome> GetPartiesNameWithOutcome()
        {
            var redParty = ChallengersTeam.GetTeamParty();
            var blueParty = ChallengersTeam.GetTeamParty();

            var parties = new[] { redParty, blueParty };

            return parties.Select((x, i) => Tuple.Create(i, x?.Name)).Where(x => x.Item2 != null).Select(x => new NamedPartyTeamWithOutcome(new NamedPartyTeam((sbyte)x.Item1, x.Item2), (ushort)Teams[x.Item1].GetOutcome()));
        }

        #endregion Get Methods
    }
}