using System.Linq;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Messages;
using Stump.Server.BaseServer.Network;
using Stump.Server.WorldServer.Core.Network;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Arena;

namespace Stump.Server.WorldServer.Handlers.Context
{
    public partial class ContextHandler
    {
        [WorldHandler(GameRolePlayArenaFightAnswerMessage.Id)]
        public static void HandleGameRolePlayArenaFightAnswerMessage(WorldClient client, GameRolePlayArenaFightAnswerMessage message)
        {
            var popup = client.Character.ArenaPopup;

            if (popup == null)
                return;

            if (message.accept)
                popup.Accept();
            else
                popup.Deny();
        }

        [WorldHandler(GameRolePlayArenaRegisterMessage.Id)]
        public static void HandleGameRolePlayArenaRegisterMessage(WorldClient client, GameRolePlayArenaRegisterMessage message)
        {
            int Battle = message.arenaType == 1 ? 1 : client.Character.ArenaParty != null && client.Character.ArenaParty.MembersCount == 3 ? 3 : 2;

            if (Battle == 1 || Battle == 2)
            {
                ArenaManager.Instance.AddToQueue(client.Character, Battle);
            }
            else
            {
                if (client.Character.IsPartyLeader(client.Character.ArenaParty.Id))
                {
                    ArenaManager.Instance.AddToQueue(client.Character.ArenaParty);
                }
                else
                {
                    client.Character.SendServerMessageLang("Apenas o Líder do Koliseu pode fazer a inscrição de sua equipe.", "Only Kolossium Leader can register your group.", "Solo jefe del koliseo puede inscribir a tu grupo.", "Seul chef de Kolizéum peut inscrire votre groupe.");
                    return;
                }
            }
        }

        [WorldHandler(GameRolePlayArenaUnregisterMessage.Id)]
        public static void HandleGameRolePlayArenaUnregisterMessage(WorldClient client, GameRolePlayArenaUnregisterMessage message)
        {
            if (client.Character.ArenaParty != null)
            {
                if (client.Character.IsPartyLeader(client.Character.ArenaParty.Id))
                {
                    ArenaManager.Instance.RemoveFromQueue(client.Character.ArenaParty);
                }
            }
            else
            {
                ArenaManager.Instance.RemoveFromQueue(client.Character);
            }
        }   
        
        public static void SendGameRolePlayArenaFightPropositionMessage(IPacketReceiver client, ArenaPopup popup, int delay)
        {
            var members = popup.Team.Members.Select(x => (double)x.Character.Id);
            client.Send(new GameRolePlayArenaFightPropositionMessage((ushort)popup.Team.Fight.Id, members, (ushort)delay));
        }

        public static void SendGameRolePlayArenaFighterStatusMessage(IPacketReceiver client, int fightId, Character character, bool accepted)
        {
            client.Send(new GameRolePlayArenaFighterStatusMessage((ushort)fightId, (ulong)character.Id, accepted));
        }

        public static void SendGameRolePlayArenaRegistrationStatusMessage(IPacketReceiver client, bool registred, PvpArenaStepEnum step, PvpArenaTypeEnum type)
        {
            client.Send(new GameRolePlayArenaRegistrationStatusMessage(registred, (sbyte)step, (sbyte)type));
        }

        public static void SendTeleportToBuddyOfferMessage(IPacketReceiver client, ushort dungid,ulong buddy, int delay)
        {          
            client.Send(new TeleportToBuddyOfferMessage(dungid, buddy, (ushort)delay));
        }

        //public static void SendTeleportToBuddyAnswerMessage(IPacketReceiver client, ushort dungid, ulong buddy, bool accept)
        //{
        //    client.Send(new TeleportToBuddyAnswerMessage(dungid, buddy, accept));
        //}

        [WorldHandler(TeleportToBuddyAnswerMessage.Id)]
        public static void SendTeleportToBuddyAnswerMessage(WorldClient client, TeleportToBuddyAnswerMessage message)
        {
            var popup = client.Character.DungPopup;

            if (popup == null)
                return;

            if (message.accept)
                popup.Accept();
            else
                popup.Deny();
        }

        public static void SendTeleportToBuddyCloseMessage(IPacketReceiver client, ushort dungid, ulong buddy)
        {
            client.Send(new TeleportToBuddyCloseMessage(dungid, buddy));
        }
    }
}