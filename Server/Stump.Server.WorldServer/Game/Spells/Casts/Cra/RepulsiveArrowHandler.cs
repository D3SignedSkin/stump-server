using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Fights;

namespace Stump.Server.WorldServer.Game.Spells.Casts.Ecaflip
{
    [SpellCastHandler(SpellIdEnum.REPULSIVE_ARROW_13072)]
    public class RepulsiveArrowHandler : DefaultSpellCastHandler //By Kenshin Version 2.61.10
    {
        public RepulsiveArrowHandler(SpellCastInformations cast) : base(cast)
        { }

        public override void Execute()
        {
            if (!base.Initialize())
                Initialize();

            Handlers[0].Apply();
            Handlers[1].Apply();
            //Handlers[2].Apply(); //Effect Duplicate
        }
    }
}