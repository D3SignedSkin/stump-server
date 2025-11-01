using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using Stump.Core.Attributes;
using Stump.Core.Mathematics;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Messages;
using Stump.DofusProtocol.Types;
using Stump.ORM;
using Stump.Server.BaseServer;
using Stump.Server.BaseServer.Database;
using Stump.Server.BaseServer.Initialization;
using Stump.Server.BaseServer.IPC.Objects;
using Stump.Server.BaseServer.Network;
using Stump.Server.BaseServer.Plugins;
using Stump.Server.WorldServer.Core.IO;
using Stump.Server.WorldServer.Core.IPC;
using Stump.Server.WorldServer.Core.Network;
using Stump.Server.WorldServer.Game;
using ServiceStack.Text;
using DatabaseConfiguration = Stump.ORM.DatabaseConfiguration;
using Stump.Core.Reflection;
using Stump.Server.BaseServer.IPC.Messages;
using Stump.Core.Collections;
using System.Diagnostics;

namespace Stump.Server.WorldServer
{
    public class WorldServer : ServerBase<WorldServer>
    {
        /// <summary>
        /// Current server adress
        /// </summary>
        [Variable]
        public readonly static string Host = "127.0.0.1";

        /// <summary>
        /// Server port
        /// </summary>
        [Variable]
        public readonly static int Port = 3467;
        private DateTime m_lastAnnouncedTime;

        [Variable(true)]
        public static WorldServerData ServerInformation = new WorldServerData
        {
            Id = 1,
            Name = "Jiva",
            Address = "localhost",
            Port = 3467,
            Capacity = 2000,
            RequiredRole = RoleEnum.Player,
            RequireSubscription = false,
        };

        [Variable(Priority = 10)]
        public static DatabaseConfiguration DatabaseConfiguration = new DatabaseConfiguration
        {
            Host = "localhost",
            Port = "3306",
            DbName = "stump_world",
            User = "root",
            Password = "",
            ProviderName = "MySql.Data.MySqlClient",
            //UpdateFileDir = "./sql_update/",
        };

        [Variable(true)]
        public static int AutoSaveInterval = 6 * 60;

        [Variable(true)]
        public static bool SaveMessage = true;

        public WorldVirtualConsole VirtualConsoleInterface
        {
            get;
            protected set;
        }

        public WorldPacketHandler HandlerManager
        {
            get;
            private set;
        }
        //public static List<WorldClient> Clients = new List<WorldClient>();
        private static LimitedStack<Pair<DateTime, string>> m_IpErrorHistory = new LimitedStack<Pair<DateTime, string>>(500); //500 ips error in memory
        private static List<string> CheckList = new List<string>();
        private static List<string> BlockList = new List<string>();
        private static List<string> AuthList = new List<string>();
        private static readonly object m_locker = new object();
        private static readonly object m_locker_check = new object();
        public WorldServer() : base(Definitions.ConfigFilePath, Definitions.SchemaFilePath)
        {
        }
        //public override void BeiginAcceptCallBack(IAsyncResult result)
        //{

        //    Socket listener = (Socket)result.AsyncState;
        //    Socket acceptedSocket = null;
        //    try
        //    {
        //        acceptedSocket = listener.EndAccept(result);
        //    }
        //    catch (Exception ex) { logger.Error("Error Fatal!!!"); logger.Error(ex); return; }

        //    if (!CheckAttack(((IPEndPoint)(acceptedSocket.LocalEndPoint)).Address.ToString()))
        //    {
        //        socketListener.BeginAccept(BeiginAcceptCallBack, socketListener);
        //        if (acceptedSocket != null)
        //        {
        //            if (acceptedSocket.Connected)
        //            {                        //acceptedSocket.EndConnect(result);                        
        //                acceptedSocket.Disconnect(false);
        //                //acceptedSocket.EndDisconnect(result);
        //            }
        //        }
        //        return;
        //    }

