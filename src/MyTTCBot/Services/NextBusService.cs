using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
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

        public async Task<PredictionsResponse> GetPredictions()
        {
            var parameters = new Dictionary<string, object>
            {
                {"command", "predictions"},
                { "a", "ttc"},
                {"r", 97 },
                {"s", 14327 },
            };
            var queryString = BuildQueryString(parameters);
            var httpResponse = await _client.GetAsync('?' + queryString);
            var content = await httpResponse.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<PredictionsResponse>(content);
            return response;
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
