using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Fights.Triggers;
using Stump.Server.WorldServer.Game.Spells;
using Stump.Server.WorldServer.Game.Spells.Casts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Stump.Server.WorldServer.Game.Effects.Handlers.Spells.Others
{
    [EffectHandler(EffectsEnum.Effect_TriggerGlyphs)]
    public class TriggerGlyphs : SpellEffectHandler
    {
        public TriggerGlyphs(EffectDice effect, FightActor caster, SpellCastHandler castHandler, Cell targetedCell, bool critical) : base(effect, caster, castHandler, targetedCell, critical)
        { }

        protected override bool InternalApply()
        {
            var fight = Caster.Fight;
            var triggers = fight.GetTriggersByCell(Caster.Cell);

            if (Spell.Template.Id == (int)SpellIdEnum.TELEGLYPH_12986) //Spell Feca 2.61.10.19
            {
                foreach (var trigger in triggers.OfType<Glyph>().Where(x => x.CanBeForced && x.Caster == Caster && Dice.Value == x.CastedSpell.Id))
                {
                    using (fight.StartSequence(SequenceTypeEnum.SEQUENCE_GLYPH_TRAP))
                    {
                        trigger.TriggerAllCells(Caster);
                    }
                }
            }
            else
            {
                foreach (var trigger in triggers.OfType<Glyph>().Where(x => x.CanBeForced && x.Caster == Caster))
                {
                    using (fight.StartSequence(SequenceTypeEnum.SEQUENCE_GLYPH_TRAP))
                    {
                        trigger.TriggerAllCells(Caster);
                    }
                }
            }

            return true;
        }
    }
}