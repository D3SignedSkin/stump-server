using System.Linq;
using Stump.DofusProtocol.Messages;
using Stump.Server.WorldServer.Core.Network;

namespace Stump.Server.WorldServer.Handlers.Inventory
{
    public partial class InventoryHandler
    {
        public static void SendSpellListMessage(WorldClient client, bool previsualization)
        {
            var spellsList = client.Character.Spells.GetPlayableSpells().Select(entry => entry.GetSpellItem());

            client.Send(new SpellListMessage(previsualization, spellsList));
        }

        public static void SendSpellVariantActivationMessage(WorldClient client, int spellId, bool result)
        {
            client.Send(new SpellVariantActivationMessage((ushort)spellId, result));
        }
    }
}