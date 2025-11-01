using System.Collections.Generic;
using System.Linq;
using Stump.Core.Reflection;
using Stump.DofusProtocol.Messages;
using Stump.DofusProtocol.Types;
using Stump.Server.BaseServer.Network;
using Stump.Server.WorldServer.Core.Network;
using Stump.Server.WorldServer.Database.Achievements;
using Stump.Server.WorldServer.Game.Accounts.Startup;
using Stump.Server.WorldServer.Game.Achievements;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;

namespace Stump.Server.WorldServer.Handlers.Achievements
{
    public class AchievementHandler : WorldHandlerContainer
    {
        private AchievementHandler()
        { }

        #region >> World Handler
        [WorldHandler(AchievementDetailedListRequestMessage.Id)]
        public static void HandleAchievementDetailedListRequestMessage(WorldClient client, AchievementDetailedListRequestMessage message)
        {
            var category = Singleton<AchievementManager>.Instance.TryGetAchievementCategory(message.categoryId);

            if (category != null)
                SendAchievementDetailedListMessage(client, new Achievement[0], client.Character.Achievement.TryGetFinishedAchievements(category));
        }

        [WorldHandler(AchievementDetailsRequestMessage.Id)]
        public static void HandleAchievementDetailsRequestMessage(WorldClient client, AchievementDetailsRequestMessage message)
        {
            var template = Singleton<AchievementManager>.Instance.TryGetAchievement(message.achievementId);

            if (template != null)
                SendAchievementDetailsMessage(client, template.GetAchievement(client.Character.Achievement));
        }

        [WorldHandler(ConsumeAllGameActionItemMessage.Id)]
        public static void HandleConsumeAllGameActionItemMessage(WorldClient client, ConsumeAllGameActionItemMessage message)
        {
            if (client.LastMessage is AchievementRewardRequestMessage)
            {
                client.Character.Achievement.RewardAllAchievements((achievement, success) =>
                {
                    if (success)
                        SendAchievementRewardSuccessMessage(client, (short)achievement.Id);
                    else
                        SendAchievementRewardErrorMessage(client, (short)achievement.Id);
                });
            }
        }

        [WorldHandler(AchievementRewardRequestMessage.Id)]
        public static void HandleAchievementRewardRequestMessage(WorldClient client, AchievementRewardRequestMessage message)
        {
            if (message.achievementId > 0)
            {
                var achievement = client.Character.Achievement.TryGetFinishedAchievement(message.achievementId);

                if (achievement != null)
                {
                    if (client.Character.Achievement.RewardAchievement(achievement))
                    {
                        SendAchievementRewardSuccessMessage(client, message.achievementId);

                        if (client.Character.Record.UnAcceptedAchievements.Contains((ushort)achievement.Id))
                        {
                            client.Character.Record.UnAcceptedAchievements.Remove((ushort)achievement.Id);
                        }
                    }
                    else
                    {
                        SendAchievementRewardErrorMessage(client, message.achievementId);
                    }
                }
                else
                {
                    SendAchievementRewardErrorMessage(client, message.achievementId);
                }
            }
            else if (message.achievementId == -1)
            {
                List<AchievementTemplate> Achievements = new List<AchievementTemplate>();

                foreach (var achiev in client.Character.Achievement.FinishedAchievements)
                {
                    Achievements.Add(achiev);
                }

                foreach (var Achievement in Achievements)
                {
                    var achievement = client.Character.Achievement.TryGetFinishedAchievement((short)Achievement.Id);

                    if (achievement != null)
                    {
                        if (client.Character.Achievement.RewardAchievement(achievement))
                        {
                            SendAchievementRewardSuccessMessage(client, (short)Achievement.Id);

                            if (client.Character.Record.UnAcceptedAchievements.Contains((ushort)achievement.Id))
                            {
                                client.Character.Record.UnAcceptedAchievements.Remove((ushort)achievement.Id);
                            }
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        SendAchievementRewardErrorMessage(client, (short)Achievement.Id);
                    }
                }
            }
            else if (message.MessageId == 6676) //Recebimento de presentes através da interface dos sucessos by:Kenshin
            {
                if (client.Account == null || client.StartupActions == null)
                    return;

                var actions = client.StartupActions;

                if (actions == null)
                    return;

                var character = client.Characters.FirstOrDefault(entry => entry.Id == client.Character.Id);

                if (character == null)
                    return;

                List<StartupAction> ActionsList = new List<StartupAction>();

                foreach (var action in actions)
                {
                    if (action != null)
                    {
                        ActionsList.Add(action);
                    }
                }

                foreach (var Action in ActionsList)
                {
                    if (Action != null)
                    {
                        Action.GiveGiftTo(client, character, Action);
                    }
                }
            }
            else
            {
                SendAchievementRewardErrorMessage(client, (short)Achievement.Id);
            }
        }
        #endregion

        public static void SendAchievementListMessage(IPacketReceiver client, IEnumerable<ushort> finishedAchievementsIds, Character character)
        {
            var AchievementsAchieved = new List<AchievementAchieved>();

            foreach (var finishedAchievements in finishedAchievementsIds)
            {
                AchievementsAchieved.Add(new AchievementAchieved(finishedAchievements, (ulong)character.Id, 1));
            }

            client.Send(new AchievementListMessage(AchievementsAchieved.ToArray()));
        }

        public static void SendAchievementDetailedListMessage(IPacketReceiver client, IEnumerable<Achievement> startedAchievements, IEnumerable<Achievement> finishedAchievements)
        {
            client.Send(new AchievementDetailedListMessage(startedAchievements.ToArray(), finishedAchievements.ToArray()));
        }

        public static void SendAchievementDetailsMessage(IPacketReceiver client, Achievement achievement)
        {
            client.Send(new AchievementDetailsMessage(achievement));
        }

        public static void SendAchievementFinishedMessage(IPacketReceiver client, ushort id, ulong charId, ushort finishedLevel)
        {
            if (finishedLevel > 200)
                finishedLevel = 200;

            client.Send(new AchievementFinishedMessage(new AchievementAchievedRewardable(id, (uint)charId, 1, finishedLevel)));
        }

        public static void SendAchievementRewardSuccessMessage(IPacketReceiver client, short achievementId)
        {
            client.Send(new AchievementRewardSuccessMessage(achievementId));
        }

        public static void SendAchievementRewardErrorMessage(IPacketReceiver client, short achievementId)
        {
            client.Send(new AchievementRewardErrorMessage(achievementId));
        }
    }
}