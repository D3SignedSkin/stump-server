using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Fights;

namespace Stump.Server.WorldServer.Game.Spells.Casts.Ecaflip
{
    [SpellCastHandler(SpellIdEnum.INITIADON_2356)] //MonsterIdEnum.ROYALMOUTH_2854
    public class InitiadonHandler : DefaultSpellCastHandler
    {
        public InitiadonHandler(SpellCastInformations cast) : base(cast)
        { }
        public override void Execute()
        {
            if (!base.Initialize())
                Initialize();

            //if (!this.Caster.HasState(56))
            //    Handlers[0].Apply();

            Handlers[1].Apply();
            Handlers[2].Apply();

            if (this.Caster.HasState(56))
                Handlers[3].Apply();
        }
    }
}