using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Spells;

namespace Stump.Server.WorldServer.AI.Fights.Brain.Custom.Boss
{
    [BrainIdentifier((int)MonsterIdEnum.GUERRIER_DU_K_O__4016)]
    public class GuerrierDuKO : Brain
    {
        public GuerrierDuKO(AIFighter fighter) : base(fighter)
        {
            fighter.Fight.FightStarted += Fight_FightStarted;
        }

        private void Fight_FightStarted(IFight obj)
        {
            Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.MORNINGSTAR_5584, 1), Fighter.Cell);
            //Fighter.CastAutoSpell(new Spell(12105, 1), Fighter.Cell);
        }
    }
}