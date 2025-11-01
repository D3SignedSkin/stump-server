using Stump.Core.Extensions;
using Stump.Server.BaseServer.Database;
using Stump.Server.BaseServer.Initialization;
using Stump.Server.WorldServer.Database.Arena;
using Stump.Server.WorldServer.Database.Arena.Leagues;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Stump.Server.WorldServer.Game.Arena.Leagues
{
    public enum LeaguesEnum
    {
        Bronze,
        Silver,
        Gold,
        Cristal,
        Diamond,
        Legend
    }

    public class VersusLeagueData
    {
        public VersusLeagueData(LeaguesEnum type, int pointstonextleague, int winpoints, int lostpoints)
        {
            Type = type;
            PointsToNextLeague = pointstonextleague;
            WinPoints = winpoints;
            LostPoints = lostpoints;
        }

        public LeaguesEnum Type
        {
            get;
            set;
        }

        public int PointsToNextLeague
        {
            get;
            set;
        }

        public int WinPoints
        {
            get;
            set;
        }

        public int LostPoints
        {
            get;
            set;
        }
    }

    public class LeaguesManager : DataManager<LeaguesManager>
    {
        private List<CharactersLeagues> CharactersLeagues = new List<CharactersLeagues>();
        private List<ArenaLeague> Leagues = new List<ArenaLeague>();
        private List<ArenaLeagueReward> LeaguesRewards = new List<ArenaLeagueReward>();
        private List<ArenaLeagueSeason> LeaguesSeasons = new List<ArenaLeagueSeason>();
        private Dictionary<LeaguesEnum, VersusLeagueData> LeaguesInformations = new Dictionary<LeaguesEnum, VersusLeagueData>();


        [Initialization(InitializationPass.Fifth)]
        public override void Initialize()
        {
            Leagues = Database.Query<ArenaLeague>(ArenaLeagueRelator.FetchQuery).OrderBy(x => x.NextLeagueId).ToList();
            LeaguesRewards = Database.Query<ArenaLeagueReward>(ArenaLeagueRewardRelator.FetchQuery).ToList();
            LeaguesSeasons = Database.Query<ArenaLeagueSeason>(ArenaLeagueSeasonRelator.FetchQuery).ToList();
            CharactersLeagues = Database.Query<CharactersLeagues>(CharactersLeagueRelator.FetchQuery).ToList();

            InitializeLeaguesDatas();
        }

        private void InitializeLeaguesDatas()
        {
            var Bronze = new VersusLeagueData(LeaguesEnum.Bronze, 250, 300, 50);
            var Silver = new VersusLeagueData(LeaguesEnum.Silver, 100, 150, 80);
            var Gold = new VersusLeagueData(LeaguesEnum.Gold, 100, 100, 50);
            var Cristal = new VersusLeagueData(LeaguesEnum.Cristal, 100, 50, 20);
            var Diamond = new VersusLeagueData(LeaguesEnum.Diamond, 100, 50, 40);
            var Legend = new VersusLeagueData(LeaguesEnum.Legend, 0, 50, 50);

            LeaguesInformations.Clear();
            LeaguesInformations.Add(LeaguesEnum.Bronze, Bronze);
            LeaguesInformations.Add(LeaguesEnum.Silver, Silver);
            LeaguesInformations.Add(LeaguesEnum.Gold, Gold);
            LeaguesInformations.Add(LeaguesEnum.Cristal, Cristal);
            LeaguesInformations.Add(LeaguesEnum.Diamond, Diamond);
            LeaguesInformations.Add(LeaguesEnum.Legend, Legend);

            int currentrank = 1;
            Tuple<int, int> savedgoldfix = new Tuple<int, int>(0, 0);

            foreach (var league in Leagues)
            {
                if (league.Type == LeaguesEnum.Legend)
                {
                    league.MinRequiredRank = 5501;
                    league.MaxRequiredRank = int.MaxValue;
                    continue;
                }

                if (league.Id == 34)
                {
                    league.MinRequiredRank = savedgoldfix.Item1;
                    league.MaxRequiredRank = savedgoldfix.Item2;
                    continue;
                }

                var RankGap = GetLeagueData(league.Type).PointsToNextLeague;

                league.MinRequiredRank = currentrank;

                if (league.Id == 6) league.MinRequiredRank = int.MinValue;

                currentrank += RankGap;

                league.MaxRequiredRank = currentrank - 1;

                if (league.Id == 33)
                {
                    savedgoldfix = new Tuple<int, int>(currentrank, currentrank + RankGap - 1);
                    currentrank += RankGap;
                }
            }
        }

        #region >> Koliseu Rank 1vs1
        //ArenaRanking
        public int GetRank_1vs1(Character character) => GetRankPosition1vs1(character);//Pegar a posição do jogador de acordo com a quantidade de pontos dele.
        public int GetMaxRank_1vs1(Character character) => character.ArenaMaxPointsRank_1vs1; //E o maior Ranking que já pegou na temporada
        //ArenaLeagueRanking
        public int GetDayMatchs_1vs1(Character character) => GetCotationPosition1vs1(character);//Cotação
        public int GetLeagueId_1vs1(Character character) => 6;//Pega a LigaID referente a quantidade de pontos do jogador
        public int GetPoints_1vs1(Character character) => character.ArenaPointsRank_1vs1;//Pegar a quantidade de Pontos que o jogador possui
        public int GetMaxPoint_1vs1(Character character) => 0;//Pegar a o máximo de pontos que a liga atual precisa
        //ArenaRankInfos
        public int GetVictoryCount_1vs1(Character character) => character.ArenaDayVictoryCount_1vs1;
        public int GetFightCount_1vs1(Character character) => character.ArenaDayFightCount_1vs1;
        public int GetFightNeededForLadder_1vs1(Character character) => 0;
        #endregion Koliseu Rank 1vs1

        #region >> Koliseu Rank 3vs3 Solo
        //ArenaRanking
        public int GetRank_3vs3_Solo(Character character) => GetRankPosition3vs3Solo(character); //Pegar a posição do jogador de acordo com a quantidade de pontos dele.
        public int GetMaxRank_3vs3_Solo(Character character) => character.ArenaMaxPointsRank_3vs3_Solo; //E o maior Ranking que já pegou na temporada
        //ArenaLeagueRanking
        public int GetDayMatchs_3vs3_Solo(Character character) => GetCotationPosition3vs3Solo(character); //Cotação
        public int GetLeagueId_3vs3_Solo(Character character) => 6; //Pega a LigaID referente a quantidade de pontos do jogador
        public int GetPoints_3vs3_Solo(Character character) => character.ArenaPointsRank_3vs3_Solo; //Pegar a quantidade de Pontos que o jogador possui
        public int GetMaxPoint_3vs3_Solo(Character character) => 0; //Pegar a o máximo de pontos que a liga atual precisa
        //ArenaRankInfos
        public int GetVictoryCount_3vs3_Solo(Character character) => character.ArenaDayVictoryCount_3vs3_Solo;
        public int GetFightCount_3vs3_Solo(Character character) => character.ArenaDayFightCount_3vs3_Solo;
        public int GetFightNeededForLadder_3vs3_Solo(Character character) => 0;
        #endregion >> Koliseu Rank 3vs3 Solo

        #region >> Koliseu Rank 3vs3 Team
        //ArenaRanking
        public int GetRank_3vs3_Team(Character character) => GetRankPosition3vs3Team(character); //Pegar a posição do jogador de acordo com a quantidade de pontos dele.
        public int GetMaxRank_3vs3_Team(Character character) => character.ArenaMaxPointsRank_3vs3_Team; //E o maior Ranking que já pegou na temporada
        //ArenaLeagueRanking
        public int GetDayMatchs_3vs3_Team(Character character) => GetCotationPosition3vs3Team(character); //Cotação
        public int GetLeagueId_3vs3_Team(Character character) => 6; //Pega a LigaID referente a quantidade de pontos do jogador
        public int GetPoints_3vs3_Team(Character character) => character.ArenaPointsRank_3vs3_Team; //Pegar a quantidade de Pontos que o jogador possui
        public int GetMaxPoint_3vs3_Team(Character character) => 0; //Pegar a o máximo de pontos que a liga atual precisa
        //ArenaRankInfos
        public int GetVictoryCount_3vs3_Team(Character character) => character.ArenaDayVictoryCount_3vs3_Team;
        public int GetFightCount_3vs3_Team(Character character) => character.ArenaDayFightCount_3vs3_Team;
        public int GetFightNeededForLadder_3vs3_Team(Character character) => 0;
        #endregion >> Koliseu Rank 3vs3 Team

        public short GetLeagueCharacter() => 6;

        public ArenaLeague GetLeague(int leagueId) => Leagues.FirstOrDefault(x => x.LeagueId == leagueId);

        public VersusLeagueData GetLeagueData(LeaguesEnum type)
        {
            return LeaguesInformations.GetOrDefault(type);
        }

        public ArenaLeague GetNextLeague(ArenaLeague league) => Leagues.FirstOrDefault(x => x.LeagueId == league.NextLeagueId);

        public List<ArenaLeague> GetLeaguesByType(LeaguesEnum type) => Leagues.Where(x => x.Type == type).ToList();

        #region >> GetPosition Player
        public int GetRankPosition1vs1(Character character)
        {
            var orderedList = CharactersLeagues.OrderByDescending(c => c.ArenaPointsRank_1vs1).ToList();
            int position = orderedList.FindIndex(owner => owner.Id == character.Id);

            if (position < 0)
            {
                return orderedList.Count + 1;
            }

            return position + 1;
        }

        public int GetRankPosition3vs3Solo(Character character)
        {
            var orderedList = CharactersLeagues.OrderByDescending(c => c.ArenaPointsRank_3vs3_Solo).ToList();
            int position = orderedList.FindIndex(owner => owner.Id == character.Id);

            if (position < 0)
            {
                return orderedList.Count + 1;
            }

            return position + 1;
        }

        public int GetRankPosition3vs3Team(Character character)
        {
            var orderedList = CharactersLeagues.OrderByDescending(c => c.ArenaPointsRank_3vs3_Team).ToList();
            int position = orderedList.FindIndex(owner => owner.Id == character.Id);

            if (position < 0)
            {
                return orderedList.Count + 1;
            }

            return position + 1;
        }
        #endregion

        #region >> GetCotation Player
        public int GetCotationPosition1vs1(Character character)
        {
            int winnersCount = character.ArenaDayVictoryCount_1vs1;
            int lossersCount = character.ArenaDayFightCount_1vs1 - character.ArenaDayVictoryCount_1vs1;

            var result = winnersCount * 3 + character.ArenaPointsRank_1vs1 - lossersCount * 2;

            if (result < 0)
                result = 0;

            return result;
        }

        public int GetCotationPosition3vs3Solo(Character character)
        {
            int winnersCount = character.ArenaDayVictoryCount_3vs3_Solo;
            int lossersCount = character.ArenaDayFightCount_3vs3_Solo - character.ArenaDayVictoryCount_3vs3_Solo;

            var result = winnersCount * 3 + character.ArenaPointsRank_3vs3_Solo - lossersCount * 2;

            if (result < 0)
                result = 0;

            return result;
        }

        public int GetCotationPosition3vs3Team(Character character)
        {
            int winnersCount = character.ArenaDayVictoryCount_3vs3_Team;
            int lossersCount = character.ArenaDayFightCount_3vs3_Team - character.ArenaDayVictoryCount_3vs3_Team;

            var result = winnersCount * 3 + character.ArenaPointsRank_3vs3_Team - lossersCount * 2;

            if (result < 0)
                result = 0;

            return result;
        }
        #endregion
    }
}