using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Types;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Spells;
using System;
using Stump.Server.WorldServer.Game.Effects.Handlers.Spells;
using System.Collections.Generic;
using NLog;

namespace Stump.Server.WorldServer.Game.Fights.Buffs
{
    public class SpellBuff : Buff
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public SpellBuff(int id, FightActor target, FightActor caster, SpellEffectHandler effect, Spell spell, Spell boostedSpell, short boost, bool critical, FightDispellableEnum dispelable, List<CharacterSpell> spellos = null) : base(id, target, caster, effect, spell, critical, dispelable)
        {
            BoostedSpell = boostedSpell;
            Boost = boost;
            Spellos = spellos;
        }

        public Spell BoostedSpell
        {
            get;
        }

        public List<CharacterSpell> Spellos
        {
            get;
        }

        public short Boost
        {
            get;
        }

        List<int> SpellsIgnore = new List<int> { 4973, 4571, 4269 };

        public override void Apply()
        {
            base.Apply();

            try
            {
                var character = Caster.Owner;

                if (character == null)
                    return;

                if (SpellsIgnore.Contains(BoostedSpell.Id))
                    return;

                //if (Effect.EffectId == EffectsEnum.Effect_294)
                //{
                //    character.SpellRangeHandler((short)BoostedSpell.Id, (short)-Boost);
                //}
                //else if (Effect.EffectId == EffectsEnum.Effect_SpellRangeIncrease)
                //{
                //    character.SpellRangeHandler((short)BoostedSpell.Id, Boost);
                //}
                //else if (Effect.EffectId == EffectsEnum.Effect_SpellObstaclesDisable)
                //{
                //    if (Spellos == null)
                //    {
                //        character.SpellObstaclesDisable((short)BoostedSpell.Id);
                //    }
                //    else
                //    {
                //        Spellos.ForEach(x => character.SpellObstaclesDisable(x));
                //    }
                //}
                //else if (Effect.EffectId == EffectsEnum.Effect_ApCostReduce)
                //{
                //    character.ReduceSpellCost((short)BoostedSpell.Id, (uint)Boost);
                //}
                //else
                //{
                //    character.SpellAddDamage((short)BoostedSpell.Id, (uint)Boost);
                //    Target.BuffSpell(BoostedSpell, Boost);
                //}

                #region MongoDB Logs Staff
                //var document = new BsonDocument
                //    {
                //        { "EffectName", Effect.EffectId },
                //        { "EffectDescri", Effect.Template.Description },
                //        { "CasterName", Caster.Owner.Name },
                //        { "SpellId", BoostedSpell.Id },
                //        { "SpellName", BoostedSpell.Template.Name },
                //        { "Function", "Apply" },
                //        { "Date", DateTime.Now.ToString(CultureInfo.CreateSpecificCulture("pt-BR")) }
                //    };
                //MongoLogger.Instance.Insert("World_SpellBuff", document);
                #endregion
            }
            catch
            {
                logger.Error("Effect: " + Effect.EffectId + "Caster:" + Caster + " Caster inf:" + Caster.GetGameFightFighterInformations().ToString() + " Spell id:" + BoostedSpell.Id);
            }
        }

        public override void Dispell()
        {
            base.Dispell();

            try
            {
                var character = Caster.Owner;

                if (character == null)
                    return;

                if (SpellsIgnore.Contains(BoostedSpell.Id))
                    return;

                //if (Effect.EffectId == EffectsEnum.Effect_294)
                //{
                //    character.IncreaseRangeDisable((short)BoostedSpell.Id, (short)-Boost);
                //}
                //else if (Effect.EffectId == EffectsEnum.Effect_SpellRangeIncrease)
                //{
                //    character.IncreaseRangeDisable((short)BoostedSpell.Id, Boost);
                //}
                //else if (Effect.EffectId == EffectsEnum.Effect_SpellObstaclesDisable)
                //{
                //    if (Spellos == null)
                //    {
                //        character.SpellObstaclesEnable(BoostedSpell);
                //    }
                //    else
                //    {
                //        Spellos.ForEach(x => character.SpellObstaclesEnable(x));
                //    }
                //}
                //else if (Effect.EffectId == EffectsEnum.Effect_ApCostReduce)
                //{
                //    character.SpellCostDisable((short)BoostedSpell.Id, Boost);
                //}
                //else
                //{
                //    Target.UnBuffSpell(BoostedSpell, Boost);
                //    character.SpellAddDamageDisable((short)BoostedSpell.Id);
                //}

                #region MongoDB Logs Staff
                //var document = new BsonDocument
                //    {
                //        { "EffectName", Effect.EffectId },
                //        { "EffectDescri", Effect.Template.Description },
                //        { "CasterName", Caster.Owner.Name },
                //        { "SpellId", BoostedSpell.Id },
                //        { "SpellName", BoostedSpell.Template.Name },
                //        { "Function", "Apply" },
                //        { "Date", DateTime.Now.ToString(CultureInfo.CreateSpecificCulture("pt-BR")) }
                //    };
                //MongoLogger.Instance.Insert("World_SpellBuff", document);
                #endregion
            }
            catch
            {
                logger.Error("Effect: " + Effect.EffectId + "Caster:" + Caster + " Caster inf:" + Caster.GetGameFightFighterInformations().ToString() + " Spell id:" + BoostedSpell.Id);
            }
        }

        public override AbstractFightDispellableEffect GetAbstractFightDispellableEffect()
        {
            if (Delay == 0)
            {
                if (Effect.EffectId == EffectsEnum.Effect_294 || Effect.EffectId == EffectsEnum.Effect_SpellRangeIncrease || Effect.EffectId == EffectsEnum.Effect_SpellObstaclesDisable || Effect.EffectId == EffectsEnum.Effect_ApCostReduce) return new AbstractFightDispellableEffect();
                var x = new FightTemporarySpellBoostEffect((ushort)Id, Target.Id, Duration, (sbyte)Dispellable, (ushort)Spell.Id, (uint)Effect.Id, 0, Boost, (ushort)BoostedSpell.Id);
                return x;
            }

            var values = Effect.GetValues();

            return new FightTriggeredEffect((uint)Id, Target.Id, Delay,
                (sbyte)Dispellable,
                (ushort)Spell.Id, (uint)(EffectFix?.ClientEffectId ?? Effect.Id), 0,
                (values.Length > 0 ? Convert.ToInt32(values[0]) : 0),
                (values.Length > 1 ? Convert.ToInt32(values[1]) : 0),
                (values.Length > 2 ? Convert.ToInt32(values[2]) : 0),
                Delay);
        }
    }
}