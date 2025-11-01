using System.Collections.Generic;
using Stump.Server.BaseServer.Database;
using Stump.Server.BaseServer.Initialization;
using NLog;
using Stump.Server.WorldServer.Database.Items.Usables;

namespace Stump.Server.WorldServer.Game.Fairy
{
    class ItemsFairyManager : DataManager<ItemsFairyManager>
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly Dictionary<int, ItemsFairynRecord> m_itemsfairy = new Dictionary<int, ItemsFairynRecord>();

        [Initialization(InitializationPass.Any)]
        public override void Initialize()
        {
            #region >> Limpando as variaveis para que o comando Reload não duplique elas.

            if (m_itemsfairy != null)
                m_itemsfairy.Clear();

            #endregion

            foreach (var item in Database.Query<ItemsFairynRecord>(ItemsFairyRelator.FetchQuery))
            {
                if (m_itemsfairy.ContainsKey(item.ItemId))
                    m_itemsfairy.Remove(item.ItemId);

                m_itemsfairy.Add(item.ItemId, item);
            }
        }

        public ItemsFairynRecord GetFairyItemById(int itemID)
        {
            foreach (var item in m_itemsfairy)
            {
                if (item.Value.ItemId == itemID)
                    return item.Value;
            }
            return null;
        }

        public Dictionary<int, ItemsFairynRecord> GetItemsUsables()
        {
            return m_itemsfairy;
        }
    }
}
