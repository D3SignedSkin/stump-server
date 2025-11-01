using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Fights;

namespace Stump.Server.WorldServer.Game.Spells.Casts
{
    [SpellCastHandler(SpellIdEnum.EMERALD_DOFUS_8393)]
    public class DofusEmerald : DefaultSpellCastHandler
    {
        public DofusEmerald(SpellCastInformations cast) : base(cast) { }

        public override void Execute()
        {
            Handlers[0].Apply();
            //Handlers[1].Apply(); //Duplicating
        }
    }
}