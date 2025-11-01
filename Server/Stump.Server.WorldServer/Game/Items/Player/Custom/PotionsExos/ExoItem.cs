//using Stump.DofusProtocol.Enums;
//using Stump.Server.WorldServer.Database.Items;
//using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
//using Stump.Server.WorldServer.Game.Effects;
//using Stump.Server.WorldServer.Game.Effects.Handlers.Items;
//using Stump.Server.WorldServer.Game.Effects.Instances;
//using System.Linq;

//namespace Stump.Server.WorldServer.Game.Items.Player.Custom
//{
//    [ItemType(ItemTypeEnum.EXOTISME)]
//    public class ExoItem : BasePlayerItem
//    {
//        public ExoItem(Character owner, PlayerItemRecord record)
//            : base(owner, record)
//        {
//        }

//        public override bool CanDrop(BasePlayerItem item)
//        {
//            return true;
//        }

//        public override bool CanEquip()
//        {
//            return false;
//        }
//        public void lang(int text)
//        {
//            #region Textos
//            switch (text)
//            {
//                case 1:
//                    switch (Owner.Account.Lang)
//                    {
//                        case "fr":
//                            Owner.SendServerMessage("Quelque chose de mal s'est passé: Vous avez déjà un EXO de cet effet.");
//                            break;
//                        case "es":
//                            Owner.SendServerMessage("Algo malo ocurrió: Usted ya tiene un EXO de ese efecto.");
//                            break;
//                        case "en":
//                            Owner.SendServerMessage("Something wrong happened: You already have an EXO of this effect.");
//                            break;
//                        default:
//                            Owner.SendServerMessage("Algo de errado aconteceu: Você já tem um EXO desse efeito.");
//                            break;
//                    }
//                    break;
//                case 2:
//                    switch (Owner.Account.Lang)
//                    {
//                        case "fr":
//                            Owner.SendServerMessage("Quelque chose de mal s'est passé: Cet article a déjà PO");
//                            break;
//                        case "es":
//                            Owner.SendServerMessage("Algo malo ocurrió: Este elemento ya posee PO.");
//                            break;
//                        case "en":
//                            Owner.SendServerMessage("Something wrong happened: This item already has RANGE.");
//                            break;
//                        default:
//                            Owner.SendServerMessage("Algo de errado aconteceu: Esse item ja possuí PO.");
//                            break;
//                    }

