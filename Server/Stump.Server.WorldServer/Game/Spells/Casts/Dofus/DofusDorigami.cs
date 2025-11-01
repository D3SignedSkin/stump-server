//using Stump.DofusProtocol.Enums;
//using Stump.Server.WorldServer.Game.Fights;

//namespace Stump.Server.WorldServer.Game.Spells.Casts
//{
//    [SpellCastHandler(SpellIdEnum.DORIGAMI_17307)]
//    public class DofusDorigami : DefaultSpellCastHandler
//    {
//        public DofusDorigami(SpellCastInformations cast) : base(cast) { }

//        public override void Execute()
//        {
//            Handlers[0].Apply();
//            Handlers[1].Apply();
//            Handlers[2].Apply();
//            Handlers[3].Apply();
//        }
//    }

//    [SpellCastHandler(SpellIdEnum.DORIGAMI_17308)]
//    public class DofusDorigamiOne : DefaultSpellCastHandler
//    {
//        public DofusDorigamiOne(SpellCastInformations cast) : base(cast) { }

//        public override void Execute()
//        {
//            foreach (var handler in Handlers)
//            {
//                handler.DefaultDispellableStatus = FightDispellableEnum.DISPELLABLE_BY_DEATH;
//                handler.Apply();
//            }
//        }
//    }

//    [SpellCastHandler(SpellIdEnum.DORIGAMI_17309)]
//    public class DofusDorigamiTwo : DefaultSpellCastHandler
//    {
//        public DofusDorigamiTwo(SpellCastInformations cast) : base(cast) { }

//        public override void Execute()
//        {
//            if (this.Caster.Fight.TimeLine.RoundNumber <= 5)
//            {
//                foreach (var handler in Handlers)
//                {
//                    handler.DefaultDispellableStatus = FightDispellableEnum.DISPELLABLE_BY_DEATH;
//                    handler.SetAffectedActor(this.Caster);
//                    handler.Apply();
//                }
//            }
//        }
//    }

//    [SpellCastHandler(SpellIdEnum.DORIGAMI_17310)]
//    public class DofusDorigamiThree : DefaultSpellCastHandler
//    {
//        public DofusDorigamiThree(SpellCastInformations cast) : base(cast) { }

//        public override void Execute()
//        {
//            var Target = Fight.GetOneFighter(TargetedCell);

//            foreach (var handler in Handlers)
//            {
//                handler.DefaultDispellableStatus = FightDispellableEnum.DISPELLABLE_BY_DEATH;
//                handler.SetAffectedActor(Target);
//                handler.Apply();
//            }
//        }
//    }
//}