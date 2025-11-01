using Stump.Server.WorldServer.Commands.Commands.Patterns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stump.Server.WorldServer.Commands.Trigger;

namespace Commands
{
    class Notification_Vote : InGameCommand
    {
        public Notification_Vote()
        {
            Aliases = new string[] { "Vote" };
            Description = "Cette commande permet d'activer ou de désactiver les notifications de vote. Faites .vote on pour les activer, et .vote off pour les désactiver.";
            RequiredRole = Stump.DofusProtocol.Enums.RoleEnum.Player;
            AddParameter<string>("On/Off");
        }
        public override void Execute(GameTrigger trigger)
        {
            var character = trigger.Character;
            if (character != null)
            {
                var saisie = trigger.Get<string>("On/Off");
                switch (saisie.ToUpper())
                {
                    case "ON":
                        if (character.Record.BoteVoteNotification != 0)
                        {
                            character.Record.BoteVoteNotification = 0;
                            character.SendServerMessage("La notification de vote est désormais activée.", System.Drawing.Color.Red);
                        }
                        else
                        {
                            character.SendServerMessage("La notification de vote est déjà activée.", System.Drawing.Color.Red);
                        }
                        break;
                    case "OFF":
                        if (character.Record.BoteVoteNotification == 0)
                        {
                            character.Record.BoteVoteNotification = 1;
                            character.SendServerMessage("La notification de vote est désormais désactivée.", System.Drawing.Color.Red);
                        }
                        else
                        {
                            character.SendServerMessage("La notification de vote est déjà désactivée.", System.Drawing.Color.Red);
                        }
                        break;
                    default:
                        character.SendServerMessage("La saisie est incorrecte. Pour activer la notification de vote, veuillez écrire .vote on. Pour la désactiver, veuillez écrire .vote off.", System.Drawing.Color.Red);
                        break;
                }
            }
        }
    }
}
