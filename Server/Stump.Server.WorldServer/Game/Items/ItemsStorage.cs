using System;

namespace Stump.Server.WorldServer.Game.Items
{
    public class ItemsStorage<T> : PersistantItemsCollection<T>
        where T : IPersistantItem
    {
        public event Action<ItemsStorage<T>, long> KamasAmountChanged;

        private void NotifyKamasAmountChanged(long kamas)
        {
            OnKamasAmountChanged(kamas);
            KamasAmountChanged?.Invoke(this, kamas);
        }

        protected virtual void OnKamasAmountChanged(long amount)
        {
        }

        public long AddKamas(long amount)
        {
            if (amount == 0)
                return 0;

            return SetKamas(Kamas + amount);
        }

        public long SubKamas(long amount)
        {
            if (amount == 0)
                return 0;

            return SetKamas(Kamas - amount);
        }

        public virtual long SetKamas(long amount)
        {
            var oldKamas = Kamas;

            if (amount < 0)
                amount = 0;

            Kamas = amount;
            NotifyKamasAmountChanged(amount - oldKamas);
            return amount - oldKamas;
        }

        public virtual long Kamas
        {
            get;
            protected set;
        }
    }
}