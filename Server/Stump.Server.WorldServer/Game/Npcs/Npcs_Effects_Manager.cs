using System.Collections.Generic;
using Stump.Server.BaseServer.Database;
using Stump.Server.BaseServer.Initialization;
using NLog;
using Stump.Server.WorldServer.Database.Npcs.Effects;

namespace Stump.Server.WorldServer.Game.Npcs
{
    class Npcs_Effects_Manager : DataManager<Npcs_Effects_Manager>
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly Dictionary<int, Npcs_Effects> m_effects = new Dictionary<int, Npcs_Effects>();

        [Initialization(InitializationPass.Any)]
        public override void Initialize()
        {
            foreach (
                var reward in Database.Query<Npcs_Effects>(NpcsEffectsTableRelator.FetchQuery))
            {
                m_effects.Add(reward.EffectID, reward);
            }
        }

        public Npcs_Effects GetEffectById(int effectID)
        {
            foreach (var item in m_effects)
            {
                if (item.Value.EffectID == effectID)
                    return item.Value;
            }
            return null;
        }

        public Dictionary<int, Npcs_Effects> GetEffects()
        {
            return m_effects;
        }
    }
}
