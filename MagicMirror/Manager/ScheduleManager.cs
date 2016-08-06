using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicMirror.Manager
{


    public class ScheduleManager
    {
        private class MagicMirrorDateTimeProvider : IDateTimeNowProvider
        {
            public DateTime Now
            {
                get
                {
                    return Factory.DateTimeFactory.Instance.Now;
                }
            }
        }


        #region SINGLETON
        private static ScheduleManager _Instance;
        public static ScheduleManager Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new ScheduleManager();
                return _Instance;
            }
        }
        ScheduleManager()
        {
            Scheduler = new Scheduler(new MagicMirrorDateTimeProvider());
        }
        #endregion

        
        public Scheduler Scheduler
        {
            get;
            private set;
        }
    }
}
