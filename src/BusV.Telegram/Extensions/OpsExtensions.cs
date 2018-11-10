using Microsoft.Extensions.DependencyInjection;
using BusV.Ops;
using NextBus.NET;

namespace BusV.Telegram.Extensions
{
    internal static class OpsExtensions
    {
        /// <summary>
        /// Adds operations services to the app's service collection
        /// </summary>
        public static void AddOperationServices(
            this IServiceCollection services
        )
        {
            services.AddTransient<INextBusClient, NextBusClient>();

            services.AddScoped<IDataSeeder, DataSeeder>();

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ILocationService, LocationService>();
            services.AddScoped<IPredictionsService, PredictionsService>();

            services.AddScoped<IAgencyRouteParser, AgencyRouteParser>();
        }
    }
}
