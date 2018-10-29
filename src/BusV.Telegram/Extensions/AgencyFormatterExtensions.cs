using BusV.Telegram.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace BusV.Telegram.Extensions
{
    internal static class AgencyFormatterExtensions
    {
        public static void AddMessageFormattingServices(
            this IServiceCollection services,
            IConfigurationSection agenciesSection // ToDo
        )
        {
            services.AddTransient<IRouteMessageFormatter, RouteMessageFormatter>();

//            services.Configure<AgencyTimeZonesAccessor>(agenciesSection);

//            services.AddTransient<TtcMessageFormatter>();
//            services.AddTransient<IAgencyServiceAccessor, AgencyServiceAccessor>(factory =>
//            {
//                var parsers = new IAgencyDataParser[] { factory.GetRequiredService<TtcDataParser>() };
//                var formatters = new IAgencyMessageFormatter[] { factory.GetRequiredService<TtcMessageFormatter>() };
//                return new AgencyServiceAccessor(
//                    factory.GetRequiredService<IDefaultAgencyDataParser>(),
//                    factory.GetRequiredService<IDefaultAgencyMessageFormatter>(),
//                    parsers,
//                    formatters
//                );
//            });
        }
    }
}
