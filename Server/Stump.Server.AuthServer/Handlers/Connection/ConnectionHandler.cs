using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using Stump.Core.Attributes;
using Stump.Core.Extensions;
using Stump.Core.Threading;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Messages;
using Stump.Server.AuthServer.Database;
using Stump.Server.AuthServer.Managers;
using Stump.Server.AuthServer.Network;
using Stump.Server.BaseServer.Initialization;
using Stump.Server.BaseServer.Network;
using Version = Stump.DofusProtocol.Types.Version;
using Stump.DofusProtocol.Messages.Custom;
using Stump.Core.IO;
using System.Security.Cryptography;
using MongoDB.Bson;
using System.Globalization;
using Stump.Server.BaseServer.Logging;
using Stump.DofusProtocol.Types;

namespace Stump.Server.AuthServer.Handlers.Connection
{
    public partial class ConnectionHandler : AuthHandlerContainer
    {
        public static SynchronizedCollection<AuthClient> ConnectionQueue = new SynchronizedCollection<AuthClient>();
        private static Task m_queueRefresherTask;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        [Initialization(InitializationPass.First)]
        private static void Initialize()
        {
            m_queueRefresherTask = Task.Factory.StartNewDelayed(3000, RefreshQueue);
        }

        // still thread safe theorically
        private static void RefreshQueue()
        {
            try
            {
                lock (ConnectionQueue.SyncRoot)
                {
                    var toRemove = new List<AuthClient>();
                    var count = 0;
                    foreach (var authClient in ConnectionQueue)
                    {
                        count++;

                        if (!authClient.Connected)
                        {
                            toRemove.Add(authClient);
                        }

                        if (DateTime.Now - authClient.InQueueUntil <= TimeSpan.FromSeconds(3))
                            continue;

                        SendQueueStatusMessage(authClient, (short)count, (short)ConnectionQueue.Count);
                        authClient.QueueShowed = true;
                    }

                    foreach (var authClient in toRemove)
                    {
                        ConnectionQueue.Remove(authClient);
                    }
                }
            }
            finally
            {
                m_queueRefresherTask = Task.Factory.StartNewDelayed(3000, RefreshQueue);
            }
        }

        /// <summary>
        /// Max Number of connection to logs in the database
        /// </summary>
        [Variable]
        public static uint MaxConnectionLogs = 5;

