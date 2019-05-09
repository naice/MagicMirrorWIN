using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MagicMirror.Configuration;
using Windows.UI.Xaml;
using MagicMirror.Factory;
using System.Threading;

namespace MagicMirror.ViewModel
{
    public class Calendar : BaseViewModel, IUpdateViewModel
    {
        private DispatcherTimer timerTime = null;

        private DateTime _CurrentDateTimeSecond;
        public DateTime CurrentDateTimeSecond
        {
            get { return _CurrentDateTimeSecond; }
            set
            {
                if (value != _CurrentDateTimeSecond)
                {
                    _CurrentDateTimeSecond = value;
                    RaisePropertyChanged("CurrentDateTimeSecond");
                }
            }
        }
        private DateTime _CurrentDateTimeMinute;
        public DateTime CurrentDateTimeMinute
        {
            get { return _CurrentDateTimeMinute; }
            set
            {
                if (value != _CurrentDateTimeMinute)
                {
                    _CurrentDateTimeMinute = value;
                    RaisePropertyChanged("CurrentDateTimeMinute");
                }
            }
        }
        private DateTime _CurrentDate;
        public DateTime CurrentDate
        {
            get { return _CurrentDate; }
            set
            {
                if (value != _CurrentDate)
                {
                    _CurrentDate = value;
                    RaisePropertyChanged("CurrentDate");
                }
            }
        }

        private string _CurrentDateString;
        public string CurrentDateString
        {
            get { return _CurrentDateString; }
            set
            {
                if (value != _CurrentDateString)
                {
                    _CurrentDateString = value;
                    RaisePropertyChanged("CurrentDateString");
                }
            }
        }
        private string _CurrentDateTimeMinuteString;
        public string CurrentDateTimeMinuteString
        {
            get { return _CurrentDateTimeMinuteString; }
            set
            {
                if (value != _CurrentDateTimeMinuteString)
                {
                    _CurrentDateTimeMinuteString = value;
                    RaisePropertyChanged("CurrentDateTimeMinuteString");
                }
            }
        }
        private string _CurrentDateTimeSecondString;
        public string CurrentDateTimeSecondString
        {
            get { return _CurrentDateTimeSecondString; }
            set
            {
                if (value != _CurrentDateTimeSecondString)
                {
                    _CurrentDateTimeSecondString = value;
                    RaisePropertyChanged("CurrentDateTimeSecondString");
                }
            }
        }

        private ObservableCollection<CalendarItem> _CalendarItems = new ObservableCollection<CalendarItem>();
        public ObservableCollection<CalendarItem> CalendarItems
        {
            get { return _CalendarItems; }
            set
            {
                if (value != _CalendarItems)
                {
                    _CalendarItems = value;
                    RaisePropertyChanged("CalendarItems");
                }
            }
        }


        public Calendar()
        {
            timerTime = new DispatcherTimer();
            timerTime.Interval = TimeSpan.FromSeconds(0.5);
            timerTime.Tick += timerTime_Tick;
            timerTime.Start();
        }

        private async void timerTime_Tick(object sender, object e)
        {
            var now = DateTimeFactory.Instance.Now;
            var date = now.Date;
            var dateTimeMinute = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);
            var dateTimeSecond = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);

            CurrentDate = date;
            CurrentDateTimeMinute = dateTimeMinute;
            CurrentDateTimeSecond = dateTimeSecond;

            CurrentDateString = _CurrentDate.ToString("dddd, d. MMMM yyyy");
            CurrentDateTimeMinuteString = _CurrentDateTimeMinute.ToString("HH:mm");
            CurrentDateTimeSecondString = _CurrentDateTimeSecond.ToString("ss");

