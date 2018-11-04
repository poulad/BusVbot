using Newtonsoft.Json;

namespace BusV.Telegram.Models.Cache
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class BusPredictionsContext
    {
        [JsonProperty("route")]
        public string RouteTag { get; set; }

        [JsonProperty("dir")]
        public string DirectionTag { get; set; }
    }
}
