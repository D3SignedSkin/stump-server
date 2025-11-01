using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Spells;

namespace Stump.Server.WorldServer.AI.Fights.Brain.Custom.Boss
{
    [BrainIdentifier((int)MonsterIdEnum.SHUSHU_DEBRUKSAYL_4816)]
    public class ShushuDebruksayl : Brain
    {
        public ShushuDebruksayl(AIFighter fighter) : base(fighter)
        {
            fighter.Fight.FightStarted += Fight_FightStarted;
        }

        private void Fight_FightStarted(IFight obj)
        {
            Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.ELEMENTAL_SIPHONING_8550, 1), Fighter.Cell);
            //Fighter.CastAutoSpell(new Spell(12105, 1), Fighter.Cell);
        }
    }
}