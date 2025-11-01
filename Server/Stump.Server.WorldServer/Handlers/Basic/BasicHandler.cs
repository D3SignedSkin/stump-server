using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Stump.Core.Extensions;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Messages;
using Stump.DofusProtocol.Types;
using Stump.Server.BaseServer.Network;
using Stump.Server.WorldServer.Core.Network;
using Stump.Server.WorldServer.Game;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;

namespace Stump.Server.WorldServer.Handlers.Basic
{
    public class BasicHandler : WorldHandlerContainer
    {
        private static ushort MaxLatencyStats = 50;

        protected static readonly Logger logger = LogManager.GetCurrentClassLogger();

        [WorldHandler(NumericWhoIsRequestMessage.Id)] // TODO
        public static void HandleBasicLatencyStatsRequestMessage(WorldClient client, BasicLatencyStatsRequestMessage message)
        {
            //client.SequenceNumber++;

            //client.Send(new SequenceNumberMessage(client.SequenceNumber));
            //client.Send(new BasicLatencyStatsMessage(200, 1, MaxLatencyStats));

            //Oficial Message
            //[13:18:48:070] [ServerConnection][server_game][RCV] BasicLatencyStatsRequestMessage, id: 6273
            //[13:18:48:109][ServerConnection][server_game][SND] > SequenceNumberMessage, id: 1188 number = 2
            //[13:18:48:111] [ServerConnection] [server_game] [SND] > BasicLatencyStatsMessage, id: 7194 max = 50 latency = 174 sampleCount = 9
        }

        [WorldHandler(BasicWhoAmIRequestMessage.Id)]
        public static void HandleBasicWhoAmIRequestMessage(WorldClient client, BasicWhoAmIRequestMessage message)
        {
            try
            {
                /* Get Current character */
                var character = client.Character;
                var accountTag = new AccountTagInformation(character.Client.WorldAccount.Nickname, character.Account.Id.ToString());

                /* Send informations about it */
                client.Send(new BasicWhoIsMessage(
                    character == client.Character,
                    message.verbose,
                    (sbyte)character.UserGroup.Role,
                    accountTag,
                    character.Account.Id,
                    character.Name,
                    (ulong)character.Id,
                    (short)character.Map.Area.Id,
                    (short)WorldServer.ServerInformation.Id,
                    (short)WorldServer.ServerInformation.Id,
                    character.GuildMember == null ? new AbstractSocialGroupInfos[0] : new[] { character.Guild.GetBasicGuildInformations() },
                    character.IsInFight() ? (sbyte)PlayerStateEnum.GAME_TYPE_FIGHT : (sbyte)PlayerStateEnum.GAME_TYPE_ROLEPLAY));
            }
            catch
            {
                logger.Error("Error HandleBasicWhoAmIRequestMessage ");
                logger.Error("Error HandleBasicWhoAmIRequestMessage  client.Character.ID:" + client.Character.Id + " client.Character.name" + client.Character.Name);
            }
        }

        [WorldHandler(BasicWhoIsRequestMessage.Id)]
        public static void HandleBasicWhoIsRequestMessage(WorldClient client, BasicWhoIsRequestMessage message)
        {
            /* Get character */
            var playerSearch = (PlayerSearchCharacterNameInformation)message.target;
            var character = World.Instance.GetCharacter(playerSearch.name);

            /* check null */
            if (character == null)
            {
                client.Send(new BasicWhoIsNoMatchMessage(new PlayerSearchCharacterNameInformation(playerSearch.name)));
            }
            else
            {
                try
                {
                    var accountTag = new AccountTagInformation(character.Client.WorldAccount.Nickname, character.Account.Id.ToString());

                    client.Send(new BasicWhoIsMessage(
                        character == client.Character,
                        message.verbose,
                        (sbyte)character.UserGroup.Role,
                        accountTag,
                        character.Account.Id,
                        character.Name,
                        (ulong)character.Id,
                        (short)character.Map.SubArea.Id,
                        (short)WorldServer.ServerInformation.Id,
                        (short)WorldServer.ServerInformation.Id,
                        character.GuildMember == null ? new AbstractSocialGroupInfos[0] : new[] { character.Guild.GetBasicGuildInformations() },
                        character.IsInFight() ? (sbyte)PlayerStateEnum.GAME_TYPE_FIGHT : (sbyte)PlayerStateEnum.GAME_TYPE_ROLEPLAY));

                }
                catch
                {
                    logger.Error("Error HandleBasicWhoIsRequestMessage ");
                    logger.Error("Error HandleBasicWhoIsRequestMessage  client.Character.ID:" + client.Character.Id + " client.Character.name" + client.Character.Name);
                }
            }
        }

