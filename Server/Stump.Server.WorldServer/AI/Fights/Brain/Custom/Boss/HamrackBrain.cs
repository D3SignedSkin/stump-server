using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.AI.Fights.Actions;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Spells;
using System.Linq;
using TreeSharp;

namespace Stump.Server.WorldServer.AI.Fights.Brain.Custom
{
    [BrainIdentifier((int)MonsterIdEnum.HAMRACK_2955)]
    public class HamrackBrain : Brain
    {
        public HamrackBrain(AIFighter fighter) : base(fighter)
        {
            Fighter.GetAlive += OnGetAlive;
        }

        public override void Play()
        {
            var target = Environment.GetNearestFighter(x => x.IsFriendlyWith(Fighter) && x != Fighter && Fighter.Position.Point.SquareDistanceTo(x.Position.Point) < 3);
            var selector = new PrioritySelector();
            var spellinvu = Fighter.Spells.Where(x => x.Value.Id == (int)SpellIdEnum.RUDDER_2618).FirstOrDefault().Value;
            var spell = Fighter.Spells.Where(x => x.Value.Id == (int)SpellIdEnum.HEEL_682).FirstOrDefault().Value;
            var enemy = Environment.GetNearestEnemy();

            if (target != null && spellinvu != null && (target as MonsterFighter).Monster.Template.Id == (int)MonsterIdEnum.BEN_LE_RIPATE_2877)
            {
                selector.AddChild(new PrioritySelector(
                    new Decorator(ctx => Fighter.CanCastSpell(spellinvu, target.Cell) == SpellCastResult.OK,
                        new Sequence(new SpellCastAction(Fighter, spellinvu, target.Cell, false))),
                    new Sequence(new MoveNearTo(Fighter, target),
                        new Decorator(ctx => Fighter.CanCastSpell(spellinvu, target.Cell) == SpellCastResult.OK,
                        new Sequence(new SpellCastAction(Fighter, spellinvu, target.Cell, false))))
                    ));
            }
            else
            {
                selector.AddChild(new PrioritySelector(
                    new Decorator(ctx => Fighter.Position.Point.GetAdjacentCells().Contains(enemy.Position.Point),
                        new Sequence(new SpellCastAction(Fighter, spell, Fighter.Cell, false))),
                    new Sequence(new MoveNearTo(Fighter, enemy),
                        new Decorator(ctx => Fighter.Position.Point.GetAdjacentCells().Contains(enemy.Position.Point),
                        new Sequence(new SpellCastAction(Fighter, spell, Fighter.Cell, false))))
                    ));
            }

            foreach (var action in selector.Execute(this))
            {

            }
        }

        private void OnGetAlive(FightActor fighter)
        {
            if (fighter != Fighter)
                return;

            if (fighter is SummonedMonster && (fighter as SummonedMonster).Monster.MonsterId == (int)MonsterIdEnum.HAMRACK_2955)
                fighter.CastAutoSpell(new Spell((int)SpellIdEnum.BARD_BOMB_2617, 1), fighter.Cell);
        }
    }
}