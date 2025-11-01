using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Stump.Core.Attributes;
using Stump.Core.IO;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Types;
using Stump.Server.BaseServer.Initialization;
using Stump.Server.BaseServer.Network;
using Stump.Server.WorldServer.Commands.Trigger;
using Stump.Server.WorldServer.Core.Network;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Actors.Interfaces;
using Stump.Server.WorldServer.Game.Actors.RolePlay;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Handlers.Chat;
using MongoDB.Bson;
using Stump.Server.BaseServer.Logging;
using System.Globalization;
using Stump.Server.BaseServer.Database;
using Stump.Server.WorldServer.Database;
using Stump.Server.WorldServer.Database.Social;
using System.Linq;
using Stump.Core.Extensions;
using Stump.Server.WorldServer.Discord;

namespace Stump.Server.WorldServer.Game.Social
{
    public class ChatManager : DataManager<ChatManager>, ISaveable
    {
        #region Delegates

        /// <summary>
        ///   Delegate for parsing incomming in game messages.
        /// </summary>
        public delegate void ChatParserDelegate(WorldClient client, string msg, IEnumerable<ObjectItem> objectItems);

        #endregion

        /// <summary>
        /// Prefix used for chat commands
        /// </summary>
        [Variable]
        public static readonly string CommandPrefix = ".";

        /// <summary>
        /// Minimal role level to access the admin chat
        /// </summary>
        [Variable]
        public static readonly RoleEnum AdministratorChatMinAccess = RoleEnum.Moderator_Helper;

        /// <summary>
        /// In milliseconds
        /// </summary>
        [Variable]
        public static int AntiFloodTimeBetweenTwoMessages = 500;

        /// <summary>
        /// In seconds
        /// </summary>
        [Variable]
        public static int AntiFloodTimeBetweenTwoGlobalMessages = 60;

        /// <summary>
        /// Amount of messages allowed in a given time
        /// </summary>
        [Variable]
        public static int AntiFloodAllowedMessages = 4;

        /// <summary>
        /// Time in seconds
        /// </summary>
        [Variable]
        public static int AntiFloodAllowedMessagesResetTime = 10;

        /// <summary>
        /// Time in seconds
        /// </summary>
        [Variable]
        public static int AntiFloodMuteTime = 10;

        public List<BadWordRecord> BadWords;

        /// <summary>
        ///   Chat handler for each channel Id.
        /// </summary>
        public readonly Dictionary<ChatActivableChannelsEnum, ChatParserDelegate> ChatHandlers = new Dictionary<ChatActivableChannelsEnum, ChatParserDelegate>();

        private Dictionary<uint, Emote> m_emotes = new Dictionary<uint, Emote>();

        public IReadOnlyDictionary<uint, Emote> Emotes => new ReadOnlyDictionary<uint, Emote>(m_emotes);

        [Initialization(InitializationPass.First)]
        public void Initialize()
        {
            ChatHandlers.Clear();

            ChatHandlers.Add(ChatActivableChannelsEnum.CHANNEL_GLOBAL, SayGlobal);
            ChatHandlers.Add(ChatActivableChannelsEnum.CHANNEL_GUILD, SayGuild);
            ChatHandlers.Add(ChatActivableChannelsEnum.CHANNEL_ALLIANCE, SayAlliance);
            ChatHandlers.Add(ChatActivableChannelsEnum.CHANNEL_PARTY, SayParty);
            ChatHandlers.Add(ChatActivableChannelsEnum.CHANNEL_ARENA, SayArena);
            ChatHandlers.Add(ChatActivableChannelsEnum.CHANNEL_SALES, SaySales);
            ChatHandlers.Add(ChatActivableChannelsEnum.CHANNEL_SEEK, SaySeek);
            ChatHandlers.Add(ChatActivableChannelsEnum.CHANNEL_ADMIN, SayAdministrators);
            ChatHandlers.Add(ChatActivableChannelsEnum.CHANNEL_VIP, SayVip);
            ChatHandlers.Add(ChatActivableChannelsEnum.CHANNEL_TEAM, SayTeam);

            BadWords = Database.Query<BadWordRecord>(BadWordRelator.FetchQuery).ToList();
            m_emotes = Database.Query<Emote>(EmoteRelator.FetchQuery).ToDictionary(x => x.Id);

            World.Instance.RegisterSaveableInstance(this);
        }

