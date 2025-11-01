using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.AI.Fights.Actions;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Spells;
using System;
using System.Linq;
using TreeSharp;

namespace Stump.Server.WorldServer.AI.Fights.Brain.Custom.Boss
{
    [BrainIdentifier((int)MonsterIdEnum.RAPHAELA_944)]
    [BrainIdentifier((int)MonsterIdEnum.DONATELLA_945)]
    [BrainIdentifier((int)MonsterIdEnum.LEONARDAWA_947)]
    [BrainIdentifier((int)MonsterIdEnum.MICHELANGELA_946)]
    public class TurtlesCellBrain : Brain
    {
        public TurtlesCellBrain(AIFighter fighter) : base(fighter)
        { }

        public override void Play()
        {
            try
            {
                foreach (var spell in Fighter.Spells.Values)
                {
                    var target = Environment.GetNearestEnemy();
                    var targetSphincter = Environment.GetNearestFighter(x => x.IsFriendlyWith(Fighter) && x.IsAlive() && Fighter.Position.Point.GetAdjacentCells().Any(cell => cell.CellId == x.Cell.Id) && x != Fighter && x is MonsterFighter && (x as MonsterFighter).Monster.Template.Id == (int)MonsterIdEnum.SPHINCTER_CELL_943);
                    var selector = new PrioritySelector();

                    selector.AddChild(new Decorator(ctx => target == null, new DecoratorContinue(new RandomMove(Fighter))));
                    selector.AddChild(new Decorator(ctx => spell == null, new DecoratorContinue(new FleeAction(Fighter))));

                    if (targetSphincter != null && spell.Id == (int)SpellIdEnum.KAWABUNGA_1019 && this.Fighter is SummonedMonster && (this.Fighter as SummonedMonster).Monster.Template.Id == (int)MonsterIdEnum.RAPHAELA_944)
                    {
                        Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.KAWABUNGA_1019, 2), targetSphincter.Cell);
                    }
                    else if (targetSphincter != null && spell.Id == (int)SpellIdEnum.KAWABUNGA_1019 && this.Fighter is SummonedMonster && (this.Fighter as SummonedMonster).Monster.Template.Id == (int)MonsterIdEnum.DONATELLA_945)
                    {
                        Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.KAWABUNGA_1019, 1), targetSphincter.Cell);
                    }
                    else if (targetSphincter != null && spell.Id == (int)SpellIdEnum.KAWABUNGA_1019 && this.Fighter is SummonedMonster && (this.Fighter as SummonedMonster).Monster.Template.Id == (int)MonsterIdEnum.MICHELANGELA_946)
                    {
                        Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.KAWABUNGA_1019, 4), targetSphincter.Cell);
                    }
                    else if (targetSphincter != null && spell.Id == (int)SpellIdEnum.KAWABUNGA_1019 && this.Fighter is SummonedMonster && (this.Fighter as SummonedMonster).Monster.Template.Id == (int)MonsterIdEnum.LEONARDAWA_947)
                    {
                        Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.KAWABUNGA_1019, 3), targetSphincter.Cell);
                    }

                    if (target != null && spell != null)
                    {
                        selector.AddChild(new PrioritySelector(
                            new Decorator(ctx => Fighter.CanCastSpell(spell, target.Cell) == SpellCastResult.OK, new Sequence(new SpellCastAction(Fighter, spell, target.Cell, true), new Decorator(new MoveNearTo(Fighter, target)))),
                            new Sequence(new MoveNearTo(Fighter, target), new Decorator(ctx => Fighter.CanCastSpell(spell, target.Cell) == SpellCastResult.OK, new Sequence(new SpellCastAction(Fighter, spell, target.Cell, true))))));
                    }

                    foreach (var action in selector.Execute(this))
                    {

                    }
                }
            }
            catch (Exception ex)
            {
                Brain.logger.Error($"Error Brain: {ex}");
            }
        }
    }
}