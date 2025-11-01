//using Accord.Statistics.Distributions.Univariate;
//using Stump.Core.Reflection;
//using Stump.DofusProtocol.Enums;
//using Stump.DofusProtocol.Messages;
//using Stump.Server.BaseServer.Commands;
//using Stump.Server.BaseServer.IPC.Messages;
//using Stump.Server.WorldServer.Commands.Commands.Patterns;
//using Stump.Server.WorldServer.Commands.Trigger;
//using Stump.Server.WorldServer.Core.IPC;
//using Stump.Server.WorldServer.Game;
//using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
//using Stump.Server.WorldServer.Game.Effects;
//using Stump.Server.WorldServer.Game.Effects.Instances;
//using Stump.Server.WorldServer.Game.Items.Player;
//using Stump.Server.WorldServer.Game.Maps.Cells;
//using System.Globalization;
//using System.IO;
//using System.Linq;
//using System.Net;
//using System.Runtime.Serialization;
//using System.Runtime.Serialization.Json;


//namespace Stump.Server.WorldServer.Commands.Commands.Players
//{

//    class Exo_command : InGameCommand
//    {
//        private static int SHOP_POINT = Settings.ExoCommandPrice;
//        public EffectsEnum[] effectAP = { EffectsEnum.Effect_AddAP_111 };
//        public EffectsEnum[] effectMP = { EffectsEnum.Effect_AddMP_128, EffectsEnum.Effect_AddMP };
//        public EffectsEnum[] effectPO = { EffectsEnum.Effect_AddRange, EffectsEnum.Effect_AddRange_136 };
//        public EffectsEnum[] effectInvoc = { EffectsEnum.Effect_AddSummonLimit };
//        public Exo_command()
//        {
//            Aliases = new string[] { "exo" };
//            RequiredRole = RoleEnum.Player;
//            Description = "Exo PA, PM, PO ou Invoc por 800 OG => Modo de Usar : .exo [pa|pm|po|invoc] [capa|chapeu|amuleto|anel_um|anel_dois|botas|cinto|escudo|armas]";
//            Description_en = "Exo PA, PM, PO ou Invoc por 800 OG => Modo de Usar : .exo [pa|pm|po|invoc] [cover|hat|amulet|ring_one|ring_two|boots|belt|shield|weapons]";
//            Description_es = "Exo PA, PM, PO ou Invoc por 800 OG => Modo de Usar : .exo [pa|pm|po|invoc] [portada|sombrero|amuleto|anillo_uno|anillo_dos|botas|cinturon|escudo|armas]";
//            Description_fr = "Exo PA, PM, PO ou Invoc por 800 OG => Modo de Usar : .exo [pa|pm|po|invoc] [couverture|chapeau|amulette|anneau_un|anneau_deux|bottes|courroie|bouclier|armes]";
//            AddParameter<string>("type", "type", "type", null, true);
//            AddParameter<string>("choice", "effet", "effet", null, true);
//        }
//        public override void Execute(GameTrigger trigger)
//        {
//            string str1 = (string)trigger.Get<string>("choice");
//            string str2 = (string)trigger.Get<string>("type");
//            BasePlayerItem jetons = Enumerable.FirstOrDefault<BasePlayerItem>(trigger.Character.Inventory.GetItems((CharacterInventoryPositionEnum)63), (x => x.Template.Id == Inventory.TokenTemplateId));
//            bool flag = false;

