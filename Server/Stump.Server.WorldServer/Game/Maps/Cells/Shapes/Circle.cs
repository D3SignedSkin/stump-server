using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Database.World;
using System.Collections.Generic;

namespace Stump.Server.WorldServer.Game.Maps.Cells.Shapes
{
    public class Circle 
    {
        // FIELDS

        // PROPERTIES
        public uint Surface
        {
            get
            {
                return (uint)(this.Radius * 4);
            }
        }

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

        // CONSTRUCTORS
        public Circle(byte radius)
        {
            this.Radius = radius;
            this.Direction = DirectionsEnum.DIRECTION_SOUTH_EAST;
        }

        // METHODS
        public Cell[] GetCells(Cell centerCell, Map map)
        {
            var result = new List<Cell>();
            var mapPoint = new MapPoint(centerCell);

            for (int i = 0; i < this.Radius; i++)
            {
                if (i == 0)
                {
                    Circle.AddCellIfValid(mapPoint.X, mapPoint.Y - (this.Radius - i), map, result);
                    Circle.AddCellIfValid(mapPoint.X, mapPoint.Y + (this.Radius - i), map, result);
                }
                else
                {
                    Circle.AddCellIfValid(mapPoint.X + i, mapPoint.Y - (this.Radius - i), map, result);
                    Circle.AddCellIfValid(mapPoint.X + i, mapPoint.Y + (this.Radius - i), map, result);

                    Circle.AddCellIfValid(mapPoint.X - i, mapPoint.Y - (this.Radius - i), map, result);
                    Circle.AddCellIfValid(mapPoint.X - i, mapPoint.Y + (this.Radius - i), map, result);
                }
            }

            Circle.AddCellIfValid(mapPoint.X + this.Radius, mapPoint.Y, map, result);
            Circle.AddCellIfValid(mapPoint.X - this.Radius, mapPoint.Y, map, result);

            return result.ToArray();
        }

        private static void AddCellIfValid(int x, int y, Map map, IList<Cell> container)
        {
            if (MapPoint.IsInMap(x, y))
            {
                container.Add(map.Cells[(int)((System.UIntPtr)MapPoint.CoordToCellId(x, y))]);
            }
        }
    }
}
