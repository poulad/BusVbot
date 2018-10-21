using System.Threading;
using System.Threading.Tasks;
using BusV.Data.Entities;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;

namespace BusV.Data
{
    public static class Initializer
    {
        public static async Task CreateSchemaAsync(
            IMongoDatabase database,
            CancellationToken cancellationToken = default
        )
        {
            {
                // "bots" collection
                await database
                    .CreateCollectionAsync(Constants.Collections.Bots.Name, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                var botsCollection = database.GetCollection<ChatBot>(Constants.Collections.Bots.Name);

                // create unique index "bot_id" on the field "name"
                var key = Builders<ChatBot>.IndexKeys.Ascending(u => u.Name);
                await botsCollection.Indexes.CreateOneAsync(new CreateIndexModel<ChatBot>(
                        key,
                        new CreateIndexOptions
                            { Name = Constants.Collections.Bots.Indexes.BotId, Unique = true }),
                    cancellationToken: cancellationToken
                ).ConfigureAwait(false);
            }

            {
                // "registrations" collection
                await database.CreateCollectionAsync(
                    Constants.Collections.Registrations.Name,
                    cancellationToken: cancellationToken
                ).ConfigureAwait(false);
                var regsCollection =
                    database.GetCollection<Registration>(Constants.Collections.Registrations.Name);

                // create unique index "bot_username" on the fields "bot" and "username"
                var indexBuilder = Builders<Registration>.IndexKeys;
                var key = indexBuilder.Combine(
                    indexBuilder.Ascending(tl => tl.ChatBotDbRef.Id),
                    indexBuilder.Ascending(tl => tl.Username)
                );
                await regsCollection.Indexes.CreateOneAsync(new CreateIndexModel<Registration>(
                        key,
                        new CreateIndexOptions
                            { Name = Constants.Collections.Registrations.Indexes.BotUsername, Unique = true }),
                    cancellationToken: cancellationToken
                ).ConfigureAwait(false);
            }
        }

        public static void RegisterClassMaps()
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof(ChatBot)))
            {
                BsonClassMap.RegisterClassMap<ChatBot>(map =>
                {
                    map.MapIdProperty(b => b.Id).SetIdGenerator(new StringObjectIdGenerator());
                    map.MapProperty(b => b.Name).SetElementName("name").SetOrder(1);
                    map.MapProperty(u => u.Platform).SetElementName("platform");
                    map.MapProperty(u => u.Url).SetElementName("url");
                    map.MapProperty(u => u.Token).SetElementName("token");
                    map.MapProperty(u => u.JoinedAt).SetElementName("created_at");
                });
            }

            if (!BsonClassMap.IsClassMapRegistered(typeof(Registration)))
            {
                BsonClassMap.RegisterClassMap<Registration>(map =>
                {
                    map.MapIdProperty(reg => reg.Id).SetIdGenerator(new StringObjectIdGenerator());
                    map.MapProperty(reg => reg.Username).SetElementName("username").SetOrder(1);
                    map.MapProperty(reg => reg.ChatUserId).SetIsRequired(true).SetElementName("user_id");
                    map.MapProperty(tl => tl.RegisteredAt).SetElementName("created_at");
                    map.MapProperty(tl => tl.ChatBotDbRef).SetIsRequired(true).SetElementName("bot");
                });
            }
        }
    }
}
