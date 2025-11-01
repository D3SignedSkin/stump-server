using System.Linq;
using Stump.DofusProtocol.D2oClasses;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Database.Items;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Effects.Handlers.Items;
using Stump.Server.WorldServer.Game.Effects.Instances;

namespace Stump.Server.WorldServer.Game.Items.Player.Custom
{
    [ItemType(ItemTypeEnum.PERSONNAGE_SUIVEUR_32)]
    public class FollowerItem : BasePlayerItem
    {
        private bool m_removed;

        public FollowerItem(Character owner, PlayerItemRecord record) : base(owner, record)
        {
            BasePlayerItem item = Owner.Inventory.GetItems(entry => entry.Template.Type.ItemType == ItemTypeEnum.PERSONNAGE_SUIVEUR_32).FirstOrDefault(x => x.Template.Id == record.ItemId);

            if (item != null)
            {
                var itemToRemove = Owner.Inventory.GetItems(CharacterInventoryPositionEnum.INVENTORY_POSITION_FOLLOWER).FirstOrDefault();

                if (itemToRemove != null)
                    Owner.Inventory.RemoveItem(itemToRemove);

                int type = (item.Effects.FirstOrDefault(x => x.EffectId == EffectsEnum.Effect_148) as EffectInteger).Value;
                int follower = (item.Template.Effects.FirstOrDefault(x => x.EffectId == EffectsEnum.Effect_148) as EffectDice).Value;

                Owner.Inventory.RefreshItem(item);

                if (this.IsUsable())
                    Owner.Inventory.MoveItem(item, CharacterInventoryPositionEnum.INVENTORY_POSITION_FOLLOWER);

                Owner.RefreshActor();
            }

            Owner.FightEnded += OnFightEnded;
            Owner.ContextChanged += OnContextChanged;
        }

        private void OnContextChanged(Character character, bool infight)
        {
            if (infight && character.Fight.IsPvP)
            {
                Owner.Inventory.ApplyItemEffects(this, force: ItemEffectHandler.HandlerOperation.UNAPPLY);
            }
        }

        public override bool OnEquipItem(bool unequip)
        {
            if (unequip && !m_removed)
                Owner.Inventory.RemoveItem(this);

            return base.OnEquipItem(unequip);
        }

        public override bool OnRemoveItem()
        {
            m_removed = true;
            Owner.FightEnded -= OnFightEnded;

            return base.OnRemoveItem();
        }

        private void OnFightEnded(Character character, CharacterFighter fighter)
        {
            var effect = Effects.FirstOrDefault(entry => entry.EffectId == EffectsEnum.Effect_RemainingFights) as EffectInteger;

            if (effect == null)
                return;

            var effectDice = Effects.FirstOrDefault(entry => entry.EffectId == EffectsEnum.Effect_RemainingFights) as EffectDice;

            if (fighter.Fight.IsPvP)
            {
                Owner.Inventory.ApplyItemEffects(this, force: ItemEffectHandler.HandlerOperation.APPLY);
            }

            Invalidate();

            bool fightIsDead = fighter.Fight != null && fighter.Fight.Losers != null && fighter.Fight.Losers.Fighters.Any(x => x == fighter);

            if (!fighter.Fight.IsPvP && !fighter.Loot.Items.Any(x => x.Value.ItemId == this.Template.Id) && fightIsDead)
            {
                if (--effect.Value > 1)
                {
                    effect.Value--;
                    effectDice.Value--;
                    Owner.Inventory.RefreshItem(this);

                    return;
                }
                else if (--effect.Value <= 1)
                {
                    Owner.Inventory.RemoveItem(this);
                    Owner.RefreshActor();

                    return;
                }
            }
        }
    }
}