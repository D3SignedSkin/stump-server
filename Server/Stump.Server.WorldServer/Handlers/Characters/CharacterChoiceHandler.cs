using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Messages;
using Stump.DofusProtocol.Types;
using Stump.Server.BaseServer.Network;
using Stump.Server.WorldServer.Core.Network;
using Stump.Server.WorldServer.Database.Characters;
using Stump.Server.WorldServer.Game.Accounts;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Breeds;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Handlers.Basic;
using Stump.Server.WorldServer.Handlers.Chat;
using Stump.Server.WorldServer.Handlers.Context;
using Stump.Server.WorldServer.Handlers.Context.RolePlay;
using Stump.Server.WorldServer.Handlers.Guilds;
using Stump.Server.WorldServer.Handlers.Friends;
using Stump.Server.WorldServer.Handlers.Initialization;
using Stump.Server.WorldServer.Handlers.Inventory;
using Stump.Server.WorldServer.Handlers.Mounts;
using Stump.Server.WorldServer.Handlers.PvP;
using Stump.Server.WorldServer.Handlers.Shortcuts;
using Stump.Server.WorldServer.Handlers.Actions;
using Stump.Server.WorldServer.Game.Items;
using Stump.Core.Reflection;
using Stump.Server.WorldServer.Game.HavenBags;
using Stump.Server.WorldServer.Game.Spells;
using Stump.Server.WorldServer.Game.Presets;
using Stump.Server.WorldServer.Handlers.Achievements;
using MongoDB.Bson;
using Stump.Server.BaseServer.Logging;
using Stump.Server.WorldServer.Game.Gifts;
using System.Globalization;
using Stump.Server.BaseServer.IPC.Messages;
using Stump.Server.WorldServer.Core.IPC;
using NLog;
using Stump.Server.WorldServer.Game.Items.Player;

