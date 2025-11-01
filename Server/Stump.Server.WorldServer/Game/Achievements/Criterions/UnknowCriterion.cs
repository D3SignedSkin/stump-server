using System;
using System.Linq;
using Stump.Server.WorldServer.Database.Achievements;
using Stump.Server.WorldServer.Game.Achievements.Criterions.Data;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Conditions;

namespace Stump.Server.WorldServer.Game.Achievements.Criterions
{
    public class UnknowCriterion : AbstractCriterion<UnknowCriterion, DefaultCriterionData>
    {
        // FIELDS
        public const string Identifier = "Wo";
        private ushort? m_maxValue;

        // CONSTRUCTORS
        public UnknowCriterion(AchievementObjectiveRecord objective) : base(objective)
        { }

        // METHODS
        public override DefaultCriterionData Parse(ComparaisonOperatorEnum @operator, params string[] parameters)
        {
            return new DefaultCriterionData(@operator, parameters);
        }

        public override bool Eval(Character character)
        {
            return true;
        }

        public override ushort GetPlayerValue(PlayerAchievement player)
        {
            return (ushort) Math.Min(0, player.GetRunningCriterion(this));
        }
    }
}