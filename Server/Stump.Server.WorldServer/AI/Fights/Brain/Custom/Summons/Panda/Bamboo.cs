using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Spells;

namespace Stump.Server.WorldServer.AI.Fights.Brain.Custom.Summons
{
    [BrainIdentifier((int)MonsterIdEnum.BAMBOU_5846)]
    public class Bamboo : Brain
    {
        public Bamboo(AIFighter fighter) : base(fighter)
        {
            fighter.DamageInflicted += OnDamageInflicted;
        }

        private void OnDamageInflicted(FightActor fighter, Damage dmg)
        {
            if (fighter != Fighter)
                return;

            fighter.CastAutoSpell(new Spell((int)SpellIdEnum.BAMBOO_SHERPA_12831, 1), fighter.Cell);
        }
    }
}