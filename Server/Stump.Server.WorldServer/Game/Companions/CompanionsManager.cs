using System;
using System.Collections.Generic;
using System.Linq;
using Stump.Server.WorldServer.Database.Companion;
using Stump.Server.BaseServer.Database;
using System.Text;
using System.Threading.Tasks;

namespace Stump.Server.WorldServer.Game.Companions
{
    public class CompanionsManager : DataManager<CompanionsManager>
    {
        public IEnumerable<CompanionRecord> companion;
        public IEnumerable<CompanionRecord> GetCompanionById(int ItemId)
        {
            companion = base.Database.Fetch<CompanionRecord>(string.Format(CompanionRelator.FetchQuery), new object[0])
                 .Where(x => x.ItemId == ItemId);
            return companion;
        }
    }
}
