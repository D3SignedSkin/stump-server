using System.Collections.Generic;
using System.Linq;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Database.World;

namespace Stump.Server.WorldServer.Game.Maps.Cells.Shapes
{
    public class SemiColumn : IShape
    {
        private IEnumerable<short> Cells
        {
            get;
            set;
        }

        public SemiColumn(IEnumerable<short> cells)
        {
            this.Cells = cells;
        }

        public  Cell[] GetCells(Cell centerCell, Cell casterCell, Map map)
        {
            return Cells.Select(x => map.GetCell(x)).ToArray();
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