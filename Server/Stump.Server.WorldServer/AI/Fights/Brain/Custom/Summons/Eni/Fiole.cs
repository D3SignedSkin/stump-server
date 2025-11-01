using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Fights.Teams;
using Stump.Server.WorldServer.Game.Spells;

namespace Stump.Server.WorldServer.AI.Fights.Brain.Custom.Summons
{
    [BrainIdentifier((int)MonsterIdEnum.FIOLE_5116)]
    [BrainIdentifier((int)MonsterIdEnum.FIOLE_5906)]
    public class FioleBrain : Brain
    {
        public FioleBrain(AIFighter fighter) : base(fighter)
        {
            fighter.BeforeDead += OnBeforeDead;
            fighter.Team.FighterAdded += OnFighterAdded;
        }

        public void OnFighterAdded(FightTeam team, FightActor fighter)
        {
            if (fighter != Fighter)
                return;

            Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.FLASKTIVATION_14646, 1), Fighter.Cell);
        }

        public void OnBeforeDead(FightActor fighter, FightActor killedBy)
        {
            if (fighter != Fighter)
                return;

            Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.FLASKTIVATION_13222, 1), Fighter.Cell);
        }
    }
}