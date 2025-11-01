using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Spells;

namespace Stump.Server.WorldServer.AI.Fights.Brain.Custom.Boss
{

    [BrainIdentifier((int)MonsterIdEnum.DAZAK_MARTEGEL_5319)]
    public class DazakMartegel : Brain
    {
        public DazakMartegel(AIFighter fighter) : base(fighter)
        {
            fighter.Fight.FightStarted += Fight_FightStarted;
        }

        private void Fight_FightStarted(IFight obj)
        {
            //Fighter.Stats[PlayerFields.SummonLimit].Additional = 9999;
            //Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.STUBBYGERNESS_10332, 1), Fighter.Cell); //Estado 1
            //Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.STUBBYGERNESS_10336, 1), Fighter.Cell);
            Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.STUBBYGERNESS_10346, 1), Fighter.Cell);
            //Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.STUBBYGERNESS_10347, 1), Fighter.Cell);
        }
    }

    [BrainIdentifier((int)MonsterIdEnum.BARBLIER_5316)]
    [BrainIdentifier((int)MonsterIdEnum.KASROK_5317)]
    [BrainIdentifier((int)MonsterIdEnum.VATENBIRE_5318)]
    [BrainIdentifier((int)MonsterIdEnum.BOUFBOS_5315)]
    [BrainIdentifier((int)MonsterIdEnum.TANKLUME_5314)]
    public class BichosDazahk : Brain
    {
        public BichosDazahk(AIFighter fighter) : base(fighter)
        {
            fighter.Fight.FightStarted += Fight_FightStarted;
        }

        private void Fight_FightStarted(IFight obj)
        {
            //Fighter.Stats[PlayerFields.SummonLimit].Additional = 9999;
            //Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.STUBBYGERNESS_10332, 1), Fighter.Cell);
            //Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.STUBBYGERNESS_10336, 1), Fighter.Cell);
            Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.STUBBYGERNESS_10346, 1), Fighter.Cell);
            //Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.STUBBYGERNESS_10347, 1), Fighter.Cell);
        }
    }
}