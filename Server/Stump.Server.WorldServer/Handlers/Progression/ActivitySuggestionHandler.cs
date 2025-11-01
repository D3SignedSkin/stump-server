using Stump.DofusProtocol.Messages;
using Stump.Server.WorldServer.Core.Network;
using Stump.Server.WorldServer.Database.Progression;
using Stump.Server.WorldServer.Game.Progression;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Stump.Server.WorldServer.Handlers.Progression
{
    public class ActivitySuggestionsHandler : WorldHandlerContainer
    {
        [WorldHandler(ActivitySuggestionsRequestMessage.Id)]
        public static void HandleActivitySuggestionsRequestMessage(WorldClient client, ActivitySuggestionsRequestMessage message)
        {
            List<ushort> locked = new List<ushort>();
            List<ushort> unlocked = new List<ushort>();
            DateTimeOffset currentDateTimeOffset = DateTimeOffset.Now;

            var suggestions = new List<ActivitySuggestionRecord>();

            if (message.activityCategoryId == 0 && message.areaId != 32767)
            {
                suggestions = ActivitySuggestionsManager.Instance.GetSuggestionsAllCatByArea(message.areaId, message.minLevel, message.maxLevel);
            }
            else if (message.activityCategoryId != 0 && message.areaId == 32767)
            {
                suggestions = ActivitySuggestionsManager.Instance.GetSuggestionsByCatAllArea(message.activityCategoryId, message.areaId, message.minLevel, message.maxLevel);
            }
            else if (message.activityCategoryId != 0 && message.areaId != 32767)
            {
                suggestions = ActivitySuggestionsManager.Instance.GetSuggestionsByCatByArea(message.activityCategoryId, message.areaId, message.minLevel, message.maxLevel);
            }
            else
            {
                suggestions = ActivitySuggestionsManager.Instance.GetSuggestionsAllCatAllArea(message.minLevel, message.maxLevel);
            }

            var randomResults = suggestions.OrderBy(x => Guid.NewGuid()).Take(message.nbCards).ToList();

            foreach (var result in randomResults)
            {
                if (result.StartDate > 0 && result.EndDate > 0)
                {
                    long startTimestamp = result.StartDate;
                    long endTimestamp = result.EndDate;

                    DateTimeOffset startDate = ConvertLongToDateTimeOffset(startTimestamp);
                    DateTimeOffset endDate = ConvertLongToDateTimeOffset(endTimestamp);

                    if (startDate < endDate && startDate <= currentDateTimeOffset && currentDateTimeOffset <= endDate)
                    {
                        unlocked.Add((ushort)result.Id);
                    }
                }
                else
                {
                    unlocked.Add((ushort)result.Id);
                }
            }

            client.Send(new ActivitySuggestionsMessage(lockedActivitiesIds: locked, unlockedActivitiesIds: unlocked));
        }

        [WorldHandler(ActivityLockRequestMessage.Id)]
        public static void HandleActivityLockRequestMessage(WorldClient client, ActivityLockRequestMessage message)
        {
            /*
                Salvar em uma coluna na tabela de personagem.. filtrar na hora de puxar a lista para não aparecer elas e adicionar a lista Locked.
             */

            client.Character.SendServerMessageLang(
                "Função está sendo criada pela equipe do projeto. Favor tentar novamente em um outro momento.",
                "Function is being created by the project team. Please try again at another time.",
                "La función está siendo creada por el equipo del proyecto. Por favor, inténtelo de nuevo en otro momento.",
                "La fonction est créée par l’équipe du projet. Veuillez réessayer à un autre moment.");
        }

        [WorldHandler(ActivityHideRequestMessage.Id)]
        public static void HandleActivityHideRequestMessage(WorldClient client, ActivityHideRequestMessage message)
        {
            /*
                Salvar em uma Variavel no personagens todos que estiverem ocultas.. assim, após o reinicio do personagem essa variavel será limpa.
             */

            client.Character.SendServerMessageLang(
                "Função está sendo criada pela equipe do projeto. Favor tentar novamente em um outro momento.",
                "Function is being created by the project team. Please try again at another time.",
                "La función está siendo creada por el equipo del proyecto. Por favor, inténtelo de nuevo en otro momento.",
                "La fonction est créée par l’équipe du projet. Veuillez réessayer à un autre moment.");
        }

        private static DateTimeOffset ConvertLongToDateTimeOffset(long timestamp)
        {
            DateTime start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime date = start.AddMilliseconds(timestamp).ToLocalTime();

            return date;
        }
    }
}