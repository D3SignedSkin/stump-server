using System.Collections.Generic;
using System.Linq;
using Stump.Core.Attributes;
using Stump.Core.Extensions;
using Stump.Core.Threading;
using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Database;
using Stump.Server.BaseServer.Initialization;
using Stump.Server.WorldServer.Database.Arena;
using Stump.Server.WorldServer.Database.Items.Templates;
using Stump.Server.WorldServer.Discord;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Items;
using Stump.Server.WorldServer.Game.Parties;
using Stump.Server.WorldServer.Handlers.Basic;
using Stump.Server.WorldServer.Handlers.Context;

namespace Stump.Server.WorldServer.Game.Arena
{
    public class ArenaManager : DataManager<ArenaManager>
    {
        [Variable] public static int MaxPlayersPerFights = 3;

        [Variable] public static int ArenaMinLevel = 50;

        [Variable] public static int ArenaMaxLevelDifference = 40;
        /// <summary>
        /// in ms
        /// </summary>
        [Variable] public static int ArenaUpdateInterval = 100;

        /// <summary>
        /// is seconds
        /// </summary>
        [Variable] public static int ArenaMatchmakingInterval = 30;

        /// <summary>
        /// in minutes
        /// </summary>
        [Variable] public static int ArenaPenalityTime = 30;

        /// <summary>
        /// in minutes
        /// </summary>
        [Variable]
        public static int ArenaWaitTime = 10;

        /// <summary>
        /// Kolizeum MapId that show all Arena Fights
        /// </summary>
        [Variable]
        public static long KolizeumMapId = 81788928;

        public static long AstrubMapId = 191105026;

        public ItemTemplate TokenItemTemplate => m_tokenTemplate ?? (m_tokenTemplate = ItemManager.Instance.TryGetTemplate((int)ItemIdEnum.KOLIZETON_12736));

        Dictionary<int, ArenaRecord> m_arenas_3vs3;
        Dictionary<int, ArenaRecord> m_arenas_1vs1;

        readonly SelfRunningTaskPool m_arenaTaskPool = new SelfRunningTaskPool(ArenaUpdateInterval, "Arena");
        readonly List<ArenaQueueMember> m_queue = new List<ArenaQueueMember>();
        ItemTemplate m_tokenTemplate;

        [Initialization(InitializationPass.Fifth)]
        public override void Initialize()
        {
            m_arenas_3vs3 = Database.Query<ArenaRecord>(ArenaRelator_3vs3.FetchQuery).ToDictionary(x => x.Id);
            m_arenas_1vs1 = Database.Query<ArenaRecord>(ArenaRelator_1vs1.FetchQuery).ToDictionary(x => x.Id);
            m_arenaTaskPool.CallPeriodically(ArenaMatchmakingInterval * 1000, ComputeMatchmaking);
            m_arenaTaskPool.Start();
        }

        public SelfRunningTaskPool ArenaTaskPool => m_arenaTaskPool;

        public Dictionary<int, ArenaRecord> Arenas_3vs3 => m_arenas_3vs3;

        public Dictionary<int, ArenaRecord> Arenas_1vs1 => m_arenas_1vs1;

        public bool CanJoinQueue(Character character)
        {
            if (m_arenas_3vs3.Count == 0 || m_arenas_1vs1.Count == 0)
                return false;

            //Already in queue
            if (IsInQueue(character))
                return false;

            return character.CanEnterArena();
        }

        public List<ArenaRecord> GetArenasRecord() => m_arenas_1vs1.Values.Concat(m_arenas_3vs3.Values).ToList();

        public bool IsInQueue(Character character) => m_queue.Exists(x => x.Character == character);

        public bool IsInQueue(ArenaParty party) => m_queue.Exists(x => x.Party == party);

        public ArenaQueueMember GetQueueMember(Character character) => m_queue.FirstOrDefault(x => x.Character == character);

        public int GetQueueMemberCount(Character character, int mode) => m_queue.Where(x => x.Character.Client.IP == character.Client.IP && x.Character.ArenaMode == mode && x.Character.Id != character.Id).Count();

