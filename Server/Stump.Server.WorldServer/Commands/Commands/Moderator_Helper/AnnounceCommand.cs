using System.Drawing;
using Stump.Core.Attributes;
using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Commands;
using Stump.Server.WorldServer.Commands.Trigger;
using Stump.Server.WorldServer.Game;

namespace Stump.Server.WorldServer.Commands.Commands.Moderator_Helper
{
    public class AnnounceCommand : CommandBase
    {
        [Variable(true)]
        public static string AnnounceColor = ColorTranslator.ToHtml(Color.DarkOrange);

        public AnnounceCommand()
        {
            Aliases = new[] { "announce", "a" };
            Description = "Enviar um anúncio para todos os jogadores online.";
            Description_es = "Envíe un anuncio a todos los jugadores en línea.";
            Description_en = "Send an announcement to all online players.";
            Description_fr = "Envoyez une annonce à tous les joueurs en ligne.";
            RequiredRole = RoleEnum.Moderator_Helper;
            AddParameter<string>("message", "msg", "The announce");
        }

        public override void Execute(TriggerBase trigger)
        {
            var character = (trigger as GameTrigger).Character;

            var color = ColorTranslator.FromHtml(AnnounceColor);
            var msg = trigger.Get<string>("msg");
            var formatMsg = trigger is GameTrigger
                                ? string.Format("★{0} : {1}", ((GameTrigger)trigger).Character.Name, msg)
                                : string.Format("★{0} : ", msg);

            World.Instance.SendAnnounce(formatMsg, color);
        }
    }
}