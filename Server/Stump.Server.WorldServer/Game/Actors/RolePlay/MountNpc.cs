using Stump.DofusProtocol.Types;
using Stump.Server.WorldServer.Database.Mounts;
using Stump.Server.WorldServer.Game.Actors.Interfaces;
using Stump.Server.WorldServer.Game.Actors.Look;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Mounts;
using Stump.Server.WorldServer.Game.Maps;
using Stump.Server.WorldServer.Game.Maps.Pathfinding;
using Stump.Server.WorldServer.Handlers.Context;
using System;

namespace Stump.Server.WorldServer.Game.Actors.RolePlay
{
    public class MountNpc : NamedActor, IContextDependant, IAutoMovedEntity
    {
        private ActorLook m_look;
        readonly int m_contextId;
        readonly MountRecord m_record;

        public override int Id => m_contextId;

        public Mount Mount
        {
            get;
            protected set;
        }

        public override ActorLook Look
        {
            get { return m_look; }
            set
            {
                m_look = value;
            }
        }

        #region >> Movement

        public DateTime NextMoveDate { get; set; }

        public DateTime LastMoveDate { get; private set; }

        public override bool StartMove(Path movementPath)
        {
            if (!CanMove() || movementPath.IsEmpty())
                return false;

            Position = movementPath.EndPathPosition;
            var keys = movementPath.GetServerPathKeys();

            Mount.PaddockCellId = movementPath.EndPathPosition.Cell.Id;
            Mount.PaddockMountDirection = (int)movementPath.EndPathPosition.Direction;
            Mount.IsUpdate = true;

            Map.ForEach(entry => ContextHandler.SendGameMapMovementMessage(entry.Client, keys, this));

            StopMove();
            LastMoveDate = DateTime.Now;

            return true;
        }
        #endregion

        public override GameContextActorInformations GetGameContextActorInformations(Character character)
        {
            return new GameRolePlayMountInformations(
               contextualId: Id,
               look: Look.GetEntityLook(),
               disposition: GetEntityDispositionInformations(),
               name: Mount.OwnerName,
               ownerName: Mount.Name,
               level: (byte)Mount.Level);
        }

        protected override void OnDisposed()
        {
            base.Dispose();
            base.OnDisposed();
        }
    }
}