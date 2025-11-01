using System;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Messages;
using Stump.DofusProtocol.Types;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Maps;
using Stump.Server.WorldServer.Game.Maps.Cells;
using System.Linq;

namespace Stump.Server.WorldServer.Game.Actors.RolePlay
{
    public abstract class RolePlayActor : ContextActor
    {
        public event Action<RolePlayActor, Map> EnterMap;
        public event Action<RolePlayActor, SubArea> ChangeSubArea;

        public virtual void OnEnterMap(Map map)
        {
            if (LastMap == null || LastMap.SubArea != map.SubArea)
            {
                OnChangeSubArea(map.SubArea);
            }

            EnterMap?.Invoke(this, map);
        }

        public virtual void OnChangeSubArea(SubArea subArea)
        {
            ChangeSubArea?.Invoke(this, subArea);
        }

        public event Action<RolePlayActor, Map> LeaveMap;
        public virtual void OnLeaveMap(Map map)
        {
            LeaveMap?.Invoke(this, map);
        }

        #region Network

        public override GameContextActorInformations GetGameContextActorInformations(Character character)
        {
            return new GameRolePlayActorInformations(
                Id,
                GetEntityDispositionInformations(),
                Look.GetEntityLook());
        }

        #endregion

        #region Actions

        #region Teleport

        public event Action<ContextActor, ObjectPosition> Teleported;

        protected virtual void OnTeleported(ObjectPosition position)
        {
            var handler = Teleported;
            if (handler != null) handler(this, position);
        }

        protected override void OnInstantMoved(Cell cell)
        {
            Map.Clients.Send(new TeleportOnSameMapMessage(Id, (ushort)cell.Id));

            base.OnInstantMoved(cell);
        }

        public virtual bool Teleport(MapNeighbour mapNeighbour)
        {
            var neighbour = Position.Map.GetNeighbouringMap(mapNeighbour);

            if (neighbour == null)
                return false;

            var character = this as Character;

            if (character != null)
            {
                if (character.Area.Id == 42 && character.IsInIncarnation && neighbour.SubArea.Id != character.SubArea.Id)
                {
                    return false;
                }
            }

            var cell = Position.Map.GetCellAfterChangeMap(Position.Cell.Id, mapNeighbour);

            if (cell < 0 || cell >= 560)
            {
                logger.Error("Cell {0} out of range, current={1} neighbour={2}", cell, Cell.Id, mapNeighbour);
                return false;
            }

            var destination = new ObjectPosition(neighbour, cell, Position.Direction);

            if (!destination.Cell.Walkable)
            {
                var cells = MapPoint.GetBorderCells(mapNeighbour).Select(x => neighbour.Cells[x.CellId]).Where(x => x.Walkable);
                var walkableCell = cells.Select(x => new MapPoint(x)).OrderBy(x => x.EuclideanDistanceTo(destination.Point)).FirstOrDefault();

                if (walkableCell != null)
                {
                    destination.Cell = neighbour.Cells[walkableCell.CellId];
                }
            }

            return Teleport(destination);
        }

