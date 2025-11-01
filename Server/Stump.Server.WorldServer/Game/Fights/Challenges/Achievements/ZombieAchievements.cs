using Stump.DofusProtocol.Enums.Custom;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Fights.Teams;

namespace Stump.Server.WorldServer.Game.Fights.Challenges.Custom
{
    [ChallengeIdentifier((int)ChallengeEnum.KARDORIM__CHALLENGE_1_)]
    [ChallengeIdentifier((int)ChallengeEnum.SCARABOSSE_DORÉ__CHALLENGE_1_)]
    [ChallengeIdentifier((int)ChallengeEnum.KWAKWA__CHALLENGE_1_)]
    [ChallengeIdentifier((int)ChallengeEnum.KANNIBOUL_EBIL__CHALLENGE_1_)]
    [ChallengeIdentifier((int)ChallengeEnum.MANTISCORE__CHALLENGE_2_)]
    [ChallengeIdentifier((int)ChallengeEnum.KOULOSSE__CHALLENGE_1_)]
    [ChallengeIdentifier((int)ChallengeEnum.TYNRIL_AHURI__CHALLENGE_2_)]
    [ChallengeIdentifier((int)ChallengeEnum.USH_GALESH__CHALLENGE_2_)]
    [ChallengeIdentifier((int)ChallengeEnum.PÈRE_VER__CHALLENGE_2_)]
    [ChallengeIdentifier((int)ChallengeEnum.KORRIANDRE__CHALLENGE_2_)]
    [ChallengeIdentifier((int)ChallengeEnum.BETHEL_AKARNA__CHALLENGE_2_)]
    public class ZombieAchievements : DefaultChallenge
    {
        public ZombieAchievements(int id, IFight fight) : base(id, fight)
        {
            BonusMin = 0;
            BonusMax = 0;

            IsAchievement = true;
        }

        public override void Initialize()
        {
            base.Initialize();

            Fight.BeforeTurnStopped += OnBeforeTurnStopped;
        }

        private void OnBeforeTurnStopped(IFight fight, FightActor fighter)
        {
            if (!(fighter is CharacterFighter))
                return;

            if (fighter.UsedMP == 1)
                return;

            UpdateStatus(ChallengeStatusEnum.FAILED_1);
            Fight.BeforeTurnStopped -= OnBeforeTurnStopped;
        }

        protected override void OnWinnersDetermined(IFight fight, FightTeam winners, FightTeam losers, bool draw)
        {
            OnBeforeTurnStopped(fight, fight.FighterPlaying);

            base.OnWinnersDetermined(fight, winners, losers, draw);

            Fight.BeforeTurnStopped -= OnBeforeTurnStopped;
        }
    }
}