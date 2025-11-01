using System;
using System.Collections.Generic;
using System.Linq;
using Stump.Core.Extensions;
using Stump.Core.Mathematics;
using Stump.Core.Reflection;
using Stump.Core.Timers;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Effects;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Exchanges.Trades;
using Stump.Server.WorldServer.Game.Exchanges.Trades.Players;
using Stump.Server.WorldServer.Game.Interactives;
using Stump.Server.WorldServer.Game.Interactives.Skills;
using Stump.Server.WorldServer.Game.Items;
using Stump.Server.WorldServer.Game.Items.Player;
using Stump.Server.WorldServer.Game.Jobs;

namespace Stump.Server.WorldServer.Game.Exchanges.Craft.Runes
{
    public abstract class RuneMagicCraftDialog : BaseCraftDialog
    {
        public const int MAX_STAT_POWER_25 = 25;
        public const int MAX_STAT_POWER_50 = 50;
        public const int MAX_STAT_POWER_75 = 75;
        public const int MAX_STAT_POWER_120 = 120;
        public const int AUTOCRAFT_INTERVAL = 400;

        private TimedTimerEntry m_autoCraftTimer;

        public RuneMagicCraftDialog(InteractiveObject interactive, Skill skill, Job job) : base(interactive, skill, job)
        { }

        public RuneCrafter RuneCrafter => Crafter as RuneCrafter;

        public PlayerTradeItem ItemToImprove
        {
            get;
            private set;
        }

        public IEnumerable<EffectInteger> ItemEffects => ItemToImprove.Effects.OfType<EffectInteger>();

        public PlayerTradeItem Rune
        {
            get;
            private set;
        }

        public PlayerTradeItem SpecialRune
        {
            get;
            private set;
        }

        public PlayerTradeItem Orbe
        {
            get; private set;
        }

        public PlayerTradeItem Potion
        {
            get;
            private set;
        }

        public PlayerTradeItem SignatureRune
        {
            get;
            private set;
        }

        public virtual void Open()
        {
            FirstTrader.ItemMoved += OnItemMoved;
            SecondTrader.ItemMoved += OnItemMoved;
        }

        public override void Close()
        {
            StopAutoCraft();
        }

        public void StopAutoCraft(ExchangeReplayStopReasonEnum reason = ExchangeReplayStopReasonEnum.STOPPED_REASON_USER)
        {
            if (m_autoCraftTimer != null)
            {
                m_autoCraftTimer.Stop();
                m_autoCraftTimer = null;

                OnAutoCraftStopped(reason);
                ChangeAmount(1);
            }
        }

        protected virtual void OnAutoCraftStopped(ExchangeReplayStopReasonEnum reason)
        {

        }

