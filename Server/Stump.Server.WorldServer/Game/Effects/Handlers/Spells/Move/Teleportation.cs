using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Handlers.Actions;
using Stump.Server.WorldServer.Game.Maps.Cells;
using Stump.Server.WorldServer.Game.Spells.Casts;
using Spell = Stump.Server.WorldServer.Game.Spells.Spell;
using System.Collections.Generic;

namespace Stump.Server.WorldServer.Game.Effects.Handlers.Spells.Move
{
    [EffectHandler(EffectsEnum.Effect_Teleport)]
    public class Teleportation : SpellEffectHandler
    {
        public Teleportation(EffectDice effect, FightActor caster, SpellCastHandler castHandler, Cell targetedCell, bool critical) : base(effect, caster, castHandler, targetedCell, critical)
        { }

        protected override bool InternalApply()
        {
            var carryingActor = Caster.GetCarryingActor();
            var casterPoint = Caster.Position.Point;
            var targetPoint = TargetedPoint;
            var cell = Caster.Position.Cell;
            var pushDirection = Caster.Position.Point.OrientationTo(TargetedCell);
            var pushDirectiontarget = Caster.Position.Point.OrientationTo(Caster.Cell);

            Caster.Direction = pushDirection;

            if (!Caster.CanBeMoved())
                return false;

            if (carryingActor != null)
            {
                carryingActor.ThrowActor(TargetedCell);
            }
            else if (Spell.Id == (int)SpellIdEnum.BACKUP_13043) //Feca 2.61.10.19 - Teleport Symetric
            {
                var directionAdjustments = new Dictionary<DirectionsEnum, (int, int)>
                {
                    { DirectionsEnum.DIRECTION_SOUTH_EAST, (-1, 0) },
                    { DirectionsEnum.DIRECTION_SOUTH_WEST, (0, 1) },
                    { DirectionsEnum.DIRECTION_NORTH_EAST, (0, -1) },
                    { DirectionsEnum.DIRECTION_NORTH_WEST, (1, 0) }
                };

                if (directionAdjustments.TryGetValue(Caster.Direction, out var adjustment))
                {
                    var (deltaX, deltaY) = adjustment;
                    var cellarbol = new MapPoint(targetPoint.X + deltaX, targetPoint.Y + deltaY);
                    var destarbol = Map.GetCell(cellarbol.CellId);

                    Caster.Direction = pushDirectiontarget;
                    Caster.Position.Cell = destarbol;

                    Fight.ForEach(entry => ActionsHandler.SendGameActionFightTeleportOnSameMapMessage(entry.Client, Caster, Caster, destarbol), true);
                }
            }
            else if (Spell.Id == (int)SpellIdEnum.FURIOUS_667) //Yokai Gelifox - Teleport Symetric
            {
                var directionAdjustments = new Dictionary<DirectionsEnum, (int, int)>
                {
                    { DirectionsEnum.DIRECTION_SOUTH_EAST, (-1, 0) },
                    { DirectionsEnum.DIRECTION_SOUTH_WEST, (0, 1) },
                    { DirectionsEnum.DIRECTION_NORTH_EAST, (0, -1) },
                    { DirectionsEnum.DIRECTION_NORTH_WEST, (1, 0) }
                };

                if (directionAdjustments.TryGetValue(Caster.Direction, out var adjustment))
                {
                    var (deltaX, deltaY) = adjustment;
                    var cellarbol = new MapPoint(targetPoint.X + deltaX, targetPoint.Y + deltaY);
                    var destarbol = Map.GetCell(cellarbol.CellId);

                    Caster.Direction = pushDirectiontarget;
                    Caster.Position.Cell = destarbol;

                    Fight.ForEach(entry => ActionsHandler.SendGameActionFightTeleportOnSameMapMessage(entry.Client, Caster, Caster, destarbol), true);
                }
            }
            else if (Spell.Id == (int)SpellIdEnum.ASSISTANCE_13878 || Spell.Id == (int)SpellIdEnum.REPOTTING_13581)
            {
                var directionAdjustments = new Dictionary<DirectionsEnum, (int, int)>
                {
                    { DirectionsEnum.DIRECTION_SOUTH_EAST, (-1, 0) },
                    { DirectionsEnum.DIRECTION_SOUTH_WEST, (0, 1) },
                    { DirectionsEnum.DIRECTION_NORTH_EAST, (0, -1) },
                    { DirectionsEnum.DIRECTION_NORTH_WEST, (1, 0) }
                };

                if (directionAdjustments.TryGetValue(Caster.Direction, out var adjustment))
                {
                    var (deltaX, deltaY) = adjustment;
                    var cellarbol = new MapPoint(targetPoint.X + deltaX, targetPoint.Y + deltaY);
                    var destarbol = Map.GetCell(cellarbol.CellId);

                    Caster.Direction = pushDirectiontarget;
                    Caster.Position.Cell = destarbol;

                    Fight.ForEach(entry => ActionsHandler.SendGameActionFightTeleportOnSameMapMessage(entry.Client, Caster, Caster, destarbol), true);
                }
            }
            else if (Spell.Id == (int)SpellIdEnum.LIGHT_SPEED_12724 && Caster.Direction == pushDirection)
            {
                var directionAdjustments = new Dictionary<DirectionsEnum, (int, int)>
                {
                    { DirectionsEnum.DIRECTION_SOUTH_EAST, (-1, 0) },
                    { DirectionsEnum.DIRECTION_SOUTH_WEST, (0, 1) },
                    { DirectionsEnum.DIRECTION_NORTH_EAST, (0, -1) },
                    { DirectionsEnum.DIRECTION_NORTH_WEST, (1, 0) }
                };

                if (directionAdjustments.TryGetValue(Caster.Direction, out var adjustment))
                {
                    var (deltaX, deltaY) = adjustment;
                    var cellarbol = new MapPoint(targetPoint.X + deltaX, targetPoint.Y + deltaY);
                    var destarbol = Map.GetCell(cellarbol.CellId);

                    Caster.Direction = pushDirection;
                    Caster.Position.Cell = destarbol;

                    Fight.ForEach(entry => ActionsHandler.SendGameActionFightTeleportOnSameMapMessage(entry.Client, Caster, Caster, destarbol), true);
                }
            }
            else
            {
                Caster.Position.Cell = TargetedCell;

                Fight.ForEach(entry => ActionsHandler.SendGameActionFightTeleportOnSameMapMessage(entry.Client, Caster, Caster, TargetedCell), true);
            }

            return true;
        }
    }
}