        public string CanSendMessage(string message)
        {
            foreach (var badWord in BadWords)
            {
                if (message.ToLower().RemoveWhitespace().Contains(badWord.Text.ToLower()))
                    return badWord.Text;
            }

            return string.Empty;
        }

        public bool CanUseChannel(Character character, ChatActivableChannelsEnum channel)
        {
            switch (channel)
            {
                case ChatActivableChannelsEnum.CHANNEL_GLOBAL:
                    {
                        if (character.Map.IsMuted && character.UserGroup.Role <= AdministratorChatMinAccess)
                        {

                            switch (character.Account.Lang)
                            {
                                case "fr":
                                    character.SendServerMessage("La map est actuellement réduite au silence !");
                                    break;
                                case "es":
                                    character.SendServerMessage("¡El mapa está actualmente silenciado!");
                                    break;
                                case "en":
                                    character.SendServerMessage("The map is currently silenced!");
                                    break;
                                default:
                                    character.SendServerMessage("O mapa está atualmente silenciado!");
                                    break;
                            }
                            return false;
                        }
                        return true;
                    }

                case ChatActivableChannelsEnum.CHANNEL_TEAM:
                    return character.IsFighting();
                case ChatActivableChannelsEnum.CHANNEL_ARENA:
                    return character.IsInParty(PartyTypeEnum.PARTY_TYPE_ARENA);
                case ChatActivableChannelsEnum.CHANNEL_GUILD:
                    return character.Guild != null;
                case ChatActivableChannelsEnum.CHANNEL_ALLIANCE:
                    return character.Guild?.Alliance != null;
                case ChatActivableChannelsEnum.CHANNEL_PARTY:
                    return character.IsInParty(PartyTypeEnum.PARTY_TYPE_CLASSICAL);
                case ChatActivableChannelsEnum.CHANNEL_SALES:
                    return !character.IsMuted();
                case ChatActivableChannelsEnum.CHANNEL_SEEK:
                    return !character.IsMuted();
                case ChatActivableChannelsEnum.CHANNEL_NOOB:
                    return true;
                case ChatActivableChannelsEnum.CHANNEL_ADMIN:
                    return character.UserGroup.Role >= AdministratorChatMinAccess;
                case ChatActivableChannelsEnum.CHANNEL_VIP:
                    return !character.IsMuted() && character.Account.IsSubscribe;
                case ChatActivableChannelsEnum.PSEUDO_CHANNEL_PRIVATE:
                    return !character.IsMuted();
                case ChatActivableChannelsEnum.PSEUDO_CHANNEL_INFO:
                    return false;
                case ChatActivableChannelsEnum.PSEUDO_CHANNEL_FIGHT_LOG:
                    return false;
                default:
                    return false;
            }
        }

        public Emote GetEmote(uint id)
        {
            Emote emote;
            return m_emotes.TryGetValue(id, out emote) ? emote : null;
        }

        #region Handlers

