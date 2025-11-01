using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Commands.Commands.Patterns;
using Stump.Server.WorldServer.Commands.Trigger;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using System;

namespace Stump.Server.WorldServer.Commands.Commands.GameMaster
{
    class Salvarchar : InGameCommand
    {
      //  private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        public Salvarchar()
        {
            Aliases = new string[]
            {
               "save"
            };
            RequiredRole = RoleEnum.Player;
            Description = "Salvar o Jogador";
            Description_en = "Save the Player";
            Description_es = "Guarda el jugador";
            Description_fr = "Sauver le joueur";
        }
       
        public override void Execute(GameTrigger trigger)
        {
            Character player = trigger.Character;
            int variavel;
            TimeSpan teste = new TimeSpan(0, 0, 0);
            TimeSpan tempo = new TimeSpan(0, 0, 10);
            if (player.last != teste)
            {
                variavel = 1;
            }
            else
            {
                variavel = 2;
            }

            string value = trigger.Args.PeekNextWord();
            if (string.IsNullOrEmpty(value))
            {

                if (variavel == 1)
                {
                    if ((DateTime.Now.TimeOfDay - player.last).TotalSeconds > (double)tempo.TotalSeconds)
                    {
                        // logger.Info("save from in queue {0}", "Salvarchar1");
                        player.SaveLater();
                        switch (trigger.Character.Account.Lang)
                        {
                            case "fr":
                                trigger.Reply("Joueur sauvé !!!");
                                break;
                            case "es":
                                trigger.Reply("Jugador guardado !!!");
                                break;
                            case "en":
                                trigger.Reply("Player saved !!!");
                                break;
                            default:
                                trigger.Reply("Jogador salvo !!!");
                                break;
                        }

                        player.last = DateTime.Now.TimeOfDay;
                    }
                    else
                    {
                        switch (trigger.Character.Account.Lang)
                        {
                            case "fr":
                                throw new System.Exception("Il faut attendre " + tempo.TotalSeconds.ToString("#0") + " secondes pour courir à nouveau !!!\n" + (tempo - (DateTime.Now.TimeOfDay - player.last)).TotalSeconds.ToString("#0") + " secondes restantes.");
                                break;
                            case "es":
                                throw new System.Exception("Usted debe esperar " + tempo.TotalSeconds.ToString("#0") + " segundos para ejecutar de nuevo !!!\n" + (tempo - (DateTime.Now.TimeOfDay - player.last)).TotalSeconds.ToString("#0") + " segundos restantes.");
                                break;
                            case "en":
                                throw new System.Exception("VYou must wait " + tempo.TotalSeconds.ToString("#0") + " seconds to run again !!!\n" + (tempo - (DateTime.Now.TimeOfDay - player.last)).TotalSeconds.ToString("#0") + " seconds remaining.");
                                break;
                            default:
                                throw new System.Exception("Você deve esperar " + tempo.TotalSeconds.ToString("#0") + " segundos para executar de novo !!!\n" + (tempo - (DateTime.Now.TimeOfDay - player.last)).TotalSeconds.ToString("#0") + " segundos restantes.");
                                break;
                        }

                    }
                }
                else
                {
                    //logger.Info("save from in queue {0}", "Salvarchar2");
                    player.SaveLater();
                    switch (trigger.Character.Account.Lang)
                    {
                        case "fr":
                            trigger.Reply("Joueur sauvé !!!");
                            break;
                        case "es":
                            trigger.Reply("Jugador guardado !!!");
                            break;
                        case "en":
                            trigger.Reply("Player saved !!!");
                            break;
                        default:
                            trigger.Reply("Jogador salvo !!!");
                            break;
                    }

                    player.last = DateTime.Now.TimeOfDay;
                }
            }
        }
    }
}