//                    break;
//                case 3:
//                    switch (Owner.Account.Lang)
//                    {
//                        case "fr":
//                            Owner.SendServerMessage("Quelque chose de mal s'est passé: Cet article a déjà PM");
//                            break;
//                        case "es":
//                            Owner.SendServerMessage("Algo malo ocurrió: Este elemento ya posee PM.");
//                            break;
//                        case "en":
//                            Owner.SendServerMessage("Something wrong happened: This item already has MP.");
//                            break;
//                        default:
//                            Owner.SendServerMessage("Algo de errado aconteceu: Esse item ja possuí PM.");
//                            break;
//                    }
//                    break;
//                case 4:
//                    switch (Owner.Account.Lang)
//                    {
//                        case "fr":
//                            Owner.SendServerMessage("Quelque chose de mal s'est passé: Cet article a déjà PA");
//                            break;
//                        case "es":
//                            Owner.SendServerMessage("Algo malo ocurrió: Este elemento ya posee PA.");
//                            break;
//                        case "en":
//                            Owner.SendServerMessage("Something wrong happened: This item already has AP.");
//                            break;
//                        default:
//                            Owner.SendServerMessage("Algo de errado aconteceu: Esse item ja possuí PA.");
//                            break;
//                    }
//                    break;
//                case 5:
//                    switch (Owner.Account.Lang)
//                    {
//                        case "fr":
//                            Owner.SendServerMessage("Quelque chose de mal s'est passé: Cet article a déjà INVOC");
//                            break;
//                        case "es":
//                            Owner.SendServerMessage("Algo malo ocurrió: Este elemento ya posee INVOC.");
//                            break;
//                        case "en":
//                            Owner.SendServerMessage("Something wrong happened: This item already has SUMMON.");
//                            break;
//                        default:
//                            Owner.SendServerMessage("Algo de errado aconteceu: Esse item ja possuí INVOC.");
//                            break;
//                    }
//                    break;
//                case 6:
//                    switch (Owner.Account.Lang)
//                    {
//                        case "fr":
//                            Owner.SendServerMessage("Vous ne pouvez pas utiliser ce type de potion dans cet article.");
//                            break;
//                        case "es":
//                            Owner.SendServerMessage("No puedes utilizar este tipo de poción en este item.");
//                            break;
//                        case "en":
//                            Owner.SendServerMessage("You can not use this type of potion in this item.");
//                            break;
//                        default:
//                            Owner.SendServerMessage("Você não pode usar esse tipo de poção neste item.");
//                            break;
//                    }
//                    break;
//                case 7:
//                    switch (Owner.Account.Lang)
//                    {
//                        case "fr":
//                            Owner.SendServerMessage("EXO AVEC SUCCÈS! Déconnectez-vous et reconnectez-vous pour afficher les modifications.");
//                            break;
//                        case "es":
//                            Owner.SendServerMessage("¡EXO CON ÉXITO! Desconecte y vuelva a conectar para ver los cambios.");
//                            break;
//                        case "en":
//                            Owner.SendServerMessage("EXO WITH SUCCESS! Disconnect and reconnect to view changes.");
//                            break;
//                        default:
//                            Owner.SendServerMessage("EXO COM SUCESSO! Desconecte-se e reconecte para visualizar as alterações.");
//                            break;
//                    }
//                    break;
//                default:
//                    switch (Owner.Account.Lang)
//                    {
//                        case "fr":
//                            Owner.SendServerMessage("Erreur inconnue!");
//                            break;
//                        case "es":
//                            Owner.SendServerMessage("¡Error desconocido!");
//                            break;
//                        case "en":
//                            Owner.SendServerMessage("Unknown error!");
//                            break;
//                        default:
//                            Owner.SendServerMessage("Erro Desconhecido!");
//                            break;
//                    }
//                    break;
//            }

//            #endregion
//        }

//        public bool teste(EffectsEnum temp, BasePlayerItem dropOnItem, out bool travar)
//        {
//            travar = false;
//            var atualInt = dropOnItem.Effects.OfType<EffectInteger>().FirstOrDefault(x => x.EffectId == temp || (x.Template.OppositeId > 0 && x.Template.OppositeId == (int)temp));
//            var atualDice = dropOnItem.Template.Effects.OfType<EffectDice>().FirstOrDefault(x => x.EffectId == temp || (x.Template.OppositeId > 0 && x.Template.OppositeId == (int)temp));

//            var range1Dice = dropOnItem.Template.Effects.OfType<EffectDice>().FirstOrDefault(x => x.EffectId == EffectsEnum.Effect_AddRange || (x.Template.OppositeId > 0 && x.Template.OppositeId == (int)EffectsEnum.Effect_AddRange));
//            var range1Int = dropOnItem.Effects.OfType<EffectInteger>().FirstOrDefault(x => x.EffectId == EffectsEnum.Effect_AddRange || (x.Template.OppositeId > 0 && x.Template.OppositeId == (int)EffectsEnum.Effect_AddRange));
//            var range2Dice = dropOnItem.Template.Effects.OfType<EffectDice>().FirstOrDefault(x => x.EffectId == EffectsEnum.Effect_AddRange_136 || (x.Template.OppositeId > 0 && x.Template.OppositeId == (int)EffectsEnum.Effect_AddRange_136));
//            var range2Int = dropOnItem.Effects.OfType<EffectInteger>().FirstOrDefault(x => x.EffectId == EffectsEnum.Effect_AddRange_136 || (x.Template.OppositeId > 0 && x.Template.OppositeId == (int)EffectsEnum.Effect_AddRange_136));

