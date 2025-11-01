using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Database;
using Stump.Server.WorldServer.Database.Arena;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Npcs;
using Stump.Server.WorldServer.Game.Arena;
using Stump.Server.WorldServer.Game.Breeds;
using System.Collections.Generic;
using System.Linq;

namespace Stump.Server.WorldServer.Database.Npcs.Replies
{
    [Discriminator("Championship", typeof(NpcReply), new System.Type[]
    {
        typeof(NpcReplyRecord)
    })]

    public class ArenaChampionshipReply : NpcReply
    {
        private List<string> BlockPlayersEmail = new List<string>()
        {

        };

        private List<string> BlockPlayersMac = new List<string>()
        {

        };

        public ArenaChampionshipReply(NpcReplyRecord record) : base(record)
        { }

        public override bool Execute(Npc npc, Character character)
        {
            var HasAllRecord = ArenaChampsManager.Instance.GetAllRecord();
            var HasRecord = ArenaChampsManager.Instance.GetOwnerRecord(character.Id);
            var HasIpRecord = ArenaChampsManager.Instance.GetIpOwnerRecord(character.Client.IP);

            if (BlockPlayersEmail.Contains(character.Account.Email) || BlockPlayersMac.Contains(character.Account.LastHardwareId))
            {
                character.SendServerMessageLang
                    (
                    "Informamos que você foi marcado como banido dos eventos organizados pela nossa equipe. Caso discorde dessa punição, recomendamos que abra um ticket de suporte para que possamos avaliar o seu caso com maior detalhe.",
                    "You have been marked as banned from the events organized by our staff. If you disagree with this punishment, we encourage you to open a support ticket so that we can assess your case in more detail.",
                    "Se le ha marcado como excluido/a de los eventos organizados por nuestro equipo. Si no está de acuerdo con esta sanción, le recomendamos que abra un ticket de soporte para que podamos evaluar su caso con mayor detalle.",
                    "Vous avez été marqué(e) comme exclu(e) des événements organisés par notre équipe. Si vous n'êtes pas d'accord avec cette sanction, nous vous recommandons d'ouvrir un ticket de support afin que nous puissions examiner votre cas plus en détail."
                    );

                return false;
            }

            if (HasRecord.Any() || HasIpRecord.Any())
            {
                SendRegistrationErrorMessage(character);
                return false;
            }

            if (HasAllRecord.Count() >= 8)
            {
                SendRegistrationLimitErrorMessage(character);
                return false;
            }

            if (character.Inventory.Kamas < 30000000)
            {
                character.SendServerMessageLang
                    (
                    "Você não possui kamas suficiente para se registrar nesse evento.",
                    "You do not have enough kamas to register for this event.",
                    "No tienes suficientes kamas para registrarte en este evento.",
                    "Vous n'avez pas assez de kamas pour vous inscrire à cet événement."
                    );
                return false;
            }

            if (character.Level < 190)
            {
                SendLevelRequirementErrorMessage(character);
                return false;
            }

            character.Inventory.SubKamas(30000000);
            character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 46, 30000000);

            ArenaChampsManager.Instance.AddRecord(new ArenaChampionship
            {
                OwnerId = character.Id,
                OwnerName = character.NameClean,
                Level = character.Level,
                Classe = BreedManager.Instance.GetBreed(character.BreedId).ShortName,
                Email = character.Account.Email,
                Ip = character.Client.IP,
                Date = System.DateTime.Now,
                IsNew = true
            });

            SendRegistrationConfirmationMessage(character);

            return true;
        }

        private void SendRegistrationConfirmationMessage(Character character)
        {
            character.SendServerMessageLang(
                "Você se registrou para participar do evento. Favor aguardar nosso comunicado no Discord.",
                "You have registered to attend the event. Please wait for our announcement on Discord.",
                "Te has registrado para asistir al evento. Espere nuestro anuncio en Discord.",
                "Vous vous êtes inscrit pour assister à l'événement. Veuillez attendre notre annonce sur Discord."
            );
        }

        private void SendRegistrationErrorMessage(Character character)
        {
            character.SendServerMessageLang(
                "Você já se registrou para participar do evento. Favor aguardar nosso comunicado no Discord.",
                "You have already registered to participate in the event. Please wait for our announcement on Discord.",
                "Ya te has registrado para participar en el evento. Espere nuestro anuncio en Discord.",
                "Vous êtes déjà inscrit pour participer à l'événement. Veuillez attendre notre annonce sur Discord."
            );
        }

        private void SendLevelRequirementErrorMessage(Character character)
        {
            character.SendServerMessageLang(
                "Você precisa possuir nível 190 ou superior para poder participar do evento.",
                "You need to be level 190 or higher to participate in the event.",
                "Debes tener nivel 190 o superior para participar en el evento.",
                "Vous devez être de niveau 190 ou supérieur pour participer à l'événement."
            );
        }

        private void SendRegistrationLimitErrorMessage(Character character)
        {
            character.SendServerMessageLang(
                "Já alcançamos o limite de inscrições para este campeonato. Fique à vontade para tentar novamente na próxima edição.",
                "We have reached the maximum number of registered participants for this tournament. Feel free to try again in the next tournament.",
                "Hemos alcanzado el número máximo de participantes inscritos para este campeonato. No dudes en intentarlo de nuevo en el próximo campeonato.",
                "Nous avons atteint le nombre maximum de participants inscrits pour ce tournoi. N'hésitez pas à réessayer lors du prochain tournoi."
            );
        }
    }
}
