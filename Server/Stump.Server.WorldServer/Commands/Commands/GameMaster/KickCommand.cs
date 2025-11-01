using MongoDB.Bson;
using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Commands;
using Stump.Server.BaseServer.Logging;
using Stump.Server.WorldServer.Commands.Commands.Patterns;
using Stump.Server.WorldServer.Commands.Trigger;
using Stump.Server.WorldServer.Core.Network;
using System;
using System.Globalization;

namespace Stump.Server.WorldServer.Commands.Commands.GameMaster
{
    public class KickCommand : TargetCommand
    {
        public KickCommand()
        {
            Aliases = new[] { "kick" };
            RequiredRole = RoleEnum.GameMaster_Padawan;
            Description = "Kick a player";
            AddTargetParameter();
        }

        public override void Execute(TriggerBase trigger)
        {
            var source = trigger.GetSource() as WorldClient;

            foreach (var target in GetTargets(trigger))
            {
                if (!target.IsInFight())
                {
                    var kicker = (trigger is GameTrigger) ? (trigger as GameTrigger).Character.Name : "Server";

                    target.SendSystemMessage(18, true, kicker, string.Empty); // you were kicked by %1
                    target.Client.Disconnect();
                }
                else
                {
                    if (target.IsFighting())
                        target.Fighter.LeaveFight();
                    if (target.IsSpectator())
                        target.Spectator.Leave();

                    var kicker = (trigger is GameTrigger) ? (trigger as GameTrigger).Character.Name : "Server";

                    target.SendSystemMessage(18, true, kicker, string.Empty); // you were kicked by %1
                    target.Client.Disconnect();
                }

                #region // ----------------- Sistema de Logs MongoDB Kick by: Kenshin ---------------- //
                try
                {
                    var document = new BsonDocument
                                {
                                    { "Staff_IP", source.IP },
                                    { "StaffName", source.Character.Name },
                                    { "Target_ID", target.Id },
                                    { "Target_Name", target.NameClean },
                                    { "Target_IP", target.Client.IP },
                                    { "Date", DateTime.Now.ToString(CultureInfo.CreateSpecificCulture("pt-BR")) }
                                };

                    MongoLogger.Instance.Insert("Staff_ManagementKick", document);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Erro no Mongologs Kick : " + e.Message);
                }
                #endregion

                trigger.Reply("You have kicked {0}", target.Name);
            }
        }
    }
}