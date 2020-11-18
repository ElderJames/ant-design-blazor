﻿using System;
using System.Globalization;

namespace AntDesign
{
    internal static class DayOfWeekHelper
    {
        internal static int GetDiffForDayOfWeek(DayOfWeek dayOfWeek)
        {
            switch (dayOfWeek)
            {
                case DayOfWeek.Saturday: return 1;
                case DayOfWeek.Friday: return 2;
                case DayOfWeek.Thursday: return 3;
                case DayOfWeek.Wednesday: return 4;
                case DayOfWeek.Tuesday: return 5;
                case DayOfWeek.Monday: return 6;
                case DayOfWeek.Sunday: return 7;
                default: return 0;
            }
        }

        internal static string[] GetShortWeekDays(DayOfWeek firstDayOfWeek, CultureInfo cultureInfo = null)
        {
            var culture = cultureInfo ?? CultureInfo.CurrentCulture;
            DayOfWeek currentDay = DateTime.Now.DayOfWeek;
            DateTime referenceDay = DateTime.Today;

            if(firstDayOfWeek != currentDay)
            {
                int diff = firstDayOfWeek - currentDay;
                referenceDay = referenceDay.AddDays(diff);
            }

            return new[] { referenceDay.GetTwoLetterCode(culture),
                           referenceDay.AddDays(1).GetTwoLetterCode(culture),
                           referenceDay.AddDays(2).GetTwoLetterCode(culture),
                           referenceDay.AddDays(3).GetTwoLetterCode(culture),
                           referenceDay.AddDays(4).GetTwoLetterCode(culture),
                           referenceDay.AddDays(5).GetTwoLetterCode(culture),
                           referenceDay.AddDays(6).GetTwoLetterCode(culture),
            };
        }

        private static string GetTwoLetterCode(this DateTime today, CultureInfo cultureInfo)
            => today.ToString("ddd", cultureInfo).Substring(0, 2);
    }
}
