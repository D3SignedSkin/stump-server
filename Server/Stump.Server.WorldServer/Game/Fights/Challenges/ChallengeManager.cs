using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NLog;
using Stump.Core.Mathematics;
using Stump.Core.Reflection;
using Stump.Server.BaseServer.Database;
using Stump.Server.BaseServer.Initialization;
using Stump.Server.WorldServer.Database.Fights;

namespace Stump.Server.WorldServer.Game.Fights.Challenges
{
    public class ChallengeManager : DataManager<ChallengeManager>
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly Dictionary<int, Type> _challenges = new Dictionary<int, Type>();
        private static Dictionary<int, ChallengeRecord> m_challengesRecords = new Dictionary<int, ChallengeRecord>();

        [Initialization(InitializationPass.Fourth)]
        public override void Initialize()
        {
            m_challengesRecords = Database.Fetch<ChallengeRecord>(ChallengeRelator.FetchQuery).ToDictionary(x => x.Id);

            RegisterAll(Assembly.GetExecutingAssembly());
        }

        public void RegisterAll(Assembly assembly)
        {
            if (assembly == null)
                return;

            foreach (var type in assembly.GetTypes().Where(type => type.IsSubclassOf(typeof(DefaultChallenge))))
            {
                RegisterChallenge(type);
            }
        }

        public void RegisterChallenge(Type challenge)
        {
            var challengeIdentifiers = challenge.GetCustomAttributes<ChallengeIdentifierAttribute>().SelectMany(attr => attr.Identifiers).Where(identifier => !_challenges.ContainsKey(identifier));

            foreach (var identifier in challengeIdentifiers)
            {
                if (!m_challengesRecords.Any(entry => entry.Value.Id == identifier))
                {
                    logger.Warn($"Challenge Manager record does not contain challenge key {identifier}.");
                    continue;
                }

                _challenges.Add(identifier, challenge);
            }
        }

        public DefaultChallenge GetDefaultChallenge(IFight fight)
        {
            return new DefaultChallenge(0, fight);
        }

        public DefaultChallenge GetChallenge(int identifier, IFight fight)
        {
            return _challenges.TryGetValue(identifier, out var challengeType) ? (DefaultChallenge)Activator.CreateInstance(challengeType, identifier, fight) : null;
        }

        public DefaultChallenge GetRandomChallenge(IFight fight)
        {
            var eligibleChallenges = _challenges.Keys
                .OrderBy(_ => new CryptoRandom().Next())
                .Select(id => GetChallenge(id, fight))
                .Where(challenge => challenge != null && challenge.IsEligible() && !challenge.IsAchievement)
                .ToList();

            return eligibleChallenges.FirstOrDefault();
        }

        public List<DefaultChallenge> GetRandomChallenges(IFight fight, int challengeAmount = 2)
        {
            var eligibleChallenges = _challenges.Keys
                .OrderBy(_ => new CryptoRandom().Next())
                .Select(id => GetChallenge(id, fight))
                .Where(challenge => challenge != null && challenge.IsEligible() && !challenge.IsAchievement && !fight.Challenges.Contains(challenge))
                .Take(challengeAmount)
                .ToList();

            return eligibleChallenges;
        }
    }
}