using BusV.Telegram.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BusV.Telegram.Extensions
{
    internal static class RedisExtensions
    {
        /// <summary>
        /// Adds Redis services to the app's service collection
        /// </summary>
        public static void AddRedisCache(
            this IServiceCollection services,
            IConfigurationSection redisSection
        )
        {
            services.AddDistributedRedisCache(options =>
            {
                options.Configuration = redisSection[nameof(RedisOptions.Configuration)];
            });
        }

        public static void EnsureRedisConnection(
            this IApplicationBuilder app
        )
        {
            var logger = app.ApplicationServices.GetRequiredService<ILogger<Startup>>();
            logger.LogDebug("Checking Redis connection");

            using (IServiceScope scope = app.ApplicationServices.CreateScope())
            {
                var cache = scope.ServiceProvider.GetRequiredService<IDistributedCache>();
                string _ = cache.GetString("foo");
            }
        }
    }
}
