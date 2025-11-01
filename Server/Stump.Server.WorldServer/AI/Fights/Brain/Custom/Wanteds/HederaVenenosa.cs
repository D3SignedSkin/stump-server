using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Spells;

namespace Stump.Server.WorldServer.AI.Fights.Brain.Custom.Boss
{
    [BrainIdentifier((int)MonsterIdEnum.AMY_LEMPOISONNEUSE_3528)]
    public class HederaVenenosa : Brain
    {
        public HederaVenenosa(AIFighter fighter) : base(fighter)
        {
            fighter.Fight.FightStarted += Fight_FightStarted;
        }

        private void Fight_FightStarted(IFight obj)
        {
            Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.VEGGIE_GROWTH_4005, 1), Fighter.Cell);

            //Fighter.CastAutoSpell(new Spell(12105, 1), Fighter.Cell);
        }
    }
}