        protected virtual void OnItemMoved(Trader trader, TradeItem item, bool modified, int difference)
        {
            var playerItem = item as PlayerTradeItem;

            if (playerItem == null)
                return;

            #region Bloqueio de Runas
            //if (runes_safe.Contains(playerItem.PlayerItem.Template.Id))
            //{ 
            //}
            //else if (playerItem.PlayerItem.Template.Type.ItemType == ItemTypeEnum.RUNE_DE_FORGEMAGIE)
            //{

            //    switch (playerItem.Owner.Account.Lang)
            //    {
            //        case "fr":
            //            playerItem.Owner.DisplayNotification("Runa pas permis, seules les runes sont autorisées: {item,1557}, {item,1558}, {item,7438}, {item,7442}, {item,1554}, {item,1556}, {item,1552}, {item,10619}, {item,1553}, {item,1551}, {item,10616}, {item,1555}, {item,11646}, {item,11648}, {item,11640}, {item,11638}, {item,11642}, {item,11644}, {item,11662}, {item,11658}, {item,11666}, {item,11660}, {item,11650}, {item,11654}, {item,11664}, {item,7433}, {item,7435}, {item,10057}.");
            //            break;
            //        case "es":
            //            playerItem.Owner.DisplayNotification("¡Runa no permitida, solo se permiten runas: {item,1557}, {item,1558}, {item,7438}, {item,7442}, {item,1554}, {item,1556}, {item,1552}, {item,10619}, {item,1553}, {item,1551}, {item,10616}, {item,1555}, {item,11646}, {item,11648}, {item,11640}, {item,11638}, {item,11642}, {item,11644}, {item,11662}, {item,11658}, {item,11666}, {item,11660}, {item,11650}, {item,11654}, {item,11664}, {item,7433}, {item,7435}, {item,10057}.");
            //            break;
            //        case "en":
            //            playerItem.Owner.DisplayNotification("Rune not allowed, only runes are allowed: {item,1557}, {item,1558}, {item,7438}, {item,7442}, {item,1554}, {item,1556}, {item,1552}, {item,10619}, {item,1553}, {item,1551}, {item,10616}, {item,1555}, {item,11646}, {item,11648}, {item,11640}, {item,11638}, {item,11642}, {item,11644}, {item,11662}, {item,11658}, {item,11666}, {item,11660}, {item,11650}, {item,11654}, {item,11664}, {item,7433}, {item,7435}, {item,10057}.");
            //            break;
            //        default:
            //            playerItem.Owner.DisplayNotification("Runa não permitida, permitida somente as runas: {item,1557}, {item,1558}, {item,7438}, {item,7442}, {item,1554}, {item,1556}, {item,1552}, {item,10619}, {item,1553}, {item,1551}, {item,10616}, {item,1555}, {item,11646}, {item,11648}, {item,11640}, {item,11638}, {item,11642}, {item,11644}, {item,11662}, {item,11658}, {item,11666}, {item,11660}, {item,11650}, {item,11654}, {item,11664}, {item,7433}, {item,7435}, {item,10057}.");
            //            break;
            //    }
            //    return;
            //}
            #endregion

            //Bloqueio de Venda de Itens comprados em NPCs Shops
            if (playerItem.Effects.Exists(x => x.EffectId == EffectsEnum.Effect_BlockItemNpcShop) && playerItem.Owner.Account.UserGroupId <= 3)
                return;

            if (playerItem.PlayerItem.Template.Type.ItemType == ItemTypeEnum.RUNE_DE_FORGEMAGIE_78)
            {
                foreach (var effect in playerItem.Effects.OfType<EffectInteger>())
                {
                    if (effect.EffectId == EffectsEnum.Effect_795)
                    {
                        if (ItemToImprove != null && !ItemToImprove.Template.IsWeapon())
                        {
                            #region MSG - Runa não permitida, permitida somente em arma!
                            switch (playerItem.Owner.Account.Lang)
                            {
                                case "fr":
                                    playerItem.Owner.DisplayNotification("Runa pas permis, autorisé uniquement dans les armes !");
                                    break;
                                case "es":
                                    playerItem.Owner.DisplayNotification("¡Runa no permitida, permitido solo en armas!");
                                    break;
                                case "en":
                                    playerItem.Owner.DisplayNotification("Rune not allowed, allowed only in weapons!");
                                    break;
                                default:
                                    playerItem.Owner.DisplayNotification("Runa não permitida, permitida somente em arma!");
                                    break;
                            }
                            #endregion

                            return;
                        }
                    }
                }
            }

            if (item.Template.Type.ItemType == ItemTypeEnum.RUNE_DE_FORGEMAGIE_78 && (playerItem != Rune || playerItem.Stack == 0))
            {
                Rune = playerItem.Stack > 0 ? playerItem : null;
            }
            else if ((item.Template.Type.ItemType == ItemTypeEnum.RUNE_DE_TRANSCENDANCE_211) && (playerItem != SpecialRune || playerItem.Stack == 0))
            {
                SpecialRune = playerItem.Stack > 0 ? playerItem : null;
            }
            else if (item.Template.Id == (int)ItemIdEnum.RUNE_DE_SIGNATURE_7508 && (playerItem != SignatureRune || playerItem.Stack == 0))
            {
                SignatureRune = playerItem.Stack > 0 ? playerItem : null;
            }
            else if (item.Template.Type.ItemType == ItemTypeEnum.POTION_DE_FORGEMAGIE_26 && (playerItem != Potion || playerItem.Stack == 0))
            {
                Potion = playerItem.Stack > 0 ? playerItem : null;
            }
            else if (item.Template.Type.ItemType == ItemTypeEnum.ORBE_DE_FORGEMAGIE_189 && (playerItem != Orbe || playerItem.Stack == 0))
            {
                Orbe = playerItem.Stack > 0 ? playerItem : null;
            }
            else if (IsItemEditable(item) && (playerItem != ItemToImprove || playerItem.Stack == 0))
            {
                ItemToImprove = playerItem.Stack > 0 ? playerItem : null;
            }

            if (Crafter.Character.Vip && Rune != null && ItemToImprove != null && Receiver == null)
            {
                //var rune = Rune;

                //if (rune.Template.Type.ItemType != ItemTypeEnum.POTION_DE_FORGEMAGIE)
                //{
                //    foreach (var effect in rune.Effects.OfType<EffectInteger>())
                //    {
                //        var existantEffect = GetEffectToImprove(effect);
                //        var existantEffectMax = GetEffectToImproveMax(effect);
                //        double criticalSuccess, neutralSuccess, criticalFailure;
                //        int nbExo = GetNumberExos();
                //        GetChances(nbExo, existantEffectMax, existantEffect, effect, out criticalSuccess, out neutralSuccess, out criticalFailure);

                //        var total = criticalSuccess + neutralSuccess + criticalFailure;
                //        var criticalsuscporc = (criticalSuccess / total) * 100.0;
                //        var neutralporc = (neutralSuccess / total) * 100.0;
                //        var criticalfailporc = (criticalFailure / total) * 100.0;

                //        #region MSG - Chance de aplicar sem cair:..
                //        switch (Crafter.Character.Account.Lang)
                //        {
                //            case "fr":
                //                Crafter.Character.DisplayNotification("Chance d'appliquer sans tomber: " + criticalsuscporc + "%\n Chance d'appliquer et de tomber: " + neutralporc + "%\n Chance de tomber sans appliquer: " + (100 - (criticalsuscporc + neutralporc)) + "%");
                //                break;
                //            case "es":
                //                Crafter.Character.DisplayNotification("Oportunidad de aplicar sin caer: " + criticalsuscporc + "%\n Oportunidad de aplicar y caer: " + neutralporc + "%\n Oportunidad de caída sin aplicar: " + (100 - (criticalsuscporc + neutralporc)) + "%");
                //                break;
                //            case "en":
                //                Crafter.Character.DisplayNotification("Chance to apply without falling: " + criticalsuscporc + "%\n Chance to apply and fall: " + neutralporc + "%\n Chance to fall without applying: " + (100 - (criticalsuscporc + neutralporc)) + "%");
                //                break;
                //            default:
                //                Crafter.Character.DisplayNotification("Chance de aplicar sem cair: " + criticalsuscporc + "%\n Chance de aplicar e cair: " + neutralporc + "%\n Chance de cair sem aplicar: " + (100 - (criticalsuscporc + neutralporc)) + "%");
                //                break;
                //        }
                //        #endregion
                //    }
                //}
            }
        }

