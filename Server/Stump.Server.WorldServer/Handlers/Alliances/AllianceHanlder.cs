using Stump.Core.Reflection;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Enums.Custom;
using Stump.DofusProtocol.Messages;
using Stump.DofusProtocol.Types;
using Stump.Server.BaseServer.Network;
using Stump.Server.WorldServer.Core.Network;
using Stump.Server.WorldServer.Game;
using Stump.Server.WorldServer.Game.Alliances;
using Stump.Server.WorldServer.Game.Guilds;
using System.Linq;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using System;
using Stump.Server.WorldServer.Game.Dialogs.Alliances;
using Stump.Core.Extensions;
using Stump.Server.WorldServer.Handlers.TaxCollector;
using MongoDB.Bson;
using Stump.Server.BaseServer.Logging;
using System.Globalization;
using Stump.Server.WorldServer.Game.Dialogs.Guilds;

namespace Stump.Server.WorldServer.Handlers.Alliances
{
    public class AllianceHandler : WorldHandlerContainer
    {
        private AllianceHandler() { }

        #region >> Alliance Creation/Modifications

        [WorldHandler(AllianceCreationValidMessage.Id)]
        public static void HandleAllianceCreationValidMessage(WorldClient client, AllianceCreationValidMessage message)
        {
            var allianceCreationPanel = client.Character.Dialog as AllianceCreationPanel;

            allianceCreationPanel?.CreateAlliance(message.allianceName, message.allianceTag, message.allianceEmblem);
        }

        [WorldHandler(AllianceModificationValidMessage.Id)]
        public static void HandleAllianceModificationValidMessage(WorldClient client, AllianceModificationValidMessage message)
        {
            var panel = client.Character.Dialog as AllianceModificationPanel;

            panel?.ModifyAllianceName(message.allianceName, message.allianceTag);
            panel?.ModifyAllianceEmblem(message.allianceEmblem);
        }

        [WorldHandler(AllianceModificationNameAndTagValidMessage.Id)]
        public static void HandleGuildModificationNameValidMessage(WorldClient client, AllianceModificationNameAndTagValidMessage message)
        {
            var panel = client.Character.Dialog as AllianceModificationPanel;

            panel?.ModifyAllianceName(message.allianceName, message.allianceTag);
        }

        [WorldHandler(AllianceModificationEmblemValidMessage.Id)]
        public static void HandleAllianceModificationEmblemValidMessage(WorldClient client, AllianceModificationEmblemValidMessage message)
        {
            (client.Character.Dialog as AllianceModificationPanel)?.ModifyAllianceEmblem(message.allianceEmblem);
        }

        [WorldHandler(AllianceChangeMemberRankMessage.Id)]
        public static void HandleAllianceChangeGuildRightsMessage(WorldClient client, AllianceChangeMemberRankMessage message)
        {
            WorldServer.Instance.IOTaskPool.ExecuteInContext(() =>
            {
                if (client.Character.Guild == null)
                    return;
                if (client.Character.GuildMember == null)
                    return;
                if (client.Character.Guild.Alliance == null)
                    return;
                var alliance = Singleton<AllianceManager>.Instance.TryGetAlliance(client.Character.Guild.Alliance.Id);
                if (alliance == null)
                    return;
                if (alliance.Boss != client.Character.Guild)
                    return;

                var target = alliance.GetGuildById((uint)message.memberId);
                if (target == null)
                    return;
                if (client.Character.GuildMember.RankId > 1 || message.rankId > 1) // idk message..rights????
                    return;

                alliance.SetBoss(target);            //(client.Character.Guild?.Alliance.Clients
                                                     //SendAllianceInsiderInfoMessage(client, client.Character.Guild?.Alliance);
                                                     //SendAllianceMembershipMessage(client, client.Character.Guild?.Alliance, false);

                SendAllianceInsiderInfoMessage(client.Character.Guild?.Alliance.Clients, client.Character.Guild?.Alliance);
                client.Character.Guild?.Alliance.Save(WorldServer.Instance.DBAccessor.Database);
                //SendAllianceFactsMessage(client, client.Character.Guild?.Alliance);
                //SendAllianceJoinedMessage(client, alliance,true);
            });
        }

