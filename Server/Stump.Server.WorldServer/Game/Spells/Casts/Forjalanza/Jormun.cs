using System.Collections.Generic;
using System.Linq;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Spells.Casts;

namespace Stump.Server.WorldServer.game.spells.Casts.Forjalanza
{
    [SpellCastHandler(23268)]
    public class Jormun: DefaultSpellCastHandler {
        public Jormun(SpellCastInformations cast) : base(cast)
        {
        }
        
        public override void Execute()
        {
            if (!m_initialized)
                Initialize();

            var range = CastPoint.DistanceTo(TargetedCell);
            var direction = CastPoint.OrientationTo(TargetedCell);
            
            var affectCells = Fight.GetAllFightersInLine(CastCell, (int)range, direction).Where(x => x.IsEnnemyWith(Caster)).ToList();

            Handlers[0].Apply();
            Handlers[1].Apply();
            
            if (TargetedCell != Caster.Cell)
            {
                //affectCells.Add(TargetedCell);
                Handlers[2].SetAffectedActors(affectCells);
            }
            else
            {
                Handlers[3].Apply();
            }
            Handlers[4].Apply();
            Handlers[5].Apply();
            Handlers[6].Apply();
            Handlers[7].Apply();
        }
    }
}