using Stump.DofusProtocol.Enums.Custom;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Fights.Teams;

namespace Stump.Server.WorldServer.Game.Fights.Challenges.Custom
{
    [ChallengeIdentifier((int)ChallengeEnum.SCARABOSSE_DORÉ__CHALLENGE_2_)]
    [ChallengeIdentifier((int)ChallengeEnum.KWAKWA__CHALLENGE_2_)]
    [ChallengeIdentifier((int)ChallengeEnum.BULBIG_BROZEUR__CHALLENGE_2_)]
    [ChallengeIdentifier((int)ChallengeEnum.WA_WOBOT__CHALLENGE_1_)]
    [ChallengeIdentifier((int)ChallengeEnum.MANTISCORE__CHALLENGE_1_)]
    [ChallengeIdentifier((int)ChallengeEnum.KOULOSSE__CHALLENGE_2_)]
    [ChallengeIdentifier((int)ChallengeEnum.MINOTOROR__CHALLENGE_2_)]
    [ChallengeIdentifier((int)ChallengeEnum.ROYALMOUTH__CHALLENGE_1_)]
    public class CleanHandsAchievements : DefaultChallenge
    {
        public CleanHandsAchievements(int id, IFight fight) : base(id, fight)
        {
            BonusMin = 0;
            BonusMax = 0;

            IsAchievement = true;
        }

        public override void Initialize()
        {
            base.Initialize();

            foreach (var fighter in Fight.GetAllFighters<MonsterFighter>())
                fighter.DamageInflicted += OnDamageInflicted;
        }

        void OnDamageInflicted(FightActor fighter, Damage damage)
        {
            if (fighter.IsAlive())
                return;

            if (!(damage.Source is CharacterFighter))
                return;

            if (damage.Spell == null)
            {
                UpdateStatus(ChallengeStatusEnum.FAILED_1, damage.Source);
                return;
            }

            if (fighter.IsIndirectSpellCast(damage.Spell) || fighter.IsPoisonSpellCast(damage.Spell))
                return;

            UpdateStatus(ChallengeStatusEnum.FAILED_1, damage.Source);
        }

        protected override void OnWinnersDetermined(IFight fight, FightTeam winners, FightTeam losers, bool draw)
        {
            base.OnWinnersDetermined(fight, winners, losers, draw);

            foreach (var fighter in Fight.GetAllFighters<MonsterFighter>())
                fighter.DamageInflicted -= OnDamageInflicted;
        }
    }
}