        public virtual bool Teleport(Map mapScroll, MapNeighbour mapNeighbour)
        {
            var neighbour = mapScroll;

            if (neighbour == null)
                return false;

            var cell = Position.Map.GetCellAfterChangeMap(Position.Cell.Id, mapNeighbour);

            if (neighbour.Id == 154010883)
                cell = 370;

            if (cell < 0 || cell >= 560)
            {
                logger.Error("Cell {0} out of range, current={1} neighbour={2}", cell, Cell.Id, mapNeighbour);
                return false;
            }

            var destination = new ObjectPosition(neighbour, cell, Position.Direction);

            if (!destination.Cell.Walkable)
            {
                var cells = MapPoint.GetBorderCells(mapNeighbour).Select(x => neighbour.Cells[x.CellId]).Where(x => x.Walkable == true);

                if (!cells.ToList().Any())
                {
                    if ((int)mapNeighbour == 2)
                    {
                        if (MapPoint.GetBorderCells((MapNeighbour)3).Select(x => neighbour.Cells[x.CellId]).Where(x => x.Walkable == true).ToList().Any())
                        {
                            cells = MapPoint.GetBorderCells((MapNeighbour)3).Select(x => neighbour.Cells[x.CellId]).Where(x => x.Walkable == true);
                        }
                        else if (MapPoint.GetBorderCells((MapNeighbour)4).Select(x => neighbour.Cells[x.CellId]).Where(x => x.Walkable == true).ToList().Any())
                        {
                            cells = MapPoint.GetBorderCells((MapNeighbour)4).Select(x => neighbour.Cells[x.CellId]).Where(x => x.Walkable == true);
                        }
                        else if (MapPoint.GetBorderCells((MapNeighbour)1).Select(x => neighbour.Cells[x.CellId]).Where(x => x.Walkable == true).ToList().Any())
                        {
                            cells = MapPoint.GetBorderCells((MapNeighbour)1).Select(x => neighbour.Cells[x.CellId]).Where(x => x.Walkable == true);
                        }
                    }
                    else if ((int)mapNeighbour == 3)
                    {
                        if (MapPoint.GetBorderCells((MapNeighbour)4).Select(x => neighbour.Cells[x.CellId]).Where(x => x.Walkable == true).ToList().Any())
                        {
                            cells = MapPoint.GetBorderCells((MapNeighbour)4).Select(x => neighbour.Cells[x.CellId]).Where(x => x.Walkable == true);
                        }
                        else if (MapPoint.GetBorderCells((MapNeighbour)1).Select(x => neighbour.Cells[x.CellId]).Where(x => x.Walkable == true).ToList().Any())
                        {
                            cells = MapPoint.GetBorderCells((MapNeighbour)1).Select(x => neighbour.Cells[x.CellId]).Where(x => x.Walkable == true);
                        }
                        else if (MapPoint.GetBorderCells((MapNeighbour)2).Select(x => neighbour.Cells[x.CellId]).Where(x => x.Walkable == true).ToList().Any())
                        {
                            cells = MapPoint.GetBorderCells((MapNeighbour)2).Select(x => neighbour.Cells[x.CellId]).Where(x => x.Walkable == true);
                        }
                    }
                    else if ((int)mapNeighbour == 4)
                    {
                        if (MapPoint.GetBorderCells((MapNeighbour)1).Select(x => neighbour.Cells[x.CellId]).Where(x => x.Walkable == true).ToList().Any())
                        {
                            cells = MapPoint.GetBorderCells((MapNeighbour)1).Select(x => neighbour.Cells[x.CellId]).Where(x => x.Walkable == true);
                        }
                        else if (MapPoint.GetBorderCells((MapNeighbour)2).Select(x => neighbour.Cells[x.CellId]).Where(x => x.Walkable == true).ToList().Any())
                        {
                            cells = MapPoint.GetBorderCells((MapNeighbour)2).Select(x => neighbour.Cells[x.CellId]).Where(x => x.Walkable == true);
                        }
                        else if (MapPoint.GetBorderCells((MapNeighbour)3).Select(x => neighbour.Cells[x.CellId]).Where(x => x.Walkable == true).ToList().Any())
                        {
                            cells = MapPoint.GetBorderCells((MapNeighbour)3).Select(x => neighbour.Cells[x.CellId]).Where(x => x.Walkable == true);
                        }
                    }
                    else if ((int)mapNeighbour == 1)
                    {
                        if (MapPoint.GetBorderCells((MapNeighbour)2).Select(x => neighbour.Cells[x.CellId]).Where(x => x.Walkable == true).ToList().Any())
                        {
                            cells = MapPoint.GetBorderCells((MapNeighbour)2).Select(x => neighbour.Cells[x.CellId]).Where(x => x.Walkable == true);
                        }
                        else if (MapPoint.GetBorderCells((MapNeighbour)3).Select(x => neighbour.Cells[x.CellId]).Where(x => x.Walkable == true).ToList().Any())
                        {
                            cells = MapPoint.GetBorderCells((MapNeighbour)3).Select(x => neighbour.Cells[x.CellId]).Where(x => x.Walkable == true);
                        }
                        else if (MapPoint.GetBorderCells((MapNeighbour)4).Select(x => neighbour.Cells[x.CellId]).Where(x => x.Walkable == true).ToList().Any())
                        {
                            cells = MapPoint.GetBorderCells((MapNeighbour)4).Select(x => neighbour.Cells[x.CellId]).Where(x => x.Walkable == true);
                        }
                    }
                }

                var walkableCell = cells.Select(x => new MapPoint(x)).OrderBy(x => x.EuclideanDistanceTo(destination.Point)).FirstOrDefault();

                if (walkableCell != null)
                    destination.Cell = neighbour.Cells[walkableCell.CellId];
            }

            return Teleport(destination);
        }

        public virtual bool Teleport(Map map, Cell cell) => Teleport(new ObjectPosition(map, cell));

        public virtual bool Teleport(ObjectPosition destination, bool performCheck = true)
        {
            if (IsMoving())
                StopMove();

            if (!CanChangeMap() && performCheck)
                return false;

            if (Position.Map == destination.Map)
                return MoveInstant(destination);

            NextMap = destination.Map;
            LastMap = Map;

            Position.Map.Leave(this);

            if (this is Character)
            {
                foreach (var following in (this as Character).following)
                {
                    following.Position.Map.Leave(following);
                }
            }

            if (!NextMap.Area.IsRunning)
                NextMap.Area.Start();

            //NextMap.Area.ExecuteInContext(() =>
            //    {
            Position = destination.Clone();
            Position.Map.Enter(this);

            NextMap = null;
            LastMap = null;

            OnTeleported(Position);

            if (this is Character)
            {
                foreach (var following in (this as Character).following)
                {
                    var position_char = destination.Clone();
                    var excludedCells = position_char.Map.GetActors<RolePlayActor>().Select(entry => entry.Cell.Id);
                    var position = new ObjectPosition(position_char.Map, position_char.Point.GetAdjacentCells(true).Where(x => x.IsInMap()).OrderBy(x => x.ManhattanDistanceTo(position_char.Point)).Where(x => position_char.Map.Cells[x.CellId].Walkable && !excludedCells.Contains(x.CellId)).FirstOrDefault().CellId, position_char.Direction);
                    following.Position = position;
                    if (following is FollowerActor)
                        (following as FollowerActor).m_contextId = following.Position.Map.GetNextContextualId();
                    following.Position.Map.Enter(following);
                    following.OnTeleported(following.Position);
                }
            }
            //});

            return true;
        }

        public virtual bool CanChangeMap() => Map != null && Map.IsActor(this);

        #endregion

        #endregion

        protected override void OnDisposed()
        {
            if (Map != null && Map.IsActor(this))
            {
                //Map.Area.ExecuteInContext(() =>
                //{
                if (Map != null && Map.IsActor(this))
                {

                    Map.Leave(this);
                    if (this is Character)
                        foreach (var following in (this as Character).following)
                        {
                            following.Map.Leave(following);
                        }
                }

                base.OnDisposed();
                //});
            }
            else
            {
                base.OnDisposed();
            }
        }
    }
}