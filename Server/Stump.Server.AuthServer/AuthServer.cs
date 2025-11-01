
using Stump.Core.Attributes;
using Stump.Core.Collections;
using Stump.DofusProtocol.Messages;
using Stump.DofusProtocol.Types;
using Stump.ORM;
using Stump.Server.AuthServer.IO;
using Stump.Server.AuthServer.IPC;
using Stump.Server.AuthServer.Managers;
using Stump.Server.AuthServer.Network;
using Stump.Server.BaseServer;
using Stump.Server.BaseServer.Database;
using Stump.Server.BaseServer.Initialization;
using Stump.Server.BaseServer.Network;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using DatabaseConfiguration = Stump.ORM.DatabaseConfiguration;

namespace Stump.Server.AuthServer
{

    public class AuthServer : ServerBase<AuthServer>
    {
        //public static List<AuthClient> Clients = new List<AuthClient>();
        private static LimitedStack<Pair<DateTime, string>> m_IpErrorHistory = new LimitedStack<Pair<DateTime, string>>(500);//500 ips error in memory
        private static List<string> CheckList = new List<string>();
        private static List<string> BlockList = new List<string>();
        private static List<string> AuthList = new List<string>();
        private static readonly object m_locker = new object();
        private static readonly object m_locker_check = new object();

        [Variable]
        public static readonly bool HostAutoDefined = true;

        /// <summary>
        /// Current server address. Used if HostAutoDefined = false
        /// </summary>
        [Variable]
        public static readonly string Host = "127.0.0.1";

        /// <summary>
        /// Public server address. Used for AntiBotMesure
        /// </summary>
        [Variable]
        public static readonly string CustomHost = "127.0.0.1";

        /// <summary>
        /// Server port
        /// </summary>
        [Variable]
        public static readonly int Port = 443;

        [Variable]
        public static string IpcAddress = "localhost";

        [Variable]
        public static int IpcPort = 9100;


        [Variable]
        public static string ConnectionSwfPatch = "./swf_patchs/AuthPatch.swf";

        string m_host;

        [Variable(Priority = 10)]
        public static DatabaseConfiguration DatabaseConfiguration = new DatabaseConfiguration
        {
            ProviderName = "MySql.Data.MySqlClient",
            Host = "localhost",
            Port = "3306",
            DbName = "stump_auth",
            User = "root",
            Password = "",
        };

        private byte[] m_patchBuffer;
        public bool m_maintenanceMode;

        public IPCHost IpcHost
        {
            get;
            private set;
        }

        public AuthPacketHandler HandlerManager
        {
            get;
            private set;
        }

        public AuthServer() :
            base(Definitions.ConfigFilePath, Definitions.SchemaFilePath)
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

        //    if (CountClientWithSameIp(((IPEndPoint)(acceptedSocket.LocalEndPoint)).Address) >= ClientManager.MaxIPConnexions.Value)
        //    {
        //        socketListener.BeginAccept(BeiginAcceptCallBack, socketListener);
        //        if (acceptedSocket != null)
        //        {
        //            if (acceptedSocket.Connected)
        //            {
        //                //acceptedSocket.EndConnect(result);                        
        //                acceptedSocket.Disconnect(false);
        //                //acceptedSocket.EndDisconnect(result);
        //            }
        //        }
        //        return;
        //    }
        //    AuthClient client = new AuthClient(acceptedSocket);
        //    AuthServer.Clients.Add(client);

