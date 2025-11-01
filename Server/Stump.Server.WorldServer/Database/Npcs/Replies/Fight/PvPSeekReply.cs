using Stump.Core.Collections;
using Stump.Core.Extensions;
using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Database;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Npcs;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Items;
using Stump.Server.WorldServer.Game.Seeklog;
using System.Drawing;
using System.Linq;
using System.Threading;

namespace Stump.Server.WorldServer.Database.Npcs.Replies
{
    [Discriminator("PvPSeek", typeof(NpcReply), typeof(NpcReplyRecord))]
    public class PvPSeekReply : NpcReply
    {
        private TimedStack<Pair<int, int>> m_PvPSeekkHistoryFree = new TimedStack<Pair<int, int>>(60 * 3);

        private TimedStack<Pair<int, int>> m_PvPSeekkHistoryVip = new TimedStack<Pair<int, int>>(60 * 2);

        private TimedStack<Pair<int, int>> m_PvPSeekkHistoryGoldVip = new TimedStack<Pair<int, int>>(60 * 1);

        private Timer BattlefieldTimer;
        private int retryBattleField;

        public PvPSeekReply(NpcReplyRecord record) : base(record)
        {
        }

        public override bool Execute(Npc npc, Character character)
        {
            if (!base.Execute(npc, character))
                return false;

            //Check if player already get a contract in the last 10 mins
            m_PvPSeekkHistoryFree.Clean();
            m_PvPSeekkHistoryVip.Clean();
            m_PvPSeekkHistoryGoldVip.Clean();

            #region Mensagens de Bloqueio de Time

            if (character.Account.IsSubscribe == false && m_PvPSeekkHistoryFree.Any(x => x.First.First == character.Id))
            {
                switch (character.Account.Lang)
                {
                    case "fr":
                        character.SendServerMessage("Vous avez eu une mission il y a moins de <b>30 minutes</b>, réessayez plus tard.");
                        break;
                    case "es":
                        character.SendServerMessage("Ya has tenido una misión hace menos de <b>30 minutos</b>, vuelve a intentarlo más tarde.");
                        break;
                    case "en":
                        character.SendServerMessage("You've had a mission less than <b>30 minutes</b> ago, try again later.");
                        break;
                    default:
                        character.SendServerMessage("Você teve uma missão há menos de <b>30 minutos</b>, tente novamente mais tarde.");
                        break;
                }

                return false;
            }

            if (character.Client.UserGroup.Role == RoleEnum.Vip && character.Account.IsSubscribe == true && m_PvPSeekkHistoryVip.Any(x => x.First.First == character.Id))
            {
                switch (character.Account.Lang)
                {
                    case "fr":
                        character.SendServerMessage("Vous avez eu une mission il y a moins de <b>20 minutes</b>, réessayez plus tard.");
                        break;
                    case "es":
                        character.SendServerMessage("Ya has tenido una misión hace menos de <b>20 minutos</b>, vuelve a intentarlo más tarde.");
                        break;
                    case "en":
                        character.SendServerMessage("You've had a mission less than <b>20 minutes</b> ago, try again later.");
                        break;
                    default:
                        character.SendServerMessage("Você teve uma missão há menos de <b>20 minutos</b>, tente novamente mais tarde.");
                        break;
                }

                return false;
            }

            if (character.Client.UserGroup.Role >= RoleEnum.Gold_Vip && character.Account.IsSubscribe == true && m_PvPSeekkHistoryGoldVip.Any(x => x.First.First == character.Id))
            {
                switch (character.Account.Lang)
                {
                    case "fr":
                        character.SendServerMessage("Vous avez eu une mission il y a moins de <b>10 minutes</b>, réessayez plus tard.");
                        break;
                    case "es":
                        character.SendServerMessage("Ya has tenido una misión hace menos de <b>10 minutos</b>, vuelve a intentarlo más tarde.");
                        break;
                    case "en":
                        character.SendServerMessage("You've had a mission less than <b>10 minutes</b> ago, try again later.");
                        break;
                    default:
                        character.SendServerMessage("Você teve uma missão há menos de <b>10 minutos</b>, tente novamente mais tarde.");
                        break;
                }

                return false;
            }
            #endregion

            #region Bloqueio de Alinhamento
            if (character.AlignmentSide == AlignmentSideEnum.ALIGNMENT_NEUTRAL)
            {
                switch (character.Account.Lang)
                {
                    case "fr":
                        character.SendServerMessage("Vous devez avoir un <b>alignement</b> pour commencer une mission.");
                        break;
                    case "es":
                        character.SendServerMessage("Você deve ter um <b>alinhamento</b> para iniciar uma missão.");
                        break;
                    case "en":
                        character.SendServerMessage("You must have an <b>alignment</b> to start a mission.");
                        break;
                    default:
                        character.SendServerMessage("Debe tener una <b>alineación</b> para iniciar una misión.");
                        break;
                }
                return false;
            }
            #endregion

            #region Bloquei de Wing Ativa
            if (!character.PvPEnabled)
            {
                switch (character.Account.Lang)
                {
                    case "fr":
                        character.SendServerMessage("Vous devez activer vos <b>Ailes</b> Pour pouvoir commencer une mission.");
                        break;
                    case "es":
                        character.SendServerMessage("Debes activar tus <b>Alas</b> Para poder iniciar una misión");
                        break;
                    case "en":
                        character.SendServerMessage("You must activate your <b>Wings</b> To be able to start a mission.");
                        break;
                    default:
                        character.SendServerMessage("Você deve ativar suas <b>Asas</b> Para poder iniciar uma missão.");
                        break;
                }
                return false;
            }
            #endregion

            #region Bloqueio de Niveis
            if (character.Level <= 50)
            {
                character.SendServerMessageLangColor("Você deve ser <b>pelo menos lvl 50</b> Para poder realizar uma missão", "You must be at <b>least lvl 50</b> To be able to carry out a mission", "Debes ser por lo <b>mínimo lvl 50</b> Para poder realizar una misión", "Vous devez être au <b>moins lvl 50</b> Pour pouvoir accomplir une mission", Color.DarkOrange);
                return false;
            }
            #endregion

            this.retryBattleField = 0;

            character.SendServerMessageLangColor("Busca do oponente... (60 segundos)", "Search of opponent... (60 secondes)", "Busqueda de oponente... (60 segundos)", "Recherche de l'adversaire... (60 secondes)", Color.Aqua);

            OnQueueBattleField(character, this);

            return true;
        }

