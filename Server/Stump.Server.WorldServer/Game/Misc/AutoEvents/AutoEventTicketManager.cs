using NLog;
using Stump.Server.BaseServer;
using Stump.Server.BaseServer.Database;
using Stump.Server.BaseServer.Initialization;
using Stump.Server.WorldServer.Database.Misc;
using Stump.Server.WorldServer.Discord;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Stump.Server.WorldServer.Game.Misc.AutoEvents
{
    public class AutoEventTicketManager : DataManager<AutoEventTicketManager>
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private Dictionary<int, EventTicketRecord> m_PlayersTicketsEvent = new Dictionary<int, EventTicketRecord>();

        [Initialization(InitializationPass.Fifth)]
        public override void Initialize()
        {
            m_PlayersTicketsEvent = Database.Query<EventTicketRecord>(EventTicketRelator.FetchQuery).ToDictionary(x => x.Id);
        }

        public int HasPlayerRewardRecord(long accountid, long Lasttime)
        {
            if (accountid == 0 || Lasttime == 0)
                return 0;

            if (m_PlayersTicketsEvent.Any(x => x.Value.AccountId == accountid))
            {
                var EventTicketRecord = m_PlayersTicketsEvent.FirstOrDefault(x => x.Value.AccountId == accountid);

                if (EventTicketRecord.Value != null)
                {
                    long timeActual = Lasttime - EventTicketRecord.Value.GameTimeInSeconds;

                    if (timeActual < 0)
                        return 0;

                    int numberOfTwoHours = (int)(timeActual / (2 * 3600));

                    return numberOfTwoHours > 0 ? numberOfTwoHours : 0;
                }
            }
            else
            {
                int numberOfTwoHours = (int)(Lasttime / (2 * 3600));

                return numberOfTwoHours > 0 ? numberOfTwoHours : 0;
            }

            return 0;
        }

        public void SetPlayerRecord(long accountid, string accountName, long time, int quantity)
        {
            var existingRecord = m_PlayersTicketsEvent.FirstOrDefault(entry => entry.Value.AccountId == accountid);

            WorldServer.Instance.IOTaskPool.AddMessage(() =>
            {
                if (existingRecord.Key > 0)
                {
                    existingRecord.Value.GameTimeInSeconds = time;
                    existingRecord.Value.TicketsQuantity += quantity;
                    existingRecord.Value.Date = DateTime.Now;
                    existingRecord.Value.IsUpdate = true;
                }
                else
                {
                    EventTicketRecord record = new EventTicketRecord()
                    {
                        Id = EventTicketRecord.PopNextId(),
                        AccountId = accountid,
                        AccountName = accountName,
                        GameTimeInSeconds = time,
                        TicketsQuantity = quantity,
                        Date = DateTime.Now,
                        IsNew = true
                    };

                    m_PlayersTicketsEvent.Add(record.Id, record);
                }

                Instance.Save(ServerBase<WorldServer>.Instance.DBAccessor.Database, accountid);
            });
        }

        public void SendMessageWebHook(Character character, int ticketQuantity, int hoursInGame)
        {
            if (DiscordIntegration.EnableDiscordWebHook)
            {
                string text = "[UNKNOW_TEXT_WEBHOOK]";

                switch (character.Account.Lang)
                {
                    case "fr":
                        text = $"﻿Le joueur **{character.Name}** vient de gagner **{ticketQuantity}x [Billet Fortune Mystique]** pour avoir passé le plus grand nombre **{hoursInGame} d'heures** à jouer.";
                        break;
                    case "es":
                        text = $"﻿El jugador **{character.Name}** acaba de ganar **{ticketQuantity}x [Ticket de la Fortuna Mística]** por pasar el mayor número de **{hoursInGame} horas** jugando.";
                        break;
                    case "en":
                        text = $"﻿Player **{character.Name}** has just won **{ticketQuantity}x [Mystic Fortune Ticket]** for spending the most **{hoursInGame} hours** playing.";
                        break;
                    default:
                        text = $"﻿O Jogador **{character.Name}** acabou de ganhar **{ticketQuantity}x [Ticket da Fortuna Mística]** por passar mais **{hoursInGame} horas** jogando.";
                        break;
                }

                PlainText.SendWebHook(DiscordIntegration.DiscordChatGlobalUrl, text, DiscordIntegration.DiscordWHUsername);
            }
        }

        public void Save(ORM.Database database, long accountId)
        {
            WorldServer.Instance.IOTaskPool.AddMessage(() =>
            {
                try
                {
                    foreach (var record in m_PlayersTicketsEvent.Values.Where(record => record.AccountId == accountId))
                    {
                        if (record.IsUpdate && !record.IsNew)
                        {
                            database.Update(record);
                            record.IsUpdate = false;
                        }
                        else if (record.IsNew)
                        {
                            database.Insert(record);
                            record.IsNew = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error($"Error saving AutoEventTicketManager: {ex.Message}");
                }
            });
        }
    }
}