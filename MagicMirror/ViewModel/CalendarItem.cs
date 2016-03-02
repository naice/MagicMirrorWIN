using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Media;

namespace MagicMirror.ViewModel
{
    public class CalendarItem : BaseViewModel
    {
        public double Opacity { get; set; }

        public string Text { get; set; }
        public string TextBrush { get; set; }
        public string Time { get; set; }
        public string RRULE { get; set; }

        public DateTime Start{ get; set; }
        public DateTime End{ get; set; }
        public DateTime Stamp { get; set; }

        public Dictionary<string, string> Values { get; set; } = new Dictionary<string, string>();


        public CalendarItem Copy()
        {
            CalendarItem c = this.MemberwiseClone() as CalendarItem;
            return c;
        }

        public override bool Equals(object obj)
        {
            if (obj is CalendarItem)
            {
                CalendarItem a = this, b = obj as CalendarItem;

                if (a.Text == b.Text && a.Start == b.Start)
                    return true;
            }
            return false;
        }

        public static bool operator <(CalendarItem a, CalendarItem b)
        {
            return a.Start < b.Start;
        }
        public static bool operator >(CalendarItem a, CalendarItem b)
        {
            return a.Start > b.Start;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return Text + Start.ToString();
        }
    }
}