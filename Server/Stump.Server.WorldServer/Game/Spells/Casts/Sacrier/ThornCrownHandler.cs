using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Fights;

namespace Stump.Server.WorldServer.Game.Spells.Casts.Ecaflip
{
    [SpellCastHandler(SpellIdEnum.THORN_CROWN_12761)]
    public class ThornCrownHandler : DefaultSpellCastHandler //By Kenshin Version 2.61.10
    {
        public ThornCrownHandler(SpellCastInformations cast) : base(cast)
        { }

        public override void Execute()
        {
            if (!base.Initialize())
                Initialize();

            Handlers[0].Apply();
            //Handlers[1].Apply();
            Handlers[2].Apply();
            Handlers[3].Apply();
            Handlers[4].Apply();
            Handlers[5].Apply();
            Handlers[6].Apply();
        }
    }
}