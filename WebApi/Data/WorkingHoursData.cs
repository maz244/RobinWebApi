using Newtonsoft.Json;

namespace WebApi.Data
{
    public class WorkingHoursData
    {
        [JsonProperty("start")]
        public string Start { get; set; }

        [JsonProperty("end")]
        public string End { get; set; }
    }
}
