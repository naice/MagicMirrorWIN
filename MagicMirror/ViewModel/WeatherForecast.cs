using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public double DMaxTemp { get; set; }
        public double DMinTemp { get; set; }

        public ObservableCollection<WeatherForecastDetail> Details { get; set; } = new ObservableCollection<WeatherForecastDetail>();

        public void Update(WeatherForecast wf)
        {
            this.Day = wf.Day;
            this.Icon = wf.Icon;
            this.MaxTemp = wf.MaxTemp;
            this.MinTemp = wf.MinTemp;
            this.DMaxTemp = wf.DMaxTemp;
            this.DMinTemp = wf.DMinTemp;
            this.DateTime = wf.DateTime;
            this.Temp = wf.Temp;

            if (this.Details.Count == wf.Details.Count)
            {
                for (int i = 0; i < wf.Details.Count; i++)
                {
                    this.Details[i].Update(wf.Details[i]);
                }
            }
            else
            {
                this.Details.Clear();
                foreach (var item in wf.Details)
                {
                    this.Details.Add(item);
                }
            }
        }
    }
}
