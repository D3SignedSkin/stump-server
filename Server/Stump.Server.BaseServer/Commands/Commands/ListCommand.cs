using System;
using System.Collections.Generic;
using System.Linq;
using Stump.DofusProtocol.Enums;

namespace Stump.Server.BaseServer.Commands.Commands
{
    public class ListCommand : CommandBase
    {
        public ListCommand()
        {
            Aliases = new [] { "commandslist" };
            RequiredRole = RoleEnum.Player;
            Description = "Listar todos os comandos disponíveis";
            Description_en = "List all available commands";
            Description_es = "Listar todos los comandos disponibles";
            Description_fr = "Liste toutes les commandes disponibles";
            Parameters = new List<IParameterDefinition>
            {
                new ParameterDefinition<string>("comando", "cmd", "Listar sub comandos específicos", string.Empty),
                new ParameterDefinition<RoleEnum>("função", "função", "Listar comandos disponíveis para uma determinada função", RoleEnum.None, true) // we must define isOptional = true because RoleEnum.None = default(RoleEnum)
            };
            Parameters_en = new List<IParameterDefinition>
            {
                new ParameterDefinition<string>("command", "cmd", "List specifics sub commands", string.Empty),
                new ParameterDefinition<RoleEnum>("role", "role", "List commands available for a given role", RoleEnum.None, true) // we must define isOptional = true because RoleEnum.None = default(RoleEnum)
              };
            Parameters_es = new List<IParameterDefinition>
            {
                new ParameterDefinition<string>("comando", "cmd", "Listar sub comandos específicos", string.Empty),
                new ParameterDefinition<RoleEnum>("rol", "rol", "Listar comandos disponibles para un rol dado", RoleEnum.None, true) // we must define isOptional = true because RoleEnum.None = default(RoleEnum)
            };
            Parameters_fr = new List<IParameterDefinition>
            {
                new ParameterDefinition<string>("commande", "cmd", "Liste des sous commandes spécifiques", string.Empty),
                new ParameterDefinition<RoleEnum>("rôle", "rôle", "Liste des commandes disponibles pour un rôle donné", RoleEnum.None, true) // we must define isOptional = true because RoleEnum.None = default(RoleEnum)
            };
        }

        public override void Execute(TriggerBase trigger)
        {
            var role = trigger.Get<RoleEnum>("função");
            var cmd = trigger.Get<string>("comando");
         
            role = role != RoleEnum.None && role <= trigger.UserRole ?
                role : trigger.UserRole;

            IEnumerable<CommandBase> availableCommands =
                CommandManager.Instance.AvailableCommands.Where(command => !(command is SubCommand));

            if (cmd != string.Empty)
            {
                var command = CommandManager.Instance.GetCommand(cmd);

                if (command == null || !trigger.CanAccessCommand(command))
                {
                    trigger.ReplyError("Não foi encontrado '{0}'", command);
                    return;
                }

                if (command is SubCommandContainer)
                    availableCommands = (command as SubCommandContainer); // if a command is specified we display his childrens
                else
                    availableCommands = new []{command};
            }

            var commands = from entry in availableCommands
                           where entry.RequiredRole <= role || (!trigger.IsArgumentDefined("função") && trigger.CanAccessCommand(entry))
                           select entry;

            trigger.Reply(string.Join(", ", from entry in commands
                                            select (entry is SubCommandContainer ?
                                                string.Format(trigger.CanFormat ? "<b>{0}</b>({1})" : "{0}({1})", entry.Aliases.First(), ( entry as SubCommandContainer ).Count)
                                                : entry.Aliases.First())));
        }
    }
}