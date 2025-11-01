using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Fights.Teams;
using Stump.Server.WorldServer.Game.Spells;

namespace Stump.Server.WorldServer.AI.Fights.Brain.Custom.Summons.Enu
{
    [BrainIdentifier((int)MonsterIdEnum.SAC_ANIM_237)]
    [BrainIdentifier((int)MonsterIdEnum.SAC_ANIM_5830)]
    public class LivingBagBrain : Brain
    {
        public LivingBagBrain(AIFighter fighter) : base(fighter)
        {
            fighter.Team.FighterAdded += OnFighterAdded;
        }

        void OnFighterAdded(FightTeam team, FightActor fighter)
        {
            if (fighter != Fighter)
                return;

            fighter.CastAutoSpell(new Spell((int)SpellIdEnum.BAGRIFICE_13351, (byte)fighter.Level), fighter.Cell);
        }

        [BrainIdentifier((int)MonsterIdEnum.MUSETTE_ANIME_5125)]
        [BrainIdentifier((int)MonsterIdEnum.MUSETTE_ANIME_5838)]
        public class MusetteBrain : Brain
        {
            public MusetteBrain(AIFighter fighter) : base(fighter)
            {
                fighter.Team.FighterAdded += OnFighterAdded;
            }

            void OnFighterAdded(FightTeam team, FightActor fighter)
            {
                if (fighter != Fighter)
                    return;

                fighter.CastAutoSpell(new Spell((int)SpellIdEnum.SHARING_13373, 1), fighter.Cell);
            }
        }
    }
}