        public bool IsItemEditable(IItem item)
        {
            return Skill.SkillTemplate.ModifiableItemTypes.Contains((int)item.Template.TypeId);
        }

        public override bool CanMoveItem(BasePlayerItem item)
        {
            return item.Template.TypeId == (int)ItemTypeEnum.RUNE_DE_FORGEMAGIE_78 || Skill.SkillTemplate.ModifiableItemTypes.Contains((int)item.Template.TypeId);
        }

        protected virtual void OnRuneApplied(CraftResultEnum result, MagicPoolStatus poolStatus)
        {
        }

        public void ApplyAllRunes()
        {
            if (m_autoCraftTimer != null)
                StopAutoCraft();

            if (Amount == 1 || Amount == 0)
                ApplyRune();
            else
                AutoCraft();
        }

        private void AutoCraft()
        {
            ApplyRune();
            if (ItemToImprove != null && Rune != null && Amount == -1)
                m_autoCraftTimer = Crafter.Character.Area.CallDelayed(AUTOCRAFT_INTERVAL, AutoCraft);
            else
                StopAutoCraft(ExchangeReplayStopReasonEnum.STOPPED_REASON_OK);
        }

        public void ApplyRune()
        {
            var rune = Rune;
            var potion = Potion;
            var specialrune = SpecialRune;
            var orbe = Orbe;
            var signature = SignatureRune;

            #region Bloqueios
            if (ItemToImprove == null || (Rune == null && Potion == null && SpecialRune == null && Orbe == null))
                return;

            if (ItemToImprove.Effects.Exists(x => x.EffectId == EffectsEnum.Effect_LivingObjectId))
            {
                Crafter.Character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 552, ItemToImprove.Template.Id, ItemToImprove.Guid);
                return;
            }

            if (ItemToImprove.Effects.Exists(x => x.EffectId == EffectsEnum.Effect_Appearance || x.EffectId == EffectsEnum.Effect_Apparence_Wrapper))
            {
                #region MSG - Você não pode forjar um item Mimibiótico ou ter um Item de Aparência.
                Crafter.Character.SendServerMessageLang
                    (
                    "Você não pode forjar um item Mimibiótico ou ter um Item de Aparência. Liberte-o do Mimibiótico ou de seu Item Aparência e tente novamente.",
                    "You cannot forge a Mimbiotic item or have an Appearance Item. Release it from the Mimibiotic or its Appearance Item and try again.",
                    "No puede forja un elemento mimbiótico ni tener un elemento de apariencia. Libérelo del mimibiótico o su elemento de apariencia y vuelva a intentarlo.",
                    "Vous ne pouvez pas forger un objet Mimbiotic ou avoir un objet d'apparence. Relâchez-le du Mimibiotic ou de son élément d'apparence et réessayez."
                    );
                #endregion

                return;
            }

            if (ItemToImprove.Effects.FirstOrDefault(x => x.EffectId == EffectsEnum.Effect_CantFM) != null)
            {
                #region MSG - Você não pode forjar um item Mimibiótico ou ter um Item de Aparência.
                Crafter.Character.SendServerMessageLang
                    (
                    "O item que você está tentando modificar possui um encantamento que impede a forjamagia.",
                    "The item you are trying to modify has an enchantment that prevents smithmagic.",
                    "El objeto que intentas modificar tiene un encantamiento que evita la forjamagia.",
                    "L'objet que vous essayez de modifier possède un enchantement qui empêche la forgemagie."
                    );
                #endregion
            }

            if (SpecialRune != null && ItemToImprove.Effects.Where(x => x.EffectId != EffectsEnum.Effect_PowerSink).Any(x => IsExotic(x))) //|| //IsOverMax(x as EffectInteger)))
            {
                return;
            }
            #endregion

            #region Assinatura de Modificação do Item by:Kenshin
            if (signature != null && signature.Stack != 0)
            {
                signature.Owner.Inventory.RemoveItem(signature.PlayerItem, 1);
                if (signature.Owner.Id == Crafter.Id)
                    Crafter.MoveItem((int)(uint)signature.Guid, -1);
                else
                {
                    if (signature.Stack <= 1)
                    {
                        signature.Stack = 0;
                        signature.Owner.Inventory.RemoveItem(signature.PlayerItem);
                    }
                    else
                    {
                        signature.Stack -= 1;
                    }
                    (this as MultiRuneMagicCraftDialog)?.OnItemMoved(Crafter, signature, true, 1);
                }

                if (ItemToImprove.Effects.Exists(x => x.EffectId == EffectsEnum.Effect_985))
                {
                    ItemToImprove.Effects.RemoveAll(x => x.EffectId == EffectsEnum.Effect_985);
                    ItemToImprove.Effects.Add(new EffectString(EffectsEnum.Effect_985, Crafter.Character.Namedefault));
                }
                else
                {
                    ItemToImprove.Effects.Add(new EffectString(EffectsEnum.Effect_985, Crafter.Character.Namedefault));
                }
            }
            #endregion

