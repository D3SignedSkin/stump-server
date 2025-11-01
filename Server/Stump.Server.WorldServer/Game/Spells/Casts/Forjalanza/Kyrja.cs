using System.Collections.Generic;
using System.Linq;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Spells.Casts;

namespace Stump.Server.WorldServer.game.spells.Casts.Forjalanza
{
    [SpellCastHandler(23823)]
    public class Kyrja:  DefaultSpellCastHandler
    {
        public Kyrja(SpellCastInformations cast) : base(cast)
        {
        }
        
        public override void Execute()
        {
            if (!m_initialized)
                Initialize();

            var range = CastPoint.DistanceTo(TargetedCell);
            var direction = CastPoint.OrientationTo(TargetedCell);
            
            List<FightActor> affectedCells = Fight.GetAllFightersInLine(CastCell, (int)range, direction).Where(x => x.IsEnnemyWith(Caster)).ToList();

            Handlers[3].SetAffectedActors(affectedCells);
            Handlers[4].AddAffectedActor(Caster);

            if (affectedCells.Count > 1)
                for (int i = 0; i < affectedCells.Count - 1; i++)
                {
                    Handlers[4].Apply();
                }

            if (Caster.HasState(3360))
                Handlers[6].SetAffectedActors(affectedCells);
            if (Caster.HasState(3589))
                Handlers[7].SetAffectedActors(affectedCells);

            base.Execute();
        }
    }
}