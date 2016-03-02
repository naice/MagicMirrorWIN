using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Diagnostics;
using System.Globalization;

// EXTRACTED FROM DDay.iCal https://sourceforge.net/projects/dday-ical/ to use in UWP

namespace MagicMirror.Factory.RRule
{
    public class RecurrencePatternSerializer
    {
        #region Static Public Methods

        static Calendar Calendar = new GregorianCalendar();

        static public DayOfWeek GetDayOfWeek(string value)
        {
            switch (value.ToUpper())
            {
                case "SU": return DayOfWeek.Sunday;
                case "MO": return DayOfWeek.Monday;
                case "TU": return DayOfWeek.Tuesday;
                case "WE": return DayOfWeek.Wednesday;
                case "TH": return DayOfWeek.Thursday;
                case "FR": return DayOfWeek.Friday;
                case "SA": return DayOfWeek.Saturday;
            }
            throw new ArgumentException(value + " is not a valid iCal day-of-week indicator.");
        }


        static bool?[] GetExpandBehaviorList(RecurrencePattern p)
        {
            // See the table in RFC 5545 Section 3.3.10 (Page 43).
            switch (p.Frequency)
            {
                case FrequencyType.Minutely: return new bool?[] { false, null, false, false, false, false, false, true, false };
                case FrequencyType.Hourly: return new bool?[] { false, null, false, false, false, false, true, true, false };
                case FrequencyType.Daily: return new bool?[] { false, null, null, false, false, true, true, true, false };
                case FrequencyType.Weekly: return new bool?[] { false, null, null, null, true, true, true, true, false };
                case FrequencyType.Monthly:
                    {
                        bool?[] row = new bool?[] { false, null, null, true, true, true, true, true, false };

                        // Limit if BYMONTHDAY is present; otherwise, special expand for MONTHLY.
                        if (p.ByMonthDay.Count > 0)
                            row[4] = false;

                        return row;
                    }
                case FrequencyType.Yearly:
                    {
                        bool?[] row = new bool?[] { true, true, true, true, true, true, true, true, false };

                        // Limit if BYYEARDAY or BYMONTHDAY is present; otherwise,
                        // special expand for WEEKLY if BYWEEKNO present; otherwise,
                        // special expand for MONTHLY if BYMONTH present; otherwise,
                        // special expand for YEARLY.
                        if (p.ByYearDay.Count > 0 || p.ByMonthDay.Count > 0)
                            row[4] = false;

                        return row;
                    }
                default:
                    return new bool?[] { false, null, false, false, false, false, false, false, false };
            }
        }
        const int maxIncrementCount = 1000;
        public static List<DateTime> GetDates(DateTime seed, DateTime periodStart, DateTime periodEnd, int maxCount, RecurrencePattern pattern, bool includeReferenceDateInResults)
        {
            List<DateTime> dates = new List<DateTime>();
            DateTime originalDate = new DateTime(seed.Ticks, seed.Kind);
            DateTime seedCopy = new DateTime(seed.Ticks, seed.Kind);

            if (includeReferenceDateInResults)
                dates.Add(seedCopy);

            // If the interval is set to zero, or our count prevents us
            // from getting additional items, then return with the reference
            // date only.
            if (pattern.Interval == 0 ||
                (pattern.Count != int.MinValue && pattern.Count <= dates.Count))
            {
                return dates;
            }

            // optimize the start time for selecting candidates
            // (only applicable where a COUNT is not specified)
            if (pattern.Count == int.MinValue)
            {
                DateTime incremented = seedCopy;
                // FIXME: we can more aggresively increment here when
                // the difference between dates is greater.
                IncrementDate(ref incremented, pattern, pattern.Interval);
                while (incremented < periodStart)
                {
                    seedCopy = incremented;
                    IncrementDate(ref incremented, pattern, pattern.Interval);
                }
            }

            bool?[] expandBehavior = GetExpandBehaviorList(pattern);

            int invalidCandidateCount = 0;
            int noCandidateIncrementCount = 0;
            DateTime candidate = DateTime.MinValue;
            while ((maxCount < 0) || (dates.Count < maxCount))
            {
                if (pattern.Until != DateTime.MinValue && candidate != DateTime.MinValue && candidate > pattern.Until)
                    break;

                if (periodEnd != null && candidate != DateTime.MinValue && candidate > periodEnd)
                    break;

                if (pattern.Count >= 1 && (dates.Count + invalidCandidateCount) >= pattern.Count)
                    break;

                List<DateTime> candidates = GetCandidates(seedCopy, pattern, expandBehavior);
                if (candidates.Count > 0)
                {
                    noCandidateIncrementCount = 0;

                    // sort candidates for identifying when UNTIL date is exceeded..
                    candidates.Sort();

                    for (int i = 0; i < candidates.Count; i++)
                    {
                        candidate = candidates[i];

                        // don't count candidates that occur before the original date..
                        if (candidate >= originalDate)
                        {
                            // candidates MAY occur before periodStart
                            // For example, FREQ=YEARLY;BYWEEKNO=1 could return dates
                            // from the previous year.
                            //
                            // candidates exclusive of periodEnd..
                            if (candidate >= periodEnd)
                            {
                                invalidCandidateCount++;
                            }
                            else if (pattern.Count >= 1 && (dates.Count + invalidCandidateCount) >= pattern.Count)
                            {
                                break;
                            }
                            else if (pattern.Until == DateTime.MinValue || candidate <= pattern.Until)
                            {
                                if (!dates.Contains(candidate))
                                    dates.Add(candidate);
                            }
                        }
                    }
                }
                else
                {
                    noCandidateIncrementCount++;
                    if ((maxIncrementCount > 0) && (noCandidateIncrementCount > maxIncrementCount))
                        break;
                }

                IncrementDate(ref seedCopy, pattern, pattern.Interval);
            }

            // sort final list..
            dates.Sort();
            return dates;
        }
        