            #region Multi-Character
            if (Receiver == null)
            {
                if (rune != null)
                {
                    rune.Owner.Inventory.RemoveItem(rune.PlayerItem, 1);
                    Crafter.MoveItem(rune.Guid, -1);
                }
                else if (potion != null)
                {
                    potion.Owner.Inventory.RemoveItem(potion.PlayerItem, 1);
                    Crafter.MoveItem(potion.Guid, -1);
                }
                else if (specialrune != null)
                {
                    specialrune.Owner.Inventory.RemoveItem(specialrune.PlayerItem, 1);
                    Crafter.MoveItem(specialrune.Guid, -1);
                }
                else if (orbe != null)
                {
                    orbe.Owner.Inventory.RemoveItem(orbe.PlayerItem, 1);
                    Crafter.MoveItem(orbe.Guid, -1);
                }
                else
                {
                    Console.WriteLine("Error RuneMagicCraftDialog Multi-Character = Null");
                }
            }
            else
            {
                if (rune != null)
                {
                    rune.Owner.Inventory.RemoveItem(rune.PlayerItem, 1);
                    Crafter.MoveItemToInventory_Receiver(rune.Guid, (int)1);
                }
                else if (potion != null)
                {
                    potion.Owner.Inventory.RemoveItem(potion.PlayerItem, 1);
                    Crafter.MoveItemToInventory_Receiver(potion.Guid, (int)1);
                }
                else if (specialrune != null)
                {
                    specialrune.Owner.Inventory.RemoveItem(specialrune.PlayerItem, 1);
                    Crafter.MoveItemToInventory_Receiver(specialrune.Guid, (int)1);
                }
                else if (orbe != null)
                {
                    orbe.Owner.Inventory.RemoveItem(orbe.PlayerItem, 1);
                    Crafter.MoveItemToInventory_Receiver(orbe.Guid, (int)1);
                }
                else
                {
                    Console.WriteLine("Error RuneMagicCraftDialog Multi-Character = Null");
                }
            }
            #endregion

            bool rune_hunter = false;

