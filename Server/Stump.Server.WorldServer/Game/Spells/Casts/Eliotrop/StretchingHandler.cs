using Stump.Core.Reflection;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Effects.Handlers.Spells;
using Stump.Server.WorldServer.Game.Effects;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Spells.Casts;
using System.Collections.Generic;
using System.Linq;

namespace Stump.Server.WorldServer.game.spells.Casts.Sadida
{
    [SpellCastHandler(SpellIdEnum.STRETCHING_14580)]
    public class StretchingHandler : DefaultSpellCastHandler
    {
        public StretchingHandler(SpellCastInformations cast) : base(cast)
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
                handler.Apply();
            }
        }
    }
}