//            if (trigger.Character.Inventory.CanTokenBlock())
//            {
//                //Servidor: La interacción con las ogrinas está en mantenimiento. Vuelve a intentarlo más tarde.
//                trigger.Character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 244);
//            }
//            else if (jetons == null)
//            { 
//                switch (trigger.Character.Account.Lang)
//                {
//                    case "fr":
//                        trigger.Character.SendServerMessage("Vous n'avez pas d'Ogrines pour effectuer cette action.Vous êtes à court de stock d'ogrines? <a href=\"http://v2.dofushydra.life/fr/boutique/paiement/choix-pays\" ><u>Obtenez des Ogrines</u></a>", System.Drawing.Color.Red);
//                        break;
//                    case "es":
//                        trigger.Character.SendServerMessage("No tienes ogrinas para realizar esta acción. ¿Te quedaste sin caldo de ogrinas? <a href=\"http://v2.dofushydra.life/es/tienda/pago/choose-country\" ><u>Consigue Ogrinas</u></a>", System.Drawing.Color.Red);
//                        break;
//                    case "en":
//                        trigger.Character.SendServerMessage("You have no Ogrines to perform this action. Did you run out of ogrine stock? <a href=\"http://v2.dofushydra.life/en/shop/payment/choose-country\" ><u>Get Ogrines</u></a>", System.Drawing.Color.Red);
//                        break;
//                    default:
//                        trigger.Character.SendServerMessage("Você não tem Ogrines para realizar essa ação. Acabou o seu estoque de ogrines ? <a href=\"http://v2.dofushydra.life/pt/loja/pagamento/escolher-pais\" ><u>Obter Ogrines</u></a>", System.Drawing.Color.Red);
//                        break;
//                }
//            }
//            else if (jetons.Stack < SHOP_POINT)
//            {
//                switch (trigger.Character.Account.Lang)
//                {
//                    case "fr":
//                        trigger.Character.SendServerMessage($"Vous n'avez pas <b>{SHOP_POINT}</b> Ogrines pour effectuer cette action. Vous êtes à court de stock d'ogrines? <a href=\"http://v2.dofushydra.life/fr/boutique/paiement/choix-pays\" ><u>Obtenez des Ogrines</u></a>", System.Drawing.Color.Red);
//                        break;
//                    case "es":
//                        trigger.Character.SendServerMessage($"No tienes <b>{SHOP_POINT}</b> ogrinas para realizar esta acción. ¿Te quedaste sin caldo de ogrinas? <a href=\"http://v2.dofushydra.life/es/tienda/pago/choose-country\" ><u>Consigue Ogrinas</u></a>", System.Drawing.Color.Red);
//                        break;
//                    case "en":
//                        trigger.Character.SendServerMessage($"You do not have <b>{ SHOP_POINT} </b> Ogrines to perform this action. Did you run out of ogrine stock ? < a href=\"http://v2.dofushydra.life/en/shop/payment/choose-country\" ><u>Get Ogrines</u></a>", System.Drawing.Color.Red);
//                        break;
//                    default:
//                        trigger.Character.SendServerMessage($"Você não tem <b>{SHOP_POINT}</b> Ogrines para realizar essa ação. Acabou o seu estoque de ogrines ? <a href=\"http://v2.dofushydra.life/pt/loja/pagamento/escolher-pais\" ><u>Obter Ogrines</u></a>", System.Drawing.Color.Red);
//                        break;
//                }
//            }
//            else
//            {
//                EffectsEnum effectsEnum = EffectsEnum.Effect_AddAP_111;
//                EffectsEnum effecttype;
//                if (str2.ToLower() == "pa")
//                    effecttype = EffectsEnum.Effect_AddAP_111;
//                else if (str2.ToLower() == "pm")
//                    effecttype = EffectsEnum.Effect_AddMP_128;
//                else if (str2.ToLower() == "po")
//                {
//                    effecttype = EffectsEnum.Effect_AddRange;
//                }
//                else if (str2.ToLower() == "invoc")
//                {
//                    effecttype = EffectsEnum.Effect_AddSummonLimit;
//                }
//                else
//                {
//                    trigger.Character.SendServerMessage("Não é possível encontrar o item em FM: " + (object)effectsEnum);
//                    goto label_20;
//                }
//                switch (str1)
//                {
//                    case "couverture": //FR
//                    case "portada": //ES
//                    case "cover": //EN
//                    case "capa": //PT
//                        flag = this.ApplyMagicalCraft(trigger.Character, effecttype, str2.ToUpper(), CharacterInventoryPositionEnum.ACCESSORY_POSITION_CAPE);
//                        break;
//                    case "chapeau": //FR
//                    case "sombrero": //ES
//                    case "hat": //EN
//                    case "chapeu": //PT
//                        flag = this.ApplyMagicalCraft(trigger.Character, effecttype, str2.ToUpper(), CharacterInventoryPositionEnum.ACCESSORY_POSITION_HAT);
//                        break;
//                    case "amulette": //FR
//                    case "amulet": //EN
//                    case "amuleto": //PT-ES
//                        flag = this.ApplyMagicalCraft(trigger.Character, effecttype, str2.ToUpper(), CharacterInventoryPositionEnum.ACCESSORY_POSITION_AMULET);
//                        break;
//                    case "anneau_un": //FR
//                    case "anillo_uno": //ES
//                    case "ring_one": //EN
//                    case "anel_um": //PT
//                        flag = this.ApplyMagicalCraft(trigger.Character, effecttype, str2.ToUpper(), CharacterInventoryPositionEnum.INVENTORY_POSITION_RING_RIGHT);
//                        break;
//                    case "anneau_deux": //FR
//                    case "anillo_dos": //ES
//                    case "ring_two": //EN
//                    case "anel_dois": //PT
//                        flag = this.ApplyMagicalCraft(trigger.Character, effecttype, str2.ToUpper(), CharacterInventoryPositionEnum.INVENTORY_POSITION_RING_LEFT);
//                        break;
//                    case "bottes": //FR
//                    case "boots": //EN
//                    case "botas": //PT-ES
//                        flag = this.ApplyMagicalCraft(trigger.Character, effecttype, str2.ToUpper(), CharacterInventoryPositionEnum.ACCESSORY_POSITION_BOOTS);
//                        break;
//                    case "courroie": //FR
//                    case "cinturon": //ES
//                    case "belt": //EN
//                    case "cinto": //PT
//                        flag = this.ApplyMagicalCraft(trigger.Character, effecttype, str2.ToUpper(), CharacterInventoryPositionEnum.ACCESSORY_POSITION_BELT);
//                        break;
//                    case "bouclier": //FR
//                    case "shield": //EN
//                    case "escudo": //PT-ES
//                        flag = this.ApplyMagicalCraft(trigger.Character, effecttype, str2.ToUpper(), CharacterInventoryPositionEnum.ACCESSORY_POSITION_SHIELD);
//                        break;
//                    case "armes": //FR
//                    case "weapons": //EN
//                    case "armas": //PT-ES
//                        flag = this.ApplyMagicalCraft(trigger.Character, effecttype, str2.ToUpper(), CharacterInventoryPositionEnum.ACCESSORY_POSITION_WEAPON);
//                        break;
//                    default:
//                        trigger.Character.SendServerMessage("type incorrect");
//                        break;
//                }
//            }
//        label_20:
//            if (!flag)
//                return;

