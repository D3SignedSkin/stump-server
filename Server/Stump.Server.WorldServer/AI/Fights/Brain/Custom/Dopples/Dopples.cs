using System;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.AI.Fights.Actions;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Spells;
using TreeSharp;

namespace Stump.Server.WorldServer.AI.Fights.Brain.Custom.Boss
{
    [BrainIdentifier((int)MonsterIdEnum.DOPEUL_SACRIEUR_455)]
    public class DopeulSacro : Brain
    {
        public DopeulSacro(AIFighter fighter) : base(fighter)
        {
            fighter.Fight.FightStarted += Fight_FightStarted;
        }

        private void Fight_FightStarted(IFight obj)
        { }

        public override void Play()
        {
            foreach (var spell in Fighter.Spells.Values)
            {
                var target = Environment.GetNearestEnemy();
                var selector = new PrioritySelector();

                if (spell.Id == (int)SpellIdEnum.DOPPLESQUE_BLOODTHIRSTY_PUNISHMENT_8142)
                {
                    Fighter.CastSpell(new Spell((int)SpellIdEnum.DOPPLESQUE_BLOODTHIRSTY_PUNISHMENT_8142, 6), Fighter.Cell);
                }

                selector.AddChild(new Decorator(ctx => target == null, new DecoratorContinue(new RandomMove(Fighter))));
                selector.AddChild(new Decorator(ctx => spell == null, new DecoratorContinue(new FleeAction(Fighter))));

                if (target != null && spell != null)
                {
                    selector.AddChild(new PrioritySelector(new Decorator(ctx => Fighter.CanCastSpell(spell, target.Cell) == SpellCastResult.OK, new Sequence(
                        new SpellCastAction(Fighter, spell, target.Cell, true), new Decorator(
                            new MoveNearTo(Fighter, target)))), new Sequence(
                            new MoveNearTo(Fighter, target), new Decorator(ctx => Fighter.CanCastSpell(spell, target.Cell) == SpellCastResult.OK, new Sequence(
                                new SpellCastAction(Fighter, spell, target.Cell, true))))));
                }

                foreach (var action in selector.Execute(this))
                { }
            }
        }
    }

    [BrainIdentifier((int)MonsterIdEnum.DOPEUL_HUPPERMAGE_4290)]
    public class DopeulHipper : Brain
    {
        public DopeulHipper(AIFighter fighter) : base(fighter)
        {
            fighter.Fight.FightStarted += Fight_FightStarted;
        }

        private void Fight_FightStarted(IFight obj)
        { }

        public override void Play()
        {
            foreach (var spell in Fighter.Spells.Values)
            {
                var target = Environment.GetNearestEnemy();
                var selector = new PrioritySelector();

                if (spell.Id == (int)SpellIdEnum.GLACIER_5840)
                {
                    Fighter.CastSpell(new Spell((int)SpellIdEnum.GLACIER_5840, 6), Fighter.Cell);
                }
                if (spell.Id == (int)SpellIdEnum.RUNIFICATION_5833)
                {
                    Fighter.CastSpell(new Spell((int)SpellIdEnum.RUNIFICATION_5833, 6), Fighter.Cell);
                }

                selector.AddChild(new Decorator(ctx => target == null, new DecoratorContinue(new RandomMove(Fighter))));
                selector.AddChild(new Decorator(ctx => spell == null, new DecoratorContinue(new FleeAction(Fighter))));

                if (target != null && spell != null)
                {
                    selector.AddChild(new PrioritySelector(
                        new Decorator(ctx => Fighter.CanCastSpell(spell, target.Cell) == SpellCastResult.OK, new Sequence(
                            new SpellCastAction(Fighter, spell, target.Cell, true), new Decorator(
                            new MoveNearTo(Fighter, target)))), new Sequence(
                            new MoveNearTo(Fighter, target),
                            new Decorator(ctx => Fighter.CanCastSpell(spell, target.Cell) == SpellCastResult.OK, new Sequence(new SpellCastAction(Fighter, spell, target.Cell, true))))));
                }

                foreach (var action in selector.Execute(this))
                { }
            }
        }
    }

    [BrainIdentifier((int)MonsterIdEnum.DOPEUL_ENIRIPSA_166)]
    public class DopeulEni : Brain
    {
        public DopeulEni(AIFighter fighter) : base(fighter)
        {
            fighter.Fight.FightStarted += Fight_FightStarted;
        }

        private void Fight_FightStarted(IFight obj)
        { }

        public override void Play()
        {
            foreach (var spell in Fighter.Spells.Values)
            {
                var target = Environment.GetNearestEnemy();
                var invo = Environment.GetFreeAdjacentCell();
                var selector = new PrioritySelector();

                if (spell.Id == (int)SpellIdEnum.DOPPLESQUE_FRIENDSHIP_WORD_2094)
                {
                    Fighter.CastSpell(new Spell((int)SpellIdEnum.DOPPLESQUE_FRIENDSHIP_WORD_2094, 6), invo);
                }
                if (spell.Id == (int)SpellIdEnum.DOPPLESQUE_FRIENDSHIP_WORD_6686)
                {
                    Fighter.CastSpell(new Spell((int)SpellIdEnum.DOPPLESQUE_FRIENDSHIP_WORD_6686, 6), invo);
                }

                selector.AddChild(new Decorator(ctx => target == null, new DecoratorContinue(new RandomMove(Fighter))));
                selector.AddChild(new Decorator(ctx => spell == null, new DecoratorContinue(new FleeAction(Fighter))));

                if (target != null && spell != null)
                {
                    selector.AddChild(new PrioritySelector(
                        new Decorator(ctx => Fighter.CanCastSpell(spell, target.Cell) == SpellCastResult.OK,
                            new Sequence(
                                new SpellCastAction(Fighter, spell, target.Cell, true),
                                new Decorator(new MoveNearTo(Fighter, target)))),
                        new Sequence(
                            new MoveNearTo(Fighter, target),
                            new Decorator(
                                ctx => Fighter.CanCastSpell(spell, target.Cell) == SpellCastResult.OK,
                                new Sequence(
                                    new SpellCastAction(Fighter, spell, target.Cell, true))))));
                }

                foreach (var action in selector.Execute(this))
                { }
            }
        }
    }

