using NLog;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Fights.Buffs;
using Stump.Server.WorldServer.Game.Fights.Triggers;
using Stump.Server.WorldServer.Game.Spells;
using Stump.Server.WorldServer.Game.Spells.Casts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Stump.Server.WorldServer.Game.Effects.Handlers.Spells.Others
{
    [EffectHandler(EffectsEnum.Effect_TriggerBuff)]
    [EffectHandler(EffectsEnum.Effect_TriggerBuff_793)]
    [EffectHandler(EffectsEnum.Effect_CastSpell_1160)]
    [EffectHandler(EffectsEnum.Effect_CastSpell_1017)]
    [EffectHandler(EffectsEnum.Effect_CastSpell_2160)]
    [EffectHandler(EffectsEnum.Effect_CastSpell_1175)]
    [EffectHandler(EffectsEnum.Effect_2792)]
    [EffectHandler(EffectsEnum.Effect_2793)] //New TEST
    [EffectHandler(EffectsEnum.Effect_2794)]
    [EffectHandler(EffectsEnum.Effect_1018)]
    [EffectHandler(EffectsEnum.Effect_1019)]
    public class CastSpellEffect : SpellEffectHandler
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private List<int> _spellsBoombs = new List<int>
        {
            (int)SpellIdEnum.LATENT_EXPLOBOMB_13471,  //Bomba INT
            (int)SpellIdEnum.LATENT_WATER_BOMB_13478, //Bomba Agua
            (int)SpellIdEnum.LATENT_GRENADO_13474,    //Bomba Ar
            (int)SpellIdEnum.LATENT_SEISMOBOMB_13486, //Bomba Terra
        };

        public CastSpellEffect(EffectDice effect, FightActor caster, SpellCastHandler castHandler, Cell targetedCell, bool critical) : base(effect, caster, castHandler, targetedCell, critical)
        { }

        protected override bool InternalApply()
        {
            try
            {
                var spellEffect = new Spell(Dice.DiceNum, (byte)Dice.DiceFace);

                #region >> Ignore Spell

                if (spellEffect.Template != null)
                {
                    //Ignore - To avoid duplicating the effect
                    if (spellEffect.Template.Id == (int)SpellIdEnum.TORPOR_14433) //Feca 2.61.10.19
                        return false;

                    //Ignore - To avoid duplicating the effect
                    if (spellEffect.Template.Id == (int)SpellIdEnum.IMPARTIAL_WORD_13208) //Eni 2.61.10.19
                        return false;
                }

                #endregion

                if (spellEffect.Id == 4840)
                {
                    this.Caster.CastAutoSpell(spellEffect, this.Caster.Cell);
                    return true;
                }

                foreach (var affectedActor in GetAffectedActors())
                {
                    if (Dice.Duration != 0 || Dice.Delay != 0)
                    {
                        if (Dice.DiceNum == 8495)
                        {
                            var spell = new Spell(Dice.DiceNum, (byte)Dice.DiceFace);
                            Caster.CastAutoSpell(spell, affectedActor.Cell);
                        }
                        else
                        {
                            var buffId = affectedActor.PopNextBuffId();
                            var spell = new Spell(Dice.DiceNum, (byte)Dice.DiceFace);
                            var buff = new TriggerBuff(buffId, affectedActor, Caster, this, spell, Spell, false, FightDispellableEnum.DISPELLABLE_BY_DEATH, Priority, DefaultBuffTrigger);

                            affectedActor.AddBuff(buff);
                        }
                    }
                    else
                    {
                        var spell = new Spell(Dice.DiceNum, (byte)Dice.DiceFace);

                        //Huppermage Tribute Spell correction for version 2.61.10.19 by Kenshin
                        if (spell.Id == (int)SpellIdEnum.TRIBUTE_14341 || spell.Id == (int)SpellIdEnum.TRIBUTE_14354 && Effect.EffectId == EffectsEnum.Effect_CastSpell_1160)
                        {
                            #region >> Functions Spell

                            if (affectedActor.HasState(290) && Dice.DiceFace == 2) //Fire
                            {
                                Caster.CastAutoSpell(spell, affectedActor.Cell);
                            }
                            else if (affectedActor.HasState(291) && Dice.DiceFace == 1) //Water
                            {
                                Caster.CastAutoSpell(spell, affectedActor.Cell);
                            }
                            else if (affectedActor.HasState(292) && Dice.DiceFace == 4) //Str
                            {
                                Caster.CastAutoSpell(spell, affectedActor.Cell);
                            }
                            else if (affectedActor.HasState(293) && Dice.DiceFace == 3) //Air
                            {
                                Caster.CastAutoSpell(spell, affectedActor.Cell);
                            }
                            else
                            {
                                return false;
                            }

                            #endregion
                        }
                        else if (Effect.EffectId == EffectsEnum.Effect_CastSpell_1160 || Effect.EffectId == EffectsEnum.Effect_CastSpell_2160)
                        {
                            if (Spell.Id == (int)SpellIdEnum.ABYSSAL_DOFUS_6828)
                            {
                                var ignored = new[]
                                    {
                                    SpellCastResult.CANNOT_PLAY,
                                    SpellCastResult.CELL_NOT_FREE,
                                    SpellCastResult.HAS_NOT_SPELL,
                                    SpellCastResult.HISTORY_ERROR,
                                    SpellCastResult.NOT_ENOUGH_AP,
                                    SpellCastResult.NOT_IN_ZONE,
                                    SpellCastResult.STATE_FORBIDDEN,
                                    SpellCastResult.STATE_REQUIRED,
                                    SpellCastResult.UNWALKABLE_CELL
                                };

                                Caster.CastSpell(new SpellCastInformations(Caster, spell, affectedActor.Cell)
                                {
                                    Silent = true,
                                    ApFree = true,
                                    BypassedConditions = ignored
                                });
                            }
                            else
                            {
                                Caster.CastAutoSpell(spell, affectedActor.Cell);
                            }
                        }
                        else if (Effect.EffectId == EffectsEnum.Effect_CastSpell_1017)
                        {
                            affectedActor.CastAutoSpell(spell, Caster.Cell);
                        }
                        else if (Effect.EffectId == EffectsEnum.Effect_2794)
                        {
                            affectedActor.CastAutoSpell(spell, TargetedCell);
                        }
                        else
                        {
                            affectedActor.CastAutoSpell(spell, affectedActor.Cell);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Caster: {this.Caster} Spell: {this.Spell.Template.Id} Error: {ex} ");
                return false;
            }

            return true;
        }

        void DefaultBuffTrigger(TriggerBuff buff, FightActor triggerrer, BuffTriggerType trigger, object token)
        {
            var damages = token as Fights.Damage;

            if (damages != null && damages.Spell != null && damages.Spell.Id == buff.Spell.Id)
                return;

            if (Effect.EffectId == EffectsEnum.Effect_CastSpell_1160)
            {
                if (buff.Spell.Id == 9917)
                {
                    buff.Target.CastSpell(new SpellCastInformations(buff.Target, buff.Spell, buff.Target.Cell)
                    {
                        Force = true,
                        Silent = true,
                        ApFree = true,
                        TriggerEffect = this,
                        Triggerer = triggerrer
                    });
                }
                else
                {
                    buff.Caster.CastSpell(new SpellCastInformations(buff.Caster, buff.Spell, buff.Target.Cell)
                    {
                        Force = true,
                        Silent = true,
                        ApFree = true,
                        TriggerEffect = this,
                        Triggerer = triggerrer
                    });
                }
            }
            else if (Effect.EffectId == EffectsEnum.Effect_CastSpell_1017 || buff.Spell.Id == (int)SpellIdEnum.FRIKT_3356)
            {
                buff.Target.CastSpell(new SpellCastInformations(buff.Target, buff.Spell, triggerrer.Cell)
                {
                    Force = true,
                    Silent = true,
                    ApFree = true,
                    TriggerEffect = this,
                    Triggerer = triggerrer
                });
            }
            else if (Effect.EffectId == EffectsEnum.Effect_1018 || Effect.EffectId == EffectsEnum.Effect_1019)
            {
                buff.Target.CastSpell(new SpellCastInformations(triggerrer, buff.Spell, buff.Target.Cell)
                {
                    Force = true,
                    Silent = true,
                    ApFree = true,
                    TriggerEffect = this,
                    Triggerer = triggerrer
                });
            }
            else if (Effect.EffectId == EffectsEnum.Effect_2794 && _spellsBoombs.Contains(Spell.Id))
            {
                var cellToCast = buff.Target.Fight.GetTriggers().Where(x => x is Rune && x.Caster == buff.Target && x.OriginEffect.SpellId == Spell.Id).FirstOrDefault().CenterCell;

                if (cellToCast != null)
                {
                    buff.Target.CastSpell(new SpellCastInformations(buff.Target, buff.Spell, cellToCast)
                    {
                        Force = true,
                        Silent = true,
                        ApFree = true,
                        TriggerEffect = this,
                        Triggerer = triggerrer
                    });
                }
                else
                {
                    return;
                }
            }
            else
            {
                if (buff.Spell.Id == (int)SpellIdEnum.BEARBARKENTINE_2514)
                {
                    buff.Target.CastSpell(new SpellCastInformations(buff.Target, buff.Spell, buff.Caster.Cell)
                    {
                        Force = true,
                        Silent = true,
                        ApFree = true,
                        TriggerEffect = this,
                        Triggerer = triggerrer
                    });
                }
                else
                {
                    buff.Target.CastSpell(new SpellCastInformations(buff.Target, buff.Spell, buff.Target.Cell)
                    {
                        Force = true,
                        Silent = true,
                        ApFree = true,
                        TriggerEffect = this,
                        Triggerer = triggerrer
                    });
                }
            }
        }
    }
}