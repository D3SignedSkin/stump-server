//using Stump.DofusProtocol.Enums;
//using Stump.Server.WorldServer.Game.Fights;

//namespace Stump.Server.WorldServer.Game.Spells.Casts
//{
//    [SpellCastHandler(SpellIdEnum.DOMAKURO_17004)]
//    public class DofusDomakuro : DefaultSpellCastHandler
//    {
//        public DofusDomakuro(SpellCastInformations cast) : base(cast) { }

//        public override void Execute()
//        {
//            foreach (var handler in Handlers)
//            {
//                handler.DefaultDispellableStatus = FightDispellableEnum.DISPELLABLE_BY_DEATH;
//                handler.Apply();
//            }
//        }
//    }

//    [SpellCastHandler(SpellIdEnum.DOMAKURO_17005)]
//    public class DofusDomakuroOne : DefaultSpellCastHandler
//    {
//        public DofusDomakuroOne(SpellCastInformations cast) : base(cast) { }

//        public override void Execute()
//        {
//            foreach (var handler in Handlers)
//            {
//                handler.DefaultDispellableStatus = FightDispellableEnum.DISPELLABLE_BY_DEATH;
//                handler.Apply();
//            }
//        }
//    }

//    [SpellCastHandler(SpellIdEnum.DOMAKURO_17006)]
//    public class DofusDomakuroTwo : DefaultSpellCastHandler
//    {
//        public DofusDomakuroTwo(SpellCastInformations cast) : base(cast) { }

//        public override void Execute()
//        {
//            foreach (var handler in Handlers)
//            {
//                handler.DefaultDispellableStatus = FightDispellableEnum.DISPELLABLE_BY_DEATH;
//                handler.SetAffectedActor(this.Caster);
//                handler.Apply();
//            }
//        }
//    }

//    [SpellCastHandler(SpellIdEnum.DOMAKURO_17007)]
//    public class DofusDomakuroThree : DefaultSpellCastHandler
//    {
//        public DofusDomakuroThree(SpellCastInformations cast) : base(cast) { }

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

//    [SpellCastHandler(SpellIdEnum.DOMAKURO_17008)]
//    public class DofusDomakuroFor : DefaultSpellCastHandler
//    {
//        public DofusDomakuroFor(SpellCastInformations cast) : base(cast) { }

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

//    [SpellCastHandler(SpellIdEnum.DOMAKURO_17009)]
//    public class DofusDomakuroFive : DefaultSpellCastHandler
//    {
//        public DofusDomakuroFive(SpellCastInformations cast) : base(cast) { }

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