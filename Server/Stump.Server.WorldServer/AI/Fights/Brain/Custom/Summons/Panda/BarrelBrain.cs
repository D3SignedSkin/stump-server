using System.Collections.Generic;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Fights.Teams;
using Stump.Server.WorldServer.Game.Spells;

namespace Stump.Server.WorldServer.AI.Fights.Brain.Custom.Summons.Panda
{
    [BrainIdentifier((int)MonsterIdEnum.TONNEAU_ATTRACTIF_5843)]
    public class BarrelBrain : Brain
    {
        public BarrelBrain(AIFighter fighter) : base(fighter)
        {
            fighter.Team.FighterAdded += OnFighterAdded;
            fighter.Fight.TurnStarted += OnTurnStarted;
        }

        void OnFighterAdded(FightTeam team, FightActor fighter)
        {
            if (fighter != Fighter)
                return;

            Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.POURING_12827, 1), Fighter.Cell);
        }

        private void OnTurnStarted(IFight fight, FightActor player)
        {
            if (player != Fighter)
                return;

            if (!(Fighter is SummonedMonster))
                return;

            var barrel = (SummonedMonster)Fighter;

            List<FightActor>
            fighters = Fighter.Fight.GetAllFightersInLine(Fighter.Cell, (int)(11 + 2 * (1 - barrel.MonsterGrade.GradeId)), DirectionsEnum.DIRECTION_NORTH_EAST);
            fighters.AddRange(Fighter.Fight.GetAllFightersInLine(Fighter.Cell, (int)(11 + 2 * (1 - barrel.MonsterGrade.GradeId)), DirectionsEnum.DIRECTION_SOUTH_EAST));
            fighters.AddRange(Fighter.Fight.GetAllFightersInLine(Fighter.Cell, (int)(11 + 2 * (1 - barrel.MonsterGrade.GradeId)), DirectionsEnum.DIRECTION_SOUTH_WEST));
            fighters.AddRange(Fighter.Fight.GetAllFightersInLine(Fighter.Cell, (int)(11 + 2 * (1 - barrel.MonsterGrade.GradeId)), DirectionsEnum.DIRECTION_NORTH_WEST));

            var spell = new Spell((int)SpellIdEnum.BINGE_12801, (short)barrel.MonsterGrade.GradeId);

            foreach (var fighter in fighters)
            {
                if (barrel.Position.Point.IsAdjacentTo(fighter.Position.Point) || barrel.CanCastSpell(spell, fighter.Cell) != SpellCastResult.OK)
                    continue;

                if (barrel.IsFriendlyWith(fighter) && fighter.HasState((int)SpellStatesEnum.DRUNK_498))
                {
                    barrel.CastSpell(spell, fighter.Cell);
                }
                else if (!barrel.IsFriendlyWith(fighter))
                {
                    barrel.CastSpell(spell, fighter.Cell);
                }
            }
        }
    }

    [BrainIdentifier((int)MonsterIdEnum.TONNEAU_INCAPACITANT_5844)]
    public class Barrel2Brain : Brain
    {
        public Barrel2Brain(AIFighter fighter) : base(fighter)
        {
            fighter.Team.FighterAdded += OnFighterAdded;
        }

        void OnFighterAdded(FightTeam team, FightActor fighter)
        {
            if (fighter != Fighter)
                return;

            Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.POURING_12827, 1), Fighter.Cell);
        }
    }
}