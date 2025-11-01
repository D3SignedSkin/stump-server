using System;
using System.Collections.Generic;
using System.Linq;
using Stump.Core.Extensions;
using Stump.DofusProtocol.D2oClasses;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Database.Items;
using Stump.Server.WorldServer.Database.Items.Pets;
using Stump.Server.WorldServer.Database.Items.Templates;
using Stump.Server.WorldServer.Database.Monsters;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Actors.Look;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Effects;
using Stump.Server.WorldServer.Game.Effects.Handlers.Items;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Fights;
using System.Drawing;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Monsters;

namespace Stump.Server.WorldServer.Game.Items.Player.Custom
{
    [ItemType(ItemTypeEnum.MONTILIER_121)]
    [ItemType(ItemTypeEnum.FAMILIER_18)]
    public sealed class PetItem : BasePlayerItem
    {
        private List<(int id, short size)> petSkins = new List<(int id, short size)>
        {
            (27719, 130),//Paladur
            (20052, 80), //Minowang
            (10107, 130),//Borcego
            (15133, 130),//Black Pekewabbit
            (23647, 80), //Zobascote
        };

        private List<(int id, short size)> MontSkins = new List<(int id, short size)>
        {
            (27323, 140),//Lupokami
            (18505, 150),//Perfilax
            (22996, 100),//Grifforis, o Soberano
            (22994, 100),//Grifforis, o Campeão
            (22992, 100),//Grifforis, o Sábio
            (22990, 100),//Grifforis, o Corajoso
            (28359, 120),//Besta de Teste
            (28368, 130),//Bruturso
            (28386, 140),//Lupino
            (28387, 140),//Ulcervo
            (23070, 150),//Codex Æris
            (23069, 150),//Codex Terra
            (23073, 150),//Codex Shushum
            (23072, 150),//Codex Magicae
            (23071, 150),//Codex Ignis
            (23068, 150),//Codex Aqua
            (19031, 130),//Tofuror
            (19380, 140),//Bregão
            (22361, 145),//Awaw de Servidão
            (21872, 130),//Javaloinc
            (18500, 155),//Perfolã
            (26212, 120),//Ursolar
            (22359, 120),//Paparog de Guerra
            (22360, 120),//Abutre de Miséria
            (25846, 120),//Sherajah Blindada
            (27668, 90),//Espírito de Descendor
            (29646, 160),//Corpulental Ström
        };

        private Dictionary<int, EffectDice> m_monsterKilledEffects;

        public PetItem(Character owner, PlayerItemRecord record) : base(owner, record)
        {
            PetTemplate = PetManager.Instance.GetPetTemplate(Template.Id);

            if (PetTemplate == null)
                return;

            MaxPower = IsRegularPet ? GetItemMaxPower() : 0;

            InitializeEffects();

            if (IsEquiped())
                Owner.FightEnded += OnFightEnded;
        }

        private void InitializeEffects()
        {
            if (Effects.OfType<EffectInteger>().All(x => x.EffectId != EffectsEnum.Effect_PetLevel)) //New Pet Item
            {
                Effects.Add(new EffectDice(EffectsEnum.Effect_PetLevel, 1, 18, 0));
                Effects.Add(new EffectDice(EffectsEnum.Effect_PetExp, 0, 0, (int)ExperienceManager.Instance.GetPetNextLevelExperience(1)));

                ChangeEffectsSquallingOnLevel(0);
            }
        }

        public bool IsRegularPet => PetTemplate?.PossibleEffects.Count > 0;

        public int Experience
        {
            get { return (Effects.Where(x => x.EffectId == EffectsEnum.Effect_PetExp).FirstOrDefault() as EffectDice).Value; }
            set { (Effects.Where(x => x.EffectId == EffectsEnum.Effect_PetExp).FirstOrDefault() as EffectDice).Value = value; }
        }

        public int MaxExperience
        {
            get { return (Effects.Where(x => x.EffectId == EffectsEnum.Effect_PetExp).FirstOrDefault() as EffectDice).DiceFace; }
            set { (Effects.Where(x => x.EffectId == EffectsEnum.Effect_PetExp).FirstOrDefault() as EffectDice).DiceFace = value; }
        }

