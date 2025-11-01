using System.Linq;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Spells.Casts;

namespace Stump.Server.WorldServer.game.spells.Casts.Forjalanza
{
    [SpellCastHandler(23392)]
    public class JabalinaRayo:  DefaultSpellCastHandler
    {
        public JabalinaRayo(SpellCastInformations cast) : base(cast)
        {
        }
        
        public override void Execute()
        {
            if (!m_initialized)
                Initialize();

            var range = CastPoint.DistanceTo(TargetedCell);
            var direction = CastPoint.OrientationTo(TargetedCell);
            
            var affectActors = Fight.GetAllFightersInLine(CastCell, (int)range, direction).Where(x => x.IsEnnemyWith(Caster)).ToList();
            Handlers[5].SetAffectedActors(affectActors);
            if (!Fight.IsCellFree(TargetedCell))
            {
                //var affectActorsAdjacent = Fight.Map.cell
            }

            base.Execute();
        }
    }
}