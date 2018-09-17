﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForkBot
{
    class CourseDay
    {
        public string Term { get; }
        public string Section { get; }
        public string Professor { get; }
        public Dictionary<string, string> DayTimes = new Dictionary<string, string>();
        public string WeekDay { get; set; }
        public string Time { get; set; }

        public CourseDay(string term, string section, string prof)
        {
            Term = term;
            Section = section;
            Professor = prof;
            WeekDay = "";
            Time = "";

        }

        public void AddDayTime(string day, string time)
        {
            DayTimes.Add(day, time);
        }
    }
}