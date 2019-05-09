using MagicMirror.Factory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MagicMirror.TemplateSelector
{
    public class CalendarItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DefaultTemplate { get; set; }
        public DataTemplate AppointmentWithin6HoursOrToday { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            return SelectTemplateCore(item, null);
        }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (!(item is ViewModel.CalendarItem calendarItem))
                return base.SelectTemplateCore(item, container);

            var now = DateTimeFactory.Instance.Now;
            var nowDay = new DateTime(now.Year, now.Month, now.Day);
            var nowDayEnd = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59);

            if ((calendarItem.Start > nowDay && calendarItem.Start <= nowDayEnd) ||
                ((calendarItem.Start - now).TotalHours < 6))
            {
                return AppointmentWithin6HoursOrToday;
            }

            return DefaultTemplate;
        }
    }
}
