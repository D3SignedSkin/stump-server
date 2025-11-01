using System.Collections.Generic;
using Stump.DofusProtocol.Messages;
using Stump.Server.BaseServer.Network;
using Stump.Server.WorldServer.Core.Network;
using Stump.Server.WorldServer.Game.Accounts.Startup;
using System.Linq;
using Stump.Server.BaseServer;

namespace Stump.Server.WorldServer.Handlers.Startup
{
    public class StartupHandler : WorldHandlerContainer
    {
        public static ORM.Database OrmDatabase = ServerBase<WorldServer>.Instance.DBAccessor.Database;

        //[WorldHandler(StartupActionsExecuteMessage.Id, IsGamePacket = false, ShouldBeLogged = false, IgnorePredicate = true)]
        //public static void HandleStartupActionsExecuteMessage(WorldClient client, StartupActionsExecuteMessage message)
        //{
        //    if (client.Account == null)
        //        return;

        //    WorldServer.Instance.IOTaskPool.ExecuteInContext(() =>
        //    {
        //        var source = Singleton<ItemManager>.Instance.FindStartupActions(client.Account.Id);

        //        var actions = source.Select(startupActionRecord => new StartupAction(startupActionRecord)).ToList();

        //        //  actions.AddRange(VetRewardsManager.Instance.GetStartupActions(client));

        //        client.StartupActions = actions;

        //        SendGameActionItemListMessage(client, actions);
        //    });
        //}

        //Por Algum motivo usar apenas a ID deixa o achiviments funcionar ingame e não deixa a opção do menu de personagem funcionar
        [WorldHandler(ConsumeGameActionItemMessage.Id, IsGamePacket = false, ShouldBeLogged = false, IgnorePredicate = true)]
        public static void HandleConsumeGameActionItemMessage(WorldClient client, ConsumeGameActionItemMessage message)
        {
            if (client.Account == null || client.StartupActions == null)
                return;

            var action = client.StartupActions.FirstOrDefault(entry => entry.Id == message.actionId);

            if (action == null)
                return;

            var character = client.Characters.FirstOrDefault(entry => entry.Id == (int)message.characterId);

            if (character == null)
                return;

            action.GiveGiftTo(client, character, action);
        }

        public static void SendGameActionItemListMessage(IPacketReceiver client, IEnumerable<StartupAction> actions)
        {
            client.Send(new GameActionItemListMessage(from entry in actions select entry.GetStartupActionAddObject()));
        }

        public static void SendGameActionItemConsumedMessage(IPacketReceiver client, StartupAction action, bool success)
        {
            client.Send(new GameActionItemConsumedMessage(success, true, action.Id));
        }
    }
}
