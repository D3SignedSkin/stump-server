using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Spells;

namespace Stump.Server.WorldServer.AI.Fights.Brain.Custom.Boss
{
    [BrainIdentifier((int)MonsterIdEnum.AMBI_GUMAN_3525)]
    public class Ceboyix : Brain
    {
        public Ceboyix(AIFighter fighter) : base(fighter)
        {
            fighter.Fight.FightStarted += Fight_FightStarted;
        }

        private void Fight_FightStarted(IFight obj)
        {
            Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.VEG_MEN_3973, 1), Fighter.Cell);

            //Fighter.CastAutoSpell(new Spell(12105, 1), Fighter.Cell);
        }
    }

    [BrainIdentifier((int)MonsterIdEnum.CITWOUILLETTE_3529)]
    public class CeboyixInvoCalabaza : Brain
    {
        public CeboyixInvoCalabaza(AIFighter fighter) : base(fighter)
        {
            fighter.Fight.FightStarted += Fight_FightStarted;
        }

        private void Fight_FightStarted(IFight obj)
        {
            Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.VEGETICITY_3978, 1), Fighter.Cell);

            //Fighter.CastAutoSpell(new Spell(12105, 1), Fighter.Cell);
        }
    }

    [BrainIdentifier((int)MonsterIdEnum.SOISSANTH_AFFAM_3981)]
    public class CeboyixRosa : Brain
    {
        public CeboyixRosa(AIFighter fighter) : base(fighter)
        {
            fighter.Fight.FightStarted += Fight_FightStarted;
        }

        private void Fight_FightStarted(IFight obj)
        {
            Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.STRAIGHT_THROUGH_4278, 1), Fighter.Cell);

            //Fighter.CastAutoSpell(new Spell(12105, 1), Fighter.Cell);
        }
    }
}