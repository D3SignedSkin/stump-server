using Stump.DofusProtocol.Messages;
using Stump.DofusProtocol.Types;
using Stump.Server.BaseServer.Network;
using Stump.Server.WorldServer.Core.Network;
using Stump.Server.WorldServer.Game.Actors;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Monsters;
using Stump.Server.WorldServer.Game.Arena.Leagues;
using Stump.Server.WorldServer.Game.Fights;
using System.Collections.Generic;
using System.Linq;

namespace Stump.Server.WorldServer.Handlers.Context.RolePlay
{
    public partial class ContextRoleplayHandler
    {
        [WorldHandler(GameRolePlayAttackMonsterRequestMessage.Id)]
        public static void HandleGameRolePlayAttackMonsterRequestMessage(WorldClient client, GameRolePlayAttackMonsterRequestMessage message)
        {
            var map = client.Character?.Map;
            var monster = map?.GetActor<MonsterGroup>(entry => entry.Id == message.monsterGroupId);

            if (monster != null)
            {
                if (monster.Position?.Cell == client.Character?.Position?.Cell && monster.Map?.Id != 26740736)
                {
                    // Restrição PVM Hydra By: Kenshin
                    if (Settings.ModoPVM)
                    {
                        monster.FightWith(client.Character);
                    }
                    else
                    {
                        SendNotAllowedToEnterFightMessage(client);
                    }
                }
                else if (monster.Map?.Id == 26740736 && monster.Cell?.Id == 344) //Iniciar a Luta contra o Kralove by:Kenshin
                {
                    monster.FightWith(client.Character);
                }
            }
        }

        private static void SendNotAllowedToEnterFightMessage(WorldClient client)
        {
            // Mensagem de servidor quando não permitido entrar na luta
            switch (client.Character?.Account.Lang)
            {
                case "fr":
                    client.Character.SendServerMessage("Vous n'êtes pas autorisé à entrer dans un combat. Ce serveur n'a pas le système PVM publié.", System.Drawing.Color.Red);
                    break;
                case "es":
                    client.Character.SendServerMessage("No se le permite participar en una pelea. Este servidor no tiene el sistema PVM liberado.", System.Drawing.Color.Red);
                    break;
                case "en":
                    client.Character.SendServerMessage("You are not allowed to enter a fight. This server does not have the PVM system released.", System.Drawing.Color.Red);
                    break;
                default:
                    client.Character.SendServerMessage("Você não tem permissão para entrar em uma luta. Esse servidor não possui o sistema de PVM liberado.", System.Drawing.Color.Red);
                    break;
            }
        }

        [WorldHandler(GameFightPlacementSwapPositionsRequestMessage.Id)]
        public static void HandleGameFightPlacementSwapPositionsRequestMessage(WorldClient client, GameFightPlacementSwapPositionsRequestMessage message)
        {
            if (!client.Character.IsFighting())
                return;

            if (client.Character.Fighter.Position.Cell.Id != message.cellId)
            {
                var cell = client.Character.Fight.Map.Cells[message.cellId];
                var target = client.Character.Fighter.Team.GetOneFighter(cell);

                if (target == null)
                    return;

                if (target is CharacterFighter)
                {
                    if (client.Character.Fighter.IsTeamLeader())
                    {
                        client.Character.Fighter.SwapPrePlacement(target);
                    }
                    else
                    {
                        var swapRequest = new SwapRequest(client.Character, (target as CharacterFighter).Character);
                        swapRequest.Open();
                    }
                }
                else if (target is CompanionActor)
                {
                    client.Character.Fighter.SwapPrePlacement(target);
                }
            }
        }

        [WorldHandler(GameFightPlacementSwapPositionsAcceptMessage.Id)]
        public static void HandleGameFightPlacementSwapPositionsAcceptMessage(WorldClient client, GameFightPlacementSwapPositionsAcceptMessage message)
        {
            if (!client.Character.IsInRequest() || !(client.Character.RequestBox is SwapRequest))
                return;

            if (message.requestId == client.Character.RequestBox.Source.Id)
                client.Character.RequestBox.Accept();
        }