        [WorldHandler(AllianceKickRequestMessage.Id)]
        public static void HandleAllianceKickRequestMessage(WorldClient client, AllianceKickRequestMessage message)
        {
            WorldServer.Instance.IOTaskPool.ExecuteInContext(() =>
            {
                if (client.Character.Guild == null)
                    return;

                if (client.Character.GuildMember == null)
                    return;

                if (client.Character.Guild.Alliance == null)
                    return;

                var alliance = Singleton<AllianceManager>.Instance.TryGetAlliance((int)client.Character.Guild.Alliance.Id);

                if (alliance == null)
                    return;

                var target = alliance.GetGuildById((uint)message.kickedId);

                if (target == null)
                    return;

                if (alliance.Boss != client.Character.Guild)
                {
                    if (target != client.Character.Guild)
                        return;
                }

                if (alliance.Boss.Boss != client.Character.GuildMember)// if are alliance boss
                {
                    if (target.Boss != client.Character.GuildMember) // also if are boss guild
                    {
                        return;
                    }
                    else
                    {
                        alliance.Clients.Send(new AllianceMemberLeavingMessage(false, (uint)target.Id));
                    }
                }
                else if (alliance.Boss != target)
                {
                    alliance.Clients.Send(new AllianceMemberLeavingMessage(true, (uint)target.Id));
                }
                else
                {
                    alliance.Clients.Send(new AllianceMemberLeavingMessage(false, (uint)target.Id));

                }

                alliance.KickGuild(target);
                SendAllianceInsiderInfoMessage(alliance.Clients, alliance);
                target.Save(WorldServer.Instance.DBAccessor.Database);
                alliance.Save(WorldServer.Instance.DBAccessor.Database);

                #region >> MongoLogs
                var document = new BsonDocument
                                    {
                                        { "allianceId", alliance.Id },
                                        { "allianceName", alliance.Name },
                                        { "guildId", target.Id },
                                        { "guildName",  target.Name },
                                        { "removedbyId", client.Character.Id},
                                        { "removedbyname", client.Character.Name},

                                        { "Date", DateTime.Now.ToString(CultureInfo.CreateSpecificCulture("pt-BR")) }
                                    };
                MongoLogger.Instance.Insert("AllianceKickGuild", document);
                #endregion
            });
        }

        #endregion

        #region >> Alliance Invitation

        [WorldHandler(AllianceInvitationAnswerMessage.Id)]
        public static void HandleAllianceInvitationAnswerMessage(WorldClient client, AllianceInvitationAnswerMessage message)
        {
            var request = client.Character.RequestBox as AllianceInvitationRequest;

            if (request == null)
                return;

            if (client.Character == request.Source && !message.accept)
            {
                request.Cancel();
            }
            else if (client.Character == request.Target)
            {
                if (message.accept)
                    request.Accept();
                else
                    request.Deny();
            }
        }

