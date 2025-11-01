using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Spells;

namespace Stump.Server.WorldServer.AI.Fights.Brain.Custom.Boss
{
    [BrainIdentifier((int)MonsterIdEnum.COMTE_RAZOF_4803)]
    public class ConteRazof : Brain
    {
        public ConteRazof(AIFighter fighter) : base(fighter)
        {
            fighter.Fight.FightStarted += Fight_FightStarted;
        }

        private void Fight_FightStarted(IFight obj)
        {
            Fighter.Stats[PlayerFields.SummonLimit].Additional = 9999;

            Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.DANGEROUS_GAME_8512, 1), Fighter.Cell);

            //Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.DANGEROUS_GAME_4872, 1), Fighter.Cell);
            //Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.DANGEROUS_GAME_4873, 1), Fighter.Cell);
            //Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.DANGEROUS_GAME_4877, 1), Fighter.Cell);
            //Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.DANGEROUS_GAME_8527, 1), Fighter.Cell);
            //Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.DANGEROUS_GAME_8529, 1), Fighter.Cell);
        }
    }

    [BrainIdentifier((int)MonsterIdEnum.NEMROZ_4807)]
    public class Nemroza : Brain
    {
        public Nemroza(AIFighter fighter) : base(fighter)
        {
            fighter.Fight.FightStarted += Fight_FightStarted;
        }

        private void Fight_FightStarted(IFight obj)
        {
            //Fighter.Stats[PlayerFields.SummonLimit].Additional = 9999;
            Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.RAPHEL_MAI_8543, 1), Fighter.Cell);
        }
    }
}