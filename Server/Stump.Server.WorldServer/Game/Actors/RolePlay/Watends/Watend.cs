using Stump.Core.Cache;
using Stump.DofusProtocol.Types;
using Stump.Server.WorldServer.Database.Npcs;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Actors.Look;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Maps.Cells;
using System;
using System.Linq;

namespace Stump.Server.WorldServer.Game.Actors.RolePlay.Npcs
{
    public sealed class Watend : RolePlayActor
    {
        public WatendSpawn Record { get; }

        public Watend(int id, NpcSpawn spawn, WatendSpawn watend) : this(id, spawn.Template, spawn.GetPosition(), spawn.Look, watend)
        {
            Spawn = spawn;
        }

        public Watend(int id, NpcTemplate template, ObjectPosition position, ActorLook look, WatendSpawn watend)
        {
            Id = id;
            Template = template;
            Position = position;
            Record = watend;
            Look = look;

            m_gameContextActorInformations = new ObjectValidator<GameContextActorInformations>(BuildGameContextActorInformations);
        }

        public override bool CanBeSee(Maps.WorldObject byObj)
        {
            Character character = byObj as Character;

            //Boolean hasWatendComplete = true;
            Boolean hasWatendComplete = character.Quests.Any(x => x.CurrentStep.Objectives.Any(y => y.Template.Parameter0 == this.Record.MonsterId && x.CurrentStep.Quest.Finished));

            return base.CanBeSee(byObj) && hasWatendComplete;
        }

        public int TemplateId => Template.Id;

        public NpcTemplate Template
        {
            get;
        }

        public NpcSpawn Spawn
        {
            get;
        }

        public override int Id
        {
            get;
            protected set;
        }

        public override ActorLook Look
        {
            get;
            set;
        }

        public void Refresh()
        {
            m_gameContextActorInformations.Invalidate();

            if (Map != null)
                Map.Refresh(this);
        }

        public override string ToString() => string.Format("{0} ({1}) [{2}]", Template.Name, Id, TemplateId);

        #region GameContextActorInformations
        private readonly ObjectValidator<GameContextActorInformations> m_gameContextActorInformations;

        private GameContextActorInformations BuildGameContextActorInformations()
        {
            return new GameRolePlayNpcInformations(
                Id,
                GetEntityDispositionInformations(),
                Look.GetEntityLook(),
                (ushort)Template.Id,
                false,
                0);
        }
        #endregion
    }
}