using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Effects;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Spells.Casts;
using System.Linq;

namespace Stump.Server.WorldServer.game.spells.Casts.Sadida
{
    [SpellCastHandler(SpellIdEnum.JOURNEY_13683)]
    public class JourneyHandler : DefaultSpellCastHandler
    {
        public JourneyHandler(SpellCastInformations cast) : base(cast)
        { }

        public override void Execute()
        {
            if (!base.Initialize())
                Initialize();

            var allFighters = Fight.GetAllFighters(entry => !entry.IsDead() && entry.Team != Caster.Team).OrderBy(entry => entry.Position.Point.ManhattanDistanceTo(TargetedCell)).ToList();

            foreach (var fighter in allFighters)
            {
                //Dano AR
                if (Handlers[0].Effect.EffectId == EffectsEnum.Effect_DamageAir && fighter.HasState(293))
                {
                    var damage = new Damage(Handlers[0].Dice, EffectSchoolEnum.Water, Caster, Spell, TargetedCell, Handlers[0].EffectZone)
                    {
                        MarkTrigger = MarkTrigger,
                        IsCritical = Critical
                    };

                    damage.GenerateDamages();
                    fighter.InflictDamage(damage);
                }

                //Dano de Água
                if (Handlers[1].Effect.EffectId == EffectsEnum.Effect_DamageWater && fighter.HasState(291))
                {
                    var damage = new Damage(Handlers[1].Dice, EffectSchoolEnum.Water, Caster, Spell, TargetedCell, Handlers[1].EffectZone)
                    {
                        MarkTrigger = MarkTrigger,
                        IsCritical = Critical
                    };

                    damage.GenerateDamages();
                    fighter.InflictDamage(damage);
                }

                //Dano de Fogo
                if (Handlers[2].Effect.EffectId == EffectsEnum.Effect_DamageFire && fighter.HasState(290))
                {
                    var damage = new Damage(Handlers[2].Dice, EffectSchoolEnum.Water, Caster, Spell, TargetedCell, Handlers[2].EffectZone)
                    {
                        MarkTrigger = MarkTrigger,
                        IsCritical = Critical
                    };

                    damage.GenerateDamages();
                    fighter.InflictDamage(damage);
                }

                //Dano de Terra
                if (Handlers[3].Effect.EffectId == EffectsEnum.Effect_DamageEarth && fighter.HasState(292))
                {
                    var damage = new Damage(Handlers[3].Dice, EffectSchoolEnum.Water, Caster, Spell, TargetedCell, Handlers[3].EffectZone)
                    {
                        MarkTrigger = MarkTrigger,
                        IsCritical = Critical
                    };

                    damage.GenerateDamages();
                    fighter.InflictDamage(damage);
                }
            }

            Handlers[4].Apply();
        }
    }
}