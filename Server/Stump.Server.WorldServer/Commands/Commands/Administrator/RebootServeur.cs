using MongoDB.Bson;
using NLog;
using Stump.Core.Reflection;
using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer;
using Stump.Server.BaseServer.Commands;
using Stump.Server.BaseServer.Logging;
using Stump.Server.WorldServer.Commands.Commands.Patterns;
using Stump.Server.WorldServer.Commands.Trigger;
using Stump.Server.WorldServer.Core.Network;
using Stump.Server.WorldServer.Game;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using System;
using System.Globalization;
using System.Threading;
namespace Stump.Server.WorldServer.Commands.Commands.GameMaster
{
    public class TimeREboot : TargetCommand
    {
      //  private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        public TimeREboot()
        {
            base.Aliases = new string[]
            {
                "reboot"
            };
            base.Description = "Save the player";
            base.RequiredRole = RoleEnum.Administrator;
            base.AddParameter<int>("time", "time", "Noire duration (in minutes)", 3, false, null);

        }
        public override void Execute(TriggerBase trigger)
        {
            var source = trigger.GetSource() as WorldClient;
            string value = trigger.Args.PeekNextWord();

            if (string.IsNullOrEmpty(value))
            {
                if (trigger is GameTrigger)
                {
                    int time = trigger.Get<int>("time");

                    Singleton<World>.Instance.SendAnnounceLang("Reinicialização automática do servidor em "+ time + " minutos.", "Automatic reboot of the server in " + time + " minutes.", "Reinicialización automático del servidor en " + time + " minutos.", "Réinitialisation automatique du serveur dans " + time + " minutes.", System.Drawing.Color.Orchid);
                    Thread Restart =
                        new Thread(
                          unused => SaveMinuteBefore(time * 60000)
                        );
                    Restart.Start();

                    #region // ----------------- Sistema de Logs MongoDB Reboot by: Kenshin ---------------- //
                    try
                    {
                        var document = new BsonDocument
                                {
                                    { "Staff_IP", source.IP },
                                    { "StaffName", source.Character.Name },
                                    { "Time_Reboot",  time + "Minutos" },
                                    { "Date", DateTime.Now.ToString(CultureInfo.CreateSpecificCulture("pt-BR")) }
                                };

                        MongoLogger.Instance.Insert("Staff_ManagementReboot", document);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Erro no Mongologs Jail : " + e.Message);
                    }
                    #endregion
                }
            }
            else
            {

            }
        }

        public void SaveMinuteBefore(int time)
        {
            
            if(time - 60000>=0)
                Thread.Sleep(time - 60000);
            System.Predicate<Character> predicate = (Character x) => true;
            System.Collections.Generic.IEnumerable<Character> characters = Singleton<World>.Instance.GetCharacters(predicate);
            Singleton<World>.Instance.SendAnnounceLang("Reinicialização automática do servidor em 1 minuto.", "Automatic reboot of the server in 1 minute.", "Reinicialización automático del servidor en 1 minuto.", "Réinitialisation automatique du serveur dans 1 minute.", System.Drawing.Color.Orchid);
            Thread.Sleep(60000);
            foreach (Character cr in characters)
            {
                
           //  logger.Info("save from in queue {0}", "TimeREboot");
                cr.SaveLater();
                
                switch (cr.Account.Lang)
                {
                    case "fr":
                        cr.SendServerMessage("Votre personnage à était sauvegardé.");
                        break;
                    case "es":
                        cr.SendServerMessage("Su carácter se salvó.");
                        break;
                    case "en":
                        cr.SendServerMessage("Your character was saved.");
                        break;
                    default:
                        cr.SendServerMessage("Seu personagem foi salvo.");
                        break;
                }
            }
            ServerBase<WorldServer>.Instance.IOTaskPool.AddMessage(new Action(Singleton<World>.Instance.Save));
            Singleton<World>.Instance.SendAnnounceLang("Reinicialização !!!", "Reboot !!!", "¡Reinicialización !!!", "Réinitialisation !!!", System.Drawing.Color.Orchid);
            Singleton<World>.Instance.SendAnnounceLang("Reinicialização !!!", "Reboot !!!", "¡Reinicialización !!!", "Réinitialisation !!!", System.Drawing.Color.Orchid);
            Singleton<World>.Instance.SendAnnounceLang("Reinicialização !!!", "Reboot !!!", "¡Reinicialización !!!", "Réinitialisation !!!", System.Drawing.Color.Orchid);
            Thread.Sleep(5000);
            Environment.Exit(1);
        }
    }
}
