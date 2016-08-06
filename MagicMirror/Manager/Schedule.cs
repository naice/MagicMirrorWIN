using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicMirror.Manager
{
    public class Schedule
    {
        /// <summary>
        /// Free object to set.
        /// </summary>
        public object Tag { get; set; }
        /// <summary>
        /// This date will trigger the schedule.
        /// </summary>
        public DateTime Begin { get; internal set; }
        /// <summary>
        /// This is the action triggerd by this schedule.
        /// </summary>
        public Action Action { get; set; }
        /// <summary>
        /// CancelationToken to cancel the schedule.
        /// </summary>
        public System.Threading.CancellationTokenSource CancellationTokenSource => new System.Threading.CancellationTokenSource();
        /// <summary>
        /// If set to <see cref="TimeSpan.Zero"/> or smaller no recurrence will occur, otherwise the <see cref="TimeSpan"/> will be added to <see cref="Begin"/> and the schedule gets restartet.
        /// </summary>
        public TimeSpan Recurrence { get; set; } = TimeSpan.Zero;

        /// <summary>
        /// Create a new <see cref="Schedule"/> instance.
        /// </summary>
        /// <param name="action">The action performed.</param>
        /// <param name="begin">The DateTime in future to trigger the schedules action.</param>
        public Schedule(Action action, DateTime begin)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            if (begin == DateTime.MinValue)
            {
                throw new ArgumentException("begin");
            }

            Begin = begin;
            Action = action;
        }

        /// <summary>
        /// No recurrence at all.
        /// </summary>
        public static readonly TimeSpan RecurrenceOnce = TimeSpan.Zero;
        /// <summary>
        /// Recurrence for exactly one day.
        /// </summary>
        public static readonly TimeSpan RecurrenceDaily = TimeSpan.FromDays(1);
        /// <summary>
        /// Recurrence for exactly one week = 7 days.
        /// </summary>
        public static readonly TimeSpan RecurrenceWeekly = TimeSpan.FromDays(7);
    }
}