        //    OnConnectionAccepted(acceptedSocket);
        //    socketListener.BeginAccept(BeiginAcceptCallBack, socketListener);
        //}
        public override void Initialize()
        {

            try
            {
                base.Initialize();
                ConsoleInterface = new AuthConsole();
                ConsoleBase.SetTitle($"#Charly Authentification Server - {Version}");            
                logger.Info("Initializing Database...");
                DBAccessor = new DatabaseAccessor(DatabaseConfiguration);
                DBAccessor.RegisterMappingAssembly(Assembly.GetExecutingAssembly());
                InitializationManager.Initialize(InitializationPass.Database);
                DBAccessor.Initialize();

                logger.Info("Opening Database...");
                DBAccessor.OpenConnection();
                DataManager.DefaultDatabase = DBAccessor.Database;
                DataManagerAllocator.Assembly = Assembly.GetExecutingAssembly();

                logger.Info("Register Messages...");
                MessageReceiver.Initialize();
                ProtocolTypeManager.Initialize();

                logger.Info("Register Packets Handlers...");
                HandlerManager = AuthPacketHandler.Instance;
                HandlerManager.RegisterAll(Assembly.GetExecutingAssembly());

                logger.Info("Register Commands...");
                CommandManager.RegisterAll(Assembly.GetExecutingAssembly());

                logger.Info("Start World Servers Manager");
                WorldServerManager.Instance.Initialize();

                logger.Info("Initialize Account Manager");
                AccountManager.Instance.Initialize();

                //Task.Factory.StartNewDelayed(800, () => logger.Info("Initialize DDOS Firewall Manager")).Wait();
                //idk whats is !??!?

                //Task.Factory.StartNewDelayed(1200, () => logger.Info("DDOS Patched !")).Wait();

                logger.Info("Initialize IPC Server..");
                IpcHost = new IPCHost(IpcAddress, IpcPort);

                InitializationManager.InitializeAll();
                IsInitialized = true;

                if (Environment.GetCommandLineArgs().Contains("-maintenance"))
                    m_maintenanceMode = true;
            }
            catch (Exception ex)
            {
                HandleCrashException(ex);
                Shutdown();
            }
        }

        public override void Start()
        {
            base.Start();

            logger.Info("Start Ipc Server");
            IpcHost.Start();

            logger.Info("Starting Console Handler Interface...");
            ConsoleInterface.Start();

            logger.Info("Start listening on port : " + Port + "...");
            m_host = HostAutoDefined ? IPAddress.Loopback.ToString() : Host;

            //socketListener.Bind(new IPEndPoint(((IEnumerable<IPAddress>)Dns.GetHostAddresses(m_host)).First<IPAddress>((Func<IPAddress, bool>)(ip => ip.AddressFamily == AddressFamily.InterNetwork)), Port));
            //socketListener.Listen(ClientManager.MaxPendingConnections);
            //socketListener.BeginAccept(BeiginAcceptCallBack, base.socketListener);
            ClientManager.Start(m_host, Port);

            IOTaskPool.Start();

            StartTime = DateTime.Now;
        }

        public byte[] GetConnectionSwfPatch()
        {
            if (m_patchBuffer != null)
                return m_patchBuffer;

            if (File.Exists(ConnectionSwfPatch))
                return m_patchBuffer = File.ReadAllBytes(ConnectionSwfPatch);

            logger.Warn("SWF Patch for connection not found ({0}", ConnectionSwfPatch);
            return null;
        }


        protected override void OnShutdown()
        {
            DBAccessor.CloseConnection();
        }

        protected override BaseClient CreateClient(Socket s)
        {
            return new AuthClient(s);
        }

        public IEnumerable<AuthClient> FindClients(Predicate<AuthClient> predicate)
        {
            //return Clients.Where(x => x != null).ToList().FindAll(predicate);
            return ClientManager.FindAll(predicate);
        }
        //public int CountClientWithSameIp(IPAddress ipAddress)
        //{
        //    lock (Clients)
        //    {
        //        var count = 0;
        //        foreach (var t in Clients.Where(x => x != null))
        //        {
        //            if (t.Socket != null && t.Socket.Connected && t.Socket.RemoteEndPoint != null)
        //            {
        //                var ip = ((IPEndPoint)t.Socket.RemoteEndPoint).Address;
        //                if (ip.Equals(ipAddress) && ip.AddressFamily != AddressFamily.InterNetwork) // not a fake client
        //                    count++;
        //            }
        //        }

        //        return count;
        //    }
        //}
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
        //    catch {
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
                if(!force)
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