        #region Identification
        [AuthHandler(ClearIdentificationMessage.Id)]
        public static void HandleClearIdentificationMessage(AuthClient client, ClearIdentificationMessage message)
        {
            lock (ConnectionQueue.SyncRoot)
                ConnectionQueue.Remove(client);

            if (client.QueueShowed)
                SendQueueStatusMessage(client, 0, 0); // close the popup

            /* Invalid password */
            Account account;

            if (!CredentialManager.Instance.DecryptCredentials(out account, message))
            {
                SendIdentificationFailedMessage(client, IdentificationFailureReasonEnum.WRONG_CREDENTIALS);
                client.DisconnectLater(1000);
                return;
            }

            /* Check ServerIP - AntiBot Mesure */
            //if (!message.serverIp.Contains(AuthServer.CustomHost)) //Will cause problem with NAT as CustomHost will not be the public server IP
            //{
            //    SendIdentificationFailedMessage(client, IdentificationFailureReasonEnum.BANNED);
            //    client.DisconnectLater(1000);
            //    return;
            //}

            SendCredentialsAcknowledgementMessage(client);

            client.Account = account;

            /* Check Sanctions */
            if (account.IsBanned && account.BanEndDate > DateTime.Now)
            {
                #region // ----------------- Sistema de Logs MongoDB Banned by: Kenshin ---------------- //
                try
                {
                    var document = new BsonDocument
                    {
                            { "AccountId", account.Id },
                            { "AccountLogin", account.Login },
                            { "AccountNickname", account.Nickname },
                            { "AccountEmail", account.Email },
                            { "LastConnectedIp", account.LastConnectedIp },
                            { "LastHardwareId", account.LastHardwareId },
                            { "BanReason", account.BanReason },
                            { "BanEndDate", account.BanEndDate },
                            { "Status", "Isbanned" },
                            { "Date", DateTime.Now.ToString(CultureInfo.CreateSpecificCulture("pt-BR")) }
                    };

                    MongoLogger.Instance.Insert("Player_LoginBanned", document);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Erro no Mongologs Banned : " + e.Message);
                }
                #endregion

                SendIdentificationFailedBannedMessage(client, account.BanEndDate.Value);
                client.DisconnectLater(1000);
                return;
            }

            if (account.IsLifeBanned)
            {
                #region // ----------------- Sistema de Logs MongoDB Banned by: Kenshin ---------------- //
                try
                {
                    var document = new BsonDocument
                    {
                            { "AccountId", account.Id },
                            { "AccountLogin", account.Login },
                            { "AccountNickname", account.Nickname },
                            { "AccountEmail", account.Email },
                            { "LastConnectedIp", account.LastConnectedIp },
                            { "LastHardwareId", account.LastHardwareId },
                            { "BanReason", account.BanReason },
                            { "BanEndDate", account.BanEndDate },
                            { "Status", "IsLifebanned" },
                            { "Date", DateTime.Now.ToString(CultureInfo.CreateSpecificCulture("pt-BR")) }
                    };

                    MongoLogger.Instance.Insert("Player_LoginBanned", document);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Erro no Mongologs Banned : " + e.Message);
                }
                #endregion

                SendIdentificationFailedBannedMessage(client);
                client.DisconnectLater(1000);
                return;
            }

            if (account.BanEndDate < DateTime.Now)
            {
                account.IsBanned = false;
                account.IsJailed = false;
                account.BanEndDate = null;
            }

            var ipBan = AccountManager.Instance.FindMatchingIpBan(client.IP);

            if (ipBan != null && ipBan.GetRemainingTime() > TimeSpan.Zero)
            {
                #region // ----------------- Sistema de Logs MongoDB Banned by: Kenshin ---------------- //
                try
                {
                    var document = new BsonDocument
                    {
                            { "AccountId", account.Id },
                            { "AccountLogin", account.Login },
                            { "AccountNickname", account.Nickname },
                            { "AccountEmail", account.Email },
                            { "LastConnectedIp", account.LastConnectedIp },
                            { "LastHardwareId", account.LastHardwareId },
                            { "BanReason", account.BanReason },
                            { "BanEndDate", account.BanEndDate },
                            { "Status", "IpBanned" },
                            { "Date", DateTime.Now.ToString(CultureInfo.CreateSpecificCulture("pt-BR")) }
                    };

                    MongoLogger.Instance.Insert("Player_LoginBanned", document);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Erro no Mongologs Banned : " + e.Message);
                }
                #endregion

                SendIdentificationFailedBannedMessage(client, ipBan.GetEndDate());
                client.DisconnectLater(1000);
                return;
            }

            var hardwareIdBan = AccountManager.Instance.FindHardwareIdBan(message.hardwareId);

            if (hardwareIdBan != null)
            {
                #region // ----------------- Sistema de Logs MongoDB Banned by: Kenshin ---------------- //
                try
                {
                    var document = new BsonDocument
                    {
                            { "AccountId", account.Id },
                            { "AccountLogin", account.Login },
                            { "AccountNickname", account.Nickname },
                            { "AccountEmail", account.Email },
                            { "LastConnectedIp", account.LastConnectedIp },
                            { "LastHardwareId", account.LastHardwareId },
                            { "BanReason", account.BanReason },
                            { "BanEndDate", account.BanEndDate },
                            { "Status", "HardwareIdBanned" },
                            { "Date", DateTime.Now.ToString(CultureInfo.CreateSpecificCulture("pt-BR")) }
                    };

                    MongoLogger.Instance.Insert("Player_LoginBanned", document);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Erro no Mongologs Banned : " + e.Message);
                }
                #endregion

                SendIdentificationFailedBannedMessage(client);
                client.DisconnectLater(1000);
                return;
            }

            AccountManager.Instance.DisconnectClientsUsingAccount(account, client, success => AuthServer.Instance.IOTaskPool.AddMessage(() =>
            {
                // we must reload the record since it may have been modified
                if (success)
                    account = AccountManager.Instance.FindAccountById(account.Id);

                /* Bind Account to Client */
                client.Account = account;
                client.UserGroup = AccountManager.Instance.FindUserGroup(account.Role);
                client.Account.LastHardwareId = message.hardwareId;

                if (client.UserGroup == null)
                {
                    SendIdentificationFailedMessage(client, IdentificationFailureReasonEnum.UNKNOWN_AUTH_ERROR);
                    logger.Error("User group {0} doesn't exist !", client.Account.Role);
                    return;
                }

                /* Propose at client to give a nickname */
                if (client.Account.Nickname == string.Empty)
                {
                    client.Send(new NicknameRegistrationMessage());
                    return;
                }

                //SendServerSettingsMessage(client);
                SendIdentificationSuccessMessage(client, false);

                /* If autoconnect, send to the lastServer */
                if (message.autoconnect && client.Account.LastConnectionWorld != null && WorldServerManager.Instance.CanAccessToWorld(client, client.Account.LastConnectionWorld.Value))
                {
                    if (message.serverId != client.Account.LastConnectionWorld.Value && WorldServerManager.Instance.CanAccessToWorld(client, message.serverId))
                        SendSelectServerData(client, WorldServerManager.Instance.GetServerById(message.serverId));
                    else
                        SendSelectServerData(client, WorldServerManager.Instance.GetServerById(client.Account.LastConnectionWorld.Value));
                }
                else
                {
                    SendServersListMessage(client, true);
                }
            }), () =>
            {
                client.Disconnect();
                logger.Error("Error while joining last used world server, connection aborted");
            });
        }

