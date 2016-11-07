using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace moodtracker.Models
{
    public class MoodAverage
    {
        public DayOfWeek DayOfWeek { get; set; }

        public double AverageScale { get; set; }

        public double TotalScale { get; set; }

        public int Count { get; set; }
    }
}