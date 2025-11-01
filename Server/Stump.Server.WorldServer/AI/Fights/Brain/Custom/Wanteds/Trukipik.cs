using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Spells;

namespace Stump.Server.WorldServer.AI.Fights.Brain.Custom.Boss
{
    [BrainIdentifier((int)MonsterIdEnum.TRUKIPIK_4240)]
    public class Trukipik : Brain
    {
        public Trukipik(AIFighter fighter) : base(fighter)
        {
            fighter.Fight.FightStarted += Fight_FightStarted;
        }

        private void Fight_FightStarted(IFight obj)
        {
            Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.CLOSE_COMBAT_5648, 1), Fighter.Cell);
            //Fighter.CastAutoSpell(new Spell(12105, 1), Fighter.Cell);
        }
    }
}