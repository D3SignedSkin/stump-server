using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Fights;
using System.Linq;

namespace Stump.Server.WorldServer.Game.Spells.Casts
{
    [SpellCastHandler(SpellIdEnum.SILVER_DOFUS_18674)]
    public class DofusSilver : DefaultSpellCastHandler
    {
        public DofusSilver(SpellCastInformations cast) : base(cast) { }

        public override void Execute()
        {
            foreach (var handler in Handlers)
            {
                handler.SetAffectedActor(this.Caster);
                handler.Apply();
            }
        }
    }

    [SpellCastHandler(SpellIdEnum.SILVER_DOFUS_18675)]
    public class DofusSilverOne : DefaultSpellCastHandler
    {
        public DofusSilverOne(SpellCastInformations cast) : base(cast) { }

        public override void Execute()
        {
            foreach (var handler in Handlers)
            {
                handler.SetAffectedActor(this.Caster);
                handler.Apply();
            }
        }
    }
}