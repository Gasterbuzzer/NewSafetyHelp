using System.Collections.Generic;

namespace NewSafetyHelp.CustomDesktop.Utils
{
    public static class DateUtil
    {
        
        /// <summary>
        /// Returns the next day given a specific day, month, and year.
        /// Handles month rollover, year rollover, and leap years for February.
        /// </summary>
        /// <param name="day">The current day (1-31).</param>
        /// <param name="month">The current month (1-12).</param>
        /// <param name="year">The current year.</param>
        /// <returns>A list with [day, month, year] representing the next date.</returns>
        public static List<int> nextDay(int day, int month, int year)
        {
            day++; // Increment day first

            while (true)
            {
                int maxDays = DaysInMonth(month, year);

                if (day <= maxDays)
                {
                    break; // Day is valid, exit loop
                }

                day -= maxDays; // Reduce overflow days

                month++; // Increment month

                if (month > 12)
                {
                    month = 1; // Reset to January
                    year++;
                }
            }

            return new List<int> { day, month, year };
        }
        
        /// <summary>
        /// Returns the correct day, month and year given a specific day (can overflow), month, and year.
        /// Handles month rollover, year rollover, and leap years for February.
        /// </summary>
        /// <param name="day">The current day (1-31). (If overflowed, it will correct it) </param>
        /// <param name="month">The current month (1-12).</param>
        /// <param name="year">The current year.</param>
        /// <returns>A list with [day, month, year] representing the next date.</returns>
        public static List<int> fixDayMonthYear(int day, int month, int year)
        {
            while (true)
            {
                int maxDays = DaysInMonth(month, year);

                if (day <= maxDays)
                {
                    break; // Day is valid, exit loop
                }

                day -= maxDays; // Reduce overflow days

                month++; // Increment month

                if (month > 12)
                {
                    month = 1; // Reset to January
                    year++;
                }
            }

            return new List<int> { day, month, year };
        }

        /// <summary>
        /// Returns the number of days in the specified month and year.
        /// Considers leap years for February.
        /// </summary>
        /// <param name="month">The month (1-12).</param>
        /// <param name="year">The year.</param>
        /// <returns>The number of days in the month.</returns>
        public static int DaysInMonth(int month, int year)
        {
            switch (month)
            {
                case 0:
                    return DaysInMonth(1, year); // 0 is the same as january. It just makes notation easier, even if I dislike it.
                
                // 31 Day Months
                case 1:
                case 3:
                case 5:
                case 7:
                case 8:
                case 10:
                case 12:    
                    return 31;
                
                // 30 Day Months
                case 4:
                case 6:
                case 9:
                case 11:    
                    return 30;
                
                // February
                case 2:
                    if (IsLeapYear(year)) // 29 Days
                    {
                        return 29;
                    }
                    else // Not leap year, we have 28 days.
                    {
                        return 28;
                    }
                
                default: // Unknown month, we just return january in case we loop.
                    return DaysInMonth(1, year);
            }
        }
        

        /// <summary>
        /// Determines whether a given year is a leap year.
        /// </summary>
        /// <param name="year">The year to check.</param>
        /// <returns>True if leap year, else false.</returns>
        public static bool IsLeapYear(int year)
        {
            // Leap year if divisible by 4,
            // but not by 100 unless also divisible by 400
            return (year % 4 == 0) && ((year % 100 != 0) || (year % 400 == 0));
        }
    }
}