using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using Stump.Core.Extensions;
using Stump.Core.Threading;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Messages;
using Stump.DofusProtocol.Types;
using Stump.Server.BaseServer.Commands;
using Stump.Server.BaseServer.Initialization;
using Stump.Server.BaseServer.IPC.Messages;
using Stump.Server.BaseServer.Network;
using Stump.Server.WorldServer.Core.IPC;
using Stump.Server.WorldServer.Core.Network;
using Stump.Server.WorldServer.Game;
using Stump.Server.WorldServer.Game.Accounts;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Breeds;
using Stump.Server.WorldServer.Game.Parties;

namespace Stump.Server.WorldServer.Handlers.Approach
{
    public class ApproachHandler : WorldHandlerContainer
    {
        public static SynchronizedCollection<WorldClient> ConnectionQueue = new SynchronizedCollection<WorldClient>();
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private static Task m_queueRefresherTask;
        private static readonly object m_locker = new object();

        [Initialization(InitializationPass.First)]
        private static void Initialize()
        {
            m_queueRefresherTask = Task.Factory.StartNewDelayed(3000, RefreshQueue);
        }

        private static void RefreshQueue()
        {
            try
            {
                lock (ConnectionQueue.SyncRoot)
                {
                    var toRemove = new List<WorldClient>();
                    var count = 0;

                    foreach (var worldClient in ConnectionQueue)
                    {
                        count++;

                        if (!worldClient.Connected)
                        {
                            toRemove.Add(worldClient);
                        }

                        if (DateTime.Now - worldClient.InQueueUntil <= TimeSpan.FromSeconds(3))
                            continue;

                        SendQueueStatusMessage(worldClient, (short)count, (short)ConnectionQueue.Count);
                        worldClient.QueueShowed = true;
                    }

                    foreach (var worldClient in toRemove)
                    {
                        ConnectionQueue.Remove(worldClient);
                    }
                }
            }
            finally
            {
                m_queueRefresherTask = Task.Factory.StartNewDelayed(3000, RefreshQueue);
            }
        }

