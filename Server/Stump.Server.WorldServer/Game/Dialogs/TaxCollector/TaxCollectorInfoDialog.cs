using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Messages;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.TaxCollectors;
using Stump.Server.WorldServer.Handlers.Dialogs;

namespace Stump.Server.WorldServer.Game.Dialogs.TaxCollector
{
    public class TaxCollectorInfoDialog : IDialog
    {
        public TaxCollectorInfoDialog(Character character, TaxCollectorNpc taxCollector)
        {
            TaxCollector = taxCollector;
            Character = character;
        }

        public TaxCollectorNpc TaxCollector
        {
            get;
            private set;
        }

        public Character Character
        {
            get;
            private set;
        }

        public DialogTypeEnum DialogType
        {
            get { return DialogTypeEnum.DIALOG_DIALOG; }
        }

        public void Close()
        {
            Character.CloseDialog(this);
            TaxCollector.OnDialogClosed(this);

            DialogHandler.SendLeaveDialogMessage(Character.Client, DialogType);
        }

        public void Open()
        {
            Character.SetDialog(this);
            TaxCollector.OnDialogOpened(this);

            Character.Client.Send(new NpcDialogCreationMessage(TaxCollector.Map.Id, TaxCollector.Id));

            var _taxCollector = new TaxCollectorDialogQuestionExtendedMessage(
                allianceInfo: TaxCollector.Guild.Alliance is null ? new DofusProtocol.Types.BasicAllianceInformations() : TaxCollector.Guild.Alliance.GetBasicAllianceInformations(),
                maxPods: (ushort)TaxCollector.Guild.TaxCollectorPods,
                prospecting: (ushort)TaxCollector.Guild.TaxCollectorProspecting,
                alliance: TaxCollector.Guild.Alliance is null ? new DofusProtocol.Types.BasicNamedAllianceInformations() : TaxCollector.Guild.Alliance.GetBasicNamedAllianceInformations(),
                taxCollectorsCount: (sbyte)TaxCollector.Guild.TaxCollectors.Count,
                taxCollectorAttack: 0,
                pods: (uint)TaxCollector.Bag.BagWeight,
                itemsValue: (ulong)TaxCollector.Bag.BagValue);

            Character.Client.Send(_taxCollector);
        }
    }
}