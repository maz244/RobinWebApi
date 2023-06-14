using System.ComponentModel.DataAnnotations.Schema;

namespace WebApi.Models
{
    [ComplexType]
    public class WorkingHours
    {
        public int WorkingHoursId { get; set; }
        public string Start { get; set; }

        public string End { get; set; }

    }
}
