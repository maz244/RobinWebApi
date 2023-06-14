using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using WebApi.Data;
using WebApi.Models;
using WebApi.Service.TimeSlotService;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TimeSlotController : ControllerBase
    {
        private readonly TimeSlotService _timeSlotService;
        private readonly DataContext _dbContext;

        public TimeSlotController(TimeSlotService timeSlotService, DataContext dbContext)
        {
            _timeSlotService = timeSlotService;
            _dbContext = dbContext;
        }

        [HttpPost("Challenge1")]
        public IActionResult  FindOverlappingTimes([FromBody] TimeSlotRequest model)
        {           
            List<DateTime> overlappingTimes = _timeSlotService.OverlappingMultipleUserEventsWithinGivenTimeSpan(model);

            return Ok(overlappingTimes);
        }

        [HttpPost("Challenge2")]
        public IActionResult FindOverlappingTimesForWorkingHours([FromBody] TimeSlotRequest model)
        {
            List<DateTime> response = _timeSlotService.OverlappingEventsWithinWorkingHours(model);

            return Ok(response);


        }

        //[HttpGet("Challenge3")]
        //public IActionResult ReturnFunc()
        //{
        //    Dictionary<int, List<DateTime>> response = _timeSlotService.ReturnFunc();

        //    return Ok(response);


        //}
    }
}