        [WorldHandler(NumericWhoIsRequestMessage.Id)]
        public static void HandleNumericWhoIsRequestMessage(WorldClient client, NumericWhoIsRequestMessage message)
        {
            /* Get character */
            var character = World.Instance.GetCharacter((int)message.playerId);

            /* check null */
            if (character != null)
            {
                /* Send info about it */
                client.Send(new NumericWhoIsMessage((ulong)character.Id, character.Account.Id));
            }
        }

        public static void SendTextInformationMessage(IPacketReceiver client, TextInformationTypeEnum msgType, short msgId, params string[] arguments)
        {
            client.Send(new TextInformationMessage((sbyte)msgType, (ushort)msgId, arguments));
        }

        public static void SendTextInformationMessage(IPacketReceiver client, TextInformationTypeEnum msgType, short msgId, params object[] arguments)
        {
            client.Send(new TextInformationMessage((sbyte)msgType, (ushort)msgId, arguments.Select(entry => entry.ToString())));
        }

        public static void SendTextInformationMessage(IPacketReceiver client, TextInformationTypeEnum msgType, short msgId)
        {
            client.Send(new TextInformationMessage((sbyte)msgType, (ushort)msgId, new string[0]));
        }

        public static void SendSystemMessageDisplayMessage(IPacketReceiver client, bool hangUp, short msgId, IEnumerable<string> arguments)
        {
            client.Send(new SystemMessageDisplayMessage(hangUp, (ushort)msgId, arguments));
        }

        public static void SendSystemMessageDisplayMessage(IPacketReceiver client, bool hangUp, short msgId, params object[] arguments)
        {
            client.Send(new SystemMessageDisplayMessage(hangUp, (ushort)msgId, arguments.Select(entry => entry.ToString())));
        }

        public static void SendBasicTimeMessage(IPacketReceiver client)
        {
            var offset = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).TotalMinutes;
            client.Send(new BasicTimeMessage(DateTime.Now.GetUnixTimeStampDouble(), (short)offset));
        }

        public static void SendBasicNoOperationMessage(IPacketReceiver client)
        {
            client.Send(new BasicNoOperationMessage());
        }

        public static void SendBasicTime(IPacketReceiver client, short offset)
        {
            client.Send(new BasicTimeMessage(DateTime.Now.GetUnixTimeStampLong(), offset));
        }

        public static void SendCinematicMessage(IPacketReceiver client, short cinematicId)
        {
            client.Send(new CinematicMessage((ushort)cinematicId));
        }

        public static void SendServerExperienceModificatorMessage(IPacketReceiver client, Character character)
        {
            List<(double, RoleEnum)> BaseRates = new List<(double, RoleEnum)>
            {
                (Rates.XpRate, RoleEnum.None),
                (Rates.XpRate, RoleEnum.Player),
                (Rates.VipXpRate, RoleEnum.Vip),
                (Rates.GoldXpRate, RoleEnum.Gold_Vip),
                (Rates.GoldXpRate, RoleEnum.Moderator_Helper),
                (Rates.GoldXpRate, RoleEnum.GameMaster_Padawan),
                (Rates.GoldXpRate, RoleEnum.GameMaster),
                (Rates.GoldXpRate, RoleEnum.Administrator),
                (Rates.GoldXpRate, RoleEnum.Non_ADM),
                (Rates.GoldXpRate, RoleEnum.Developer),
            };

            ushort rate = (ushort)BaseRates.FirstOrDefault(x => x.Item2 == character.UserGroup.Role).Item1;

            client.Send(new ServerExperienceModificatorMessage(rate));
        }
    }
}