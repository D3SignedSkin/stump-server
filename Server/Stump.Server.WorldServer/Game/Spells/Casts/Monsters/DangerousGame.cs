using Stump.Core.Reflection;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Effects;
using Stump.Server.WorldServer.Game.Effects.Handlers.Spells;
using Stump.Server.WorldServer.Game.Fights;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Stump.Server.WorldServer.Game.Spells.Casts.Ecaflip
{
    [SpellCastHandler(SpellIdEnum.DANGEROUS_GAME_8512)]
    public class DangerousGame : DefaultSpellCastHandler
    {
        public DangerousGame(SpellCastInformations cast) : base(cast)
        { }

        public override bool Initialize()
        {
            var listOne = Critical ? SpellLevel.CriticalEffects : SpellLevel.Effects;
            var listTwo = new List<SpellEffectHandler>();

            foreach (var current in listOne)
            {
                var spellEffectHandler = Singleton<EffectManager>.Instance.GetSpellEffectHandler(current, Caster, this, TargetedCell, Critical);

                if (MarkTrigger != null)
                    spellEffectHandler.MarkTrigger = MarkTrigger;

                listTwo.Add(spellEffectHandler);
            }

            Handlers = listTwo.ToArray();
            return true;
        }

        public override void Execute()
        {
            if (!m_initialized)
                Initialize();

            foreach (var handler in Handlers)
            {
                handler.Apply();
            }
        }
    }
}