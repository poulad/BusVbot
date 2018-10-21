using System.Threading;
using System.Threading.Tasks;
using BusV.Data.Entities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;
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
                // "agencies" collection
                await database
                    .CreateCollectionAsync(Constants.Collections.Agencies.Name, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                var botsCollection = database.GetCollection<Agency>(Constants.Collections.Agencies.Name);

                // create unique index "agency_name" on the field "tag"
                var key = Builders<Agency>.IndexKeys.Ascending(a => a.Tag);
                await botsCollection.Indexes.CreateOneAsync(new CreateIndexModel<Agency>(
                        key,
                        new CreateIndexOptions
                            { Name = Constants.Collections.Agencies.Indexes.AgencyName, Unique = true }),
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
            if (!BsonClassMap.IsClassMapRegistered(typeof(Agency)))
            {
                BsonClassMap.RegisterClassMap<Agency>(map =>
                {
                    map.MapIdProperty(u => u.Id)
                        .SetIdGenerator(StringObjectIdGenerator.Instance)
                        .SetSerializer(new StringSerializer(BsonType.ObjectId));
                    map.MapProperty(b => b.Tag).SetElementName("tag").SetOrder(1);
                    map.MapProperty(u => u.CreatedAt).SetElementName("created_at");
                    map.MapProperty(u => u.Title).SetElementName("title").SetIgnoreIfDefault(true);
                    map.MapProperty(u => u.Region).SetElementName("region").SetIgnoreIfDefault(true);
                    map.MapProperty(u => u.Country).SetElementName("country").SetIgnoreIfDefault(true);
                    map.MapProperty(u => u.ShortTitle).SetElementName("short_title").SetIgnoreIfDefault(true);
                    map.MapProperty(u => u.MaxLatitude).SetElementName("max_lat").SetIgnoreIfDefault(true);
                    map.MapProperty(u => u.MinLatitude).SetElementName("min_lat").SetIgnoreIfDefault(true);
                    map.MapProperty(u => u.MaxLongitude).SetElementName("max_lon").SetIgnoreIfDefault(true);
                    map.MapProperty(u => u.MinLongitude).SetElementName("min_lon").SetIgnoreIfDefault(true);
                    map.MapProperty(u => u.ModifiedAt).SetElementName("modified_at").SetIgnoreIfDefault(true);
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
