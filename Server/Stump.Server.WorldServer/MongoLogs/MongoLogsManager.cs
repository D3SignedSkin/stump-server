using MongoDB.Bson;
using Stump.Server.BaseServer.Logging;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using System;
using System.Globalization;

namespace Stump.Server.WorldServer.MongoLogs
{
    public class MongoLogsManager
    {
        public static void SendErrorGeneric(Character character, string errorMsg)
        {
            try
            {
                var document = new BsonDocument
                {
                    { "AccountId", character.Id },
                    { "Date", DateTime.Now.ToString(CultureInfo.CreateSpecificCulture("pt-BR")) }
                };

                MongoLogger.Instance.Insert("Player_LoginBanned", document);
            }
            catch (Exception e)
            {
                Console.WriteLine("Erro no Mongologs Banned : " + e.Message);
            }
        }
    }
}
