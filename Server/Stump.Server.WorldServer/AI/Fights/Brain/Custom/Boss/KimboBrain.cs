using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.AI.Fights.Actions;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Effects;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Spells;
using System;
using System.Linq;
using TreeSharp;

namespace Stump.Server.WorldServer.AI.Fights.Brain.Custom.Boss
{
    [BrainIdentifier((int)MonsterIdEnum.KIMBO_1045)]
    public class KimboBrain : Brain
    {
        Spell spellUsed = null;

        public KimboBrain(AIFighter fighter) : base(fighter)
        {
            fighter.DamageInflicted += OnDamageInflicted;
        }

        private void OnDamageInflicted(FightActor fighter, Damage dmg)
        {
            if (fighter != Fighter)
                return;

            if (dmg.ElementId == (int)EffectSchoolEnum.Earth || dmg.ElementId == (int)EffectSchoolEnum.Water)
            {
                spellUsed = new Spell((int)SpellIdEnum.ODD_STATE_1076, 1);
            }
            else if (dmg.ElementId == (int)EffectSchoolEnum.Air || dmg.ElementId == (int)EffectSchoolEnum.Fire)
            {
                spellUsed = new Spell((int)SpellIdEnum.EVEN_STATE_1075, 1);
            }
        }

        public override void Play()
        {
            try
            {
                foreach (var spell in Fighter.Spells.Values)
                {
                    var invoCell = Environment.GetFreeAdjacentCell();
                    var target = Environment.GetNearestEnemy();
                    var targetDisciple = Environment.GetNearestFighter(x => x.IsFriendlyWith(Fighter) && x.IsAlive() && x != Fighter && x is SummonedMonster && (x as SummonedMonster).Monster.Template.Id == (int)MonsterIdEnum.DISCIPLE_DU_KIMBO_1088);
                    var selector = new PrioritySelector();

                    if (spell.Id == (int)SpellIdEnum.EVEN_STATE_1075 || spell.Id == (int)SpellIdEnum.ODD_STATE_1076)
                    {
                        if (spellUsed != null && targetDisciple != null)
                        {
                            Fighter.CastAutoSpell(spellUsed, targetDisciple.Cell);
                            spellUsed = null;
                        }
                    }

                    if (spell.Id == (int)SpellIdEnum.SUMMONING_OF_DISCIPLE_1074 && targetDisciple is null)
                    {
                        Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.SUMMONING_OF_DISCIPLE_1074, (short)spell.CurrentLevel), invoCell);
                    }

                    selector.AddChild(new Decorator(ctx => target == null, new DecoratorContinue(new RandomMove(Fighter))));
                    selector.AddChild(new Decorator(ctx => spell == null, new DecoratorContinue(new FleeAction(Fighter))));

                    if (target != null && spell != null && spell.Template.Id != (int)SpellIdEnum.KIMBO_TELEPORTATION_1118)
                    {
                        selector.AddChild(new PrioritySelector(
                            new Decorator(ctx => Fighter.CanCastSpell(spell, target.Cell) == SpellCastResult.OK, new Sequence(new SpellCastAction(Fighter, spell, target.Cell, true), new Decorator(new MoveNearTo(Fighter, target)))),
                            new Sequence(new MoveNearTo(Fighter, target), new Decorator(ctx => Fighter.CanCastSpell(spell, target.Cell) == SpellCastResult.OK, new Sequence(new SpellCastAction(Fighter, spell, target.Cell, true))))));
                    }

                    //if (spell.Template.Id == (int)SpellIdEnum.KIMBO_TELEPORTATION_1118)
                    //{
                    //   // var cellTarget = Fighter.Fight.Map.GetCell(target.Position.Point.GetActorAdjacentCells(x => target.Map.Cells[x].Walkable && target.Map.IsCellFree(x) && !target.Map.IsObjectItemOnCell(x)).FirstOrDefault().CellId);
                    //    //Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.KIMBO_TELEPORTATION_1118, (short)spell.CurrentLevel), cellTarget);
                    //}

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