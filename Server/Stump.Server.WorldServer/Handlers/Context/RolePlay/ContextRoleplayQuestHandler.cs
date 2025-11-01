using Stump.Core.Extensions;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Messages;
using Stump.DofusProtocol.Types;
using Stump.Server.BaseServer.Network;
using Stump.Server.WorldServer.Core.Network;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Npcs;
using Stump.Server.WorldServer.Game.Quests;
using System.Collections.Generic;
using System.Linq;

namespace Stump.Server.WorldServer.Handlers.Context.RolePlay
{
    public partial class ContextRoleplayHandler
    {
        [WorldHandler(QuestObjectiveValidationMessage.Id)]
        public static void HandleQuestObjectiveValidationMessage(WorldClient client, QuestObjectiveValidationMessage message)
        {
            var quest = client.Character.Quests.FirstOrDefault(x => x.Id == message.questId && !x.Finished);

            if (quest == null)
                return;

            var objective = quest.CurrentStep.Objectives.FirstOrDefault(x => !x.Finished && x.ObjectiveRecord.ObjectiveId == message.objectiveId);

            if (objective != null && objective.Template.Type == QuestObjectiveTypeEnum.NONE)
            {
                objective.CompleteObjective();
            }

            if (quest != null && objective != null)
            {
                if (quest.Id == 489 && objective.Template.Id == 3510)
                    quest.CurrentStep.Objectives.FirstOrDefault(x => !x.Finished && x.ObjectiveRecord.ObjectiveId == 3511).CompleteObjective();
            }
        }

        [WorldHandler(QuestListRequestMessage.Id)]
        public static void HandleQuestListRequestMessage(WorldClient client, QuestListRequestMessage message)
        {
            client.Send(new QuestListMessage(client.Character.Quests.Where(x => x.Finished).Select(x => x.Id).ToArray(),
                new ushort[client.Character.Quests.Select(x => x.Finished).Count()], client.Character.Quests.Where(x => !x.Finished).Select(x => x.GetQuestActiveInformations()).ToArray(), new ushort[0]));
        }

        public static void SendQuestListMessage(IPacketReceiver client)
        {
            client.Send(new QuestListMessage(new ushort[0], new ushort[0], new QuestActiveInformations[0], new ushort[0]));
        }

        [WorldHandler(QuestStepInfoRequestMessage.Id)]
        public static void HandleQuestStepInfoRequestMessage(WorldClient client, QuestStepInfoRequestMessage message)
        {
            var quest = client.Character.Quests.FirstOrDefault(x => x.Id == message.questId);

            if (quest == null)
                return;

            if (quest.Finished)
                client.Send(new QuestStepInfoMessage(new QuestActiveInformations(quest.Id)));
            else
                client.Send(new QuestStepInfoMessage(quest.GetQuestActiveInformations()));
        }

        //Version 2.61 Corrigir
        public static void SendRefreshMapQuestWithout(WorldClient client, int questId)
        {
            var questflag = new GameRolePlayNpcQuestFlag[0];

            questflag.Add(new GameRolePlayNpcQuestFlag(new[] { (ushort)questId }, new ushort[0]));

            var npcs = client.Character.Map.Actors.Where(x => x is Npc);
            var mapQuestInfo = new MapNpcQuestInfo(client.Character.Map.Id, new int[0], questflag);

            client.Send(new ListMapNpcsQuestStatusUpdateMessage(new[] { mapQuestInfo })); //revisar esto
        }

        public static void SendQuestStepValidatedMessage(WorldClient client, ushort questId, ushort stepId)
        {
            client.Send(new QuestStepValidatedMessage(questId, stepId));
        }

        [WorldHandler(QuestStartRequestMessage.Id)]
        public static void HandleQuestStartRequestMessage(WorldClient client, QuestStartRequestMessage message)
        {
            if (client.Character.HasQuestById(message.questId))
                return;

            var questStep = QuestManager.Instance.TryGetQuestStepByQuestId(message.questId);

            if (questStep is null)
                return;

            client.Character.StartQuest(questStep.Id);
        }
    }
}