            if (orbe != null)
            {
                #region Região da função de Orbes
                if (orbe.Template.Level < ItemToImprove.Template.Level)
                {
                    #region MSG - Você não pode aplicar um orbe de nível inferior para este item.
                    Crafter.Character.SendServerMessageLang
                        (
                        "Você não pode aplicar um orbe de nível inferior para este item.",
                        "You cannot apply a low level orb to this item.",
                        "No puedes aplicar un orbe de nivel inferior a este item.",
                        "Vous ne pouvez pas appliquer un orbe de bas niveau à cet item."
                        );
                    #endregion

                    OnRuneApplied(CraftResultEnum.CRAFT_SUCCESS, MagicPoolStatus.UNMODIFIED);
                }
                else
                {
                    var effects = Singleton<ItemManager>.Instance.GenerateItemEffects(ItemToImprove.Template);
                    ItemToImprove.PlayerItem.Effects = effects;
                    OnRuneApplied(CraftResultEnum.CRAFT_SUCCESS, MagicPoolStatus.UNMODIFIED);
                }
                #endregion
            }
            else if (potion != null)
            {
                #region Região da função de Poções
                if (ItemToImprove.Effects.Exists(x => x.EffectId == EffectsEnum.Effect_DamageNeutral))
                {
                    var runetype = (EffectsEnum)0;

                    if (rune.Template.Id == (int)ItemIdEnum.POTION_DE_SECOUSSE_1338 || rune.Template.Id == (int)ItemIdEnum.POTION_DEBOULEMENT_1340 || rune.Template.Id == (int)ItemIdEnum.POTION_DE_SEISME_1348)
                        runetype = EffectsEnum.Effect_DamageEarth;
                    else if (rune.Template.Id == (int)ItemIdEnum.POTION_DE_RAFALE_1342 || rune.Template.Id == (int)ItemIdEnum.POTION_DE_COURANT_DAIR_1337 || rune.Template.Id == (int)ItemIdEnum.POTION_DOURAGAN_1347)
                        runetype = EffectsEnum.Effect_DamageAir;
                    else if (rune.Template.Id == (int)ItemIdEnum.POTION_DAVERSE_1341 || rune.Template.Id == (int)ItemIdEnum.POTION_DE_TSUNAMI_1346 || rune.Template.Id == (int)ItemIdEnum.POTION_DE_CRACHIN_1335)
                        runetype = EffectsEnum.Effect_DamageWater;
                    else if (rune.Template.Id == (int)ItemIdEnum.POTION_DINCENDIE_1345 || rune.Template.Id == (int)ItemIdEnum.POTION_DE_FLAMBEE_1343 || rune.Template.Id == (int)ItemIdEnum.POTION_DETINCELLE_1333)
                        runetype = EffectsEnum.Effect_DamageFire;

                    var effect = ItemToImprove.Effects.FindAll(x => x.EffectId == EffectsEnum.Effect_DamageNeutral);
                    int rune_value = 100;

                    switch (rune.Template.Id)
                    {
                        case (int)ItemIdEnum.POTION_DE_SECOUSSE_1338:
                        case (int)ItemIdEnum.POTION_DETINCELLE_1333:
                        case (int)ItemIdEnum.POTION_DE_COURANT_DAIR_1337:
                        case (int)ItemIdEnum.POTION_DE_CRACHIN_1335:
                            rune_value = 50;
                            break;

                        case (int)ItemIdEnum.POTION_DAVERSE_1341:
                        case (int)ItemIdEnum.POTION_DE_FLAMBEE_1343:
                        case (int)ItemIdEnum.POTION_DE_RAFALE_1342:
                        case (int)ItemIdEnum.POTION_DEBOULEMENT_1340:
                            rune_value = 32;
                            break;

                        case (int)ItemIdEnum.POTION_DE_SEISME_1348:
                        case (int)ItemIdEnum.POTION_DOURAGAN_1347:
                        case (int)ItemIdEnum.POTION_DE_TSUNAMI_1346:
                        case (int)ItemIdEnum.POTION_DINCENDIE_1345:
                            rune_value = 15;
                            break;
                    }

                    List<EffectDice> test = new List<EffectDice>();

                    foreach (var temp in effect)
                    {
                        var effect_ = (EffectDice)temp;
                        test.Add(new EffectDice((short)runetype, 0, (short)(effect_.DiceNum - (effect_.DiceNum * (rune_value / 100d))), (short)(effect_.DiceFace - (effect_.DiceFace * (rune_value / 100d))), new EffectBase()));
                    }

                    var criticalsuscporc = 20;
                    var neutralporc = 50;
                    var rand = new CryptoRandom();
                    var randNumber = (int)(rand.NextDouble() * 100);

                    if (SignatureRune != null)
                    {
                        var signatureRune = SignatureRune;
                        var nameEffect = ItemToImprove.Effects.FirstOrDefault(x => x.EffectId == EffectsEnum.Effect_985);

                        if (nameEffect != null)
                            ItemToImprove.Effects.Remove(nameEffect);

                        ItemToImprove.Effects.Add(new EffectString(EffectsEnum.Effect_985, Job.Owner.Namedefault));
                    }
                    if (randNumber <= criticalsuscporc && randNumber > 0)
                    {
                        foreach (var temp in effect)
                        {
                            ItemToImprove.Effects.Remove(temp);
                        }
                        foreach (var temp in test)
                        {
                            ItemToImprove.Effects.Add(temp);
                        }

                        OnRuneApplied(CraftResultEnum.CRAFT_SUCCESS, MagicPoolStatus.UNMODIFIED);
                    }
                    else if (randNumber <= criticalsuscporc + neutralporc && randNumber > 0)
                    {
                        foreach (var temp in effect)
                        {
                            ItemToImprove.Effects.Remove(temp);
                        }

                        foreach (var temp in test)
                        {
                            ItemToImprove.Effects.Add(temp);
                        }

                        var resid = (test.FirstOrDefault() as EffectInteger);
                        resid.Value = (short)test.Sum(x => x.Max);
                        resid.Template.Characteristic = rune_value; // because 15 = value 1 and rest.max(resid.value) * 1  (15 = value 1)
                        int residual = DeBoostEffect(resid);

                        OnRuneApplied(CraftResultEnum.CRAFT_SUCCESS, GetMagicPoolStatus(residual));
                    }
                    else
                    {
                        var resid = (test.FirstOrDefault() as EffectInteger);
                        resid.Value = (short)test.Sum(x => x.Max);
                        resid.Template.Characteristic = rune_value;
                        int residual = DeBoostEffect(resid);

                        OnRuneApplied(CraftResultEnum.CRAFT_FAILED, GetMagicPoolStatus(residual));
                    }
                }
                else
                {
                    rune.Owner.Inventory.AddItem(rune.PlayerItem.Template, 1);
                    return;
                }
                #endregion
            }
            else if (specialrune != null)
            {
                #region Região da função das Runas Especiais
                foreach (var effect in specialrune.Effects.OfType<EffectInteger>().Where(x => x.EffectId != EffectsEnum.Effect_FMPercentOfChance))
                {
                    var percent = (specialrune.Effects.FirstOrDefault(x => x.EffectId == EffectsEnum.Effect_FMPercentOfChance) as EffectInteger);

                    if (percent == null)
                        return;

                    var existantEffect = GetEffectToImprove(effect);
                    var rand = new CryptoRandom();
                    var randNumber = (int)(rand.NextDouble() * 100);

                    if (randNumber <= percent.Value)
                    {
                        if (effect.Template.Operator != "-")
                            BoostEffect(effect);
                        else
                            BoostEffect(effect, true);

                        OnRuneApplied(CraftResultEnum.CRAFT_SUCCESS, MagicPoolStatus.UNMODIFIED);
                    }
                }
                #endregion
            }
            else if (rune != null)
            {
                #region Região da função das Runas Comuns
                foreach (var effect in rune.Effects.OfType<EffectInteger>())
                {
                    var existantEffect = GetEffectToImprove(effect);
                    var existantEffectMax = GetEffectToImproveMax(effect);
                    double criticalSuccess, neutralSuccess, criticalFailure;
                    int nbExo = GetNumberExos();
                    GetChances(nbExo, existantEffectMax, existantEffect, effect, out criticalSuccess, out neutralSuccess, out criticalFailure);

                    var rand = new CryptoRandom();
                    var randNumber = (int)(rand.NextDouble() * 100);

                    if (randNumber == 0 && criticalSuccess == 0 && neutralSuccess == 0 && criticalFailure == 100)
                        randNumber = 1;

                    if (SignatureRune != null)
                    {
                        var signatureRune = SignatureRune;
                        signatureRune.Owner.Inventory.RemoveItem(signatureRune.PlayerItem, 1);

                        Crafter.MoveItem(signatureRune.Guid, -1);

                        var nameEffect = ItemToImprove.Effects.FirstOrDefault(x => x.EffectId == EffectsEnum.Effect_985);

                        if (nameEffect != null)
                            ItemToImprove.Effects.Remove(nameEffect);

                        ItemToImprove.Effects.Add(new EffectString(EffectsEnum.Effect_985, Job.Owner.Namedefault));
                    }

                    if (randNumber <= criticalSuccess)
                    {
                        BoostEffect(effect);
                        OnRuneApplied(CraftResultEnum.CRAFT_SUCCESS, MagicPoolStatus.UNMODIFIED);

                        if (effect.EffectId == EffectsEnum.Effect_795)
                            rune_hunter = true;
                    }
                    else if (randNumber <= criticalSuccess + neutralSuccess)
                    {
                        BoostEffect(effect);
                        int residual = DeBoostEffect(effect);
                        OnRuneApplied(CraftResultEnum.CRAFT_SUCCESS, GetMagicPoolStatus(residual));

                        if (effect.EffectId == EffectsEnum.Effect_795)
                            rune_hunter = true;
                    }
                    else
                    {
                        int residual = DeBoostEffect(effect);
                        OnRuneApplied(CraftResultEnum.CRAFT_FAILED, GetMagicPoolStatus(residual));
                    }
                }
                #endregion
            }

