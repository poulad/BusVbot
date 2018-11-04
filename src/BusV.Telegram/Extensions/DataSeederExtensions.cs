using BusV.Data;
using BusV.Ops;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace BusV.Telegram.Extensions
{
    internal static class DataSeederExtensions
    {
        public static void EnsureDatabaseSeeded(
            this IApplicationBuilder app
        )
        {
            var logger = app.ApplicationServices.GetRequiredService<ILogger<Startup>>();
            logger.LogDebug("Seeding the database");

            using (IServiceScope scope = app.ApplicationServices.CreateScope())
            {
                var seeder = scope.ServiceProvider.GetRequiredService<IDataSeeder>();
                bool isEmpty = seeder.IsEmptyAsync().GetAwaiter().GetResult();
                if (isEmpty)
                {
                    seeder.SeedAgenciesAsync().GetAwaiter().GetResult();
                }
            }
        }

        public static void EnsureDatabaseSchema(
            this IApplicationBuilder app
        )
        {
            var logger = app.ApplicationServices.GetRequiredService<ILogger<Startup>>();

            using (IServiceScope scope = app.ApplicationServices.CreateScope())
            {
                var database = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();
                var collections = database.ListCollectionNamesAsync().Result.ToListAsync().Result;
                if (collections.Contains("agencies"))
                {
                    logger.LogDebug("Database already has the \"agencies\" collection. Skipping.");
                }
                else
                {
                    logger.LogInformation("Creating the database schema");
                    MongoInitializer.CreateSchemaAsync(database).GetAwaiter().GetResult();
                }
            }
        }
    }
}