//            switch (trigger.Character.Account.Lang)
//            {
//                case "fr":
//                    trigger.Character.SendServerMessage("L'équipement souhaité a été forgé + <b>1 " + str2.ToUpper() + "</b> Veuillez le rééquiper pour ne pas perdre l'équipement forgé.");
//                    trigger.Character.SendServerMessage($"Les <b>{SHOP_POINT}</b> points de vente vous ont été retirés.");
//                    trigger.Character.SaveLater();
//                    break;
//                case "es":
//                    trigger.Character.SendServerMessage("El equipo deseado ha sido forjado + <b>1 " + str2.ToUpper() + "</b> Por favor, vuelve a equiparlo para no perder el equipo forjado.");
//                    trigger.Character.SendServerMessage($"Se te han quitado los puntos de la tienda <b>{SHOP_POINT}</b>.");
//                    trigger.Character.SaveLater();
//                    break;
//                case "en":
//                    trigger.Character.SendServerMessage("The desired equipment has been forged + <b>1 " + str2.ToUpper() + "</b> Please re-equip it so as not to lose the forged equipment");
//                    trigger.Character.SendServerMessage($"The <b>{SHOP_POINT}</b> store points have been taken from you.");
//                    trigger.Character.SaveLater();
//                    break;
//                default:
//                    trigger.Character.SendServerMessage("O equipamento desejado foi forjado + <b>1 " + str2.ToUpper() + "</b> Favor re-equipá-lo para não perder o equipamento forjado");
//                    trigger.Character.SendServerMessage($"Os <b>{SHOP_POINT}</b> pontos da loja foram retirados de você.");
//                    trigger.Character.SaveLater();
//                    break;
//            }

