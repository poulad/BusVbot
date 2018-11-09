using Newtonsoft.Json;

namespace BusV.Telegram.Models.Cache
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class UserLocationContext
    {
        [JsonProperty("lat")]
        public float Latitude { get; set; }

        [JsonProperty("lon")]
        public float Longitude { get; set; }

        [JsonProperty("location_msg")]
        public int LocationMessageId { get; set; }
    }
}
