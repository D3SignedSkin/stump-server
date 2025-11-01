//using Stump.DofusProtocol.Enums;
//using Stump.Server.WorldServer.Game.Actors.Fight;
//using Stump.Server.WorldServer.Game.Effects.Handlers.Spells.States;
//using Stump.Server.WorldServer.Game.Effects.Instances;
//using Stump.Server.WorldServer.Game.Fights;
//using Stump.Server.WorldServer.Game.Fights.Buffs;

//namespace Stump.Server.WorldServer.Game.Spells.Casts.Boss
//{
//    [SpellCastHandler(SpellIdEnum.BOOM_PRETTY_BOOM_1071)]//spell kimbo
//    public class BoomPrettyBoomCastHandler : DefaultSpellCastHandler
//    {
//        public BoomPrettyBoomCastHandler(SpellCastInformations cast) : base(cast)
//        { }

//        public override void Execute()
//        {
//            if (!m_initialized)
//                Initialize();

//            var stateodd = SpellManager.Instance.GetSpellState((uint)SpellStatesEnum.GLYPHE_IMPAIRE_29);
//            var stateever = SpellManager.Instance.GetSpellState((uint)SpellStatesEnum.GLYPHE_PAIRE_30);
//            var test1 = Caster.GetStates();

//            if (Caster.HasState(stateever))
//            {
//                Handlers[2].Apply();
//                Handlers[1].Apply();
//            }
//            if (Caster.HasState(stateodd))
//            {
//                Handlers[8].Apply();
//                Handlers[7].Apply();
//            }


//            Handlers[0].AddTriggerBuff(Caster, BuffTriggerType.OnDamagedFire, efeito);
//            Handlers[2].AddTriggerBuff(Caster, BuffTriggerType.OnDamagedFire, efeito);

//            Handlers[3].AddTriggerBuff(Caster, BuffTriggerType.OnDamagedAir, efeito);
//            Handlers[5].AddTriggerBuff(Caster, BuffTriggerType.OnDamagedAir, efeito);

//            Handlers[6].AddTriggerBuff(Caster, BuffTriggerType.OnDamagedEarth, efeito);
//            Handlers[8].AddTriggerBuff(Caster, BuffTriggerType.OnDamagedEarth, efeito);

//            Handlers[9].AddTriggerBuff(Caster, BuffTriggerType.OnDamagedWater, efeito);
//            Handlers[11].AddTriggerBuff(Caster, BuffTriggerType.OnDamagedWater, efeito);

//            Handlers[12].Apply();

//            //    //Handlers[0].Apply();      //remove o estado do glifo impar
//            //    // Handlers[1].Apply(); //remove o estado do glifo par
//            //    //Handlers[2].Apply(); glifo par

//            //    //Handlers[3].Apply();//remove o estado do glifo impar
//            //    //Handlers[4].Apply();//remove o estado do glifo par
//            //    //Handlers[5].Apply();glifo par

//            //    //Handlers[6].Apply();//remove o estado do glifo par
//            //    //Handlers[7].Apply();//remove o estado do glifo impar
//            //    //Handlers[8].Apply();//glifo impar

//            //    //Handlers[9].Apply();//remove o estado do glifo par
//            //    //Handlers[10].Apply();//remove o estado do glifo impar
//            //    //Handlers[11].Apply();//glifo impar


//        }
//        private void efeito(TriggerBuff buff, FightActor trigerrer, BuffTriggerType trigger, object token)
//        {

//            if (buff.EffectHandler.Effect.EffectId == EffectsEnum.Effect_AddState && (trigger == BuffTriggerType.OnDamagedFire || trigger == BuffTriggerType.OnDamagedAir))
//            {
//                var actorBuffId = Caster.PopNextBuffId();
//                var addStateHandler = new AddState(new EffectDice(EffectsEnum.Effect_AddState, (short)SpellStatesEnum.GLYPHE_PAIRE_30, 0, 0), Caster, null, Caster.Cell, false);
//                var actorBuff = new StateBuff(actorBuffId, Caster, Caster, addStateHandler, Spell, FightDispellableEnum.DISPELLABLE_BY_DEATH, SpellManager.Instance.GetSpellState((uint)SpellStatesEnum.GLYPHE_PAIRE_30))
//                {
//                    //Duration = (short)buff.EffectHandler.Effect.Duration
//                    Duration = 0,
//                    Delay = 1
//                };

//                Caster.AddBuff(actorBuff, true);

//            }
//            else if (buff.EffectHandler.Effect.EffectId == EffectsEnum.Effect_AddState && (trigger == BuffTriggerType.OnDamagedEarth || trigger == BuffTriggerType.OnDamagedWater))
//            {
//                var actorBuffId = Caster.PopNextBuffId();
//                var addStateHandler = new AddState(new EffectDice(EffectsEnum.Effect_AddState, (short)SpellStatesEnum.GLYPHE_IMPAIRE_29, 0, 0), Caster, null, Caster.Cell, false);
//                var actorBuff = new StateBuff(actorBuffId, Caster, Caster, addStateHandler, Spell, FightDispellableEnum.DISPELLABLE_BY_DEATH, SpellManager.Instance.GetSpellState((uint)SpellStatesEnum.GLYPHE_IMPAIRE_29))
//                {
//                    //Duration = (short)buff.EffectHandler.Effect.Duration
//                    Duration = 0,
//                    Delay = 1
//                };

//                Caster.AddBuff(actorBuff, true);
//            }

//            else
//            {
//                buff.EffectHandler.Delay = 1;
//                buff.EffectHandler.Duration = 0;
//                buff.EffectHandler.Apply();
//            }
//        }
//    }
//}