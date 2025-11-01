using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Interactives;
using Stump.Server.WorldServer.Handlers.Dialogs;
using Stump.Server.WorldServer.Handlers.Interactives;
using Stump.Server.WorldServer.Handlers.Inventory;
using MapPaddock = Stump.Server.WorldServer.Game.Maps.Paddocks.Paddock;

namespace Stump.Server.WorldServer.Game.Dialogs.Paddock
{
    public class PaddockBuySell : IDialog
    {
        public PaddockBuySell(Character character, MapPaddock paddock, bool bsell, ulong price, InteractiveObject InteractiveObjecta)
        {
            Character = character;
            Paddock = paddock;
            Bsell = bsell;
            Price = price;
            m_InteractiveObjecta = InteractiveObjecta;
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

        public bool Bsell
        {
            get;
            private set;
        }

        public ulong Price
        {
            get;
            private set;
        }

        private InteractiveObject m_InteractiveObjecta
        {
            get;
            set;
        }

        public DialogTypeEnum DialogType => DialogTypeEnum.DIALOG_PURCHASABLE;

        #region IDialog Members

        public void Open()
        {
            Character.SetDialog(this);
            InventoryHandler.SendExchangeStartPaddockBuySell(Character.Client, Bsell, (uint)Character.Id, Price);
        }

        public void Close()
        {
            Character.CloseDialog(this);
            DialogHandler.SendLeaveDialogMessage(Character.Client, DialogType);
        }

        public void BuyPaddock(ulong proposedPrice)
        {
            if (this.Paddock == null)
            {
                this.Close();
                return;
            }

            if (this.Paddock.Price != proposedPrice)
            {
                this.Close();
                return;
            }

            if (Character.Kamas < (int)proposedPrice)
            {
                //Você não tem kamas suficientes para realizar esta ação.
                Character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 82);
                this.Close();
                return;
            }

            if (Paddock.Guild?.Boss?.Character != null)
            {
                Paddock.Guild.Boss.Character.Inventory.AddKamas((int)proposedPrice);
            }
            else if (Paddock.Guild != null)
            {
                Items.ItemManager.CreatItemOffline(Paddock.Guild.Boss.Id, 15399, 1, (int)proposedPrice);
            }

            Paddock.OnSale = false;
            Paddock.Locked = false;
            Paddock.Price = 0;

            if (Paddock.Guild != null)
            {
                Paddock.Guild.RemovePaddock(Paddock);
            }

            Paddock.Guild = Character.Guild;
            Character.Guild.AddPaddock(Paddock);

            Character.Inventory.SubKamas((int)proposedPrice);

            this.Close();
            RefreshPaddock();
        }

        public void SellPaddock(ulong Price, bool forSale)
        {
            if (this.Paddock == null)
            {
                this.Close();
                return;
            }

            if (forSale == false)
            {
                Paddock.Locked = true;
                Paddock.OnSale = false;
                Paddock.Price = 0;
            }
            else
            {
                Paddock.Price = Price;
                Paddock.OnSale = true;
                Paddock.Locked = false;
            }

            this.Close();
            RefreshPaddock();
        }
        #endregion

        private void RefreshPaddock()
        {
            if (Paddock.Guild != null)
            {
                m_InteractiveObjecta.Map.ForEach(x => x.Client.Send(Paddock.GetPaddockPropertiesGuildMessage()));
            }
            else
            {
                m_InteractiveObjecta.Map.ForEach(x => x.Client.Send(Paddock.GetPaddockPropertiesMessage()));
            }

            m_InteractiveObjecta.Map.ForEach(entry => InteractiveHandler.SendInteractiveMapUpdateMessage(entry.Client, entry, m_InteractiveObjecta.Map.GetInteractiveObject(m_InteractiveObjecta.Id)));
        }
    }
}
