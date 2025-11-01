using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Fights;

namespace Stump.Server.WorldServer.Game.Spells.Casts
{
    [SpellCastHandler((int)SpellIdEnum.IVORY_DOFUS_18891)]
    public class DofusIvoryOne : DefaultSpellCastHandler
    {
        public DofusIvoryOne(SpellCastInformations cast) : base(cast)
        { }

        public override void Execute()
        {
            Handlers[0].Apply();
        }
    }

    [SpellCastHandler((int)SpellIdEnum.IVORY_DOFUS_18667)]
    public class DofusIvoryTwo : DefaultSpellCastHandler
    {
        public DofusIvoryTwo(SpellCastInformations cast) : base(cast)
        { }

        public override void Execute()
        {

            foreach (var handler in Handlers)
            {
                handler.DefaultDispellableStatus = FightDispellableEnum.DISPELLABLE_BY_DEATH;
                handler.Apply();
            }
        }
    }
}