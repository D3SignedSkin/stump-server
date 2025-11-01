using Stump.Core.Reflection;
using Stump.Core.Timers;
using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Database;
using Stump.Server.WorldServer.Database.Interactives;
using Stump.Server.WorldServer.Database.Items.Templates;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Items;
using Stump.Server.WorldServer.Game.Items.Player;
using Stump.Server.WorldServer.Handlers.Inventory;
using System;
using System.Linq;

namespace Stump.Server.WorldServer.Game.Interactives.Skills
{
    [Discriminator("AddItem", typeof(Skill), typeof(int), typeof(InteractiveCustomSkillRecord), typeof(InteractiveObject))]
    public class SkillAddItem : CustomSkill
    {
        public static int HarvestTime = 3000;

        public static int RegrowTime = 60000;

        private TimedTimerEntry m_regrowTimer;

        public SkillAddItem(int id, InteractiveCustomSkillRecord skillTemplate, InteractiveObject interactiveObject) : base(id, skillTemplate, interactiveObject)
        { }

        private ItemTemplate m_itemTemplate;

        public int ItemId
        {
            get
            {
                return this.Record.GetParameter<int>(0U, false);
            }
            set
            {
                this.Record.SetParameter<int>(0U, value);
            }
        }

        public int ItemQuantity => Record.GetParameter<int>(1);

        public ItemTemplate Item
        {
            get
            {
                ItemTemplate itemTemplate;

                if ((itemTemplate = this.m_itemTemplate) == null)
                    itemTemplate = this.m_itemTemplate = Singleton<ItemManager>.Instance.TryGetTemplate(this.ItemId);

                return itemTemplate;
            }
            set
            {
                this.m_itemTemplate = value;
                this.ItemId = value.Id;
            }
        }

        public DateTime? HarvestedSince
        {
            get;
            private set;
        }

        public override int GetDuration(Character character, bool forNetwork = false) => HarvestTime;

        public override int StartExecute(Character character)
        {
            InteractiveObject.SetInteractiveState(InteractiveStateEnum.STATE_ANIMATED);

            base.StartExecute(character);
            return GetDuration(character);
        }

        public override void EndExecute(Character character)
        {
            SetHarvested();

            InteractiveObject.SetInteractiveState(InteractiveStateEnum.STATE_ACTIVATED);

            BasePlayerItem playerItem = Singleton<ItemManager>.Instance.CreatePlayerItem(character, Item, ItemQuantity, false);

            if (character.Inventory.IsFull(playerItem.Template, ItemQuantity))
            {
                character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 964); //Seu inventário está cheio. Você não pode adicionar mais nenhum item.
                base.EndExecute(character);
                return;
            }

            character.Inventory.AddItem(playerItem);
            InventoryHandler.SendObtainedItemWithBonusMessage(character.Client, playerItem.Template, ItemQuantity, 0);
            character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 21, ItemQuantity, playerItem.Template.Id);

            base.EndExecute(character);
        }

        public void SetHarvested()
        {
            HarvestedSince = DateTime.Now;
            InteractiveObject.Map.Refresh(InteractiveObject);
            m_regrowTimer = InteractiveObject.Area.CallDelayed(RegrowTime, Regrow);
        }

        public void Regrow()
        {
            if (m_regrowTimer != null)
            {
                m_regrowTimer.Stop();
                m_regrowTimer = null;
            }

            InteractiveObject.Map.Refresh(InteractiveObject);
            InteractiveObject.SetInteractiveState(InteractiveStateEnum.STATE_NORMAL);
        }
    }
}