using Stump.Core.Attributes;
using Stump.Core.Reflection;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Messages;
using Stump.Server.BaseServer.Commands;
using Stump.Server.WorldServer.Commands.Trigger;
using Stump.Server.WorldServer.Game;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using System;
using System.Drawing;
namespace Stump.Server.WorldServer.Commands.Commands.GameMaster
{
    public class PopupAnnounce : CommandBase
    {
        [Variable(true)]
        public static string AnnounceColor = ColorTranslator.ToHtml(Color.Red);
        public PopupAnnounce()
        {
            base.Aliases = new string[]
            {
                "popup",
                "pop"
            };
            base.Description = "Exibir um anúncio para todos os jogadores";
            base.RequiredRole = RoleEnum.Administrator;
            base.AddParameter<string>("message", "msg", "The announce", null, false, null);
        }
        public override void Execute(TriggerBase trigger)
        {
            System.Predicate<Character> predicate = (Character x) => true;
            Color color = ColorTranslator.FromHtml(Moderator_Helper.AnnounceCommand.AnnounceColor);
            string text = trigger.Get<string>("msg");
            System.Collections.Generic.IEnumerable<Character> characters = Singleton<World>.Instance.GetCharacters(predicate);
            int num = 0;
            foreach (Character current in characters)
            {
                current.Client.Send(new PopupWarningMessage(0, "Staff", text)); //agregar nombre de servidor by charly
                num++;

            }
            trigger.Reply("Sent to " + num + " joueurs");
        }
    }
}
