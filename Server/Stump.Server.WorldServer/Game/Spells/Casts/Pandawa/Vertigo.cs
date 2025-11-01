using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Fights;
using System.Linq;

namespace Stump.Server.WorldServer.Game.Spells.Casts.Panda
{
    [SpellCastHandler(SpellIdEnum.VERTIGO_12788)]
    public class Vertigo : DefaultSpellCastHandler
    {
        public Vertigo(SpellCastInformations cast) : base(cast)
        { }

        public override void Execute()
        {
            if (!m_initialized)
                Initialize();

            var m_affectedactors = Caster.GetCarriedActor();

            if (m_affectedactors != null)
            {
                Handlers[0].AddAffectedActor(m_affectedactors);

                Handlers[0].Priority = 1300;
                Handlers[1].Priority = 1400;
                Handlers[2].Priority = 2000;
                Handlers[3].Priority = 1200;
            }

            foreach (var handler in Handlers.OrderBy(x => x.Priority))
            {
                handler.Apply();
            }
        }
    }
}