using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Fights;

namespace Stump.Server.WorldServer.Game.Spells.Casts.Ecaflip
{
    [SpellCastHandler(SpellIdEnum.TYRANNICAL_ARROW_13082)]
    public class TyrannicalArrowHandler : DefaultSpellCastHandler //By Kenshin Version 2.61.10
    {
        public TyrannicalArrowHandler(SpellCastInformations cast) : base(cast)
        { }

        public override void Execute()
        {
            if (!base.Initialize())
                Initialize();

            Handlers[0].Apply();
            Handlers[1].Apply();
            //Handlers[2].Apply(); //Effect Duplicate
            Handlers[3].Apply();
            //Handlers[4].Apply(); //Effect Duplicate
            Handlers[5].Apply();
        }
    }
}