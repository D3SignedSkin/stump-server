using System.Collections.Generic;
using System.Linq;
using Stump.DofusProtocol.Enums;

namespace Stump.Server.BaseServer.Commands.Commands
{
    public class HelpCommand : CommandBase
    {
        public HelpCommand()
        {
            Aliases = new[] {"??"};
            RequiredRole = RoleEnum.Administrator;
            Description = "Listar todos os comandos disponíveis";
            Parameters = new List<IParameterDefinition>
                             {
                                 new ParameterDefinition<string>("command", "cmd", "Exibe a ajuda completa de um comando", string.Empty),
                                 new ParameterDefinition<string>("subcommand", "subcmd", "Exibe a ajuda completa de um subcomando", string.Empty),
                             };
        }

        public override void Execute(TriggerBase trigger)
        {
            var cmdStr = trigger.Get<string>("command");
            var subcmdStr = trigger.Get<string>("subcmd");

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
                    trigger.Reply("Comando '{0}' não existe", cmdStr);
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
                        trigger.Reply("Comando '{0}' não possui subcomando", cmdStr);
                        return;
                    }

                    var subcommand = (command as SubCommandContainer)[subcmdStr];

                    if (subcommand == null || subcommand.RequiredRole > trigger.UserRole)
                    {
                        trigger.Reply("Comando '{0} {1}' não existe", cmdStr, subcmdStr);
                        return;
                    }

                    DisplayFullSubCommandDescription(trigger, command, subcommand);
                }
            }
        }

        public static void DisplayCommandDescription(TriggerBase trigger, CommandBase command)
        {
            trigger.Reply(trigger.Bold("{0}") + "{1} - {2}",
                          string.Join("/", command.Aliases),
                          command is SubCommandContainer
                              ? string.Format(" ({0} subcmds)", ( command as SubCommandContainer ).Count(entry => entry.RequiredRole <= trigger.UserRole))
                              : "",
                          command.Description);
        }

        public static void DisplaySubCommandDescription(TriggerBase trigger, CommandBase command, SubCommand subcommand)
        {
            trigger.Reply(trigger.Bold("{0}") + " {1} - {2}",
                          command.Aliases.First(),
                          string.Join("/", subcommand.Aliases),
                          subcommand.Description);
        }


        public static void DisplayFullCommandDescription(TriggerBase trigger, CommandBase command)
        {
          
            trigger.Reply(trigger.Bold("{0}") + "{1} - {2}",
                          string.Join("/", command.Aliases),
                          command is SubCommandContainer && (command as SubCommandContainer).Count > 0
                              ? string.Format(" ({0} subcmds)", (command as SubCommandContainer).Count(trigger.CanAccessCommand))
                              : "",
                          command.Description);

            if (!(command is SubCommandContainer))
                trigger.Reply("  -> " + command.Aliases.First() + " " + command.GetSafeUsage());

            if (command.Parameters != null)
                foreach (var commandParameter in command.Parameters)
                {
                    DisplayCommandParameter(trigger, commandParameter);
                }

            if (!(command is SubCommandContainer))
                return;

            foreach (var subCommand in command as SubCommandContainer)
            {
                if (trigger.CanAccessCommand(subCommand))
                    DisplayFullSubCommandDescription(trigger, command, subCommand);
            }
        }

        public static void DisplayFullSubCommandDescription(TriggerBase trigger, CommandBase command,
                                                             SubCommand subcommand)
        {
            trigger.Reply(trigger.Bold("{0} {1}") + " - {2}",
                          command.Aliases.First(),
                          string.Join("/", subcommand.Aliases),
                          subcommand.Description);
            trigger.Reply("  -> " + command.Aliases.First() + " " + subcommand.Aliases.First() + " " + subcommand.GetSafeUsage());

            foreach (var commandParameter in subcommand.Parameters)
            {
                DisplayCommandParameter(trigger, commandParameter);
            }
        }

        public static void DisplayCommandParameter(TriggerBase trigger, IParameterDefinition parameter)
        {
            trigger.Reply("\t(" + trigger.Bold("{0}") + " : {1})",
                          parameter.GetUsage(),
                          parameter.Description ?? "");
        }
    }
}