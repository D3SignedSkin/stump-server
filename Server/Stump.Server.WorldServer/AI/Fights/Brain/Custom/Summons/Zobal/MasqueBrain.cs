using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Fights.Teams;
using Stump.Server.WorldServer.Game.Spells;

namespace Stump.Server.WorldServer.AI.Fights.Brain.Custom.Summons.Ani
{
    [BrainIdentifier((int)MonsterIdEnum.MASQUE_GRIMAANT_5152)]
    class MasqueBrain : Brain
    {
        public MasqueBrain(AIFighter fighter) : base(fighter)
        {
            fighter.Team.FighterAdded += OnFighterAdded;
        }

        void OnFighterAdded(FightTeam team, FightActor fighter)
        {
            if (fighter != Fighter)
                return;

            if (fighter.IsSummoned())
                fighter.CastAutoSpell(new Spell((int)SpellIdEnum.GRIMACE_9942, 1), fighter.Summoner.Cell);
        }
    }
}