        static void IncrementDate(ref DateTime dt, RecurrencePattern pattern, int interval)
        {
            // FIXME: use a more specific exception.
            if (interval == 0)
                throw new Exception("Cannot evaluate with an interval of zero.  Please use an interval other than zero.");

            DateTime old = dt;
            switch (pattern.Frequency)
            {
                case FrequencyType.Secondly: dt = old.AddSeconds(interval); break;
                case FrequencyType.Minutely: dt = old.AddMinutes(interval); break;
                case FrequencyType.Hourly: dt = old.AddHours(interval); break;
                case FrequencyType.Daily: dt = old.AddDays(interval); break;
                case FrequencyType.Weekly: dt = AddWeeks(old, interval, pattern.FirstDayOfWeek); break;
                case FrequencyType.Monthly: dt = old.AddDays(-old.Day + 1).AddMonths(interval); break;
                case FrequencyType.Yearly: dt = old.AddDays(-old.DayOfYear + 1).AddYears(interval); break;
                // FIXME: use a more specific exception.
                default: throw new Exception("FrequencyType.NONE cannot be evaluated. Please specify a FrequencyType before evaluating the recurrence.");
            }
        }

        static DateTime AddWeeks(DateTime dt, int interval, DayOfWeek firstDayOfWeek)
        {
            // NOTE: fixes WeeklyUntilWkst2() eval.
            // NOTE: simplified the execution of this - fixes bug #3119920 - missing weekly occurences also
            dt = dt.AddDays(interval * 7);
            while (dt.DayOfWeek != firstDayOfWeek)
                dt = dt.AddDays(-1);

            return dt;
        }
        /**
         * Returns a list of possible dates generated from the applicable BY* rules, using the specified date as a seed.
         * @param date the seed date
         * @param value the type of date list to return
         * @return a DateList
         */
        static private List<DateTime> GetCandidates(DateTime date, RecurrencePattern pattern, bool?[] expandBehaviors)
        {
            List<DateTime> dates = new List<DateTime>();
            dates.Add(date);
            dates = GetMonthVariants(dates, pattern, expandBehaviors[0]);
            dates = GetWeekNoVariants(dates, pattern, expandBehaviors[1]);
            dates = GetYearDayVariants(dates, pattern, expandBehaviors[2]);
            dates = GetMonthDayVariants(dates, pattern, expandBehaviors[3]);
            dates = GetDayVariants(dates, pattern, expandBehaviors[4]);
            dates = GetHourVariants(dates, pattern, expandBehaviors[5]);
            dates = GetMinuteVariants(dates, pattern, expandBehaviors[6]);
            dates = GetSecondVariants(dates, pattern, expandBehaviors[7]);
            dates = ApplySetPosRules(dates, pattern);
            return dates;
        }

        /**
         * Applies BYSETPOS rules to <code>dates</code>. Valid positions are from 1 to the size of the date list. Invalid
         * positions are ignored.
         * @param dates
         */
        static private List<DateTime> ApplySetPosRules(List<DateTime> dates, RecurrencePattern pattern)
        {
            // return if no SETPOS rules specified..
            if (pattern.BySetPosition.Count == 0)
                return dates;

            // sort the list before processing..
            dates.Sort();

            List<DateTime> setPosDates = new List<DateTime>();
            int size = dates.Count;

            for (int i = 0; i < pattern.BySetPosition.Count; i++)
            {
                int pos = pattern.BySetPosition[i];
                if (pos > 0 && pos <= size)
                {
                    setPosDates.Add(dates[pos - 1]);
                }
                else if (pos < 0 && pos >= -size)
                {
                    setPosDates.Add(dates[size + pos]);
                }
            }
            return setPosDates;
        }

