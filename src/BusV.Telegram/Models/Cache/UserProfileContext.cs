using Newtonsoft.Json;

namespace BusV.Telegram.Models.Cache
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class UserProfileContext
    {
        [JsonProperty("instructions_sent")]
        public bool IsInstructionsSent { get; set; }

        [JsonProperty("agency_selection_msg")]
        public int AgencySelectionMessageId { get; set; }

        [JsonProperty("location_msg")]
        public int LocationSharingMessageId { get; set; }
    }
}
