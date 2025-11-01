using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Core.Network;
using Stump.Server.WorldServer.Game.Effects.Instances;
using System;
using System.Linq;

namespace Stump.Server.WorldServer.Game.Items.Player.Custom.StatusPotion
{
    public class PotionStatus
    {
        public static void SetPotionStatus(WorldClient client)
        {
            bool isVip = client.Character.UserGroup.Role == RoleEnum.Vip;
            var endDateVip = isVip ? client.Account.SubscriptionEndDate : client.Account.GoldSubscriptionEndDate;
            int endDateHours = (int)endDateVip.Subtract(DateTime.Now).TotalHours;
            int selectItem = isVip ? 30009 : 30010;
            var createItem = ItemManager.Instance.CreatePlayerItem(client.Character, selectItem, 1, true);

            int rateXp = client.Character.UserGroup.Role == RoleEnum.Vip ? (int)Rates.VipXpRate : (int)Rates.GoldXpRate;
            int rateDrop = client.Character.UserGroup.Role == RoleEnum.Vip ? (int)Rates.VipDropsRate : (int)Rates.GoldDropsRate;
            int rateKamas = client.Character.UserGroup.Role == RoleEnum.Vip ? (int)Rates.VipKamasRate : (int)Rates.GoldKamasRate;

            (createItem.Effects.FirstOrDefault(x => x.EffectId == EffectsEnum.Effect_RateXP) as EffectInteger).Value = rateXp;
            (createItem.Effects.FirstOrDefault(x => x.EffectId == EffectsEnum.Effect_RateDrop) as EffectInteger).Value = rateDrop;
            (createItem.Effects.FirstOrDefault(x => x.EffectId == EffectsEnum.Effect_RateKamas) as EffectInteger).Value = rateKamas;
            (createItem.Effects.FirstOrDefault(x => x.EffectId == EffectsEnum.Effect_QtyOrnament) as EffectInteger).Value = isVip ? Settings.VipOrnament.Count() : (Settings.VipOrnament.Count() + Settings.GoldVipOrnament.Count());
            (createItem.Effects.FirstOrDefault(x => x.EffectId == EffectsEnum.Effect_QtyTitles) as EffectInteger).Value = isVip ? Settings.VipTitle.Count() : (Settings.VipTitle.Count() + Settings.GoldTitle.Count());

            //if (endDateHours > 0)
            //{
            //    (createItem.Effects.FirstOrDefault(x => x.EffectId == EffectsEnum.Effect_EndVipHours) as EffectInteger).Value = endDateHours;
            //}

            createItem.Effects.RemoveAll(effect => effect.EffectId == EffectsEnum.Effect_EndVipHours);

            client.Character.Inventory.MoveItem(client.Character.Inventory.AddItem(createItem), CharacterInventoryPositionEnum.INVENTORY_POSITION_BOOST_FOOD, true);
        }
    }
}