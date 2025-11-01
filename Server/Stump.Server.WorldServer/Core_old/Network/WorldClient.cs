using System;
using System.Collections.Generic;
using System.Net.Sockets;
using NLog;
using Stump.DofusProtocol.Messages;
using Stump.Server.BaseServer.IPC.Objects;
using Stump.Server.BaseServer.Network;
using Stump.Server.WorldServer.Database.Accounts;
using Stump.Server.WorldServer.Database.Characters;
using Stump.Server.WorldServer.Game.Accounts;
using Stump.Server.WorldServer.Game.Accounts.Startup;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Handlers.Approach;
using Stump.Server.WorldServer.Handlers.Basic;
using Stump.Server.WorldServer.Core.IPC;
using Stump.Core.Reflection;
using System.Net;
using Stump.Core.IO;
using Stump.Server.BaseServer;
using Stump.Core.Collections;
using System.Linq;
using Stump.Server.BaseServer.IPC.Messages;
using Stump.DofusProtocol.Enums;

namespace Stump.Server.WorldServer.Core.Network
{
    public sealed class WorldClient : BaseClient, IEquatable<WorldClient>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public Queue<Message> MessageQueue;
        public DateTime TimeOfArrivingPacket;

        public CharacterRecord ForceCharacterSelection
        {
            get;
            set;
        }

        public UserGroup UserGroup
        {
            get;
            private set;
        }
        // CONSTRUCTORS
        public WorldClient(Socket socket)
            : base(socket)
        {

            Send(new ProtocolRequired(VersionExtension.ProtocolRequired, VersionExtension.ActualProtocol));
            Send(new HelloGameMessage());

            CanReceive = true;
            StartupActions = new List<StartupAction>();

            lock (ApproachHandler.ConnectionQueue.SyncRoot)
                ApproachHandler.ConnectionQueue.Add(this);

            InQueueUntil = DateTime.Now;
        }

        // PROPERTIES
        public bool AutoConnect { get; set; }

        public AccountData Account { get; private set; }

        public DateTime InQueueUntil { get; set; }

        public bool QueueShowed { get; set; }

        public WorldAccount WorldAccount { get; internal set; }

        public List<StartupAction> StartupActions { get; set; }

        public List<CharacterRecord> Characters { get; internal set; }

        public Character Character { get; internal set; }
        public int PacketsNumber
        {
            get;
            set;
        }

        // METHODS
        public int GetHighLevel()
        {
            var highLevel = 0;
            foreach (var character in Characters)
            {
                int level = Singleton<ExperienceManager>.Instance.GetCharacterLevel(character.Experience);
                if (level > highLevel)
                    highLevel = level;
            }
            return highLevel;
        }
        public void SetCurrentAccount(AccountData account)
        {
            Account = account;
            Characters = Singleton<CharacterManager>.Instance.GetCharactersByAccount(this);
            if (IPCAccessor.Instance.IsConnected)
            {
                //account. = Characters.Count;
                IPCAccessor.Instance.Send(new BaseServer.IPC.Messages.UpdateAccountMessage(account));
            }
            UserGroup = AccountManager.Instance.GetGroupOrDefault(account.UserGroupId);

            if (UserGroup == AccountManager.DefaultUserGroup)
                logger.Error("Group {0} not found. Use default group instead !", account.UserGroupId);
        }

        public void DisconnectAfk()
        {
            BasicHandler.SendSystemMessageDisplayMessage(this, true, 1);
            Disconnect();
        }

