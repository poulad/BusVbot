using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Wit.Ai.Client.Types
{
    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    public class Meaning
    {
        [JsonProperty("_text")]
        public string Text { get; set; }

        [JsonProperty("msg_id")]
        public string MessageId { get; set; }

        public IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>[]> Entities { get; set; }
    }
}
