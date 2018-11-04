using System;
using System.Net.Http;
using System.Threading.Tasks;
using BusV.Data;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;
using Telegram.Bot;

namespace TelegramTests.Shared
{
    public class TestsFixture
    {
        public HttpClient HttpClient { get; }

        public Mock<ITelegramBotClient> MockBotClient => WebAppFactory.MockBotClient;

        public IDistributedCache Cache => Services.GetRequiredService<IDistributedCache>();

        public IServiceProvider Services => WebAppFactory.Server.Host.Services;

        public WebAppFactory WebAppFactory { get; }

        public TestsFixture()
        {
            WebAppFactory = new WebAppFactory();
            HttpClient = WebAppFactory.CreateClient();

            InitDbAsync().GetAwaiter().GetResult();
        }

        private async Task InitDbAsync()
        {
            var db = Services.GetRequiredService<IMongoDatabase>();
            var agencyCollection = db.GetCollection<BsonDocument>("agencies");

            long docsCount = await agencyCollection.CountDocumentsAsync(FilterDefinition<BsonDocument>.Empty);

            if (docsCount > 0) return;

            MongoInitializer.RegisterClassMaps();
            await MongoInitializer.CreateSchemaAsync(db);

            await agencyCollection.InsertManyAsync(new[]
            {
                // Test Agency from NextBus
                BsonDocument.Parse(@"{
                    tag: ""configdev"",
                    created_at: new Date(),
                    title: ""Config Stuff"",
                    region: ""Other"",
                    country: ""Test"",
                    max_lat: 39.3075428,
                    max_lon: -76.5711916,
                    min_lat: 38.9488713,
                    min_lon: -77.211631
                }"),

                // Single TTC agency for Toronto
                BsonDocument.Parse(@"{
                    tag: ""ttc"",
                    created_at: new Date(),
                    title: ""Toronto Transit Commission"",
                    region: ""Ontario"",
                    country: ""Canada"",
                    short_title: ""Toronto TTC"",
                    max_lat: 43.9095299,
                    max_lon: -79.12305,
                    min_lat: 43.5918099,
                    min_lon: -79.6499
                }"),

                // 3 Agencies for Los Angles
                BsonDocument.Parse(@"{
                    tag: ""lametro"",
                    created_at: new Date(),
                    title: ""Los Angeles Metro"",
                    region: ""California-Southern"",
                    country: ""USA"",
                    max_lat: 34.3261599,
                    max_lon: -117.9134699,
                    min_lat: 33.70685,
                    min_lon: -118.86091
                }"),
                BsonDocument.Parse(@"{
                    tag: ""lametro-rail"",
                    created_at: new Date(),
                    title: ""Los Angeles Rail"",
                    region: ""California-Southern"",
                    country: ""USA"",
                    max_lat: 34.1684999,
                    max_lon: -117.89164,
                    min_lat: 33.7680699,
                    min_lon: -118.49138
                }"),
                BsonDocument.Parse(@"{
                    tag: ""pvpta"",
                    created_at: new Date(),
                    title: ""Palos Verdes Transit"",
                    region: ""California-Southern"",
                    country: ""USA"",
                    max_lat: 33.81904,
                    max_lon: -118.28767,
                    min_lat: 33.7273099,
                    min_lon: -118.42314
                }"),
            });
        }
    }
}