//        }

//        private bool ApplyMagicalCraft(Character character, EffectsEnum effecttype, string EffectName, CharacterInventoryPositionEnum position)
//        {
//            BasePlayerItem[] items = character.Inventory.GetItems(position);
//            Enumerable.ToArray<BasePlayerItem>(Enumerable.Where<BasePlayerItem>(character.Inventory.GetItems((CharacterInventoryPositionEnum)63), (entry => entry.Template.Id == Inventory.TokenTemplateId)));
//            bool flag = false;
//            EffectInteger effectInteger1 = new EffectInteger(effecttype, (short)1);

//            foreach (BasePlayerItem basePlayerItem in items)
//            {

//                if (basePlayerItem.Position == position && basePlayerItem.IsEquiped())
//                {
//                    bool result = true;
//                    switch (effecttype)
//                    {
//                        case EffectsEnum.Effect_AddAP_111:
//                            foreach (EffectsEnum effect in effectAP)
//                            {
//                                if (basePlayerItem.Effects.Exists(x => x.EffectId == effect))
//                                {
//                                    result = false;
//                                    break;
//                                }
//                            }
//                            break;
//                        case EffectsEnum.Effect_AddMP_128:
//                            foreach (EffectsEnum effect in effectMP)
//                            {
//                                if (basePlayerItem.Effects.Exists(x => x.EffectId == effect))
//                                {
//                                    result = false;
//                                    break;
//                                }
//                            }
//                            break;
//                        case EffectsEnum.Effect_AddRange:
//                            foreach (EffectsEnum effect in effectPO)
//                            {
//                                if (basePlayerItem.Effects.Exists(x => x.EffectId == effect))
//                                {
//                                    result = false;
//                                    break;
//                                }
//                            }
//                            break;
//                        case EffectsEnum.Effect_AddSummonLimit:
//                            foreach (EffectsEnum effect in effectInvoc)
//                            {
//                                if (basePlayerItem.Effects.Exists(x => x.EffectId == effect))
//                                {
//                                    result = false;
//                                    break;
//                                }
//                            }
//                            break;
//                    }
//                    if (!result)
//                    {
//                        character.OpenPopup("O equipamento já tem esse efeito!");

//                        break;
//                    }

//                    if (character.Inventory.RemoveTokenItem(SHOP_POINT, "ExoCommand"))
//                    {
//                        EffectInteger effectInteger2 = new EffectInteger(effecttype, (short)1);
//                        basePlayerItem.Effects.Add((EffectBase)effectInteger2);
//                        character.Client.Send(new ExchangeCraftInformationObjectMessage((sbyte)2, (ushort)basePlayerItem.Template.Id, (ulong)character.Id));
//                        character.Inventory.RefreshItem(basePlayerItem);
//                        Singleton<EffectManager>.Instance.GetItemEffectHandler((EffectBase)effectInteger2, character, null, true).Apply();
//                        character.RefreshStats();
//                        character.SaveLater();
//                        flag = true;
//                    }
//                    else
//                    {
//                        flag = false;
//                    }
//                }
//            }
//            return flag;
//        }
//        public class fmcac : InGameCommand
//        {
//            private const int SHOP_POINTS = 400;

//            public fmcac()
//            {
//                Aliases = new[] { "fmcac", "ap" };
//                RequiredRole = RoleEnum.Player;
//                Description = "Forjar um CAC em um equipamento por 80 OG. => Modo de Usar : .fm agua|terra|fogo|ar";
//                AddParameter<string>("choice", "strenght", "FMterre", null, true, null);
//            }

//            public override void Execute(GameTrigger trigger)
//            {

