using System.Threading.Tasks;
using MyTTCBot.Models.Cache;
using NetTelegramBotApi.Types;
using NextBus.NET.Models;

namespace MyTTCBot.Services
{
    public interface ITtcBusService
    {
        Task<bool> RouteExists(string busTag);

        Task<bool> RouteExists(string busTag, BusDirection direction);

        Task<Stop> FindNearestBusStop(string busTag, BusDirection dir, Location location);

        Task<BusDirection[]> FindDirectionsForRoute(string routeTag);

        Task<RoutePrediction[]> GetPredictionsForRoute(string stopTag, string routeTag);
    }
}
