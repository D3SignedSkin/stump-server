using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Fights;

namespace Stump.Server.WorldServer.Game.Spells.Casts.Ecaflip
{
    [SpellCastHandler(SpellIdEnum.FUNGAL_PUNISHMENT_1145)]
    public class FungalPunishmentHandler : DefaultSpellCastHandler //By Kenshin Version 2.61.10
    {
        public FungalPunishmentHandler(SpellCastInformations cast) : base(cast)
        { }

        public override void Execute()
        {
            if (!base.Initialize())
                Initialize();

            Handlers[0].Apply();
            Handlers[1].Apply();
        }
    }
}