            if (rune_hunter == true)
            {
                BasePlayerItem newInstance;

                if (ItemToImprove.Owner == Crafter.Character)
                {
                    newInstance = ItemToImprove.Owner.Inventory.RefreshItemInstance(ItemToImprove.PlayerItem);
                    ItemToImprove.PlayerItem = newInstance;
                }
                else
                {
                    newInstance = Receiver.Character.Inventory.RefreshItemInstance(ItemToImprove.PlayerItem);
                    ItemToImprove.PlayerItem = newInstance;
                }
            }

            ItemToImprove.PlayerItem.Invalidate();

            if (Skill.SkillTemplate.ParentJobId != 1)
            {
                var xp = JobManager.Instance.GetHarvestJobXp((int)Skill.SkillTemplate.LevelMin);
                Job.Owner.Jobs[Skill.SkillTemplate.ParentJobId].Experience += xp;
            }
        }

        private void BoostEffect(EffectInteger runeEffect, bool decrease = false)
        {
            var effect = GetEffectToImprove(runeEffect);

            if (effect != null)
            {
                if (decrease)
                {
                    effect.Value -= (short)((effect.Template.BonusType == -1 ? -1 : 1) * runeEffect.Value);

                    if (effect.Value == 0)
                    {
                        ItemToImprove.Effects.Remove(effect);
                    }
                    else if (effect.Value > 0 && effect.Value <= runeEffect.Value && effect.Template.OppositeId > 0) // from negativ to positiv
                    {
                        ItemToImprove.Effects.Remove(effect);
                        ItemToImprove.Effects.Add(new EffectInteger((EffectsEnum)effect.Template.OppositeId, Math.Abs(effect.Value)));
                    }
                }
                else
                {
                    effect.Value += (short)((effect.Template.BonusType == -1 ? -1 : 1) * runeEffect.Value);

                    if (effect.Value == 0)
                    {
                        ItemToImprove.Effects.Remove(effect);
                    }
                    else if (effect.Value > 0 && effect.Value <= runeEffect.Value && effect.Template.OppositeId > 0) // from negativ to positiv
                    {
                        ItemToImprove.Effects.Remove(effect);
                        ItemToImprove.Effects.Add(new EffectInteger((EffectsEnum)effect.Template.OppositeId, effect.Value));
                    }
                }
            }
            else
            {
                ItemToImprove.Effects.Add(new EffectInteger(runeEffect.EffectId, runeEffect.Value));
            }
        }

        private int DeBoostEffect(EffectInteger runeEffect)
        {
            var pwrToLose = (int)Math.Ceiling(EffectManager.Instance.GetEffectPower(runeEffect));
            short residual = 0;

            if (ItemToImprove.PlayerItem.PowerSink > 0)
            {
                residual = (short)-Math.Min(pwrToLose, ItemToImprove.PlayerItem.PowerSink);
                ItemToImprove.PlayerItem.PowerSink += residual;
                pwrToLose += residual;
            }

            if (pwrToLose == 0)
                return residual;

            while (pwrToLose > 0)
            {
                var effect = GetEffectToDown(runeEffect);

                if (effect == null)
                    break;

                var maxLost = (int)Math.Ceiling(EffectManager.Instance.GetEffectBasePower(runeEffect) / Math.Abs(EffectManager.Instance.GetEffectBasePower(effect)));

                var rand = new CryptoRandom();
                var lost = rand.Next(1, maxLost + 1);

                var oldValue = effect.Value;

                if (effect.Template.BonusType == -1 && effect.Value + lost > (ItemToImprove.Template.Effects.FirstOrDefault(x => x.EffectId == effect.EffectId) as EffectDice).Min * 2)
                    return 0;

                effect.Value -= (short)((effect.Template.BonusType == -1 ? -1 : 1) * lost);
                pwrToLose -= (int)Math.Ceiling(lost * Math.Abs(EffectManager.Instance.GetEffectBasePower(effect)));

                if (effect.Value == 0 || (effect.Value < 0 && oldValue > 0))
                {
                    if (effect.Template.Id == (int)EffectsEnum.Effect_DamageWater || effect.Template.Id == 97 || effect.Template.Id == 98 || effect.Template.Id == 99 ||
                        effect.Template.Id == 100 || effect.Template.Id == 91 || effect.Template.Id == 92 || effect.Template.Id == 93 ||
                        effect.Template.Id == 94 || effect.Template.Id == 95 || effect.Template.Id == 101 || effect.Template.Id == 108)
                    {
                        break;
                    }
                    else
                    {
                        ItemToImprove.Effects.Remove(effect);
                    }
                }
                else if (effect.Value < 0 && effect.Value >= -lost && effect.Template.OppositeId > 0) // from positiv to negativ stat
                {
                    if (effect.Template.Id == 96 || effect.Template.Id == 97 || effect.Template.Id == 98 || effect.Template.Id == 99 ||
                        effect.Template.Id == 100 || effect.Template.Id == 91 || effect.Template.Id == 92 || effect.Template.Id == 93 ||
                        effect.Template.Id == 94 || effect.Template.Id == 95 || effect.Template.Id == 101 || effect.Template.Id == 108)
                    {
                        break;
                    }
                    else
                    {
                        ItemToImprove.Effects.Remove(effect);
                        ItemToImprove.Effects.Add(new EffectInteger((EffectsEnum)effect.Template.OppositeId, (short)-effect.Value));
                    }

                }
            }

            residual = (short)(pwrToLose < 0 ? -pwrToLose : 0);
            ItemToImprove.PlayerItem.PowerSink += residual;

            return residual;
        }


