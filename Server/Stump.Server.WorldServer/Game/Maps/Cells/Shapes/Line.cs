using System.Collections.Generic;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Database.World;
using Stump.DofusProtocol.Enums.Extensions;

namespace Stump.Server.WorldServer.Game.Maps.Cells.Shapes
{
    public class Line : IShape
    {
        private bool FromCaster { get; set; }
        private bool StopAtTarget { get; set; }

        public Line(byte minRadius, byte radius, bool fromCaster = false, bool stopAtTarget = false)
        {
            Radius = radius;
            Direction = DirectionsEnum.DIRECTION_SOUTH_EAST;
            Radius = radius;
            MinRadius = minRadius;
            FromCaster = fromCaster;
            StopAtTarget = stopAtTarget;
        }

        #region IShape Members

        public uint Surface => (uint)Radius + 1;

        public byte MinRadius { get; set; }

        public DirectionsEnum Direction { get; set; }

        public byte Radius { get; set; }

        public Cell[] GetCells(Cell centerCell, Cell casterCell, Map map)
        {
            uint distance = 0;
            List<Cell> cells = new List<Cell>();
            MapPoint origin = !FromCaster ? new MapPoint(centerCell.Id) : new MapPoint(casterCell.Id);
            int x = origin.X;
            int y = origin.Y;
            uint length = (uint)(!FromCaster ? Radius : Radius + MinRadius - 1);
            if (FromCaster && StopAtTarget)
            {
                distance = origin.DistanceTo(new MapPoint(centerCell.Id));
                length = distance < length ? (distance) : (length);
            }

            for (int r = MinRadius; r <= length; r++)
            {
                switch (Direction)
                {
                    case DirectionsEnum.DIRECTION_WEST:
                        AddCellIfValid(x - r, y - r, map, cells);
                        break;
                    case DirectionsEnum.DIRECTION_NORTH:
                        AddCellIfValid(x - r, y + r, map, cells);
                        break;
                    case DirectionsEnum.DIRECTION_EAST:
                        AddCellIfValid(x + r, y + r, map, cells);
                        break;
                    case DirectionsEnum.DIRECTION_SOUTH:
                        AddCellIfValid(x + r, y - r, map, cells);
                        break;
                    case DirectionsEnum.DIRECTION_NORTH_WEST:
                        AddCellIfValid(x - r, y, map, cells);
                        break;
                    case DirectionsEnum.DIRECTION_SOUTH_WEST:
                        AddCellIfValid(x, y - r, map, cells);
                        break;
                    case DirectionsEnum.DIRECTION_SOUTH_EAST:
                        AddCellIfValid(x + r, y, map, cells);
                        break;
                    case DirectionsEnum.DIRECTION_NORTH_EAST:
                        AddCellIfValid(x, y + r, map, cells);
                        break;
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