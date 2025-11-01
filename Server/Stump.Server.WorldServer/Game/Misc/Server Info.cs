using MongoDB.Driver;
using NLog;
using Stump.Core.Reflection;
using Stump.Server.BaseServer.Database;
using Stump.Server.BaseServer.Initialization;
using Stump.Server.WorldServer.Database.Misc;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Stump.Server.WorldServer.Game.Misc
{
    public class ServerInfoManager : DataManager<ServerInfoManager>
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        private Dictionary<int, ServerInfo> m_record;

        [Initialization(InitializationPass.Any)]
        public override void Initialize()
        {
            m_record = Database.Query<ServerInfo>(ServerInfoRelator.FecthQuery).ToDictionary(x => x.Id);
        }

        public void AddRecord(int record)
        {
            if (m_record.Any())
            {
                if (record > m_record[1].RecordON)
                {
                    TimeSpan date = Convert.ToDateTime(DateTime.Now) - Convert.ToDateTime(m_record[1].RecordUpdate);
                    int totalDias = date.Days;
                    IEnumerable<Character> characters = Singleton<World>.Instance.GetCharacters();

                    foreach (Character character in characters)
                    {
                        switch (character.Account.Lang)
                        {
                            case "fr":
                                //character.DisplayNotification("Hydra a atteint un nouveau record avec " + record + " joueurs en ligne simultanément, cela fait " + totalDias + " jours depuis le dernier record!");
                                character.SendServerMessage("Server a atteint un nouveau record avec " + record + " joueurs en ligne simultanément, cela fait " + totalDias + " jours depuis le dernier record!");
                                break;
                            case "es":
                                //character.DisplayNotification("Hydra alcanzó un nuevo récord con " + record + " jugadores en línea simultáneamente, ¡han pasado " + totalDias + " días desde el último récord!");
                                character.SendServerMessage("Server alcanzó un nuevo récord con " + record + " jugadores en línea simultáneamente, ¡han pasado " + totalDias + " días desde el último récord!");
                                break;
                            case "en":
                                //character.DisplayNotification("Hydra reached a new record with " + record + " players online simultaneously, it's been " + totalDias + " days since the last record!");
                                character.SendServerMessage("Server reached a new record with " + record + " players online simultaneously, it's been " + totalDias + " days since the last record!");
                                break;
                            default:
                                //character.DisplayNotification("O Hydra atingiu um novo recorde com "+ record + " jogadores online simultaneamente, já fazem "+ totalDias + " dias desde o último recorde!");
                                character.SendServerMessage("O Server atingiu um novo recorde com " + record + " jogadores online simultaneamente, já fazem " + totalDias + " dias desde o último recorde!");
                                break;
                        }
                    }

                    m_record[1].RecordON = record;
                    m_record[1].RecordUpdate = DateTime.Now;
                    WorldServer.Instance.IOTaskPool.ExecuteInContext(() => this.Save(WorldServer.Instance.DBAccessor.Database));
                }
            }
        }

        public int? GetRecord()
        {
            if (m_record.Any())
            {
                return m_record[1].RecordON;
            }
            else
            {
                return 0;
            }
        }

        public void Save(ORM.Database database)
        {
            try
            {
                foreach (var server in m_record)
                {
                    if (!server.Value.IsNew)
                    {
                        database.Update(server.Value);
                    }

                    if (server.Value.IsNew)
                    {
                        //dont need this, wtf?!
                        //DopeulManager.Instance.AddRecord(dopeul);
                        //database.Insert(dopeul);
                        //dopeul.IsNew = false;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error saving Server Info: {ex.Message}");
            }
        }
    }
}