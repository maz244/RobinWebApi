namespace WebApi.Models
{
    public class Event
    {
        public int EventId { get; set; }

        public string Title { get; set; }

        public DateTime Start { get; set; }

        public DateTime End { get; set; }

        public ICollection<UserEvent> UserEvents { get; set; }
    }
}
