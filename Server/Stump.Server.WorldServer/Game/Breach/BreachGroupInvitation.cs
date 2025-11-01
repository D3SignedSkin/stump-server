using Stump.DofusProtocol.Messages;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;

namespace Stump.Server.WorldServer.Game.Breach
{
    public class BreachGroupInvitation
    {
        private Character host;
        private BreachInvitationOfferMessage message;

        public BreachGroupInvitation(Character host, BreachInvitationOfferMessage message)
        {
            this.host = host;
            this.message = message;
        }

        public Character Host
        {
            get => host;
            set => host = value;
        }

        public BreachInvitationOfferMessage Message
        {
            get => message;
            set => message = value;
        }
    }
}