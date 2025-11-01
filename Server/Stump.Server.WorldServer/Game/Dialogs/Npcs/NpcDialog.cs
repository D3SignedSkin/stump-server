using System.Linq;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Database.Npcs;
using Stump.Server.WorldServer.Database.Npcs.Replies;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Npcs;
using Stump.Server.WorldServer.Handlers.Context.RolePlay;
using Stump.Server.WorldServer.Handlers.Dialogs;
using System.Collections.Generic;
using System;
using Stump.Server.WorldServer.Game.Seeklog;
using Stump.Server.WorldServer.Game.Arena;
using Stump.Server.WorldServer.Game.Guilds;

namespace Stump.Server.WorldServer.Game.Dialogs.Npcs
{
    public class NpcDialog : IDialog
    {
        private static List<uint> m_dialogsDungeonsReturn = new List<uint>()
        {
            9670,
            9672,
            9917,
            10722,
            11611,
            11754,
            11755,
            11792,
            11822,
            11823,
            11860,
            11922,
            11941,
            11949,
            11955,
            11963,
            11964,
            11965,
            13374,
            13381,
            13382,
            13389,
            13434,
            13437,
            13438,
            13489,
            13490,
            13491,
            13505,
            13511,
            15075,
            15121,
            15147,
            15148,
            15190,
            15191,
            15213,
            15227,
            15228,
            15317,
            15371,
            15422,
            15827,
            15829,
            15830,
            15831,
            15832,
            15837,
            15842,
            15843,
            15844,
            15878,
            15879,
            15889,
            16244,
            16263,
            16275,
            16536,
            320980,
            321027,
            321036,
            321038,
            321040,
            321042,
            321108,
            333088,
            376564,
            391999,
            418679,
            418745,
            419027,
            419099,
            451649,
            451673,
            451677,
            460741,
            480180,
            483920,
            483921,
            485806,
            508853,
            508914,
            508925,
            508926,
            516205,
            541116,
            541130,
            551890,
            580126,
            580166,
            617030,
            617037,
            617040,
            632352,
            657742,
            657821,
            657825,
            694878,
            700079,
            713651,
            721751,
            725727,
            733088,
            755718,
            767635,
            789743,
            789761,
            789774,
            802703,
            803811,
            818777,
            819929,
            821134,
            821135,
            822695,
            825260,
            825272,
            825285,
            825298,
            825770,
            827265,
            840463,
            841887,
            841913,
            842793,
            844906,
            854803,
            855807,
            900654,
        };

        private static Dictionary<uint, int> m_dialogsDopplesTime = new Dictionary<uint, int>()
        {
            { 109395, 168 },  //Dopple Cra
            { 109393, 168 },  //Dopple Cra
            { 109275, 167 },  //Dopple Iop
            { 109281, 167 },  //Dopple Iop
            { 109325, 3132 }, //Dopple Zobal
            { 109322, 3132 }, //Dopple Zobal
            { 108751, 162 },  //Dopple Enutrof
            { 108830, 162 },  //Dopple Enutrof
            { 109178, 455 },  //Dopple Sacrier
            { 109168, 455 },  //Dopple Sacrier
            { 108769, 2691 }, //Dopple Pandawa
            { 108767, 2691 }, //Dopple Pandawa
            { 108714, 163 },  //Dopple Sram 
            { 108711, 163 },  //Dopple Sram 
            { 108971, 164 },  //Dopple Xelor
            { 108973, 164 },  //Dopple Xelor
            { 109008, 166 },  //Dopple Eniripsa
            { 109007, 166 },  //Dopple Eniripsa
            { 109203, 161 },  //Dopple Osamodas
            { 109205, 161 },  //Dopple Osamodas
            { 508835, 3286 }, //Dopple Steamer
            { 353183, 3286 }, //Dopple Steamer
            { 109105, 160 },  //Dopple Feca
            { 109103, 160 },  //Dopple Feca
            { 595924, 4290 }, //Dopple Huppermago
            { 595919, 4290 }, //Dopple Huppermago
            { 108689, 169 },  //Dopple Sadida
            { 108700, 169 },  //Dopple Sadida
            { 108992, 3111 }, //Dopple Ladino
            { 108988, 3111 }, //Dopple Ladino
            { 594776, 3976 }, //Dopple Eliotrop
            { 594777, 3976 }, //Dopple Eliotrop
            { 718546, 4777 }, //Dopple Kilorf
            { 718610, 4777 }, //Dopple Kilorf
        };

