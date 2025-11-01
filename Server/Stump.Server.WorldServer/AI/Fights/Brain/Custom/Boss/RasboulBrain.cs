using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Spells;

namespace Stump.Server.WorldServer.AI.Fights.Brain.Custom.Boss
{
    [BrainIdentifier((int)MonsterIdEnum.SILF_LE_RASBOUL_MAJEUR_1071)] //Debug até que a mecânica esteja correta.
    public class RasboulBrain : Brain
    {
        public RasboulBrain(AIFighter fighter) : base(fighter)
        {
            fighter.Fight.FightStarted += Fight_FightStarted;
        }

        private void Fight_FightStarted(IFight obj)
        {
            this.Fighter.Stats[PlayerFields.NeutralResistPercent].Base = 60;
            this.Fighter.Stats[PlayerFields.EarthResistPercent].Base = 60;
            this.Fighter.Stats[PlayerFields.FireResistPercent].Base = 60;
            this.Fighter.Stats[PlayerFields.WaterResistPercent].Base = 60;
            this.Fighter.Stats[PlayerFields.AirResistPercent].Base = 60;
        }
    }
}