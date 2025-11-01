using Stump.Core.Reflection;

using System;
using System.Threading.Tasks;
using Stump.Core.Threading;
using Stump.Core.Collections;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game;
using System.Drawing;
using Stump.Server.BaseServer.Initialization;

namespace Game.Vote
{
    public class AnnounceManager : Singleton<AnnounceManager>
    {
        #region Valeur 
        private static Task _queueRefresherTask;
        private static int _queueRefresherElapsedTime = 1500000;//1800000
        private static int _queueRefresherElapsedTime2 = 2000000;
        private static int _queueResresherElaspedTime3 = 3600000;
        private static int _queueResresherElaspedTime4 = 4200000;


        private static string _displayFirstAnnounce = string.Format("ANNONCE : Un mode VIP est disponible sur notre site web! CLIQUEZ <u><a href=\"http://brumaire.be/brumaires/\" >Ici.</a></u> ");
        private static string _displaySecondAnnounce = string.Format("ANNONCE : Les Points boutique lors des votes effectués ont étaient fixé à 20 pb/vote ! Pour voter, cliquez <u><a href=\"http://brumaire.be/brumaires/\" >Ici.</a></u> ");
        private static string _displayTresAnnounce = string.Format("ANNONCE: Stock de point boutique en rupture ? ACHETEZ-EN ! <u><a href=\"http://brumaire.be/brumaires/\" >Ici.</a></u>");

        private readonly object m_sync = new object();

        private ConcurrentList<Character> m_characters;
        #endregion

        private AnnounceManager()
        {
            this.m_characters = new ConcurrentList<Character>();
        }

        #region Initialization
        [Initialization(InitializationPass.First)]
        private static void Initialize()
        {
            AnnounceManager._queueRefresherTask = Task.Factory.StartNewDelayed(AnnounceManager._queueRefresherElapsedTime, new Action(Singleton<AnnounceManager>.Instance.SendAnnounce));
            AnnounceManager._queueRefresherTask = Task.Factory.StartNewDelayed(AnnounceManager._queueRefresherElapsedTime2, new Action(Singleton<AnnounceManager>.Instance.SendSecondAnnounce));
            AnnounceManager._queueRefresherTask = Task.Factory.StartNewDelayed(AnnounceManager._queueResresherElaspedTime3, new Action(Singleton<AnnounceManager>.Instance.SendTresAnnounce));
            AnnounceManager._queueRefresherTask = Task.Factory.StartNewDelayed(AnnounceManager._queueResresherElaspedTime4, new Action(Singleton<AnnounceManager>.Instance.SendQuadAnnounce));

        }
        #endregion
        #region SendAnnounce
        private void SendAnnounce()
        {
            try
            {
                Singleton<World>.Instance.SendAnnounce(_displayFirstAnnounce, Color.Red);

            }
            finally
            {
                AnnounceManager._queueRefresherTask = Task.Factory.StartNewDelayed(AnnounceManager._queueRefresherElapsedTime, new Action(Singleton<AnnounceManager>.Instance.SendAnnounce));

            }
        }
        private void SendSecondAnnounce()
        {
            try
            {
                Singleton<World>.Instance.SendAnnounce(_displaySecondAnnounce, Color.Red);

            }
            finally
            {
                AnnounceManager._queueRefresherTask = Task.Factory.StartNewDelayed(AnnounceManager._queueRefresherElapsedTime2, new Action(Singleton<AnnounceManager>.Instance.SendSecondAnnounce));

            }
        }
        private void SendTresAnnounce()
        {
            try
            {
                Singleton<World>.Instance.SendAnnounce(_displayTresAnnounce, Color.Red);

            }
            finally
            {
                AnnounceManager._queueRefresherTask = Task.Factory.StartNewDelayed(AnnounceManager._queueResresherElaspedTime3, new Action(Singleton<AnnounceManager>.Instance.SendTresAnnounce));

            }
        }
        private void SendQuadAnnounce()
        {
            try
            {
                Singleton<World>.Instance.SendAnnounce(_displayTresAnnounce, Color.Red);

            }
            finally
            {
                AnnounceManager._queueRefresherTask = Task.Factory.StartNewDelayed(AnnounceManager._queueResresherElaspedTime4, new Action(Singleton<AnnounceManager>.Instance.SendQuadAnnounce));
            }
        }
        #endregion
        #region Enter&Leave
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
        #endregion
    }
}