        #region CallBack
        public override void DisconectedCallBack(IAsyncResult asyncResult)
        {
            try
            {
                WorldServer.Clients.Remove(this);
               
                base.Runing = false;
                Socket client = (Socket)asyncResult.AsyncState;
                client.EndDisconnect(asyncResult);

                client.Shutdown(SocketShutdown.Both);
                client.Close();

                OnDisconnected(new DisconnectedEventArgs(Socket));

            }
            catch (System.Exception ex)
            {
                OnError(new ErrorEventArgs(ex));
            }
        }
        public override void ReceiveCallBack(IAsyncResult asyncResult)
        {
            try
            {
                Socket client = (Socket)asyncResult.AsyncState;
                if (client.Connected == false)
                {
                    Runing = false;
                    return;
                }
                if (Runing)
                {
                    int bytesRead = 0;

                    try
                    {

                        if (client == null || asyncResult == null)
                        {
                            return;
                        }
                        try
                        {
                            SocketError errorCode;
                            bytesRead = client.EndReceive(asyncResult, out errorCode);
                            if (errorCode != SocketError.Success)
                            {
                                bytesRead = 0;
                            }
                        }
                        catch { bytesRead = 0; }


                        if (bytesRead == 0)
                        {
                            Runing = false;
                            this.Disconnect();
                            return;
                        }

                        byte[] data = new byte[bytesRead];
                        Array.Copy(receiveBuffer, data, bytesRead);
                        buffer.Add(data, 0, data.Length);

                        ThreatBuffer();
                        var messagePart = DataReceivedEventArgs.Data;
                        // this.currentMessage = null;
                        if (messagePart == null)
                        {


                            Disconnect();
                            return;
                        }
                        BigEndianReader Reader = new BigEndianReader(messagePart.Data);
                        Message message = null;
                        try
                        {
                            message = MessageReceiver.BuildMessage((ushort)messagePart.MessageId, Reader, this.IP);
                        }
                        catch
                        {
                            //Adcionar trava DDOS IPSEC
                            //WorldServer.AddErrorIP(this.IP);
                            try
                            {
                                if (this.m_IPFirst != null)
                                    WorldServer.AddErrorIP(this.m_IPFirst);
                                else
                                {
                                    WorldServer.AddErrorIP(this.IP);
                                }

                            }
                            catch { }
                            Disconnect();
                            return;
                        }
#if DEBUG
                        if (WorldServer.Host == "127.0.0.1")
                        {
                            Console.WriteLine(string.Format("[RCV] {0} -> {1}", ((IPEndPoint)this.Socket.RemoteEndPoint).Address.ToString(), message));

                        }
                        //logger.Info(string.Format("[RCV] {0} -> {1}", ((IPEndPoint)this.Socket.RemoteEndPoint).Address.ToString(), message));
#endif
                        if (message is BasicPingMessage)
                        {
                            Send(new BasicPongMessage((message as BasicPingMessage).quiet));
                        }
                        else
                        {

                            Singleton<WorldPacketHandler>.Instance.Dispatch(this, message);
                        }
                        if (!MessagesWhitelist.Contains(message.ToString()))
                            m_messagesHistory.Push(new Pair<DateTime, Message>(DateTime.Now, message));

                        var time = m_messagesHistory.Last.Value.First.Subtract(m_messagesHistory.First.Value.First);

                        //Flood check, 
                        if (FloodCheck && (m_messagesHistory.Count == m_messagesHistory.MaxItems && time.TotalSeconds < FloodMinTime))
                        {
                            logger.Error($"Forced disconnection {this}: Flood: {m_messagesHistory.Count} messages in {time.TotalSeconds} seconds ! - LastMessages: {m_messagesHistory.Select(x => x.Second).ToCSV(",")}");
                            Disconnect();

                            return;
                        }
                        client.BeginReceive(receiveBuffer, 0, BufferSize, SocketFlags.None, new AsyncCallback(ReceiveCallBack), client);

                    }
                    catch (System.Exception ex)
                    {
                        //WorldServer.AddErrorIP(this.IP);
                        try
                        {
                            if (this.m_IPFirst != null)
                                WorldServer.AddErrorIP(this.m_IPFirst);
                            else
                            {
                                WorldServer.AddErrorIP(this.IP);
                            }

                        }
                        catch { }
                        Logger.Error(ex.ToString());
                        Disconnect();
                    }
                }
                else
                    Console.WriteLine("Receive data but not running");
            }
            catch (System.Exception ex)
            {
                try
                {
                    Logger.Error(ex.ToString());
                    OnError(new ErrorEventArgs(ex));
                }
                catch (OutOfMemoryException e)
                {
                    logger.Error("Error received of type OutOfMemoryException in ReceiveCallBack");
                }
            }
        }
        public override void SendCallBack(IAsyncResult asyncResult)
        {
            try
            {
                Socket client = (Socket)asyncResult.AsyncState;
                client.EndSend(asyncResult);
                OnDataSended(new DataSendedEventArgs());
            }
            catch (System.Exception ex)
            {
                try
                {
                    if (this.m_IPFirst != null)
                        WorldServer.AddErrorIP(this.m_IPFirst);
                    else
                    {
                        WorldServer.AddErrorIP(this.IP);
                    }

                }
                catch { }


                OnError(new ErrorEventArgs(ex));
                Disconnect();
            }
        }
#endregion
        //public void Disconnect(bool serverStopping = false)
        //{
        //    Character?.LogOut();

