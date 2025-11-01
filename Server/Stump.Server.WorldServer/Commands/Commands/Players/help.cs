using System.Collections.Generic;
using System.Linq;
using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Commands;
using Stump.Server.WorldServer.Commands.Commands.Patterns;
using Stump.Server.WorldServer.Commands.Trigger;

namespace Stump.Server.WorldServer.Commands.Commands.Players

{
    public class AideCommand : InGameCommand
    {
        public AideCommand()
        {
            Aliases = new[] { "ajuda","help","ayuda","aide","?" };
            RequiredRole = RoleEnum.Player;
            Description = "Listar todos os comandos disponíveis em português";
            Description_en = "List all available commands in english";
            Description_es = "Listar todos los comandos disponibles en español";
            Description_fr = "Liste toutes les commandes disponibles en français";
            Parameters = new List<IParameterDefinition>
                             {
                                 new ParameterDefinition<string>("comando", "cmd", "Exibe a ajuda completa de um comando", string.Empty),
                                 new ParameterDefinition<string>("subcomando", "subcmd", "Exibe a ajuda completa de um subcomando", string.Empty),
                             };
            Parameters_en = new List<IParameterDefinition>
                             {
                                 new ParameterDefinition<string>("command", "cmd", "Display the complete help of a command", string.Empty),
                                 new ParameterDefinition<string>("subcommand", "subcmd", "Display the complete help of a subcommand", string.Empty),
                             };
            Parameters_es = new List<IParameterDefinition>
                             {
                                 new ParameterDefinition<string>("comando", "cmd", "Muestra la ayuda completa de un comando", string.Empty),
                                 new ParameterDefinition<string>("subcomando", "subcmd", "Muestra la ayuda completa de un subcomando", string.Empty),
                             };
            Parameters_fr = new List<IParameterDefinition>
                             {
                                 new ParameterDefinition<string>("commande", "cmd", "Afficher l'aide complète d'une commande", string.Empty),
                                 new ParameterDefinition<string>("sous-commande", "souscmd", "Afficher l'aide complète d'une sous-commande", string.Empty),
                             };
            /*
             *       Description = "Listar todos os comandos disponíveis em português";
            Description_en = "List all available commands in portuguese";
            Description_es = "Listar todos los comandos disponibles en portugués";
            Description_fr = "Liste toutes les commandes disponibles en portugais";
            Parameters = new List<IParameterDefinition>
                             {
                                 new ParameterDefinition<string>("command", "cmd", "Exibe a ajuda completa de um comando", string.Empty),
                                 new ParameterDefinition<string>("subcommand", "subcmd", "Exibe a ajuda completa de um subcomando", string.Empty),
                             };
             * 
             * 
             * 
             * 
             * 
             * 
             * Description = "Listar todos os comandos disponíveis em inglês";
            Description_en = "List all available commands in english";
            Description_es = "Listar todos los comandos disponibles en inglés";
            Description_fr = "Liste toutes les commandes disponibles en anglais";
            Parameters = new List<IParameterDefinition>
                             {
                                 new ParameterDefinition<string>("command", "cmd", "Display the complete help of a command", string.Empty),
                                 new ParameterDefinition<string>("subcommand", "subcmd", "Display the complete help of a subcommand", string.Empty),
                             }; 
             * 
             * 
             * 
             *
             RequiredRole = RoleEnum.Player;
            Description = "Listar todos os comandos disponíveis em francês";
            Description_en = "List all available commands in french";
            Description_es = "Listar todos los comandos disponibles en francés";
            Description_fr = "Liste toutes les commandes disponibles en français";
            Parameters = new List<IParameterDefinition>
                             {
                                 new ParameterDefinition<string>("command", "cmd", "Afficher l'aide complète d'une commande", string.Empty),
                                 new ParameterDefinition<string>("subcommand", "subcmd", "Affiche l'aide complète d'une sous-commande", string.Empty),
                             };    



            Description = "Listar todos os comandos disponíveis em espanhol";
            Description_en = "List all available commands in spanish";
            Description_es = "Listar todos los comandos disponibles en español";
            Description_fr = "Liste toutes les commandes disponibles en espagnol";
            Parameters = new List<IParameterDefinition>
                             {
                                 new ParameterDefinition<string>("command", "cmd", "Muestra la ayuda completa de un comando", string.Empty),
                                 new ParameterDefinition<string>("subcommand", "subcmd", "Muestra la ayuda completa de un subcomando", string.Empty),
                             };
             
             */
        }
        public static string Commandlang(GameTrigger trigger, CommandBase command)
        {
            switch (trigger.Character.Account.Lang)
            {
                case "fr":
                    return command.Description_fr;
                    break;
                case "es":
                    return command.Description_es;
                    break;
                case "en":
                    return command.Description_en;
                    break;
                default:
                    return command.Description;
                    break;
            }
         
        }
        public static string SubCommandlang(GameTrigger trigger, SubCommand subcommand)
        {
            switch (trigger.Character.Account.Lang)
            {
                case "fr":
                    return subcommand.Description_fr;
                    break;
                case "es":
                    return subcommand.Description_es;
                    break;
                case "en":
                    return subcommand.Description_en;
                    break;
                default:
                    return subcommand.Description;
                    break;
            }

        }
       