        private EffectInteger GetEffectToDown(EffectInteger runeEffect)
        {
            var effectToImprove = GetEffectToImprove(runeEffect);
            // recherche de jet exotique
            var exoticEffect = ItemEffects.Where(x => IsExotic(x) && x != effectToImprove).RandomElementOrDefault();

            if (exoticEffect != null)

                return exoticEffect;

            // recherche de jet overmax
            var overmaxEffect = ItemEffects.Where(x => IsOverMax(x, runeEffect) && x != effectToImprove)
                .RandomElementOrDefault();

            if (overmaxEffect != null)
                return overmaxEffect;

            var rand = new CryptoRandom();

            if (ItemToImprove.Template.Level >= 1 && ItemToImprove.Template.Level <= 50)
            {
                foreach (var effect in ItemEffects.ShuffleLinq().Where(x => x != effectToImprove))
                {
                    if (EffectManager.Instance.GetEffectPower(effect) - EffectManager.Instance.GetEffectPower(runeEffect) < MAX_STAT_POWER_25)
                        continue;

                    if (rand.NextDouble() <= EffectManager.Instance.GetEffectPower(runeEffect) / Math.Abs(EffectManager.Instance.GetEffectBasePower(effect)))
                        return effect;
                }
            }
            else if (ItemToImprove.Template.Level >= 51 && ItemToImprove.Template.Level <= 100)
            {
                foreach (var effect in ItemEffects.ShuffleLinq().Where(x => x != effectToImprove))
                {
                    if (EffectManager.Instance.GetEffectPower(effect) - EffectManager.Instance.GetEffectPower(runeEffect) < MAX_STAT_POWER_50)
                        continue;

                    if (rand.NextDouble() <= EffectManager.Instance.GetEffectPower(runeEffect) / Math.Abs(EffectManager.Instance.GetEffectBasePower(effect)))
                        return effect;
                }
            }
            else if (ItemToImprove.Template.Level >= 101 && ItemToImprove.Template.Level <= 150)
            {
                foreach (var effect in ItemEffects.ShuffleLinq().Where(x => x != effectToImprove))
                {
                    if (EffectManager.Instance.GetEffectPower(effect) - EffectManager.Instance.GetEffectPower(runeEffect) < MAX_STAT_POWER_75)
                        continue;

                    if (rand.NextDouble() <= EffectManager.Instance.GetEffectPower(runeEffect) / Math.Abs(EffectManager.Instance.GetEffectBasePower(effect)))
                        return effect;
                }
            }
            else if (ItemToImprove.Template.Level >= 151)
            {
                foreach (var effect in ItemEffects.ShuffleLinq().Where(x => x != effectToImprove))
                {
                    if (EffectManager.Instance.GetEffectPower(effect) - EffectManager.Instance.GetEffectPower(runeEffect) < MAX_STAT_POWER_120)
                        continue;

                    if (rand.NextDouble() <= EffectManager.Instance.GetEffectPower(runeEffect) / Math.Abs(EffectManager.Instance.GetEffectBasePower(effect)))
                        return effect;
                }
            }

            return ItemEffects.FirstOrDefault(x => x != effectToImprove);
        }

