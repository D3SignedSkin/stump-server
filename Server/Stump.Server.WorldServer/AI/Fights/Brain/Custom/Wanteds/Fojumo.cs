using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Spells;

namespace Stump.Server.WorldServer.AI.Fights.Brain.Custom.Boss
{
    [BrainIdentifier((int)MonsterIdEnum.FOJUMO_4015)]
    public class Fojumo : Brain
    {
        public Fojumo(AIFighter fighter) : base(fighter)
        {
            fighter.Fight.FightStarted += Fight_FightStarted;
        }

        private void Fight_FightStarted(IFight obj)
        {
            Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.EVENING_THE_SCORE_5640, 1), Fighter.Cell);

            //Fighter.CastAutoSpell(new Spell(12105, 1), Fighter.Cell);
        }
    }

    [BrainIdentifier((int)MonsterIdEnum.FOJUMO_4026)]
    public class FojumoInvo : Brain
    {
        public FojumoInvo(AIFighter fighter) : base(fighter)
        {
            fighter.Fight.TurnStarted += OnTurnStarted;
        }

        private void OnTurnStarted(IFight obj, FightActor actor)
        {
            if (Fighter.IsSummoned())
                Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.EVENING_THE_SCORE_5706, 1), Fighter.Cell);

            //Fighter.CastAutoSpell(new Spell(12105, 1), Fighter.Cell);
        }
    }
}