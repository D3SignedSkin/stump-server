//using Stump.DofusProtocol.Enums;
//using Stump.Server.WorldServer.Game.Actors.Fight;
//using Stump.Server.WorldServer.Game.Fights;
//using Stump.Server.WorldServer.Game.Fights.Teams;
//using Stump.Server.WorldServer.Game.Spells;
//using System.Linq;

//namespace Stump.Server.WorldServer.AI.Fights.Brain.Custom.Boss
//{
//    [BrainIdentifier((int)MonsterIdEnum.PROTOZORREUR_3828)]
//    public class BallenaBrain : Brain
//    {
//        public BallenaBrain(AIFighter fighter) : base(fighter)
//        {
//            fighter.Fight.FightStarted += Fight_FightStarted;
//        }

//        private void Fight_FightStarted(IFight obj)
//        {
//            Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.INFECTIOUS_RESTORATION_4972, 1), Fighter.Cell);
//            Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.INFECTION_4976, 1), Fighter.Cell);
//        }
//    }

//    [BrainIdentifier((int)MonsterIdEnum.TABACILLE_3823)]
//    public class TabacilleBrain : Brain
//    {
//        public TabacilleBrain(AIFighter fighter) : base(fighter)
//        {
//            fighter.Fight.FightStarted += Fight_FightStarted;
//            fighter.Fight.TurnStarted += OnTurnStarted;
//        }

//        private void Fight_FightStarted(IFight obj)
//        {
//            Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.CURATIVE_RESTORATION_5179, 1), Fighter.Cell);
//        }

//        void OnTurnStarted(IFight fight, FightActor player)
//        {
//            if (Fighter.IsFighterTurn())
//                Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.CURATIVE_RESTORATION_5179, 1), Fighter.Cell);
//        }
//    }

//    [BrainIdentifier((int)MonsterIdEnum.VERMINOCULE_3824)]
//    public class Pulginfecto : Brain
//    {
//        public Pulginfecto(AIFighter fighter) : base(fighter)
//        {
//            fighter.Fight.FightStarted += Fight_FightStarted;
//            fighter.Fight.TurnStarted += OnTurnStarted;
//        }

//        private void Fight_FightStarted(IFight obj)
//        {
//            //Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.PROXIMITY_RESTORATION_4942, 1), Fighter.Cell);
//        }

//        void OnTurnStarted(IFight fight, FightActor player)
//        {
//            //if (Fighter.IsFighterTurn())
//            //    Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.PROXIMITY_RESTORATION_4942, 1), Fighter.Cell);
//        }
//    }

//    [BrainIdentifier((int)MonsterIdEnum.BACTERRIB_3825)]
//    public class BacTerrible : Brain
//    {
//        public BacTerrible(AIFighter fighter) : base(fighter)
//        {
//            fighter.Fight.FightStarted += Fight_FightStarted;
//            fighter.Fight.TurnStarted += OnTurnStarted;
//        }

//        private void Fight_FightStarted(IFight obj)
//        {
//            //Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.DISTANT_RESTORATION_4948, 1), Fighter.Cell);
//        }

//        void OnTurnStarted(IFight fight, FightActor player)
//        {
//            //Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.DISTANT_RESTORATION_4948, 1), Fighter.Cell);
//        }
//    }

//    [BrainIdentifier((int)MonsterIdEnum.VIRUSTINE_3826)]
//    public class Virusca : Brain
//    {
//        public Virusca(AIFighter fighter) : base(fighter)
//        {
//            fighter.Fight.FightStarted += Fight_FightStarted;
//            fighter.Fight.TurnStarted += OnTurnStarted;
//        }

//        private void Fight_FightStarted(IFight obj)
//        {
//            Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.MOBILE_RESTORATION_4953, 1), Fighter.Cell);
//        }

//        void OnTurnStarted(IFight fight, FightActor player)
//        {
//            if (Fighter.IsFighterTurn())
//                Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.MOBILE_RESTORATION_4953, 1), Fighter.Cell);
//        }
//    }

//    [BrainIdentifier((int)MonsterIdEnum.PATAUGERME_3827)]
//    public class Patogermen : Brain
//    {
//        public Patogermen(AIFighter fighter) : base(fighter)
//        {
//            fighter.Fight.FightStarted += Fight_FightStarted;
//            fighter.Fight.TurnStarted += OnTurnStarted;
//        }

//        private void Fight_FightStarted(IFight obj)
//        {
//            if (Fighter.IsFighterTurn())
//                Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.ELEMENTAL_RESTORATION_4959, 1), Fighter.Cell);
//        }

//        void OnTurnStarted(IFight fight, FightActor player)
//        {
//            if (Fighter.IsFighterTurn())
//                Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.ELEMENTAL_RESTORATION_4959, 1), Fighter.Cell);
//        }
//    }

//    [BrainIdentifier((int)MonsterIdEnum.MALAMIBE_3822)]
//    public class BallenaInvo : Brain
//    {
//        public BallenaInvo(AIFighter fighter) : base(fighter)
//        {
//            fighter.Team.FighterAdded += OnFighterAdded;
//        }

//        void OnFighterAdded(FightTeam team, FightActor fighter)
//        {
//            if (fighter != Fighter)
//                return;
            
//            Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.DECELLULARISATION_4933, 1), Fighter.Cell);
//        }
//    }
//}