        [WorldHandler(AllianceInvitationMessage.Id)]
        public static void HandleAllianceInvitationMessage(WorldClient client, AllianceInvitationMessage message)
        {
            Console.WriteLine(message.targetId);

            if (client.Character.Guild?.Alliance != null)
            {
                if (!client.Character.GuildMember.HasRight(GuildRightsBitEnum.GUILD_RIGHT_INVITE_NEW_MEMBERS)) // ALLIANCE_RIGHT_RECRUIT_GUILDS = 8
                {
                    client.Character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 207); //TODO: Explore messages to send correctly ids
                }
                else
                {
                    var character = Singleton<World>.Instance.GetCharacter((int)message.targetId);

                    if (character == null)
                    {
                        client.Character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 208);
                    }
                    else
                    {
                        if (character.Guild == null || character.Guild.Alliance != null || !character.GuildMember.IsBoss)
                        {
                            client.Character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 206);
                        }
                        else
                        {
                            if (character.IsBusy())
                            {
                                client.Character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 209);
                            }
                            else
                            {
                                AllianceInvitationRequest guildInvitationRequest = new AllianceInvitationRequest(client.Character, character);
                                guildInvitationRequest.Open();
                            }
                        }
                    }
                }
            }
        }
        #endregion

        [WorldHandler(GuildFactsRequestMessage.Id)]
        public static void HandleGuildFactsRequestMessage(WorldClient client, GuildFactsRequestMessage message)
        {
            var guild = Singleton<GuildManager>.Instance.TryGetGuild((int)message.guildId);

            if (guild != null)
            {
                SendGuildFactsMessage(client, guild);
            }
        }

        [WorldHandler(AllianceInsiderInfoRequestMessage.Id)]
        public static void HandleAllianceInsiderInfoRequestMessage(WorldClient client, AllianceInsiderInfoRequestMessage message)
        {
            if (client.Character.Guild?.Alliance != null)
                SendAllianceInsiderInfoMessage(client, client.Character.Guild.Alliance);
        }

        [WorldHandler(AllianceFactsRequestMessage.Id)]
        public static void HandleAllianceFactsRequestMessage(WorldClient client, AllianceFactsRequestMessage message)
        {
            var alliance = Singleton<AllianceManager>.Instance.TryGetAlliance((int)message.allianceId);

            if (alliance != null)
                SendAllianceFactsMessage(client, alliance);
        }

        [WorldHandler(SetEnableAVARequestMessage.Id)]
        public static void HandleSetEnableAVARequestMessage(WorldClient client, SetEnableAVARequestMessage message)
        {
            //if combat zone TextInfo | type 1 | Id 339
            if (client.Character.SubArea.HasPrism)
            {
                if (client.Character.SubArea.Prism.State == PrismStateEnum.PRISM_STATE_VULNERABLE)
                {
                    client.Character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 339);
                    return;
                }
            }

            if (client.Character.Guild?.Alliance == null)
                return;

            client.Character.AvAActived = message.enable;

            //TODO
        }

        [WorldHandler(AllianceMotdSetRequestMessage.Id)]
        public static void HandleAllianceMotdSetRequestMessage(WorldClient client, AllianceMotdSetRequestMessage message)
        {
            if (client.Character.GuildMember == null)
            {
                SendAllianceMotdSetErrorMessage(client);
                return;
            }

            client.Character.Guild?.Alliance.UpdateMotd(client.Character.GuildMember, message.content);

            WorldServer.Instance.IOTaskPool.ExecuteInContext(() =>
            {
                client.Character.Guild?.Alliance.Save(WorldServer.Instance.DBAccessor.Database);
            });
        }

        [WorldHandler(AllianceBulletinSetRequestMessage.Id)]
        public static void HandleAllianceBulletinSetRequestMessage(WorldClient client, AllianceBulletinSetRequestMessage message)
        {
            if (client.Character.GuildMember == null)
            {
                SendAllianceBulletinSetErrorMessage(client, SocialNoticeErrorEnum.SOCIAL_NOTICE_UNKNOWN_ERROR);
                return;
            }

            if (client.Character.GuildMember.RankId > 1)
            {
                SendAllianceBulletinSetErrorMessage(client, SocialNoticeErrorEnum.SOCIAL_NOTICE_INVALID_RIGHTS);
                return;
            }

            client.Character.Guild.Alliance.UpdateBulletin(client.Character.GuildMember, message.content);

            WorldServer.Instance.IOTaskPool.ExecuteInContext(() =>
            {
                client.Character.Guild?.Alliance.Save(WorldServer.Instance.DBAccessor.Database);
            });
        }

        public static void SendAllianceModificationStartedMessage(IPacketReceiver client, bool changeName, bool changeTag, bool changeEmblem)
        {
            client.Send(new AllianceModificationStartedMessage(changeName, changeTag, changeEmblem));
        }

        public static void SendAllianceBulletinMessage(IPacketReceiver client, Alliance alliance)
        {
            string bulletinContent = alliance.BulletinContent is null ? string.Empty : alliance.BulletinContent;
            int bulletinDate = alliance.BulletinDate.GetUnixTimeStamp();
            ulong bulletinMemberId = alliance.BulletinMember is null ? 0 : (ulong)alliance.BulletinMember.Id;
            string bulletinMember = alliance.BulletinMember is null ? string.Empty : alliance.BulletinMember.Name;

            client.Send(new AllianceBulletinMessage(bulletinContent, bulletinDate, bulletinMemberId, bulletinMember));
        }

        internal static void SendAllianceInvitedMessage(WorldClient client, Character source)
        {
            client.Send(new AllianceInvitedMessage(source.Name, source.Guild.Alliance.GetAllianceInformations()));
        }

        internal static void SendAllianceInvitationStateRecrutedMessage(WorldClient client, GuildInvitationStateEnum gUILD_INVITATION_SENT)
        {
            client.Send(new AllianceInvitationStateRecrutedMessage((sbyte)gUILD_INVITATION_SENT));
        }

        public static void SendAllianceBulletinSetErrorMessage(IPacketReceiver client, SocialNoticeErrorEnum error)
        {
            client.Send(new AllianceBulletinSetErrorMessage((sbyte)error));
        }

        public static void SendAllianceMotdSetErrorMessage(IPacketReceiver client)
        {
            client.Send(new AllianceMotdSetErrorMessage(0));
        }

        public static void SendAllianceMotdMessage(IPacketReceiver client, Alliance alliance)
        {
            client.Send(new AllianceMotdMessage(alliance.MotdContent, alliance.MotdDate.GetUnixTimeStamp(), (ulong?)alliance.MotdMember?.Id ?? 0, alliance.MotdMember?.Name ?? "Unknown"));
        }

        public static void SendAllianceCreationStartedMessage(IPacketReceiver client)
        {

            client.Send(new AllianceCreationStartedMessage());
        }

        public static void SendAllianceCreationResultMessage(IPacketReceiver client, SocialGroupCreationResultEnum result)
        {
            client.Send(new AllianceCreationResultMessage((sbyte)result));
        }

        public static void SendAllianceInsiderInfoMessage(IPacketReceiver client, Alliance alliance)
        {
            client.Send(new AllianceInsiderInfoMessage(
                allianceInfos: alliance.GetAllianceFactSheetInformations(),
                members: alliance.GetAllianceMemberInfo(),
                prisms: alliance.GetPrismsInformations().ToArray(),
                taxCollectors: new TaxCollectorInformations[0]));

            foreach (var a in alliance.m_guilds)
            {
                TaxCollectorHandler.SendTaxCollectorListMessage(client, a.Value);
            }
        }

        public static void SendGuildFactsMessage(IPacketReceiver client, Guild guild)
        {
            client.Send(guild.GetGuildFactsMessage());
        }

        public static void SendAllianceJoinedMessage(IPacketReceiver client, Alliance alliance)
        {
            client.Send(new AllianceJoinedMessage(new AllianceInformation((uint)alliance.Id, alliance.Tag, alliance.Name, alliance.Emblem.GetNetworkGuildEmblem()), (uint)alliance.Boss.Id));
        }

        public static void SendAllianceMembershipMessage(IPacketReceiver client, Alliance alliance)
        {
            client.Send(new AllianceMembershipMessage(alliance.GetAllianceInformations(), (uint)alliance.Boss.Id));
        }

        public static void SendAllianceFactsMessage(IPacketReceiver client, Alliance alliance)
        {
            client.Send(new AllianceFactsMessage(
                infos: alliance.GetAllianceFactSheetInformations(),
                members: alliance.GetCharacterMinimalSocialPublicInformations(),
                controlledSubareaIds: alliance.Prisms.Select(x => (ushort)x.SubArea.Id).ToArray(),
                leaderCharacterId: (uint)alliance.Boss.Boss.Id,
                leaderCharacterName: alliance.Boss.Boss.Name));
        }

        internal static void SendAllianceInvitationStateRecruterMessage(WorldClient client, Character target, GuildInvitationStateEnum gUILD_INVITATION_SENT)
        {
            client.Send(new AllianceInvitationStateRecruterMessage(target.Name, (sbyte)gUILD_INVITATION_SENT));
        }
    }
}
