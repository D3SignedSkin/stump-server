using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Fights;

namespace Stump.Server.WorldServer.Game.Spells.Casts
{
    [SpellCastHandler(SpellIdEnum.SPARKLING_SILVER_DOFUS_18672)]
    public class DofusSilverSparkling : DefaultSpellCastHandler
    {
        public DofusSilverSparkling(SpellCastInformations cast) : base(cast) { }

        public override void Execute()
        {
            foreach (var handler in Handlers)
            {
                handler.DefaultDispellableStatus = FightDispellableEnum.DISPELLABLE_BY_DEATH;
                handler.SetAffectedActor(this.Caster);
                handler.Apply();
            }
        }
    }

    [SpellCastHandler(SpellIdEnum.SPARKLING_SILVER_DOFUS_18673)]
    public class DofusSilverSparklingOne : DefaultSpellCastHandler
    {
        public DofusSilverSparklingOne(SpellCastInformations cast) : base(cast) { }

        public override void Execute()
        {
            foreach (var handler in Handlers)
            {
                handler.DefaultDispellableStatus = FightDispellableEnum.DISPELLABLE_BY_DEATH;
                handler.SetAffectedActor(this.Caster);
                handler.Apply();
            }
        }
    }
}