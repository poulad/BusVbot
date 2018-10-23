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
                // "users" collection
                await database.CreateCollectionAsync(
                    Constants.Collections.Users.Name,
                    cancellationToken: cancellationToken
                ).ConfigureAwait(false);
                var regsCollection = database.GetCollection<UserProfile>(Constants.Collections.Users.Name);

                // create unique index "user_chat" on the fields "user" and "chat"
                var indexBuilder = Builders<UserProfile>.IndexKeys;
                var key = indexBuilder.Combine(
                    indexBuilder.Ascending(u => u.UserId),
                    indexBuilder.Ascending(u => u.ChatId)
                );
                await regsCollection.Indexes.CreateOneAsync(new CreateIndexModel<UserProfile>(
                        key,
                        new CreateIndexOptions
                            { Name = Constants.Collections.Users.Indexes.UserChat, Unique = true }),
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
                    map.MapIdProperty(a => a.Id)
                        .SetIdGenerator(StringObjectIdGenerator.Instance)
                        .SetSerializer(new StringSerializer(BsonType.ObjectId));
                    map.MapProperty(a => a.Tag).SetElementName("tag").SetOrder(1);
                    map.MapProperty(a => a.CreatedAt).SetElementName("created_at");
                    map.MapProperty(a => a.Title).SetElementName("title").SetIgnoreIfDefault(true);
                    map.MapProperty(a => a.Region).SetElementName("region").SetIgnoreIfDefault(true);
                    map.MapProperty(a => a.Country).SetElementName("country").SetIgnoreIfDefault(true);
                    map.MapProperty(a => a.ShortTitle).SetElementName("short_title").SetIgnoreIfDefault(true);
                    map.MapProperty(a => a.MaxLatitude).SetElementName("max_lat").SetIgnoreIfDefault(true);
                    map.MapProperty(a => a.MinLatitude).SetElementName("min_lat").SetIgnoreIfDefault(true);
                    map.MapProperty(a => a.MaxLongitude).SetElementName("max_lon").SetIgnoreIfDefault(true);
                    map.MapProperty(a => a.MinLongitude).SetElementName("min_lon").SetIgnoreIfDefault(true);
                    map.MapProperty(a => a.ModifiedAt).SetElementName("modified_at").SetIgnoreIfDefault(true);
                });
            }

            if (!BsonClassMap.IsClassMapRegistered(typeof(UserProfile)))
            {
                BsonClassMap.RegisterClassMap<UserProfile>(map =>
                {
                    map.MapIdProperty(u => u.Id)
                        .SetIdGenerator(StringObjectIdGenerator.Instance)
                        .SetSerializer(new StringSerializer(BsonType.ObjectId));
                    map.MapProperty(u => u.UserId).SetElementName("user");
                    map.MapProperty(u => u.ChatId).SetElementName("chat");
                    map.MapProperty(u => u.AgencyDbRef).SetElementName("agency");
                    map.MapProperty(u => u.CreatedAt).SetElementName("created_at");
                    map.MapProperty(u => u.ModifiedAt).SetElementName("modified_at").SetIgnoreIfDefault(true);
                });
            }
        }
    }
}
