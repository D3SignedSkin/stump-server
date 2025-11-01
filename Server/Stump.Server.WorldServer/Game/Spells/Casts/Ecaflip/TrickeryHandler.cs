using Stump.Core.Reflection;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Effects.Handlers.Spells;
using Stump.Server.WorldServer.Game.Effects;
using Stump.Server.WorldServer.Game.Fights;
using System.Collections.Generic;
using System;
using System.Linq;
using Stump.Server.WorldServer.Game.Effects.Instances;

namespace Stump.Server.WorldServer.Game.Spells.Casts.Ecaflip
{
    [SpellCastHandler(SpellIdEnum.TRICKERY_12881)]
    public class TrickeryCastHandler : DefaultSpellCastHandler //By Kenshin Version 2.61.10
    {
        public TrickeryCastHandler(SpellCastInformations cast) : base(cast)
        { }

        private EffectsEnum randomEffect = new EffectsEnum();

        private List<Tuple<EffectsEnum, short>> randomEffectList = new List<Tuple<EffectsEnum, short>>()
        {
            new Tuple<EffectsEnum, short>(EffectsEnum.Effect_DamageEarth, 10),
            new Tuple<EffectsEnum, short>(EffectsEnum.Effect_DamageAir, 8),
            new Tuple<EffectsEnum, short>(EffectsEnum.Effect_DamageWater, 6),
            new Tuple<EffectsEnum, short>(EffectsEnum.Effect_DamageFire, 4),
        };

        public override bool Initialize()
        {
            Random random = new Random();
            int randomIndex = random.Next(randomEffectList.Count);

            randomEffect = randomEffectList[randomIndex].Item1;

            Spell.ApCostReduction = (uint)(randomEffect == EffectsEnum.Effect_DamageAir ? 1 : randomEffect == EffectsEnum.Effect_DamageWater ? 2 : randomEffect == EffectsEnum.Effect_DamageFire ? 3 : 0);

            var list = Critical ? SpellLevel.CriticalEffects : SpellLevel.Effects;
            var list2 = new List<SpellEffectHandler>();

            foreach (var current in list)
            {
                var spellEffectHandler = Singleton<EffectManager>.Instance.GetSpellEffectHandler(current, Caster, this, TargetedCell, Critical);

                if (MarkTrigger != null)
                    spellEffectHandler.MarkTrigger = MarkTrigger;

                list2.Add(spellEffectHandler);
            }

            Handlers = list2.ToArray();
            return true;
        }

        public override void Execute()
        {
            if (!m_initialized)
                Initialize();

            var handlersList = Handlers.OrderBy(x => x.Priority);

            foreach (var handler in handlersList)
            {
                if (randomEffectList.Any(tuple => tuple.Item1 == handler.Effect.EffectId) && handler.Effect.EffectId != randomEffect)
                    continue;

                if (handler.Effect.EffectId == EffectsEnum.Effect_AddCriticalHit && randomEffectList.FirstOrDefault(x => x.Item1 == randomEffect).Item2 != (handler.Effect as EffectDice).DiceNum)
                    continue;

                handler.Apply();
            }
        }
    }
}