        public NpcDialog(Character character, Npc npc)
        {
            Character = character;
            Npc = npc;
        }

        public DialogTypeEnum DialogType => DialogTypeEnum.DIALOG_DIALOG;

        public Character Character
        {
            get;
        }

        public Npc Npc
        {
            get;
        }

        public NpcMessage CurrentMessage
        {
            get;
            protected set;
        }

        public virtual void Open()
        {
            Character.SetDialog(this);
            ContextRoleplayHandler.SendNpcDialogCreationMessage(Character.Client, Npc);
        }

        public virtual void Close()
        {
            DialogHandler.SendLeaveDialogMessage(Character.Client, DialogType);
            Character.CloseDialog(this);
        }

        public virtual void Reply(uint replyId)
        {
            var lastMessage = CurrentMessage;
            var replies = CurrentMessage.Replies.Where(entry => entry.ReplyId == replyId && entry.Active != 0).ToArray();

            if (replies.Any(x => !x.CanExecute(Npc, Character) && replies.Count() <= 1))
            {
                Character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 34);
                Close();

                return;
            }

            foreach (var npcReply in replies)
            {
                if (!npcReply.CanExecute(Npc, Character))
                    continue;

                Reply(npcReply);
            }

            // default action : close dialog
            if (replies.Length == 0 || lastMessage == CurrentMessage)
                Close();
        }

        public void Reply(NpcReply reply)
        {
            reply.Execute(Npc, Character);
        }

        public void ChangeMessage(short id)
        {
            var message = NpcManager.Instance.GetNpcMessage(id);

            if (message != null)
                ChangeMessage(message);
        }

