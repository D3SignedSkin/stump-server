using NLog;
using Stump.Core.Attributes;
using Stump.Server.BaseServer.Commands;
using Stump.Server.BaseServer.Database;
using Stump.Server.BaseServer.Initialization;
using Stump.Server.WorldServer.Commands.Trigger;
using Stump.Server.WorldServer.Database.Startup;
using Stump.Server.WorldServer.Game.Accounts;
using System.Collections.Generic;
using System.Linq;

namespace Stump.Server.WorldServer.Game.Startupactions
{
    public class StartupManager : DataManager<StartupManager>
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private Dictionary<int, StartupActionRecordItems> m_startup;
        private Dictionary<int, CommandToConsole> m_console;
        public Dictionary<int, List<int>> confirmacao = new Dictionary<int, List<int>>();

        //private bool[] confirmacao = new bool [0];
        [Variable]
        public static int StartupDelaySeconds = 5;

        [Initialization(InitializationPass.Any)]
        public override void Initialize()
        {
            m_console = Database.Query<CommandToConsole>(StartupActionRecordRelator.FecthQuery_console).ToDictionary(x => x.Id);
            WorldServer.Instance.IOTaskPool.CallPeriodically(StartupDelaySeconds * 3 * 1000, Executar_Console);

            ////Desativado por não conseguir usar ainda o menu da tela de login do personagem com a função in-game
            //m_startup = Database.Query<StartupActionRecordItems>(StartupActionRecordRelator.FecthQuery_presentes).ToDictionary(x => x.Id);
            //WorldServer.Instance.IOTaskPool.CallPeriodically(StartupDelaySeconds * 1000, Atualizar_presentes);
        }

        #region Executar_Console
        public void Executar_Console()
        {
            try
            {
                m_console = Database.Query<CommandToConsole>(StartupActionRecordRelator.FecthQuery_console).ToDictionary(x => x.Id);
            }
            catch
            {
                return;
            }

            if (!m_console.Any())
                return;

            foreach (var console in m_console.Values)
            {
                WorldServer.Instance.IOTaskPool.AddMessage(() =>
                {
                    var test = AccountManager.Instance.FindById(console.OwnerId);

                    if (test != null)
                    {
                        if (!string.IsNullOrEmpty(console.Command))
                        {
                            var UserGroup = AccountManager.Instance.GetGroupOrDefault(console.RoleId);
                            var command = CommandManager.Instance.GetCommand(new Stump.Core.IO.StringStream(console.Command).NextWord());

                            if (command != null)
                                if (UserGroup.IsCommandAvailable(command))
                                    CommandManager.Instance.HandleCommand(new WorldConsoleTrigger(console.Command));
                        }
                    }
                    WorldServer.Instance.DBAccessor.Database.Delete(console);
                });
            }
        }
        #endregion

        #region Atualizar_Presente (Desativado)
        public void Atualizar_presentes()
        {
            try
            {
                m_startup = Database.Query<StartupActionRecordItems>(StartupActionRecordRelator.FecthQuery_presentes).ToDictionary(x => x.Id);
            }
            catch
            {
                return;
            }

            if (!m_startup.Any())
                return;

            foreach (var startup in m_startup.Values)
            {
                WorldServer.Instance.IOTaskPool.AddMessage(() =>
                {
                    var account = World.Instance.GetConnectedAccount(startup.OwnerId);

                    // doesn't exist anymore, so we delete it
                    if (account == null)
                    {
                        var test = AccountManager.Instance.FindById(startup.OwnerId);

                        if (test == null)
                            WorldServer.Instance.DBAccessor.Database.Delete(startup);
                    }
                    else
                    {
                        if (account.ConnectedCharacter.HasValue)
                        {
                            var character = World.Instance.GetCharacter(account.ConnectedCharacter.Value);
                            var hasKey = confirmacao.ContainsKey(startup.OwnerId) ? true : false;
                            var hasValue = false;

                            if (hasKey)
                                hasValue = confirmacao[startup.OwnerId].Any(value => value == startup.Id) ? true : false;

                            if (!hasValue) // isso aqui vai só verificar se tem esse ID no array ne 
                            {
                                if (hasKey)
                                    confirmacao[startup.OwnerId].Add(startup.Id);
                                else
                                {
                                    List<int> item = new List<int>();
                                    item.Add(startup.Id);

                                    confirmacao.Add(startup.OwnerId, item);
                                }

                                //StartupHandler.HandleStartupActionsExecuteMessage(character.Client, null);
                                //StartupHandler.SendStartupActionsListMessage(character.Client, character.Client.StartupActions);

                                switch (character.Account.Lang)
                                {
                                    case "fr":
                                        character.SendServerMessage("Un nouveau cadeau est disponible dans l'interface cadeaux et récompenses.");

                                        break;
                                    case "es":
                                        character.SendServerMessage("Un nuevo regalo está disponible en la interfaz de regalos y recompensas.");
                                        break;
                                    case "en":
                                        character.SendServerMessage("A new gift is available in the gifts and rewards interface.");
                                        break;
                                    default:
                                        character.SendServerMessage("Um novo presente está disponível na interface de presentes e recompensas.");
                                        break;
                                }
                            }
                        }
                    }
                });
            }
        }
        #endregion
    }
}