    [BrainIdentifier((int)MonsterIdEnum.DOPEUL_ROUBLARD_3111)]
    [BrainIdentifier((int)MonsterIdEnum.DOPEUL_XLOR_164)]
    public class DopeulNormales : Brain
    {
        public DopeulNormales(AIFighter fighter) : base(fighter)
        {
            fighter.Fight.FightStarted += Fight_FightStarted;
        }

        private void Fight_FightStarted(IFight obj)
        { }

        public override void Play()
        {
            foreach (var spell in Fighter.Spells.Values)
            {
                var target = Environment.GetNearestEnemy();
                var selector = new PrioritySelector();

                selector.AddChild(new Decorator(ctx => target == null, new DecoratorContinue(new RandomMove(Fighter))));
                selector.AddChild(new Decorator(ctx => spell == null, new DecoratorContinue(new FleeAction(Fighter))));

                if (target != null && spell != null)
                {
                    selector.AddChild(new PrioritySelector(
                        new Decorator(ctx => Fighter.CanCastSpell(spell, target.Cell) == SpellCastResult.OK,
                            new Sequence(
                                new SpellCastAction(Fighter, spell, target.Cell, true),
                                new Decorator(new MoveNearTo(Fighter, target)))),
                        new Sequence(
                            new MoveNearTo(Fighter, target),
                            new Decorator(
                                ctx => Fighter.CanCastSpell(spell, target.Cell) == SpellCastResult.OK,
                                new Sequence(
                                    new SpellCastAction(Fighter, spell, target.Cell, true))))));
                }

                foreach (var action in selector.Execute(this))
                { }
            }
        }
    }

    [BrainIdentifier((int)MonsterIdEnum.DOPEUL_SADIDA_169)]
    public class DopeulSadida : Brain
    {
        public DopeulSadida(AIFighter fighter) : base(fighter)
        {
            fighter.Fight.FightStarted += Fight_FightStarted;
        }

        private void Fight_FightStarted(IFight obj)
        { }

        public override void Play()
        {
            foreach (var spell in Fighter.Spells.Values)
            {
                var target = Environment.GetNearestEnemy();
                var invo = Environment.GetFreeAdjacentCell();
                var selector = new PrioritySelector();

                if (spell.Id == (int)SpellIdEnum.DOPPLESQUE_TREE_5676)
                    Fighter.CastSpell(new Spell((int)SpellIdEnum.DOPPLESQUE_TREE_5676, 6), invo);

                if (spell.Id == (int)SpellIdEnum.DOPPLESQUE_ULTRA_POWERFUL_2130)
                    Fighter.CastSpell(new Spell((int)SpellIdEnum.DOPPLESQUE_ULTRA_POWERFUL_2130, 6), invo);

                if (spell.Id == (int)SpellIdEnum.DOPPLESQUE_SACRIFICIAL_DOLL_2132)
                    Fighter.CastSpell(new Spell((int)SpellIdEnum.DOPPLESQUE_SACRIFICIAL_DOLL_2132, 6), invo);

                if (spell.Id == (int)SpellIdEnum.DOPPLESQUE_MADOLL_2127)
                    Fighter.CastSpell(new Spell((int)SpellIdEnum.DOPPLESQUE_MADOLL_2127, 6), invo);

                if (spell.Id == (int)SpellIdEnum.DOPPLESQUE_INFLATABLE_2133)
                    Fighter.CastSpell(new Spell((int)SpellIdEnum.DOPPLESQUE_INFLATABLE_2133, 6), invo);

                selector.AddChild(new Decorator(ctx => target == null, new DecoratorContinue(new RandomMove(Fighter))));
                selector.AddChild(new Decorator(ctx => spell == null, new DecoratorContinue(new FleeAction(Fighter))));

                if (target != null && spell != null)
                {
                    selector.AddChild(new PrioritySelector(
                        new Decorator(ctx => Fighter.CanCastSpell(spell, target.Cell) == SpellCastResult.OK,
                            new Sequence(
                                new SpellCastAction(Fighter, spell, target.Cell, true),
                                new Decorator(new MoveNearTo(Fighter, target)))),
                        new Sequence(
                            new MoveNearTo(Fighter, target),
                            new Decorator(
                                ctx => Fighter.CanCastSpell(spell, target.Cell) == SpellCastResult.OK,
                                new Sequence(
                                    new SpellCastAction(Fighter, spell, target.Cell, true))))));
                }

                foreach (var action in selector.Execute(this))
                { }
            }
        }
    }
}