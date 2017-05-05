using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using MyTTCBot.Models;
using MyTTCBot.Models.NextBus;
using Newtonsoft.Json;

namespace MyTTCBot.Services
{
    public class NextBusService : INextBusService
    {
        private readonly HttpClient _client;

        private const string Url = "http://webservices.nextbus.com/service/publicJSONFeed";

        public NextBusService()
        {
            _client = new HttpClient { BaseAddress = new Uri(Url) };
        }

        public async Task<PredictionsResponse> GetPredictions(string busNumber, string stopId)
        {
            var parameters = new Dictionary<string, object>
            {
                {"command", "predictions"},
                { "a", "ttc"},
                {"r", busNumber },
                {"s", stopId },
            };
            var queryString = BuildQueryString(parameters);
            var httpResponse = await _client.GetAsync('?' + queryString);
            var content = await httpResponse.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<PredictionsResponse>(content);
            return response;
        }

        public async Task<string> FindNearestStopId(string busNumber, BusDirection dir, double longitude, double latitude)
        {
            var parameters = new Dictionary<string, object>
            {
                { "command", "routeConfig" },
                { "a", "ttc" },
                { "r", busNumber },
            };

            var queryString = BuildQueryString(parameters);
            try
            {
                var httpResponse = await _client.GetAsync('?' + queryString)
                    .ConfigureAwait(false);
                var content = await httpResponse.Content.ReadAsStringAsync()
                    .ConfigureAwait(false);
                var response = JsonConvert.DeserializeObject<RouteConfigResponse>(content);

                double Difference(double a, double b)
                {
                    return Math.Abs(Math.Abs(a) - Math.Abs(b));
                }

                var tmpStops = response.Route.Direction
                    .Where(x => x.Name.Equals(dir.ToString(), StringComparison.OrdinalIgnoreCase))
                    .Select(x => x.Stop);

                var thatDirectionStopTags = new List<string>();
                foreach (var tmpStop in tmpStops)
                {
                    thatDirectionStopTags.AddRange(tmpStop.Select(x => x.Tag));
                }

                var nearestStops = response.Route.Stop
                    .Where(x => thatDirectionStopTags.Contains(x.Tag))
                    .Select(x => new
                    {
                        Stop = x,
                        LocationDiff = Difference(x.Lat, latitude) + Difference(x.Lon, longitude),
                    })
                    .OrderBy(x => x.LocationDiff)
                    .Select(x => x.Stop)
                    .Take(3)
                    .ToArray();

                return nearestStops.First().Tag;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                throw;
            }
        }

        private string BuildQueryString(Dictionary<string, object> parameters)
        {
            var paramsList = new List<string>();
            foreach (var pair in parameters)
            {
                var key = System.Net.WebUtility.UrlEncode(pair.Key);
                var val = System.Net.WebUtility.UrlEncode(pair.Value.ToString());
                paramsList.Add($"{key}={val}");
            }
            return string.Join("&", paramsList);
        }
    }
}
