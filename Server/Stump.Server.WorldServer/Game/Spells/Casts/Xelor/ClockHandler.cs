using Stump.Core.Reflection;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Effects.Handlers.Spells;
using Stump.Server.WorldServer.Game.Effects;
using Stump.Server.WorldServer.Game.Fights;
using System.Collections.Generic;
using System.Linq;
using Stump.Server.WorldServer.Game.Fights.Buffs;
using Stump.Server.WorldServer.Game.Effects.Instances;

namespace Stump.Server.WorldServer.Game.Spells.Casts.Ecaflip
{
    [SpellCastHandler(SpellIdEnum.CLOCK_13256)]
    public class ClockHandler : DefaultSpellCastHandler //By Kenshin Version 2.61.10
    {
        public ClockHandler(SpellCastInformations cast) : base(cast)
        { }

        public override bool Initialize()
        {
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
                if (handler.Effect.EffectId == EffectsEnum.Effect_AddAP_111) //TODO
                    continue;

                if (handler.Effect.EffectId == EffectsEnum.Effect_CastSpell_1160 && handler.Dice.DiceNum == 19721) //TODO
                    continue;

                handler.Apply();
            }
        }
    }
}