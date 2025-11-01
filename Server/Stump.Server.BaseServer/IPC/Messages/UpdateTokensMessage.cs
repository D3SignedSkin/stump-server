using ProtoBuf;

namespace Stump.Server.BaseServer.IPC.Messages
{
    [ProtoContract]
    public class UpdateTokensMessage : IPCMessage
    {
        public UpdateTokensMessage()
        {

        }

        public UpdateTokensMessage(int tokens, int accountId)
        {
            Tokens = tokens;
            AccountId = accountId;
        }

        [ProtoMember(2)]
        public int Tokens
        {
            get;
            set;
        }
        [ProtoMember(3)]
        public int AccountId
        {
            get;
            set;
        }
    }
}