//            var pm1Dice = dropOnItem.Template.Effects.OfType<EffectDice>().FirstOrDefault(x => x.EffectId == EffectsEnum.Effect_AddMP || (x.Template.OppositeId > 0 && x.Template.OppositeId == (int)EffectsEnum.Effect_AddMP));
//            var pm1Int = dropOnItem.Effects.OfType<EffectInteger>().FirstOrDefault(x => x.EffectId == EffectsEnum.Effect_AddMP || (x.Template.OppositeId > 0 && x.Template.OppositeId == (int)EffectsEnum.Effect_AddMP));
//            var pm2Dice = dropOnItem.Template.Effects.OfType<EffectDice>().FirstOrDefault(x => x.EffectId == EffectsEnum.Effect_AddMP_128 || (x.Template.OppositeId > 0 && x.Template.OppositeId == (int)EffectsEnum.Effect_AddMP_128));
//            var pm2Int = dropOnItem.Effects.OfType<EffectInteger>().FirstOrDefault(x => x.EffectId == EffectsEnum.Effect_AddMP_128 || (x.Template.OppositeId > 0 && x.Template.OppositeId == (int)EffectsEnum.Effect_AddMP_128));

//            var pa1Dice = dropOnItem.Template.Effects.OfType<EffectDice>().FirstOrDefault(x => x.EffectId == EffectsEnum.Effect_AddAP_111 || (x.Template.OppositeId > 0 && x.Template.OppositeId == (int)EffectsEnum.Effect_AddAP_111));
//            var pa1Int = dropOnItem.Effects.OfType<EffectInteger>().FirstOrDefault(x => x.EffectId == EffectsEnum.Effect_AddAP_111 || (x.Template.OppositeId > 0 && x.Template.OppositeId == (int)EffectsEnum.Effect_AddAP_111));

//            var invoc1Dice = dropOnItem.Template.Effects.OfType<EffectDice>().FirstOrDefault(x => x.EffectId == EffectsEnum.Effect_AddSummonLimit || (x.Template.OppositeId > 0 && x.Template.OppositeId == (int)EffectsEnum.Effect_AddSummonLimit));
//            var invoc1Int = dropOnItem.Effects.OfType<EffectInteger>().FirstOrDefault(x => x.EffectId == EffectsEnum.Effect_AddSummonLimit || (x.Template.OppositeId > 0 && x.Template.OppositeId == (int)EffectsEnum.Effect_AddSummonLimit));


//            if (Effects.Any(x => x.EffectId == temp))
//            {
//                #region Bloqueios de IF
//                if (atualInt != null)
//                {//se o itemja tiver um effeito deste tipo

//                    if (atualDice == null)
//                    { // se o atual tiver umvalor mas o tempalte for nulo quer dizer q adicionou um exo fora de seu template e esta na segunda tentativa
//                        lang(1);
//                        return false;
//                    }
//                    else if (atualInt.Value < atualDice.Max)// --------------------- verificar o max de um item q recebeu um exo de efeito q n tinha
//                    {
//                        // n acontecer nada '-' simplesmente seguirá em frente
//                        return true;
//                    }
//                    else if (atualInt.Value == atualDice.Max)// se ele estiver no maximo, só poderá ter 1 exo
//                    {
//                        if (range1Int != null)
//                        {
//                            if (range1Dice == null)
//                            {
//                                lang(2);
//                                return false;
//                            }
//                            else if (range1Int.Value > range1Dice.Max)
//                            {
//                                lang(2);
//                                return false;
//                                //vc ja tem um exo maximo
//                            }
//                        }
//                        if (range2Int != null)
//                        {
//                            if (range2Dice == null)
//                            {
//                                lang(2);
//                                return false;
//                            }
//                            else if (range2Int.Value > range2Dice.Max)
//                            {
//                                lang(2);
//                                return false;
//                                //vc ja tem um exo maximo
//                            }
//                        }
//                        if (pm1Int != null)
//                        {
//                            if (pm1Dice == null)
//                            {
//                                lang(3);
//                                return false;
//                            }
//                            else if (pm1Int.Value > pm1Dice.Max)
//                            {
//                                lang(3);
//                                return false;
//                                //vc ja tem um exo maximo
//                            }
//                        }
//                        if (pm2Int != null)
//                        {
//                            if (pm2Dice == null)
//                            {
//                                lang(3);
//                                return false;
//                            }
//                            else if (pm2Int.Value > pm2Dice.Max)
//                            {
//                                lang(3);
//                                return false;
//                                //vc ja tem um exo maximo
//                            }
//                        }
//                        if (pa1Int != null)
//                        {
//                            if (pa1Dice == null)
//                            {
//                                lang(4);
//                                return false;
//                            }
//                            else if (pa1Int.Value > pa1Dice.Max)
//                            {
//                                lang(4);
//                                return false;
//                                //vc ja tem um exo maximo
//                            }
//                        }
//                        if (invoc1Int != null)
//                        {
//                            if (invoc1Dice == null)
//                            {
//                                lang(5);
//                                return false;
//                            }
//                            else if (invoc1Int.Value > invoc1Dice.Max)
//                            {
//                                lang(5);
//                                return false;
//                                //vc ja tem um exo maximo
//                            }
//                        }
//                        // n acontecer nada '-' simplesmente seguirá em frente
//                        travar = true;
//                        return true;
//                    }
//                    else
//                    {
//                        lang(1);
//                        return false;
//                        // vc ja atingiu o exo maximo deste efeito
//                    }
//                }
//                else
//                {
//                    if (atualDice != null)
//                    { //  o item n tem esse exo mas o template tem, ou seja, ele caiu o atributo e esta tentando colocar novamente