        private void OnQueueBattleField(Character character, PvPSeekReply obj)
        {
            if (character.IsInFight())
                return;

            Character SeekTarget = null;

            SeekTarget = Game.World.Instance.GetCharacters(x => character.CanAgress(x, true) == FighterRefusedReasonEnum.FIGHTER_ACCEPTED && CheckAgress(character, x) == FighterRefusedReasonEnum.FIGHTER_ACCEPTED).RandomElementOrDefault();

            obj.retryBattleField++;

            character.SendServerMessageLangColor("Busca do oponente em progresso... (" + obj.retryBattleField + " tentativas)", "Search of opponent in progress... (" + obj.retryBattleField + " attempts)", "Busqueda de oponente en curso... (" + obj.retryBattleField + " intentos)", "Recherche d'adversaire en cours... (" + obj.retryBattleField + " tentatives)", Color.Aqua);

            if (SeekTarget != null)
            {
                ApplySeek(character, SeekTarget);
            }
            else
            {
                if (character.IsInWorld)
                {
                    if (obj.retryBattleField == 6)
                    {
                        character.SendServerMessageLangColor("Atualmente, ninguém está disponível no campo de batalha. Tente novamente mais tarde...", "Currently, nobody is available on the battlefield. Try again later...", "Actualmente, nadie está disponible en el campo de batalla. Inténtalo de nuevo más tarde...", "Actuellement, personne n'est disponible sur le champ de bataille. Réessayez plus tard...", Color.Aqua);
                        return;
                    }

                    obj.BattlefieldTimer = new Timer(_ => OnQueueBattleField(character, obj), null, 1000 * 10, Timeout.Infinite);
                }
            }
        }