        [AuthHandler(IdentificationMessage.Id)]
        public static void HandleIdentificationMessage(AuthClient client, IdentificationMessage message)
        {
            //Console.WriteLine("Message major: " + message.version.major);
            //Console.WriteLine("Message minor: " + message.version.minor);
            //Console.WriteLine("Message code: " + message.version.code);
            //Console.WriteLine("Message build: " + message.version.build);
            //Console.WriteLine("Message buildType: " + message.version.buildType);

            bool HasVersionOld = message.version.major == 2 && message.version.minor == 71 && message.version.code == 6 && message.version.build == 7 && message.version.buildType == 0 ? false : true;

            if (HasVersionOld)
            {
                switch (message.lang)
                {
                    case "fr":
                        client.Send(new SystemMessageDisplayMessage(true, 50, new List<string>() { "Votre jeu n'est pas la dernière version, pour continuer à jouer, vous devrez télécharger le client complet depuis le site Web. \n\nVeuillez vous rendre sur notre site Web", "<b><u><a href=\"https://serverhydra.com/fr/telecharger\">Téléchargez le nouveau client Hydra.</a></u><b>\n" }));
                        break;
                    case "es":
                        client.Send(new SystemMessageDisplayMessage(true, 50, new List<string>() { "Tu juego no es la última versión, para seguir jugando tendrás que descargar el cliente completo del sitio web. \n\nVisite nuestro sitio web", "<b><u><a href=\"https://serverhydra.com/es/descargar\">Descargar Nuevo Cliente Hydra.</a></u><b>\n" }));
                        break;
                    case "en":
                        client.Send(new SystemMessageDisplayMessage(true, 50, new List<string>() { "Your game is not the latest version, to continue playing you will have to download the full client from the website. \n\nPlease go to our website", "<b><u><a href=\"https://serverhydra.com/en/download\">Download New Hydra Client.</a></u><b>\n" }));
                        break;
                    default:
                        client.Send(new SystemMessageDisplayMessage(true, 50, new List<string>() { "O seu jogo não está na versão mais recente, para continuar jogando, você terá que baixar o cliente completo no site. \n\nPor favor, acesse o nosso website", "<b><u><a href=\"https://serverhydra.com/pt/download\">Baixar Novo Cliente Hydra.</a></u><b>\n" }));
                        break;
                }

                client.DisconnectLater(1000);
                return;
            }

            ///* Wrong Version */
            //if (!message.version.IsUpToDate())
            //{
            //    SendIdentificationFailedForBadVersionMessage(client, VersionExtension.ExpectedVersion);
            //    client.DisconnectLater(1000);
            //    return;
            //}

            var patch = AuthServer.Instance.GetConnectionSwfPatch();

            if (patch != null)
                client.Send(new RawDataMessageFixed(patch));
        }

