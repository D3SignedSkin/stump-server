using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Fights;

namespace Stump.Server.WorldServer.Game.Spells.Casts.Osamodas
{
    [SpellCastHandler(SpellIdEnum.TOFU_ASCENDANT_14159)]
    [SpellCastHandler(SpellIdEnum.GOBBALL_ASCENDANT_14160)]
    [SpellCastHandler(SpellIdEnum.TOAD_ASCENDANT_14161)]
    [SpellCastHandler(SpellIdEnum.WYRMLING_ASCENDANT_14162)]
    public class AscendantHandler : DefaultSpellCastHandler //By Kenshin Version 2.61.10
    {
        public AscendantHandler(SpellCastInformations cast) : base(cast)
        { }

        public override void Execute()
        {
            if (!base.Initialize())
                Initialize();

            //Effect_TriggerBuff - 13992
            Handlers[0].Apply();

            //Effect_TriggerBuff - 14235
            Handlers[1].Apply();
        }
    }
}