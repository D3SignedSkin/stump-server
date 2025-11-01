//using Stump.DofusProtocol.Enums;
//using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
//using Stump.Server.WorldServer.Game.Actors.RolePlay.Monsters;
//using Stump.Server.WorldServer.Game.Effects.Instances;
//using Stump.Server.WorldServer.Game.Interactives.Skills;
//using Stump.Server.WorldServer.Game.Items.Player;
//using System.Linq;

//namespace Stump.Server.WorldServer.Game.Effects.Handlers.Usables
//{
//    [EffectHandler(EffectsEnum.Effect_MaximumStar)]
//    public class StarPotion : UsableEffectHandler
//    {
//        public StarPotion(EffectBase effect, Character target, BasePlayerItem item) : base(effect, target, item)
//        {
//        }

//        protected override bool InternalApply()
//        {
//            var integerEffect = Effect.GenerateEffect(EffectGenerationContext.Item) as EffectInteger;

//            if (integerEffect == null)
//                return false;

//            var Amount = (short)(integerEffect.Value * NumberOfUses);

//            AddStar(Target, Amount);

//            Target.SendServerMessageLang("Você invocou o poder de atribuir estrelas com sucesso.", "You have successfully invoked the power to assign stars.", "Has invocado con éxito el poder de asignar estrellas.", "Vous avez invoqué avec succès le pouvoir d'attribuer des étoiles.");

//            UsedItems = NumberOfUses;

//            return true;
//        }

//        private void AddStar(Character Owner, short amount)
//        {
//            try
//            {
//                if (Owner.UserGroup.Role >= RoleEnum.Vip)
//                {
//                    #region Área destinada aos Mobs do Servidor (Versão 2.51 não possui Star em MOBS)
//                    //foreach (var monster in World.Instance.GetMaps().SelectMany(x => x.Actors.OfType<MonsterGroup>()).Where(y => y.SubArea.Id == Target.Map.SubArea.Id && y.AgeBonus < 200))
//                    //{
//                    //    if ((monster.AgeBonus + amount) <= 200)
//                    //    {
//                    //        monster.AgeBonus += amount;
//                    //    }
//                    //    else if (monster.AgeBonus < 200)
//                    //    {
//                    //        monster.AgeBonus += (short)(amount - monster.AgeBonus);
//                    //    }

//                    //    monster.Map.ForEach(x => x.Client.Send(x.Client.Character.Map.GetMapComplementaryInformationsDataMessage(x.Client.Character)));
//                    //}
//                    #endregion

//                    #region Área destinada aos Interativos do Servidor
//                    foreach (var m_harvest in World.Instance.GetMaps().SelectMany(z => z.GetInteractiveObjects().SelectMany(x => x.GetSkills().Where(y => y is SkillHarvest).Select(y => y as SkillHarvest).Where(y => !y.Harvested && y.AgeBonus < 200)).Distinct()).Where(y => y.InteractiveObject.SubArea.Id == Target.Map.SubArea.Id))
//                    {
//                        if ((m_harvest.AgeBonus + amount) <= 200)
//                        {
//                            m_harvest.AgeBonus += amount;
//                        }
//                        else if (m_harvest.AgeBonus < 200)
//                        {
//                            m_harvest.AgeBonus += (short)(amount - m_harvest.AgeBonus);
//                        }

//                        m_harvest.InteractiveObject.Map.ForEach(x => x.Client.Send(x.Client.Character.Map.GetMapComplementaryInformationsDataMessage(x.Client.Character)));
//                    }
//                    #endregion
//                }
//                else
//                {
//                    #region Área destinada aos Mobs do Servidor (Versão 2.51 não possui Star em MOBS)
//                    //foreach (var monster in World.Instance.GetMaps().SelectMany(x => x.Actors.OfType<MonsterGroup>()).Where(y => y.Map.Id == Owner.Map.Id && y.AgeBonus < 200))
//                    //{
//                    //    if ((monster.AgeBonus + amount) <= 200)
//                    //    {
//                    //        monster.AgeBonus += amount;
//                    //    }
//                    //    else if (monster.AgeBonus < 200)
//                    //    {
//                    //        monster.AgeBonus += (short)(amount - monster.AgeBonus);
//                    //    }

//                    //    monster.Map.ForEach(x => x.Client.Send(x.Client.Character.Map.GetMapComplementaryInformationsDataMessage(x.Client.Character)));
//                    //}
//                    #endregion

//                    #region Área destinada aos Interativos do Servidor
//                    foreach (var m_harvest in World.Instance.GetMaps().SelectMany(z => z.GetInteractiveObjects().SelectMany(x => x.GetSkills().Where(y => y is SkillHarvest).Select(y => y as SkillHarvest).Where(y => !y.Harvested && y.AgeBonus < 200)).Distinct()).Where(y => y.InteractiveObject.Spawn.MapId == Owner.Map.Id))
//                    {
//                        if ((m_harvest.AgeBonus + amount) <= 200)
//                        {
//                            m_harvest.AgeBonus += amount;
//                        }
//                        else if (m_harvest.AgeBonus < 200)
//                        {
//                            m_harvest.AgeBonus += (short)(amount - m_harvest.AgeBonus);
//                        }

//                        m_harvest.InteractiveObject.Map.ForEach(x => x.Client.Send(x.Client.Character.Map.GetMapComplementaryInformationsDataMessage(x.Client.Character)));
//                    }
//                    #endregion
//                }
//            }
//            catch { };
//        }
//    }
//}