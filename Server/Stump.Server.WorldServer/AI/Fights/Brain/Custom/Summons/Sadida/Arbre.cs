using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Fights.Teams;
using Stump.Server.WorldServer.Game.Spells;

namespace Stump.Server.WorldServer.AI.Fights.Brain.Custom.Summons.Sadida
{
    [BrainIdentifier((int)MonsterIdEnum.ARBRE_282)]
    [BrainIdentifier((int)MonsterIdEnum.ARBRE_5894)]
    public class Arbre : Brain
    {
        public Arbre(AIFighter fighter) : base(fighter)
        {
            fighter.Team.FighterAdded += OnFighterAdded;
        }

        void OnFighterAdded(FightTeam team, FightActor fighter)
        {
            if (fighter != Fighter)
                return;

            fighter.CastAutoSpell(new Spell((int)SpellIdEnum.FOLIAGE_14700, 1), fighter.Cell);
        }
    }
}
