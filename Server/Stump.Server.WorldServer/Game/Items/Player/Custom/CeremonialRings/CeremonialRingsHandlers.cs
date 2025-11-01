using NLog;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Types;
using Stump.Server.WorldServer.Database.Items.Templates;
using Stump.Server.WorldServer.Game.Actors.Look;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Effects.Instances;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Stump.Server.WorldServer.Game.Items.Player.Custom.CeremonialRings
{
    public class CeremonialRingsHandlers
    {
        protected static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static void SetItemFollow(Character character, int skinId, Boolean refresh = false)
        {
            if (skinId == 0)
                return;

            sbyte indexSwing = (sbyte)(character.Inventory.GetItems(CharacterInventoryPositionEnum.INVENTORY_POSITION_FOLLOWER).Count() + 1);
            character.itemsFollowsLook.Add(new IndexedEntityLook(ActorLook.Parse("{" + skinId + "|||150}").GetEntityLook(), indexSwing));

            if (refresh)
                character.Map.Refresh(character);
        }

        public static void RemoveItemFollow(Character character, int skinId, Boolean refresh = false)
        {
            IndexedEntityLook getIndex = character.itemsFollowsLook.FirstOrDefault(x => x.look.bonesId == skinId);

            if (getIndex != null)
                character.itemsFollowsLook.Remove(getIndex);

            if (refresh)
                character.Map.Refresh(character);
        }

        public static void RefreshCeremonialFollows(Character character)
        {
            List<BasePlayerItem> followItems = character.Inventory.GetEquipedItems()
                .Where(entry => entry.Effects.Any(x => x.EffectId == EffectsEnum.Effect_Apparence_Wrapper))
                .ToList();

            if (followItems.Count() > 0)
            {
                character.itemsFollowsLook.Clear();

                foreach (var item in followItems.Where(item => TryGetFollowerId(item, out int followerId) && followerId > 0))
                {
                    if (TryGetFollowerId(item, out int followerId) && followerId > 0)
                    {
                        SetItemFollow(character, followerId, true);
                    }
                }
            }
        }

        public static bool TryGetFollowerId(BasePlayerItem item, out int followerId)
        {
            followerId = 0;

            if (item.Effects.Any(entry => entry.EffectId == EffectsEnum.Effect_Apparence_Wrapper))
            {
                var apparenceEffect = item.Effects.FirstOrDefault(entry => entry.EffectId == EffectsEnum.Effect_Apparence_Wrapper) as EffectInteger;

                if (apparenceEffect == null)
                {
                    logger.Error("TryGetFollowerId não encontrou o apparenceEffect.");
                    return false;
                }

                int itemTemplateId = (int)apparenceEffect.Value;

                if (itemTemplateId == 0)
                {
                    logger.Error("TryGetFollowerId não encontrou um valor maior que 0 em itemTemplateId.");
                    return false;
                }

                ItemTemplate templateApparence = ItemManager.Instance.TryGetTemplate(itemTemplateId);

                if (templateApparence == null)
                {
                    logger.Error("TryGetFollowerId não encontrou um templateApparence valido.");
                    return false;
                }

                if (templateApparence.Effects.Any(x => x.EffectId == EffectsEnum.Effect_148))
                {
                    var followerEffect = templateApparence.Effects.FirstOrDefault(entry => entry.EffectId == EffectsEnum.Effect_148) as EffectDice;

                    if (followerEffect == null)
                    {
                        logger.Error("TryGetFollowerId não encontrou um followerEffect valido.");
                        return false;
                    }

                    followerId = followerEffect.Value;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            return true;
        }
    }
}