//                if (trigger.Character.Inventory.CanTokenBlock())
//                {
//                    trigger.Character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 244); //Hydra: A interação com Ogrines está em manutenção, por favor, tentar novamente mais tarde.
//                }
//                else 
//                {
//                    var trigger_ = trigger.Get<string>("choice");
//                    if (trigger_ == "terra")
//                        ApplyMagicalCraft(trigger.Character, EffectsEnum.Effect_DamageEarth, "Terra");
//                    if (trigger_ == "fogo")
//                        ApplyMagicalCraft(trigger.Character, EffectsEnum.Effect_DamageFire, "Fogo");
//                    if (trigger_ == "ar")
//                        ApplyMagicalCraft(trigger.Character, EffectsEnum.Effect_DamageAir, "Ar");
//                    if (trigger_ == "agua")
//                        ApplyMagicalCraft(trigger.Character, EffectsEnum.Effect_DamageWater, "Água");
//                }
//            }

//            private bool ApplyMagicalCraft(Character character, EffectsEnum effecttype, string EffectName)
//            {
//                var items_ = character.Inventory.GetItems(DofusProtocol.Enums.CharacterInventoryPositionEnum.INVENTORY_POSITION_NOT_EQUIPED);

//                if (items_.Count() > 0 && items_.Any(x => x.Template.Id == 12124) && items_.First(x => x.Template.Id == 12124).Stack >= SHOP_POINTS)
//                {
//                    var items = character.Inventory.GetEquipedItems();

//                    foreach (var item in items)
//                    {
//                        if (item.Position == DofusProtocol.Enums.CharacterInventoryPositionEnum.ACCESSORY_POSITION_WEAPON && item.IsEquiped())
//                        {
//                            if (item.Effects.Exists(x => x.EffectId == EffectsEnum.Effect_DamageNeutral))
//                            {
//                                if (character.Inventory.RemoveTokenItem(SHOP_POINTS, "FmCacCommand"))
//                                {
//                                    var effect = item.Effects.Find(x => x.EffectId == EffectsEnum.Effect_DamageNeutral);
//                                    item.Effects.Remove(effect);
//                                    var effect_ = (EffectDice)effect;
//                                    item.Effects.Add(new EffectDice((short)effecttype, 0, effect_.DiceNum, effect_.DiceFace, new EffectBase()));
//                                    character.Client.Send(new ExchangeCraftInformationObjectMessage(1, (ushort)item.Template.Id, (ulong)character.Id));
//                                    character.SendServerMessage("O item foi forjado com o efeito <b> " + EffectName + " <b>");
//                                    character.Inventory.RefreshItem(item);
//                                    character.Inventory.RefreshItemInstance(item);
//                                    character.RefreshActor();
//                                    character.SaveLater();

//                                    return true;
//                                }
//                                else
//                                {
//                                    return false;
//                                }
//                            }
//                            else
//                            {
//                                //character.Client.Send(new ExchangeStartOkMulticraftCrafterMessage(1, 2));
//                                character.SendServerMessage("O item já possui uma Forja", System.Drawing.Color.Red);
//                                return false;
//                            }
//                        }
//                        else
//                        {
//                            character.SendServerMessage("Você não tem nenhuma arma equipada");
//                            return false;
//                        }
//                    }
//                }
//                else
//                {
//                    character.SendServerMessage("Você não tem Ogrines suficientes, para forjar você irá precisar de " + SHOP_POINTS + " Ogrines.");
//                    return false;
//                }

//                return false;
//            }
//        }
//        public class LevelUpCommand : TargetCommand
//        {
//            public LevelUpCommand()
//            {
//                Aliases = new[] { "addlevel" };
//                RequiredRole = RoleEnum.Developer;
//                Description = "Gives some levels to the target";
//                AddParameter("amount", "amount", "Amount of levels to add", (short)1);
//            }

//            public override void Execute(TriggerBase trigger)
//            {
//                foreach (var target in GetTargets(trigger))
//                {
//                    byte delta;

//                    var amount = trigger.Get<short>("amount");
//                    if (amount > 0 && amount <= byte.MaxValue)
//                    {
//                        delta = (byte)(amount);
//                        target.LevelUp(delta);
//                        trigger.Reply("Vous vous êtes ajouté " + trigger.Bold("{0}") + " niveaux !", delta, target.Name);

//                    }
//                    else if (amount < 0 && -amount <= byte.MaxValue)
//                    {
//                        trigger.ReplyError("Impossible de perdre des levels.");

//                    }

//                }
//            }

//        }
//    }
//}
