using System.Threading.Tasks;
using BusV.Telegram.Data;
using Microsoft.AspNetCore.Builder;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class DataSeederIApplicationBuilderExtensions
    {
        public static async Task EnsureDatabaseSeededAsync(this IApplicationBuilder app, bool includeTestData)
        {
            using (IServiceScope scope = app.ApplicationServices.CreateScope())
            {
                var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
                await seeder.SeedDatabaseAsync(includeTestData);
            }
        }
    }
}
