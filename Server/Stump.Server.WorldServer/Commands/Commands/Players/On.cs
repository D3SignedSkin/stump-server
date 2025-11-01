using NLog;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Commands.Commands.Patterns;
using Stump.Server.WorldServer.Commands.Trigger;


namespace Stump.Server.WorldServer.Commands.Commands.GameMaster
{
    class On : InGameCommand
    {
        public readonly Logger logger = LogManager.GetCurrentClassLogger();
        public On()
        {
            Aliases = new string[]
            {
                "on"
            };
            RequiredRole = RoleEnum.Player;
            Description = "Exibe informações a respeito do servidor!";
            Description_en = "Displays information about the server!";
            Description_es = "Muestra información acerca del servidor!";
            Description_fr = "Affiche des informations sur le serveur!";

        }
        public override void Execute(GameTrigger trigger)
        {
            if (trigger.Character.UserGroup.Role >= RoleEnum.Moderator_Helper)
            {
                switch (trigger.Character.Account.Lang)
                {
                    case "fr":
                        trigger.Reply("Temps : " + trigger.Bold("{0}") + " Joueurs en Ligne : " + trigger.Bold("{1}") + " Record en Ligne: " + trigger.Bold("{2}"), WorldServer.Instance.DownTime.ToString(@"hh\:mm\:ss"), WorldServer.Instance.ClientManager.Count, Game.Misc.ServerInfoManager.Instance.GetRecord());
                        break;
                    case "es":
                        trigger.Reply("Tiempo : " + trigger.Bold("{0}") + " Jugadores Online : " + trigger.Bold("{1}") + " Record Online: " + trigger.Bold("{2}"), WorldServer.Instance.DownTime.ToString(@"hh\:mm\:ss"), WorldServer.Instance.ClientManager.Count, Game.Misc.ServerInfoManager.Instance.GetRecord());
                        break;
                    case "en":
                        trigger.Reply("Time : " + trigger.Bold("{0}") + " Players Online : " + trigger.Bold("{1}") + " Record Online: " + trigger.Bold("{2}"), WorldServer.Instance.DownTime.ToString(@"hh\:mm\:ss"), WorldServer.Instance.ClientManager.Count, Game.Misc.ServerInfoManager.Instance.GetRecord());
                        break;
                    default:
                        trigger.Reply("Tempo : " + trigger.Bold("{0}") + " Jogadores Online : " + trigger.Bold("{1}") + " Record Online: " + trigger.Bold("{2}"), WorldServer.Instance.DownTime.ToString(@"hh\:mm\:ss"), WorldServer.Instance.ClientManager.Count, Game.Misc.ServerInfoManager.Instance.GetRecord());
                        break;
                }
            }
            else {

                switch (trigger.Character.Account.Lang)
                {
                    case "fr":
                        trigger.Reply("Temps : " + trigger.Bold("{0}"), WorldServer.Instance.DownTime.ToString(@"hh\:mm\:ss"), WorldServer.Instance.ClientManager.Count);
                        break;
                    case "es":
                        trigger.Reply("Temps : " + trigger.Bold("{0}"), WorldServer.Instance.DownTime.ToString(@"hh\:mm\:ss"), WorldServer.Instance.ClientManager.Count);
                        break;
                    case "en":
                        trigger.Reply("Time : " + trigger.Bold("{0}"), WorldServer.Instance.DownTime.ToString(@"hh\:mm\:ss"), WorldServer.Instance.ClientManager.Count);
                        break;
                    default:
                        trigger.Reply("Tempo : " + trigger.Bold("{0}"), WorldServer.Instance.DownTime.ToString(@"hh\:mm\:ss"), WorldServer.Instance.ClientManager.Count);
                        break;
                }
            }
        }
    }
}
