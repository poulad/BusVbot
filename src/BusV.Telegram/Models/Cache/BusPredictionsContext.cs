using System.Collections.Generic;
using Newtonsoft.Json;

namespace BusV.Telegram.Models.Cache
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class BusPredictionsContext
    {
        [JsonProperty("q")]
        public string Query { get; set; }

        [JsonProperty("route")]
        public string RouteTag { get; set; }

        [JsonProperty("dir")]
        public string DirectionTag { get; set; }

        [JsonProperty("origin")]
        public string Origin { get; set; }

        [JsonProperty("interface")]
        public IList<string> Interfaces { get; set; }
    }
}
