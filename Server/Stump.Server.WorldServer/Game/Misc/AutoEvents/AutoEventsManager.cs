using MongoDB.Bson;
using Stump.Core.Attributes;
using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Database;
using Stump.Server.BaseServer.Initialization;
using Stump.Server.BaseServer.Logging;
using Stump.Server.WorldServer.Database.Accounts;
using Stump.Server.WorldServer.Database.Npcs;
using Stump.Server.WorldServer.Discord;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Npcs;
using Stump.Server.WorldServer.Game.Items;
using Stump.Server.WorldServer.Game.Maps;
using Stump.Server.WorldServer.Game.Maps.Cells;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;

namespace Stump.Server.WorldServer.Game.Misc.AutoEvents
{
    public class AutoEventsManager : DataManager<AutoEventsManager>
    {
        [Variable]
        public static int MinimalEventsDelaySeconds = 10;
        [Variable]
        public static int AnnouncePegaPegaInformationsSeconds = 60;

        private Dictionary<int, WorldAccount> m_worldaccounts;

        [Initialization(InitializationPass.Any)]
        public override void Initialize()
        {
            m_worldaccounts = Database.Query<WorldAccount>(WorldAccountRelator.FetchQuery).ToDictionary(x => x.Id);

            WorldServer.Instance.IOTaskPool.CallPeriodically(MinimalEventsDelaySeconds * 1000, CheckEvents);
            WorldServer.Instance.IOTaskPool.CallPeriodically(AnnouncePegaPegaInformationsSeconds * 1000, PegaPegaInformations);
            WorldServer.Instance.IOTaskPool.CallPeriodically(300 * 1000, SetWebHookNotificationOnlines);
        }

        private Npc _TroolNpc;

        private void CheckEvents()
        {
            Map mapNpcTrool = World.Instance.GetMap(191105026);

            if (_TroolNpc == null)
            {
                string messageError;

                if (Settings.CampKoliseu || Settings.PegaPega) //Registro de Campeonato de Koliseu e Evento Pega-Pega
                {
                    if (TrySpawnNpc(7022, mapNpcTrool, 281, DirectionsEnum.DIRECTION_SOUTH_EAST, out messageError))
                    {
                        _TroolNpc = mapNpcTrool.Actors.OfType<Npc>().FirstOrDefault(n => n.Template.Id == 7022);
                        SendStaffMessage($"O NPC '{_TroolNpc.Template.Name}' foi adicionado no Mapa '{_TroolNpc.Map.Id}'.");
                    }
                    else
                    {
                        Console.WriteLine(messageError);
                    }
                }
            }
            else
            {
                if (!Settings.CampKoliseu && !Settings.PegaPegaNpc)
                {
                    UnSpawnNpc(_TroolNpc.Template.Id, mapNpcTrool);
                    SendStaffMessage($"O NPC '{_TroolNpc.Template.Name}' foi removido do Mapa '{_TroolNpc.Map.Id}'.");

                    _TroolNpc = null;
                }
            }
        }

        #region >> Discord WebHook Notifications
        private void SetWebHookNotificationOnlines()
        {
            if (DiscordIntegration.EnableDiscordWebHook)
            {
                try
                {
                    var clientsOnline = WorldServer.Instance.ClientManager.Count;
                    var recordsClient = ServerInfoManager.Instance.GetRecord();
                    var timeDown = WorldServer.Instance.DownTime.ToString(@"hh\:mm\:ss");

                    PlainText.SendWebHook(DiscordIntegration.DiscordChatStaffUrl, $"Tempo: {timeDown} Jogadores Online: {clientsOnline} Record Online: {recordsClient}", DiscordIntegration.DiscordWHUsername);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error SetWebHookNotificationOnlines");
                }
            }
        }
        #endregion

        #region >> Server Birthday 
        private void CheckServerBirthday()
        {
            if (!m_worldaccounts.Any())
                return;

            DateTime EventDate = new DateTime(2022, 11, 26).Date;
            List<WorldAccount> EventAccount = new List<WorldAccount>();

            foreach (var Account in m_worldaccounts.Values.Where(x => x.LastConnection.Value.Date == EventDate).OrderBy(y => y.Id))
            {
                if (Account == null)
                    return;

                EventAccount.Add(Account);
            }

            foreach (var AccountGift in EventAccount.OrderBy(x => x.Id))
            {
                SetGift(AccountGift);
            }
        }