        /**
         * Applies BYMONTH rules specified in this Recur instance to the specified date list. If no BYMONTH rules are
         * specified the date list is returned unmodified.
         * @param dates
         * @return
         */
        static private List<DateTime> GetMonthVariants(List<DateTime> dates, RecurrencePattern pattern, bool? expand)
        {
            if (expand == null || pattern.ByMonth.Count == 0)
                return dates;

            if (expand.HasValue && expand.Value)
            {
                // Expand behavior
                List<DateTime> monthlyDates = new List<DateTime>();
                for (int i = 0; i < dates.Count; i++)
                {
                    DateTime date = dates[i];
                    for (int j = 0; j < pattern.ByMonth.Count; j++)
                    {
                        int month = pattern.ByMonth[j];
                        date = date.AddMonths(month - date.Month);
                        monthlyDates.Add(date);
                    }
                }
                return monthlyDates;
            }
            else
            {
                // Limit behavior
                for (int i = dates.Count - 1; i >= 0; i--)
                {
                    DateTime date = dates[i];
                    for (int j = 0; j < pattern.ByMonth.Count; j++)
                    {
                        if (date.Month == pattern.ByMonth[j])
                            goto Next;
                    }
                    dates.RemoveAt(i);
                Next:;
                }
                return dates;
            }
        }

