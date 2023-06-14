using Newtonsoft.Json;

namespace WebApi.Models
{
    public class User
    {
        public int UserId { get; set; }
    
        public string TimeZone { get; set; }

        public WorkingHours WorkingHours { get; set; }

        public ICollection<UserEvent> UserEvents { get; set; }
    }
}