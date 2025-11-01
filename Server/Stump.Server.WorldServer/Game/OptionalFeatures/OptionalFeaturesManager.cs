using System.Collections.Generic;
using System.Linq;
using Stump.Server.BaseServer.Database;
using Stump.Server.BaseServer.Initialization;
using Stump.Server.WorldServer.Database.Npcs;

namespace Stump.Server.WorldServer.Game.Parties
{
    class OptionalFeaturesManager : DataManager<OptionalFeaturesManager>
    {
        private List<FeatureTemplatesRecord> _records = new List<FeatureTemplatesRecord>();

        [Initialization(InitializationPass.Any)]
        public override void Initialize()
        {
            _records = Database.Fetch<FeatureTemplatesRecord>("select * from feature_descriptions");
        }

        public int[] GetFeaturesId() 
        {
            return _records.Where(entry => entry.Active).Select(x => x.Id).ToArray(); 
        }
    }
}