using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Spells;

namespace Stump.Server.WorldServer.AI.Fights.Brain.Custom.Boss
{
    [BrainIdentifier((int)MonsterIdEnum.GRAND_KONGOKU_4028)]
    public class GrandeKongoku : Brain
    {
        public GrandeKongoku(AIFighter fighter) : base(fighter)
        {
            fighter.Fight.FightStarted += Fight_FightStarted;
        }

        private void Fight_FightStarted(IFight obj)
        {
            Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.AD_HOC_ALLIES_5813, 1), Fighter.Cell);

            //Fighter.CastAutoSpell(new Spell(12105, 1), Fighter.Cell);
        }
    }

    [BrainIdentifier((int)MonsterIdEnum.COCOLUNE_4029)]
    public class GrandeKongokuInvo : Brain
    {
        public GrandeKongokuInvo(AIFighter fighter) : base(fighter)
        {
            fighter.Fight.TurnStarted += OnTurnStarted;
        }

        private void OnTurnStarted(IFight obj, FightActor actor)
        {
            if (Fighter.IsSummoned())
                Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.NEW_MOON_5658, 1), Fighter.Cell);

            //Fighter.CastAutoSpell(new Spell(12105, 1), Fighter.Cell);
        }
    }
}