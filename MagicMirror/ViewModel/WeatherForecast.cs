using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicMirror.ViewModel
{
    public class WeatherForecast : BaseViewModel
    {
        private string _Day;
        public string Day
        {
            get { return _Day; }
            set
            {
                if (value != _Day)
                {
                    _Day = value;
                    RaisePropertyChanged("Day");
                }
            }
        }
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
        private string _MinTemp;
        public string MinTemp
        {
            get { return _MinTemp; }
            set
            {
                if (value != _MinTemp)
                {
                    _MinTemp = value;
                    RaisePropertyChanged("MinTemp");
                }
            }
        }
        private string _MaxTemp;
        public string MaxTemp
        {
            get { return _MaxTemp; }
            set
            {
                if (value != _MaxTemp)
                {
                    _MaxTemp = value;
                    RaisePropertyChanged("MaxTemp");
                }
            }
        }
        private double _Opacity;
        public double Opacity
        {
            get { return _Opacity; }
            set
            {
                if (value != _Opacity)
                {
                    _Opacity = value;
                    RaisePropertyChanged("Opacity");
                }
            }
        }

        public void Update(WeatherForecast wf)
        {
            this.Day = wf.Day;
            this.Icon = wf.Icon;
            this.MaxTemp = wf.MaxTemp;
            this.MinTemp = wf.MinTemp;
        }
    }
}
