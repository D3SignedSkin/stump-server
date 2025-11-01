using System.Linq;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.AI.Fights.Actions;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Fights.Buffs;
using Stump.Server.WorldServer.Game.Fights.Teams;
using Stump.Server.WorldServer.Game.Fights.Triggers;
using Stump.Server.WorldServer.Game.Spells;
using TreeSharp;

namespace Stump.Server.WorldServer.AI.Fights.Brain.Custom.Summons.Ani
{
    [BrainIdentifier(7139)]
    public class ImmortalLanceBrain : Brain
    {
        public ImmortalLanceBrain(AIFighter fighter)
            : base(fighter)
        {
            fighter.Team.FighterAdded += OnFighterAdded;
            fighter.BeforeDead += OnBeforeDead;
        }

        public void OnBeforeDead(FightActor fighter, FightActor killedBy)
        {
            if (fighter != Fighter)
                return;
            var fight = Fighter.Fight;
            var triggers = fight.GetTriggersByCell(Fighter.Cell);
            foreach (var trigger in triggers.OfType<Glyph>().Where(x => x.CanBeForced && x.Caster == Fighter))
            {
                fight.RemoveTrigger(trigger);
            }
            // agrega el estado 3590 al invocador
            if (Fighter is SummonedMonster)
            {
                var summoner = ((SummonedMonster)Fighter).Summoner;
                if (summoner != null)
                {
                    summoner.CastAutoSpell(new Spell(24387, 1), summoner.Cell);
                }

            }
        }

        void OnFighterAdded(FightTeam team, FightActor fighter)
        {
            if (fighter != Fighter)
                return;
            
            if(fighter.IsSummoned())
                fighter.CastAutoSpell(new Spell(23262, 1), fighter.Cell);
        }
    }
}