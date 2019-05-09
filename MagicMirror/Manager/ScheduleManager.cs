﻿using NcodedUniversal;
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
            _scheduler = new Scheduler(new MagicMirrorDateTimeProvider());
        }
        #endregion

        private readonly Scheduler _scheduler;
        
        public Scheduler Scheduler
        {
            get { return _scheduler; }
        }
    }
}