        public int GetQueueMemberCountWebHook(int mode) => m_queue.Where(x => x.Character.ArenaMode == mode).Count();

        public void AddToQueue(Character character, int mode) //ArenaMode 1 e 2
        {
            if (!CanJoinQueue(character))
                return;

            if (mode == 2 && GetQueueMemberCount(character, mode) > 0)
            {
                character.SendServerMessageLang
                    (
                    "Você só poderá se registrar com um personagem no koliseu 3vs3 Solo.",
                    "You will only be able to register with one character in the Kolossium 3vs3 Solo.",
                    "Solo podrás registrarte con un personaje en el Kolossium 3vs3 Solo.",
                    "Vous ne pourrez vous inscrire qu'avec un seul personnage dans le Kolizéum 3vs3 Solo."
                    );
                return;
            }

            lock (m_queue)
                m_queue.Add(new ArenaQueueMember(character, mode));

            if (mode == 1)
            {
                ContextHandler.SendGameRolePlayArenaRegistrationStatusMessage(character.Client, true, PvpArenaStepEnum.ARENA_STEP_REGISTRED, PvpArenaTypeEnum.ARENA_TYPE_1VS1);

                if (character.Account.UserGroupId <= Settings.AnnouncePlayerOnlineGroup && DiscordIntegration.EnableDiscordWebHook)
                {
                    string text = $":flag_br: Um novo jogador acabou de se inscrever no Koliseu 1vs1, elevando o total para **{GetQueueMemberCountWebHook(1)}** participantes.\n" +
                                  $":flag_es: Un nuevo jugador acaba de inscribirse en el Kolissium 1vs1, lo que eleva el total a **{GetQueueMemberCountWebHook(1)}** participantes.\n" +
                                  $":flag_us: A new player has just signed up for the 1vs1 Kolossium, bringing the total to **{GetQueueMemberCountWebHook(1)}** participants.\n" +
                                  $":flag_fr: Un nouveau joueur vient de s'inscrire au Kolizéum 1vs1, portant le total à **{GetQueueMemberCountWebHook(1)}** participants.\n";

                    PlainText.SendWebHook(DiscordIntegration.DiscordChatKoliseu, text, DiscordIntegration.DiscordWHUsername);
                }
            }
            else if (mode == 2)
            {
                ContextHandler.SendGameRolePlayArenaRegistrationStatusMessage(character.Client, true, PvpArenaStepEnum.ARENA_STEP_REGISTRED, PvpArenaTypeEnum.ARENA_TYPE_3VS3_SOLO);

                if (character.Account.UserGroupId <= Settings.AnnouncePlayerOnlineGroup && DiscordIntegration.EnableDiscordWebHook)
                {
                    string text = $":flag_br: Um novo jogador acabou de se inscrever no Koliseu 3vs3 Solo, elevando o total para **{GetQueueMemberCountWebHook(2)}** participantes.\n" +
                                  $":flag_es: Un nuevo jugador acaba de inscribirse en el Kolissium 3vs3 Solo, lo que eleva el total a **{GetQueueMemberCountWebHook(2)}** participantes.\n" +
                                  $":flag_us: A new player has just signed up for the Solo 3vs3 Kolossium, bringing the total to **{GetQueueMemberCountWebHook(2)}** participants.\n" +
                                  $":flag_fr: Un nouveau joueur vient de s'inscrire au Kolizéum 3vs3 Solo, portant le total à **{GetQueueMemberCountWebHook(2)}** participants.\n";

                    PlainText.SendWebHook(DiscordIntegration.DiscordChatKoliseu, text, DiscordIntegration.DiscordWHUsername);
                }
            }
        }

