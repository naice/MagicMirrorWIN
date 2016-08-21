using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using MagicMirror.Factory.RRule;

namespace MagicMirror.Factory
{
    public class CalendarFactory
    {

        public async Task<List<ViewModel.CalendarItem>> GetFullCalendarList(Configuration.Configuration config)
        {
            List<ViewModel.CalendarItem> items = new List<ViewModel.CalendarItem>();
            foreach (var item in config.CalendarAndColor)
            {
                var newCalendarItems = await GetCalendar(item);
                if (newCalendarItems != null)
                {
                    foreach (var nitem in newCalendarItems)
                    {
                        items.Add(nitem);
                    }
                }
            }

            items.Sort((A, B) => A.Start.CompareTo(B.Start));

            return items;
        }

        static DateTime GetDateTime(string val)
        {
            DateTime t = DateTime.MinValue;

            string tval = val.Trim();

            if (!string.IsNullOrWhiteSpace(tval) && tval.Length == 16)
            {
                try
                {
                    t = DateTime.ParseExact(val, "yyyyMMddTHHmmssZ", System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (FormatException)
                {

                }
                catch (ArgumentException)
                {

                }
            }

            return t;
        }
        static DateTime GetDateTime3(string val)
        {
            DateTime t = DateTime.MinValue;

            string tval = val.Trim();

            if (!string.IsNullOrWhiteSpace(tval) && tval.Length == 15)
            {
                try
                {
                    t = DateTime.ParseExact(val, "yyyyMMddTHHmmss", System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (FormatException)
                {

                }
                catch (ArgumentException)
                {

                }
            }

            return t;
        }
        static DateTime GetDateTime2(string val)
        {
            DateTime t = DateTime.MinValue;

            string tval = val.Trim();

            if (!string.IsNullOrWhiteSpace(tval) && tval.Length == 8)
            {
                try
                {
                    t = DateTime.ParseExact(val, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (FormatException)
                {

                }
                catch (ArgumentException)
                {

                }
            }
            return t;
        }

        public static bool CompareCalendarItemList(IList<ViewModel.CalendarItem> a, IList<ViewModel.CalendarItem> b)
        {
            if (a.Count != b.Count) return false;

            foreach (var item in a)
            {
                if (!b.Contains(item)) return false;
            }

            return true;
        }
        

        public Stream GenerateStreamFromString(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public async Task<List<ViewModel.CalendarItem>> GetCalendar(Configuration.CalendarAndColor config)
        {
            List<ViewModel.CalendarItem> items = new List<ViewModel.CalendarItem>();

            try
            {
                string icsStr = await WebHelper.DownloadString(config.URL);

                string[] cal_array = icsStr.Split(new string[] { "\r\n", "\n", }, StringSplitOptions.RemoveEmptyEntries);
                var in_event = false;
                var in_alarm = false;
                //Use as a holder for the current event being proccessed.
                ViewModel.CalendarItem cur_event = null;
                for (var i = 0; i < cal_array.Length; i++)
                {
                    try
                    {

                        string ln = cal_array[i].Trim();
                        //If we encounted a new Event, create a blank event object + set in event options.
                        if (!in_event && ln == "BEGIN:VEVENT")
                        {
                            in_event = true;
                            cur_event = new ViewModel.CalendarItem();
                            cur_event.TextBrush = config.Foreground;
                        }
                        //If we encounter end event, complete the object and add it to our events array then clear it for reuse.
                        if (in_event && ln == "END:VEVENT")
                        {
                            in_event = false;


                            items.Add(cur_event);
                            cur_event = null;
                        }
                        //If we are in an event
                        else if (in_event)
                        {

                            if (ln == "BEGIN:VALARM")
                            {
                                in_alarm = true;
                            }
                            else if (ln == "END:VALARM")
                            {
                                in_alarm = false;
                            }
                            else
                            {
                                //Split the item based on the first ":"
                                var idx = ln.IndexOf(':');

                                if (idx != -1)
                                {
                                    //Apply trimming to values to reduce risks of badly formatted ical files.
                                    var type = ln.Substring(0, idx).Trim();//Trim
                                    if (in_alarm)
                                        type = "valarm_" + type;
                                    var val = ln.Substring(idx + 1).Trim();
                                    string subkey = "";
                                    if (type.Contains(";"))
                                    {
                                        string t2 = type;
                                        int subkeysep = t2.IndexOf(";");
                                        type = t2.Substring(0, subkeysep);
                                        subkey = t2.Substring(subkeysep + 1);
                                    }


                                    //If the type is a start date, proccess it and store details
                                    if (type == "DTSTART")
                                    {
                                        cur_event.Start = GetDateTime(val);
                                        if (cur_event.Start == DateTime.MinValue)
                                        {
                                            cur_event.Start = GetDateTime2(val);
                                        }
                                        if (cur_event.Start == DateTime.MinValue)
                                        {
                                            cur_event.Start = GetDateTime3(val);
                                        }
                                    }
                                    //If the type is an end date, do the same as above
                                    else if (type == "DTEND")
                                    {
                                        cur_event.End = GetDateTime(val);
                                        if (cur_event.End == DateTime.MinValue)
                                        {
                                            cur_event.End = GetDateTime2(val);
                                        }

                                        if (cur_event.End == DateTime.MinValue)
                                        {
                                            cur_event.End = GetDateTime3(val);
                                        }
                                    }
                                    //Convert timestamp
                                    else if (type == "DTSTAMP")
                                    {
                                        cur_event.Stamp = GetDateTime(val);
                                    }
                                    else if (type == "SUMMARY")
                                    {
                                        cur_event.Text = val
                                            .Replace("\\r\\n", "")
                                            .Replace("\\n", "")
                                            .Replace("\\,", ",")
                                            .Trim();
                                    }
                                    else if (type == "DESCRIPTION")
                                    {
                                        int j = i;
                                        while (j++ < cal_array.Length && cal_array[j].StartsWith(" "))
                                        {
                                            i++;
                                            val += cal_array[j].TrimStart();
                                        }
                                    }
                                    else if (type == "RRULE")
                                    {
                                        cur_event.RRULE = val;
                                    }
                                    else {
                                        val = val
                                            .Replace("\\r\\n", "")
                                            .Replace("\\n", "")
                                            .Replace("\\,", ",")
                                            .Trim();
                                    }

                                    //Add the value to our event object.
                                    cur_event.Values[type] = val;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.e(ex);
                    }
                }

            }
            catch (Exception ex)
            {
                Log.e(ex);
            }


            if (items.Count == 0) return null;

            List<ViewModel.CalendarItem> ritems = new List<ViewModel.CalendarItem>();

            foreach (var item in items)
            {
                if (string.IsNullOrEmpty(item.RRULE))
                {
                    ritems.Add(item);
                }
                else
                {
                    try
                    {
                        RecurrencePattern r = new RecurrencePattern(item.RRULE);

                        var duration = item.End - item.Start;
                        DateTime begin = item.Start;
                        DateTime end = begin.AddYears(10);
                        foreach (var date in RecurrencePatternSerializer.GetDates(begin, begin, end, 2, r, false))
                        {
                            var nitem = item.Copy();
                            nitem.Start = date;
                            nitem.End = date + duration;
                            nitem.RRULE = null;
                            ritems.Add(nitem);
                        }

                    }
                    catch (Exception ex)
                    {
                        Log.e(ex);
                    }
                }
            }



            return ritems;
        }


    }
}
