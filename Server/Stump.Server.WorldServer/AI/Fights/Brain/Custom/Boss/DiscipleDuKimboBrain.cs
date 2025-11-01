using NLog;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Spells;
using System;

namespace Stump.Server.WorldServer.AI.Fights.Brain.Custom.Boss
{
    [BrainIdentifier((int)MonsterIdEnum.DISCIPLE_DU_KIMBO_1088)]
    public class DiscipleDuKimboBrain : Brain
    {
        public DiscipleDuKimboBrain(AIFighter fighter) : base(fighter)
        {
            fighter.Fight.TurnStarted += OnTurnStarted;
        }

        private void OnTurnStarted(IFight obj, FightActor actor)
        {
            try
            {
                if (Fighter.IsSummoned())
                {
                    if (Fighter.HasState((int)SpellStatesEnum.GLYPHE_IMPAIRE_29))
                    {
                        Fighter.CastSpell(new Spell((int)SpellIdEnum.ODD_GLYPH_1073, (short)(Fighter as SummonedMonster).MonsterGrade.GradeId), Fighter.Cell);
                    }
                    else if (Fighter.HasState((int)SpellStatesEnum.GLYPHE_PAIRE_30))
                    {
                        Fighter.CastSpell(new Spell((int)SpellIdEnum.EVEN_GLYPH_1072, (short)(Fighter as SummonedMonster).MonsterGrade.GradeId), Fighter.Cell);
                    }
                }
            }
            catch (Exception ex)
            {
                Brain.logger.Error($"Error Brain: {ex}");
            }
        }
    }
}