namespace Stump.Server.WorldServer.Handlers.Characters
{
    public partial class CharacterHandler
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        #region >> Login
        public static void CommonCharacterSelection(WorldClient client, CharacterRecord character)
        {
            if (character.IsDeleted)
                return;

            // Check if we also have a world account
            if (client.WorldAccount == null)
            {
                var account = AccountManager.Instance.FindById(client.Account.Id) ?? AccountManager.Instance.CreateWorldAccount(client);
                client.WorldAccount = account;
            }

            client.Character = new Character(character, client);
            client.Character.LoadRecord();

            ContextHandler.SendNotificationListMessage(client, new[] { 0x7FFFFFFF });
            BasicHandler.SendBasicTimeMessage(client);
            SendCharacterSelectedSuccessMessage(client);

            if (client.Character.Inventory.Presets.Any())
                InventoryHandler.SendInventoryContentAndPresetMessage(client);
            else
                InventoryHandler.SendInventoryContentMessage(client);

            ShortcutHandler.SendShortcutBarContentMessage(client, ShortcutBarEnum.GENERAL_SHORTCUT_BAR);
            ContextRoleplayHandler.SendEmoteListMessage(client, client.Character.Emotes.Select(x => (ushort)x));
            AchievementHandler.SendAchievementListMessage(client, client.Character.Achievement.FinishedAchievements.Select(x => (ushort)x.Id), client.Character);

            // Jobs
            ContextRoleplayHandler.SendJobDescriptionMessage(client, client.Character);
            ContextRoleplayHandler.SendJobExperienceMultiUpdateMessage(client, client.Character);
            ContextRoleplayHandler.SendJobCrafterDirectorySettingsMessage(client, client.Character);

            PvPHandler.SendAlignmentRankUpdateMessage(client, client.Character);

            //Chats Controlls
            if (client.Character.UserGroup.Role >= RoleEnum.Moderator_Helper)
                ChatHandler.SendEnabledChannelsMessage(client, new sbyte[] { 0, 1, 2, 3, 4, 5, 6, 8, 9, 10, 12, 13, 14 }, new sbyte[] { });
            else if (client.Character.Vip)
                ChatHandler.SendEnabledChannelsMessage(client, new sbyte[] { 0, 1, 2, 3, 4, 5, 6, 9, 10, 12, 13 }, new sbyte[] { 7, 8, 14 });
            else
                ChatHandler.SendEnabledChannelsMessage(client, new sbyte[] { 0, 1, 2, 3, 4, 5, 6, 9, 10, 13 }, new sbyte[] { 7, 8, 12, 14 });

            if (client.Character.Vip)
                ChatHandler.SendChatSmileyExtraPackListMessage(client, new DofusProtocol.Enums.Custom.SmileyPacksEnum[] { (DofusProtocol.Enums.Custom.SmileyPacksEnum)0, (DofusProtocol.Enums.Custom.SmileyPacksEnum)1, (DofusProtocol.Enums.Custom.SmileyPacksEnum)2, (DofusProtocol.Enums.Custom.SmileyPacksEnum)3, (DofusProtocol.Enums.Custom.SmileyPacksEnum)4, (DofusProtocol.Enums.Custom.SmileyPacksEnum)5 });
            else
                ChatHandler.SendChatSmileyExtraPackListMessage(client, client.Character.SmileyPacks.ToArray());

            if (character.IsInIncarnation)
                IncarnationManager.Instance.ConnectWithCustomIncarnation(client.Character, character.IncarnationId);
            else
                InventoryHandler.SendSpellListMessage(client, true);

            ShortcutHandler.SendShortcutBarContentMessage(client, ShortcutBarEnum.SPELL_SHORTCUT_BAR);
            InitializationHandler.SendSetCharacterRestrictionsMessage(client, client.Character);
            InventoryHandler.SendInventoryWeightMessage(client);
            FriendHandler.SendFriendWarnOnConnectionStateMessage(client, client.Character.FriendsBook.WarnOnConnection);
            FriendHandler.SendFriendWarnOnLevelGainStateMessage(client, client.Character.FriendsBook.WarnOnLevel);

            try
            {
                GuildHandler.SendGuildMemberWarnOnConnectionStateMessage(client, client.Character.WarnOnGuildConnection);

                if (client.Character.GuildMember != null) //Guild
                {
                    GuildHandler.SendGuildMembershipMessage(client, client.Character.GuildMember);

                    if (client.Character.Guild?.Alliance != null)
                    {
                        //AllianceHandler.SendAllianceMembershipMessage(client.Character.Client, client.Character.Guild.Alliance, true);
                        //AllianceHandler.SendAllianceInsiderInfoMessage(client.Character.Client, client.Character.Guild.Alliance);
                    }
                }
            }
            catch
            { }

            if (client.Character.EquippedMount != null) //Mount
            {
                MountHandler.SendMountSetMessage(client, client.Character.EquippedMount.GetMountClientData());
                MountHandler.SendMountXpRatioMessage(client, client.Character.EquippedMount.GivenExperience);

                if (client.Character.IsRiding)
                    MountHandler.SendMountRidingMessage(client, client.Character.IsRiding);
            }

            if (client.Character.Record.HavenBagsCSV == null || client.Character.Record.HavenBagsCSV.Length < 1)
                client.Character.Record.HavenBagsCSV = "1;2";

            HavenBagManager.Instance.SendHavenBagPackMessage(client);
            client.Character.SendConnectionMessages();

            //Don't know why ?
            ActionsHandler.SendSequenceNumberRequestMessage(client);

            //Start Cinematic
            if ((DateTime.Now - client.Character.Record.CreationDate).TotalSeconds <= 30)
                BasicHandler.SendCinematicMessage(client, 10);

            ContextRoleplayHandler.SendGameRolePlayArenaUpdatePlayerInfosMessage(client, client.Character);
            SendCharacterCapabilitiesMessage(client);

            //Presets
            PresetsManager.Instance.SendPresetsListMessage(client);
            ContextRoleplayHandler.SendAlmanachCalendarDateMessage(client);

            //Loading complete
            SendCharacterLoadingCompleteMessage(client);
            BasicHandler.SendServerExperienceModificatorMessage(client, client.Character);
            // update tokens

            #region Criando o TokenItem ao iniciar o personagem no servidor.
            BasePlayerItem item = client.Character.Inventory.GetItems(CharacterInventoryPositionEnum.INVENTORY_POSITION_NOT_EQUIPED).FirstOrDefault(x => x.Template.Id == Settings.TokenTemplateId);

            //Removendo Item Token existente
            //if (item != null)
            //    client.Character.Inventory.RemoveItem(item);

            //Criando novamente Item Token
            if (client.Account.Tokens != 0)
                client.Character.Inventory.AddItem(ItemManager.Instance.TryGetTemplate(Settings.TokenTemplateId), amount: (int)client.Account.Tokens);
            #endregion

            //#region Criando ou Removendo o Item que representa os status dos VIPs
            //List<BasePlayerItem> itemsBoost = client.Character.Inventory.GetItems(CharacterInventoryPositionEnum.INVENTORY_POSITION_BOOST_FOOD).ToList();
            //List<BasePlayerItem> itemsVip = itemsBoost.Where(x => x.Template.Id == 30009 || x.Template.Id == 30010).ToList();

            ////Removendo Item Vip existente
            //if (itemsVip.Count > 0)
            //{
            //    foreach (var itemVip in itemsVip)
            //    {
            //        client.Character.Inventory.RemoveItem(itemVip);
            //    }
            //}

            ////Criando novamente Item Vip
            //if (client.Character.UserGroup.Role == RoleEnum.Vip || client.Character.UserGroup.Role >= RoleEnum.Gold_Vip)
            //{
            //    PotionStatus.SetPotionStatus(client);
            //}
            //#endregion

            CharacterSendOnLoginMessage(client);

            // Update LastConnection and Last Ip
            client.WorldAccount.LastConnection = DateTime.Now;
            client.WorldAccount.LastIp = client.IP;
            client.WorldAccount.ConnectedCharacter = character.Id;

            WorldServer.Instance.IOTaskPool.ExecuteInContext(() =>
            {
                try
                {
                    WorldServer.Instance.DBAccessor.Database.Update(client.WorldAccount);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error during database update: {ex.Message}");
                }
            });

            //System.Threading.Tasks.Task.Factory.StartNewDelayed(10000, () => client.Character.LogIn()); // if more than 10 seg bug in 50%,so its verify if the login is right!
            //client.Character.LogIn();
        }
        #endregion

