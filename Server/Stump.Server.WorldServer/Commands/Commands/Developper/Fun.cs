using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Messages;
using Stump.Server.BaseServer.Network;
using Stump.Server.WorldServer.Commands.Commands.Patterns;
using Stump.Server.WorldServer.Commands.Trigger;
using Stump.Server.WorldServer.Game.Actors.Look;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Items;
using Stump.Server.WorldServer.Game.Items.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stump.Server.WorldServer.Commands.Commands.GameMaster
{           //Fun
    class Emotemapa : InGameCommand
    {
        public Emotemapa()
        {
            Aliases = new string[] { "emotemapa"};
            RequiredRole = RoleEnum.Developer;
            AddParameter<int>("id", "id", "Emote id", isOptional: false);
            Description = "Distribua emote para todo mapa!";

        }
        public override void Execute(GameTrigger trigger)
        {
            if (!trigger.IsArgumentDefined("id")){
                trigger.ReplyError("Specify an emote or -all");
                return;
            }
            var clients = trigger.Character.Map.GetAllCharacters().Where(x => Game.HavenBags.HavenBagManager.Instance.CanBeSeenInHavenBag(x, trigger.Character)).Select(v => v.Client);
            foreach (var target in clients)
            {
                target.Character.AddEmote((EmotesEnum)trigger.Get<int>("id"));

            }
        }
    }

    class Gfx : InGameCommand
    {
        public Gfx()
        {
            Aliases = new string[]
            {
                "gfx"
            };
            RequiredRole = RoleEnum.Developer;
            AddParameter<string>("gfx", "gfx", "GFX", isOptional: false);
            Description = "test gfx!";

        }
        public override void Execute(GameTrigger trigger)
        {
            if (!trigger.IsArgumentDefined("gfx"))
            {
                trigger.ReplyError("Specify an gfx or -all");
                return;
            }
            trigger.Character.Client.Send(new GameRolePlaySpellAnimMessage((ulong)trigger.Character.Id, (ushort)trigger.Character.Record.CellId, 0, short.Parse(trigger.Get<string>("gfx")), 6));
            
        }
    }
    class Walkable  : InGameCommand
    {
        public Walkable()
        {
            Aliases = new string[]
            {
                "Walkable"
            };
            RequiredRole = RoleEnum.Developer;
            //AddParameter<string>("gfx", "gfx", "GFX", isOptional: false);
            Description = "Walkable";

        }
        public override void Execute(GameTrigger trigger)
        {
            trigger.Character.Client.Send(new DebugHighlightCellsMessage(System.Drawing.Color.BurlyWood.ToArgb(), trigger.Character.Client.Character.Map.GetWalkableCell()));

        }
    }
    class Disobs : InGameCommand
    {
        public Disobs()
        {
            Aliases = new string[]
            {
                "disobs"
            };
            RequiredRole = RoleEnum.Developer;
           // AddParameter<string>("gfx", "gfx", "GFX", isOptional: false);
            Description = "disobs";

        }
        public override void Execute(GameTrigger trigger)
        {
         var obstacles = new List<DofusProtocol.Types.MapObstacle>();
            for (short i = 0; i < 560; i++)
            {
                obstacles.Add(new DofusProtocol.Types.MapObstacle((ushort)i, 1));
            }
            trigger.Character.Client.Send(new MapObstacleUpdateMessage(obstacles));
        }
    }
    class Emoteplaymapa : InGameCommand
    {
        public Emoteplaymapa()
        {
            Aliases = new string[] {"emoteplaymapa"};
            RequiredRole = RoleEnum.Developer;
            AddParameter<int>("id", "id", "Emote id", isOptional: false);
            Description = "Força todos do mapa fazer esse emote!";
        }
        public override void Execute(GameTrigger trigger)
        {
            if (!trigger.IsArgumentDefined("id"))
            {
                trigger.ReplyError("Specify an emote or -all");
                return;
            }
            var clients = trigger.Character.Map.GetAllCharacters().Where(x => Game.HavenBags.HavenBagManager.Instance.CanBeSeenInHavenBag(x, trigger.Character)).Select(v => v.Client);
            foreach (var target in clients)
            {
                target.Character.PlayEmote((EmotesEnum)trigger.Get<int>("id"),true);               

            }
        }
    }
    class Mapasay : InGameCommand
    {
        public Mapasay()
        {
            Aliases = new string[] { "mapasay" };
            RequiredRole = RoleEnum.Developer;      
            base.AddParameter<string>("Texto", "txt", "Texto que irá passar para os players!", null, true, null);
            Description = "Força todos do mapa a falar o texto!";

        }
        public override void Execute(GameTrigger trigger)
        {
            if (!trigger.IsArgumentDefined("txt"))
            {
                trigger.ReplyError("Specify an text");
                return;
            }
            var clients = trigger.Character.Map.GetAllCharacters().Where(x => Game.HavenBags.HavenBagManager.Instance.CanBeSeenInHavenBag(x, trigger.Character)).Select(v => v.Client);
            foreach (var target in clients)
            {
                target.Character.Say(trigger.Get<string>("txt"));

            }
        }
    }
}