        public int Level
        {
            get { return (Effects.Where(x => x.EffectId == EffectsEnum.Effect_PetLevel).FirstOrDefault() as EffectDice).Value; }
            set { (Effects.Where(x => x.EffectId == EffectsEnum.Effect_PetLevel).FirstOrDefault() as EffectDice).Value = value; }
        }

        public PetTemplate PetTemplate
        {
            get;
        }

        public double MaxPower
        {
            get;
        }

        public override bool Feed(BasePlayerItem food)
        {
            if (CanFeedPet(food))
                return false;

            var xpToAdd = 0;
            var exp = food.Effects.Where(x => x.EffectId == EffectsEnum.Effect_Exp).FirstOrDefault();

            if (food.Template.Id == 25358 && this.Template.Type.ItemType == ItemTypeEnum.MONTILIER_121) //Autopilotado
            {
                Effects.Add(new EffectInteger(EffectsEnum.Effet_Autopilotable, 0));

                Invalidate();
                Owner.Inventory.RefreshItem(this);

                Owner.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 32);
                return true;
            }

            if (food.Template.Id == (int)ItemIdEnum.REFLEXE_DE_BEBEMOTH_20968 && Level > 100)
            {
                Effects.Add(new EffectInteger(EffectsEnum.Effect_LegendaryState, 1));
            }

            if (food.Template.Id == 20974 || food.Template.Id == 20975 || food.Template.Id == 20973 || food.Template.Id == 20976)
            {
                AddLegendaryEffect(food.Template.Id);
            }

            if (exp != null)
            {
                xpToAdd += (exp as EffectInteger).Value;
            }
            else
            {
                int XpValue = 0;
                List<DroppableItem> _dropList = MonsterManager.Instance.GetDropsByItemId(food.Template.Id);

                if (_dropList is null || _dropList.Count <= 0)
                    return false;

                DroppableItem _itemDrop = MonsterManager.Instance.GetDropsByItemId(food.Template.Id).OrderBy(entry => entry.DropRateForGrade1).FirstOrDefault();

                if (_itemDrop is null)
                    return false;

                ItemTemplate _itemTemplate = ItemManager.Instance.TryGetTemplate(_itemDrop.ItemId);

                if (_itemTemplate is null)
                    return false;

                if (_itemTemplate.Level > this.Level)
                {
                    if (_itemDrop.DropRateForGrade1 <= 10)
                        XpValue = 46;
                    else if (_itemDrop.DropRateForGrade1 >= 11 && _itemDrop.DropRateForGrade1 <= 30)
                        XpValue = 33;
                    else if (_itemDrop.DropRateForGrade1 >= 31 && _itemDrop.DropRateForGrade1 <= 50)
                        XpValue = 33;
                    else if (_itemDrop.DropRateForGrade1 >= 51 && _itemDrop.DropRateForGrade1 <= 80)
                        XpValue = 16;
                    else if (_itemDrop.DropRateForGrade1 >= 81)
                        XpValue = 8;

                    xpToAdd += XpValue;
                }
            }

            if (Level <= 100)
            {
                Experience += xpToAdd;
            }

            while (Experience >= ExperienceManager.Instance.GetPetNextLevelExperience((ushort)Level) && Level <= 100)
            {
                Level++;
            }

            if (Level <= 100)
            {
                MaxExperience = (int)ExperienceManager.Instance.GetPetNextLevelExperience((ushort)Level);
            }

            ChangeEffectsSquallingOnLevel(Level);

            Invalidate();
            Owner.Inventory.RefreshItem(this);

