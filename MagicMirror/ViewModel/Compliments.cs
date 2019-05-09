using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MagicMirror.Configuration;

namespace MagicMirror.ViewModel
{
    public class Compliments : BaseViewModel, IUpdateViewModel
    {
        private List<string> _Items;
        public List<string> Items
        {
            get { return _Items; }
            set
            {
                if (value != _Items)
                {
                    _Items = value;
                    RaisePropertyChanged("Items");
                }
            }
        }

        public DateTime LastUpdate { get; set; } = DateTime.MinValue;
        public SemaphoreSlim UILock { get; set; } = new SemaphoreSlim(1, 1);
        public TimeSpan UpdateTimeout { get; set; } = TimeSpan.FromMinutes(1);
        
        private DayTime _complimentType = DayTime.NONE;

        public async Task<object> Update(Configuration.Configuration config)
        {
            await Task.Delay(100); // Maybe fetch from service? 

            var hour = Factory.DateTimeFactory.Instance.Now.Hour;

            List<string> compliments = new List<string>();

            if (hour >= 3 && hour < 12 && _complimentType != DayTime.Morning)
            {
                _complimentType = DayTime.Morning;
                compliments = new List<string>(config.DayTimeCompliments[_complimentType]);
            }
            else if (hour >= 12 && hour < 17 && _complimentType != DayTime.Afternoon)
            {
                _complimentType = DayTime.Afternoon;
                compliments = new List<string>(config.DayTimeCompliments[_complimentType]);
            }
            else if (hour >= 17 || hour < 3 && _complimentType != DayTime.Evening)
            {
                _complimentType = DayTime.Evening;
                compliments = new List<string>(config.DayTimeCompliments[_complimentType]);

            }

            return compliments;
        }

        public void UpdateUI(Configuration.Configuration config, object data)
        {
            var compliments = data as List<string>;

            if (compliments != null && compliments.Count > 0)
            {
                Items = new List<string>(compliments);
            }
        }
    }
}
