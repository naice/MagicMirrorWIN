using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicMirror.ViewModel
{
    public class WeatherForecastDetail : BaseViewModel
    {
        private string _Icon;
        public string Icon
        {
            get { return _Icon; }
            set
            {
                if (value != _Icon)
                {
                    _Icon = value;
                    RaisePropertyChanged("Icon");
                }
            }
        }
        private string _Temp;
        public string Temp
        {
            get { return _Temp; }
            set
            {
                if (value != _Temp)
                {
                    _Temp = value;
                    RaisePropertyChanged("Temp");
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
        private DateTime _DateTime;
        public DateTime DateTime
        {
            get { return _DateTime; }
            set
            {
                if (value != _DateTime)
                {
                    _DateTime = value;
                    RaisePropertyChanged("DateTime");
                }
            }
        }


        public void Update(WeatherForecastDetail wf)
        {
            this.Icon = wf.Icon;
            this.Temp = wf.Temp;
            this.Time = wf.Time;
            this.DateTime = wf.DateTime;
        }
    }
}
