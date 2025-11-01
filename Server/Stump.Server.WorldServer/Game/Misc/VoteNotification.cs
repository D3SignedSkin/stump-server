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

namespace Game.Vote
{
    public class VoteManager : Singleton<VoteManager>
    {
        // FIELDS
        private static Task _queueRefresherTask;
        private static int _queueRefresherElapsedTime = 10000;//10000

        private static string _displayMessage = string.Format("Você não votou por pelo menos 3 horas, CLIQUE <u><a href=\"http://dfHydra.online/pt/votar\" >Aqui.</a></u> PARA VOTAR! Para desativar a notificação de voto, escreva : .Vote off");
        private static string _displayMessagefr = string.Format("Vous n'avez pas voté depuis au moins 3 heures, CLIQUEZ <u><a href=\"http://dfHydra.online/fr/vote\" >Ici.</a></u> POUR VOTER ! Pour désactiver la notification de vote écrivez : .Vote off");
        private static string _displayMessagees = string.Format("Usted no votó por por lo menos 3 horas, CLICK <u><a href=\"http://dfHydra.online/es/votar\" >Aquí.</a></u> ¡PARA VOTAR ! Para desactivar la notificación de voto, escriba: .Vote off");
        private static string _displayMessageen = string.Format("You have not voted for at least 3 hours, CLICK <u><a href=\"http://dfHydra.online/en/vote\" >Here.</a></u> TO VOTE ! To disable voting notification type: .Vote off");

        private readonly object m_sync = new object();

        private ConcurrentList<Character> m_characters;
        // PROPERTIES

        // CONSTRUCTORS
        public VoteManager()
        {
            this.m_characters = new ConcurrentList<Character>();
        }

        // METHODS
        [Initialization(InitializationPass.First)]
        private static void Initialize()
        {
            VoteManager._queueRefresherTask = Task.Factory.StartNewDelayed(VoteManager._queueRefresherElapsedTime, new Action(Singleton<VoteManager>.Instance.CheckVotes));
        }
        private void CheckVotes()
        {
            try
            {
                var now = DateTime.Now;
                if (IPCAccessor.Instance.IsConnected && m_characters.Count > 0)
                {
                    foreach (var character in m_characters.Where(x => x != null && !x.IsFighting() && x.IsInWorld && !x.IsGameMaster() && x.Record.BoteVoteNotification == 0 && x.Client != null
                    && (!x.Account.LastVote.HasValue || (now - x.Account.LastVote.Value).TotalHours >= 3)))
                        switch (character.Account.Lang)
                        {
                            case "fr":
                                character.Client.Send(new NotificationByServerMessage(30, new string[] { _displayMessagefr }, true));
                                break;
                            case "es":
                                character.Client.Send(new NotificationByServerMessage(30, new string[] { _displayMessagees }, true));
                                break;
                            case "en":
                                character.Client.Send(new NotificationByServerMessage(30, new string[] { _displayMessageen }, true));
                                break;
                            default:
                                character.Client.Send(new NotificationByServerMessage(30, new string[] { _displayMessage }, true));
                                break;
                        }
                   
                }
            }
            finally
            {
                VoteManager._queueRefresherTask = Task.Factory.StartNewDelayed(VoteManager._queueRefresherElapsedTime, new Action(Singleton<VoteManager>.Instance.CheckVotes));
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
