using BusV.Ops;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BusV.Telegram.Extensions
{
    internal static class DataSeederExtensions
    {
        public static void EnsureDatabaseSeededAsync(
            this IApplicationBuilder app
        )
        {
            var logger = app.ApplicationServices.GetRequiredService<ILogger<Startup>>();
            logger.LogDebug("Seeding the database");

            using (IServiceScope scope = app.ApplicationServices.CreateScope())
            {
                var seeder = scope.ServiceProvider.GetRequiredService<IDataSeeder>();
                seeder.UpdateAllAgenciesAsync().GetAwaiter().GetResult();
            }
        }
    }
}
