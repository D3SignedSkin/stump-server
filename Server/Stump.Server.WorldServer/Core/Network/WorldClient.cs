using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Sockets;
using MongoDB.Bson;
using NLog;
using Stump.DofusProtocol.Messages;
using Stump.Server.BaseServer.IPC.Messages;
using Stump.Server.BaseServer.IPC.Objects;
using Stump.Server.BaseServer.Logging;
using Stump.Server.BaseServer.Network;
using Stump.Server.WorldServer.Core.IPC;
using Stump.Server.WorldServer.Database.Accounts;
using Stump.Server.WorldServer.Database.Characters;
using Stump.Server.WorldServer.Game.Accounts;
using Stump.Server.WorldServer.Game.Accounts.Startup;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Handlers.Approach;
using Stump.Server.WorldServer.Handlers.Basic;

namespace Stump.Server.WorldServer.Core.Network
{
    public sealed class WorldClient : BaseClient
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public WorldClient(Socket socket) : base(socket)
        {
            Send(new ProtocolRequired(VersionExtension.ProtocolRequired));
            Send(new HelloGameMessage());

            CanReceive = true;
            StartupActions = new List<StartupAction>();

            lock (ApproachHandler.ConnectionQueue.SyncRoot)
            {
                ApproachHandler.ConnectionQueue.Add(this);
            }

            InQueueUntil = DateTime.Now;
        }

        public bool AutoConnect { get; set; }

        public AccountData Account { get; set; }

        public DateTime InQueueUntil { get; set; }

        public bool QueueShowed { get; set; }

        public WorldAccount WorldAccount { get; set; }

        public List<StartupAction> StartupActions { get; set; }

        public List<CharacterRecord> Characters { get; internal set; }

        public CharacterRecord ForceCharacterSelection { get; set; }

        public Character Character { get; internal set; }

        public UserGroup UserGroup { get; private set; }

        public ushort SequenceNumber = 1;

        public uint AsyncMessageInstance = 1;

        public void SetCurrentAccount(AccountData account)
        {
            if (Account != null)
                throw new Exception("Account already set");

            Account = account;
            Characters = CharacterManager.Instance.GetCharactersByAccount(this);
            UserGroup = AccountManager.Instance.GetGroupOrDefault(account.UserGroupId);

            if (UserGroup == AccountManager.DefaultUserGroup)
                logger.Error("Group {0} not found. Use default group instead !", account.UserGroupId);
        }

        public bool CreateAccountToken(int amount, string method, string itemname, int characterid, string charactername)
        {
            try
            {
                int AccountTokens = 0;

                using (var db = WorldServer.Instance.DBAccessor.Database)
                {
                    AccountTokens = db.FirstOrDefault<int>($"SELECT Tokens FROM hydra_auth.accounts WHERE Id = '{Account.Id}'");
                }

                if (amount == 0)
                    return false;

                if (AccountTokens == Account.Tokens)
                {
                    var RoleName = "";
                    int TokenAfter = Account.Tokens;

                    if (Account.UserGroupId <= 3)
                        RoleName = "Player";
                    else
                        RoleName = "Staff";

                    Account.Tokens = (Account.Tokens + amount);
                    IPCAccessor.Instance.Send(new UpdateTokensMessage(Account.Tokens, Account.Id));

                    #region // ----------------- Creación del token del sistema de registro MongoDB por: Kenshin ---------------- //
                    var document = new BsonDocument
                        {
                          { "AccountUserGroup", RoleName },
                          { "AccountId", Account.Id },
                          { "AccountName", Account.Login },
                          { "CharacterId", characterid },
                          { "CharacterName", charactername },
                          { "Method", method },
                          { "Status", "Create" },
                          { "AmountToken", amount },
                          { "TokenName", itemname },
                          { "AfterToken", TokenAfter },
                          { "BeforeToken", Account.Tokens },
                          { "HardwareId", Account.LastHardwareId },
                          { "Date", DateTime.Now.ToString(CultureInfo.CreateSpecificCulture("pt-BR")) }
                        };
                    MongoLogger.Instance.Insert("Player_Tokens", document);
                    #endregion

                    return true; // Devuelve verdadero indicando éxito
                }

                return false; //Devuelve falso indicando error
            }
            catch (Exception e)
            {
                Console.WriteLine("Erro na Criação de Ogrines WorldClient: " + e.Message);

                return false; // Devuelve falso indicando error
            }
        }

        public override void OnMessageSent(Message message)
        {
            base.OnMessageSent(message);
        }

        protected override void OnMessageReceived(Message message)
        {
            WorldPacketHandler.Instance.Dispatch(this, message);

            base.OnMessageReceived(message);
        }

        public void DisconnectAfk()
        {
            BasicHandler.SendSystemMessageDisplayMessage(this, true, 1);
            Disconnect();
        }

        protected override void OnDisconnect()
        {
            if (Character != null)
            {
                Character.LogOut();
            }

            WorldServer.Instance.IOTaskPool.AddMessage(() =>
            {
                if (WorldAccount == null)
                    return;

                WorldAccount.ConnectedCharacter = null;

                try
                {
                    WorldServer.Instance.DBAccessor.Database.Update(WorldAccount);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error during database update: {ex.Message}. The world account could not be updated.");
                }
            });

            base.OnDisconnect();
        }

        public override string ToString() => base.ToString() + (Account != null ? " (" + Account.Login + ")" : "");
    }
}