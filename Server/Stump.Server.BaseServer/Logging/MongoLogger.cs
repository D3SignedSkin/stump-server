using MongoDB.Bson;
using MongoDB.Driver;
using Stump.Core.Attributes;
using Stump.Core.Reflection;
using Stump.ORM;
using Stump.Server.BaseServer.Initialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stump.Server.BaseServer.Logging
{
    public class MongoLogger : Singleton<MongoLogger>
    {
        [Variable(Priority = 10, DefinableRunning = true)]
        public static bool IsMongoLoggerEnabled = true;

        [Variable(Priority = 10, DefinableRunning = true)]
        public static DatabaseConfiguration MongoDBConfiguration = new DatabaseConfiguration
        {
            Host = "127.0.0.1",
            DbName = "Charly_logs",
            Port = "27017",
            User = "root",
            Password = ""
        };

        private IMongoDatabase m_database;

        [Initialization(InitializationPass.Database)]
        public void Initialize()
        {
            if (!IsMongoLoggerEnabled)
                return;

            var client = new MongoClient($"mongodb://{MongoDBConfiguration.Host}:{MongoDBConfiguration.Port}/" + $"{MongoDBConfiguration.DbName}");

            m_database = client.GetDatabase(MongoDBConfiguration.DbName);
        }

        public bool Insert(string collection, BsonDocument document)
        {
            if (!IsMongoLoggerEnabled)
            {
                if (m_database == null)
                    return false;

                m_database = null;

                return false;
            }

            if (m_database == null)
            {
                Initialize();
            }

            if (m_database != null)
            {
                m_database.GetCollection<BsonDocument>(collection).InsertOneAsync(document);
            }
            else
            {
                return false;
            }

            return true;
        }

        public async Task InsertAsync(string collection, BsonDocument document)
        {
            if (!IsMongoLoggerEnabled)
                return;

            if (m_database == null)
            {
                Initialize();
            }

            if (m_database != null)
            {
                await m_database.GetCollection<BsonDocument>(collection).InsertOneAsync(document);
            }
        }

        public async Task<IEnumerable<BsonDocument>> GetDocumentsAsync(string collectionName)
        {
            if (!IsMongoLoggerEnabled || m_database == null)
            {
                return Enumerable.Empty<BsonDocument>();
            }

            var collection = m_database.GetCollection<BsonDocument>(collectionName);

            if (collection == null)
            {
                throw new ArgumentException($"The collection '{collectionName}' was not found.");
            }

            var documents = await collection.Find(new BsonDocument()).ToListAsync();

            return documents;
        }
    }
}