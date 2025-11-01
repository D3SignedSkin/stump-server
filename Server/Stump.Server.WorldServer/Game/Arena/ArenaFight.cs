using Database.Kolilog;
using MongoDB.Bson;
using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Logging;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Fights.Results;
using Stump.Server.WorldServer.Game.KoliLog;
using Stump.Server.WorldServer.Game.Maps;
using Stump.Server.WorldServer.Handlers.Context;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Stump.Server.WorldServer.Game.Arena
{
    public class ArenaFight : Fight<ArenaTeam, ArenaTeam>
    {
        public ArenaFight(int id, Map fightMap, ArenaTeam defendersTeam, ArenaTeam challengersTeam, int mode) : base(id, fightMap, defendersTeam, challengersTeam)
        {
            ArenaMode = mode;
        }

        public override FightTypeEnum FightType => FightTypeEnum.FIGHT_TYPE_PVP_ARENA;

        public override bool IsPvP => false;

        public override bool IsMultiAccountRestricted => true;

        public override bool IsDeathTemporarily => true;

        public override bool CanKickPlayer => false;

        public int ArenaMode
        {
            get;
            private set;
        }

        public override void StartPlacement()
        {
            if (ArenaMode == 1)
                ContextHandler.SendGameRolePlayArenaRegistrationStatusMessage(Clients, false, PvpArenaStepEnum.ARENA_STEP_STARTING_FIGHT, PvpArenaTypeEnum.ARENA_TYPE_1VS1);
            else
                ContextHandler.SendGameRolePlayArenaRegistrationStatusMessage(Clients, false, PvpArenaStepEnum.ARENA_STEP_STARTING_FIGHT, PvpArenaTypeEnum.ARENA_TYPE_3VS3_TEAM);

            base.StartPlacement();

            m_placementTimer = Map.Area.CallDelayed(FightConfiguration.PlacementPhaseTime, StartFighting);
        }

        public override void StartFighting()
        {
            m_placementTimer.Dispose();

            base.StartFighting();
        }

        protected override void OnFightEnded()
        {
            if (ArenaMode == 1)
                ContextHandler.SendGameRolePlayArenaRegistrationStatusMessage(Clients, false, PvpArenaStepEnum.ARENA_STEP_UNREGISTER, PvpArenaTypeEnum.ARENA_TYPE_1VS1);
            else
                ContextHandler.SendGameRolePlayArenaRegistrationStatusMessage(Clients, false, PvpArenaStepEnum.ARENA_STEP_UNREGISTER, PvpArenaTypeEnum.ARENA_TYPE_3VS3_TEAM);

            base.OnFightEnded();
        }

        public override TimeSpan GetPlacementTimeLeft()
        {
            var timeleft = FightConfiguration.PlacementPhaseTime - (DateTime.Now - CreationTime).TotalMilliseconds;

            if (timeleft < 0)
                timeleft = 0;

            return TimeSpan.FromMilliseconds(timeleft);
        }

        protected override List<IFightResult> GetResults()
        {
            var challengersRank = (int)ChallengersTeam.GetAllFightersWithLeavers().OfType<CharacterFighter>().Average(x => x.Character.ArenaMode == 1 ? x.Character.ArenaPointsRank_1vs1 : x.Character.ArenaPointsRank_3vs3_Solo);
            var defendersRank = (int)DefendersTeam.GetAllFightersWithLeavers().OfType<CharacterFighter>().Average(x => x.Character.ArenaMode == 1 ? x.Character.ArenaPointsRank_1vs1 : x.Character.ArenaPointsRank_3vs3_Solo);

            var results = GetFightersAndLeavers()
                .OfType<CharacterFighter>()
                .Select(fighter =>
                {
                    var outcome = fighter.GetFighterOutcome();
                    var arenaModePoints = ArenaMode == 1 ? fighter.Character.ArenaPointsRank_1vs1 : fighter.Character.ArenaPointsRank_3vs3_Solo;
                    var opponentRank = fighter.Team == ChallengersTeam ? defendersRank : challengersRank;
                    var adjustedRank = ArenaRankFormulas.AdjustRank(arenaModePoints, opponentRank, outcome == FightOutcomeEnum.RESULT_VICTORY);

                    return new ArenaFightResult(fighter, outcome, fighter.Loot, adjustedRank) as IFightResult;
                })
                .ToList();

            foreach (var playerResult in results.OfType<ArenaFightResult>())
            {
                if (Winners.Id == playerResult.Fighter.Team.Id)
                {
                    var lastfights_hdr = KoliLog_manager.Instance.GetHardwareRecord(playerResult.Fighter.Character.Account.LastHardwareId);
                    var lastfights_ip = KoliLog_manager.Instance.GetIpRecords(playerResult.Fighter.Character.Client.IP);
                    var lastfights = lastfights_hdr.Concat(lastfights_ip.Where(x => !lastfights_hdr.Contains(x))).ToList();

                    foreach (var opponent in playerResult.Fighter.OpposedTeam.GetAllFightersWithLeavers().OfType<CharacterFighter>())
                    {
                        if (!lastfights.Any(x => x.Ip_opponent == opponent.Character.Client.IP && x.Koli_Type == playerResult.Fighter.Character.ArenaMode) || !lastfights.Any(x => x.Hardware_opponent == opponent.Character.Account.LastHardwareId && x.Koli_Type == playerResult.Fighter.Character.ArenaMode))
                        {
                            KoliLog_manager.Instance.AddRecord(new koli_logRecord
                            {
                                Time = DateTime.Now,
                                //Fighter_Id = UniqueId.ToString(),
                                Koli_Type = playerResult.Fighter.Character.ArenaMode,
                                IsNew = true,
                                Ip_own = playerResult.Fighter.Character.Client.IP,
                                Hardware_own = playerResult.Fighter.Character.Account.LastHardwareId,
                                Ip_opponent = opponent.Character.Client.IP,
                                Hardware_opponent = opponent.Character.Account.LastHardwareId,
                            });
                        }
                    }
                }

                #region MongoLog
                var document = new BsonDocument
                    {
                        { "FightId", UniqueId.ToString() },
                        { "Duration", GetFightDuration().TotalSeconds },
                        { "Team", Enum.GetName(typeof(TeamEnum), playerResult.Fighter.Team.Id) },
                        { "isWinner", Winners.Id == playerResult.Fighter.Team.Id},
                        { "AccountId", playerResult.Fighter.Character.Account.Id },
                        { "AccountName", playerResult.Fighter.Character.Account.Login },
                        { "CharacterId", playerResult.Fighter.Character.Id },
                        { "CharacterName", playerResult.Fighter.Character.Name},
                        { "IPAddress", playerResult.Fighter.Character.Client.IP },
                        { "ClientKey", playerResult.Fighter.Character.Account.LastHardwareId },
                        { "Date", DateTime.Now.ToString(CultureInfo.CreateSpecificCulture("pt-BR")) }
                    };

                MongoLogger.Instance.Insert("Fight_Koli", document);
                #endregion
            }

            return results;
        }

        protected override IEnumerable<IFightResult> GenerateLeaverResults(CharacterFighter leaver, out IFightResult leaverResult)
        {
            var rankLoose = CalculateRankLoose(leaver);

            leaverResult = null;

            var list = new List<IFightResult>();
            foreach (var fighter in GetFightersAndLeavers().OfType<CharacterFighter>())
            {
                var outcome = fighter.Team == leaver.Team ?
                    FightOutcomeEnum.RESULT_LOST :
                    FightOutcomeEnum.RESULT_VICTORY;

                var result = new ArenaFightResult(fighter, outcome, new FightLoot(), fighter == leaver ? rankLoose : 0, false);

                if (fighter == leaver)
                    leaverResult = result;

                list.Add(result);
            }

            return list;
        }

        protected override void OnPlayerLeft(FightActor fighter)
        {
            base.OnPlayerLeft(fighter);

            var characterFighter = fighter as CharacterFighter;
            if (characterFighter == null)
                return;

            if (characterFighter.IsDisconnected)
                return;

            characterFighter.Character.ToggleArenaPenality();

            if (characterFighter.Character.ArenaParty != null)
                characterFighter.Character.LeaveParty(characterFighter.Character.ArenaParty);

            var rankLoose = CalculateRankLoose(characterFighter);
            characterFighter.Character.UpdateArenaProperties(rankLoose, false, ArenaMode);
        }

        protected override void OnPlayerReadyToLeave(CharacterFighter characterFighter)
        {
            base.OnPlayerReadyToLeave(characterFighter);

            if (characterFighter.Character.ArenaParty != null)
                characterFighter.Character.LeaveParty(characterFighter.Character.ArenaParty);
        }

        protected override bool CanCancelFight() => false;

        protected static int CalculateRankLoose(CharacterFighter character)
        {
            var opposedTeamRank = (int)character.OpposedTeam.GetAllFightersWithLeavers().OfType<CharacterFighter>().Average(x => x.Character.ArenaMode == 1 ? x.Character.ArenaPointsRank_1vs1 : x.Character.ArenaPointsRank_3vs3_Solo);

            return ArenaRankFormulas.AdjustRank(character.Character.ArenaMode == 1 ? character.Character.ArenaPointsRank_1vs1 : character.Character.ArenaPointsRank_3vs3_Solo, opposedTeamRank, false);
        }

        protected override void OnDisposed()
        {
            if (m_placementTimer != null)
                m_placementTimer.Dispose();

            base.OnDisposed();
        }
    }
}