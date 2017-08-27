using System.Threading.Tasks;
using BusVbot.Models.Cache;
using NextBus.NET.Models;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types;

namespace BusVbot.Services
{
    public interface IPredictionsManager
    {
        bool ValidateRouteFormat(string routeTag);

        Task<string> GetSampleRouteTextAsync(UserChat userchat);

        Task TryReplyWithPredictionsAsync(IBot bot, UserChat userchat, int replyToMessageId);

        Task<(Location BusStopLocation, RoutePrediction[] Predictions)> GetPredictionsReplyAsync
            (Location userLocation, string agencyTag, string routeTag, string direction);

        (bool Success, string Route, string Direction) TryParseToRouteDirection(string input);
        
        Task<(string RouteTag, string Direction)> GetCachedRouteDirectionAsync(UserChat userchat);

        Task CacheRouteDirectionAsync(UserChat userchat, string routeTag, string direction);        
    }
}
