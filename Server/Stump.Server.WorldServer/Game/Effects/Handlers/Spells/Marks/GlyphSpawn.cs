using System.Drawing;
using NLog;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Fights.Triggers;
using Spell = Stump.Server.WorldServer.Game.Spells.Spell;
using Stump.Server.WorldServer.Game.Spells.Casts;

namespace Stump.Server.WorldServer.Game.Effects.Handlers.Spells.Marks
{
    [EffectHandler(EffectsEnum.Effect_Glyph)]
    [EffectHandler(EffectsEnum.Effect_Glyph_402)]
    [EffectHandler(EffectsEnum.Effect_GlyphAura)]
    [EffectHandler(EffectsEnum.Effect_1165)]
    public class GlyphSpawn : SpellEffectHandler
    {
        static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public GlyphSpawn(EffectDice effect, FightActor caster, SpellCastHandler castHandler, Cell targetedCell, bool critical) : base(effect, caster, castHandler, targetedCell, critical)
        { }

        protected override bool InternalApply()
        {
            var glyphSpell = new Spell(Dice.DiceNum, (byte)Dice.DiceFace);

            if (glyphSpell.Template == null || !glyphSpell.ByLevel.ContainsKey(Dice.DiceFace))
            {
                logger.Error("Cannot find glyph spell id = {0}, level = {1}. Casted Spell = {2}", Dice.DiceNum, Dice.DiceFace, Spell.Id);
                return false;
            }

            var spell = Spell;

            if (spell.Id == (int)SpellIdEnum.LOAD_HEADICE_2699)
                spell = glyphSpell;

            Glyph glyph;

            if (Effect.EffectId == EffectsEnum.Effect_GlyphAura)
            {
                glyph = new GlyphAura((short)Fight.PopNextTriggerId(), Caster, spell, Dice, glyphSpell, TargetedCell,
                                EffectZone.ShapeType, (byte)Effect.ZoneMinSize, (byte)Effect.ZoneSize, GetGlyphColorBySpell(Spell));
            }
            else if (Effect.EffectId == EffectsEnum.Effect_1165)
            {
                glyph = new GlyphBuff((short)Fight.PopNextTriggerId(), Caster, spell, Dice, glyphSpell, TargetedCell,
                                EffectZone.ShapeType, (byte)Effect.ZoneMinSize, (byte)Effect.ZoneSize, GetGlyphColorBySpell(Spell));
            }
            else
            {
                glyph = new Glyph((short)Fight.PopNextTriggerId(), Caster, spell, Dice, glyphSpell, TargetedCell,
                            EffectZone.ShapeType, (byte)Effect.ZoneMinSize, (byte)Effect.ZoneSize,
                            GetGlyphColorBySpell(Spell), Effect.EffectId == EffectsEnum.Effect_Glyph,
                            Effect.EffectId == EffectsEnum.Effect_Glyph_402 ? TriggerType.OnTurnEnd : TriggerType.OnTurnBegin);
            }

            Fight.AddTriger(glyph);

            return true;
        }

        static Color GetGlyphColorBySpell(Spell spell)
        {
            switch (spell.Id)
            {
                //Create by Kenshin - 2.61
                //Neutre

                //STR
                case (int)SpellIdEnum.BLINDING_GLYPH_12987:
                case (int)SpellIdEnum.PROTECTIVE_GLYPH_13021:
                case (int)SpellIdEnum.EARTH_TOTEM_6165:
                    return Color.FromArgb(166, 91, 42);

                //INT
                case (int)SpellIdEnum.BURNING_GLYPH_12985:
                case (int)SpellIdEnum.PERCEPTION_GLYPH_13025:
                case (int)SpellIdEnum.FIRE_TOTEM_6163:
                    return Color.FromArgb(230, 36, 8);

                //AGI
                case (int)SpellIdEnum.AGGRESSIVE_GLYPH_12992:
                case (int)SpellIdEnum.FULMINATING_GLYPH_13013:
                case (int)SpellIdEnum.AIR_TOTEM_6166:
                    return Color.FromArgb(53, 200, 120);

                //Chance
                case (int)SpellIdEnum.ROAMING_GLYPH_13023:
                case (int)SpellIdEnum.PARALYSING_GLYPH_12990:
                case (int)SpellIdEnum.WATER_TOTEM_6167:
                    return Color.FromArgb(4, 117, 142);


                //Other
                case (int)SpellIdEnum.BARRIER_13019:
                case (int)SpellIdEnum.REPULSION_GLYPH_12988:
                    return Color.FromArgb(49, 45, 134);

                case (int)SpellIdEnum.EXCURSION_GLYPH_13024:
                case (int)SpellIdEnum.GRAVITATIONAL_GLYPH_12991:
                    return Color.FromArgb(238, 223, 105);


                //Verification
                case (int)SpellIdEnum.PARALYSING_GLYPH_7542:
                    return Color.FromArgb(202, 19, 48);
                case (int)SpellIdEnum.GLYPH_OF_BLINDNESS_7544:
                    return Color.FromArgb(166, 91, 42);
                case (int)SpellIdEnum.LOAD_HEADICE_2699:
                case (int)SpellIdEnum.CAWWOT_367:
                    return Color.White;
                default:
                    return Color.Red;
            }
        }
    }
}