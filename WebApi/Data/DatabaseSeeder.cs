using WebApi.Models;
using System.IO;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace WebApi.Data
{
    public class DatabaseSeeder
    {
        private readonly DataContext _dbContext;

        public DatabaseSeeder(DataContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void SeedDataFromJson(string jsonFilePath)
        {
            // Read the JSON file
            string jsonContent = File.ReadAllText(jsonFilePath);

            // Deserialize JSON data into objects
            List<UserData> userDataList = JsonConvert.DeserializeObject<List<UserData>>(jsonContent);

            // Convert the UserData objects into User and Event entities and add them to the database
            foreach (UserData userData in userDataList)
            {
                User user = new User
                {
                    UserId = userData.UserId,
                    TimeZone = userData.TimeZone,
                    WorkingHours = new WorkingHours
                    {
                        Start = userData.WorkingHours.Start,
                        End = userData.WorkingHours.End
                    },
                    UserEvents = new List<UserEvent>()
                };

                foreach (EventData eventData in userData.Events)
                {
                    Event @event = new Event
                    {
                        EventId = eventData.EventId,
                        Title = eventData.Title,
                        Start = DateTime.Parse(eventData.Start).AddHours(-1),
                        End = DateTime.Parse(eventData.End).AddHours(-1),
                        UserEvents = new List<UserEvent>()
                    };

                    UserEvent userEvent = new UserEvent
                    {
                        User = user,
                        Event = @event
                    };

                    user.UserEvents.Add(userEvent);
                    @event.UserEvents.Add(userEvent);
                }

                _dbContext.Users.Add(user);
            }

            // Save the changes to the database
            _dbContext.SaveChanges();
        }
    }
}