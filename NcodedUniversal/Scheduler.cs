using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NcodedUniversal
{
    public interface IDateTimeNowProvider
    {
        DateTime Now { get; }
    }

    public class DefaultDateTimeProvider : IDateTimeNowProvider
    {
        public DateTime Now
        {
            get
            {
                return DateTime.Now;
            }
        }
    }

    public class SchedulerDateException : Exception
    {
        public SchedulerDateException(string msg) : base(msg)
        {

        }
    }

    public enum ScheduleReccurence { Once, Daily, Weekly, Monthly }

    public class Scheduler
    {
        private readonly IDateTimeNowProvider _dateTimeProvider;
        private readonly List<Schedule> _schedules = new List<Schedule>();
        public ReadOnlyCollection<Schedule> Schedules { get { return _schedules.AsReadOnly(); } }

        public Scheduler() : this(new DefaultDateTimeProvider())
        {

        }

        public Scheduler(IDateTimeNowProvider dateTimeProvider)
        {
            _dateTimeProvider = dateTimeProvider;
        }

        /// <summary>
        /// Starts the give schedule.
        /// </summary>
        /// <param name="schedule">The schedule that will be started.</param>
        public void StartSchedule(Schedule schedule)
        {
            //Contract.Assert(schedule != null);

            if (schedule == null)
            {
                throw new ArgumentNullException("schedule");
            }

            if (schedule.Begin <= _dateTimeProvider.Now)
            {
                if (schedule.Recurrence <= TimeSpan.Zero)
                {
                    throw new SchedulerDateException("Can't add schedule in past!");
                }

                TryStartRecurringSchedule(schedule);
                return;
            }

            _schedules.Add(schedule);
            _schedules.Sort((A, B) => A.Begin.CompareTo(B.Begin));

            Task.Delay(schedule.Begin - _dateTimeProvider.Now, schedule.CancellationTokenSource.Token)
                .ContinueWith(A => {
                // remove schedule first!
                _schedules.Remove(schedule);

                    if (A.IsCompleted)
                    {
                        schedule.Action?.Invoke();

                        TryStartRecurringSchedule(schedule);
                    }
            });
        }

        private void TryStartRecurringSchedule(Schedule schedule)
        {
            if (schedule.Recurrence > TimeSpan.Zero)
            {
                schedule.Begin = schedule.Begin.Add(schedule.Recurrence);
                StartSchedule(schedule);
            }
        }

        /// <summary>
        /// Removes given schedule and cancel task.
        /// </summary>
        /// <param name="schedule">Schedule to remove and Cancel</param>
        public void CancelSchedule(Schedule schedule)
        {
            if (_schedules.Contains(schedule))
            {
                schedule.CancellationTokenSource.Cancel();
                _schedules.Remove(schedule);
            }
        }

        public static bool IsWeekend(DateTime dt)
        {
            return dt.DayOfWeek == DayOfWeek.Saturday || dt.DayOfWeek == DayOfWeek.Sunday;
        }

        public Schedule CreateRecurringScheduleFromToday(Action action, TimeSpan when, TimeSpan recurrence)
        {
            var schedule = new Schedule(action, _dateTimeProvider.Now.Date.Add(when));
            schedule.Recurrence = recurrence;
            return schedule;
        }
    }
}
