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
    [SpellCastHandler(SpellIdEnum.ROULETTE_12900)]
    public class RouletteHandler : DefaultSpellCastHandler //By Kenshin Version 2.61.10
    {
        public RouletteHandler(SpellCastInformations cast) : base(cast)
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

            if (Handlers.Count() == 1)
            {
                Handlers[0].Apply();
            }
            else
            {
                Random random = new Random();

                var handlersList = Handlers.ToList();
                var randomHandler = handlersList.OrderBy(x => random.Next()).FirstOrDefault();

                randomHandler.Apply();
            }
        }
    }
}