        private void SetGift(WorldAccount Account)
        {
            DateTime EventDate = new DateTime(2022, 11, 26).Date;

            if (Account.LastConnection.Value.Date == EventDate)
            {
                var itemtemplateOne = ItemManager.Instance.TryGetTemplate(30386);
                var itemtemplateTwo = ItemManager.Instance.TryGetTemplate(27432);

                Gifts.Gifts.Instance.SetGift(Account.Id, "GiftServerBirthday", itemtemplateOne, 1, false, false, false, true);
                Gifts.Gifts.Instance.SetGift(Account.Id, "GiftServerBirthday", itemtemplateTwo, 1, false, false, false, true);

                #region // ----------------- Sistema de Logs MongoDB Presente de Retorno by: Kenshin ---------------- //
                try
                {
                    var document = new BsonDocument
                            {
                                { "AccountId", Account.Id },
                                { "AccountName", Account.Nickname },
                                { "ItemOne", itemtemplateOne.Name },
                                { "ItemTwo", itemtemplateTwo.Name },
                                { "Fuction", "GiftServerBirthday" },
                                { "Date", DateTime.Now.ToString(CultureInfo.CreateSpecificCulture("pt-BR")) }
                            };

                    MongoLogger.Instance.Insert("Player_ServerBirthday", document);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Erro no Mongologs Presente de Retorno : " + e.Message);
                }
                #endregion
            }
        }
        #endregion

        #region >> Pega-Pega Informações
        private void PegaPegaInformations()
        {
            if (Settings.PegaPega)
            {
                int CountCharacter = 0;

                foreach (var Map in World.Instance.GetMaps().Where(x => x.SubArea.Id == 84)) //Efetua a contagem dos jogadores na subarea
                {
                    CountCharacter += Map.GetAllCharacters().Where(x => !x.IsInFight() && x.UserGroup.Role <= RoleEnum.Gold_Vip).Count();
                }

                World.Instance.ForEachCharacter(character =>
                {
                    if (character.UserGroup.Role >= RoleEnum.Moderator_Helper && character.Map.SubArea.Id == 84)
                    {
                        character.SendServerMessageLangColor(
                            "<b>[PEGA-PEGA]</b> O Mapa evento possui " + CountCharacter + " personagens restantes.",
                            "<b>[PEGA-PEGA]</b> Event Map has " + CountCharacter + " characters left.",
                            "<b>[PEGA-PEGA]</b> Al mapa de eventos le quedan caracteres " + CountCharacter + ".",
                            "<b>[PEGA-PEGA]</b> La carte d'événement a " + CountCharacter + " caractères restants.",
                            Color.Yellow);
                    }
                });
            }
        }
        #endregion

        #region >> Spawn NPC
        private bool TrySpawnNpc(int npcId, Map mapId, short cellId, DirectionsEnum direction, out string errorMessage)
        {
            errorMessage = string.Empty;
            NpcTemplate npcTemplate = NpcManager.Instance.GetNpcTemplate(npcId);

            if (npcTemplate == null)
            {
                errorMessage = $"Não foi possível encontrar um modelo de NPC com o ID {npcId}.";
                return false;
            }

            ObjectPosition position = new ObjectPosition(mapId, cellId, direction);
            Npc npc = null;

            try
            {
                npc = position.Map.SpawnNpc(npcTemplate, position, npcTemplate.Look);
            }
            catch (Exception ex)
            {
                errorMessage = $"Não foi possível criar o NPC com o ID {npcId}: {ex.Message}";
                return false;
            }

            return true;
        }
        #endregion

        #region >> Unspawn NPC
        private void UnSpawnNpc(int NpcId, Map Map)
        {
            foreach (var current in World.Instance.GetMaps())
            {
                List<Npc> NpcsToDelete = new List<Npc>();

                var Npc = current.Actors.Where(x => x is Npc && x.Map.Id == Map.Id);

                foreach (var npc in Npc)
                {
                    if (current.GetActor<Npc>(npc.Id).Template.Id == NpcId)
                        NpcsToDelete.Add(npc as Npc);
                }

                foreach (var npc in NpcsToDelete)
                {
                    current.UnSpawnNpc(npc);
                }
            }
        }
        #endregion

        #region >> ENVIAR MENSAGENS
        private void SendStaffMessage(string message)
        {
            World.Instance.ForEachCharacter(character =>
            {
                if (IsStaffOrHigher(character))
                {
                    character.SendServerMessage($"MSG STAFF: {message}");
                }
            });
        }
        #endregion >> ENVIAR MENSAGENS

        #region >> VERIFICAÇÕES
        private bool IsStaffOrHigher(Character character)
        {
            return character.UserGroup.Role >= RoleEnum.Moderator_Helper;
        }
        #endregion >> VERIFICAÇÕES
    }
}