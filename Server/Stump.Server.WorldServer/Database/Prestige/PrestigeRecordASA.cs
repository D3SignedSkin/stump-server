using Stump.ORM.SubSonic.SQLGeneration.Schema;
using Stump.ORM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stump.Server.WorldServer.Database.Prestige
{
    public class PrestigeRewardRelator
    {
        public static string FetchQuery = "SELECT * FROM prestige_reward";
    }
    [TableName("prestige_reward")]
    public class PrestigeRewardRecord
    {
        [PrimaryKey("Id")] public int Id { get; set; }
        public int PrestigeRank { get; set; }
        public int ItemId { get; set; }
        public int ItemAmount { get; set; }
        public int TitleReward { get; set; }
        public int OrnamentReward { get; set; }
        public int LevelRequired { get; set; }
        public string MessageReward { get; set; }
    }
    public class PrestigeStatsRelator
    {
        public static string FetchQuery = "SELECT * FROM prestige_stats";
    }
    [TableName("prestige_stats")]
    public class PrestigeStatsRecord
    {
        [PrimaryKey("Id")] public int Id { get; set; }
        public int PrestigeRank { get; set; }
        public string EffectId { get; set; }
        public string EffectAmount { get; set; }
    }
}