        public static void SendIdentificationSuccessMessage(AuthClient client, bool wasAlreadyConnected)
        {
            var creationDate = (DateTime.Now - client.Account.CreationDate).TotalMilliseconds;
            bool hasUserConnected = false;

            client.Send(new IdentificationSuccessMessage(
                hasRights: client.UserGroup.IsGameMaster,
                hasReportRight: false,
                hasForceRight: false,
                wasAlreadyConnected: hasUserConnected,
                login: client.Account.Login,
                accountTag: new AccountTagInformation(client.Account.Nickname, client.Account.Id.ToString()),
                accountId: client.Account.Id,
                communityId: 6,
                accountCreation: creationDate < 0 ? 0 : creationDate,
                subscriptionEndDate: client.Account.GoldSubscriptionEnd < DateTime.Now ? client.Account.SubscriptionEnd.GetUnixTimeStampDouble() : client.Account.GoldSubscriptionEnd.GetUnixTimeStampDouble(),
                havenbagAvailableRoom: 0));

            client.LookingOfServers = true;
        }

        public static void SendIdentificationFailedMessage(AuthClient client, IdentificationFailureReasonEnum reason)
        {
            client.Send(new IdentificationFailedMessage((sbyte)reason));
        }

        public static void SendIdentificationFailedForBadVersionMessage(AuthClient client, Version version)
        {
            client.Send(new IdentificationFailedForBadVersionMessage((sbyte)IdentificationFailureReasonEnum.BAD_VERSION, version));
        }

        public static void SendIdentificationFailedBannedMessage(AuthClient client)
        {
            client.Send(new IdentificationFailedMessage((sbyte)IdentificationFailureReasonEnum.BANNED));
        }

        public static void SendIdentificationFailedBannedMessage(AuthClient client, DateTime date)
        {
            client.Send(new IdentificationFailedBannedMessage((sbyte)IdentificationFailureReasonEnum.BANNED, date.GetUnixTimeStampLong()));
        }

        public static void SendQueueStatusMessage(IPacketReceiver client, short position, short total)
        {
            client.Send(new LoginQueueStatusMessage((ushort)position, (ushort)total));
        }
        #endregion

        #region Server Selection
        [AuthHandler(ServerSelectionMessage.Id)]
        public static void HandleServerSelectionMessage(AuthClient client, ServerSelectionMessage message)
        {
            var world = WorldServerManager.Instance.GetServerById(message.serverId);

            /* World not exist */
            if (world == null)
            {
                SendSelectServerRefusedMessage(client, (short)message.serverId,
                    ServerConnectionErrorEnum.SERVER_CONNECTION_ERROR_NO_REASON);
                return;
            }

            /* Wrong state */
            if (world.Status != ServerStatusEnum.ONLINE)
            {
                SendSelectServerRefusedMessage(client, world,
                    ServerConnectionErrorEnum.SERVER_CONNECTION_ERROR_DUE_TO_STATUS);
                return;
            }

            /* not suscribe */
            if (world.RequireSubscription && client.Account.SubscriptionEnd <= DateTime.Now && client.Account.GoldSubscriptionEnd <= DateTime.Now)
            {
                SendSelectServerRefusedMessage(client, world,
                    ServerConnectionErrorEnum.SERVER_CONNECTION_ERROR_SUBSCRIBERS_ONLY);
                return;
            }

            /* not the rights */
            if (world.RequiredRole > client.UserGroup.Role && !client.UserGroup.AvailableServers.Contains(world.Id))
            {
                SendSelectServerRefusedMessage(client, world,
                    ServerConnectionErrorEnum.SERVER_CONNECTION_ERROR_ACCOUNT_RESTRICTED);
                return;
            }

            /* Send client to the server */
            SendSelectServerData(client, world);
        }

