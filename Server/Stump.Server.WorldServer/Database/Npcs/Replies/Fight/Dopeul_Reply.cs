using Stump.Core.Reflection;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Messages;
using Stump.Server.BaseServer.Database;
using Stump.Server.WorldServer.Database.Npcs;
using Stump.Server.WorldServer.Database.Npcs.Replies;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Monsters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Npcs;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Maps.Cells;
using Stump.Server.WorldServer.Handlers.Context;
using System;
using System.Drawing;
using System.Linq;

namespace Database.Npcs.Replies
{
    [Discriminator("Dopeul", typeof(NpcReply), typeof(NpcReplyRecord))]
    internal class DopeulReplies : NpcReply
    {
        public DopeulReplies(NpcReplyRecord record) : base(record)
        { }

        public int MonsterId
        {
            get
            {
                var result = 0;
                try
                {
                    result = Record.GetParameter<int>(0u);
                }
                catch { }
                return result;
            }
            set { Record.SetParameter(0u, value); }
        }

        public override bool Execute(Npc npc, Character character)
        {
            var compareTime = DateTime.Now;
            var matchingDopeul = character.DoppleCollection.Dopeul.LastOrDefault(dopeul => dopeul.DopeulId == MonsterId);

            if (matchingDopeul != null)
                compareTime = matchingDopeul.Time;

            if (compareTime > DateTime.Now)
            {
                string langMessage;

                #region >> MSG Block
                switch (character.Account.Lang)
                {
                    case "es":
                        langMessage = $"Espera, no puedes hacer esto!, El Dopeul esta cansado, Debes esperar <b>{(compareTime - DateTime.Now).Hours} hora(s), {(compareTime - DateTime.Now).Minutes} minuto(s)</b>";
                        break;
                    case "fr":
                        langMessage = $"Attendez, vous ne pouvez pas faire cela! Le Dopeul est fatigué, vous devez attendre <b>{(compareTime - DateTime.Now).Hours} heure(s) et {(compareTime - DateTime.Now).Minutes} minute(s)</b>";
                        break;
                    case "en":
                        langMessage = $"Wait, you can not do this! The Dopeul is tired, You must wait <b>{(compareTime - DateTime.Now).Hours} hour(s) and {(compareTime - DateTime.Now).Minutes} minute(s)</b>";
                        break;
                    default:
                        langMessage = $"Espere, você não pode fazer isso! O Dopeul está cansado, você deve esperar <b>{(compareTime - DateTime.Now).Hours} hora(s) e {(compareTime - DateTime.Now).Minutes} minuto(s)</b>";
                        break;
                }
                #endregion MSG Block

                character.SendServerMessage(langMessage, Color.Red);
                character.LeaveDialog();

                return false;
            }
            else
            {
                var monsterGradeId = 1;
                int monsterGradePlus10 = 10;
                int maxMonsterGradePlus10 = 200;

                while (monsterGradeId <= 11 && character.Level > monsterGradePlus10)
                {
                    monsterGradeId++;
                    monsterGradePlus10 += 20;

                    if (monsterGradePlus10 >= maxMonsterGradePlus10)
                        break;
                }

                var grade = Singleton<MonsterManager>.Instance.GetMonsterGrade(MonsterId, monsterGradeId);
                var position = new ObjectPosition(character.Map, character.Cell, DirectionsEnum.DIRECTION_NORTH_WEST);
                var monster = new Monster(grade, new MonsterGroup(0, position));
                var fight = Singleton<FightManager>.Instance.CreatePvDFight(character.Map);

                fight.ChallengersTeam.AddFighter(character.CreateFighter(fight.ChallengersTeam));
                fight.DefendersTeam.AddFighter(new MonsterFighter(fight.DefendersTeam, monster));
                fight.StartPlacement();

                ContextHandler.HandleGameFightJoinRequestMessage(character.Client, new GameFightJoinRequestMessage(character.Fighter.Id, (ushort)fight.Id));
                character.SaveLater();

                return true;
            }
        }
    }
}