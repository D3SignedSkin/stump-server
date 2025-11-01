using System.Linq;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Spells.Casts;

namespace Stump.Server.WorldServer.game.spells.Casts.Forjalanza
{
    [SpellCastHandler(23434)]
    public class Balestra : DefaultSpellCastHandler
    {
        public Balestra(SpellCastInformations cast) : base(cast)
        {
        }
        
        public override void Execute()
        {
            if (!m_initialized)
                Initialize();

            var range = CastPoint.DistanceTo(TargetedCell);
            var direction = CastPoint.OrientationTo(TargetedCell);
            
            var affectActors = Fight.GetAllFightersInLine(CastCell, (int)range, direction).Where(x => x.IsEnnemyWith(Caster)).ToList();
            
            if (Caster.HasState(3360))
             Handlers[8].SetAffectedActors(affectActors);
            if (Caster.HasState(3589))
                Handlers[9].SetAffectedActors(affectActors);
           

            base.Execute();
        }
    }
}