        private void ApplySeek(Character character, Character target)
        {
            if (character.IsInWorld)
            {
                foreach (var contract in character.Inventory.GetItems(x => x.Template.Id == (int)ItemIdEnum.ORDRE_DEXECUTION_10085))
                {
                    character.Inventory.RemoveItem(contract);
                }

                var item = ItemManager.Instance.CreatePlayerItem(character, (int)ItemIdEnum.ORDRE_DEXECUTION_10085, 25);
                var seekEffect = item.Effects.FirstOrDefault(x => x.EffectId == EffectsEnum.Effect_Seek);

                if (seekEffect != null)
                    item.Effects.Remove(seekEffect);

                item.Effects.Add(new EffectString(EffectsEnum.Effect_Seek, target.Name));
                item.Effects.Add(new EffectInteger(EffectsEnum.Effect_Alignment, (short)target.AlignmentSide));
                item.Effects.Add(new EffectInteger(EffectsEnum.Effect_Grade, target.AlignmentGrade));
                item.Effects.Add(new EffectInteger(EffectsEnum.Effect_Level, (short)target.Level));
                item.Effects.Add(new EffectInteger(EffectsEnum.Effect_NonExchangeable_981, 0));

                character.Inventory.AddItem(item);

                Push(character, target);

                #region MSG - Encontrou um alvo.
                character.SendServerMessageLangColor($"Você encontrou o jogador {target.Name} como alvo, e recebeu 25 pergaminhos de busca.", $"You found the player {target.Name} as the target, and received 25 search scrolls.", $"Usted encontró al jugador {target.Name} como blanco, y recibió 25 pergaminos de búsqueda.", $"Vous avez trouvé le joueur {target.Name} comme cible et reçu 25 parchemins de recherche.", Color.DarkOrange);
                #endregion
            }
        }

        private FighterRefusedReasonEnum CheckAgress(Character character, Character SeekTarget)
        {
            var lastfights_hdr = SeekLog_manager.Instance.GetHardwareRecord(character.Account.LastHardwareId);
            var lastfights_ip = SeekLog_manager.Instance.GetIpRecords(character.Client.IP);
            var lastfights = lastfights_hdr.Concat(lastfights_ip.Where(x => !lastfights_hdr.Contains(x))).ToList();

            if (SeekTarget.IsGameMaster())
                return FighterRefusedReasonEnum.FIGHTER_REFUSED;

            if (lastfights.Any(entry => entry.Ip_opponent == SeekTarget.Client.IP))
                return FighterRefusedReasonEnum.FIGHTER_REFUSED;

            if (lastfights.Any(entry => entry.Hardware_opponent == SeekTarget.Account.LastHardwareId))
                return FighterRefusedReasonEnum.FIGHTER_REFUSED;

            return FighterRefusedReasonEnum.FIGHTER_ACCEPTED;
        }

        private void Push(Character character, Character target)
        {
            if (character.Account.IsSubscribe == true && character.Client.UserGroup.Role == RoleEnum.Vip)
                m_PvPSeekkHistoryVip.Push(new Pair<int, int>(character.Id, target.Id));
            else if (character.Account.IsSubscribe == true && character.Client.UserGroup.Role >= RoleEnum.Gold_Vip)
                m_PvPSeekkHistoryGoldVip.Push(new Pair<int, int>(character.Id, target.Id));
            else
                m_PvPSeekkHistoryFree.Push(new Pair<int, int>(character.Id, target.Id));
        }
    }
}