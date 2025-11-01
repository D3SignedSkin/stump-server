using System.Linq;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Fights.Teams;
using Stump.Server.WorldServer.Game.Spells;

namespace Stump.Server.WorldServer.AI.Fights.Brain.Custom.Summons
{
    [BrainIdentifier((int)MonsterIdEnum.GIDE_5903)]
    [BrainIdentifier((int)MonsterIdEnum.GIDE_5128)]
    public class EgideBrain : Brain
    {
        public EgideBrain(AIFighter fighter) : base(fighter)
        {
            fighter.Team.FighterAdded += OnFighterAdded;
        }

        void OnFighterAdded(FightTeam team, FightActor fighter)
        {
            if (fighter != Fighter)
                return;

            Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.SANCTUARY_13042, 1), Fighter.Cell);
        }
    }
}