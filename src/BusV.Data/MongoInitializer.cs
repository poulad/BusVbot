using System;
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
    public static class MongoInitializer
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
                var agenciesCollection = database.GetCollection<Agency>(Constants.Collections.Agencies.Name);

                // create unique index "agency_name" on the field "tag"
                var key = Builders<Agency>.IndexKeys.Ascending(a => a.Tag);
                await agenciesCollection.Indexes.CreateOneAsync(new CreateIndexModel<Agency>(
                        key,
                        new CreateIndexOptions
                            { Name = Constants.Collections.Agencies.Indexes.AgencyName, Unique = true }),
                    cancellationToken: cancellationToken
                ).ConfigureAwait(false);
            }

            {
                // "routes" collection
                await database.CreateCollectionAsync(
                    Constants.Collections.Routes.Name,
                    cancellationToken: cancellationToken
                ).ConfigureAwait(false);
                var routesCollection = database.GetCollection<Route>(Constants.Collections.Routes.Name);

                // create unique index "tag_agency" on the fields "tag" and "agency"
                var indexBuilder = Builders<Route>.IndexKeys;
                var key = indexBuilder.Combine(
                    indexBuilder.Ascending(r => r.Tag),
                    indexBuilder.Ascending(r => r.AgencyTag)
                );
                await routesCollection.Indexes.CreateOneAsync(new CreateIndexModel<Route>(
                        key,
                        new CreateIndexOptions
                            { Name = Constants.Collections.Routes.Indexes.TagAgency, Unique = true }),
                    cancellationToken: cancellationToken
                ).ConfigureAwait(false);
            }

            {
                // "bus_stops" collection
                await database.CreateCollectionAsync("bus_stops", cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                var busStopsCollection = database.GetCollection<BusStop>("bus_stops");

//                // create unique index "tag" on the field "tag"
//                await busStopsCollection.Indexes.CreateOneAsync(new CreateIndexModel<BusStop>(
//                        Builders<BusStop>.IndexKeys.Ascending(s => s.Tag),
//                        new CreateIndexOptions { Name = "tag", Unique = true }),
//                    cancellationToken: cancellationToken
//                ).ConfigureAwait(false);

                // create unique 2dsphere index "location" on the field "location"
                await busStopsCollection.Indexes.CreateOneAsync(new CreateIndexModel<BusStop>(
                        Builders<BusStop>.IndexKeys.Geo2DSphere(s => s.Location),
                        new CreateIndexOptions { Name = "location" }),
                    cancellationToken: cancellationToken
                ).ConfigureAwait(false);
            }

            {
                // "users" collection
                await database.CreateCollectionAsync(
                    Constants.Collections.Users.Name,
                    cancellationToken: cancellationToken
                ).ConfigureAwait(false);
                var usersCollection = database.GetCollection<UserProfile>(Constants.Collections.Users.Name);

                // create unique index "user_chat" on the fields "user" and "chat"
                var indexBuilder = Builders<UserProfile>.IndexKeys;
                var key = indexBuilder.Combine(
                    indexBuilder.Ascending(u => u.UserId),
                    indexBuilder.Ascending(u => u.ChatId)
                );
                await usersCollection.Indexes.CreateOneAsync(new CreateIndexModel<UserProfile>(
                        key,
                        new CreateIndexOptions
                            { Name = Constants.Collections.Users.Indexes.UserChat, Unique = true }),
                    cancellationToken: cancellationToken
                ).ConfigureAwait(false);
            }

            {
                // "bus_predictions" collection
                await database.CreateCollectionAsync("bus_predictions", cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                var busStopsCollection = database.GetCollection<BusPrediction>("bus_predictions");

                // create 2dsphere index "user_location" on the field "user_location"
                await busStopsCollection.Indexes.CreateOneAsync(new CreateIndexModel<BusPrediction>(
                        Builders<BusPrediction>.IndexKeys.Geo2DSphere(p => p.UserLocation),
                        new CreateIndexOptions { Name = "user_location" }),
                    cancellationToken: cancellationToken
                ).ConfigureAwait(false);

                // create index "created_at" on the field "created_at" with 1 day of TTL
                await busStopsCollection.Indexes.CreateOneAsync(new CreateIndexModel<BusPrediction>(
                        Builders<BusPrediction>.IndexKeys.Ascending(p => p.CreatedAt),
                        new CreateIndexOptions { Name = "created_at", ExpireAfter = TimeSpan.FromDays(1) }),
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

            if (!BsonClassMap.IsClassMapRegistered(typeof(Route)))
            {
                BsonClassMap.RegisterClassMap<Route>(map =>
                {
                    map.MapIdProperty(r => r.Id)
                        .SetIdGenerator(StringObjectIdGenerator.Instance)
                        .SetSerializer(new StringSerializer(BsonType.ObjectId));
                    map.MapProperty(r => r.Tag).SetElementName("tag").SetOrder(1);
                    map.MapProperty(r => r.AgencyTag).SetElementName("agency").SetOrder(2);
                    map.MapProperty(r => r.CreatedAt).SetElementName("created_at");
                    map.MapProperty(r => r.Title).SetElementName("title").SetIgnoreIfDefault(true);
                    map.MapProperty(r => r.ShortTitle).SetElementName("short_title").SetIgnoreIfDefault(true);
                    map.MapProperty(r => r.Directions).SetElementName("directions").SetIgnoreIfDefault(true);
                    map.MapProperty(r => r.MaxLatitude).SetElementName("max_lat").SetIgnoreIfDefault(true);
                    map.MapProperty(r => r.MinLatitude).SetElementName("min_lat").SetIgnoreIfDefault(true);
                    map.MapProperty(r => r.MaxLongitude).SetElementName("max_lon").SetIgnoreIfDefault(true);
                    map.MapProperty(r => r.MinLongitude).SetElementName("min_lon").SetIgnoreIfDefault(true);
                    map.MapProperty(r => r.ModifiedAt).SetElementName("modified_at").SetIgnoreIfDefault(true);
                });
            }

            if (!BsonClassMap.IsClassMapRegistered(typeof(BusStop)))
            {
                BsonClassMap.RegisterClassMap<BusStop>(map =>
                {
                    map.MapIdProperty(s => s.Id)
                        .SetIdGenerator(StringObjectIdGenerator.Instance)
                        .SetSerializer(new StringSerializer(BsonType.ObjectId));
                    map.MapProperty(s => s.Tag).SetElementName("tag").SetOrder(1);
                    map.MapProperty(s => s.Location).SetElementName("location").SetOrder(3);
                    map.MapProperty(s => s.CreatedAt).SetElementName("created_at");
                    map.MapProperty(s => s.Title).SetElementName("title").SetOrder(2).SetIgnoreIfDefault(true);
                    map.MapProperty(s => s.ShortTitle).SetElementName("short_title").SetIgnoreIfDefault(true);
                    map.MapProperty(s => s.StopId).SetElementName("stop_id").SetIgnoreIfDefault(true);
                    map.MapProperty(s => s.ModifiedAt).SetElementName("modified_at").SetIgnoreIfDefault(true);
                });
            }

            if (!BsonClassMap.IsClassMapRegistered(typeof(RouteDirection)))
            {
                BsonClassMap.RegisterClassMap<RouteDirection>(map =>
                {
                    map.MapProperty(d => d.Tag).SetElementName("tag");
                    map.MapProperty(d => d.Title).SetElementName("title");
                    map.MapProperty(d => d.Name).SetElementName("name").SetIgnoreIfDefault(true);
                    map.MapProperty(d => d.Stops).SetElementName("stops");
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
                    map.MapProperty(u => u.DefaultAgencyTag).SetElementName("agency");
                    map.MapProperty(u => u.CreatedAt).SetElementName("created_at");
                    map.MapProperty(u => u.ModifiedAt).SetElementName("modified_at").SetIgnoreIfDefault(true);
                });
            }

            if (!BsonClassMap.IsClassMapRegistered(typeof(BusPrediction)))
            {
                BsonClassMap.RegisterClassMap<BusPrediction>(map =>
                {
                    map.MapIdProperty(s => s.Id)
                        .SetIdGenerator(StringObjectIdGenerator.Instance)
                        .SetSerializer(new StringSerializer(BsonType.ObjectId));
                    map.MapProperty(p => p.AgencyTag).SetElementName("agency");
                    map.MapProperty(p => p.RouteTag).SetElementName("route");
                    map.MapProperty(p => p.DirectionTag).SetElementName("direction");
                    map.MapProperty(p => p.BusStopTag).SetElementName("stop");
                    map.MapProperty(p => p.User).SetElementName("user");
                    map.MapProperty(p => p.UserLocation).SetElementName("user_location");
                    map.MapProperty(s => s.CreatedAt).SetElementName("created_at");
                    map.MapProperty(s => s.UpdatedAt).SetElementName("updated_at");
                });
            }
        }
    }
}
