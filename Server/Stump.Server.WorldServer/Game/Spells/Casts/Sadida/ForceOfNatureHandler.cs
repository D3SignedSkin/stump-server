using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Spells.Casts;

namespace Stump.Server.WorldServer.game.spells.Casts.Sadida
{
    [SpellCastHandler(SpellIdEnum.FORCE_OF_NATURE_13570)]
    public class ForceOfNatureHandler : DefaultSpellCastHandler
    {
        public ForceOfNatureHandler(SpellCastInformations cast) : base(cast)
        { }

        public override void Execute()
        {
            if (!base.Initialize())
                Initialize();

            Handlers[0].Apply();
            Handlers[1].Apply();
        }
    }

    [SpellCastHandler(SpellIdEnum.FORCE_OF_NATURE_13584)]
    public class BoostForceOfNatureHandler : DefaultSpellCastHandler
    {
        public BoostForceOfNatureHandler(SpellCastInformations cast) : base(cast)
        { }

        public override void Execute()
        {
            if (!base.Initialize())
                Initialize();

            //Effect_SpellBoost - 13570
            Handlers[0].Apply();

            //Effect_SpellBoost - 13594
            Handlers[1].Apply();
        }
    }
}