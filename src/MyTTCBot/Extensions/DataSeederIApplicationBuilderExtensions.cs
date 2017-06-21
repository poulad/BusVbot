using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using MyTTCBot.Data;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DataSeederIApplicationBuilderExtensions
    {
        public static async Task EnsureDatabaseSeededAsync(this IApplicationBuilder app)
        {
            using (IServiceScope scope = app.ApplicationServices.CreateScope())
            {
                var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
                await seeder.SeedDatabaseAsync();
            }
        }
    }
}
