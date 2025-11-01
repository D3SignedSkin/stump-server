using System;
using System.Linq;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Types;
using System.Collections.Generic;
using Stump.DofusProtocol.Enums.Custom;
using Stump.Server.WorldServer.Handlers.Basic;
using Stump.Server.WorldServer.Handlers.Context;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Fights.Teams;

namespace Stump.Server.WorldServer.Game.Fights.Challenges
{
    public class DefaultChallenge
    {
        public DefaultChallenge(int id, IFight fight)
        {
            Id = id;
            Bonus = 0;
            Fight = fight;
            Status = ChallengeStatusEnum.RUNNING_2;
            IsSelected = false;
        }

        public int Id { get; protected set; }

        public IFight Fight { get; }

        public ChallengeStatusEnum Status { get; protected set; }

        public FightActor Target { get; set; }

        public List<FightActor> Targets { get; set; }

        public uint Bonus { get; protected set; }

        public int BonusMin { get; protected set; }

        public int BonusMax { get; protected set; }

        public Boolean IsSelected { get; set; }

        public Boolean IsAchievement { get; set; }

        public virtual void Initialize()
        {
            Bonus = this.GetBonusCalculator();

            Fight.WinnersDetermined += OnWinnersDetermined;
        }

        public virtual bool IsEligible() => Fight.GetAllFighters<MonsterFighter>().All(x => !x.Monster.Template.IncompatibleChallenges.Contains((uint)Id));

        public List<ChallengeTargetInformation> GetChallengeTargetsInformations()
        {
            if (Targets is null || Targets.Count <= 0)
                return new List<ChallengeTargetInformation>();

            return Targets.Select(target => new ChallengeTargetInformation(targetId: target.Id, targetCell: target.Cell.Id)).ToList();
        }

        public void UpdateStatus(ChallengeStatusEnum status, FightActor from = null)
        {
            if (Status != ChallengeStatusEnum.RUNNING_2)
                return;

            Status = status;

            ContextHandler.SendChallengeResultMessage(Fight.Clients, this);

            if (Status == ChallengeStatusEnum.FAILED_1 && from is CharacterFighter)
            {
                BasicHandler.SendTextInformationMessage(Fight.Clients, TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 188, ((CharacterFighter)from).Name, Id);
            }
        }

        protected virtual void OnWinnersDetermined(IFight fight, FightTeam winners, FightTeam losers, bool draw)
        {
            if (winners is FightMonsterTeam)
            {
                UpdateStatus(ChallengeStatusEnum.FAILED_1);
            }

            UpdateStatus(ChallengeStatusEnum.COMPLETED_0);

            Fight.WinnersDetermined -= OnWinnersDetermined;
        }

        public uint GetBonusCalculator()
        {
            FightMonsterTeam team = Fight.DefendersTeam as FightMonsterTeam;
            FightMonsterTeam monsterTeam = team ?? (FightMonsterTeam)Fight.ChallengersTeam;

            int groupLevel = monsterTeam.Fighters.Sum(x => x.Level);
            int ratio = Math.Max(1, groupLevel / (monsterTeam.OpposedTeam.Fighters.Sum(x => x.Level) * 2));

            return (uint)Math.Min(120, Math.Round((double)(BonusMin + (BonusMax - BonusMin) * ratio)));
        }

        public ChallengeInformation GetChallengeInformation()
        {
            return new ChallengeInformation(
                challengeId: (uint)this.Id,
                targetsList: this.GetChallengeTargetsInformations(),
                dropBonus: this.GetBonusCalculator(),
                xpBonus: this.GetBonusCalculator(),
                state: (sbyte)this.Status);
        }
    }
}