using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Spells;

namespace Stump.Server.WorldServer.AI.Fights.Brain.Custom.Boss
{
    [BrainIdentifier((int)MonsterIdEnum.TENGU_GIVREFOUX_2967)]
    public class GivrefouxBrain : Brain
    {
        public GivrefouxBrain(AIFighter fighter) : base(fighter)
        {
            fighter.Fight.FightStarted += Fight_FightStarted;
            fighter.Fight.TurnStarted += OnTurnStarted;
        }

        private void OnTurnStarted(IFight obj, FightActor actor)
        {
            if (Fighter.HasState((int)SpellIdEnum.FURIBUND_668) && Fighter.HasState((int)SpellIdEnum.LIGHTNING_669))
            {
                Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.FROZEN_FLEECE_2684, 1), Fighter.Cell);

                //Fighter.RemoveSpellBuffs((int)SpellIdEnum.CROQUETTE);
                //actor.CastAutoSpell(new Spell((int)SpellIdEnum.FULGURATION,1), actor.Cell);
                //Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.LIGHTNING_FIST_5367, 1), Fighter.Cell);
            }
        }

        private void Fight_FightStarted(IFight obj)
        {
            //Fighter.Stats[PlayerFields.SummonLimit].Additional = 9999;
            //Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.CROQUETTE, 1), Fighter.Cell);
        }
    }

    [BrainIdentifier((int)MonsterIdEnum.FUJI_GIVREFOUX_NOURRICIRE_3234)]
    public class Fuji : Brain
    {
        public Fuji(AIFighter fighter) : base(fighter)
        {
            fighter.Fight.FightStarted += Fight_FightStarted;
        }

        private void Fight_FightStarted(IFight obj)
        {
            //Fighter.Stats[PlayerFields.SummonLimit].Additional = 9999;
            Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.MATERNAL_INSTINCT_2676, 1), Fighter.Cell);
        }
    }

    [BrainIdentifier((int)MonsterIdEnum.YOKA_GIVREFOUX_2888)]
    public class Yokai : Brain
    {
        public Yokai(AIFighter fighter) : base(fighter)
        {
            fighter.Fight.FightStarted += Fight_FightStarted;
            fighter.Fight.TurnStarted += OnTurnStarted;
        }

        private void OnTurnStarted(IFight obj, FightActor actor)
        {
            if (Fighter == actor)
            {
                if (!Fighter.HasState((int)SpellIdEnum.FURIBUND_668))
                {
                    Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.FURIBUND_668, 1), Fighter.Cell);
                }
            }
        }

        private void Fight_FightStarted(IFight obj)
        {
            //Fighter.Stats[PlayerFields.SummonLimit].Additional = 9999;
            Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.FURIBUND_668, 1), Fighter.Cell);
        }
    }

    [BrainIdentifier((int)MonsterIdEnum.YOMI_GIVREFOUX_2891)]
    public class YomiGivrefouxBrain : Brain
    {
        public YomiGivrefouxBrain(AIFighter fighter) : base(fighter)
        {
            fighter.BeforeDamageInflicted += OnDamageInflicted;
            fighter.Fight.TurnStarted += OnTurnStarted;
        }

        private void OnTurnStarted(IFight obj, FightActor actor)
        {
            if (Fighter == actor)
            {
                if (!Fighter.HasState((int)SpellIdEnum.LIGHTNING_669))
                {
                    Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.LIGHTNING_669, 1), Fighter.Cell);
                }
            }
        }

        private void OnDamageInflicted(FightActor fighter, Damage dmg)
        {
            if (fighter != Fighter)
                return;

            if (dmg.IsWeaponAttack && dmg.Zone.Radius == 1)
                Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.LIGHTNING_669, 1), Fighter.Cell);
        }
    }
}