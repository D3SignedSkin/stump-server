using Stump.Core.Reflection;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Effects.Handlers.Spells;
using Stump.Server.WorldServer.Game.Effects;
using Stump.Server.WorldServer.Game.Fights;
using System.Collections.Generic;
using System.Linq;

namespace Stump.Server.WorldServer.Game.Spells.Casts.Osamodas
{
    [SpellCastHandler(SpellIdEnum.BAMBOO_SHERPA_12831)]
    public class Bamboo : DefaultSpellCastHandler
    {
        public Bamboo(SpellCastInformations cast) : base(cast)
        { }

        public override bool Initialize()
        {
            var listSpellsEffects = Critical ? SpellLevel.CriticalEffects : SpellLevel.Effects;
            var listSpells = new List<SpellEffectHandler>();

            foreach (var current in listSpellsEffects)
            {
                var spellEffectHandler = Singleton<EffectManager>.Instance.GetSpellEffectHandler(current, Caster, this, TargetedCell, Critical);

                if (MarkTrigger != null)
                    spellEffectHandler.MarkTrigger = MarkTrigger;

                listSpells.Add(spellEffectHandler);
            }

            Handlers = listSpells.ToArray();
            return true;
        }

        public override void Execute()
        {
            if (!m_initialized)
                Initialize();

            var handlersList = Handlers.OrderBy(x => x.Priority);

            foreach (var handler in handlersList)
            {
                //if (handler.Effect.Id == (int)EffectsEnum.Effect_ChangeAppearance_335)
                //    handler.AddAffectedActor(Caster);

                handler.Apply();
            }
        }
    }
}