using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Enums.Custom;
using Stump.Server.WorldServer.Database.Items.Templates;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Fights.Teams;

namespace Stump.Server.WorldServer.Game.Fights.Challenges.Custom
{
    [ChallengeIdentifier((int)ChallengeEnum.KORRIANDRE__CHALLENGE_1_)]
    [ChallengeIdentifier((int)ChallengeEnum.MERKATOR__CHALLENGE_1_)]
    [ChallengeIdentifier((int)ChallengeEnum.BETHEL_AKARNA__CHALLENGE_1_)]
    public class MystiqueAchievements : DefaultChallenge
    {
        public MystiqueAchievements(int id, IFight fight) : base(id, fight)
        {
            BonusMin = 0;
            BonusMax = 0;

            IsAchievement = true;
        }

        public override void Initialize()
        {
            base.Initialize();

            foreach (var fighter in Fight.GetAllFighters<CharacterFighter>())
                fighter.WeaponUsed += OnWeaponUsed;
        }

        void OnWeaponUsed(FightActor caster, WeaponTemplate weapon, Cell target, FightSpellCastCriticalEnum critical, bool silentCast) => UpdateStatus(ChallengeStatusEnum.FAILED_1);

        protected override void OnWinnersDetermined(IFight fight, FightTeam winners, FightTeam losers, bool draw)
        {
            base.OnWinnersDetermined(fight, winners, losers, draw);

            foreach (var fighter in Fight.GetAllFighters<CharacterFighter>())
                fighter.WeaponUsed -= OnWeaponUsed;
        }
    }
}