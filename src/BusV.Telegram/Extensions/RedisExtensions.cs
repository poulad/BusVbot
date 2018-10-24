using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BusV.Telegram.Extensions
{
    internal static class RedisExtensions
    {
        /// <summary>
        /// Adds Redis services to the app's service collection
        /// </summary>
        public static void AddRedisCache(
            this IServiceCollection services,
            IConfigurationSection dataSection
        )
        {
            services.AddDistributedRedisCache(options => { options.Configuration = "localhost"; });
        }
    }
}
