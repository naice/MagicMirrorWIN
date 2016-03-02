using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;
using System.IO;

// EXTRACTED FROM DDay.iCal https://sourceforge.net/projects/dday-ical/ to use in UWP

namespace MagicMirror.Factory.RRule
{

    public partial class RecurrencePattern
    {
        #region Private Fields
        private FrequencyType _Frequency;
        private DateTime _Until = DateTime.MinValue;
        private int _Count = int.MinValue;
        private int _Interval = int.MinValue;
        private IList<int> _BySecond = new List<int>();
        private IList<int> _ByMinute = new List<int>();
        private IList<int> _ByHour = new List<int>();
        private IList<WeekDay> _ByDay = new List<WeekDay>();
        private IList<int> _ByMonthDay = new List<int>();
        private IList<int> _ByYearDay = new List<int>();
        private IList<int> _ByWeekNo = new List<int>();
        private IList<int> _ByMonth = new List<int>();
        private IList<int> _BySetPosition = new List<int>();
        private DayOfWeek _FirstDayOfWeek = DayOfWeek.Monday;
        private RecurrenceRestrictionType? _RestrictionType = null;
        private RecurrenceEvaluationModeType? _EvaluationMode = null;

        #endregion

        #region IRecurrencePattern Members

        public FrequencyType Frequency
        {
            get { return _Frequency; }
            set { _Frequency = value; }
        }

        public DateTime Until
        {
            get { return _Until; }
            set { _Until = value; }
        }

        public int Count
        {
            get { return _Count; }
            set { _Count = value; }
        }

        public int Interval
        {
            get
            {
                if (_Interval == int.MinValue)
                    return 1;
                return _Interval;
            }
            set { _Interval = value; }
        }

        public IList<int> BySecond
        {
            get { return _BySecond; }
            set { _BySecond = value; }
        }

        public IList<int> ByMinute
        {
            get { return _ByMinute; }
            set { _ByMinute = value; }
        }

        public IList<int> ByHour
        {
            get { return _ByHour; }
            set { _ByHour = value; }
        }

        public IList<WeekDay> ByDay
        {
            get { return _ByDay; }
            set { _ByDay = value; }
        }

        public IList<int> ByMonthDay
        {
            get { return _ByMonthDay; }
            set { _ByMonthDay = value; }
        }

        public IList<int> ByYearDay
        {
            get { return _ByYearDay; }
            set { _ByYearDay = value; }
        }

        public IList<int> ByWeekNo
        {
            get { return _ByWeekNo; }
            set { _ByWeekNo = value; }
        }

        public IList<int> ByMonth
        {
            get { return _ByMonth; }
            set { _ByMonth = value; }
        }

        public IList<int> BySetPosition
        {
            get { return _BySetPosition; }
            set { _BySetPosition = value; }
        }

        public DayOfWeek FirstDayOfWeek
        {
            get { return _FirstDayOfWeek; }
            set { _FirstDayOfWeek = value; }
        }

        public RecurrenceRestrictionType RestrictionType
        {
            get
            {
                // NOTE: Fixes bug #1924358 - Cannot evaluate Secondly patterns
                if (_RestrictionType != null &&
                    _RestrictionType.HasValue)
                    return _RestrictionType.Value;
                else
                    return RecurrenceRestrictionType.Default;
            }
            set { _RestrictionType = value; }
        }

        public RecurrenceEvaluationModeType EvaluationMode
        {
            get
            {
                // NOTE: Fixes bug #1924358 - Cannot evaluate Secondly patterns
                if (_EvaluationMode != null &&
                    _EvaluationMode.HasValue)
                    return _EvaluationMode.Value;
                else
                    return RecurrenceEvaluationModeType.Default;
            }
            set { _EvaluationMode = value; }
        }

        #endregion

        #region Constructors

        public RecurrencePattern()
        {

        }

        public RecurrencePattern(string value)
        {
            if (value != null)
            {
                RecurrencePatternSerializer serializer = new RecurrencePatternSerializer();
                CopyFrom(serializer.Deserialize(new StringReader(value)) as RecurrencePattern);
            }
        }

        #endregion

        #region Overrides
        
        public override bool Equals(object obj)
        {
            if (obj is RecurrencePattern)
            {
                RecurrencePattern r = (RecurrencePattern)obj;
                if (!CollectionEquals(r.ByDay, ByDay) ||
                    !CollectionEquals(r.ByHour, ByHour) ||
                    !CollectionEquals(r.ByMinute, ByMinute) ||
                    !CollectionEquals(r.ByMonth, ByMonth) ||
                    !CollectionEquals(r.ByMonthDay, ByMonthDay) ||
                    !CollectionEquals(r.BySecond, BySecond) ||
                    !CollectionEquals(r.BySetPosition, BySetPosition) ||
                    !CollectionEquals(r.ByWeekNo, ByWeekNo) ||
                    !CollectionEquals(r.ByYearDay, ByYearDay))
                    return false;
                if (r.Count != Count) return false;
                if (r.Frequency != Frequency) return false;
                if (r.Interval != Interval) return false;
                if (r.Until != DateTime.MinValue)
                {
                    if (!r.Until.Equals(Until))
                        return false;
                }
                else if (Until != DateTime.MinValue)
                    return false;
                if (r.FirstDayOfWeek != FirstDayOfWeek) return false;
                return true;
            }
            return base.Equals(obj);
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public override int GetHashCode()
        {
            int hashCode =
                ByDay.GetHashCode() ^ ByHour.GetHashCode() ^ ByMinute.GetHashCode() ^
                ByMonth.GetHashCode() ^ ByMonthDay.GetHashCode() ^ BySecond.GetHashCode() ^
                BySetPosition.GetHashCode() ^ ByWeekNo.GetHashCode() ^ ByYearDay.GetHashCode() ^
                Count.GetHashCode() ^ Frequency.GetHashCode();

            if (Interval.Equals(1))
                hashCode ^= 0x1;
            else hashCode ^= Interval.GetHashCode();
                        
            hashCode ^= Until.GetHashCode();
            hashCode ^= FirstDayOfWeek.GetHashCode();
            return hashCode;
        }

        public void CopyFrom(RecurrencePattern r)
        {
            if (r != null)
            {
                Frequency = r.Frequency;
                Until = r.Until;
                Count = r.Count;
                Interval = r.Interval;
                BySecond = new List<int>(r.BySecond);
                ByMinute = new List<int>(r.ByMinute);
                ByHour = new List<int>(r.ByHour);
                ByDay = new List<WeekDay>(r.ByDay);
                ByMonthDay = new List<int>(r.ByMonthDay);
                ByYearDay = new List<int>(r.ByYearDay);
                ByWeekNo = new List<int>(r.ByWeekNo);
                ByMonth = new List<int>(r.ByMonth);
                BySetPosition = new List<int>(r.BySetPosition);
                FirstDayOfWeek = r.FirstDayOfWeek;
                RestrictionType = r.RestrictionType;
                EvaluationMode = r.EvaluationMode;
            }            
        }

        private bool CollectionEquals<T>(IList<T> c1, IList<T> c2)
        {
            // NOTE: fixes a bug where collections weren't properly compared
            if (c1 == null ||
                c2 == null)
            {
                if (c1 == c2)
                    return true;
                else return false;
            }
            if (!c1.Count.Equals(c2.Count))
                return false;

            IEnumerator e1 = c1.GetEnumerator();
            IEnumerator e2 = c2.GetEnumerator();

            while (e1.MoveNext() && e2.MoveNext())
            {
                if (!object.Equals(e1.Current, e2.Current))
                    return false;
            }
            return true;
        }

        #endregion
        
    }
}
