using Stump.Core.Reflection;
using Stump.Server.BaseServer.Initialization;
using System;
using System.Linq;
using System.Threading.Tasks;
using Stump.Core.Threading;
using Stump.Core.Collections;
using Stump.Server.WorldServer.Core.IPC;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.DofusProtocol.Messages;
using Stump.Server.WorldServer.Game;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Monsters;
using Stump.Server.WorldServer.Game.Interactives.Skills;
using Stump.DofusProtocol.Enums;

namespace Game.Vote
{
    public class MonsterStarLoop : Singleton<MonsterStarLoop>
    {
        // FIELDS
        private static Task _queueRefresherTask;
        private readonly object m_sync = new object();
        private ConcurrentList<Character> m_characters;
        private static int _queueRefresherElapsedTime = 180000; // Padrão a metade desse valor 600000
        // PROPERTIES

        // CONSTRUCTORS
        public MonsterStarLoop()
        {
            this.m_characters = new ConcurrentList<Character>();
        }

        // METHODS
        [Initialization(InitializationPass.First)]
        private static void Initialize()
        {
            MonsterStarLoop._queueRefresherTask = Task.Factory.StartNewDelayed(MonsterStarLoop._queueRefresherElapsedTime, new Action(Singleton<MonsterStarLoop>.Instance.StartLoop));
        }

        private void StartLoop()
        {
            try
            {
                #region Área destinada aos Mobs do Servidor (Desativado v2.61)
                //foreach (var monster in World.Instance.GetMaps().SelectMany(x => x.Actors.OfType<MonsterGroup>()))
                //{
                //    if ((monster.AgeBonus + 10) < 200)
                //    {
                //        monster.AgeBonus += 10;
                //    }
                //    else if (monster.AgeBonus < 200)
                //    {
                //        monster.AgeBonus += (short)(10 - monster.AgeBonus);
                //    }

                //    monster.Map.ForEach(x => x.Client.Send(x.Client.Character.Map.GetMapComplementaryInformationsDataMessage(x.Client.Character)));
                //}
                #endregion

                #region Área destinada aos Interativos do Servidor
                var unharvestedSkillHarvests = World.Instance.GetMaps()
                    .SelectMany(map => map.GetInteractiveObjects())
                    .SelectMany(obj => obj.GetSkills())
                    .OfType<SkillHarvest>()
                    .Where(skillHarvest => !skillHarvest.Harvested && skillHarvest.InteractiveObject.State == InteractiveStateEnum.STATE_NORMAL)
                    .Distinct();

                foreach (var m_harvest in unharvestedSkillHarvests)
                {
                    if ((m_harvest.AgeBonus + 10) < 200)
                    {
                        m_harvest.AgeBonus += 10;
                    }
                    else if (m_harvest.AgeBonus < 200)
                    {
                        m_harvest.AgeBonus += (short)(10 - m_harvest.AgeBonus);
                    }

                    //TODO - Colheita buga quando o jogador está efetuando ela e dispara a atualização pelo script abaixo.
                    //m_harvest.InteractiveObject.Map.ForEach(x => x.Client.Send(x.Client.Character.Map.GetMapComplementaryInformationsDataMessage(x.Client.Character)));
                }
                #endregion

                //World.Instance.SendAnnounceLang("<b>Servidor :</b> As estrelas dos monstros e das colheitas foram aumentadas para FULL Star! Bom jogo em Hydra.", "<b>Server :</b> The monsters' and harvest' stars have been increased by Full Star! Good game in Hydra.", "<b>Servidor :</b> ¡Las estrellas de los monstruos y de las cosechas se incrementaron en Full Star! Buen juego en Hydra.", "<b>Serveur:</b> Les étoiles de monstres et de récolte ont été augmentées de Full Star ! Bon jeu à Hydra.", System.Drawing.Color.Olive);
            }
            finally
            {
                MonsterStarLoop._queueRefresherTask = Task.Factory.StartNewDelayed(MonsterStarLoop._queueRefresherElapsedTime, new Action(Singleton<MonsterStarLoop>.Instance.StartLoop));
            }
        }

        public void Enter(Character client)
        {
            lock (this.m_sync)
            {
                this.m_characters.Add(client);
            }
        }

        public void Leave(Character client)
        {
            lock (this.m_sync)
            {
                this.m_characters.Remove(client);
            }
        }
    }
}
