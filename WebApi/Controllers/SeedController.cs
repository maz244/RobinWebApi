using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApi.Data;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SeedController : ControllerBase
    {
        private readonly DatabaseSeeder _databaseSeeder;

        public SeedController(DatabaseSeeder databaseSeeder)
        {
            _databaseSeeder = databaseSeeder;
        }

        [HttpPost]
        public IActionResult SeedDatabase()
        {
            string jsonFilePath = @"C:\Users\Maznik\Desktop\test2.json.txt";
            _databaseSeeder.SeedDataFromJson(jsonFilePath);
            return Ok("Database seeding completed successfully!");
        }
    }
}

