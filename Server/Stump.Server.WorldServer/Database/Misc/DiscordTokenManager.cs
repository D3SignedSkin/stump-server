using NLog;
using Stump.Server.BaseServer.Database;
using Stump.Server.BaseServer.Initialization;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Stump.Server.WorldServer.Database.Misc
{
    public class DiscordTokenManager : DataManager<DiscordTokenManager>
    {
        private Dictionary<int, DiscordTokenRecord> m_tokens = new Dictionary<int, DiscordTokenRecord>();
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        [Initialization(InitializationPass.Any)]
        public override void Initialize()
        {
            foreach (var token in Database.Query<DiscordTokenRecord>(DiscordTokenRelator.FetchQuery))
            {
                m_tokens.Add(token.Id, token);
            }
        }

        public Dictionary<int, DiscordTokenRecord> GetDiscordTokens()
        {
            return m_tokens;
        }

        public bool hasDiscordTokens(string token)
        {
            return m_tokens.Values.Any(x => x.Token == token);
        }

        public DiscordTokenRecord GetDiscordTokensbyEmail(string email)
        {
            return m_tokens.Values.FirstOrDefault(x => x.Email == email);
        }

        public void SetDiscordToken(DiscordTokenRecord token) 
        {
            m_tokens.Add(token.Id, token);
        }

        public void Save(Character character)
        {
            var database = WorldServer.Instance.DBAccessor.Database;

            WorldServer.Instance.IOTaskPool.AddMessage(() =>
            {
                try
                {
                    if (m_tokens.Count(x => x.Value.IsUpdate && x.Value.Email == character.Account.Email) > 0)
                    {
                        foreach (var token in m_tokens.Values.Where(x => x.IsUpdate && x.Email == character.Account.Email))
                        {
                            database.Update(token);
                            token.IsUpdate = false;
                        }
                    }

                    if (m_tokens.Count(x => x.Value.IsNew && x.Value.Email == character.Account.Email) > 0)
                    {
                        foreach (var token in m_tokens.Values.Where(x => x.IsNew && x.Email == character.Account.Email))
                        {
                            database.Insert(token);
                            token.IsNew = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error($"Error saving DiscordTokens: {ex.Message}");
                }
            });
        }
    }
}