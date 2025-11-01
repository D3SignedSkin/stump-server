using Stump.DofusProtocol.Enums.Custom;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Fights.Teams;
using System.Linq;

namespace Stump.Server.WorldServer.Game.Fights.Challenges.Custom
{
    [ChallengeIdentifier((int)ChallengeEnum.CHAFER_RONIN__CHALLENGE_1_)]
    [ChallengeIdentifier((int)ChallengeEnum.COFFRE_DES_FORGERONS__CHALLENGE_1_)]
    [ChallengeIdentifier((int)ChallengeEnum.ABRAKNYDE_ANCESTRAL__CHALLENGE_1_)]
    [ChallengeIdentifier((int)ChallengeEnum.CHOUQUE__CHALLENGE_2_)]
    [ChallengeIdentifier((int)ChallengeEnum.KRALAMOUR_GÉANT__CHALLENGE_1_)]
    [ChallengeIdentifier((int)ChallengeEnum.MISSIZ_FRIZZ__CHALLENGE_1_)]
    [ChallengeIdentifier((int)ChallengeEnum.KLIME__CHALLENGE_2_)]
    public class NomadAchievements : DefaultChallenge
    {

        static readonly int[] Nomade =
        {
            (int)ChallengeEnum.CHAFER_RONIN__CHALLENGE_1_,
            (int)ChallengeEnum.COFFRE_DES_FORGERONS__CHALLENGE_1_,
            (int)ChallengeEnum.ABRAKNYDE_ANCESTRAL__CHALLENGE_1_,
            (int)ChallengeEnum.CHOUQUE__CHALLENGE_2_,
            (int)ChallengeEnum.KRALAMOUR_GÉANT__CHALLENGE_1_,
            (int)ChallengeEnum.MISSIZ_FRIZZ__CHALLENGE_1_,
            (int)ChallengeEnum.KLIME__CHALLENGE_2_,
        };

        public NomadAchievements(int id, IFight fight) : base(id, fight)
        {
            BonusMin = 0;
            BonusMax = 0;

            IsAchievement = true;
        }

        public override void Initialize()
        {
            base.Initialize();

            Fight.BeforeTurnStopped += OnTurnStopped;
            Fight.Tackled += OnTackled;
        }

        private void OnTackled(FightActor fighter, int apTackled, int mpTackled)
        {
            if (!(fighter is CharacterFighter))
                return;

            Fight.Tackled -= OnTackled;
            UpdateStatus(ChallengeStatusEnum.FAILED_1, fighter);
        }

        private void OnTurnStopped(IFight fight, FightActor fighter)
        {
            if (fighter.IsDead())
                return;

            if (!(fighter is CharacterFighter))
                return;

            if (Nomade.Contains(Id) && fighter.MP <= 0)
                return;

            if (Id == (int)ChallengeEnum.PÉTULANT && fighter.AP <= 0)
                return;

            UpdateStatus(ChallengeStatusEnum.FAILED_1);
            Fight.BeforeTurnStopped -= OnTurnStopped;
        }

        protected override void OnWinnersDetermined(IFight fight, FightTeam winners, FightTeam losers, bool draw)
        {
            OnTurnStopped(fight, fight.FighterPlaying);

            base.OnWinnersDetermined(fight, winners, losers, draw);

            Fight.BeforeTurnStopped -= OnTurnStopped;
            Fight.Tackled -= OnTackled;
        }
    }
}