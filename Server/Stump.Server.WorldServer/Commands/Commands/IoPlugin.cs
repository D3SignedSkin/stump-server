using System.Drawing;
using System.Linq;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Messages;
using Stump.Server.BaseServer.Commands;
using Stump.Server.WorldServer.Commands.Trigger;
using Stump.Server.WorldServer.Game.Maps.Cells;

namespace Stump.Server.WorldServer.Commands.Commands
{
    public class IoPlugin : CommandBase
    {
        public IoPlugin()
        {
            Aliases = new[] { "iolist", };
            Description = "Display all io's";
            RequiredRole = RoleEnum.GameMaster;
            AddParameter<short>("distance", "dis", "the maximum distance to show", 63, true);
        }

        private static readonly Color[] Colors = { Color.Blue, Color.Chartreuse, Color.Chocolate, Color.Red, Color.Gold, Color.Purple, Color.Indigo, Color.Tan, Color.Azure, Color.Cyan, Color.DarkCyan, Color.DarkRed, Color.Firebrick, Color.Silver, Color.SkyBlue };

        public override void Execute(TriggerBase trigger)
        {
            if (trigger is GameTrigger)
            {
                var character = ((GameTrigger) trigger).Character;
                var ios = character.Map.Record.Elements.ToList();

                if (trigger.IsArgumentDefined("distance"))
                {
                    var maxDistance = trigger.Get<short>("distance");
                    ios = ios.Where(entry => MapPoint.GetPoint(entry.CellId).DistanceTo(MapPoint.GetPoint(character.Cell.Id)) <= maxDistance).ToList();
                }

                var i = 0;
                foreach (var io in ios)
                {
                    character.Client.Send(new DebugHighlightCellsMessage(Colors[i].ToArgb(), new[] { io.CellId }));
                    character.SendServerMessage($"GFX Id: {io.ElementId}", Colors[i]);
                    character.SendServerMessage($"Element Id: {io.ElementId}", Colors[i]);
                    i++;
                }
            }
            else
            {
                trigger.Reply("this command can only be executed in game");
            }
        }
    }
}
