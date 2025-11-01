//using Stump.DofusProtocol.Enums;
//using Stump.Server.WorldServer.Game.Fights;

//namespace Stump.Server.WorldServer.Game.Spells.Casts
//{
//    [SpellCastHandler(SpellIdEnum.EBONY_DOFUS_18629)]
//    public class DofusEbony : DefaultSpellCastHandler
//    {
//        public DofusEbony(SpellCastInformations cast) : base(cast) { }

//        public override void Execute()
//        {
//            foreach (var handler in Handlers)
//            {
//                handler.DefaultDispellableStatus = FightDispellableEnum.DISPELLABLE_BY_DEATH;
//                handler.Apply();
//            }
//        }
//    }

//    [SpellCastHandler(SpellIdEnum.EBONY_DOFUS_18630)]
//    public class DofusEbonyOne : DefaultSpellCastHandler
//    {
//        public DofusEbonyOne(SpellCastInformations cast) : base(cast) { }

//        public override void Execute()
//        {
//            foreach (var handler in Handlers)
//            {
//                handler.DefaultDispellableStatus = FightDispellableEnum.DISPELLABLE_BY_DEATH;
//                handler.Apply();
//            }
//        }
//    }

//    [SpellCastHandler(SpellIdEnum.EBONY_DOFUS_18631)]
//    public class DofusEbonyTwo : DefaultSpellCastHandler
//    {
//        public DofusEbonyTwo(SpellCastInformations cast) : base(cast) { }

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

//    [SpellCastHandler(SpellIdEnum.EBONY_DOFUS_18632)]
//    public class DofusEbonyThree : DefaultSpellCastHandler
//    {
//        public DofusEbonyThree(SpellCastInformations cast) : base(cast) { }

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

//    [SpellCastHandler(SpellIdEnum.EBONY_DOFUS_18643)]
//    public class DofusEbonyFor : DefaultSpellCastHandler
//    {
//        public DofusEbonyFor(SpellCastInformations cast) : base(cast) { }

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

//    /// <summary>
//    /// Spells que estimula a Spell com os Venenos
//    /// </summary>
//    [SpellCastHandler(SpellIdEnum.EBONY_DOFUS_18644)]
//    public class DofusEbonyFive : DefaultSpellCastHandler
//    {
//        public DofusEbonyFive(SpellCastInformations cast) : base(cast) { }

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

//    /// <summary>
//    /// Spell que contém todos os os Venenos
//    /// </summary>
//    [SpellCastHandler(SpellIdEnum.EBONY_DOFUS_18645)]
//    public class DofusEbonySix : DefaultSpellCastHandler
//    {
//        public DofusEbonySix(SpellCastInformations cast) : base(cast) { }

//        public override void Execute()
//        {
//            foreach (var handler in Handlers)
//            {
//                handler.DefaultDispellableStatus = FightDispellableEnum.DISPELLABLE_BY_DEATH;
//                handler.Apply();
//            }
//        }
//    }
//}