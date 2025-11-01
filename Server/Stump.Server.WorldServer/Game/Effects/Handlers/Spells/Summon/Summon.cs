using NLog;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Actors.Fight.Customs;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Monsters;
using Stump.Server.WorldServer.Game.Effects.Handlers.Spells.Buffs;
using Stump.Server.WorldServer.Game.Effects.Handlers.Spells.States;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Fights.Buffs;
using Stump.Server.WorldServer.Game.Fights.Buffs.Customs;
using Stump.Server.WorldServer.Game.Fights.Triggers;
using Stump.Server.WorldServer.Game.Spells;
using Stump.Server.WorldServer.Game.Spells.Casts;
using Stump.Server.WorldServer.Handlers.Actions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Stump.Server.WorldServer.Game.Effects.Handlers.Spells.Summon
{
    [EffectHandler(EffectsEnum.Effect_SummonSlave)]
    [EffectHandler(EffectsEnum.Effect_Summon)]
    public class Summon : SpellEffectHandler
    {
        static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private List<MonsterIdEnum> SteamerInvocs = new List<MonsterIdEnum>
        {
            MonsterIdEnum.TACTIRELLE_3289,
            MonsterIdEnum.TACTIRELLE_5837,
            MonsterIdEnum.GARDIENNE_3288,
            MonsterIdEnum.GARDIENNE_5835,
            MonsterIdEnum.HARPONNEUSE_3287,
            MonsterIdEnum.HARPONNEUSE_5836,
            MonsterIdEnum.CHALUTIER_5141,
            MonsterIdEnum.CHALUTIER_5832,
            MonsterIdEnum.FOREUSE_5142,
            MonsterIdEnum.FOREUSE_5833,
            MonsterIdEnum.BATHYSCAPHE_5143,
            MonsterIdEnum.BATHYSCAPHE_5831,
        };

        private List<SpellIdEnum> OsamodasInvocs = new List<SpellIdEnum>
        {
            SpellIdEnum.BLACK_TOFU_34,
            SpellIdEnum.ALBINO_TOFU_9653,
            SpellIdEnum.GOBBALL_35,
            SpellIdEnum.PODGY_TOFU_38,
            SpellIdEnum.BLACK_GOBBALL_40,
            SpellIdEnum.GOBBLY_9661,
            SpellIdEnum.BLACK_WYRMLING_39,
            SpellIdEnum.ALBINO_WYRMLING_9662,
            SpellIdEnum.BLACK_TOAD_9664,
            SpellIdEnum.ALBINO_TOAD_9658,
            SpellIdEnum.SLIMY_TOAD_9667,
            SpellIdEnum.RED_WYRMLING_31
        };

        private List<SpellIdEnum> InvocsControl = new List<SpellIdEnum>
        {
            SpellIdEnum.FRIENDSHIP_WORD_13176,
            SpellIdEnum.AFFECTIONATE_WORD_13200,
            SpellIdEnum.FLYING_SWORD_12744,
            SpellIdEnum.SWIFT_SWORD_12765,
            SpellIdEnum.YAPPER_LAUNCHER_13774,
            SpellIdEnum.FIRST_13998,
            SpellIdEnum.SECOND_14006,
            SpellIdEnum.THIRD_14007,
            SpellIdEnum.FOURTH_14008,
            SpellIdEnum.FIFTH_14009,
            SpellIdEnum.SIXTH_14010,
            SpellIdEnum.LIVING_BAG_13328,
            SpellIdEnum.LIVING_SATCHEL_13354,
            SpellIdEnum.LIVING_SHOVEL_13344,
            SpellIdEnum.LIVING_SPADE_13361,
            SpellIdEnum.LIVING_CHEST_13347,
            SpellIdEnum.REGENERATING_CHEST_13371,
            SpellIdEnum.SUMMONING_CLAW_12856,
            SpellIdEnum.SUMMONING_STROKE_12869,
            SpellIdEnum.ELEMENTAL_GUARDIAN_13720,
            SpellIdEnum.GRIMACE_13424,
            SpellIdEnum.SPIRIT_BOND_12799,
            SpellIdEnum.MADOLL_13564,
            SpellIdEnum.TRANSMUTED_MADOLL_13515,
            SpellIdEnum.THE_BLOCK_13561,
            SpellIdEnum.TRANSMUTED_BLOCK_13526,
            SpellIdEnum.SACRIFICIAL_DOLL_13567,
            SpellIdEnum.TRANSMUTED_SACRIFICED_13522,
            SpellIdEnum.INFLATABLE_13573,
            SpellIdEnum.TRANSMUTED_INFLATABLE_13523,
            SpellIdEnum.ULTRA_POWERFUL_13578,
            SpellIdEnum.TRANSMUTED_ULTRA_POWERFUL_13520,
        };

        public Summon(EffectDice effect, FightActor caster, SpellCastHandler castHandler, Cell targetedCell, bool critical) : base(effect, caster, castHandler, targetedCell, critical)
        { }

        protected override bool InternalApply()
        {
            var monster = MonsterManager.Instance.GetMonsterGrade(Dice.DiceNum, Dice.DiceFace);

            if (monster == null)
            {
                logger.Error("Cannot summon monster {0} grade {1} (not found)", Dice.DiceNum, Dice.DiceFace);
                return false;
            }

            if (Spell.Id == (int)SpellIdEnum.AFFECTIONATE_WORD_13200 && Caster.HasSummonnerById((int)MonsterIdEnum.LAPINO_PROTECTEUR_5905))
                return false;

            if (monster.Template.UseSummonSlot && !Caster.CanSummon())
                return false;

            if (Fight.GetOneFighter(TargetedCell) != null)
            {
                if (AffectedCells.Count <= 1)
                    return false;

                var Cell = AffectedCells.Where(x => Fight.GetOneFighter(x) == null && x.Walkable && x.Id != Caster.Cell.Id).OrderBy(y => Caster.Position.Point.ManhattanDistanceTo(new Maps.Cells.MapPoint(y))).FirstOrDefault();

                if (Cell == null)
                    return false;

                TargetedCell = Cell;
            }

            if (!Caster.HasState(600) && (Spell.Template.Id == (int)SpellIdEnum.FIRE_RUNE_6192 || Spell.Template.Id == (int)SpellIdEnum.WATER_RUNE_6193 || Spell.Template.Id == (int)SpellIdEnum.AIR_RUNE_6194 || Spell.Template.Id == (int)SpellIdEnum.EARTH_RUNE_6195))
                return false;

            SummonedFighter summon;

            #region >> Monster Epe_Volante_434
            if (monster.Template.Id == (int)MonsterIdEnum.EPE_VOLANTE_434 && Caster.HasState((int)SpellStatesEnum.BLOODTHIRSTY_489))
            {
                monster = MonsterManager.Instance.GetMonsterGrade((int)MonsterIdEnum.EPE_ECORCHEUSE_4756, Dice.DiceFace);
            }
            else if (monster.Template.Id == (int)MonsterIdEnum.EPE_VOLANTE_434 && Caster.HasState((int)SpellStatesEnum.ROBUST_490))
            {
                monster = MonsterManager.Instance.GetMonsterGrade((int)MonsterIdEnum.EPE_GARDIENNE_4757, Dice.DiceFace);
            }
            else if (monster.Template.Id == (int)MonsterIdEnum.EPE_VOLANTE_434 && Caster.HasState((int)SpellStatesEnum.SWIFT_488))
            {
                monster = MonsterManager.Instance.GetMonsterGrade((int)MonsterIdEnum.PE_VLOCE_5192, Dice.DiceFace);
            }
            #endregion

            #region >> Summoned Monsters Turret/Chest/Monster
            if (SteamerInvocs.Contains((MonsterIdEnum)monster.Template.Id))
            {
                summon = new SummonedTurret(Fight.GetNextContextualId(), Caster, monster, Spell, TargetedCell) { SummoningEffect = this };
            }
            else if (monster.Template.Id == (int)MonsterIdEnum.COFFRE_ANIM_285 || monster.Template.Id == (int)MonsterIdEnum.COFFRE_ANIM_5840)
            {
                summon = new LivingChest(Fight.GetNextContextualId(), Caster.Team, Caster, monster, TargetedCell) { SummoningEffect = this };
            }
            else
            {
                summon = new SummonedMonster(Fight.GetNextContextualId(), Caster.Team, Caster, monster, TargetedCell) { SummoningEffect = this };
            }
            #endregion

            if (Effect.Id == (short)EffectsEnum.Effect_SummonSlave && Caster is CharacterFighter)
                summon.SetController(Caster as CharacterFighter);

            //if (InvocsControl.Contains((SpellIdEnum)Spell.Template.Id) && Caster is CharacterFighter)
            //    summon.SetController(Caster as CharacterFighter);

            Boolean hasTakeControl = Caster.GetBuffs(x => x.Spell.Id == (int)SpellIdEnum.SUMMONS_SKILL_18646).Any(); //Dominio de Invocações 2.61.10.19 by Kenshin

            if (hasTakeControl)
                summon.SetController(Caster as CharacterFighter);

            ActionsHandler.SendGameActionFightSummonMessage(Fight.Clients, summon);

            Caster.AddSummon(summon);
            Caster.Team.AddFighter(summon);
            Fight.TriggerMarks(summon.Cell, summon, TriggerType.MOVE);

            //if (OsamodasInvocs.Contains((SpellIdEnum)Spell.Id)) //Controler de Invoc
            //{
            //    TakeControlSummon(summon);
            //}

            return true;
        }

        private void TakeControlSummon(SummonedFighter summon)
        {
            TakeControlBuff controlBuff = null;
            StateBuff stateBuff = null;
            SpellImmunityBuff immun = null;
            StatBuff apbuff = null;

            foreach (var item in Caster.GetBuffs())
            {
                if (item.Dice.Value == 432 || item.Dice.Value == 433 || item.Dice.Value == 434 || item.Dice.Value == 599)
                {
                    var id = Caster.PopNextBuffId();
                    controlBuff = new TakeControlBuff(id, Caster, Caster, new TakeControl(item.Spell.CurrentSpellLevel.Effects.Find(x => x.EffectId == EffectsEnum.Effect_TakeControl), Caster, null, summon.Cell, false), item.Spell, FightDispellableEnum.DISPELLABLE_BY_DEATH, summon as SummonedMonster) { Duration = (short)item.Duration };
                    stateBuff = new StateBuff(id + 1, summon, Caster, new AddState(item.Spell.CurrentSpellLevel.Effects.Find(x => x.EffectId == EffectsEnum.Effect_AddState && x.Value != 447), Caster, null, summon.Cell, false), item.Spell, FightDispellableEnum.DISPELLABLE_BY_DEATH, SpellManager.Instance.GetSpellState((uint)item.Dice.Value)) { Duration = (short)item.Duration };
                    immun = new SpellImmunityBuff(id + 2, summon, Caster, new SpellImmunity(item.Spell.CurrentSpellLevel.Effects.Find(x => x.EffectId == EffectsEnum.Effect_SpellImmunity), Caster, null, summon.Cell, false), item.Spell, item.Spell.CurrentSpellLevel.Effects.Find(x => x.EffectId == EffectsEnum.Effect_SpellImmunity).DiceNum, false, FightDispellableEnum.DISPELLABLE_BY_DEATH) { Duration = (short)item.Duration };
                    apbuff = new StatBuff(id + 3, summon, Caster, new APBuff(item.Spell.CurrentSpellLevel.Effects.Find(x => x.EffectId == EffectsEnum.Effect_AddAP_111), Caster, null, summon.Cell, false), item.Spell, 2, PlayerFields.AP, false, FightDispellableEnum.DISPELLABLE_BY_DEATH) { Duration = (short)item.Duration };
                }
            }

            if (controlBuff == null || stateBuff == null)
                return;


            if ((summon as SummonedMonster).Monster.Template.Id == (int)MonsterIdEnum.TOFU_NOIR_4561 ||
                (summon as SummonedMonster).Monster.Template.Id == (int)MonsterIdEnum.TOFU_DODU_4562 ||
                (summon as SummonedMonster).Monster.Template.Id == (int)MonsterIdEnum.TOFU_ALBINOS_5131)

            {
                if (Caster.HasState(432))
                {
                    Caster.AddBuff(controlBuff);
                    summon.AddBuff(stateBuff);
                    summon.AddBuff(immun);
                    summon.AddBuff(apbuff);
                }
            }
            else if ((summon as SummonedMonster).Monster.Template.Id == (int)MonsterIdEnum.BOUFTOU_4563 ||
              (summon as SummonedMonster).Monster.Template.Id == (int)MonsterIdEnum.BOUFTOU_NOIR_4564 ||
              (summon as SummonedMonster).Monster.Template.Id == (int)MonsterIdEnum.BOUFTON_5132)

            {
                if (Caster.HasState(433))
                {
                    Caster.AddBuff(controlBuff);
                    summon.AddBuff(stateBuff);
                    summon.AddBuff(immun);
                    summon.AddBuff(apbuff);
                }
            }
            else if ((summon as SummonedMonster).Monster.Template.Id == (int)MonsterIdEnum.DRAGONNET_ROUGE_4565 ||
              (summon as SummonedMonster).Monster.Template.Id == (int)MonsterIdEnum.DRAGONNET_NOIR_4566 ||
              (summon as SummonedMonster).Monster.Template.Id == (int)MonsterIdEnum.DRAGONNET_ALBINOS_5133)
            {
                if (Caster.HasState(434))
                {
                    Caster.AddBuff(controlBuff);
                    summon.AddBuff(stateBuff);
                    summon.AddBuff(immun);
                    summon.AddBuff(apbuff);
                }
            }
            else if ((summon as SummonedMonster).Monster.Template.Id == (int)MonsterIdEnum.CRAPAUD_NOIR_5134 ||
              (summon as SummonedMonster).Monster.Template.Id == (int)MonsterIdEnum.CRAPAUD_ALBINOS_5135 ||
              (summon as SummonedMonster).Monster.Template.Id == (int)MonsterIdEnum.CRAPAUD_BAVEUX_5136)
            {
                if (Caster.HasState(599))
                {
                    Caster.AddBuff(controlBuff);
                    summon.AddBuff(stateBuff);
                    summon.AddBuff(immun);
                    summon.AddBuff(apbuff);
                }
            }
        }
    }
}