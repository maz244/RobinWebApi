namespace WebApi.Models
{
    public class TimeSlotResponse
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public TimeSlotResponse(DateTime start, DateTime end)
        {
            Start = start;
            End = end;
        }
    }
}


