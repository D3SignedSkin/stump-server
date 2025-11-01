using System;
using System.Collections.Generic;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Database.World;
using Stump.DofusProtocol.Enums.Extensions;

namespace Stump.Server.WorldServer.Game.Maps.Cells.Shapes
{
    public class Rectangle : IShape
    {
        private uint Width = 1;
      
        private uint Height = 1;
        
        public Rectangle(byte alternativeSize,  byte radius)
        {
            if(alternativeSize < 1)
            {
                alternativeSize = 1;
            }
            if(radius < 1)
            {
                radius = 1;
            }
            Radius = radius;
            Width = (uint)(1 + radius * 2);
            Height = (uint)(1 + alternativeSize);
        }

        #region IShape Members

        public uint Surface => Width * Height;

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
            uint j = 0;
            int x = 0;
            int y = 0;
            MapPoint origin = new MapPoint(centerCell.Id);
            List<Cell> cells = new List<Cell>();
            int sign = Direction == DirectionsEnum.DIRECTION_NORTH_WEST || Direction == DirectionsEnum.DIRECTION_SOUTH_WEST ? -1 : 1;
            bool axisFlag = Direction == DirectionsEnum.DIRECTION_NORTH_EAST || Direction == DirectionsEnum.DIRECTION_SOUTH_WEST;
            if (MinRadius == 0)
                cells.Add(centerCell);
            for(uint i = 0; i < Height; i++)
            {
                for(j = 0; j < Width; j++)
                {
                    if(axisFlag)
                    {
                        x = (int)(origin.X + j - Math.Floor((decimal)(this.Width / 2)));
                        y = (int)(origin.Y + i * sign);
                    }
                    else
                    {
                        x = (int)(origin.X + i * sign);
                        y = (int)(origin.Y + j - Math.Floor((decimal)(Width / 2)));
                    }
                    AddCellIfValid(x,y, map, cells);
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