using System;
using Stump.Core.Mathematics;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Effects;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Fights.Buffs;
using Stump.Server.WorldServer.Game.Fights.Triggers;
using Stump.Server.WorldServer.Game.Spells;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Maps.Cells.Shapes;
using Stump.DofusProtocol.Enums;

namespace Stump.Server.WorldServer.Game.Fights
{
    public class Damage
    {
        private int m_amount;

        public Damage(int amount)
        {
            Amount = amount;
        }

        public Damage(EffectDice effect)
        {
            BaseMaxDamages = Math.Max(effect.DiceFace, effect.DiceNum);
            BaseMinDamages = Math.Min(effect.DiceFace, effect.DiceNum);

            if (BaseMinDamages == 0)
                BaseMinDamages = BaseMaxDamages;
        }

        public Damage(EffectDice effect, EffectSchoolEnum school, FightActor source, Spell spell, Cell targetCell, Zone zone = null) : this(effect)
        {
            School = school;
            Source = source;
            Spell = spell;
            TargetCell = targetCell;
            Zone = zone;
            ElementId = effect.EffectId == EffectsEnum.Effect_Damage ? GetActorBetterStatus(source) : effect.EffectElement;
        }

        public EffectSchoolEnum School
        {
            get;
            set;
        }

        public int ElementId
        {
            get;
            set;
        }

        public FightActor Source
        {
            get;
            set;
        }

        public MarkTrigger MarkTrigger
        {
            get;
            set;
        }

        public Buff Buff
        {
            get;
            set;
        }

        public int BaseMinDamages
        {
            get;
            set;
        }

        public int BaseMaxDamages
        {
            get;
            set;
        }

        public bool IsCritical
        {
            get;
            set;
        }

        public int Amount
        {
            get { return m_amount; }
            set
            {
                if (value < 0)
                    value = 0;
                m_amount = value;
                Generated = true;
            }
        }

        public bool Generated
        {
            get;
            set;
        }

        public Spell Spell
        {
            get;
            set;
        }

        public bool IgnoreDamageReduction
        {
            get;
            set;
        }

        public bool IgnoreDamageBoost
        {
            get;
            set;
        }

        public EffectGenerationType EffectGenerationType
        {
            get;
            set;
        }

        public bool ReflectedDamages
        {
            get;
            set;
        }

        public Cell TargetCell
        {
            get;
            set;
        }

        public Zone Zone
        {
            get;
            set;
        }

        public bool IsWeaponAttack => Spell == null || Spell.Id == 0;

        public void GenerateDamages()
        {
            if (Generated)
                return;

            switch (EffectGenerationType)
            {
                case EffectGenerationType.MaxEffects:
                    Amount = BaseMaxDamages;
                    break;
                case EffectGenerationType.MinEffects:
                    Amount = BaseMinDamages;
                    break;
                default:
                    {
                        var rand = new CryptoRandom();

                        Amount = rand.Next(BaseMinDamages, BaseMaxDamages + 1);
                    }
                    break;
            }
        }

        static int GetActorBetterStatus(FightActor actor)
        {
            int airTotal = actor.Stats.Agility.Total;
            int waterTotal = actor.Stats.Chance.Total;
            int earthTotal = actor.Stats.Strength.Total;
            int fireTotal = actor.Stats.Intelligence.Total;

            int maxValue = Math.Max(airTotal, Math.Max(waterTotal, Math.Max(earthTotal, fireTotal)));

            if (maxValue == airTotal)
            {
                return 4;
            }
            else if (maxValue == waterTotal)
            {
                return 3;
            }
            else if (maxValue == earthTotal)
            {
                return 1;
            }
            else if (maxValue == fireTotal)
            {
                return 2;
            }
            else
            {
                return 0;
            }
        }
    }
}