        public void AddToQueue(ArenaParty party) //ArenaMode 3
        {
            if (!party.Members.All(CanJoinQueue))
                return;

            lock (m_queue)
                m_queue.Add(new ArenaQueueMember(party));

            ContextHandler.SendGameRolePlayArenaRegistrationStatusMessage(party.Clients, true, PvpArenaStepEnum.ARENA_STEP_REGISTRED, PvpArenaTypeEnum.ARENA_TYPE_3VS3_TEAM);

            if (DiscordIntegration.EnableDiscordWebHook)
            {
                string text = $":flag_br: Um novo time acabou de se inscrever no Koliseu 3vs3.\n" +
                  $":flag_es: Un nuevo equipo acaba de inscribirse en el Kolissium 3vs3.\n" +
                  $":flag_us: A new player has just signed up for the Solo 3vs3 Kolossium, bringing the total to **{GetQueueMemberCountWebHook(2)}** participants.\n" +
                  $":flag_fr: Un nouveau joueur vient de s'inscrire au Kolizéum 3vs3 Solo, portant le total à **{GetQueueMemberCountWebHook(2)}** participants.\n";

                PlainText.SendWebHook(DiscordIntegration.DiscordChatKoliseu, text, DiscordIntegration.DiscordWHUsername);
            }

            foreach (var character in party.Members.Where(x => x != party.Leader))
            {
                BasicHandler.SendTextInformationMessage(character.Client, TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 272, party.Leader.Name);//%1 vous a inscrit à un combat en Kolizéum.
            }
        }

        public void RemoveFromQueue(Character character)
        {
            lock (m_queue)
                m_queue.RemoveAll(x => x.Character == character);

            if (character.ArenaMode == 1)
            {
                ContextHandler.SendGameRolePlayArenaRegistrationStatusMessage(character.Client, false, PvpArenaStepEnum.ARENA_STEP_UNREGISTER, PvpArenaTypeEnum.ARENA_TYPE_1VS1);
            }
            else if (character.ArenaMode == 2)
            {
                ContextHandler.SendGameRolePlayArenaRegistrationStatusMessage(character.Client, false, PvpArenaStepEnum.ARENA_STEP_UNREGISTER, PvpArenaTypeEnum.ARENA_TYPE_3VS3_SOLO);
            }
        }

        public void RemoveFromQueue(ArenaParty party)
        {
            lock (m_queue)
                m_queue.RemoveAll(x => x.Party == party);

            ContextHandler.SendGameRolePlayArenaRegistrationStatusMessage(party.Clients, false, PvpArenaStepEnum.ARENA_STEP_UNREGISTER, PvpArenaTypeEnum.ARENA_TYPE_3VS3_TEAM);
        }

        public int Fila(int modo)
        {
            int queue = 0;

            if (modo == 1)
            {
                lock (m_queue)
                {
                    queue = m_queue.Where(x => x != null && !x.IsBusy() && (x.Character.ArenaMode == 1)).Count();
                }

                return queue;
            }
            else if (modo == 2)
            {
                lock (m_queue)
                {
                    queue = m_queue.Where(x => x != null && !x.IsBusy() && (x.Character.ArenaMode == 2)).Count();
                }

                return queue;
            }

            return queue;
        }

        public void ComputeMatchmaking()
        {
            List<ArenaQueueMember> queue;

            lock (m_queue)
            {
                queue = m_queue.Where(x => !x.IsBusy()).ToList();
            }

            ArenaQueueMember current;

            while ((current = queue.FirstOrDefault()) != null)
            {
                queue.Remove(current);

                List<ArenaQueueMember> matchs = null;

                if (current.Character != null)
                {
                    bool Is1vs1Alone = (current.Character?.ArenaMode == 1) ? true : false;
                    bool Is3vs3Alone = (current.Character?.ArenaMode == 2) ? true : false;

                    matchs = queue.Where(x => x.IsCompatibleWith(current, Is1vs1Alone, Is3vs3Alone)).ToList();
                }
                else
                {
                    matchs = queue.Where(x => x.IsCompatibleWith(current, false, false)).ToList();
                }

                var allies = new List<ArenaQueueMember> { current };
                var enemies = new List<ArenaQueueMember>();
                int missingAllies = 0;

                if (current.Character?.ArenaMode == 1)
                    missingAllies = 0;
                else
                    missingAllies = MaxPlayersPerFights - current.MembersCount;

                var i = 0;

                while (missingAllies > 0 && i < matchs.Count)
                {
                    if (matchs[i].MembersCount <= missingAllies)
                    {
                        allies.Add(matchs[i]);
                        missingAllies -= matchs[i].MembersCount;
                        matchs.Remove(matchs[i]);
                    }
                    else
                        i++;
                }

                if (missingAllies > 0)
                    continue;

                int missingEnemies = 0;

                if (current?.Character?.ArenaMode == 1)
                    missingEnemies = 1;
                else
                    missingEnemies = MaxPlayersPerFights;

                i = 0;

                while (missingEnemies > 0 && i < matchs.Count)
                {
                    if (matchs[i].MembersCount <= missingEnemies)
                    {
                        enemies.Add(matchs[i]);
                        missingEnemies -= matchs[i].MembersCount;
                        matchs.Remove(matchs[i]);
                    }
                    else
                        i++;
                }

                if (missingEnemies > 0)
                    continue;

                // start fight
                StartFight(allies, enemies);

                queue.RemoveAll(x => allies.Contains(x) || enemies.Contains(x));

                lock (m_queue)
                    m_queue.RemoveAll(x => allies.Contains(x) || enemies.Contains(x));
            }
        }