            try
            {
                await UILock.WaitAsync();
                var removeitems = new List<CalendarItem>();
                foreach (var item in CalendarItems)
                {
                    UpdateTime(item, now);

                    if (item.Start < date && item.End < date)
                        removeitems.Add(item);
                }

                foreach (var item in removeitems)
                {
                    CalendarItems.Remove(item);
                }
            }
            finally
            {
                UILock.Release();
            }
        }

        public DateTime LastUpdate { get; set; }
        public SemaphoreSlim UILock { get; set; } = new SemaphoreSlim(1, 1);
        public TimeSpan UpdateTimeout { get; set; } = TimeSpan.FromMinutes(5);

        private CalendarFactory _calendarFactory = new CalendarFactory();

        private void UpdateTime(CalendarItem item, DateTime now)
        {
            // todo: Extrahieren, globale funktion...

            var mnth = new string[] { "Januar", "Februar", "März", "April", "Mai", "Juni", "Juli", "August", "September", "Oktober", "November", "Dezember" };

            var dt = item.Start;
            var nowDay = new DateTime(now.Year, now.Month, now.Day);
            var nowDayEnd = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59);
            var nowMonth = new DateTime(now.Year, now.Month, 1);
            var nowMonthEnd = new DateTime(now.Year, now.Month, 
                DateTime.DaysInMonth(now.Year, now.Month), 23, 59, 59);
            var nowYear = new DateTime(now.Year, 1, 1);
            var nowYearEnd = new DateTime(now.Year + 1, 1, 1).AddSeconds(-1);

            var endTodayBegin = item.Start.AddDays(1).AddMinutes(-1);
            var endTodayEnd = item.Start.AddDays(1).AddMinutes(1);

            if (item.Start.Date == now.Date && item.End > endTodayBegin && item.End < endTodayEnd)
            {
                item.Time = "heute ganztägig";
            }
            else if (dt < now.AddMinutes(-1))
            {
                item.Time = string.Format("vor {0} Minuten", (int)(now - dt).TotalMinutes);
                item.TimeBrush = "#ffFF3333";
            }
            else if (dt < now.AddMinutes(1) && dt > now.AddMinutes(-1))
            {
                item.Time = "Jetzt";
                item.TimeBrush = "#ffFF3333";
            }
            else if (dt < now.AddHours(1))
                item.Time = string.Format("in {0} Minuten", (int)(dt - now).TotalMinutes);
            else if (dt < now.AddHours(6) || dt < nowDayEnd)
                item.Time = string.Format("in {0} Stunden", (int)(dt - now).TotalHours);
            else if (dt < nowDayEnd.AddDays(1))
                item.Time = string.Format("Morgen um {0:HH}:{0:mm}", dt);
            else if (dt < nowDayEnd.AddDays(2))
                item.Time = string.Format("Übermorgen um {0:HH}:{0:mm}", dt);
            else if (dt < nowDayEnd.AddDays(6))
                item.Time = string.Format("{0:dddd} um {0:HH}:{0:mm}", dt);
            else if (dt < nowDayEnd.AddDays(15) || dt < nowMonthEnd)
                item.Time = string.Format("in {0} Tagen", (int)(dt - nowDay).TotalDays);
            else if (dt < nowMonthEnd.AddMonths(1))
                item.Time = string.Format("nächsten Monat");
            else if (dt < nowMonthEnd.AddMonths(6) || dt < nowYearEnd)
                item.Time = string.Format("in {0} Monaten", nowYear.Year == dt.Year ? dt.Month - now.Month : 12 - now.Month + dt.Month);
            else if (dt.Year - now.Year == 1)
                //item.Time = string.Format("nächstes Jahr im {0:MMMM}", dt);
                item.Time = string.Format("nächstes Jahr im {0}", mnth[dt.Month-1]);
            else
                item.Time = string.Format("in {0} Jahren", dt.Year - now.Year);
        }

        public async Task<object> ProcessData(Configuration.Configuration config)
        {
            List<CalendarItem> newCalendarItems = await _calendarFactory.GetFullCalendarList(config);
            var now = DateTimeFactory.Instance.Now;
            var today = now.Date;
            List<CalendarItem> filteredCalendarItems = new List<CalendarItem>();
            foreach (var item in newCalendarItems)
            {
                if (item.Start >= today)
                {
                    try
                    {
                        UpdateTime(item, now);

                        filteredCalendarItems.Add(item);
                    }
                    catch (Exception ex)
                    {
                        Sentry.SentrySdk.CaptureException(ex);
                        Log.e(ex);
                    }

                }
            }

            return new List<CalendarItem>(filteredCalendarItems.Take(config.MaxCalendarItems));
        }

        public void UpdateUI(Configuration.Configuration config, object data)
        {
            var filteredCalendarItems = data as List<CalendarItem>;
            if (filteredCalendarItems != null)
            {
                if (!CalendarFactory.CompareCalendarItemList(_CalendarItems, filteredCalendarItems))
                {
                    double opacity = 1;
                    filteredCalendarItems.ForEach((A => { A.Opacity = opacity; opacity -= 1d / config.MaxCalendarItems; }));

                    CalendarItems = new ObservableCollection<CalendarItem>(filteredCalendarItems);
                }
            }
        }
    }
}