        public void HandleChat(WorldClient client, ChatActivableChannelsEnum channel, string message, IEnumerable<ObjectItem> objectItems = null)
        {
            if (!ChatHandlers.ContainsKey(channel))
                return;

            if (message.StartsWith(CommandPrefix) && (message.Length < CommandPrefix.Length * 2 || message.Substring(CommandPrefix.Length, CommandPrefix.Length) != CommandPrefix)) // ignore processing command whenever there is the preffix twice
            {
                message = message.Remove(0, CommandPrefix.Length); // remove our prefix

                #region // ----------------- Sistema de Logs TXT Staff by: Kenshin (Logs antigo desativado)---------------- //
                //if (Settings.LogsStaffs == true && client.Character.Account.UserGroupId >= (int)RoleEnum.Collaborator)
                //{
                //    try
                //    {
                //        string Buffer = "";
                //        string CheminFichier = @".\logstaff\log_" + DateTime.Now.ToString("dd-MM-yyyy") + ".txt";

                //        FileInfo Fichier = new FileInfo(CheminFichier);

                //        if (Fichier.Exists) //Verificando se o arquivo existe
                //        {
                //            StreamReader Lecture = new StreamReader(CheminFichier, Encoding.Default); //Abre o arquivo
                //            Buffer = Lecture.ReadToEnd(); //Colocando o arquivo inteiro em uma variável
                //            Lecture.Close(); //Fechando
                //        }

                //        if (Buffer == null || Buffer == "") //Verificando se há algo no arquivo, se sim...
                //        {
                //            StreamWriter Ecriture = new StreamWriter(CheminFichier, false, Encoding.Default); //O booleano para false sobrescreve o arquivo existente
                //            Ecriture.Write("" + client.Character.Name + " |+|Comando: " + message + " |+|Data: " + DateTime.Now + " |+|IP User: " + client.IP + "\r\n"); //Escreva a variável e seu valor
                //            Ecriture.Close(); //Fechando
                //        }
                //        else //sim não ...
                //        {
                //            StreamWriter Ecriture = new StreamWriter(CheminFichier, true, Encoding.Default); //O boolean para false permite adicionar uma linha sem sobrescrever o arquivo
                //            Ecriture.Write("" + client.Character.Name + " |+|Comando: " + message + " |+|Data: " + DateTime.Now + " |+|IP User: " + client.IP + "\r\n"); //Adicionamos a variável mais o valor (uma quebra de linha antes)
                //            Ecriture.Close(); //Fechando
                //        }
                //    }
                //    catch (Exception e)
                //    {
                //        Console.WriteLine("Erro no Log dos Staffs : " + e.Message);
                //    }
                //}
                #endregion

                WorldServer.Instance.CommandManager.HandleCommand(new TriggerChat(new StringStream(UnescapeChatCommand(message)), client.Character));
            }
            else
            {
                if (!CanUseChannel(client.Character, channel))
                    return;

                var badword = CanSendMessage(message);

                if (badword != string.Empty)
                {
                    #region // ----------------- Sistema de Logs MongoDB Mensagens Badwords by: Kenshin ---------------- //
                    try
                    {
                        var ChannelName = "";

                        if (channel == ChatActivableChannelsEnum.CHANNEL_ADMIN)
                            ChannelName = "Channel Admin";
                        else if (channel == ChatActivableChannelsEnum.CHANNEL_ALLIANCE)
                            ChannelName = "Channel Alliance";
                        else if (channel == ChatActivableChannelsEnum.CHANNEL_ARENA)
                            ChannelName = "Channel Arena";
                        else if (channel == ChatActivableChannelsEnum.CHANNEL_GLOBAL)
                            ChannelName = "Channel Global";
                        else if (channel == ChatActivableChannelsEnum.CHANNEL_GUILD)
                            ChannelName = "Channel Guild";
                        else if (channel == ChatActivableChannelsEnum.CHANNEL_PARTY)
                            ChannelName = "Channel Party";
                        else if (channel == ChatActivableChannelsEnum.CHANNEL_VIP)
                            ChannelName = "Channel Vip";
                        else if (channel == ChatActivableChannelsEnum.CHANNEL_SALES)
                            ChannelName = "Channel Sales";
                        else if (channel == ChatActivableChannelsEnum.CHANNEL_SEEK)
                            ChannelName = "Channel Seek";
                        else if (channel == ChatActivableChannelsEnum.CHANNEL_TEAM)
                            ChannelName = "Channel Team";
                        else if (channel == ChatActivableChannelsEnum.PSEUDO_CHANNEL_PRIVATE)
                            ChannelName = "Channel Private";

                        var document = new BsonDocument
                            {
                               { "SenderHardwareID", client.Account.LastHardwareId },
                               { "SenderId", client.Character.Id },
                               { "SenderName", client.Character.Name },
                               { "SenderAccountId", client.Account.Id },
                               { "SenderAccountName", client.Account.Login },
                               { "ReceiverId", 0 },
                               { "ReceiverName", "" },
                               { "ReceiverAccountId", 0 },
                               { "ReceiverAccountName", "" },
                               { "Message", message },
                               { "Badword", badword },
                               { "ChannelId", (int)channel },
                               { "ChannelName", ChannelName },
                               { "Date", DateTime.Now.ToString(CultureInfo.CreateSpecificCulture("pt-BR")) }
                            };

                        MongoLogger.Instance.Insert("Player_Badword", document);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Erro no Mongologs das Mensagens : " + e.Message);
                    }
                    #endregion

                    switch (client.Character.Account.Lang)
                    {
                        case "fr":
                            client.Character.SendServerMessage($"Message non envoyé. Le terme <b>{badword}</b> est interdit sur le serveur ! Votre Nick est surveillé.");
                            break;
                        case "es":
                            client.Character.SendServerMessage($"Mensaje no enviado ¡El término <b>{badword}</b> está prohibido en el servidor! Tu Nick está siendo monitoreado.");
                            break;
                        case "en":
                            client.Character.SendServerMessage($"Message not sent. The term <b>{badword}</b> is forbidden on the server! Your Nick is being monitored.");
                            break;
                        default:
                            client.Character.SendServerMessage($"Mensagem não enviada. O termo <b>{badword}</b> é proibido no servidor! Seu Nick está salvo e sendo monitorado.");
                            break;
                    }
                    return;
                }

                #region // ----------------- Sistema de Logs TXT Mensagens by: Kenshin (Logs Antiga Desativada)---------------- //
                //if (Settings.LogsPlayers == true && client.Character.Account.UserGroupId >= (int)RoleEnum.Player)
                //{
                //    try
                //    {
                //        string Buffer = "";
                //        string CheminFichier = @".\logsplayers\log_" + DateTime.Now.ToString("dd-MM-yyyy") + ".txt";

                //        FileInfo Fichier = new FileInfo(CheminFichier);

                //        if (Fichier.Exists) //Verificando se o arquivo existe
                //        {
                //            StreamReader Lecture = new StreamReader(CheminFichier, Encoding.Default); //Abre o arquivo
                //            Buffer = Lecture.ReadToEnd(); //Colocando o arquivo inteiro em uma variável
                //            Lecture.Close(); //Fechando
                //        }

                //        if (Buffer == null || Buffer == "") //Verificando se há algo no arquivo, se sim...
                //        {
                //            StreamWriter Ecriture = new StreamWriter(CheminFichier, false, Encoding.Default); //O booleano para false sobrescreve o arquivo existente
                //            Ecriture.Write("" + client.Character.Name + " |+|Mensagem: " + message + " |+|Data: " + DateTime.Now + " |+|IP User: " + client.IP + "\r\n"); //Escreva a variável e seu valor
                //            Ecriture.Close(); //Fechando
                //        }
                //        else //sim não ...
                //        {
                //            StreamWriter Ecriture = new StreamWriter(CheminFichier, true, Encoding.Default); //O boolean para false permite adicionar uma linha sem sobrescrever o arquivo
                //            Ecriture.Write("" + client.Character.Name + " |+|Mensagem: " + message + " |+|Data: " + DateTime.Now + " |+|IP User: " + client.IP + "\r\n"); //Adicionamos a variável mais o valor (uma quebra de linha antes)
                //            Ecriture.Close(); //Fechando
                //        }
                //    }
                //    catch (Exception e)
                //    {
                //        Console.WriteLine("Erro no Log dos Mensagens : " + e.Message);
                //    }
                //}
                #endregion

                if (client.Character.IsMuted())
                {
                    client.Character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 124, (int)client.Character.GetMuteRemainingTime().TotalSeconds);
                }
                else
                {
                    if (client.Character.ChatHistory.RegisterAndCheckFlood(new ChatEntry(message, channel, DateTime.Now)))
                        ChatHandlers[channel](client, message, objectItems);
                }

                #region // ----------------- Sistema de Logs MongoDB Mensagens by: Kenshin ---------------- //
                try
                {
                    var ChannelName = "";

                    if (channel == ChatActivableChannelsEnum.CHANNEL_ADMIN)
                        ChannelName = "Channel Admin";
                    else if (channel == ChatActivableChannelsEnum.CHANNEL_ALLIANCE)
                        ChannelName = "Channel Alliance";
                    else if (channel == ChatActivableChannelsEnum.CHANNEL_ARENA)
                        ChannelName = "Channel Arena";
                    else if (channel == ChatActivableChannelsEnum.CHANNEL_GLOBAL)
                        ChannelName = "Channel Global";
                    else if (channel == ChatActivableChannelsEnum.CHANNEL_GUILD)
                        ChannelName = "Channel Guild";
                    else if (channel == ChatActivableChannelsEnum.CHANNEL_PARTY)
                        ChannelName = "Channel Party";
                    else if (channel == ChatActivableChannelsEnum.CHANNEL_VIP)
                        ChannelName = "Channel Vip";
                    else if (channel == ChatActivableChannelsEnum.CHANNEL_SALES)
                        ChannelName = "Channel Sales";
                    else if (channel == ChatActivableChannelsEnum.CHANNEL_SEEK)
                        ChannelName = "Channel Seek";
                    else if (channel == ChatActivableChannelsEnum.CHANNEL_TEAM)
                        ChannelName = "Channel Team";
                    else if (channel == ChatActivableChannelsEnum.PSEUDO_CHANNEL_PRIVATE)
                        ChannelName = "Channel Private";

                    var document = new BsonDocument
                    {
                        { "SenderHardwareID", client.Account.LastHardwareId },
                        { "SenderId", client.Character.Id },
                        { "SenderName", client.Character.Name },
                        { "SenderAccountId", client.Account.Id },
                        { "SenderAccountName", client.Account.Login },
                        { "ReceiverId", 0 },
                        { "ReceiverName", "" },
                        { "ReceiverAccountId", 0 },
                        { "ReceiverAccountName", "" },
                        { "Message", message },
                        { "ChannelId", (int)channel },
                        { "ChannelName", ChannelName },
                        { "Date", DateTime.Now.ToString(CultureInfo.CreateSpecificCulture("pt-BR")) }
                    };

                    MongoLogger.Instance.Insert("Player_Chats", document);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Erro no Mongologs das Mensagens : " + e.Message);
                }
                #endregion

                if (DiscordIntegration.EnableDiscordWebHook)
                {
                    string DiscordUrl = null;
                    string ChannelName = null;

                    if (channel == ChatActivableChannelsEnum.CHANNEL_GLOBAL)
                    {
                        DiscordUrl = DiscordIntegration.DiscordChatGlobalUrl;
                        ChannelName = "[GLOBAL] ";
                    }
                    else if (channel == ChatActivableChannelsEnum.CHANNEL_VIP)
                    {
                        DiscordUrl = DiscordIntegration.DiscordChatVipUrl;
                        ChannelName = "[VIP] ";
                    }
                    else if (channel == ChatActivableChannelsEnum.CHANNEL_SALES)
                    {
                        DiscordUrl = DiscordIntegration.DiscordChatSallersUrl;
                        ChannelName = "[SALLES] ";
                    }
                    else if (channel == ChatActivableChannelsEnum.CHANNEL_SEEK)
                    {
                        DiscordUrl = DiscordIntegration.DiscordChatSeekUrl;
                        ChannelName = "[RECRUITMENT] ";
                    }

                    PlainText.SendWebHook(DiscordUrl, $"{ChannelName} **{client.Character.Namedefault}**: {message}", DiscordIntegration.DiscordWHUsername);
                    PlainText.SendWebHook(DiscordIntegration.DiscordChatStaffUrl, $"{ChannelName} **{client.Character.Namedefault}**: {message}", DiscordIntegration.DiscordWHUsername);
                }
            }
        }