        //    ServerBase<WorldServer>.Instance.IOTaskPool.AddMessage(delegate
        //    {
        //        if (WorldAccount == null) return;
        //        WorldAccount.ConnectedCharacter = null;
        //        ServerBase<WorldServer>.Instance.DBAccessor.Database.Update(WorldAccount);
        //    });
        //    if (!serverStopping)
        //    {
        //        base.Disconnect();
        //    }
        //    WorldServer.Clients.Remove(this);
        //    if (IPCAccessor.Instance.IsConnected)
        //    {
        //        IPCAccessor.Instance.SendRequest(new ServerUpdateMessage(WorldServer.Clients.Count, ServerStatusEnum.ONLINE), delegate (Server.BaseServer.IPC.Messages.CommonOKMessage message)
        //        {
        //        });
        //    }
        //}
        public new void DisconnectLater(int duration = 0)
        {
            WorldServer.Instance.IOTaskPool.CallDelayed(duration, Disconnect);
        }
        public new void Send(Message message)
        {
            try
            {
                var writer = new BigEndianWriter();
                message.Pack(writer);
                //base.Send(writer.Data);
                if (this.Character != null) {
                    //this.Character.temptest = message.ToString(); ;
                   
                    bool test = base.Sendteste(writer.Data);
                    //if (test == true)
                        //Logger.Error("Jogador:"+Character.Namedefault+" deu os seguintes error:1°(o de agora)"+ Character.test[0] + " 2°"+ Character.test[1] + " 3°"+ Character.test[2] + " 4°"+ Character.test[3] + " 5°" + Character.test[4] + " 6°" + Character.test[5] + " 7°" + Character.test[6] + " 8°" + Character.test[7] + " 9°" + Character.test[8] + " 10°" + Character.test[9]);
                }
                else
                    base.Send(writer.Data);

                  #if DEBUG
                if (WorldServer.Host == "127.0.0.1")
                { 
                Console.WriteLine(string.Format("[SND] {0} -> {1}", IP, message));
                   }
               //logger.Info(string.Format("[SND] {0} -> {1}", IP, message));
#endif
                //this.Character
            }
            catch (Exception e)
            {
                logger.Error("Methode Send error: " + e);
            }
        }
        public override string ToString() => base.ToString() + (Account != null ? " (" + Account.Login + ")" : "");

        public int GetHashCode(WorldClient obj)
        {
            return obj.IP.GetHashCode();
        }

        public bool Equals(WorldClient other)
        {
            if (this.IP.Equals(other.IP))
            {
                return true;
            }
            return false;
        }


        //private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        //public WorldClient(Socket socket)
        //    : base(socket)
        //{
        //    Send(new ProtocolRequired(VersionExtension.ProtocolRequired, VersionExtension.ActualProtocol));
        //    Send(new HelloGameMessage());

        //    CanReceive = true;
        //    StartupActions = new List<StartupAction>();

        //    lock (ApproachHandler.ConnectionQueue.SyncRoot)
        //        ApproachHandler.ConnectionQueue.Add(this);

        //    InQueueUntil = DateTime.Now;
        //}

        //public bool AutoConnect
        //{
        //    get;
        //    set;
        //}

        //public AccountData Account
        //{
        //    get;
        //    private set;
        //}

        //public DateTime InQueueUntil
        //{
        //    get;
        //    set;
        //}

        //public bool QueueShowed
        //{
        //    get;
        //    set;
        //}

        //public WorldAccount WorldAccount
        //{
        //    get;
        //    internal set;
        //}

        //public List<StartupAction> StartupActions
        //{
        //    get;
        //    private set;
        //}

        //public List<CharacterRecord> Characters
        //{
        //    get;
        //    internal set;
        //}

        //public CharacterRecord ForceCharacterSelection
        //{
        //    get;
        //    set;
        //}

        //public Character Character
        //{
        //    get;
        //    internal set;
        //}

        //public UserGroup UserGroup
        //{
        //    get;
        //    private set;
        //}

        //public void SetCurrentAccount(AccountData account)
        //{
        //    if (Account != null)
        //        throw new Exception("Account already set");

        //    Account = account;
        //    Characters = CharacterManager.Instance.GetCharactersByAccount(this);
        //    UserGroup = AccountManager.Instance.GetGroupOrDefault(account.UserGroupId);

        //    if (UserGroup == AccountManager.DefaultUserGroup)
        //        logger.Error("Group {0} not found. Use default group instead !", account.UserGroupId);
        //}

        //public override void OnMessageSent(Message message)
        //{
        //    base.OnMessageSent(message);
        //}

        //protected override void OnMessageReceived(Message message)
        //{
        //    WorldPacketHandler.Instance.Dispatch(this, message);

        //    base.OnMessageReceived(message);
        //}

        //public void DisconnectAfk()
        //{
        //    BasicHandler.SendSystemMessageDisplayMessage(this, true, 1);

        //    Disconnect();
        //}

        //protected override void OnDisconnect()
        //{
        //    if (Character != null)
        //    {
        //        Character.LogOut();
        //    }

        //    WorldServer.Instance.IOTaskPool.AddMessage(() =>
        //    {
        //        if (WorldAccount == null)
        //            return;

        //        WorldAccount.ConnectedCharacter = null;
        //        WorldServer.Instance.DBAccessor.Database.Update(WorldAccount);
        //    });

        //    base.OnDisconnect();
        //}

    }
}