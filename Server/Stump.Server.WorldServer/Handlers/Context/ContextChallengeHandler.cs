using System;
using System.Linq;
using Stump.DofusProtocol.Types;
using Stump.DofusProtocol.Enums;
using System.Collections.Generic;
using Stump.DofusProtocol.Messages;
using Stump.Server.BaseServer.Network;
using Stump.DofusProtocol.Enums.Custom;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Core.Network;
using Stump.Server.WorldServer.Game.Fights.Challenges;

namespace Stump.Server.WorldServer.Handlers.Context
{
    public partial class ContextHandler
    {
        #region >> Handlers

        [WorldHandler(ChallengeReadyMessage.Id)]
        public static void HandleChallengeReadyMessage(WorldClient client, ChallengeReadyMessage message)
        {
            if (!client.Character.IsFighting())
                return;

            SendChallengeProposalMessage(client, client.Character.Fight);
        }

        [WorldHandler(ChallengeValidateMessage.Id)]
        public static void HandleChallengeValidateMessage(WorldClient client, ChallengeValidateMessage message)
        {
            if (!client.Character.IsFighting())
                return;

            foreach (var challenge in client.Character.Fight.Challenges)
            {
                challenge.IsSelected = (challenge.Id == message.challengeId);
            }

            DefaultChallenge selectedChallenge = client.Character.Fight.Challenges.FirstOrDefault(chall => chall.IsSelected);

            SendChallengeAddMessage(client, selectedChallenge);

            if (client.Character.Fight.Map.IsDungeon() || client.Character.Fight is FightPvD)
            {
                SendChallengeProposalMessage(client, client.Character.Fight);
            }
        }

        [WorldHandler(ChallengeSelectionMessage.Id)]
        public static void HandleChallengeSelectionMessage(WorldClient client, ChallengeSelectionMessage message)
        {
            if (!client.Character.IsFighting())
                return;

            foreach (var challenge in client.Character.Fight.Challenges)
            {
                challenge.IsSelected = (challenge.Id == message.challengeId);
            }
        }

        [WorldHandler(ChallengeTargetsRequestMessage.Id)]
        public static void HandleChallengeTargetsRequestMessage(WorldClient client, ChallengeTargetsRequestMessage message)
        {
            if (!client.Character.IsFighting())
                return;

            DefaultChallenge _challenge = ChallengeManager.Instance.GetChallenge((int)message.challengeId, client.Character.Fight);

            if (_challenge is null)
                return;

            if (_challenge?.Target == null)
                return;

            if (!_challenge.Target.IsVisibleFor(client.Character))
                return;

            //List<FightActor> _monsterList = client.Character.Fight.DefendersTeam.Fighters.Where(entry => _challenge.Targets != null && !entry.IsDead() && _challenge.Targets.Contains(entry)).ToList();

            //foreach (FightActor monster in _monsterList)
            //{
            //    client.Send(new ShowCellMessage(sourceId: monster.Id, cellId: (ushort)monster.Cell.Id));
            //}

            SendChallengeTargetsListMessage(_challenge.Fight.Clients, _challenge);
        }

        [WorldHandler(ChallengeModSelectMessage.Id)]
        public static void HandleChallengeModSelectMessage(WorldClient client, ChallengeModSelectMessage message)
        {
            client.Character.ChallengeMod = message.challengeMod;
            client.Character.Fight.ChallengeDropOrXp = message.challengeMod;

            SendChallengeModSelectedMessage(client, message.challengeMod);
        }

        [WorldHandler(ChallengeBonusChoiceMessage.Id)]
        public static void HandleChallengeBonusChoiceMessage(WorldClient client, ChallengeBonusChoiceMessage message)
        {
            client.Character.ChallengeXpOrDrop = message.challengeBonus;

            SendChallengeBonusChoiceSelectedMessage(client, message.challengeBonus);
        }

        #endregion

        public static int GetChallengeCount(IFight fight) => fight.Map.IsDungeon() || fight is FightPvD ? 2 : 1;

