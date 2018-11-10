using System;
using BusV.Telegram.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using MongoDB.Driver.Core.Configuration;
using BusV.Data;
using BusV.Data.Entities;

namespace BusV.Telegram.Extensions
{
    internal static class MongoDbExtensions
    {
        /// <summary>
        /// Adds MongoDB services to the app's service collection
        /// </summary>
        public static void AddMongoDb(
            this IServiceCollection services,
            IConfigurationSection mongoSection
        )
        {
            string connectionString = mongoSection.GetValue<string>(nameof(MongoOptions.ConnectionString));
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException($@"Invalid MongoDB connection string: ""{connectionString}"".");
            }

            services.Configure<MongoOptions>(mongoSection);

            string dbName = new ConnectionString(connectionString).DatabaseName;
            services.AddSingleton<IMongoClient, MongoClient>(_ => new MongoClient(connectionString));
            services.AddTransient(provider =>
                provider.GetRequiredService<IMongoClient>().GetDatabase(dbName)
            );

            services.AddTransient<IAgencyRepo, AgencyRepo>();
            services.AddTransient(provider =>
                provider.GetRequiredService<IMongoDatabase>().GetCollection<Agency>("agencies")
            );

            services.AddTransient<IRouteRepo, RouteRepo>();
            services.AddTransient(provider =>
                provider.GetRequiredService<IMongoDatabase>().GetCollection<Route>("routes")
            );

            services.AddTransient<IBusStopRepo, BusStopRepo>();
            services.AddTransient(provider =>
                provider.GetRequiredService<IMongoDatabase>().GetCollection<BusStop>("bus-stops")
            );

            services.AddTransient<IUserProfileRepo, UserProfileRepo>();
            services.AddTransient(provider =>
                provider.GetRequiredService<IMongoDatabase>().GetCollection<UserProfile>("users")
            );

            MongoInitializer.RegisterClassMaps();
        }
    }
}
