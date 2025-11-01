//using Stump.DofusProtocol.Enums;
//using Stump.Server.WorldServer.Game.Actors.Fight;
//using Stump.Server.WorldServer.Game.Fights;
//using Stump.Server.WorldServer.Game.Maps.Cells;
//using Stump.Server.WorldServer.Game.Spells.Casts;
//using System.Collections.Generic;
//using System.Linq;

//namespace Stump.Server.WorldServer.game.spells.Casts.Sadida
//{
//    [SpellCastHandler(SpellIdEnum.MANGROVE_14396)]
//    public class MangroveHandler : DefaultSpellCastHandler
//    {
//        public MangroveHandler(SpellCastInformations cast) : base(cast)
//        { }

//        public override void Execute()
//        {
//            if (!base.Initialize())
//                Initialize();

//            var _affecteds = this.Fight.GetOneFighter(TargetedCell).Position.Point.GetActorAdjacentCells();
//            var _hasthree = this.Fight.GetAllFighters().Any(x => x is SummonedMonster && (x as SummonedMonster).Monster.Template.Id == (int)MonsterIdEnum.ARBRE_5894 && _affecteds.Any(y => y.CellId == x.Cell.Id));

//            //Effect_DamageWater
//            if (!_hasthree)
//                Handlers[0].Apply();

//            //Effect_DamageWater (Desativado Duplicação)
//            if (_hasthree)
//                Handlers[1].Apply();

//            //Effect_1019
//            Handlers[2].Apply();

//            //Effect_2794
//            Handlers[3].Apply();

//            //Effect_TriggerBuff
//            Handlers[4].Apply();

//            //Effect_TriggerBuff
//            Handlers[5].Apply();
//        }
//    }

//    [SpellCastHandler(SpellIdEnum.MANGROVE_14397)]
//    public class Mangrove_14397Handler : DefaultSpellCastHandler
//    {
//        public Mangrove_14397Handler(SpellCastInformations cast) : base(cast)
//        { }

//        public override void Execute()
//        {
//            if (!base.Initialize())
//                Initialize();

//            if (Spell.CurrentLevel == 1)
//            {
//                //Effect_AddState - 1438
//                Handlers[0].Apply();
//            }
//            else if (Spell.CurrentLevel == 2)
//            {
//                //Effect_RemoveSpellEffects - 14397
//                Handlers[0].Apply();
//            }
//            else if (Spell.CurrentLevel == 3)
//            {
//                ////Effect_DamageWater
//                //Handlers[0].Apply();

//                ////Effect_DamageWater
//                //Handlers[0].Apply();

//                ////Effect_DamageWater
//                //Handlers[0].Apply();

//                //Effect_2794 - 14696
//                Handlers[0].Apply();

//                //Effect_2794 - 14697
//                Handlers[0].Apply();
//            }
//            else if (Spell.CurrentLevel == 4)
//            {
//                ////Effect_DamageWater
//                //Handlers[0].Apply();

//                ////Effect_DamageWater
//                //Handlers[0].Apply();

//                ////Effect_DamageWater
//                //Handlers[0].Apply();

//                //Effect_2794 - 14696
//                Handlers[0].Apply();

//                //Effect_2794 - 14697
//                Handlers[0].Apply();
//            }
//        }
//    }
//}