        [WorldHandler(AuthenticationTicketMessage.Id, ShouldBeLogged = false, IsGamePacket = false)]
        public static void HandleAuthenticationTicketMessage(WorldClient client, AuthenticationTicketMessage message)
        {
            try
            {
                if (!IPCAccessor.Instance.IsConnected)
                {
                    client.Send(new AuthenticationTicketRefusedMessage());
                    client.DisconnectLater(1000);
                    return;
                }

                var ticketSplitted = message.ticket.Split(',');

                message.ticket = Encoding.ASCII.GetString(ticketSplitted.Select(x => (byte)int.Parse(x)).ToArray());
                logger.Debug("Client request ticket {0}", message.ticket);

                IPCAccessor.Instance.SendRequest<AccountAnswerMessage>(
                    new AccountRequestMessage { Ticket = message.ticket },
                    msg => WorldServer.Instance.IOTaskPool.AddMessage(() => OnAccountReceived(msg, client)),
                    error => client.Disconnect());
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        [WorldHandler(ReloginTokenRequestMessage.Id, IsGamePacket = false)]
        public static void HandleReloginTokenRequestMessage(WorldClient client, ReloginTokenRequestMessage message)
        {
            try
            {
                client.Send(new ReloginTokenStatusMessage(false, Encoding.ASCII.GetBytes(client.Account.Ticket).Select(x => x).ToString()));
            }
            catch (Exception ex)
            {
                logger.Error($"Error processing ReloginTokenRequestMessage: {ex.Message}");
            }
        }

        [WorldHandler(HaapiTokenRequestMessage.Id)]
        public static void HandleHaapiTokenRequestMessage(WorldClient client, HaapiTokenRequestMessage message)
        {
            client.Send(new HaapiTokenMessage(client.Account.Ticket));
        }

        static void OnAccountReceived(AccountAnswerMessage message, WorldClient client)
        {
            logger.Info($"{client} Check 1!");

            //Character dummy;
            if (AccountManager.Instance.IsAccountBlocked(message.Account.Id, out Character dummy))
            {
                logger.Info($"{client} Check 1-1!");
                logger.Error($"{client} - Account({message.Account.Id}) blocked, connection unallowed");
                client.Disconnect();
                return;
            }

            logger.Info($"{client} Check 2!");

            lock (ConnectionQueue.SyncRoot)
            {
                logger.Info($"{client} Check 2-1!");
                ConnectionQueue.Remove(client);
                logger.Info($"{client} Check 2-2!");
            }

            logger.Info($"{client} Check 3!");

            if (!IPCAccessor.Instance.IsConnected) //verify again for test
            {
                logger.Info($"{client} Check 3-1!");
                client.Send(new AuthenticationTicketRefusedMessage());
                client.DisconnectLater(1000);
                return;
            }

            logger.Info($"{client} Check 4!");

            if (client.QueueShowed)
                SendQueueStatusMessage(client, 0, 0); // close the popup

            logger.Info($"{client} Check 5!");

            var ticketAccount = message.Account;

            logger.Info($"{client} Check 6!");

            /* Check null ticket */
            if (ticketAccount == null || ticketAccount.Id <= 0)
            {
                logger.Info($"{client} Check 6-1!");
                client.Send(new AuthenticationTicketRefusedMessage());
                client.DisconnectLater(1000);
                return;
            }

            logger.Info($"{client} Check 7!");

            var clients = WorldServer.Instance.FindClients(x => x.Account != null && x.Account.Id == ticketAccount.Id).ToArray();

            logger.Info($"{client} Check 8!");

            clients.ForEach(x => x.Disconnect());

            logger.Info($"{client} Check 9!");

            // not an expected situation
            if (clients.Length > 0)
            {
                logger.Info($"{client} Check 9-1!");
                client.Disconnect();
                return;
            }

            //if (ticketAccount.Id <= 0)
            //{
            //    client.Send(new AuthenticationTicketRefusedMessage());
            //    client.DisconnectLater(1000);
            //    return;
            //}
            //Console.WriteLine("Ticket Account ID:"+ ticketAccount.Id);

            /* Bind WorldAccount if exist */
            logger.Info($"{client} Check 10!");

            Database.Accounts.WorldAccount account;

            lock (m_locker)
            {
                account = AccountManager.Instance.FindById(ticketAccount.Id);
            }

            logger.Info($"{client} Check 11!");

            if (account != null)
            {
                client.WorldAccount = account;

                if (client.WorldAccount.ConnectedCharacter != null)
                {
                    var character = World.Instance.GetCharacter(client.WorldAccount.ConnectedCharacter.Value);

                    if (character != null)
                        character.LogOut();
                }
            }

            logger.Info($"{client} Check 12!");

            /* Bind Account & Characters */
            client.SetCurrentAccount(ticketAccount);

            logger.Info($"{client} Check 13!");

            client.Send(new AuthenticationTicketAcceptedMessage());

            SendBasicTime(client);

            client.Send(new ServerSettingsMessage(isMonoAccount: false, hasFreeAutopilot: false, lang: "en", community: (sbyte)ServerCommunityEnum.INTERNATIONAL, gameType: (sbyte)GameServerTypeEnum.SERVER_TYPE_CLASSICAL, arenaLeaveBanTime: 30, itemMaxLevel: 200));

            logger.Info($"{client} Check 14!");

            int[] features = OptionalFeaturesManager.Instance.GetFeaturesId();

            ServerSessionConstant[] constants = new ServerSessionConstant[]
            {
                new ServerSessionConstantInteger((ushort)ServerConstantTypeEnum.TIME_BEFORE_DISCONNECTION, GetTimeInactityDisconnection()),
                new ServerSessionConstantInteger((ushort)ServerConstantTypeEnum.KOH_DURATION, 30),
                new ServerSessionConstantInteger((ushort)ServerConstantTypeEnum.KOH_WINNING_SCORE, 60000),
                new ServerSessionConstantInteger((ushort)ServerConstantTypeEnum.MINIMAL_TIME_BEFORE_KOH, 100),
                new ServerSessionConstantInteger((ushort)ServerConstantTypeEnum.TIME_BEFORE_WEIGH_IN_KOH, 2000),
                new ServerSessionConstantInteger((ushort)ServerConstantTypeEnum.UNKOWN_6, 0),
            };

            SendServerOptionalFeaturesMessage(client, features);
            SendServerSessionConstantsMessage(client, constants);

            logger.Info($"{client} Check 15!");
            SendAccountCapabilitiesMessage(client);

            logger.Info($"{client} Check 16!");
            client.Send(new TrustStatusMessage(true)); // Restrict actions if account is not trust

            logger.Info($"{client} Check 17!");

            /* Just to get console AutoCompletion */
            if (client.UserGroup.IsGameMaster)
                SendConsoleCommandsListMessage(client, CommandManager.Instance.AvailableCommands.Where(x => client.UserGroup.IsCommandAvailable(x)));

            logger.Info($"{client} Check 18!");
        }

        private static int GetTimeInactityDisconnection()
        {
            var time = BaseServer.Settings.InactivityDisconnectionTime * 1000 ?? -1;

            if (time == -1)
                logger.Error("GetTimeInactityDisconnection is value -1.");

            return time;
        }

        public static void SendStartupActionsListMessage(IPacketReceiver client)
        {
            client.Send(new GameActionItemListMessage());
        }

        public static void SendServerOptionalFeaturesMessage(IPacketReceiver client, params int[] features)
        {
            client.Send(new ServerOptionalFeaturesMessage(features.Select(x => x).ToArray()));
        }

        public static void SendServerSessionConstantsMessage(IPacketReceiver client, params ServerSessionConstant[] constants)
        {
            client.Send(new ServerSessionConstantsMessage(constants));
        }

        public static void SendAccountCapabilitiesMessage(WorldClient client)
        {
            client.Send(new AccountCapabilitiesMessage(
                tutorialAvailable: false,
                canCreateNewCharacter: true,
                accountId: client.Account.Id,
                breedsVisible: BreedManager.Instance.AvailableBreedsFlags,
                breedsAvailable: BreedManager.Instance.AvailableBreedsFlags,
                status: 0));
        }

        public static void SendConsoleCommandsListMessage(IPacketReceiver client, IEnumerable<CommandBase> commands)
        {
            var commandsInfos = (from command in commands
                                 let aliases = command.GetFullAliases()
                                 let usage = command.GetSafeUsage()
                                 from alias in aliases
                                 select Tuple.Create(alias, usage, command.Description ?? string.Empty)).ToList();

            client.Send(
                new ConsoleCommandsListMessage(
                    commandsInfos.Select(x => x.Item1),
                    commandsInfos.Select(x => x.Item2),
                    commandsInfos.Select(x => x.Item3)));
        }

        public static void SendBasicTime(IPacketReceiver client)
        {
            var offset = (short)TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).TotalMinutes;
            client.Send(new BasicTimeMessage(DateTime.Now.GetUnixTimeStampLong(), offset));
        }

        public static void SendQueueStatusMessage(IPacketReceiver client, short position, short total)
        {
            client.Send(new QueueStatusMessage((ushort)position, (ushort)total));
        }
    }
}