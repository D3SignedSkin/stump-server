using System.Collections.Generic;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Mounts;
using Stump.Server.WorldServer.Game.Interactives;
using Stump.Server.WorldServer.Handlers.Inventory;
using MapPaddock = Stump.Server.WorldServer.Game.Maps.Paddocks.Paddock;

namespace Stump.Server.WorldServer.Game.Exchanges.Paddock
{
    public class PaddockExchange : IExchange
    {
        private readonly PaddockExchanger m_paddockExchanger;

        public PaddockExchange(Character character, MapPaddock paddock, InteractiveObject interactive)
        {
            Character = character;
            Paddock = paddock;
            m_paddockExchanger = new PaddockExchanger(character, paddock, this, interactive);
        }

        public Character Character
        {
            get;
            private set;
        }

        public MapPaddock Paddock
        {
            get;
            private set;
        }

        public ExchangeTypeEnum ExchangeType => ExchangeTypeEnum.MOUNT_STABLE;

        public DialogTypeEnum DialogType => DialogTypeEnum.DIALOG_EXCHANGE;

        #region IDialog Members

        public void Open()
        {
            Character.SetDialoger(m_paddockExchanger);

            List<Mount> _stableMounts = MountManager.Instance.GetMountsStable(Character);
            List<Mount> _paddockMounts = !this.Paddock.IsPublicPaddock() && this.Paddock.CanTakeOthersMounts(Character) ?
                MountManager.Instance.GetPaddockMounts(Paddock) :
                MountManager.Instance.GetPaddockMounts(Character, Paddock);

            InventoryHandler.SendExchangeStartOkMountMessage(Character.Client, _stableMounts, _paddockMounts);
        }

        public void Close()
        {
            InventoryHandler.SendExchangeLeaveMessage(Character.Client, DialogType, false);
            Character.CloseDialog(this);
        }

        #endregion
    }
}