        #region >> Character Message
        [WorldHandler(CharacterFirstSelectionMessage.Id, ShouldBeLogged = false, IsGamePacket = false)]
        public static void HandleCharacterFirstSelectionMessage(WorldClient client, CharacterFirstSelectionMessage message)
        {
            // TODO ADD TUTORIAL EFFECTS
            HandleCharacterSelectionMessage(client, message);
        }

        [WorldHandler(CharacterSelectionMessage.Id, ShouldBeLogged = false, IsGamePacket = false)]
        public static void HandleCharacterSelectionMessage(WorldClient client, CharacterSelectionMessage message)
        {
            var character = client.Characters.Where(x => !x.IsDeleted).First(entry => entry.Id == (int)message.id);

            if (character == null)
            {
                client.Send(new CharacterSelectedErrorMessage());
                return;
            }

            CommonCharacterSelection(client, character);
        }

        [WorldHandler(CharactersListRequestMessage.Id, ShouldBeLogged = false, IsGamePacket = false)]
        public static void HandleCharacterListRequest(WorldClient client, CharactersListRequestMessage message)
        {
            if (client.Account != null && client.Account.Login != "")
            {
                var characterInFight = FindCharacterFightReconnection(client);

                CheckEventOnLogin(client);

                if (characterInFight != null)
                {
                    client.ForceCharacterSelection = characterInFight;
                    SendCharacterSelectedForceMessage(client, characterInFight.Id);
                }
                else
                {
                    SendCharactersListWithRemodelingMessage(client);
                }

                //if (client.WorldAccount != null && client.StartupActions.Count > 0)
                //{
                //    StartupHandler.SendStartupActionsListMessage(client, client.StartupActions);
                //}
            }
            else
            {
                client.Send(new IdentificationFailedMessage((int)IdentificationFailureReasonEnum.KICKED));
                client.DisconnectLater(1000);
            }
        }

        [WorldHandler(CharacterSelectedForceReadyMessage.Id, IsGamePacket = false, ShouldBeLogged = false)]
        public static void HandleCharacterSelectedForceReadyMessage(WorldClient client, CharacterSelectedForceReadyMessage message)
        {
            if (client.ForceCharacterSelection == null)
                client.Disconnect();
            else
                CommonCharacterSelection(client, client.ForceCharacterSelection);
        }
        #endregion