        public static bool GetFightAllowsChallenge(IFight fight) => (fight is FightPvM) || (fight is FightPvD) && fight.Map.AllowFightChallenges;

        public static void SendChallengeNumberMessage(WorldClient client, int number)
        {
            client.Send(new ChallengeNumberMessage(challengeNumber: (uint)number));
        }

        public static void SendChallengeAddMessage(IPacketReceiver client, DefaultChallenge challenge)
        {
            client.Send(new ChallengeAddMessage(challenge.GetChallengeInformation()));
        }

        public static void SendChallengeBonusChoiceSelectedMessage(IPacketReceiver client, sbyte bonus)
        {
            client.Send(new ChallengeBonusChoiceSelectedMessage(challengeBonus: bonus));
        }

        public static void SendChallengeModSelectedMessage(WorldClient client, sbyte mod)
        {
            client.Send(new ChallengeModSelectedMessage(challengeMod: mod));
        }

        public static void SendChallengeResultMessage(IPacketReceiver client, DefaultChallenge challenge)
        {
            client.Send(new ChallengeResultMessage(challengeId: (uint)challenge.Id, success: challenge.Status == ChallengeStatusEnum.COMPLETED_0));
        }

        public static void SendChallengeTargetsListMessage(IPacketReceiver client, DefaultChallenge challenge)
        {
            client.Send(new ChallengeTargetsMessage(challenge.GetChallengeInformation()));
        }

        public static void SendChallengeProposalMessage(IPacketReceiver client, IFight fight)
        {
            const int challengeCount = 2;

            List<ChallengeInformation> _listInformation = new List<ChallengeInformation>();
            List<DefaultChallenge> _challenges = ChallengeManager.Instance.GetRandomChallenges(fight, challengeCount);

            foreach (var challenge in _challenges)
            {
                _listInformation.Add(challenge.GetChallengeInformation());
                fight.AddChallenge(challenge);
            }

            client.Send(new ChallengeProposalMessage(challengeProposals: _listInformation, timer: 15));
        }

        public static void SendChallengeFightValidated(WorldClient client, IFight fight, int challengeAmount)
        {
            if (fight.Challenges is null || !fight.Challenges.Any())
            {
                List<DefaultChallenge> challenges = ChallengeManager.Instance.GetRandomChallenges(fight, challengeAmount);

                challenges.ForEach(challenge =>
                {
                    challenge.IsSelected = true;
                    fight.AddChallenge(challenge);
                    SendChallengeAddMessage(client, challenge);
                });

                SendChallengeListMessage(client, fight);
                return;
            }

            List<DefaultChallenge> unvalidatedChallenges = fight.Challenges.Where(chall => !chall.IsSelected).ToList();

            if (unvalidatedChallenges.Count > 1)
            {
                if (client.Character.ChallengeMod == (sbyte)ChallengeModEnum.CHALLENGE_RANDOM_1)
                {
                    unvalidatedChallenges.ForEach(entry => entry.IsSelected = false);
                    unvalidatedChallenges[new Random().Next(unvalidatedChallenges.Count)].IsSelected = true;
                }
                else
                {
                    DefaultChallenge _chall = unvalidatedChallenges.FirstOrDefault();

                    if (_chall != null)
                    {
                        _chall.IsSelected = true;
                    }
                }
            }

            fight.Challenges.Where(chall => chall.IsSelected).ToList().ForEach(challenge => SendChallengeAddMessage(client, challenge));
            fight.Challenges.RemoveAll(chall => !chall.IsSelected);

            SendChallengeListMessage(client, fight);
        }

        public static void SendChallengeListMessage(WorldClient client, IFight fight)
        {
            if (fight.Challenges?.Any(chall => chall.IsSelected) != true)
                return;

            List<ChallengeInformation> challengeInformationList = fight.Challenges
                .Where(challenge => challenge.IsSelected)
                .Select(challenge => challenge.GetChallengeInformation())
                .ToList();

            client.Send(new ChallengeListMessage(challengesInformation: challengeInformationList));
        }
    }
}