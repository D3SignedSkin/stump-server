using System.Linq;
using System.Collections.Generic;
using Stump.DofusProtocol.Messages;
using Stump.Server.BaseServer.Network;
using Stump.Server.WorldServer.Core.Network;
using Stump.Server.WorldServer.Game.Actors.RolePlay.TaxCollectors;
using Stump.Server.WorldServer.Game.Formulas;
using Stump.Server.WorldServer.Game.Items;
using Stump.Server.WorldServer.Game.Items.Player;

namespace Stump.Server.WorldServer.Handlers.Inventory
{
    public partial class InventoryHandler
    {
        public static void SendStorageInventoryContentMessage(IPacketReceiver client, TaxCollectorNpc taxCollector)
        {
            client.Send(taxCollector.GetStorageInventoryContent());
        }

        public static void SendStorageInventoryContentMessage(IPacketReceiver client, Bank bank)
        {
            var itemPrices = new Dictionary<uint, ulong>();

            foreach (var item in bank)
            {
                if (!itemPrices.ContainsKey(item.GetObjectItem().objectGID))
                    itemPrices.Add(item.GetObjectItem().objectGID, PriceFormulas.getItemPrice(item.Template.Id));
            }

            foreach (var item in ((WorldClient)client).Character.Inventory)
            {
                if (!itemPrices.ContainsKey(item.GetObjectItem().objectGID))
                    itemPrices.Add(item.GetObjectItem().objectGID, PriceFormulas.getItemPrice(item.Template.Id));
            }

            client.Send(new ObjectAveragePricesMessage(itemPrices.Keys.ToArray(), itemPrices.Values.ToArray()));
            client.Send(new StorageInventoryContentMessage(bank.Select(x => x.GetObjectItem()).ToArray(), (ulong)bank.Kamas));
        }

        public static void SendStorageKamasUpdateMessage(IPacketReceiver client, long kamas)
        {
            client.Send(new StorageKamasUpdateMessage((ulong)kamas));
        }

        public static void SendStorageObjectRemoveMessage(IPacketReceiver client, IItem item)
        {
            client.Send(new StorageObjectRemoveMessage((uint)item.Guid));
        }

        public static void SendStorageObjectUpdateMessage(IPacketReceiver client, IItem item)
        {
            client.Send(new StorageObjectUpdateMessage(item.GetObjectItem()));
        }

        public static void SendStorageObjectsRemoveMessage(IPacketReceiver client, IEnumerable<int> guids)
        {
            client.Send(new StorageObjectsRemoveMessage(guids.Select(x => (uint)x).ToArray()));
        }

        public static void SendStorageObjectsUpdateMessage(IPacketReceiver client, IEnumerable<IItem> items)
        {
            client.Send(new StorageObjectsUpdateMessage(items.Select(x => x.GetObjectItem()).ToArray()));
        }
    }
}