        #region >> Character Remodel
        [WorldHandler(CharacterSelectionWithRemodelMessage.Id, ShouldBeLogged = false, IsGamePacket = false)]
        public static void HandleCharacterSelectionWithRemodelMessage(WorldClient client, CharacterSelectionWithRemodelMessage message)
        {
            var character = client.Characters.Where(x => !x.IsDeleted).First(entry => entry.Id == (int)message.id);

            /* Check null */
            if (character == null)
            {
                client.Send(new CharacterSelectedErrorMessage());
                return;
            }

            var remodel = message.remodel;

            if (((character.MandatoryChanges & (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_NAME)
                == (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_NAME)
                || ((character.PossibleChanges & (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_NAME)
                == (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_NAME))
            {
                /* Check if name is valid */
                if (!Regex.IsMatch(remodel.name, "^[A-Z][a-z]{2,9}(?:-[A-Z][a-z]{2,9}|[a-z]{1,10})$", RegexOptions.Compiled))
                {
                    client.Send(new CharacterCreationResultMessage((int)CharacterCreationResultEnum.ERR_INVALID_NAME, 0)); //TODO - v2.66
                    return;
                }

                /* Check if name is free */
                if (CharacterManager.Instance.DoesNameExist(remodel.name))
                {
                    client.Send(new CharacterCreationResultMessage((int)CharacterCreationResultEnum.ERR_NAME_ALREADY_EXISTS, 0)); // TODO - v2.66
                    return;
                }
                /* Set new name */
                character.Name = remodel.name;
            }
            WorldServer.Instance.IOTaskPool.ExecuteInContext(() => // had savenow so maybe can crash, so i add this
            {
                if (((character.MandatoryChanges & (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_GENDER)
                == (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_GENDER)
                || ((character.PossibleChanges & (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_GENDER)
                == (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_GENDER))
                {
                    character.Sex = remodel.sex ? SexTypeEnum.SEX_FEMALE : SexTypeEnum.SEX_MALE;
                    client.Character = new Character(character, client);
                    client.Character.LoadRecord();

                    client.Character.ResetDefaultLook();

                    client.Character.SaveLater();
                    character = client.Character.Record;
                    client.Character = null;
                }

                if (((character.MandatoryChanges & (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_BREED)
                    == (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_BREED)
                    || ((character.PossibleChanges & (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_BREED)
                    == (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_BREED))
                {
                    client.Character = new Character(character, client);
                    client.Character.LoadRecord();

                    BreedManager.ChangeBreed(client.Character, (PlayableBreedEnum)remodel.breed);
                    client.Character.SaveLater();

                    character = client.Character.Record;
                    client.Character = null;
                }

                if (((character.MandatoryChanges & (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_COSMETIC)
                    == (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_COSMETIC)
                    || ((character.PossibleChanges & (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_COSMETIC)
                    == (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_COSMETIC))
                {
                    /* Get Head */
                    var head = BreedManager.Instance.GetHead(remodel.cosmeticId);
                    /* Get character Breed */
                    var breed = BreedManager.Instance.GetBreed((int)character.Breed);

                    if (breed == null || head.Breed != (int)character.Breed || head.Gender != (int)character.Sex)
                    {
                        client.Send(new CharacterSelectedErrorMessage());
                        return;
                    }

                    character.Head = head.Id;
                    character.DefaultLook = breed.GetLook(character.Sex);
                    character.DefaultLook.AddSkins(head.Skins);

                    foreach (var scale in character.Sex == SexTypeEnum.SEX_MALE ? breed.MaleLook.Scales : breed.FemaleLook.Scales)
                        character.DefaultLook.SetScales(scale);

                    /* Set Colors - Correção Cor Cinza by Kenshin */
                    var breedColors = character.Sex == SexTypeEnum.SEX_MALE ? breed.MaleColors : breed.FemaleColors;
                    var m_colors = new Dictionary<int, Color>();

                    foreach (var color in remodel.colors)
                    {
                        var index = color >> 24;
                        var c = Color.FromArgb(color);

                        m_colors.Add(index, c);
                    }

                    var i = 1;
                    foreach (var breedColor in breedColors)
                    {
                        if (!m_colors.ContainsKey(i))
                            m_colors.Add(i, Color.FromArgb((int)breedColor));

                        i++;
                    }

                    character.DefaultLook.SetColors(m_colors.Select(x => x.Key).ToArray(), m_colors.Select(x => x.Value).ToArray());
                }

                if (((character.MandatoryChanges & (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_COLORS)
                    == (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_COLORS)
                    || ((character.PossibleChanges & (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_COLORS)
                    == (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_COLORS))
                {
                    /* Get character Breed */
                    var breed = BreedManager.Instance.GetBreed((int)character.Breed);

                    if (breed == null)
                    {
                        client.Send(new CharacterSelectedErrorMessage());
                        return;
                    }

                    /* Set Colors */
                    var breedColors = character.Sex == SexTypeEnum.SEX_MALE ? breed.MaleColors : breed.FemaleColors;
                    var m_colors = new Dictionary<int, Color>();

                    foreach (var color in remodel.colors)
                    {
                        var index = color >> 24;
                        var c = Color.FromArgb(color);

                        m_colors.Add(index, c);
                    }

                    var i = 1;
                    foreach (var breedColor in breedColors)
                    {
                        if (!m_colors.ContainsKey(i))
                            m_colors.Add(i, Color.FromArgb((int)breedColor));

                        i++;
                    }
                    character.DefaultLook.SetColors(m_colors.Select(x => x.Key).ToArray(), m_colors.Select(x => x.Value).ToArray());
                }

                character.MandatoryChanges = 0;
                character.PossibleChanges = 0;

                try
                {
                    WorldServer.Instance.DBAccessor.Database.Update(character);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error during database update: {ex.Message}");
                }

                /* Common selection */
                CommonCharacterSelection(client, character);
            });
        }

        public static void SendCharactersListWithRemodelingMessage(WorldClient client)
        {
            var characterBaseInformations = new List<CharacterBaseInformations>();
            var charactersToRemodel = new List<CharacterToRemodelInformations>();

            foreach (var characterRecord in client.Characters.Where(x => !x.IsDeleted).OrderByDescending(x => x.LastUsage))
            {
                if (characterRecord is null)
                {
                    logger.Error("Error SendCharactersListWithRemodelingMessage: " + client.Account.Nickname);
                    continue;
                }

                characterBaseInformations.Add(new CharacterBaseInformations((ulong)characterRecord.Id, characterRecord.Name, ExperienceManager.Instance.GetCharacterLevel(characterRecord.Experience, characterRecord.PrestigeRank), characterRecord.LastLook?.GetEntityLook() ?? characterRecord.DefaultLook.GetEntityLook(), (sbyte)characterRecord.Breed, characterRecord.Sex == SexTypeEnum.SEX_MALE, 0));

                if (((characterRecord.MandatoryChanges & (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_BREED)
                != (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_BREED)
                && ((characterRecord.MandatoryChanges & (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_COLORS)
                != (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_COLORS)
                && ((characterRecord.MandatoryChanges & (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_COSMETIC)
                != (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_COSMETIC)
                && ((characterRecord.MandatoryChanges & (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_GENDER)
                != (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_GENDER)
                && ((characterRecord.MandatoryChanges & (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_NAME)
                != (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_NAME))
                    continue;

                charactersToRemodel.Add(new CharacterToRemodelInformations((ulong)characterRecord.Id, characterRecord.Name, (sbyte)characterRecord.Breed, characterRecord.Sex != SexTypeEnum.SEX_MALE, (ushort)characterRecord.Head, characterRecord.DefaultLook.Colors.Values.Select(x => x.ToArgb()).ToArray(), characterRecord.PossibleChanges, characterRecord.MandatoryChanges));
            }

            WorldServer.Instance.IOTaskPool.ExecuteInContext(() =>
            {
                var source = Singleton<ItemManager>.Instance.FindStartupActions(client.Account.Id);

                client.Send(new CharactersListWithRemodelingMessage(characterBaseInformations.ToArray(), charactersToRemodel.ToArray()));
            });
        }
        #endregion

        #region >> Character SendMessage
        private static void CharacterSendOnLoginMessage(WorldClient Client)
        {
            #region // ----------------- Mensagens ao Logar by: Kenshin ---------------- //
            var UserGroup = Client.Character.UserGroup.Id;
            switch (Client.Character.Account.Lang)
            {
                case "fr":
                    if (!Client.Account.IsSubscribe)
                        Client.Character.SendServerMessage(string.Format("<b>[FACTURE]:</b> Vous êtes sans VIP <a href=\"https://serverhydra.com/fr/boutique/paiement/br/vip/choix-offre\"> <b>RENOUVELER VIP</b></a>"), Color.Red);
                    else
                        Client.Character.SendServerMessage(string.Format("<b>[FACTURE]:</b> Vous êtes actuellement <b>" + (RoleEnum)UserGroup + "</b>."), Color.SpringGreen);
                    Client.Character.SendServerMessage(string.Format("<b>[ÉQUILIBRE]:</b> Avez-vous actuellement <b>{0}</b> Ogrines.", Client.Account.Tokens), Color.SpringGreen);
                    if (Settings.Ogrines2xAnnounce == true)
                        Client.Character.SendServerMessage(string.Format("<b>[PROMOTION] :</b> 2x ogrines sont <b>ON</b>"), Color.SpringGreen);
                    break;

                case "es":
                    if (!Client.Account.IsSubscribe)
                        Client.Character.SendServerMessage(string.Format("<b>[CUENTA]:</b> Estás sin VIP <a href=\"https://serverhydra.com/es/tienda/pago/br/vip/elegir-oferta\"><b>RENOVAR VIP</b></a>"), Color.Red);
                    else
                        Client.Character.SendServerMessage(string.Format("<b>[CUENTA]:</b> Actualmente estás <b>" + (RoleEnum)UserGroup + "</b>."), Color.SpringGreen);
                    Client.Character.SendServerMessage(string.Format("<b>[SALDO]:</b> Actualmente tienes <b>{0}</b> Ogrinas.", Client.Account.Tokens), Color.SpringGreen);
                    if (Settings.Ogrines2xAnnounce == true)
                        Client.Character.SendServerMessage(string.Format("<b>[PROMOCIÓN]:</b> 2x ogrinas están <b>ACTIVADAS</b>"), Color.SpringGreen);
                    break;

                case "en":
                    if (!Client.Account.IsSubscribe)
                        Client.Character.SendServerMessage(string.Format("<b>[BILL]:</b> You are without VIP <a href=\"https://serverhydra.com/en/shop/payment/br/vip/choose-offer\"><b>RENEW VIP</b></a>"), Color.Red);
                    else
                        Client.Character.SendServerMessage(string.Format("<b>[BILL]:</b> Are you currently <b>" + (RoleEnum)UserGroup + "</b>."), Color.SpringGreen);
                    Client.Character.SendServerMessage(string.Format("<b>[BALANCE]:</b> Do you currently have <b>{0}</b> Ogrines.", Client.Account.Tokens), Color.SpringGreen);
                    if (Settings.Ogrines2xAnnounce == true)
                        Client.Character.SendServerMessage(string.Format("<b>[PROMOTION]:</b> 2x ogrines are <b>ON</b>"), Color.SpringGreen);
                    break;

                default:
                    if (!Client.Account.IsSubscribe)
                        Client.Character.SendServerMessage(string.Format("<b>[CONTA]:</b> Você está sem VIP <a href=\"https://serverhydra.com/pt/loja/pagamento/br/vip/escolher-oferta\"><b>RENOVAR VIP</b></a>"), Color.Red);
                    else
                        Client.Character.SendServerMessage(string.Format("<b>[CONTA]:</b> Você é atualmente <b>" + (RoleEnum)UserGroup + "</b>."), Color.SpringGreen);
                    Client.Character.SendServerMessage(string.Format("<b>[SALDO]:</b> Você tem atualmente <b>{0}</b> Ogrines.", Client.Account.Tokens), Color.SpringGreen);
                    if (Settings.Ogrines2xAnnounce == true)
                        Client.Character.SendServerMessage(string.Format("<b>[PROMOÇÃO]:</b> Os ogrines 2x estão <b>ATIVADA</b>"), Color.SpringGreen);
                    break;
            }
            #endregion
        }

        private static void CheckEventOnLogin(WorldClient client)
        {
            if (client != null)
            {
                #region Evento de Login (Aniversario)
                //DateTime EventDate = new DateTime(2022, 11, 26).Date;

                //if (client.WorldAccount.ServerBirthday == 0 && EventDate == DateTime.Now.Date)
                //{
                //    var itemtemplateOne = ItemManager.Instance.TryGetTemplate(30386);
                //    var itemtemplateTwo = ItemManager.Instance.TryGetTemplate(27432);
                //    var itemtemplateThree = ItemManager.Instance.TryGetTemplate(Settings.TokenTemplateId);

                //    Gifts.Instance.SetGift(client.Account.Id, "GiftServerBirthday", itemtemplateOne, 1, false, false, false, true);
                //    Gifts.Instance.SetGift(client.Account.Id, "GiftServerBirthday", itemtemplateTwo, 1, false, false, false, true);
                //    Gifts.Instance.SetGift(client.Account.Id, "GiftServerBirthday", itemtemplateThree, 1, true, false, false, false);

                //    #region // ----------------- Sistema de Logs MongoDB Presente de Retorno by: Kenshin ---------------- //
                //    try
                //    {
                //        var document = new BsonDocument
                //            {
                //                { "HardwareId", client.Account.LastHardwareId },
                //                { "AccountId", client.Account.Id },
                //                { "AccountName", client.Account.Login },
                //                { "Fuction", "GiftServerBirthday" },
                //                { "Date", DateTime.Now.ToString(CultureInfo.CreateSpecificCulture("pt-BR")) }
                //            };

                //        MongoLogger.Instance.Insert("Player_ServerBirthday", document);
                //    }
                //    catch (Exception e)
                //    {
                //        Console.WriteLine("Erro no Mongologs Presente de Retorno : " + e.Message);
                //    }
                //    #endregion

                //    client.WorldAccount.ServerBirthday = 1;
                //}
                #endregion

                #region - Gift Return -
                if (IPCAccessor.Instance.IsConnected && Settings.EventReturn && client.Account.LastDays >= Settings.DaysEventReturn)
                {
                    int daysThreshold = Settings.DaysEventReturn;
                    var itemTemplate = ItemManager.Instance.TryGetTemplate(Settings.PrimaryItemIdReturn);

                    if (client.Account.LastDays >= daysThreshold && itemTemplate != null)
                    {
                        Gifts.Instance.SetGift(client.Account.Id, "GiftReturn", itemTemplate, 1, false, false, false, true, true);

                        #region // ----------------- Sistema de Logs MongoDB Presente de Retorno by: Kenshin ---------------- //
                        try
                        {
                            var document = new BsonDocument
                            {
                                { "HardwareId", client.Account.LastHardwareId },
                                { "AccountId", client.Account.Id },
                                { "AccountName", client.Account.Login },
                                { "Fuction", "GiftReturn" },
                                { "LastDays", client.Account.LastDays },
                                { "ItemId", itemTemplate.Id },
                                { "Item", itemTemplate.Name },
                                { "Date", DateTime.Now.ToString(CultureInfo.CreateSpecificCulture("pt-BR")) }
                            };

                            MongoLogger.Instance.Insert("Player_GiftReturn", document);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Erro no Mongologs Presente de Retorno : " + e.Message);
                        }
                        #endregion

                        IPCAccessor.Instance.Send(new AccountLastDaysMessage(client.Account, 0));
                    }
                }
                #endregion
            }
        }

        static CharacterRecord FindCharacterFightReconnection(WorldClient client)
            => (from characterInFight in client.Characters.Where(x => !x.IsDeleted).Where(x => x.LeftFightId != null)
                let fight = FightManager.Instance.GetFight(characterInFight.LeftFightId.Value)
                where fight != null
                let fighter = fight.GetLeaver(characterInFight.Id)
                where fighter != null
                select characterInFight).FirstOrDefault();

        public static void SendCharactersListMessage(WorldClient client)
        {
            var characters = client.Characters.Where(x => !x.IsDeleted).OrderByDescending(x => x.LastUsage).Select(
                characterRecord =>
                new CharacterBaseInformations(
                    (ulong)characterRecord.Id,
                    characterRecord.Name,
                    ExperienceManager.Instance.GetCharacterLevel(characterRecord.Experience, characterRecord.PrestigeRank),
                    characterRecord.LastLook.GetEntityLook(),
                    (sbyte)characterRecord.Breed,
                    characterRecord.Sex != SexTypeEnum.SEX_MALE,
                    0)).ToList();

            WorldServer.Instance.IOTaskPool.ExecuteInContext(() =>
            {
                var source = Singleton<ItemManager>.Instance.FindStartupActions(client.Account.Id);
                client.Send(new CharactersListMessage(characters));
            });
        }

        public static void SendCharacterSelectedSuccessMessage(WorldClient client)
        {
            client.Send(new CharacterSelectedSuccessMessage(client.Character.GetCharacterBaseInformations(), false));
        }

        public static void SendCharacterSelectedForceMessage(IPacketReceiver client, int id)
        {
            client.Send(new CharacterSelectedForceMessage(id));
        }

        public static void SendCharacterCapabilitiesMessage(WorldClient client)
        {
            client.Send(new CharacterCapabilitiesMessage(4095));
        }

        public static void SendCharacterLoadingCompleteMessage(WorldClient client)
        {
            client.Send(new CharacterLoadingCompleteMessage());
        }
        #endregion
    }
}