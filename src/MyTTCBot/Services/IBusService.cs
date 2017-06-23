using System.Threading.Tasks;
using MyTTCBot.Models.Cache;
using Telegram.Bot.Types;
using NextBus.NET.Models;

namespace MyTTCBot.Services
{
    public interface IBusService
    {
        Task<bool> RouteExists(string agencyTag, string busTag);

        Task<bool> RouteExists(string agencyTag, string busTag, TtcBusDirection direction);

        Task<Stop> FindNearestBusStop(string agencyTag, string busTag, TtcBusDirection dir, Location location);

        Task<TtcBusDirection[]> FindDirectionsForRoute(string agencyTag, string routeTag);

        Task<RoutePrediction[]> GetPredictionsForRoute(string agencyTag, string stopTag, string routeTag);
    }
}
