using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Media;

namespace MagicMirror.ViewModel
{
    public class CalendarItem : BaseViewModel
    {
        public double Opacity { get; set; }
        
        private string _Text;
        public string Text
        {
            get { return _Text; }
            set
            {
                if (value != _Text)
                {
                    _Text = value;
                    RaisePropertyChanged("Text");
                }
            }
        }
        private string _TextBrush;
        public string TextBrush
        {
            get { return _TextBrush; }
            set
            {
                if (value != _TextBrush)
                {
                    _TextBrush = value;
                    RaisePropertyChanged("TextBrush");
                }
            }
        }
        private string _Time;
        public string Time
        {
            get { return _Time; }
            set
            {
                if (value != _Time)
                {
                    _Time = value;
                    RaisePropertyChanged("Time");
                }
            }
        }
        private string _TimeBrush = "#ffFFFFFF";
        public string TimeBrush
        {
            get { return _TimeBrush; }
            set
            {
                if (value != _TimeBrush)
                {
                    _TimeBrush = value;
                    RaisePropertyChanged("TimeBrush");
                }
            }
        }
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