        public static string UnescapeChatCommand(string command)
        {
            var sb = new StringBuilder();

            for (var i = 0; i < command.Length; i++)
            {
                if (command[i] == '&')
                {
                    var index = command.IndexOf(';', i, 5);
                    if (index == -1)
                        continue;

                    var str = command.Substring(i + 1, index - i - 1);

                    switch (str)
                    {
                        case "lt":
                            sb.Append("<");
                            break;
                        case "gt":
                            sb.Append(">");
                            break;
                        case "quot":
                            sb.Append("\"");
                            break;
                        case "amp":
                            sb.Append("&");
                            break;
                        default:
                            int id;
                            if (!int.TryParse(str, out id))
                                continue;
                            sb.Append((char)id);
                            break;
                    }

                    i = index + 1;
                }
                else
                    sb.Append(command[i]);
            }

            return sb.ToString();
        }

        private static void SendChatServerMessage(IPacketReceiver client, Character sender, ChatActivableChannelsEnum channel, string message)
        {
            if (sender.AdminMessagesEnabled)
                ChatHandler.SendChatAdminServerMessage(client, sender, channel, message);
            else
                ChatHandler.SendChatServerMessage(client, sender, channel, message);
        }

        private static void SendChatServerMessage(IPacketReceiver client, INamedActor sender, ChatActivableChannelsEnum channel, string message)
        {
            ChatHandler.SendChatServerMessage(client, sender, channel, message);
        }

