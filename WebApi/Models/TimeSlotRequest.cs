namespace WebApi.Models
{
    public class TimeSlotRequest
    {
        public List<int> UserIds { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
