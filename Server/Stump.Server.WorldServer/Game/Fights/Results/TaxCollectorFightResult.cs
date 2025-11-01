using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Types;
using Stump.Server.WorldServer.Game.Actors.Fight;

namespace Stump.Server.WorldServer.Game.Fights.Results
{
    public class TaxCollectorFightResult : FightResult<TaxCollectorFighter>
    {
        public TaxCollectorFightResult(TaxCollectorFighter fighter, FightOutcomeEnum outcome, FightLoot loot) : base(fighter, outcome, loot)
        { }

        public override FightResultListEntry GetFightResultListEntry()
        {
            int temp = 200;

            return new FightResultTaxCollectorListEntry(
                outcome: (ushort)Outcome,
                wave: 0,
                rewards: Loot.GetFightLoot(),
                id: Id,
                alive: Alive,
                allianceInfo: Fighter.TaxCollectorNpc.Guild.Alliance.GetBasicAllianceInformations());
        }
    }
}