        //    WorldClient client = new WorldClient(acceptedSocket);
        //        Clients.Add(client);
        //    OnConnectionAccepted(acceptedSocket);
        //    socketListener.BeginAccept(BeiginAcceptCallBack, socketListener);
        //    if (IPCAccessor.Instance.IsConnected)
        //    {
        //        IPCAccessor.Instance.SendRequest(new ServerUpdateMessage(WorldServer.Clients.Count, ServerStatusEnum.ONLINE), delegate (Server.BaseServer.IPC.Messages.CommonOKMessage message)
        //        {
        //            Game.Misc.ServerInfoManager.Instance.AddRecord(WorldServer.Clients.Count);
        //        });
        //    }
        //}
        public override void Initialize()
        {
            try
            {
                base.Initialize();
                ConsoleInterface = new WorldConsole();
                VirtualConsoleInterface = new WorldVirtualConsole();
                ConsoleBase.SetTitle($"#{ServerInformation.Name} World Server - {Version} : {ServerInformation.Name}");

                logger.Info("Initializing Database...");
                DBAccessor = new DatabaseAccessor(DatabaseConfiguration);
                DBAccessor.RegisterMappingAssembly(Assembly.GetExecutingAssembly());

                foreach (var plugin in PluginManager.Instance.GetPlugins())
                    DBAccessor.RegisterMappingAssembly(plugin.PluginAssembly);

                InitializationManager.Initialize(InitializationPass.Database);
                DBAccessor.Initialize();

                logger.Info("Opening Database...");
                DBAccessor.OpenConnection();
                DataManager.DefaultDatabase = DBAccessor.Database;
                DataManagerAllocator.Assembly = Assembly.GetExecutingAssembly();

                logger.Info("Register Messages...");
                MessageReceiver.Initialize();
                ProtocolTypeManager.Initialize();

                logger.Info("Register Packet Handlers...");
                HandlerManager = WorldPacketHandler.Instance;
                HandlerManager.RegisterAll(Assembly.GetExecutingAssembly());

                logger.Info("Register Commands...");
                CommandManager.RegisterAll(Assembly.GetExecutingAssembly());

                logger.Info("Starting IPC Communications ...");
                IPCAccessor.Instance.Start();

                //socketListener.Bind(new IPEndPoint(((IEnumerable<IPAddress>)Dns.GetHostAddresses(Host)).First<IPAddress>((Func<IPAddress, bool>)(ip => ip.AddressFamily == AddressFamily.InterNetwork)), Port));
                //socketListener.Listen(ClientManager.MaxPendingConnections);

                if (IPCAccessor.Instance.IsConnected)
                {
                    IPCAccessor.Instance.SendRequest(new ServerUpdateMessage(WorldServer.Instance.ClientManager.Count, ServerStatusEnum.STARTING), delegate (Server.BaseServer.IPC.Messages.CommonOKMessage message)
                    {
                    });
                }

                InitializationManager.InitializeAll();
                CommandManager.LoadOrCreateCommandsInfo(CommandsInfoFilePath);
                IsInitialized = true;

                try
                {
                    if (BaseServer.Settings.InactivityDisconnectionTime.HasValue)
                        IOTaskPool.CallPeriodically(BaseServer.Settings.InactivityDisconnectionTime.Value * 1000, DisconnectAfkClient);//Era Dividido por /4
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                    try
                    {
                        if (BaseServer.Settings.InactivityDisconnectionTime.HasValue)
                            IOTaskPool.CallPeriodically(BaseServer.Settings.InactivityDisconnectionTime.Value * 1000, DisconnectAfkClient);//Era Dividido por /4
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                HandleCrashException(ex);
                Shutdown();
            }

        }
        private void OnExecutingDBCommand(ORM.Database arg1, IDbCommand arg2)
        {
            if (!Initializing && !IOTaskPool.IsInContext)
            {
                logger.Warn("Execute DB command out the IO task pool : " + arg2.CommandText);
            }
        }
        protected override void OnPluginAdded(PluginContext plugincontext)
        {
            CommandManager.RegisterAll(plugincontext.PluginAssembly);
            base.OnPluginAdded(plugincontext);
        }

        public override void Start()
        {
            base.Start();

            logger.Info("Start Auto-Save Cyclic Task");
            IOTaskPool.CallPeriodically(AutoSaveInterval * 1000, World.Instance.Save);

            logger.Info("Starting Console Handler Interface...");
            ConsoleInterface.Start();

            // logger.Info("Starting IPC Communications ...");
            //IPCAccessor.Instance.Start();

            //logger.Info("Start listening on port : " + Port + "...");

            //socketListener.Bind(new IPEndPoint(((IEnumerable<IPAddress>)Dns.GetHostAddresses(Host)).First<IPAddress>((Func<IPAddress, bool>)(ip => ip.AddressFamily == AddressFamily.InterNetwork)), Port));
            //socketListener.Listen(ClientManager.MaxPendingConnections);
            ////socketListener.BeginAccept(BeiginAcceptCallBack, base.socketListener);
            //socketListener.BeginAccept(BeiginAcceptCallBack, base.socketListener);

            if (IPCAccessor.Instance.IsConnected)
            {
                IPCAccessor.Instance.SendRequest(new ServerUpdateMessage(WorldServer.Instance.ClientManager.Count, ServerStatusEnum.ONLINE), delegate (Server.BaseServer.IPC.Messages.CommonOKMessage message)
                {

                });
                IPCAccessor.Instance.Tick_public();
            }
            ClientManager.Start(Host, Port);
            IOTaskPool.Start();
            StartTime = DateTime.Now;

        }

        protected override BaseClient CreateClient(Socket s)
        {
            return new WorldClient(s);
        }

        protected override void DisconnectAfkClient()
        {
            List<WorldClient> clientsAFK = FindClients(client => client.LastActivity != null && DateTime.Now.Subtract(client.LastActivity).TotalMinutes >= BaseServer.Settings.InactivityDisconnectionTime).ToList();

            foreach (var client in clientsAFK)
            {
                client.DisconnectAfk();
                logger.Warn($"The client IP {client.IP} was disconnected due to inactivity of {DateTime.Now.Subtract(client.LastActivity).TotalMinutes}/{BaseServer.Settings.InactivityDisconnectionTime} minutes on WorldServer.");
            }
        }

        public bool DisconnectClient(int accountId)
        {
            IEnumerable<WorldClient> enumerable = FindClients(client => client.Account != null && client.Account.Id == accountId);

            foreach (var current in enumerable)
            {
                current.Disconnect();
            }
            return enumerable.Any();
        }

        public bool DisconnectClient(uint accountId)
        {
            IEnumerable<WorldClient> clients = Instance.FindClients(client => client.Account != null && client.Account.Id == accountId);

            foreach (WorldClient client in clients)
            {
                client.Disconnect();
            }

            return clients.Any();
        }

        public WorldClient[] FindClients(Predicate<WorldClient> predicate)
        {
            return ClientManager.FindAll(predicate);
        }

        public override void ScheduleShutdown(TimeSpan timeBeforeShuttingDown)
        {
            base.ScheduleShutdown(timeBeforeShuttingDown);
            AnnounceTimeBeforeShutdown(timeBeforeShuttingDown, false);
        }

        public override void CancelScheduledShutdown()
        {
            base.CancelScheduledShutdown();
            Singleton<World>.Instance.SendAnnounce("Reboot canceled !", Color.Red);
        }

        protected override void CheckScheduledShutdown()
        {
            var diff = TimeSpan.FromMinutes(AutomaticShutdownTimer) - UpTime;
            var automatic = true;

            if (IsShutdownScheduled && diff > ScheduledShutdownDate - DateTime.Now)
            {
                diff = ScheduledShutdownDate - DateTime.Now;
                automatic = false;
            }

            if (diff < TimeSpan.FromHours(12))
            {
                TimeSpan announceDiff = (DateTime.Now - m_lastAnnouncedTime);

                if (diff > TimeSpan.FromHours(1) && announceDiff >= TimeSpan.FromHours(1))
                {
                    AnnounceTimeBeforeShutdown(TimeSpan.FromHours(diff.TotalHours.RoundToNearest(1)), automatic);
                }
                else if (diff > TimeSpan.FromMinutes(30) && diff <= TimeSpan.FromHours(1) && announceDiff >= TimeSpan.FromMinutes(10))
                {
                    AnnounceTimeBeforeShutdown(TimeSpan.FromMinutes(diff.TotalMinutes.RoundToNearest(30)), automatic);
                }
                else if (diff > TimeSpan.FromMinutes(10) && diff <= TimeSpan.FromMinutes(30) && announceDiff >= TimeSpan.FromMinutes(5))
                {
                    AnnounceTimeBeforeShutdown(TimeSpan.FromMinutes(diff.TotalMinutes.RoundToNearest(10)), automatic);
                }
                else if (diff > TimeSpan.FromMinutes(5) && diff <= TimeSpan.FromMinutes(10) && announceDiff >= TimeSpan.FromMinutes(2))
                {
                    AnnounceTimeBeforeShutdown(TimeSpan.FromMinutes(diff.TotalMinutes), automatic);
                }
                else if (diff > TimeSpan.FromMinutes(1) && diff <= TimeSpan.FromMinutes(5) && announceDiff >= TimeSpan.FromMinutes(1))
                {
                    AnnounceTimeBeforeShutdown(TimeSpan.FromMinutes(diff.TotalMinutes.RoundToNearest(1)), automatic);
                }
                else if (diff > TimeSpan.FromSeconds(10) && diff <= TimeSpan.FromMinutes(1) && announceDiff >= TimeSpan.FromSeconds(10))
                {
                    AnnounceTimeBeforeShutdown(TimeSpan.FromSeconds(diff.TotalSeconds.RoundToNearest(10)), automatic);
                }
                else if (diff <= TimeSpan.FromSeconds(10) && diff > TimeSpan.Zero)
                {
                    AnnounceTimeBeforeShutdown(TimeSpan.FromSeconds(diff.Seconds), automatic);
                }
            }

            base.CheckScheduledShutdown();
        }

        private void AnnounceTimeBeforeShutdown(TimeSpan time, bool automatic)
        {
            World.Instance.SendAnnounce(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 15, time.ToString(@"hh\h\ mm\m\ ss\s"));
            m_lastAnnouncedTime = DateTime.Now;
        }

        //protected override void OnShutdown()
        //{
        //    if (IsInitialized)
        //    {
        //        var wait = new AutoResetEvent(false);
        //        IOTaskPool.ExecuteInContext(delegate
        //        {
        //            Singleton<World>.Instance.Stop(true);
        //            Singleton<World>.Instance.Save();
        //            wait.Set();
        //        });
        //        wait.WaitOne();
        //    }
        //    IPCAccessor.Instance.Stop();
        //    IOTaskPool?.Stop();

        //    Discn:
        //    var client = Clients.FirstOrDefault();
        //    if (client != null)
        //    {
        //        // client.Disconnect(true);
        //        client.Disconnect();
        //        Clients.Remove(client);
        //    }
        //    if (Clients.Count > 0)
        //    {
        //        goto Discn;
        //    }
        //}
        protected override void OnShutdown()
        {
            if (IsInitialized)
            {
                var wait = new AutoResetEvent(false);
                IOTaskPool.ExecuteInContext(() =>
                {
                    World.Instance.Stop(true);
                    World.Instance.Save();
                    wait.Set();
                });

                wait.WaitOne(-1);
            }

            IPCAccessor.Instance.Stop();

            if (IOTaskPool != null)
                IOTaskPool.Stop();

            ClientManager.Pause();

            foreach (var client in ClientManager.Clients.ToArray())
            {
                client.Disconnect();
            }

            ClientManager.Close();
        }
        //public bool CheckAttack(string ipAddress)
        //{
        //    lock (m_locker_check)
        //    {
        //        if (CheckList.Contains(ipAddress) || BlockList.Contains(ipAddress))
        //            return false;
        //        if (AuthList.Contains(ipAddress))
        //            return true;
        //        CheckList.Add(ipAddress);
        //    }
        //    string[] jsonExploded = null;
        //    try
        //    {
        //        WebClient web = new WebClient();
        //        string json;
        //        json = web.DownloadString("https://api.blocklist.de/api.php?ip=" + ipAddress);
        //        if (json == null)//site off?!
        //        {
        //            lock (m_locker_check)
        //            {
        //                CheckList.Remove(ipAddress);
        //                return true;
        //            }
        //        }
        //        json = json.Replace("attacks: ", "").Replace("<br />reports: ", ",").Replace("<br />", "");
        //        Console.WriteLine(jsonExploded);
        //        jsonExploded = json.Split(',');
        //    }
        //    catch
        //    {
        //        lock (m_locker_check)
        //        {
        //            CheckList.Remove(ipAddress);
        //            return true;
        //        }
        //    }
        //    int attacks = 0;
        //    int report = 0;
        //    int.TryParse(jsonExploded[0], out attacks);
        //    int.TryParse(jsonExploded[1], out report);

        //    if (attacks > 0 || report > 0)
        //    {
        //        AddErrorIP(ipAddress, true);
        //        lock (m_locker_check)
        //        {
        //            BlockList.Add(ipAddress);
        //            CheckList.Remove(ipAddress);
        //            return false;
        //        }
        //    }
        //    else
        //    {
        //        lock (m_locker_check)
        //        {
        //            AuthList.Add(ipAddress);
        //            CheckList.Remove(ipAddress);

        //            return true;
        //        }
        //    }
        //}
        public static void AddErrorIP(string m_ip, bool force = false)
        {
            lock (m_locker)
            {
                if (m_ip == null || m_ip == "")
                    return;
                if (!force)
                    m_IpErrorHistory.Push(new Pair<DateTime, string>(DateTime.Now, m_ip));
                if (m_IpErrorHistory.Where(x => x.Second == m_ip && DateTime.Now.Subtract(x.First).TotalSeconds < 3.0).Count() >= 5 || force)
                {
                    if (force)
                    {
                        lock (m_locker_check)
                            BlockList.Add(m_ip);
                    }
                    Process process = new Process();
                    process.StartInfo.FileName = "cmd.exe";
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.RedirectStandardInput = true;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.UseShellExecute = false;
                    process.Start();
                    string code = "netsh advfirewall firewall add rule name=\"IP Block\" dir=in interface=any action=block remoteip=" + m_ip + "/32";
                    process.StandardInput.WriteLine(code);
                    process.StandardInput.Flush();
                    process.StandardInput.Close();
                }
            }

        }
    }
}