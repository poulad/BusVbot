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
            MongoInitializer.RegisterClassMaps();

            var db = Services.GetRequiredService<IMongoDatabase>();
            var agencyCollection = db.GetCollection<BsonDocument>("agencies");
            var routeCollection = db.GetCollection<BsonDocument>("routes");

            long docsCount = await agencyCollection.CountDocumentsAsync(FilterDefinition<BsonDocument>.Empty);

            if (docsCount > 0) return;

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

            await routeCollection.InsertManyAsync(new[]
            {
                // routes for TTC
                BsonDocument.Parse(@"{
                    tag : ""6"",
                    agency : ""ttc"",
                    created_at : new Date(),
                    title : ""6-Bay"",
                    max_lat : 43.6761999,
                    min_lat : 43.64152,
                    max_lon : -79.36538,
                    min_lon : -79.40196,
                    directions : [
                        {
                            tag : ""6_0_6A"",
                            title : ""South - 6 Bay towards Queens Quay and Sherbourne"",
                            name : ""South"",
                            stops : [ ""264"", ""4165"", ""1642"", ""2410"", ""7542"" ]
                        },
                        {
                            tag : ""6_1_6A"",
                            title : ""North - 6a Bay towards Dupont"",
                            name : ""North"",
                            stops : [ ""14935"", ""4166"", ""14936"", ""14569"", ""5092"" ]
                        }
                    ]
                }"),
                BsonDocument.Parse(@"{
                    tag : ""34"",
                    agency : ""ttc"",
                    created_at : new Date(),
                    title : ""34-Eglinton East"",
                    max_lat : 43.7368499,
                    min_lat : 43.7047599,
                    max_lon : -79.24785,
                    min_lon : -79.4001099,
                    directions : [
                        {
                            tag : ""34_1_34Akes"",
                            title : ""West - 34a Eglinton East towards Eglinton Station"",
                            name : ""West"",
                            stops : [ ""2463"", ""317"", ""4194"", ""8550"", ""15211"" ]
                        },
                        {
                            tag : ""34_0_34C"",
                            title : ""East - 34c Eglinton East towards Flemingdon Park (Grenoble & Spanbridge)"",
                            name : ""East"",
                            stops : [ ""14191"", ""303"", ""24060"", ""6665"", ""24061"" ]
                        }
                    ]
                }"),
            });
        }
    }
}