        [WorldHandler(GameFightPlacementSwapPositionsCancelMessage.Id)]
        public static void HandleGameFightPlacementSwapPositionsCancelMessage(WorldClient client, GameFightPlacementSwapPositionsCancelMessage message)
        {
            if (!client.Character.IsInRequest() || !(client.Character.RequestBox is SwapRequest))
                return;

            if (message.requestId == client.Character.RequestBox.Source.Id)
            {
                if (client.Character == client.Character.RequestBox.Source)
                    client.Character.RequestBox.Cancel();
                else
                    client.Character.RequestBox.Deny();
            }
        }

        public static void SendGameRolePlayPlayerFightFriendlyAnsweredMessage(IPacketReceiver client, Character replier,
                                                                              Character source, Character target,
                                                                              bool accepted)
        {
            client.Send(new GameRolePlayPlayerFightFriendlyAnsweredMessage((ushort)replier.Id,
                                                                           (ulong)source.Id,
                                                                           (ulong)target.Id,
                                                                           accepted));
        }

        public static void SendGameRolePlayPlayerFightFriendlyRequestedMessage(IPacketReceiver client, Character requester, Character source, Character target)
        {
            client.Send(new GameRolePlayPlayerFightFriendlyRequestedMessage((ushort)requester.Id, (ulong)source.Id, (ulong)target.Id));
        }

