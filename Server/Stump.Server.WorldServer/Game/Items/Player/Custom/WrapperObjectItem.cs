using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Database.Items;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Items.Player.Custom.CeremonialRings;
using Stump.Server.WorldServer.Handlers.Inventory;
using System.Linq;

namespace Stump.Server.WorldServer.Game.Items.Player.Custom
{
    [ItemType(ItemTypeEnum.ARME_D_APPARAT_251)]
    [ItemType(ItemTypeEnum.CAPE_D_APPARAT_247)]
    [ItemType(ItemTypeEnum.CHAPEAU_D_APPARAT_246)]
    [ItemType(ItemTypeEnum.BOUCLIER_D_APPARAT_248)]
    [ItemType(ItemTypeEnum.FAMILIER_D_APPARAT_249)]
    [ItemType(ItemTypeEnum.MONTILIER_D_APPARAT_250)]
    [ItemType(ItemTypeEnum.OBJET_D_APPARAT_DIVERS_252)]
    public class WrapperObjectItem : BasePlayerItem
    {
        public WrapperObjectItem(Character owner, PlayerItemRecord record) : base(owner, record)
        { }

        public override bool CanDrop(BasePlayerItem item)
        {
            return true;
        }

        public override bool CanEquip()
        {
            Owner.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 445); //Vous ne pouvez pas équiper un objet d'apparat directement, essayez plutôt de l'associer à un objet équipé compatible.
            return false;
        }

        public override bool Drop(BasePlayerItem dropOnItem)
        {
            if (Owner.IsInFight())
                return false;

            var compatibleEffects = Effects.Where(x => x.EffectId == EffectsEnum.Effect_Compatible && x is EffectInteger).Cast<EffectInteger>().ToList();
            bool forcedEquipped = false;

            if (dropOnItem.Template.TypeId == 81) //Equipando Capa de Aparencia em Mochilas
            {
                forcedEquipped = compatibleEffects.Any(effect => effect.Value == 17);
            }

            if (compatibleEffects.Count == 0 && !forcedEquipped)
                return false;

            if (!compatibleEffects.Any(effect => effect.Value == dropOnItem.Template.TypeId) && dropOnItem.Template.TypeId != 81)
                return false;

            if (dropOnItem.Effects.Any(x => x.EffectId == EffectsEnum.Effect_LivingObjectId || x.EffectId == EffectsEnum.Effect_Appearance || x.EffectId == EffectsEnum.Effect_Apparence_Wrapper))
            {
                var host = Owner.Inventory.TryGetItem(dropOnItem.Guid);

                if (host == null)
                    return false;

                var apparenceWrapper = host.Effects.FirstOrDefault(x => x.EffectId == EffectsEnum.Effect_Apparence_Wrapper) as EffectInteger;

                if (apparenceWrapper == null)
                    return false;

                var wrapperItemTemplate = ItemManager.Instance.TryGetTemplate(apparenceWrapper.Value);

                host.Effects.RemoveAll(x => x.EffectId == EffectsEnum.Effect_Apparence_Wrapper);

                if (wrapperItemTemplate.Effects.Any(x => x.EffectId == EffectsEnum.Effect_148))
                {
                    int followerValue = (wrapperItemTemplate.Effects.FirstOrDefault(x => x.EffectId == EffectsEnum.Effect_148) as EffectDice).Value;

                    if (followerValue > 0)
                    {
                        CeremonialRingsHandlers.RemoveItemFollow(Owner, followerValue, true);
                    }
                }

                host.Invalidate();
                Owner.Inventory.RefreshItem(host);
                host.OnObjectModified();

                var wrapperItem = ItemManager.Instance.CreatePlayerItem(Owner, wrapperItemTemplate, 1);

                Owner.Inventory.AddItem(wrapperItem);
                Owner.UpdateLook();

                InventoryHandler.SendInventoryWeightMessage(Owner.Client);
            }

            dropOnItem.Effects.Add(new EffectInteger(EffectsEnum.Effect_Apparence_Wrapper, (short)Template.Id));

            if (Template.Effects.Any(x => x.EffectId == EffectsEnum.Effect_148))
            {
                int followerValue = (Template.Effects.FirstOrDefault(x => x.EffectId == EffectsEnum.Effect_148) as EffectDice).Value;

                if (followerValue > 0)
                {
                    CeremonialRingsHandlers.SetItemFollow(Owner, followerValue, true);
                }
            }

            dropOnItem.Invalidate();
            Owner.Inventory.RefreshItem(dropOnItem);
            dropOnItem.OnObjectModified();

            Owner.UpdateLook();

            InventoryHandler.SendWrapperObjectAssociatedMessage(Owner.Client, dropOnItem);

            return true;
        }
    }
}
