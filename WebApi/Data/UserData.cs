using Newtonsoft.Json;

namespace WebApi.Data
{
    public class UserData
    {
        [JsonProperty("user_id")]
        public int UserId { get; set; }

        [JsonProperty("time_zone")]
        public string TimeZone { get; set; }

        [JsonProperty("working_hours")]
        public WorkingHoursData WorkingHours { get; set; }

        [JsonProperty("events")]
        public List<EventData> Events { get; set; }
    }
}