        public override void Execute(GameTrigger trigger)
        {
            
            var cmdStr = trigger.Get<string>("comando");
            var subcmdStr = trigger.Get<string>("subcmd");
            var gameTrigger = trigger as GameTrigger;
            if (gameTrigger != null)
            {
                if (cmdStr == string.Empty)
                {
                    foreach (var command in CommandManager.Instance.AvailableCommands.Where(command => !(command is SubCommand)).Where(trigger.CanAccessCommand))
                    {
                        DisplayCommandDescription(trigger, command);
                    }
                }
                else
                {
                    CommandBase command = CommandManager.Instance.GetCommand(cmdStr);

                    if (command == null || !trigger.CanAccessCommand(command))
                    {
                        switch (trigger.Character.Account.Lang)
                        {
                            case "fr":
                                trigger.Reply("Commande '{0}' n'existe pas", cmdStr);
                                break;
                            case "es":
                                trigger.Reply("Comando '{0}' no existe", cmdStr);
                                break;
                            case "en":
                                trigger.Reply("Command '{0}' doesn't exist", cmdStr);
                                break;
                            default:
                                trigger.Reply("Comando '{0}' não existe", cmdStr);
                                break;
                        }
                      
                        return;
                    }

                    if (subcmdStr == string.Empty)
                    {
                        DisplayFullCommandDescription(trigger, command);
                    }
                    else
                    {
                        if (!(command is SubCommandContainer))
                        {
                            switch (trigger.Character.Account.Lang)
                            {
                                case "fr":
                                    trigger.Reply("Commande '{0}' n'a pas de sous-commandes", cmdStr);
                                    break;
                                case "es":
                                    trigger.Reply("Comando '{0}' no tiene subcomandos", cmdStr);
                                    break;
                                case "en":
                                    trigger.Reply("Command '{0}' has no sub commands", cmdStr);
                                    break;
                                default:
                                    trigger.Reply("Comando '{0}' não possui subcomando", cmdStr);
                                    break;
                            }
                           
                            return;
                        }

                        var subcommand = (command as SubCommandContainer)[subcmdStr];

                        if (subcommand == null || subcommand.RequiredRole > trigger.UserRole)
                        {
                            switch (trigger.Character.Account.Lang)
                            {
                                case "fr":
                                    trigger.Reply("Commande '{0} {1}' n'existe pas", cmdStr, subcmdStr);
                                    break;
                                case "es":
                                    trigger.Reply("Comando '{0} {1}' no existe", cmdStr, subcmdStr);
                                    break;
                                case "en":
                                    trigger.Reply("Command '{0} {1}' doesn't exist", cmdStr, subcmdStr);
                                    break;
                                default:
                                    trigger.Reply("Comando '{0} {1}' não existe", cmdStr, subcmdStr);
                                    break;
                            }
                      
                            return;
                        }

                        DisplayFullSubCommandDescription(trigger, command, subcommand);
                    }
                }
            }
        }

        public static void DisplayCommandDescription(GameTrigger trigger, CommandBase command )
        {
            trigger.Reply(trigger.Bold("{0}") + "{1} - {2}",
                          string.Join("/", command.Aliases),
                          command is SubCommandContainer
                              ? string.Format(" ({0} subcmds)", (command as SubCommandContainer).Count(entry => entry.RequiredRole <= trigger.UserRole))
                              : "",
                          Commandlang(trigger,command) );
        }

        public static void DisplaySubCommandDescription(GameTrigger trigger, CommandBase command, SubCommand subcommand)
        {
            trigger.Reply(trigger.Bold("{0}") + " {1} - {2}",
                          command.Aliases.First(),
                          string.Join("/", subcommand.Aliases),
                           SubCommandlang(trigger, subcommand) );
        }