        /**
         * Applies BYWEEKNO rules specified in this Recur instance to the specified date list. If no BYWEEKNO rules are
         * specified the date list is returned unmodified.
         * @param dates
         * @return
         */
        static private List<DateTime> GetWeekNoVariants(List<DateTime> dates, RecurrencePattern pattern, bool? expand)
        {
            if (expand == null || pattern.ByWeekNo.Count == 0)
                return dates;

            if (expand.HasValue && expand.Value)
            {
                // Expand behavior
                List<DateTime> weekNoDates = new List<DateTime>();
                for (int i = 0; i < dates.Count; i++)
                {
                    DateTime date = dates[i];
                    for (int j = 0; j < pattern.ByWeekNo.Count; j++)
                    {
                        // Determine our target week number
                        int weekNo = pattern.ByWeekNo[j];

                        // Determine our current week number
                        int currWeekNo = Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, pattern.FirstDayOfWeek);
                        while (currWeekNo > weekNo)
                        {
                            // If currWeekNo > weekNo, then we're likely at the start of a year
                            // where currWeekNo could be 52 or 53.  If we simply step ahead 7 days
                            // we should be back to week 1, where we can easily make the calculation
                            // to move to weekNo.
                            date = date.AddDays(7);
                            currWeekNo = Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, pattern.FirstDayOfWeek);
                        }

                        // Move ahead to the correct week of the year
                        date = date.AddDays((weekNo - currWeekNo) * 7);

                        // Step backward single days until we're at the correct DayOfWeek
                        while (date.DayOfWeek != pattern.FirstDayOfWeek)
                            date = date.AddDays(-1);

                        for (int k = 0; k < 7; k++)
                        {
                            weekNoDates.Add(date);
                            date = date.AddDays(1);
                        }
                    }
                }
                return weekNoDates;
            }
            else
            {
                // Limit behavior
                for (int i = dates.Count - 1; i >= 0; i--)
                {
                    DateTime date = dates[i];
                    for (int j = 0; j < pattern.ByWeekNo.Count; j++)
                    {
                        // Determine our target week number
                        int weekNo = pattern.ByWeekNo[j];

                        // Determine our current week number
                        int currWeekNo = Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, pattern.FirstDayOfWeek);

                        if (weekNo == currWeekNo)
                            goto Next;
                    }

                    dates.RemoveAt(i);
                Next:;
                }
                return dates;
            }
        }

        /**
         * Applies BYYEARDAY rules specified in this Recur instance to the specified date list. If no BYYEARDAY rules are
         * specified the date list is returned unmodified.
         * @param dates
         * @return
         */
        static private List<DateTime> GetYearDayVariants(List<DateTime> dates, RecurrencePattern pattern, bool? expand)
        {
            if (expand == null || pattern.ByYearDay.Count == 0)
                return dates;

            if (expand.HasValue && expand.Value)
            {
                // Expand behavior
                List<DateTime> yearDayDates = new List<DateTime>();
                for (int i = 0; i < dates.Count; i++)
                {
                    DateTime date = dates[i];
                    for (int j = 0; j < pattern.ByYearDay.Count; j++)
                    {
                        int yearDay = pattern.ByYearDay[j];

                        DateTime newDate;
                        if (yearDay > 0)
                            newDate = date.AddDays(-date.DayOfYear + yearDay);
                        else
                            newDate = date.AddDays(-date.DayOfYear + 1).AddYears(1).AddDays(yearDay);

                        yearDayDates.Add(newDate);
                    }
                }
                return yearDayDates;
            }
            else
            {
                // Limit behavior
                for (int i = dates.Count - 1; i >= 0; i--)
                {
                    DateTime date = dates[i];
                    for (int j = 0; j < pattern.ByYearDay.Count; j++)
                    {
                        int yearDay = pattern.ByYearDay[j];

                        DateTime newDate;
                        if (yearDay > 0)
                            newDate = date.AddDays(-date.DayOfYear + yearDay);
                        else
                            newDate = date.AddDays(-date.DayOfYear + 1).AddYears(1).AddDays(yearDay);

                        if (newDate.DayOfYear == date.DayOfYear)
                            goto Next;
                    }

                    dates.RemoveAt(i);
                Next:;
                }

                return dates;
            }
        }

        /**
         * Applies BYMONTHDAY rules specified in this Recur instance to the specified date list. If no BYMONTHDAY rules are
         * specified the date list is returned unmodified.
         * @param dates
         * @return
         */
        static private List<DateTime> GetMonthDayVariants(List<DateTime> dates, RecurrencePattern pattern, bool? expand)
        {
            if (expand == null || pattern.ByMonthDay.Count == 0)
                return dates;

            if (expand.HasValue && expand.Value)
            {
                // Expand behavior
                List<DateTime> monthDayDates = new List<DateTime>();
                for (int i = 0; i < dates.Count; i++)
                {
                    DateTime date = dates[i];
                    for (int j = 0; j < pattern.ByMonthDay.Count; j++)
                    {
                        int monthDay = pattern.ByMonthDay[j];

                        int daysInMonth = Calendar.GetDaysInMonth(date.Year, date.Month);
                        if (Math.Abs(monthDay) <= daysInMonth)
                        {
                            // Account for positive or negative numbers
                            DateTime newDate;
                            if (monthDay > 0)
                                newDate = date.AddDays(-date.Day + monthDay);
                            else
                                newDate = date.AddDays(-date.Day + 1).AddMonths(1).AddDays(monthDay);

                            monthDayDates.Add(newDate);
                        }
                    }
                }
                return monthDayDates;
            }
            else
            {
                // Limit behavior
                for (int i = dates.Count - 1; i >= 0; i--)
                {
                    DateTime date = dates[i];
                    for (int j = 0; j < pattern.ByMonthDay.Count; j++)
                    {
                        int monthDay = pattern.ByMonthDay[j];

                        int daysInMonth = Calendar.GetDaysInMonth(date.Year, date.Month);
                        if (Math.Abs(monthDay) > daysInMonth)
                            throw new ArgumentException("Invalid day of month: " + date + " (day " + monthDay + ")");

                        // Account for positive or negative numbers
                        DateTime newDate;
                        if (monthDay > 0)
                            newDate = date.AddDays(-date.Day + monthDay);
                        else
                            newDate = date.AddDays(-date.Day + 1).AddMonths(1).AddDays(monthDay);

                        if (newDate.Day.Equals(date.Day))
                            goto Next;
                    }

                Next:;
                    dates.RemoveAt(i);
                }

                return dates;
            }
        }

        /**
         * Applies BYDAY rules specified in this Recur instance to the specified date list. If no BYDAY rules are specified
         * the date list is returned unmodified.
         * @param dates
         * @return
         */
        static private List<DateTime> GetDayVariants(List<DateTime> dates, RecurrencePattern pattern, bool? expand)
        {
            if (expand == null || pattern.ByDay.Count == 0)
                return dates;

            if (expand.HasValue && expand.Value)
            {
                // Expand behavior
                List<DateTime> weekDayDates = new List<DateTime>();
                for (int i = 0; i < dates.Count; i++)
                {
                    DateTime date = dates[i];
                    for (int j = 0; j < pattern.ByDay.Count; j++)
                    {
                        weekDayDates.AddRange(GetAbsWeekDays(date, pattern.ByDay[j], pattern, expand));
                    }
                }

                return weekDayDates;
            }
            else
            {
                // Limit behavior
                for (int i = dates.Count - 1; i >= 0; i--)
                {
                    DateTime date = dates[i];
                    for (int j = 0; j < pattern.ByDay.Count; j++)
                    {
                        WeekDay weekDay = pattern.ByDay[j];
                        if (weekDay.DayOfWeek.Equals(date.DayOfWeek))
                        {
                            // If no offset is specified, simply test the day of week!
                            // FIXME: test with offset...
                            if (date.DayOfWeek.Equals(weekDay.DayOfWeek))
                                goto Next;
                        }
                    }
                    dates.RemoveAt(i);
                Next:;
                }

                return dates;
            }
        }

        /**
         * Returns a list of applicable dates corresponding to the specified week day in accordance with the frequency
         * specified by this recurrence rule.
         * @param date
         * @param weekDay
         * @return
         */
        static private List<DateTime> GetAbsWeekDays(DateTime date, WeekDay weekDay, RecurrencePattern pattern, bool? expand)
        {
            List<DateTime> days = new List<DateTime>();

            DayOfWeek dayOfWeek = weekDay.DayOfWeek;
            if (pattern.Frequency == FrequencyType.Daily)
            {
                if (date.DayOfWeek == dayOfWeek)
                    days.Add(date);
            }
            else if (pattern.Frequency == FrequencyType.Weekly || pattern.ByWeekNo.Count > 0)
            {
                // Rewind to the first day of the week
                while (date.DayOfWeek != pattern.FirstDayOfWeek)
                    date = date.AddDays(-1);

                // Step forward until we're on the day of week we're interested in
                while (date.DayOfWeek != dayOfWeek)
                    date = date.AddDays(1);

                days.Add(date);
            }
            else if (pattern.Frequency == FrequencyType.Monthly || pattern.ByMonth.Count > 0)
            {
                int month = date.Month;

                // construct a list of possible month days..
                date = date.AddDays(-date.Day + 1);
                while (date.DayOfWeek != dayOfWeek)
                    date = date.AddDays(1);

                while (date.Month == month)
                {
                    days.Add(date);
                    date = date.AddDays(7);
                }
            }
            else if (pattern.Frequency == FrequencyType.Yearly)
            {
                int year = date.Year;

                // construct a list of possible year days..
                date = date.AddDays(-date.DayOfYear + 1);
                while (date.DayOfWeek != dayOfWeek)
                    date = date.AddDays(1);

                while (date.Year == year)
                {
                    days.Add(date);
                    date = date.AddDays(7);
                }
            }
            return GetOffsetDates(days, weekDay.Offset);
        }

        /**
         * Returns a single-element sublist containing the element of <code>list</code> at <code>offset</code>. Valid
         * offsets are from 1 to the size of the list. If an invalid offset is supplied, all elements from <code>list</code>
         * are added to <code>sublist</code>.
         * @param list
         * @param offset
         * @param sublist
         */
        static private List<DateTime> GetOffsetDates(List<DateTime> dates, int offset)
        {
            if (offset == int.MinValue)
                return dates;

            List<DateTime> offsetDates = new List<DateTime>();
            int size = dates.Count;
            if (offset < 0 && offset >= -size)
            {
                offsetDates.Add(dates[size + offset]);
            }
            else if (offset > 0 && offset <= size)
            {
                offsetDates.Add(dates[offset - 1]);
            }
            return offsetDates;
        }

        /**
         * Applies BYHOUR rules specified in this Recur instance to the specified date list. If no BYHOUR rules are
         * specified the date list is returned unmodified.
         * @param dates
         * @return
         */
        static private List<DateTime> GetHourVariants(List<DateTime> dates, RecurrencePattern pattern, bool? expand)
        {
            if (expand == null || pattern.ByHour.Count == 0)
                return dates;

            if (expand.HasValue && expand.Value)
            {
                // Expand behavior
                List<DateTime> hourlyDates = new List<DateTime>();
                for (int i = 0; i < dates.Count; i++)
                {
                    DateTime date = dates[i];
                    for (int j = 0; j < pattern.ByHour.Count; j++)
                    {
                        int hour = pattern.ByHour[j];
                        date = date.AddHours(-date.Hour + hour);
                        hourlyDates.Add(date);
                    }
                }
                return hourlyDates;
            }
            else
            {
                // Limit behavior
                for (int i = dates.Count - 1; i >= 0; i--)
                {
                    DateTime date = dates[i];
                    for (int j = 0; j < pattern.ByHour.Count; j++)
                    {
                        int hour = pattern.ByHour[j];
                        if (date.Hour == hour)
                            goto Next;
                    }
                    // Remove unmatched dates
                    dates.RemoveAt(i);
                Next:;
                }
                return dates;
            }
        }

        /**
         * Applies BYMINUTE rules specified in this Recur instance to the specified date list. If no BYMINUTE rules are
         * specified the date list is returned unmodified.
         * @param dates
         * @return
         */
        static private List<DateTime> GetMinuteVariants(List<DateTime> dates, RecurrencePattern pattern, bool? expand)
        {
            if (expand == null || pattern.ByMinute.Count == 0)
                return dates;

            if (expand.HasValue && expand.Value)
            {
                // Expand behavior
                List<DateTime> minutelyDates = new List<DateTime>();
                for (int i = 0; i < dates.Count; i++)
                {
                    DateTime date = dates[i];
                    for (int j = 0; j < pattern.ByMinute.Count; j++)
                    {
                        int minute = pattern.ByMinute[j];
                        date = date.AddMinutes(-date.Minute + minute);
                        minutelyDates.Add(date);
                    }
                }
                return minutelyDates;
            }
            else
            {
                // Limit behavior
                for (int i = dates.Count - 1; i >= 0; i--)
                {
                    DateTime date = dates[i];
                    for (int j = 0; j < pattern.ByMinute.Count; j++)
                    {
                        int minute = pattern.ByMinute[j];
                        if (date.Minute == minute)
                            goto Next;
                    }
                    // Remove unmatched dates
                    dates.RemoveAt(i);
                Next:;
                }
                return dates;
            }
        }

        /**
         * Applies BYSECOND rules specified in this Recur instance to the specified date list. If no BYSECOND rules are
         * specified the date list is returned unmodified.
         * @param dates
         * @return
         */
        static private List<DateTime> GetSecondVariants(List<DateTime> dates, RecurrencePattern pattern, bool? expand)
        {
            if (expand == null || pattern.BySecond.Count == 0)
                return dates;

            if (expand.HasValue && expand.Value)
            {
                // Expand behavior
                List<DateTime> secondlyDates = new List<DateTime>();
                for (int i = 0; i < dates.Count; i++)
                {
                    DateTime date = dates[i];
                    for (int j = 0; j < pattern.BySecond.Count; j++)
                    {
                        int second = pattern.BySecond[j];
                        date = date.AddSeconds(-date.Second + second);
                        secondlyDates.Add(date);
                    }
                }
                return secondlyDates;
            }
            else
            {
                // Limit behavior
                for (int i = dates.Count - 1; i >= 0; i--)
                {
                    DateTime date = dates[i];
                    for (int j = 0; j < pattern.BySecond.Count; j++)
                    {
                        int second = pattern.BySecond[j];
                        if (date.Second == second)
                            goto Next;
                    }
                    // Remove unmatched dates
                    dates.RemoveAt(i);
                Next:;
                }
                return dates;
            }
        }

        #endregion

        #region Static Protected Methods

        static protected void AddInt32Values(IList<int> list, string value)
        {
            string[] values = value.Split(',');
            foreach (string v in values)
                list.Add(Convert.ToInt32(v));
        }

        #endregion

        #region Content Validation

        virtual public void CheckRange(string name, IList<int> values, int min, int max)
        {
            bool allowZero = (min == 0 || max == 0) ? true : false;
            foreach (int value in values)
                CheckRange(name, value, min, max, allowZero);
        }

        virtual public void CheckRange(string name, int value, int min, int max)
        {
            CheckRange(name, value, min, max, (min == 0 || max == 0) ? true : false);
        }

        virtual public void CheckRange(string name, int value, int min, int max, bool allowZero)
        {
            if (value != int.MinValue && (value < min || value > max || (!allowZero && value == 0)))
                throw new ArgumentException(name + " value " + value + " is out of range. Valid values are between " + min + " and " + max + (allowZero ? "" : ", excluding zero (0)") + ".");
        }

        virtual public void CheckMutuallyExclusive<T, U>(string name1, string name2, T obj1, U obj2)
        {
            if (object.Equals(obj1, default(T)) || object.Equals(obj2, default(U)))
                return;
            else
            {
                // If the object is MinValue instead of its default, consider
                // that to be unassigned.
                bool 
                    isMin1 = false,
                    isMin2 = false;

                Type 
                    t1 = obj1.GetType(),
                    t2 = obj2.GetType();

                FieldInfo fi1 = t1.GetField("MinValue");
                FieldInfo fi2 = t1.GetField("MinValue");
                
                isMin1 = fi1 != null && obj1.Equals(fi1.GetValue(null));
                isMin2 = fi2 != null && obj2.Equals(fi2.GetValue(null));
                if (isMin1 || isMin2)
                    return;                    
            }

            throw new ArgumentException("Both " + name1 + " and " + name2 + " cannot be supplied together; they are mutually exclusive.");
        }

        #endregion

        #region Private Methods

        private void SerializeByValue(List<string> aggregate, IList<int> byValue, string name)
        {
            if (byValue.Count > 0)
            {
                List<string> byValues = new List<string>();
                foreach (int i in byValue)
                    byValues.Add(i.ToString());

                aggregate.Add(name + "=" + string.Join(",", byValues.ToArray()));
            }
        }

        #endregion

        #region Overrides

        public Type TargetType
        {
            get { return typeof(RecurrencePattern); }
        }

        public object Deserialize(TextReader tr)
        {
            string value = tr.ReadToEnd();

            // Instantiate the data type
            RecurrencePattern r = new RecurrencePattern();

            if (r != null)
            {

                Match match = Regex.Match(value, @"FREQ=(SECONDLY|MINUTELY|HOURLY|DAILY|WEEKLY|MONTHLY|YEARLY);?(.*)", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    // Parse the frequency type
                    r.Frequency = (FrequencyType)Enum.Parse(typeof(FrequencyType), match.Groups[1].Value, true);

                    // NOTE: fixed a bug where the group 2 match
                    // resulted in an empty string, which caused
                    // an error.
                    if (match.Groups[2].Success &&
                        match.Groups[2].Length > 0)
                    {
                        string[] keywordPairs = match.Groups[2].Value.Split(';');
                        foreach (string keywordPair in keywordPairs)
                        {
                            string[] keyValues = keywordPair.Split('=');
                            string keyword = keyValues[0];
                            string keyValue = keyValues[1];

                            switch (keyword.ToUpper())
                            {
                                case "UNTIL":
                                    {
                                        try
                                        {
                                            r.Until = DateTime.Parse(keyValue, System.Globalization.CultureInfo.InvariantCulture);
                                        }
                                        catch (Exception ex)
                                        {
                                            //Log.e(ex);
                                        }                                        
                                    } break;
                                case "COUNT": r.Count = Convert.ToInt32(keyValue); break;
                                case "INTERVAL": r.Interval = Convert.ToInt32(keyValue); break;
                                case "BYSECOND": AddInt32Values(r.BySecond, keyValue); break;
                                case "BYMINUTE": AddInt32Values(r.ByMinute, keyValue); break;
                                case "BYHOUR": AddInt32Values(r.ByHour, keyValue); break;
                                case "BYDAY":
                                    {
                                        string[] days = keyValue.Split(',');
                                        foreach (string day in days)
                                        {
                                            WeekDay wd = new WeekDay();
                                            Match match2 = Regex.Match(value, @"(\+|-)?(\d{1,2})?(\w{2})");
                                            if (match2.Success)
                                            {
                                                if (match2.Groups[2].Success)
                                                {
                                                    wd.Offset = Convert.ToInt32(match2.Groups[2].Value);
                                                    if (match2.Groups[1].Success && match2.Groups[1].Value.Contains("-"))
                                                        wd.Offset *= -1;
                                                }

                                                wd.DayOfWeek = GetDayOfWeek(match2.Groups[3].Value);

                                                r.ByDay.Add(wd);
                                            }
                                        }
                                    } break;
                                case "BYMONTHDAY": AddInt32Values(r.ByMonthDay, keyValue); break;
                                case "BYYEARDAY": AddInt32Values(r.ByYearDay, keyValue); break;
                                case "BYWEEKNO": AddInt32Values(r.ByWeekNo, keyValue); break;
                                case "BYMONTH": AddInt32Values(r.ByMonth, keyValue); break;
                                case "BYSETPOS": AddInt32Values(r.BySetPosition, keyValue); break;
                                case "WKST": r.FirstDayOfWeek = GetDayOfWeek(keyValue); break;
                            }
                        }
                    }
                }
                
                //
                // This matches strings such as:
                //
                // "Every 6 minutes"
                // "Every 3 days"
                //
                else if ((match = Regex.Match(value, @"every\s+(?<Interval>other|\d+)?\w{0,2}\s*(?<Freq>second|minute|hour|day|week|month|year)s?,?\s*(?<More>.+)", RegexOptions.IgnoreCase)).Success)
                {
                    if (match.Groups["Interval"].Success)
                    {
                        int interval;
                        if (!int.TryParse(match.Groups["Interval"].Value, out interval))
                            r.Interval = 2; // "other"
                        else r.Interval = interval;
                    }
                    else r.Interval = 1;

                    switch (match.Groups["Freq"].Value.ToLower())
                    {
                        case "second": r.Frequency = FrequencyType.Secondly; break;
                        case "minute": r.Frequency = FrequencyType.Minutely; break;
                        case "hour": r.Frequency = FrequencyType.Hourly; break;
                        case "day": r.Frequency = FrequencyType.Daily; break;
                        case "week": r.Frequency = FrequencyType.Weekly; break;
                        case "month": r.Frequency = FrequencyType.Monthly; break;
                        case "year": r.Frequency = FrequencyType.Yearly; break;
                    }

                    string[] values = match.Groups["More"].Value.Split(',');
                    foreach (string item in values)
                    {
                        if ((match = Regex.Match(item, @"(?<Num>\d+)\w\w\s+(?<Type>second|minute|hour|day|week|month)", RegexOptions.IgnoreCase)).Success ||
                            (match = Regex.Match(item, @"(?<Type>second|minute|hour|day|week|month)\s+(?<Num>\d+)", RegexOptions.IgnoreCase)).Success)
                        {
                            int num;
                            if (int.TryParse(match.Groups["Num"].Value, out num))
                            {
                                switch (match.Groups["Type"].Value.ToLower())
                                {
                                    case "second":
                                        r.BySecond.Add(num);
                                        break;
                                    case "minute":
                                        r.ByMinute.Add(num);
                                        break;
                                    case "hour":
                                        r.ByHour.Add(num);
                                        break;
                                    case "day":
                                        switch (r.Frequency)
                                        {
                                            case FrequencyType.Yearly:
                                                r.ByYearDay.Add(num);
                                                break;
                                            case FrequencyType.Monthly:
                                                r.ByMonthDay.Add(num);
                                                break;
                                        }
                                        break;
                                    case "week":
                                        r.ByWeekNo.Add(num);
                                        break;
                                    case "month":
                                        r.ByMonth.Add(num);
                                        break;
                                }
                            }
                        }
                        else if ((match = Regex.Match(item, @"(?<Num>\d+\w{0,2})?(\w|\s)+?(?<First>first)?(?<Last>last)?\s*((?<Day>sunday|monday|tuesday|wednesday|thursday|friday|saturday)\s*(and|or)?\s*)+", RegexOptions.IgnoreCase)).Success)
                        {
                            int num = int.MinValue;
                            if (match.Groups["Num"].Success)
                            {
                                if (int.TryParse(match.Groups["Num"].Value, out num))
                                {
                                    if (match.Groups["Last"].Success)
                                    {
                                        // Make number negative
                                        num *= -1;
                                    }
                                }
                            }
                            else if (match.Groups["Last"].Success)
                                num = -1;
                            else if (match.Groups["First"].Success)
                                num = 1;

                            foreach (Capture capture in match.Groups["Day"].Captures)
                            {
                                WeekDay wd = new WeekDay();
                                wd.Offset = num;
                                wd.DayOfWeek = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), capture.Value, true);
                                r.ByDay.Add(wd);
                            }
                        }
                        else if ((match = Regex.Match(item, @"at\s+(?<Hour>\d{1,2})(:(?<Minute>\d{2})((:|\.)(?<Second>\d{2}))?)?\s*(?<Meridian>(a|p)m?)?", RegexOptions.IgnoreCase)).Success)
                        {
                            int hour, minute, second;

                            if (int.TryParse(match.Groups["Hour"].Value, out hour))
                            {
                                // Adjust for PM
                                if (match.Groups["Meridian"].Success &&
                                    match.Groups["Meridian"].Value.ToUpper().StartsWith("P"))
                                    hour += 12;

                                r.ByHour.Add(hour);

                                if (match.Groups["Minute"].Success &&
                                    int.TryParse(match.Groups["Minute"].Value, out minute))
                                {
                                    r.ByMinute.Add(minute);
                                    if (match.Groups["Second"].Success &&
                                        int.TryParse(match.Groups["Second"].Value, out second))
                                        r.BySecond.Add(second);
                                }
                            }
                        }
                        else if ((match = Regex.Match(item, @"^\s*until\s+(?<DateTime>.+)$", RegexOptions.IgnoreCase)).Success)
                        {
                            DateTime dt = DateTime.Parse(match.Groups["DateTime"].Value);
                            DateTime.SpecifyKind(dt, DateTimeKind.Utc);

                            r.Until = dt;
                        }
                        else if ((match = Regex.Match(item, @"^\s*for\s+(?<Count>\d+)\s+occurrences\s*$", RegexOptions.IgnoreCase)).Success)
                        {
                            int count;
                            if (!int.TryParse(match.Groups["Count"].Value, out count))
                                return false;
                            else r.Count = count;
                        }
                    }
                }
                else
                {
                    // Couldn't parse the object, return null!
                    r = null;
                }

                if (r != null)
                {
                    CheckMutuallyExclusive("COUNT", "UNTIL", r.Count, r.Until);
                    CheckRange("INTERVAL", r.Interval, 0, int.MaxValue);
                    CheckRange("COUNT", r.Count, 0, int.MaxValue);
                    CheckRange("BYSECOND", r.BySecond, 0, 59);
                    CheckRange("BYMINUTE", r.ByMinute, 0, 59);
                    CheckRange("BYHOUR", r.ByHour, 0, 23);
                    CheckRange("BYMONTHDAY", r.ByMonthDay, -31, 31);
                    CheckRange("BYYEARDAY", r.ByYearDay, -366, 366);
                    CheckRange("BYWEEKNO", r.ByWeekNo, -53, 53);
                    CheckRange("BYMONTH", r.ByMonth, 1, 12);
                    CheckRange("BYSETPOS", r.BySetPosition, -366, 366);
                }
            }

            return r;            
        }

        #endregion
    }
}
