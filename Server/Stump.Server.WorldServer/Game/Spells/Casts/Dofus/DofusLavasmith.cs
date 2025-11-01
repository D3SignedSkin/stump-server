using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Fights;

namespace Stump.Server.WorldServer.Game.Spells.Casts
{
    [SpellCastHandler(SpellIdEnum.LAVASMITH_DOFUS_10164)]
    public class DofusLavasmith : DefaultSpellCastHandler
    {
        public DofusLavasmith(SpellCastInformations cast) : base(cast) { }

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