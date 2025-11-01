using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Messages;
using Stump.DofusProtocol.Types;
using Stump.Server.WorldServer.Core.Network;
using Stump.Server.WorldServer.Game.Presets;

namespace Stump.Server.WorldServer.Handlers.Inventory
{
    public partial class InventoryHandler : WorldHandlerContainer
    {
        [WorldHandler(IconNamedPresetSaveRequestMessage.Id)]
        public static void HandleCharacterPresetSaveRequestMessage(WorldClient client, IconNamedPresetSaveRequestMessage message)
        {
            PresetsManager.Instance.SavePreset(client.Character, PresetId: message.presetId, SymbolId: message.symbolId, Name: message.name, UpdateData: message.updateData, Type: message.type);
        }

        [WorldHandler(PresetUseRequestMessage.Id)]
        public static void HandlePresetUseRequestMessage(WorldClient client, PresetUseRequestMessage message)
        {
            PresetsManager.Instance.UsePreset(client.Character, message.presetId, client.Character.Id);
        }

        [WorldHandler(PresetDeleteRequestMessage.Id)]
        public static void HandlePresetDeleteRequestMessage(WorldClient client, PresetDeleteRequestMessage message)
        {
            PresetsManager.Instance.DeletePreset(client.Character, message.presetId, client.Character.Id);
        }

        public static void SendPresetSavedMessage(WorldClient client, short presetId, Preset preset, Preset[] presets) //Modifiquei de IPacketReceiver para WorldClient
        {
            client.Send(new PresetSavedMessage(presetId, preset));
        }

        public static void PresetDeletedMessage(WorldClient client, short PresetId, PresetDeleteResultEnum result)
        {
            client.Send(new PresetDeleteResultMessage(PresetId, (sbyte)result));
        }

        public static void PresetUsedMessage(WorldClient client, short PresetId, PresetUseResultEnum result)
        {
            client.Send(new PresetUseResultMessage(PresetId, (sbyte)result));
        }
        public static void PresetUsedMessage(WorldClient client, short PresetId, ItemForPreset Item)
        {
            client.Send(new ItemForPresetUpdateMessage(PresetId, Item));
        }

        public static void SendPresetsListMessage(WorldClient client, Preset[] preset)
        {
            client.Send(new PresetsMessage(preset));
        }

        //[WorldHandler(IdolsPresetSaveRequestMessage.Id)]
        //public static void HandleCharacterIdolsPresetSaveRequestMessage(WorldClient client, IdolsPresetSaveRequestMessage message) //TODO - Sistema de Idolos
        //{
        //    //PresetsManager.Instance.SaveIdolsPreset(client.Character, message.presetId, message.symbolId, message.updateData);
        //}
    }
}