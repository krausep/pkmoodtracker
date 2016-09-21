using System;
using System.Collections.Generic;
using moodtracker.Models;

namespace moodtracker.Interfaces
{
    interface IMoodDataProvider
    {
        MoodItem Add(MoodItem moodItem);
        MoodItem Update(MoodItem moodItem);
        void Delete(MoodItem moodItem);
        MoodItem Get(Guid moodId);
        List<MoodItem> GetAll();
        List<MoodItem> FindByDateRange(DateTime startTime, DateTime endTime);
        List<MoodItem> FindByString(string searchText);

    }
}
