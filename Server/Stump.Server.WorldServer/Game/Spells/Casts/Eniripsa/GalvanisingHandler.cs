using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Fights;

namespace Stump.Server.WorldServer.Game.Spells.Casts.Ecaflip
{
    [SpellCastHandler(SpellIdEnum.GALVANISING_WORD_13218)]
    public class GalvanisingHandler : DefaultSpellCastHandler //By Kenshin Version 2.61.10
    {
        public GalvanisingHandler(SpellCastInformations cast) : base(cast)
        { }

        public override void Execute()
        {
            if (!base.Initialize())
                Initialize();

            Handlers[0].Apply();
            Handlers[1].Apply();
            //Handlers[2].Apply(); //Effect Duplicate (Desactive)
        }
    }
}