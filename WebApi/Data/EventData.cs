using Newtonsoft.Json;

namespace WebApi.Data
{
    public class EventData
    {

        [JsonProperty("id")]
        public int EventId { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("start")]
        public string Start { get; set; }

        [JsonProperty("end")]
        public string End { get; set; }
    }
}
