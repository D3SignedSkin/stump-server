using System.Linq;
using System.Collections.Generic;
using Stump.Server.BaseServer.Database;
using Stump.Server.BaseServer.Initialization;
using Stump.Server.WorldServer.Database.Progression;

namespace Stump.Server.WorldServer.Game.Progression
{
    class ActivitySuggestionsManager : DataManager<ActivitySuggestionsManager>
    {
        private List<int> _ignoreCategory = new List<int>
        {
            5, //Quest
            13, //Eventos
        };

        private Dictionary<int, ActivitySuggestionRecord> m_suggestions = new Dictionary<int, ActivitySuggestionRecord>();

        [Initialization(InitializationPass.Third)]
        public override void Initialize()
        {
            m_suggestions = Database.Query<ActivitySuggestionRecord>(ActivitySuggestionRelator.FetchQuery).ToDictionary(entry => entry.Id);
        }

        public List<ActivitySuggestionRecord> GetSuggestions()
        {
            return m_suggestions.Values.ToList();
        }

        public List<ActivitySuggestionRecord> GetSuggestionsAllCatAllArea(ushort minLevel, ushort maxLevel)
        {
            return m_suggestions.Values.Where(x => x.Level >= minLevel && x.Level <= maxLevel && !_ignoreCategory.Contains(x.CategoryId)).ToList();
        }

        public List<ActivitySuggestionRecord> GetSuggestionsByCatAllArea(ushort catId, ushort AreaId, ushort minLevel, ushort maxLevel)
        {
            return m_suggestions.Values.Where(x => x.CategoryId == catId && x.Level >= minLevel && x.Level <= maxLevel && !_ignoreCategory.Contains(x.CategoryId)).ToList();
        }

        public List<ActivitySuggestionRecord> GetSuggestionsAllCatByArea(ushort subAreaId, ushort minLevel, ushort maxLevel)
        {
            var mapsSubArea = World.Instance.GetMapsByArea(subAreaId);

            return m_suggestions.Values.Where(x => x.Level >= minLevel && x.Level <= maxLevel && !_ignoreCategory.Contains(x.CategoryId) && mapsSubArea.Any(y => y.Id == x.Id)).ToList();
        }

        public List<ActivitySuggestionRecord> GetSuggestionsByCatByArea(ushort catId, ushort AreaId, ushort minLevel, ushort maxLevel)
        {
            var mapsSubArea = World.Instance.GetMapsByArea(AreaId);

            return m_suggestions.Values.Where(x => x.CategoryId == catId && x.Level >= minLevel && x.Level <= maxLevel && !_ignoreCategory.Contains(x.CategoryId) && mapsSubArea.Any(y => y.Id == x.MapId)).ToList();
        }
    }
}