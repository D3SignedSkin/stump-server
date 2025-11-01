using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Fights;
using System.Linq;

namespace Stump.Server.WorldServer.Game.Spells.Casts.Ecaflip
{
    [SpellCastHandler(SpellIdEnum.XELORS_SANDGLASS_13261)]
    public class SandGlassHandler : DefaultSpellCastHandler //By Kenshin Version 2.61.10
    {
        public SandGlassHandler(SpellCastInformations cast) : base(cast)
        { }

        public override void Execute()
        {
            if (!base.Initialize())
                Initialize();

            var target = Fight.Fighters.FirstOrDefault(x => x.Cell == TargetedCell);

            Handlers[0].Apply();
            Handlers[1].Apply();

            //Ataque Alvo
            Handlers[2].Apply();

            Handlers[3].Apply();
            Handlers[4].Apply();

            //Ataque Grupo
            if (target != null && target.HasState((int)SpellStatesEnum.TELEFRAG_244) || target.HasState((int)SpellStatesEnum.TELEFRAG_251))
                Handlers[5].Apply();
        }
    }
}