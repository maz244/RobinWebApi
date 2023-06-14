using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using WebApi.Data;
using WebApi.Models;

namespace WebApi.Service.TimeSlotService
{
    public class TimeSlotService
    {
        private readonly DataContext _dbContext;

        public TimeSlotService(DataContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Dictionary<int, List<DateTime>> CreateDictionary(DataContext dbContext)
        {
            Dictionary<int, List<DateTime>> userEventsDictionary = new Dictionary<int, List<DateTime>>();

            var userEvents = dbContext.UserEvents.Include(ue => ue.Event).ToList();

            foreach (var userEvent in userEvents)
            {
                int userId = userEvent.UserId;
                DateTime start = userEvent.Event.Start;
                DateTime end = userEvent.Event.End;

                if (!userEventsDictionary.ContainsKey(userId))
                {
                    userEventsDictionary[userId] = new List<DateTime>();
                }

                userEventsDictionary[userId].AddRange(new[] { start, end });

            }

            foreach (var userId in userEventsDictionary.Keys)
            {
                var userEventsList = userEventsDictionary[userId];

                DateTime dayStart = userEventsList.Min().Date;
                DateTime dayEnd = dayStart.AddDays(1).AddSeconds(-1);

                if (!userEventsList.Contains(dayStart))
                {
                    userEventsList.Insert(0, dayStart);
                }

                if (!userEventsList.Contains(dayEnd))
                {
                    userEventsList.Add(dayEnd);
                }
            }

            return userEventsDictionary;

        }

        public List<DateTime> OverlappingTwoUserEvents(List<DateTime> list1, List<DateTime> list2)
        {
            List<DateTime> overlappingTimes = new List<DateTime>();

            int i = 0;
            int j = 0;

            var firstEventStart = new DateTime();
            var secondEventStart = new DateTime();
            var firstEventEnd = new DateTime();
            var secondEventEnd = new DateTime();
            var maxStartTime = new DateTime();
            var minEndTime = new DateTime();


            while ((i < list1.Count) && (j < list2.Count))
            {


                firstEventStart = list1[i];
                secondEventStart = list2[j];
                firstEventEnd = list1[i + 1];
                secondEventEnd = list2[j + 1];



                maxStartTime = DateTime.Compare(firstEventStart, secondEventStart) >= 0 ? firstEventStart : secondEventStart;
                minEndTime = DateTime.Compare(firstEventEnd, secondEventEnd) < 0 ? firstEventEnd : secondEventEnd;

                if (maxStartTime < minEndTime)
                {
                    overlappingTimes.Add(maxStartTime);
                    overlappingTimes.Add(minEndTime);
                }

                if (firstEventEnd > secondEventEnd)
                {
                    j = j + 2;
                }
                else
                {
                    i = i + 2;
                }


            }

            return overlappingTimes;
        }

        public List<DateTime> OverlappingMultipleUserEvents(Dictionary<int, List<DateTime>> userEventsDictionary)
        {
            List<DateTime> overlappingTimes = new List<DateTime>();
            var keys = userEventsDictionary.Keys.ToList();

            if (keys.Count <= 1)
            {
                return overlappingTimes;
            }

            var firstKey = keys[0];
            var firstList = userEventsDictionary[firstKey];

            for (int i = 1; i < keys.Count; i++)
            {
                var currentKey = keys[i];
                var currentList = userEventsDictionary[currentKey];

                List<DateTime> overlapping = OverlappingTwoUserEvents(firstList, currentList);
                overlappingTimes.AddRange(overlapping);

                
                firstList = overlapping;
                firstKey = currentKey;
            }

            // Remove duplicates from the overlapping times
            overlappingTimes = overlappingTimes.Distinct().ToList();

            return overlappingTimes;
        }




        public List<DateTime> OverlappingMultipleUserEventsWithinGivenTimeSpan(TimeSlotRequest request)
        {
            Dictionary<int, List<DateTime>> userEventsDictionary = CreateDictionary(_dbContext);

            List<DateTime> overlappingTimes = OverlappingMultipleUserEvents(userEventsDictionary);

            DateTime firstDate = request.StartTime;
            DateTime lastDate = request.EndTime;

            int startIndex = overlappingTimes.FindIndex(date => date >= firstDate);
            int endIndex = overlappingTimes.FindLastIndex(date => date <= lastDate);

            if (startIndex >= 0 && endIndex >= 0)
            {
                overlappingTimes.RemoveRange(0, startIndex);
                overlappingTimes.RemoveRange(endIndex + 1 - startIndex, overlappingTimes.Count - (endIndex + 1 - startIndex));
                overlappingTimes.Insert(0, firstDate);
                overlappingTimes.Add(lastDate);
            }

            return overlappingTimes;
        }

        public Dictionary<int, List<DateTime>> CreateDictionaryWithWorkingHours(DataContext dbContext)
        {
            Dictionary<int, List<DateTime>> userWorkingHoursDictionary = new Dictionary<int, List<DateTime>>();

            var users = dbContext.Users.Include(u => u.WorkingHours).ToList();

            foreach (var user in users)
            {
                int userId = user.UserId;
                string timeZoneId = user.TimeZone;

                TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

                DateTime startDateTime = DateTime.Parse(user.WorkingHours.Start);
                DateTime endDateTime = DateTime.Parse(user.WorkingHours.End);

                DateTime convertedStartDateTime = TimeZoneInfo.ConvertTimeToUtc(startDateTime, timeZone);
                DateTime convertedEndDateTime = TimeZoneInfo.ConvertTimeToUtc(endDateTime, timeZone);

                if (!userWorkingHoursDictionary.ContainsKey(userId))
                {
                    userWorkingHoursDictionary[userId] = new List<DateTime>();
                }

                userWorkingHoursDictionary[userId].AddRange(new[] { convertedStartDateTime, convertedEndDateTime });
            }
            

            var userEvents = dbContext.UserEvents
                    .GroupBy(ue => ue.UserId)
                    .Select(g => new { UserId = g.Key, MinEventDate = g.Min(ue => ue.Event.Start) })
                    .ToList();

            foreach (var eventInfo in userEvents)
            {
                if (userWorkingHoursDictionary.ContainsKey(eventInfo.UserId))
                {
                    var workingHours = userWorkingHoursDictionary[eventInfo.UserId];
                    DateTime minEventDateTime = eventInfo.MinEventDate.Date;

                    for (int i = 0; i < workingHours.Count; i++)
                    {
                        DateTime dateWithTime = minEventDateTime + workingHours[i].TimeOfDay;
                        workingHours[i] = dateWithTime;
                    }
                }
            }

            return userWorkingHoursDictionary;

        }


        public TimeSlotRequest OverlapBetweenWorkingHoursAndTimeRequest(TimeSlotRequest request, List<DateTime> workingHours)
        {
            DateTime requestStartTime = request.StartTime;
            DateTime requestEndTime = request.EndTime;
            DateTime workingHoursStartTime = workingHours[0];
            DateTime workingHoursEndTime = workingHours[1];

            if (requestEndTime < workingHoursStartTime || requestStartTime > workingHoursEndTime)
            {
                return null; // No overlap
            }

            DateTime overlapStartTime = requestStartTime > workingHoursStartTime ? requestStartTime : workingHoursStartTime;
            DateTime overlapEndTime = requestEndTime < workingHoursEndTime ? requestEndTime : workingHoursEndTime;

            return new TimeSlotRequest
            {
                UserIds = request.UserIds,
                StartTime = overlapStartTime,
                EndTime = overlapEndTime
            };
        }

        public List<DateTime> OverlappingEventsWithinWorkingHours(TimeSlotRequest request)
        {
            Dictionary<int, List<DateTime>> dictionaryWithWorkingHours = CreateDictionaryWithWorkingHours(_dbContext);


            List<DateTime> overlapingEventsList = new List<DateTime>();


            overlapingEventsList = OverlappingMultipleUserEvents(dictionaryWithWorkingHours);


            TimeSlotRequest newRequest = OverlapBetweenWorkingHoursAndTimeRequest(request, overlapingEventsList);

            Console.WriteLine(newRequest);

            List<DateTime> result = OverlappingMultipleUserEventsWithinGivenTimeSpan(newRequest);

            return result;

        }

        //--------------------------------NOTFINISHED-----------------------------------------//

        //public Dictionary<List<int>, List<DateTime>> MergeDictionaries(Dictionary<int, List<DateTime>> dict1, Dictionary<int, List<DateTime>> dict2)
        //{
        //    Dictionary<List<int>, List<DateTime>> mergedDict = new Dictionary<List<int>, List<DateTime>>();

        //    foreach (var kvp1 in dict1)
        //    {
        //        int userKey = kvp1.Key;
        //        List<DateTime> userTimes1 = kvp1.Value;

        //        if (dict2.TryGetValue(userKey, out List<DateTime> userTimes2))
        //        {
        //            List<DateTime> mergedTimes = new List<DateTime>();

        //            for (int i = 0; i < userTimes1.Count; i += 2)
        //            {
        //                DateTime startTime1 = userTimes1[i];
        //                DateTime endTime1 = userTimes1[i + 1];

        //                for (int j = 0; j < userTimes2.Count; j += 2)
        //                {
        //                    DateTime startTime2 = userTimes2[j];
        //                    DateTime endTime2 = userTimes2[j + 1];

        //                    // Check if the values of dict2 are within the intervals of dict1
        //                    if (startTime2 >= startTime1 && endTime2 <= endTime1)
        //                    {
        //                        mergedTimes.AddRange(userTimes2.GetRange(j, 2));
        //                        break;  // Stop checking further intervals in dict2 for this user
        //                    }
        //                }
        //            }

        //            if (mergedTimes.Count > 0)
        //            {
        //                mergedDict[new List<int> { userKey }] = mergedTimes;
        //            }
        //        }
        //    }

        //    return mergedDict;
        //}



        //public Dictionary<int, List<DateTime>> CreateFifteenMinuteTimeInterval(Dictionary<int, List<DateTime>> dict1)
        //{
        //    Dictionary<int, List<DateTime>> intervalDict = new Dictionary<int, List<DateTime>>();

        //    foreach (var kvp in dict1)
        //    {
        //        int userKey = kvp.Key;
        //        List<DateTime> userTimes = kvp.Value;

        //        List<DateTime> intervalTimes = new List<DateTime>();

        //        // Iterate over the user's time slots and create 15-minute intervals
        //        for (int i = 0; i < userTimes.Count; i += 2)
        //        {
        //            DateTime startTime = userTimes[i];
        //            DateTime endTime = userTimes[i + 1];

        //            DateTime currentInterval = startTime;

        //            // Add the start time and subsequent 15-minute intervals until the end time
        //            while (currentInterval <= endTime)
        //            {
        //                intervalTimes.Add(currentInterval);
        //                currentInterval = currentInterval.AddMinutes(15);
        //            }
        //        }

        //        intervalDict[userKey] = intervalTimes;
        //    }

        //    return intervalDict;
        //}

        
        //public Dictionary<int, List<DateTime>> ReturnFunc()
        //{
        //    var dict = CreateDictionary(_dbContext);
        //    var dict2 = CreateDictionaryWithWorkingHours(_dbContext);
        //    var dict4 = CreateFifteenMinuteTimeInterval(dict2);
            
        //}
        
    }
}