//                        return true;
//                    }
//                    //// adicionar um atributo q o item n tem, verificar se ele tem um exo dew outro atributo
//                    ///if (range1 != null)
//                    if (range1Int != null)
//                    {
//                        if (range1Dice == null)
//                        {
//                            lang(2);
//                            return false;
//                        }
//                        else if (range1Int.Value > range1Dice.Max)
//                        {
//                            lang(2);
//                            return false;
//                            //vc ja tem um exo maximo
//                        }
//                    }
//                    if (range2Int != null)
//                    {
//                        if (range2Dice == null)
//                        {
//                            lang(2);
//                            return false;
//                        }
//                        else if (range2Int.Value > range2Dice.Max)
//                        {
//                            lang(2);
//                            return false;
//                            //vc ja tem um exo maximo
//                        }
//                    }
//                    if (pm1Int != null)
//                    {
//                        if (pm1Dice == null)
//                        {
//                            lang(3);
//                            return false;
//                        }
//                        else if (pm1Int.Value > pm1Dice.Max)
//                        {
//                            lang(3);
//                            return false;
//                            //vc ja tem um exo maximo
//                        }
//                    }
//                    if (pm2Int != null)
//                    {
//                        if (pm2Dice == null)
//                        {
//                            lang(3);
//                            return false;
//                        }
//                        else if (pm2Int.Value > pm2Dice.Max)
//                        {
//                            lang(3);
//                            return false;
//                            //vc ja tem um exo maximo
//                        }
//                    }
//                    if (pa1Int != null)
//                    {
//                        if (pa1Dice == null)
//                        {
//                            lang(4);
//                            return false;
//                        }
//                        else if (pa1Int.Value > pa1Dice.Max)
//                        {
//                            lang(4);
//                            return false;
//                            //vc ja tem um exo maximo
//                        }
//                    }
//                    if (invoc1Int != null)
//                    {
//                        if (invoc1Dice == null)
//                        {
//                            lang(5);
//                            return false;
//                        }
//                        else if (invoc1Int.Value > invoc1Dice.Max)
//                        {
//                            lang(5);
//                            return false;
//                            //vc ja tem um exo maximo
//                        }
//                    }
//                    // n acontecer nada '-' simplesmente seguirá em frente
//                    travar = true;
//                    return true;

//                }
//                #endregion
//            }

//            // nunca  irar chegar aki '-'
//            return false;

//        }
//        private EffectInteger GetEffectToImprove(EffectInteger runeEffect, BasePlayerItem dropOnItem)
//        {
//            return dropOnItem.Effects.OfType<EffectInteger>().FirstOrDefault(x => x.EffectId == runeEffect.EffectId || (x.Template.OppositeId != 0 && x.Template.OppositeId == runeEffect.Id));
//        }

