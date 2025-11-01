//using Stump.DofusProtocol.Enums;
//using Stump.Server.WorldServer.Game.Fights;

//namespace Stump.Server.WorldServer.Game.Spells.Casts.Panda
//{
//    [SpellCastHandler(SpellIdEnum.PESTILENTIAL_FOG_15975)]
//    public class PestilentialHandler : DefaultSpellCastHandler
//    {
//        public PestilentialHandler(SpellCastInformations cast) : base(cast)
//        { }

//        public override void Execute()
//        {
//            if (!m_initialized)
//                Initialize();

//            Handlers[0].AddAffectedActor(Caster);

//            foreach (var handler in Handlers)
//            {
//                handler.Apply();
//            }
//        }
//    }
//}