        private static void SendChatServerWithObjectMessage(IPacketReceiver client, INamedActor sender, ChatActivableChannelsEnum channel, string message, IEnumerable<ObjectItem> objectItems)
        {
            ChatHandler.SendChatServerWithObjectMessage(client, sender, channel, message, "", objectItems);
        }

        public void SayGlobal(WorldClient client, string msg, IEnumerable<ObjectItem> objectItems)
        {
            if (!CanUseChannel(client.Character, ChatActivableChannelsEnum.CHANNEL_GLOBAL))
                return;

            //var clients = client.Character.Map.Clients;
            var clients = client.Character.Map.GetAllCharacters().Where(x => Game.HavenBags.HavenBagManager.Instance.CanBeSeenInHavenBag(x, client.Character)).Select(v => v.Client);


            /* if (client.Character.IsFighting())
                 clients = client.Character.Fight.Clients;
             else if (client.Character.IsSpectator())
                 clients = client.Character.Fight.SpectatorClients;*/
            if (client.Character.IsFighting())
                clients = client.Character.Fight.GetAllCharacters().Select(x => x.Client);
            else if (client.Character.IsSpectator())
                clients = client.Character.Fight.GetSpectators().Select(x => x.Client);

            if (objectItems != null)
                //Impossible d'afficher des objets dans ce canal.
                client.Character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 114);
            else
                //SendChatServerMessage(clients, client.Character, ChatActivableChannelsEnum.CHANNEL_GLOBAL, msg);
                clients.ForEach(x => SendChatServerMessage(x, client.Character, ChatActivableChannelsEnum.CHANNEL_GLOBAL, msg));
        }

        public void SayGlobal(NamedActor actor, string msg)
        {
            SendChatServerMessage(actor.CharacterContainer.Clients, actor, ChatActivableChannelsEnum.CHANNEL_GLOBAL, msg);
        }

        public void SayAdministrators(WorldClient client, string msg, IEnumerable<ObjectItem> objectItems)
        {
            if (client.UserGroup.Role < AdministratorChatMinAccess)
                return;

            World.Instance.ForEachCharacter(entry =>
            {
                if (!CanUseChannel(entry, ChatActivableChannelsEnum.CHANNEL_ADMIN))
                    return;

                if (objectItems != null)
                    SendChatServerWithObjectMessage(entry.Client, client.Character, ChatActivableChannelsEnum.CHANNEL_ADMIN, msg, objectItems);
                else
                    SendChatServerMessage(entry.Client, client.Character, ChatActivableChannelsEnum.CHANNEL_ADMIN, msg);
            });
        }

        public void SayVip(WorldClient client, string msg, IEnumerable<ObjectItem> objectItems)
        {
            if (client.Account.IsSubscribe == false)
            {
                client.Character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 114);
                return;
            }

            World.Instance.ForEachCharacter(entry =>
            {
                if (!CanUseChannel(entry, ChatActivableChannelsEnum.CHANNEL_VIP))
                    return;

                if (objectItems != null)
                    SendChatServerWithObjectMessage(entry.Client, client.Character, ChatActivableChannelsEnum.CHANNEL_VIP, msg, objectItems);
                else
                    SendChatServerMessage(entry.Client, client.Character, ChatActivableChannelsEnum.CHANNEL_VIP, msg);
            });
        }

        public void SayParty(WorldClient client, string msg, IEnumerable<ObjectItem> objectItems)
        {
            if (!CanUseChannel(client.Character, ChatActivableChannelsEnum.CHANNEL_PARTY))
            {
                ChatHandler.SendChatErrorMessage(client, ChatErrorEnum.CHAT_ERROR_NO_PARTY);
                return;
            }

            client.Character.Party.ForEach(entry =>
            {
                if (objectItems != null)
                    SendChatServerWithObjectMessage(entry.Client, client.Character, ChatActivableChannelsEnum.CHANNEL_PARTY, msg, objectItems);
                else
                    SendChatServerMessage(entry.Client, client.Character, ChatActivableChannelsEnum.CHANNEL_PARTY, msg);
            });
        }
        public void SayArena(WorldClient client, string msg, IEnumerable<ObjectItem> objectItems)
        {
            if (!CanUseChannel(client.Character, ChatActivableChannelsEnum.CHANNEL_ARENA))
            {
                ChatHandler.SendChatErrorMessage(client, ChatErrorEnum.CHAT_ERROR_NO_PARTY_ARENA);
                return;
            }

            client.Character.ArenaParty.ForEach(entry =>
            {
                if (objectItems != null)
                    SendChatServerWithObjectMessage(entry.Client, client.Character, ChatActivableChannelsEnum.CHANNEL_ARENA, msg, objectItems);
                else
                    SendChatServerMessage(entry.Client, client.Character, ChatActivableChannelsEnum.CHANNEL_ARENA, msg);
            });
        }

        public void SayGuild(WorldClient client, string msg, IEnumerable<ObjectItem> objectItems)
        {
            if (!CanUseChannel(client.Character, ChatActivableChannelsEnum.CHANNEL_GUILD))
            {
                ChatHandler.SendChatErrorMessage(client, ChatErrorEnum.CHAT_ERROR_NO_GUILD);
                return;
            }

            if (objectItems != null)
                SendChatServerWithObjectMessage(client.Character.Guild.Clients, client.Character, ChatActivableChannelsEnum.CHANNEL_GUILD, msg, objectItems);
            else
                SendChatServerMessage(client.Character.Guild.Clients, client.Character, ChatActivableChannelsEnum.CHANNEL_GUILD, msg);
        }
        public void SayAlliance(WorldClient client, string msg, IEnumerable<ObjectItem> objectItems)
        {
            if (!CanUseChannel(client.Character, ChatActivableChannelsEnum.CHANNEL_ALLIANCE))
            {
                ChatHandler.SendChatErrorMessage(client, ChatErrorEnum.CHAT_ERROR_ALLIANCE);
                return;
            }

            if (objectItems != null)
                SendChatServerWithObjectMessage(client.Character.Guild.Alliance.Clients, client.Character, ChatActivableChannelsEnum.CHANNEL_ALLIANCE, msg, objectItems);
            else
                SendChatServerMessage(client.Character.Guild.Alliance.Clients, client.Character, ChatActivableChannelsEnum.CHANNEL_ALLIANCE, msg);
        }
        public void SayTeam(WorldClient client, string msg, IEnumerable<ObjectItem> objectItems)
        {
            if (!CanUseChannel(client.Character, ChatActivableChannelsEnum.CHANNEL_TEAM))
            {
                ChatHandler.SendChatErrorMessage(client, ChatErrorEnum.CHAT_ERROR_NO_TEAM);
                return;
            }

            foreach (var fighter in client.Character.Fighter.Team.GetAllFighters<CharacterFighter>())
            {
                if (objectItems != null)
                    SendChatServerWithObjectMessage(fighter.Character.Client, client.Character, ChatActivableChannelsEnum.CHANNEL_TEAM, msg, objectItems);
                else
                    SendChatServerMessage(fighter.Character.Client, client.Character, ChatActivableChannelsEnum.CHANNEL_TEAM, msg);
            }
        }

        public void SaySeek(WorldClient client, string msg, IEnumerable<ObjectItem> objectItems)
        {
            if (!CanUseChannel(client.Character, ChatActivableChannelsEnum.CHANNEL_SEEK))
                return;

            if (objectItems != null)
            {
                //Impossible d'afficher des objets dans ce canal.
                client.Character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 114);
                return;
            }

            World.Instance.ForEachCharacter(entry =>
            {
                SendChatServerMessage(entry.Client, client.Character, ChatActivableChannelsEnum.CHANNEL_SEEK, msg);
            });
        }

        public void SaySales(WorldClient client, string msg, IEnumerable<ObjectItem> objectItems)
        {
            if (!CanUseChannel(client.Character, ChatActivableChannelsEnum.CHANNEL_SALES))
                return;

            World.Instance.ForEachCharacter(entry =>
            {
                if (objectItems != null)
                    SendChatServerWithObjectMessage(entry.Client, client.Character, ChatActivableChannelsEnum.CHANNEL_SALES, msg, objectItems);
                else
                    SendChatServerMessage(entry.Client, client.Character, ChatActivableChannelsEnum.CHANNEL_SALES, msg);
            });
        }

        public static bool IsGlobalChannel(ChatActivableChannelsEnum channel)
        {
            return channel == ChatActivableChannelsEnum.CHANNEL_SALES ||
                   channel == ChatActivableChannelsEnum.CHANNEL_SEEK;
        }

        public void Save()
        {
            foreach (var badWord in BadWords.Where(x => x.IsDirty || x.IsNew))
            {
                if (badWord.IsNew)
                    Database.Insert(badWord);
                else
                    Database.Update(badWord);

                badWord.IsNew = false;
                badWord.IsDirty = false;
            }
        }

        #endregion
    }
}