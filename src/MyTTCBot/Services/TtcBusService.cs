using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MyTTCBot.Extensions;
using MyTTCBot.Models.Cache;
using NetTelegramBotApi.Types;
using NextBus.NET;
using NextBus.NET.Models;

namespace MyTTCBot.Services
{
    public class TtcBusService : ITtcBusService
    {
        private readonly INextBusClient _client;

        public TtcBusService(INextBusClient client)
        {
            _client = client;
        }

        public async Task<bool> RouteExists(string busTag)
        {
            bool exists;
            try
            {
                var routeConfigs = await _client.GetRouteConfig("ttc", busTag);
                exists = routeConfigs != null;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                // ToDo: Log
                exists = false;
            }
            return exists;
        }

        public async Task<bool> RouteExists(string busTag, BusDirection direction)
        {
            bool exists;
            try
            {
                var routeConfigs = await _client.GetRouteConfig("ttc", busTag);
                exists = routeConfigs.Directions
                    .Any(d => d.Name.Equals(direction.ToString(), StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                // ToDo: Log
                exists = false;
            }
            return exists;
        }

        public async Task<Stop> FindNearestBusStop(string busTag, BusDirection dir, Location location)
        {
            try
            {
                // assume bus tag exists
                var routeConfigs = await _client.GetRouteConfig("ttc", busTag);

                double Difference(double a, double b)
                {
                    return Math.Abs(Math.Abs(a) - Math.Abs(b));
                }

                var stopTagsInThatDirection = routeConfigs.Directions
                    .Where(d => d.Name.Equals(dir + "", StringComparison.OrdinalIgnoreCase))
                    .SelectMany(d => d.StopTags)
                    .ToArray();
                // ToDo: Fix logic for finding stops in right direction
                var nearestStop = routeConfigs.Stops
                    .Where(s => stopTagsInThatDirection.Any(tag => tag == s.Tag))
                    .Select(s => new
                    {
                        Stop = s,
                        Distance = Difference(location.Latitude, (double)s.Lat) +
                                   Difference(location.Longitude, (double)s.Lon)
                    })
                    .OrderBy(x => x.Distance)
                    .Select(x => x.Stop)
                    .FirstOrDefault();

                return nearestStop;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                throw;
            }
        }

        public async Task<BusDirection[]> FindDirectionsForRoute(string routeTag)
        {
            var config = await _client.GetRouteConfig(Constants.TtcAgentTag, routeTag);

            var directions = config.Directions.Select(x => x.Name.ParseBusDirectionOrNull())
                .Where(x => x != null)
                .Select(x => (BusDirection)x)
                .Distinct()
                .ToArray();

            return directions;
        }

        public async Task<RoutePrediction[]> GetPredictionsForRoute(string stopTag, string routeTag)
        {
            var predictions = await _client.GetRoutePredictionsByStopTag(Constants.TtcAgentTag, stopTag, routeTag);
            return predictions.ToArray();
        }

        private static class Constants
        {
            public const string TtcAgentTag = "ttc";
        }
    }
}
