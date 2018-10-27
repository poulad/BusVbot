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
            IConfigurationSection dataSection
        )
        {
            string connectionString = dataSection.GetValue<string>(nameof(MongoOptions.ConnectionString));
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException($@"Invalid MongoDB connection string: ""{connectionString}"".");
            }

            services.Configure<MongoOptions>(dataSection);

            string dbName = new ConnectionString(connectionString).DatabaseName;
            services.AddSingleton<IMongoClient, MongoClient>(_ => new MongoClient(connectionString));
            services.AddTransient(provider =>
                provider.GetRequiredService<IMongoClient>().GetDatabase(dbName)
            );

            services.AddTransient<IAgencyRepo, AgencyRepo>();
            services.AddTransient(provider => provider.GetRequiredService<IMongoDatabase>()
                .GetCollection<Agency>(BusV.Data.Constants.Collections.Agencies.Name)
            );

            services.AddTransient<IRouteRepo, RouteRepo>();
            services.AddTransient(provider => provider.GetRequiredService<IMongoDatabase>()
                .GetCollection<Route>(BusV.Data.Constants.Collections.Routes.Name)
            );

            services.AddTransient<IUserProfileRepo, UserProfileRepo>();
            services.AddTransient(provider => provider.GetRequiredService<IMongoDatabase>()
                .GetCollection<UserProfile>(BusV.Data.Constants.Collections.Users.Name)
            );

            Initializer.RegisterClassMaps();
        }
    }
}
