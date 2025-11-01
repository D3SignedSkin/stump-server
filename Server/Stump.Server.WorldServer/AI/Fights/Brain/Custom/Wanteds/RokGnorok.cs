using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Spells;

namespace Stump.Server.WorldServer.AI.Fights.Brain.Custom.Boss
{
    [BrainIdentifier((int)MonsterIdEnum.ROK_GNOROK_550)]
    public class RokGnorok : Brain
    {
        public RokGnorok(AIFighter fighter) : base(fighter)
        {
            fighter.Fight.FightStarted += Fight_FightStarted;
        }

        private void Fight_FightStarted(IFight obj)
        {
            Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.NOT_LONG_NOW_4173, 1), Fighter.Cell);

            //Fighter.CastAutoSpell(new Spell(12105, 1), Fighter.Cell);
        }
    }
}