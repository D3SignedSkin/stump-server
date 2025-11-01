using System.Linq;
using Stump.DofusProtocol.Enums.Custom;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Fights.Teams;

namespace Stump.Server.WorldServer.Game.Fights.Challenges.Custom
{
    [ChallengeIdentifier((int)ChallengeEnum.BOOSTACHE__CHALLENGE_2_)]
    [ChallengeIdentifier((int)ChallengeEnum.KANKREBLATH__CHALLENGE_1_)]
    [ChallengeIdentifier((int)ChallengeEnum.NELWEEN__CHALLENGE_1_)]
    [ChallengeIdentifier((int)ChallengeEnum.CHOUQUE__CHALLENGE_1_)]
    [ChallengeIdentifier((int)ChallengeEnum.TOFU_ROYAL__CHALLENGE_1_)]
    [ChallengeIdentifier((int)ChallengeEnum.MANSOT_ROYAL__CHALLENGE_1_)]
    [ChallengeIdentifier((int)ChallengeEnum.SPHINCTER_CELL__CHALLENGE_2_)]
    [ChallengeIdentifier((int)ChallengeEnum.OUGAH__CHALLENGE_2_)]
    [ChallengeIdentifier((int)ChallengeEnum.KOLOSSO__CHALLENGE_1_)]
    [ChallengeIdentifier((int)ChallengeEnum.FUJI_GIVREFOUX_NOURRICIÈRE__CHALLENGE_1_)]
    [ChallengeIdentifier((int)ChallengeEnum.NILEZA__CHALLENGE_2_)]
    [ChallengeIdentifier((int)ChallengeEnum.MERKATOR__CHALLENGE_1_)]
    [ChallengeIdentifier((int)ChallengeEnum.DANTINEA__CHALLENGE_1_)]
    public class ReprieveAchievements : DefaultChallenge
    {
        public ReprieveAchievements(int id, IFight fight) : base(id, fight)
        {
            BonusMin = 0;
            BonusMax = 0;

            IsAchievement = true;
        }

        public override void Initialize()
        {
            base.Initialize();

            Target = Fight.GetRandomFighter<MonsterFighter>();

            foreach (var fighter in Target.Team.Fighters)
                fighter.Dead += OnDead;
        }

        public override bool IsEligible() => Fight.GetAllFighters<MonsterFighter>().Count() > 1;

        void OnDead(FightActor victim, FightActor killer)
        {
            if (Target.Team.Fighters.Where(x => x.IsAlive()).Count() != 0 && victim == Target)
                UpdateStatus(ChallengeStatusEnum.FAILED_1);
        }

        protected override void OnWinnersDetermined(IFight fight, FightTeam winners, FightTeam losers, bool draw)
        {
            base.OnWinnersDetermined(fight, winners, losers, draw);

            foreach (var fighter in Target.Team.Fighters)
                fighter.Dead -= OnDead;
        }
    }
}