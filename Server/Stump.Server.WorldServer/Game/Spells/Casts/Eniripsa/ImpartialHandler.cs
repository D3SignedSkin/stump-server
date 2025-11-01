using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Fights;

namespace Stump.Server.WorldServer.Game.Spells.Casts.Ecaflip
{
    [SpellCastHandler(SpellIdEnum.IMPARTIAL_WORD_13208)]
    public class ImpartialHandler : DefaultSpellCastHandler //By Kenshin Version 2.61.10
    {
        public ImpartialHandler(SpellCastInformations cast) : base(cast)
        { }

        public override void Execute()
        {
            if (!base.Initialize())
                Initialize();

            Handlers[0].Apply();
            //Handlers[1].Apply(); //Duplicating
            Handlers[2].Apply();
            //Handlers[3].Apply(); //Duplicating
        }
    }
}