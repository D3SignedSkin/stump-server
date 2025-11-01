using System.Collections.Generic;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Database.World;
using Stump.DofusProtocol.Enums.Extensions;

namespace Stump.Server.WorldServer.Game.Maps.Cells.Shapes
{
    public class Fork : IShape
    {
        private uint Length = 0;
        
        public Fork(byte radius)
        {
            Radius = radius;
            Length = (uint)(Radius + 1);
        }

        #region IShape Members

        public uint Surface => 1 + 3 * Length;

        public byte MinRadius
        {
            get;
            set;
        }

        public DirectionsEnum Direction
        {
            get;
            set;
        }

        public byte Radius
        {
            get;
            set;
        }

        public Cell[] GetCells(Cell centerCell,Cell casterCell, Map map)
        {
            int j = 0;
            int x = 0;
            int y = 0;
            MapPoint origin = new MapPoint(centerCell.Id);
            List<Cell> cells = new List<Cell>();
            int sign =  Direction == DirectionsEnum.DIRECTION_NORTH_WEST || Direction == DirectionsEnum.DIRECTION_SOUTH_WEST ? -1 : 1;
            bool axisFlag = Direction == DirectionsEnum.DIRECTION_NORTH_WEST || Direction == DirectionsEnum.DIRECTION_SOUTH_EAST;
            if (MinRadius == 0)
                cells.Add(centerCell);
            for(int i = 1; i <= Length; i++)
            {
                for(j = -1; j <= 1; j++)
                {
                    x = 0;
                    y = 0;
                    if(axisFlag)
                    {
                        x = origin.X + i * sign;
                        y = origin.Y + j * i;
                    }
                    else
                    {
                        x = origin.X + j * i;
                        y = origin.Y + i * sign;
                    }
                    AddCellIfValid(x, y, map, cells);
                }
            }
            return cells.ToArray();
        }

        private static void AddCellIfValid(int x, int y, Map map, IList<Cell> container)
        {
            if (!MapPoint.IsInMap(x, y))
                return;

            container.Add(map.Cells[MapPoint.CoordToCellId(x, y)]);
        }
        #endregion
    }
}