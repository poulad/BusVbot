using BusV.Telegram.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wit.Ai.Client;

namespace BusV.Telegram.Extensions
{
    internal static class WitAiExtensions
    {
        /// <summary>
        /// Adds Wit.ai services to the app's service collection
        /// </summary>
        public static void AddWitAi(
            this IServiceCollection services,
            IConfigurationSection witAiSection
        )
        {
            services.Configure<WitAiOptions>(witAiSection);

            string token = witAiSection[nameof(WitAiOptions.AccessToken)];
            services.AddTransient<IWitClient, WitClient>(_ =>
                new WitClient(token)
            );
        }
    }
}