        public static void DisplayFullCommandDescription(GameTrigger trigger, CommandBase command)
        {

            trigger.Reply(trigger.Bold("{0}") + "{1} - {2}",
                          string.Join("/", command.Aliases),
                          command is SubCommandContainer && (command as SubCommandContainer).Count > 0
                              ? string.Format(" ({0} subcmds)", (command as SubCommandContainer).Count(trigger.CanAccessCommand))
                              : "",
                           Commandlang(trigger, command));

            if (!(command is SubCommandContainer))            
            switch (trigger.Character.Account.Lang)
            {
                case "fr":
                        trigger.Reply("  -> " + command.Aliases.First() + " " + command.GetSafeUsage_fr());
                        break;
                case "es":
                        trigger.Reply("  -> " + command.Aliases.First() + " " + command.GetSafeUsage_es());
                        break;
                case "en":
                        trigger.Reply("  -> " + command.Aliases.First() + " " + command.GetSafeUsage_en());
                        break;
                default:
                        trigger.Reply("  -> " + command.Aliases.First() + " " + command.GetSafeUsage());
                        break;
            }

            switch (trigger.Character.Account.Lang)
            {
                case "fr":
                    if (command.Parameters_fr != null)
                        foreach (var commandParameter in command.Parameters_fr)
                        {
                            DisplayCommandParameter(trigger, commandParameter);
                        }
                    break;
                case "es":
                    if (command.Parameters_es != null)
                        foreach (var commandParameter in command.Parameters_es)
                        {
                            DisplayCommandParameter(trigger, commandParameter);
                        }
                    break;
                case "en":
                    if (command.Parameters_en != null)
                        foreach (var commandParameter in command.Parameters_en)
                        {
                            DisplayCommandParameter(trigger, commandParameter);
                        }
                    break;
                default:
                    if (command.Parameters != null)
                        foreach (var commandParameter in command.Parameters)
                        {
                            DisplayCommandParameter(trigger, commandParameter);
                        }
                    break;
            }
          

            if (!(command is SubCommandContainer))
                return;

            foreach (var subCommand in command as SubCommandContainer)
            {
                if (trigger.CanAccessCommand(subCommand))
                    DisplayFullSubCommandDescription(trigger, command, subCommand);
            }
        }

        public static void DisplayFullSubCommandDescription(GameTrigger trigger, CommandBase command,
                                                             SubCommand subcommand)
        {
            trigger.Reply(trigger.Bold("{0} {1}") + " - {2}",
                          command.Aliases.First(),
                          string.Join("/", subcommand.Aliases),
                          SubCommandlang(trigger, subcommand));          
            switch (trigger.Character.Account.Lang)
            {
                case "fr":
                    trigger.Reply("  -> " + command.Aliases.First() + " " + subcommand.Aliases.First() + " " + subcommand.GetSafeUsage_fr());
                    break;
                case "es":
                    trigger.Reply("  -> " + command.Aliases.First() + " " + subcommand.Aliases.First() + " " + subcommand.GetSafeUsage_es());
                    break;
                case "en":
                    trigger.Reply("  -> " + command.Aliases.First() + " " + subcommand.Aliases.First() + " " + subcommand.GetSafeUsage_en());
                    break;
                default:
                    trigger.Reply("  -> " + command.Aliases.First() + " " + subcommand.Aliases.First() + " " + subcommand.GetSafeUsage());
                    break;
            }


            switch (trigger.Character.Account.Lang)
            {
                case "fr":
                    foreach (var commandParameter in subcommand.Parameters_fr)
                    {
                        DisplayCommandParameter(trigger, commandParameter);
                    }
                    break;
                case "es":
                    foreach (var commandParameter in subcommand.Parameters_es)
                    {
                        DisplayCommandParameter(trigger, commandParameter);
                    }
                    break;
                case "en":
                    foreach (var commandParameter in subcommand.Parameters_en)
                    {
                        DisplayCommandParameter(trigger, commandParameter);
                    }
                    break;
                default:
                    foreach (var commandParameter in subcommand.Parameters)
                    {
                        DisplayCommandParameter(trigger, commandParameter);
                    }
                    break;
            }
        }

        public static void DisplayCommandParameter(GameTrigger trigger, IParameterDefinition parameter)
        {
            trigger.Reply("\t(" + trigger.Bold("{0}") + " : {1})",
                          parameter.GetUsage(),
                          parameter.Description ?? "");
        }
    }
}