        private void GetChances(int nbExo, EffectInteger effectMaxToImprove, EffectInteger effectToImprove, EffectInteger runeEffect, out double criticalSuccess, out double neutralSuccess, out double criticalFailure)
        {
            var minPwr = EffectManager.Instance.GetItemMinPower(ItemToImprove);
            var maxPwr = EffectManager.Instance.GetItemMaxPower(ItemToImprove);
            var pwr = EffectManager.Instance.GetItemPower(ItemToImprove);

            var itemStatus = Math.Max(0, GetProgress(pwr, maxPwr, minPwr) * 100);
            var parentEffect = GetTemplateEffect(runeEffect);
            double geteffectToImproveMax = 0;
            double geteffectToImprove = 0;
            double geteffectrune = 0;

            if (effectToImprove != null)
            {
                if (effectMaxToImprove == null)
                {
                    geteffectToImproveMax = 0;
                }
                else
                {
                    geteffectToImproveMax = EffectManager.Instance.GetEffectPower(effectMaxToImprove);
                }

                geteffectToImprove = EffectManager.Instance.GetEffectPower(effectToImprove);
                geteffectrune = EffectManager.Instance.GetEffectPower(runeEffect);
            }

            if (geteffectToImprove == 0 && geteffectToImproveMax == 0 && nbExo >= 2)
            {
                neutralSuccess = 0;
                criticalSuccess = 0;
                criticalFailure = 100;

                return;
            }

            if (effectToImprove != null && (geteffectToImprove - geteffectToImproveMax + geteffectrune > EffectManager.Instance.GetOverMax(runeEffect) || GetExoticPower() > EffectManager.Instance.GetOverMax(runeEffect)))
            {
                neutralSuccess = 0;
                criticalSuccess = 0;
                criticalFailure = 100;

                return;
            }

            double effectStatus;
            double diceFactor;
            double itemFactor;
            double effectSuccess;
            double itemSuccess;

            if (parentEffect == null) // exo
            {
                effectStatus = 100;
                itemStatus = 89 + Math.Sqrt(EffectManager.Instance.GetEffectPower(runeEffect)) + Math.Sqrt(itemStatus);
                diceFactor = 30;
                itemFactor = 54;
            }
            else
            {
                effectStatus = Math.Max(0, GetProgress(effectToImprove?.Value ?? 0, parentEffect.Max, parentEffect.Min) * 100);

                if (effectToImprove != null && IsOverMax(effectToImprove, runeEffect))
                {
                    itemStatus = Math.Max(itemStatus, effectStatus / 2);
                    effectStatus += EffectManager.Instance.GetEffectPower(runeEffect);
                }

                diceFactor = 20;
                itemFactor = 50;
            }

            effectStatus = Math.Min(100, effectStatus);
            itemStatus = Math.Min(99, itemStatus);

            if (effectStatus >= 80)
                effectSuccess = diceFactor * effectStatus / 100;
            else
                effectSuccess = effectStatus / 4;

            if (itemStatus >= 50)
                itemSuccess = itemFactor * itemStatus / 100;
            else
                itemSuccess = itemStatus;

            neutralSuccess = 27.5d;

            criticalSuccess = Math.Max(1, 100 - Math.Ceiling(effectSuccess + itemSuccess));

            if (criticalSuccess > 60)
                neutralSuccess = 100 - criticalSuccess;
            else if (criticalSuccess < 25)
                neutralSuccess = 20 + criticalSuccess;

            criticalFailure = 100 - (criticalSuccess + neutralSuccess);
        }

        private int GetNumberExos()
        {
            var itemTemplate = ItemToImprove.Template;
            var item = ItemManager.Instance.CreatePlayerItem(ItemToImprove.Owner, itemTemplate, 1, true);
            IEnumerable<EffectInteger> ItemEffectsMax = item.Effects.OfType<EffectInteger>();
            IEnumerable<EffectInteger> ItemEffect = ItemToImprove.Effects.OfType<EffectInteger>();
            int countmax = 0;
            int countItem = 0;

            foreach (EffectInteger efec in ItemEffectsMax)
            {
                if (efec.Value > 0)
                    countmax++;
            }

            foreach (EffectInteger efec in ItemEffect)
            {
                if (efec.Value > 0)
                    countItem++;
            }

            return countItem - countmax;
        }

        private EffectInteger GetEffectToImproveMax(EffectInteger runeEffect)
        {
            var itemTemplate = ItemToImprove.Template;
            var item = ItemManager.Instance.CreatePlayerItem(ItemToImprove.Owner, itemTemplate, 1, true);

            foreach (var efec in item.Effects.OfType<EffectInteger>())
            {
                var existantEffect = GetEffectToImprove(efec);
            }

            IEnumerable<EffectInteger> ItemEffectsMax = item.Effects.OfType<EffectInteger>();

            return ItemEffectsMax.FirstOrDefault(x => x.EffectId == runeEffect.EffectId || (x.Template.OppositeId != 0 && x.Template.OppositeId == runeEffect.Id));
        }

        private EffectInteger GetEffectToImprove(EffectInteger runeEffect)
        {
            return ItemEffects.FirstOrDefault(x => x.EffectId == runeEffect.EffectId || (x.Template.OppositeId != 0 && x.Template.OppositeId == runeEffect.Id));
        }

        private MagicPoolStatus GetMagicPoolStatus(int residual)
        {
            return residual == 0 ? MagicPoolStatus.UNMODIFIED : (residual > 0 ? MagicPoolStatus.INCREASED : MagicPoolStatus.DECREASED);
        }

        private bool IsExotic(EffectBase effect)
        {
            return ItemToImprove.Template.Effects.All(x => x.EffectId != effect.EffectId);
        }

        private double GetExoticPower()
        {
            return ItemToImprove.Effects.Where(IsExotic).OfType<EffectInteger>().Sum(x => EffectManager.Instance.GetEffectPower(x));
        }

        private bool IsOverMax(EffectInteger effect, EffectInteger runeEffect)
        {
            var template = GetTemplateEffect(effect);

            return effect.Template.BonusType > -1 && effect.Value + runeEffect.Value > template?.Max;
        }

        private EffectDice GetTemplateEffect(EffectBase effect)
        {
            return ItemToImprove.Template.Effects.OfType<EffectDice>().FirstOrDefault(x => x.EffectId == effect.EffectId || x.Template.OppositeId > 0 && x.Template.OppositeId == (int)effect.EffectId);
        }

        private double GetProgress(double value, double max, double min)
        {
            if (min < 0 || max < 0)
            {
                var x = max;
                max = -min;
                min = -x;
            }

            if (max == min && max != 0) return value / max;
            if (max == 0) return 1d;
            return (value - min) / (max - min);
        }
    }
}