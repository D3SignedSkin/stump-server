using System.Linq;
using Stump.Server.WorldServer.Game.Effects.Handlers.Spells.Move;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Spells.Casts;

namespace Stump.Server.WorldServer.game.spells.Casts.Forjalanza
{
    [SpellCastHandler(23729)]
    public class LanzaCiclon:  DefaultSpellCastHandler
    {
        public LanzaCiclon(SpellCastInformations cast) : base(cast)
        {
        }
        
        public override void Execute()
        {
            if (!m_initialized)
                Initialize();

            var range = CastPoint.DistanceTo(TargetedCell);
            var direction = CastPoint.OrientationTo(TargetedCell);
            
            var affectActors = Fight.GetAllFightersInLine(CastCell, (int)range, direction).Where(x => x.IsEnnemyWith(Caster)).ToList();
            
            Handlers[4].SetAffectedActors(affectActors);
            Handlers[7].SetAffectedActors(affectActors);
            ((Push)Handlers[7]).PushDirection = direction;
           

            base.Execute();
        }
    }
}