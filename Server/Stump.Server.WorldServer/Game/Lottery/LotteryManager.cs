using System.Collections.Generic;
using Stump.Server.BaseServer.Database;
using Stump.Server.BaseServer.Initialization;
using Stump.Server.WorldServer.Database.Lottery;
using NLog;

namespace Stump.Server.WorldServer.Game.Lottery
{
    class LotteryManager : DataManager<LotteryManager>
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly Dictionary<int, LotteryRewards> m_lottery = new Dictionary<int, LotteryRewards>();

        [Initialization(InitializationPass.Any)]
        public override void Initialize()
        {
            foreach (var reward in Database.Query<LotteryRewards>(LotteryRewardsTableRelator.FetchQuery))
            {
                m_lottery.Add(reward.ItemID, reward);
            }
        }

        public LotteryRewards GetLotteryItemById(int itemID)
        {
            foreach (var item in m_lottery)
            {
                if (item.Value.ItemID == itemID)
                    return item.Value;
            }
            return null;
        }

        public Dictionary<int, LotteryRewards> GetItemsLottery()
        {
            return m_lottery;
        }
    }
}