        public static void SendSelectServerData(AuthClient client, WorldServer world)
        {
            /* Check if is null */
            if (world == null)
                return;

            client.LookingOfServers = false;

            /* Bind Ticket */
            client.Account.Ticket = new AsyncRandom().RandomString(32);
            AccountManager.Instance.CacheAccount(client.Account);

            //Verifica quantos dias ficou sem conectar da última vez by:Kenshin
            if (client.Account.LastConnection.HasValue)
            {
                TimeSpan dateDays = DateTime.Now.Subtract(client.Account.LastConnection.Value);
                client.Account.LastDaysConnection = dateDays.Days >= 1 ? dateDays.Days : 0;
            }

            client.Account.LastConnection = DateTime.Now;
            client.Account.LastConnectedIp = client.IP;
            client.Account.LastConnectionWorld = world.Id;
            client.SaveNow();

            BigEndianWriter writer = new BigEndianWriter();
            writer.WriteUTFBytes(client.Account.Ticket);

            //Version : 2.61 By Kenshin
            client.Send(new SelectedServerDataMessage(
                serverId: (ushort)world.Id,
                address: world.Address,
                ports: new ushort[1] { world.Port },
                canCreateNewCharacter: client.UserGroup.Role >= world.RequiredRole || client.UserGroup.AvailableServers.Contains(world.Id),
                ticket: Encoding.ASCII.GetBytes(client.Account.Ticket).Select(x => (sbyte)x).ToArray()));

            client.DisconnectLater(1000);
        }

        public static byte[] EncryptAES(byte[] data, byte[] key)
        {
            var iv = key.Take(16).ToArray();
            try
            {
                using (var rijndaelManaged = new RijndaelManaged { })
                {
                    var crypto = rijndaelManaged.CreateEncryptor();

                    return crypto.TransformFinalBlock(data, 0, data.Length);
                }
            }
            catch (CryptographicException e)
            {
                Console.WriteLine("A Cryptographic error occurred: {0}", e.Message);
                return null;
            }
        }

        public static void SendSelectServerRefusedMessage(AuthClient client, WorldServer world, ServerConnectionErrorEnum reason)
        {
            client.Send(new SelectedServerRefusedMessage((ushort)world.Id, (sbyte)reason, (sbyte)world.Status));
        }

        public static void SendSelectServerRefusedMessage(AuthClient client, short worldId, ServerConnectionErrorEnum reason)
        {
            client.Send(new SelectedServerRefusedMessage((ushort)worldId, (sbyte)reason, (sbyte)ServerStatusEnum.STATUS_UNKNOWN));
        }

        public static void SendServersListMessage(AuthClient client, bool canCreateCharacter)
        {
            GameServerInformations[] serversInformations = WorldServerManager.Instance.GetServersInformationArray(client);

            client.Send(new ServersListMessage(serversInformations, canCreateCharacter));
        }

        public static void SendServerStatusUpdateMessage(AuthClient client, WorldServer world)
        {
            if (world != null)
                client.Send(new ServerStatusUpdateMessage(WorldServerManager.Instance.GetServerInformation(client, world)));
        }

        public static void SendCredentialsAcknowledgementMessage(AuthClient client)
        {
            client.Send(new CredentialsAcknowledgementMessage());
        }
        #endregion
    }
}