        public virtual void ChangeMessage(NpcMessage message)
        {
            CurrentMessage = message;
            List<int> replies = new List<int>();

            if (message.Replies.Count > 0)
            {
                foreach (var reply in message.Replies.OrderBy(x => x.ReplyId))
                {
                    if (reply is null)
                        continue;

                    if (replies.Contains(reply.ReplyId))
                        continue;

                    if (reply.Active == 0)
                        continue;

                    if (reply.CriteriaExpression != null && !reply.CriteriaExpression.Eval(Character))
                        continue;

                    if ((reply.CriteriaExpression != null && !reply.CriteriaExpression.Eval(Character) && reply.CanShow(Npc, Character)) || replies.Contains(reply.ReplyId))
                        continue;

                    replies.Add(reply.ReplyId);
                }
            }

            if (m_dialogsDopplesTime.Any(x => x.Key == CurrentMessage.MessageId))
            {
                #region >> Npc Dopple Time
                int doppleId = m_dialogsDopplesTime[CurrentMessage.MessageId];

                TimeSpan timeDifference;
                var hoursDifference = 0;
                var compareTime = DateTime.Now;
                var matchingDopeul = Character.DoppleCollection.Dopeul.LastOrDefault(dopeul => dopeul.DopeulId == doppleId);

                if (matchingDopeul != null)
                {
                    timeDifference = matchingDopeul.Time - compareTime;
                    hoursDifference = (int)Math.Ceiling(timeDifference.TotalHours);
                    hoursDifference = Math.Max(hoursDifference, 1);
                }

                ContextRoleplayHandler.SendNpcDialogQuestionMessage(Character.Client, CurrentMessage, replies, hoursDifference.ToString());
                #endregion
            }
            else if (CurrentMessage.MessageId == 189020)
            {
                #region > Mensagem Mostrando Custo do Uso do Banco
                uint CostKamas = 0;
                CostKamas = (uint)Character.Bank.Count();
                CostKamas = CostKamas * 1;
                var CostResult = Convert.ToString(CostKamas);

                ContextRoleplayHandler.SendNpcDialogQuestionMessage(Character.Client, CurrentMessage, replies, CostResult);
                #endregion
            }
            else if (CurrentMessage.MessageId == 1100246)
            {
                #region > Mensagem Mostrando na NPC de Scrolls Astrub
                var CostElement = "3.300";
                var CostElementComplet = "20.000";
                ContextRoleplayHandler.SendNpcDialogQuestionMessage(Character.Client, CurrentMessage, replies, CostElement, CostElementComplet);
                #endregion
            }
            else if (CurrentMessage.MessageId == 1100289)
            {
                #region > Mensagem Mostrando na NPC de Conversão de Ogrines por Kamas
                var CostOgrines = "100";
                var GiveKamas = "2.000.000";
                ContextRoleplayHandler.SendNpcDialogQuestionMessage(Character.Client, CurrentMessage, replies, CostOgrines, GiveKamas);
                #endregion
            }
            else if (CurrentMessage.MessageId == 1100293 || CurrentMessage.MessageId == 546535)
            {
                #region > NPCs que apresente o nome do personagem
                ContextRoleplayHandler.SendNpcDialogQuestionMessage(Character.Client, CurrentMessage, replies, Character.Namedefault);
                #endregion
            }
            else if (CurrentMessage.MessageId == 1100431 || CurrentMessage.MessageId == 1100433 || CurrentMessage.MessageId == 1100435)
            {
                #region > NPCs que apresente o nome do personagem
                int SeekCount = Game.World.Instance.GetCharacters(x => Character.CanAgress(x, true) == FighterRefusedReasonEnum.FIGHTER_ACCEPTED && CheckAgress(Character, x) == FighterRefusedReasonEnum.FIGHTER_ACCEPTED).Count();

                ContextRoleplayHandler.SendNpcDialogQuestionMessage(Character.Client, CurrentMessage, replies, SeekCount.ToString());
                #endregion
            }
            else if (CurrentMessage.MessageId == 1100448)
            {
                #region > NPCs que apresente o nome do personagem
                int fila1players = ArenaManager.Instance.Fila(1);
                int fila2players = ArenaManager.Instance.Fila(2);
                int fila3players = ArenaManager.Instance.Fila(3);

                var Convertefila1players = Convert.ToString(fila1players);
                var Convertefila3party = Convert.ToString(fila2players);
                var Convertefila3players = Convert.ToString(fila3players);

                ContextRoleplayHandler.SendNpcDialogQuestionMessage(Character.Client, CurrentMessage, replies, Convertefila1players, Convertefila3party, Convertefila3players);
                #endregion
            }
            else if (CurrentMessage.MessageId == 1100499 || CurrentMessage.MessageId == 1100503 || CurrentMessage.MessageId == 1100507)
            {
                #region > NPCs que retiram a honra
                var ConverteCost = Convert.ToString(5000);
                var ConvertePercent = Convert.ToString(10);

                ContextRoleplayHandler.SendNpcDialogQuestionMessage(Character.Client, CurrentMessage, replies, ConverteCost, ConvertePercent);
                #endregion
            }
            else if (CurrentMessage.MessageId == 1100532)
            {
                #region >> Npc Ranking de Kills Percs
                List<Guild> GuildsRanking = new List<Guild>();

                foreach (var Guilds in GuildManager.Instance.GetGuilds().OrderByDescending(x => x.PercsKills).Take(3))
                    GuildsRanking.Add(Guilds);

                var PrimaryWinner = GuildsRanking[0];
                var SecondWinner = GuildsRanking[1];
                var ThirdWinner = GuildsRanking[2];

                ContextRoleplayHandler.SendNpcDialogQuestionMessage(Character.Client, CurrentMessage, replies, PrimaryWinner.PercsKills > 0 ? PrimaryWinner.Name : "Vazio", Convert.ToString(PrimaryWinner.PercsKills), SecondWinner.PercsKills > 0 ? SecondWinner.Name : "Vazio", Convert.ToString(SecondWinner.PercsKills), ThirdWinner.PercsKills > 0 ? ThirdWinner.Name : "Vazio", Convert.ToString(ThirdWinner.PercsKills));
                #endregion
            }
            else if (CurrentMessage.MessageId == 1100534)
            {
                #region >> Npc Ranking de Defesas Percs
                List<Guild> GuildsRanking = new List<Guild>();

                foreach (var Guilds in GuildManager.Instance.GetGuilds().OrderByDescending(x => x.PercsDefenders).Take(3))
                    GuildsRanking.Add(Guilds);

                var PrimaryWinner = GuildsRanking[0];
                var SecondWinner = GuildsRanking[1];
                var ThirdWinner = GuildsRanking[2];

                ContextRoleplayHandler.SendNpcDialogQuestionMessage(Character.Client, CurrentMessage, replies, PrimaryWinner.PercsDefenders > 0 ? PrimaryWinner.Name : "Vazio", Convert.ToString(PrimaryWinner.PercsDefenders), SecondWinner.PercsDefenders > 0 ? SecondWinner.Name : "Vazio", Convert.ToString(SecondWinner.PercsDefenders), ThirdWinner.PercsDefenders > 0 ? ThirdWinner.Name : "Vazio", Convert.ToString(ThirdWinner.PercsDefenders));
                #endregion
            }
            else if (CurrentMessage.MessageId == 1100297 || CurrentMessage.MessageId == 1100305 || CurrentMessage.MessageId == 1100312)
            {
                #region >> Npc Jobs XP

                string amount = "[UNKNOW]";

                if (CurrentMessage.MessageId == 1100297)
                {
                    amount = "5.000";
                }
                else if (CurrentMessage.MessageId == 1100305)
                {
                    amount = "2.500";
                }
                else if (CurrentMessage.MessageId == 1100312)
                {
                    amount = "10.000";
                }

                ContextRoleplayHandler.SendNpcDialogQuestionMessage(Character.Client, CurrentMessage, replies, amount);
                #endregion
            }
            else if (m_dialogsDungeonsReturn.Contains(CurrentMessage.MessageId))
            {
                #region >> Messages Return Dungeons

                List<NpcReplyRecord> replys = NpcManager.Instance.GetMessageRepliesRecords(this.Npc.Template.Id);
                NpcReplyRecord currentReply = replys.FirstOrDefault(reply => reply.MessageId == CurrentMessage.Id);

                if (currentReply != null)
                {
                    int dungeonReplyId = int.Parse(currentReply.Parameter0);

                    if (dungeonReplyId <= 0)
                        ContextRoleplayHandler.SendNpcDialogQuestionMessage(Character.Client, CurrentMessage, replies);

                    long[] dungeonMapId = Character.DungeonReturn.FirstOrDefault(x => x.Contains(dungeonReplyId));

                    if (dungeonMapId[1] > 0)
                        ContextRoleplayHandler.SendNpcDialogQuestionMessage(Character.Client, CurrentMessage, replies, $"{dungeonMapId[1]}");
                    else
                        ContextRoleplayHandler.SendNpcDialogQuestionMessage(Character.Client, CurrentMessage, replies);
                }
                else
                {
                    ContextRoleplayHandler.SendNpcDialogQuestionMessage(Character.Client, CurrentMessage, replies);
                }

                #endregion
            }
            else
            {
                ContextRoleplayHandler.SendNpcDialogQuestionMessage(Character.Client, CurrentMessage, replies);
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
    }
}