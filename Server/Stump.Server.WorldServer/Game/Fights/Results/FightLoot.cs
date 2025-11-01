using System.Linq;
using Stump.DofusProtocol.Types;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Stump.Server.WorldServer.Game.Items;

namespace Stump.Server.WorldServer.Game.Fights.Results
{
    public class FightLoot
    {
        private readonly Dictionary<int, DroppedItem> m_items = new Dictionary<int, DroppedItem>();

        public FightLoot()
        { }

        public IReadOnlyDictionary<int, DroppedItem> Items
        {
            get { return new ReadOnlyDictionary<int, DroppedItem>(m_items); }
        }

        public long Kamas { get; set; }

        public long Experience { get; set; }

        public void AddItem(int itemId)
        {
            AddItem(itemId, 1);
        }

        public void AddItem(int itemId, uint amount)
        {
            if (m_items.ContainsKey(itemId))
            {
                m_items[itemId].Amount++;
            }
            else
            {
                m_items.Add(itemId, new DroppedItem(itemId, amount));
            }
        }

        public void AddItem(DroppedItem item)
        {
            if (m_items.ContainsKey(item.ItemId))
            {
                m_items[item.ItemId].Amount += item.Amount;
            }
            else
            {
                m_items.Add(item.ItemId, item);
            }
        }

        public DofusProtocol.Types.FightLoot GetFightLoot()
        {
            List<FightLootObject> _listObjects = new List<FightLootObject>();

            foreach (var item in m_items.Values)
            {
                _listObjects.Add(new FightLootObject(objectId: item.ItemId, quantity: (int)item.Amount, priorityHint: 0)); //TODO - 2.71 Drop Priority
            }

            return new DofusProtocol.Types.FightLoot(_listObjects, (ulong)Kamas);
        }

        public string FightItemsString()
        {
            return string.Join("|", m_items.Select(item => item.Value.ItemId + "_" + item.Value.Amount + "_" + 0).ToList());
        }
    }
}