//        private void BoostEffect(EffectBase runeEffect, BasePlayerItem dropOnItem)
//        {
//            var effect = GetEffectToImprove((runeEffect as EffectInteger), dropOnItem);


//            if (effect != null)
//            {
//                effect.Value += (short)((effect.Template.BonusType == -1 ? -1 : 1) * (runeEffect as EffectInteger).Value); // Value

//                if (effect.Value == 0)
//                    dropOnItem.Effects.Remove(effect);
//                else if (effect.Value > 0 && effect.Value <= (runeEffect as EffectInteger).Value && effect.Template.OppositeId > 0) // from negativ to positiv
//                {
//                    dropOnItem.Effects.Remove(effect);
//                    dropOnItem.Effects.Add(new EffectInteger((EffectsEnum)effect.Template.OppositeId, effect.Value));
//                }
//            }
//            else
//            {
//                dropOnItem.Effects.Add(new EffectInteger(runeEffect.EffectId, (runeEffect as EffectInteger).Value));
//            }
//        }

//        public override bool Drop(BasePlayerItem dropOnItem)
//        {
//            var allowedItemType = new[] {
//                ItemTypeEnum.AMULETTE,
//                ItemTypeEnum.ARC,
//                ItemTypeEnum.BAGUETTE,
//                ItemTypeEnum.BÂTON,
//                ItemTypeEnum.DAGUE,
//                ItemTypeEnum.ÉPÉE,
//                ItemTypeEnum.MARTEAU,
//                ItemTypeEnum.PELLE,
//                ItemTypeEnum.ANNEAU,
//                ItemTypeEnum.CEINTURE,
//                ItemTypeEnum.BOTTES,
//                ItemTypeEnum.CHAPEAU,
//                ItemTypeEnum.CAPE,
//                ItemTypeEnum.HACHE,
//                ItemTypeEnum.PIOCHE,
//                ItemTypeEnum.FAUX,
//                ItemTypeEnum.SAC_À_DOS
//            };

//            if (!allowedItemType.Contains((ItemTypeEnum)dropOnItem.Template.TypeId))
//            {
//                lang(6);
//                return false;
//            }

//            bool travar;
//            var Verificãcao = teste(Effects.FirstOrDefault().EffectId, dropOnItem, out travar);

//            if (Verificãcao == false)
//            {
//                return false;
//            }

//            if (travar)
//            {
//                var effectBase = dropOnItem.Effects.FirstOrDefault(x => x.EffectId == EffectsEnum.Effect_1081);

//                if (effectBase != null)
//                    dropOnItem.Effects.Remove(effectBase);

//                dropOnItem.Effects.Add(new EffectString(EffectsEnum.Effect_1082, Owner.Namedefault));

//            }

//            ApplyEffects(dropOnItem, ItemEffectHandler.HandlerOperation.UNAPPLY);
//            BoostEffect(Effects.FirstOrDefault(), dropOnItem);
//            dropOnItem.Invalidate();
//            Owner.Inventory.RefreshItem(dropOnItem);
//            dropOnItem.OnObjectModified();

//            ApplyEffects(dropOnItem, ItemEffectHandler.HandlerOperation.APPLY);
//            Owner.RefreshStats();

//            lang(7);

//            return true;
//        }

//        private void ApplyEffects(BasePlayerItem item, ItemEffectHandler.HandlerOperation operation)
//        {
//            foreach (var handler in item.Effects.Select(effect => EffectManager.Instance.GetItemEffectHandler(effect, Owner, this)))
//            {
//                handler.Operation = operation;

//                if (Owner.Inventory.GetEquipedItems().Any(x => x != item && x.GetExoEffects().ToList().Exists(y => item.GetExoEffects().Any(z => z == y)))
//                    && item.GetExoEffects().Any(x => x == handler.Effect))
//                {
//                    handler.Operation = ItemEffectHandler.HandlerOperation.UNAPPLY;
//                }

//                handler.Apply();
//            }
//        }
//    }
//}