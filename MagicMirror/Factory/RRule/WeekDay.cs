using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicMirror.Factory.RRule
{
    public class WeekDay
    {
        public int Offset { get; set; } = int.MinValue;
        public DayOfWeek DayOfWeek { get; set; }
    }
}