        public static void SendGameRolePlayArenaUpdatePlayerInfosMessage(IPacketReceiver client, Character character)
        {
            //#region Arena3vs3_Solo

            //ArenaRanking _arenaRanking3vs3Solo = new ArenaRanking(
            //    rank: (ushort)LeaguesManager.Instance.GetRank_3vs3_Solo(character),
            //    bestRank: (ushort)LeaguesManager.Instance.GetMaxRank_3vs3_Solo(character));

            //ArenaLeagueRanking _arenaLeagueRanking3vs3Solo = new ArenaLeagueRanking(
            //    rank: (ushort)LeaguesManager.Instance.GetDayMatchs_3vs3_Solo(character),
            //    leagueId: (ushort)LeaguesManager.Instance.GetLeagueId_3vs3_Solo(character),
            //    leaguePoints: (short)LeaguesManager.Instance.GetPoints_3vs3_Solo(character),
            //    totalLeaguePoints: (short)LeaguesManager.Instance.GetMaxPoint_3vs3_Solo(character),
            //    ladderPosition: LeaguesManager.Instance.GetRank_3vs3_Solo(character));

            //ArenaRankInfos _arenaRankInfos3vs3Solo = new ArenaRankInfos(
            //    ranking: _arenaRanking3vs3Solo,
            //    leagueRanking: _arenaLeagueRanking3vs3Solo,
            //    victoryCount: (ushort)LeaguesManager.Instance.GetVictoryCount_3vs3_Solo(character),
            //    fightcount: (ushort)LeaguesManager.Instance.GetFightCount_3vs3_Solo(character),
            //    numFightNeededForLadder: (short)LeaguesManager.Instance.GetFightNeededForLadder_3vs3_Solo(character));

            //#endregion

            //#region Arena3vs3_Team

            //ArenaRanking _arenaRanking3vs3Team = new ArenaRanking(
            //    rank: (ushort)LeaguesManager.Instance.GetRank_3vs3_Team(character),
            //    bestRank: (ushort)LeaguesManager.Instance.GetMaxRank_3vs3_Team(character));

            //ArenaLeagueRanking _arenaLeagueRanking3vs3Team = new ArenaLeagueRanking(
            //    rank: (ushort)LeaguesManager.Instance.GetDayMatchs_3vs3_Team(character),
            //    leagueId: (ushort)LeaguesManager.Instance.GetLeagueId_3vs3_Team(character),
            //    leaguePoints: (short)LeaguesManager.Instance.GetPoints_3vs3_Team(character),
            //    totalLeaguePoints: (short)LeaguesManager.Instance.GetMaxPoint_3vs3_Team(character),
            //    ladderPosition: LeaguesManager.Instance.GetRank_3vs3_Team(character));

            //ArenaRankInfos _arenaRankInfos3vs3Team = new ArenaRankInfos(
            //    ranking: _arenaRanking3vs3Team,
            //    leagueRanking: _arenaLeagueRanking3vs3Team,
            //    victoryCount: (ushort)LeaguesManager.Instance.GetVictoryCount_3vs3_Team(character),
            //    fightcount: (ushort)LeaguesManager.Instance.GetFightCount_3vs3_Team(character),
            //    numFightNeededForLadder: (short)LeaguesManager.Instance.GetFightNeededForLadder_3vs3_Team(character));

            //#endregion

            //#region Arena1vs1

            //ArenaRanking _arenaRanking1vs1 = new ArenaRanking(
            //    rank: (ushort)LeaguesManager.Instance.GetRank_1vs1(character),
            //    bestRank: (ushort)LeaguesManager.Instance.GetMaxRank_1vs1(character));

            //ArenaLeagueRanking _arenaLeagueRanking1vs1 = new ArenaLeagueRanking(
            //    rank: (ushort)LeaguesManager.Instance.GetDayMatchs_1vs1(character),
            //    leagueId: (ushort)LeaguesManager.Instance.GetLeagueId_1vs1(character),
            //    leaguePoints: (short)LeaguesManager.Instance.GetPoints_1vs1(character),
            //    totalLeaguePoints: (short)LeaguesManager.Instance.GetMaxPoint_1vs1(character),
            //    ladderPosition: LeaguesManager.Instance.GetRank_1vs1(character));

            //ArenaRankInfos _arenaRankInfos1vs1 = new ArenaRankInfos(
            //    ranking: _arenaRanking1vs1,
            //    leagueRanking: _arenaLeagueRanking1vs1,
            //    victoryCount: (ushort)LeaguesManager.Instance.GetVictoryCount_1vs1(character),
            //    fightcount: (ushort)LeaguesManager.Instance.GetFightCount_1vs1(character),
            //    numFightNeededForLadder: (short)LeaguesManager.Instance.GetFightNeededForLadder_1vs1(character));

            //#endregion

            //client.Send(new GameRolePlayArenaUpdatePlayerInfosAllQueuesMessage(_arenaRankInfos3vs3Solo, _arenaRankInfos3vs3Team, _arenaRankInfos1vs1));
        }

        public static void SendGameRolePlayAggressionMessage(IPacketReceiver client, Character challenger, Character defender)
        {
            client.Send(new GameRolePlayAggressionMessage((ulong)challenger.Id, (ulong)defender.Id));
        }

        public static void SendGameFightPlacementSwapPositionsMessage(IPacketReceiver client, IEnumerable<ContextActor> actors)
        {
            client.Send(new GameFightPlacementSwapPositionsMessage(actors.Select(entry => entry.GetIdentifiedEntityDispositionInformations())));
        }

        public static void SendGameFightPlacementSwapPositionsOfferMessage(IPacketReceiver client, Character source, Character target)
        {
            client.Send(new GameFightPlacementSwapPositionsOfferMessage(source.Id, source.Fighter.Id, (ushort)source.Cell.Id, target.Fighter.Id, (ushort)target.Cell.Id));
        }

        public static void SendGameFightPlacementSwapPositionsCancelledMessage(IPacketReceiver client, Character source, Character canceller)
        {
            client.Send(new GameFightPlacementSwapPositionsCancelledMessage(source.Fighter.Id, canceller.Fighter.Id));
        }
    }
}