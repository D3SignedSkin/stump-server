using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Fights;

namespace Stump.Server.WorldServer.AI.Fights.Brain.Custom.Boss
{
    [BrainIdentifier((int)MonsterIdEnum.MOON_226)]
    public class MoonBrain : Brain
    {
        public MoonBrain(AIFighter fighter) : base(fighter)
        {
            fighter.Fight.FightStarted += Fight_FightStarted;
        }

        private void Fight_FightStarted(IFight obj)
        {
            Fighter.Stats[PlayerFields.SummonLimit].Additional = 1;

            //Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.DARK_POWER_302, 1), Fighter.Cell);
            //Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.DARK_POWER_3276, 1), Fighter.Cell);
        }
    }
}