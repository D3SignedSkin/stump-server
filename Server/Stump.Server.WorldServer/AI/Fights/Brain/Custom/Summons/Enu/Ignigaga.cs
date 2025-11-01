using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Spells;
using System.Linq;

namespace Stump.Server.WorldServer.AI.Fights.Brain.Custom.Summons
{
    [BrainIdentifier((int)MonsterIdEnum.PELLE_ENFLAMME_5829)]
    public class IgnigagaBrain : Brain
    {
        public IgnigagaBrain(AIFighter fighter) : base(fighter)
        {
            fighter.BeforeDead += OnBeforeDead;
        }

        public void OnBeforeDead(FightActor fighter, FightActor killedBy)
        {
            if (fighter != Fighter)
                return;

            var spellHandler = SpellManager.Instance.GetSpellCastHandler((Fighter as SummonedMonster).Summoner, new Spell((int)SpellIdEnum.SHOVEL_SHOVED_14276, 1), killedBy.Cell, false);

            spellHandler.Initialize();

            var handlers = spellHandler.GetEffectHandlers().ToArray();

            using (Fighter.Fight.StartSequence(SequenceTypeEnum.SEQUENCE_SPELL))
            {
                spellHandler.Execute();
            }
        }
    }
}