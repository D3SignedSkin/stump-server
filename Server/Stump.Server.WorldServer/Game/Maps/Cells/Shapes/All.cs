using System.Linq;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Database.World;

namespace Stump.Server.WorldServer.Game.Maps.Cells.Shapes
{
    public class All : IShape
    {
       

        public  Cell[] GetCells(Cell centerCell, Cell casterCell, Map map)
        {
            return map.Cells.Where(x => x.Walkable).ToArray();
        }

        public uint Surface { get; }
        public byte MinRadius { get; set; }
        public DirectionsEnum Direction { get; set; }
        public byte Radius { get; set; }
        public Cell[] GetCells(Cell centerCell, Map map)
        {
            throw new System.NotImplementedException();
        }
    }
}