            Owner.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 32);
            return true;
        }

        private bool CanFeedPet(BasePlayerItem food)
        {
            if (food.Template.Id == (int)(ItemIdEnum)25358)
            {
                if (this.Effects.Any(effect => effect.EffectId == EffectsEnum.Effet_Autopilotable))
                {
                    return true;
                }
            }
            else
            {
                if (IsDeleted || food.AppearanceId != 0 || food.Template.TypeId == (int)ItemTypeEnum.FAMILIER_18 || food.Template.TypeId == (int)ItemTypeEnum.MONTILIER_121)
                    return true;

                if (food.Template.Type.ItemType == ItemTypeEnum.RUNE_DE_FORGEMAGIE_78 || food.Template.Type.ItemType == ItemTypeEnum.RUNE_DE_TRANSCENDANCE_211)
                    return true;

                if (Level > food.Template.Level && !food.Template.Effects.Any(x => x.EffectId == EffectsEnum.Effect_Exp))
                    return true;
            }

            return false;
        }

        public void AddLegendaryEffect(int legendaryFoodId)
        {
            List<int> primaryEffectsDicenum = new List<int>()
            {
                12485,
                12483,
                12486,
                12482,
                12484,
                12488,
                12492,
                12487,
                12497,
                12490,
                12489,
                12498,
                12475,
                12480,
                12481,
                12479,
                12477
            };
            List<int> secondyEffectsDicenum = new List<int>()
            {
                12505,
                12501,
                12499,
                12503,
                12500,
                12506,
            };

            EffectBase legendary = new EffectBase();

            foreach (var effect in Effects)
            {
                var eff = effect as EffectDice;

                if (eff is EffectDice && primaryEffectsDicenum.Contains(eff.DiceNum))
                {
                    Effects.Remove(effect);
                    break;
                }
                else if (eff is EffectDice && secondyEffectsDicenum.Contains(eff.DiceNum))
                {
                    legendary = effect;
                }
            }

            var random = new Random();

            #region Effects Legendary
            if (legendaryFoodId == 20974 && legendary == new EffectBase()) //Quintessência de Pukachi
            {
                var rnd = random.Next(1, 5);

                switch (rnd)
                {
                    case 1:
                        Effects.Add(new EffectDice(EffectsEnum.Effect_CastSpell_1175, 0, (int)SpellIdEnum.BWORK_PRECISION_12485, 1));
                        break;

                    case 2:
                        Effects.Add(new EffectDice(EffectsEnum.Effect_CastSpell_1175, 0, (int)SpellIdEnum.WABBIT_IMPATIENCE_12483, 1));
                        break;

                    case 3:
                        Effects.Add(new EffectDice(EffectsEnum.Effect_CastSpell_1175, 0, (int)SpellIdEnum.BOOWOLF_FURY_12486, 1));
                        break;

                    case 4:
                        Effects.Add(new EffectDice(EffectsEnum.Effect_CastSpell_1175, 0, (int)SpellIdEnum.MOSKITO_THIRST_12482, 1));
                        break;

                    case 5:
                        Effects.Add(new EffectDice(EffectsEnum.Effect_CastSpell_1175, 0, (int)SpellIdEnum.TROOL_BRUTALITY_12484, 1));
                        break;
                }
            }
            else if (legendaryFoodId == 20975 && legendary == new EffectBase()) //Quintessência de Magikaranguejo
            {
                var rnd2 = random.Next(1, 7);

                switch (rnd2)
                {
                    case 1:
                        Effects.Add(new EffectDice(EffectsEnum.Effect_CastSpell_1175, 0, (int)SpellIdEnum.GOBBETTE_SWEETNESS_12488, 1));
                        break;

                    case 2:
                        Effects.Add(new EffectDice(EffectsEnum.Effect_CastSpell_1175, 0, (int)SpellIdEnum.GREEDOBLOP_GLUTTONY_12492, 1));
                        break;

                    case 3:
                        Effects.Add(new EffectDice(EffectsEnum.Effect_CastSpell_1175, 0, (int)SpellIdEnum.BWAK_GENEROSITY_12487, 1));
                        break;

                    case 4:
                        Effects.Add(new EffectDice(EffectsEnum.Effect_CastSpell_1175, 0, (int)SpellIdEnum.CHAFER_IMMORTALITY_12497, 1));
                        break;

                    case 5:
                        Effects.Add(new EffectDice(EffectsEnum.Effect_CastSpell_1175, 0, (int)SpellIdEnum.SAUROSHELL_PROTECTION_12490, 1));
                        break;

                    case 6:
                        Effects.Add(new EffectDice(EffectsEnum.Effect_CastSpell_1175, 0, (int)SpellIdEnum.CRACKLER_ROBUSTNESS_12489, 1));
                        break;

                    case 7:
                        Effects.Add(new EffectDice(EffectsEnum.Effect_CastSpell_1175, 0, (int)SpellIdEnum.TREECHNID_BARK_12498, 1));
                        break;
                }
            }
            else if (legendaryFoodId == 20973 && legendary == new EffectBase()) //Quintessência de Kwarkwetch
            {
                var rnd3 = random.Next(1, 5);

                switch (rnd3)
                {
                    case 1:
                        Effects.Add(new EffectDice(EffectsEnum.Effect_CastSpell_1175, 0, (int)SpellIdEnum.PIWI_VELOCITY_12475, 1));
                        break;

                    case 2:
                        Effects.Add(new EffectDice(EffectsEnum.Effect_CastSpell_1175, 0, (int)SpellIdEnum.ARACHNEE_WEB_12480, 1));
                        break;

                    case 3:
                        Effects.Add(new EffectDice(EffectsEnum.Effect_CastSpell_1175, 0, (int)SpellIdEnum.HYSTRITCHERIA_12481, 1));
                        break;

                    case 4:
                        Effects.Add(new EffectDice(EffectsEnum.Effect_CastSpell_1175, 0, (int)SpellIdEnum.KITSOU_EVASION_12479, 1));
                        break;

                    case 5:
                        Effects.Add(new EffectDice(EffectsEnum.Effect_CastSpell_1175, 0, (int)SpellIdEnum.KOALAK_SAVINGS_12477, 1));
                        break;
                }
            }
            else if (legendaryFoodId == 20976 && legendary == new EffectBase()) //Quintessência de Bakusharna
            {
                var rnd3 = random.Next(1, 6);

                switch (rnd3)
                {
                    case 1:
                        Effects.Add(new EffectDice(EffectsEnum.Effect_CastSpell_1175, 0, (int)SpellIdEnum.SCURVION_TOXICITY_12505, 1));
                        break;

                    case 2:
                        Effects.Add(new EffectDice(EffectsEnum.Effect_CastSpell_1175, 0, (int)SpellIdEnum.PANDORAS_CALL_12501, 1));
                        break;

                    case 3:
                        Effects.Add(new EffectDice(EffectsEnum.Effect_CastSpell_1175, 0, (int)SpellIdEnum.SPIMUSH_ALTRUISM_12499, 1));
                        break;

                    case 4:
                        Effects.Add(new EffectDice(EffectsEnum.Effect_CastSpell_1175, 0, (int)SpellIdEnum.DOMOIZELLE_DEVOTION_12503, 1));
                        break;

                    case 5:
                        Effects.Add(new EffectDice(EffectsEnum.Effect_CastSpell_1175, 0, (int)SpellIdEnum.TOFOONE_BENEVOLENCE_12500, 1));
                        break;

                    case 6:
                        Effects.Add(new EffectDice(EffectsEnum.Effect_CastSpell_1175, 0, (int)SpellIdEnum.PERSAPORT_EMPATHY_12506, 1));
                        break;
                }
            }
            else
            {
                Owner.SendServerMessage("Seu animal de estimação tem um bônus especial que o impede de alterar os efeitos.", Color.OrangeRed);
            }
            #endregion
        }

        public override bool OnRemoveItem()
        {
            return base.OnRemoveItem();
        }

        private List<EffectBase> EffectsMax()
        {
            var effects = Template.Effects.Where(x => x.EffectId != EffectsEnum.Effect_PetLevel && x.EffectId != EffectsEnum.Effect_PetExp).ToList();
            return effects;
        }

        private void ChangeEffectsSquallingOnLevel(int level)
        {
            foreach (var effect in EffectsMax())
            {
                var effectToChange = Effects.OfType<EffectInteger>().Where(x => x.EffectId == effect.EffectId).FirstOrDefault();

                if (effectToChange != null)
                {
                    if (IsEquiped())
                    {
                        var handler = EffectManager.Instance.GetItemEffectHandler(effectToChange, Owner, this);
                        handler.Operation = ItemEffectHandler.HandlerOperation.UNAPPLY;
                        handler.Apply();
                        effectToChange.Value = (short)Math.Floor((effect as EffectDice).Max / 101f * level);
                        handler.Operation = ItemEffectHandler.HandlerOperation.APPLY;
                        handler.Apply();

                        Owner.RefreshStats();
                    }
                    else
                    {
                        effectToChange.Value = (short)Math.Floor(((effect as EffectDice).Max / 101f * level));
                    }
                }
            }
        }

        public override bool OnEquipItem(bool unequip)
        {
            if (unequip)
                Owner.FightEnded -= OnFightEnded;
            else
                Owner.FightEnded += OnFightEnded;

            if (unequip)
                return base.OnEquipItem(true);

            if (Owner.IsRiding)
                Owner.ForceDismount();

            return base.OnEquipItem(false);
        }

        public override ActorLook UpdateItemSkin(ActorLook characterLook)
        {
            var petLook = PetTemplate?.Look?.Clone();

            if (Template.Type.ItemType != ItemTypeEnum.FAMILIER_18 && Template.Type.ItemType != ItemTypeEnum.MONTILIER_121)
                return characterLook;

            if (petLook == null)
            {
                if (Template.Type.ItemType == ItemTypeEnum.FAMILIER_18)
                {
                    if (IsEquiped())
                    {
                        short Size = PetTemplate?.LookSize ?? 90;
                        var appareanceId = Template.AppearanceId;

                        if (AppearanceId != 0)
                            appareanceId = AppearanceId;

                        if (Effects.Any(x => x.EffectId == EffectsEnum.Effect_Apparence_Wrapper))
                        {
                            var petSkinId = (Effects.FirstOrDefault(x => x.EffectId == EffectsEnum.Effect_Apparence_Wrapper) as EffectInteger)?.Value;

                            if (petSkinId.HasValue)
                            {
                                var petSkin = petSkins.FirstOrDefault(s => s.id == petSkinId.Value);

                                if (petSkin != default)
                                    Size = petSkin.size;
                                else
                                    Size = 90;
                            }
                        }

                        characterLook.SetPetSkin((short)appareanceId, new short[] { Size });

                        Color color1;
                        Color color2;
                        Color color3;
                        Color color4;
                        Color color5;

                        if (characterLook.Colors.TryGetValue(1, out color1) &&
                            characterLook.Colors.TryGetValue(2, out color2) &&
                            characterLook.Colors.TryGetValue(3, out color3) &&
                            characterLook.Colors.TryGetValue(4, out color4) &&
                            characterLook.Colors.TryGetValue(5, out color5))
                        {
                            if (characterLook.PetLook != null)
                            {
                                characterLook.PetLook.AddColor(1, color1);
                                characterLook.PetLook.AddColor(2, color2);
                                characterLook.PetLook.AddColor(3, color3);
                                characterLook.PetLook.AddColor(4, color4);
                                characterLook.PetLook.AddColor(5, color5);
                            }
                        }
                    }
                    else
                    {
                        characterLook.RemovePets();
                    }
                }
                else if (Template.Type.ItemType == ItemTypeEnum.MONTILIER_121)
                {
                    if (IsEquiped())
                    {
                        short Size = 110;
                        characterLook = characterLook.GetRiderLook() ?? characterLook;

                        if (Effects.Any(x => x.EffectId == EffectsEnum.Effect_Apparence_Wrapper))
                        {
                            var petSkinId = (Effects.FirstOrDefault(x => x.EffectId == EffectsEnum.Effect_Apparence_Wrapper) as EffectInteger)?.Value;

                            if (petSkinId.HasValue)
                            {
                                var petSkin = MontSkins.FirstOrDefault(s => s.id == petSkinId.Value);

                                if (petSkin != default)
                                    Size = petSkin.size;
                                else
                                    Size = 110;
                            }
                        }

                        petLook = ActorLook.Parse("{" + AppearanceId + "|||" + Size + "}");

                        Color color1;
                        Color color2;

                        if (characterLook.Colors.TryGetValue(3, out color1) && characterLook.Colors.TryGetValue(4, out color2))
                        {
                            petLook.AddColor(1, color1);
                            petLook.AddColor(2, color2);
                            petLook.AddColor(3, color1);
                            petLook.AddColor(4, color2);
                            petLook.AddColor(5, color2);
                        }

                        if (AppearanceId != 0)
                            petLook.BonesID = (short)AppearanceId;

                        characterLook.BonesID = 2;
                        petLook.SetRiderLook(characterLook);

                        return petLook;
                    }
                    else
                    {
                        var look = characterLook.GetRiderLook();

                        if (look != null)
                        {
                            characterLook = look;
                            characterLook.BonesID = 1;
                        }

                        return characterLook;
                    }
                }
            }

            return characterLook;
        }

        private void OnFightEnded(Character character, CharacterFighter fighter)
        {
            if (PetTemplate == null)
                return;

            bool update = false;

            if (!fighter.Fight.IsDeathTemporarily && fighter.Fight.Losers == fighter.Team && IsEquiped())
            {
                update = true;
            }

            FightPvM fightPvM = fighter.Fight as FightPvM;

            if (fightPvM != null)
            {
                foreach (var monster in fightPvM.MonsterTeam.Fighters.OfType<MonsterFighter>().Where(x => x.IsDead()))
                {
                    var food = PetTemplate.Foods.FirstOrDefault(x => x.FoodType == FoodTypeEnum.MONSTER && x.FoodId == monster.Monster.Template.Id);

                    if (food != null)
                    {
                        if (IncreaseCreatureKilledCount(monster.Monster.Template) % food.FoodQuantity == 0)
                            AddBonus(food);

                        Invalidate();
                        update = true;
                    }
                }

            }

            if (update)
                Owner.Inventory.RefreshItem(this);
        }

        private int IncreaseCreatureKilledCount(MonsterTemplate monster)
        {
            EffectDice effect;
            if (!m_monsterKilledEffects.TryGetValue(monster.Id, out effect))
            {
                effect = new EffectDice(EffectsEnum.Effect_MonsterKilledCount, 1, (short)monster.Id, 0);
                m_monsterKilledEffects.Add(monster.Id, effect);
                Effects.Add(effect);
            }
            else
            {
                effect.Value++;
            }

            return effect.Value;
        }

        private bool AddBonus(PetFoodRecord food)
        {
            var possibleEffect = PetTemplate.PossibleEffects.OfType<EffectDice>().FirstOrDefault(x => x.EffectId == food.BoostedEffect);
            var effect = Effects.OfType<EffectInteger>().FirstOrDefault(x => x.EffectId == food.BoostedEffect);

            if (possibleEffect == null)
                return false;

            if (effect?.Value >= possibleEffect.Max)
                return false;

            if (PetTemplate.PossibleEffects.Count > 0 && EffectManager.Instance.GetItemPower(this) >= MaxPower)
                return false;

            if (effect == null)
            {
                Effects.Add(effect = new EffectInteger(food.BoostedEffect, (short)food.BoostAmount));
                if (IsEquiped())
                {
                    var handler = EffectManager.Instance.GetItemEffectHandler(effect, Owner, this);
                    handler.Operation = ItemEffectHandler.HandlerOperation.APPLY;
                    handler.Apply();

                    Owner.RefreshStats();
                }
            }
            else
            {
                if (IsEquiped())
                {
                    var handler = EffectManager.Instance.GetItemEffectHandler(effect, Owner, this);
                    handler.Operation = ItemEffectHandler.HandlerOperation.UNAPPLY;
                    handler.Apply();

                    effect.Value += (short)food.BoostAmount;

                    handler.Operation = ItemEffectHandler.HandlerOperation.APPLY;
                    handler.Apply();
                    Owner.RefreshStats();
                }
                else
                    effect.Value += (short)food.BoostAmount;
            }

            return true;
        }

        private double GetItemMaxPower()
        {
            var groups = PetTemplate.Foods.GroupBy(x => x.BoostedEffect).ToArray();
            double max = 0;

            foreach (var group1 in groups)
            {
                var possibleEffect = PetTemplate.PossibleEffects.OfType<EffectDice>().FirstOrDefault(x => x.EffectId == group1.Key);

                if (possibleEffect == null)
                    continue;

                var sum = PetManager.Instance.GetEffectMaxPower(possibleEffect);
                foreach (var group2 in groups.Where(x => x != group1))
                {
                    if (group1.CompareEnumerable(group2))
                    {
                        possibleEffect = PetTemplate.PossibleEffects.OfType<EffectDice>().FirstOrDefault(x => x.EffectId == group1.Key);

                        if (possibleEffect == null)
                            continue;

                        sum += PetManager.Instance.GetEffectMaxPower(possibleEffect);
                    }
                }

                if (sum > max)
                    max = sum;
            }

            return max;
        }

        public override bool CanFeed(BasePlayerItem item)
        {
            return IsRegularPet && item.Template.Type.SuperType != ItemSuperTypeEnum.SUPERTYPE_PET;
        }
    }
}