        void StartFight(IEnumerable<ArenaQueueMember> team1, IEnumerable<ArenaQueueMember> team2)
        {
            ArenaRecord arena = null;

            var m_characterscount1 = team1.SelectMany(x => x.EnumerateCharacters()).ToList().Count;
            var m_characterscount2 = team2.SelectMany(x => x.EnumerateCharacters()).ToList().Count;

            if (m_characterscount1 + m_characterscount2 <= 2)
            {
                Character first = team1.SelectMany(x => x.EnumerateCharacters()).ToArray()[0];
                Character second = team2.SelectMany(x => x.EnumerateCharacters()).ToArray()[0];

                arena = m_arenas_1vs1.RandomElementOrDefault().Value;
                first.CharacterToSeekName = second.Account.Nickname;
                second.CharacterToSeekName = first.Account.Nickname;
            }
            else
                arena = m_arenas_3vs3.RandomElementOrDefault().Value;

            var preFight = FightManager.Instance.CreateArenaPreFight(arena);

            foreach (var character in team1.SelectMany(x => x.EnumerateCharacters()))
            {
                character.DenyAllInvitations(PartyTypeEnum.PARTY_TYPE_ARENA);
                preFight.DefendersTeam.AddCharacter(character);
                character.LeaveDialog();
            }

            foreach (var character in team2.SelectMany(x => x.EnumerateCharacters()))
            {
                character.DenyAllInvitations(PartyTypeEnum.PARTY_TYPE_ARENA);
                preFight.ChallengersTeam.AddCharacter(character);
                character.LeaveDialog();
            }

            if (m_characterscount1 + m_characterscount2 <= 2)
            {
                preFight.ShowPopups();
                return;
            }

            var challengersParty = preFight.ChallengersTeam.Members.Select(x => x.Character.GetParty(PartyTypeEnum.PARTY_TYPE_ARENA)).FirstOrDefault() ??
                PartyManager.Instance.Create(PartyTypeEnum.PARTY_TYPE_ARENA);

            var defendersParty = preFight.DefendersTeam.Members.Select(x => x.Character.GetParty(PartyTypeEnum.PARTY_TYPE_ARENA)).FirstOrDefault() ??
                PartyManager.Instance.Create(PartyTypeEnum.PARTY_TYPE_ARENA);

            challengersParty.RemoveAllGuest();
            defendersParty.RemoveAllGuest();

            foreach (var character in preFight.ChallengersTeam.Members.Select(x => x.Character).Where(character => !challengersParty.IsInGroup(character)))
            {
                if (challengersParty.Leader != null)
                    challengersParty.Leader.Invite(character, PartyTypeEnum.PARTY_TYPE_ARENA, true);
                else
                    character.EnterParty(challengersParty);
            }

            foreach (var character in preFight.DefendersTeam.Members.Select(x => x.Character).Where(character => !defendersParty.IsInGroup(character)))
            {
                if (defendersParty.Leader != null)
                    defendersParty.Leader.Invite(character, PartyTypeEnum.PARTY_TYPE_ARENA, true);
                else
                    character.EnterParty(defendersParty);
            }

            preFight.ShowPopups();
        }
    }
}