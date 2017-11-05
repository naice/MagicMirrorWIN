using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MagicMirror.Configuration;
using MagicMirror.Factory;
using MagicMirror.Provider;
using MagicMirror.Contracts;

namespace MagicMirror.ViewModel
{
    public class Weather : BaseViewModel, IUpdateViewModel, ISpeechRecognitionResultGenerated
    {
        private string _BeaufortWindScale;
        public string BeaufortWindScale
        {
            get { return _BeaufortWindScale; }
            set
            {
                if (value != _BeaufortWindScale)
                {
                    _BeaufortWindScale = value;
                    RaisePropertyChanged("BeaufortWindScale");
                }
            }
        }
        private string _SunState;
        public string SunState
        {
            get { return _SunState; }
            set
            {
                if (value != _SunState)
                {
                    _SunState = value;
                    RaisePropertyChanged("SunState");
                }
            }
        }
        private string _SunStateTime;
        public string SunStateTime
        {
            get { return _SunStateTime; }
            set
            {
                if (value != _SunStateTime)
                {
                    _SunStateTime = value;
                    RaisePropertyChanged("SunStateTime");
                }
            }
        }
        private string _WeatherIcon;
        public string WeatherIcon
        {
            get { return _WeatherIcon; }
            set
            {
                if (value != _WeatherIcon)
                {
                    _WeatherIcon = value;
                    RaisePropertyChanged("WeatherIcon");
                }
            }
        }
        private string _Temperature;
        public string Temperature
        {
            get { return _Temperature; }
            set
            {
                if (value != _Temperature)
                {
                    _Temperature = value;
                    RaisePropertyChanged("Temperature");
                }
            }
        }

        private bool _ShowDetail;
        public bool ShowDetail
        {
            get { return _ShowDetail; }
            set
            {
                if (value != _ShowDetail)
                {
                    _ShowDetail = value;
                    RaisePropertyChanged("ShowDetail");
                }
            }
        }

        public ObservableCollection<WeatherForecast> Forecasts { get; set; } = new ObservableCollection<WeatherForecast>();
        public ObservableCollection<WeatherForecast> ForecastDetails { get; set; } = new ObservableCollection<WeatherForecast>();

        private Timeout _hideDetailTimeout;

        public Weather()
        {
            try
            {
                _hideDetailTimeout = new Timeout(() => HideDetail());
                _hideDetailTimeout.Duration = TimeSpan.FromMinutes(5);
            }
            catch
            {
                // Also used in background thread.
            }

        }

        public async void HideDetail()
        {
            if (_ShowDetail)
            {
                _hideDetailTimeout.Stop();
                ShowDetail = false;
                await Task.Delay(900);
            }
        }
        public async void ViewDetail()
        {
            if (!_ShowDetail)
            {
                ShowDetail = true;
                await Task.Delay(900);

                _hideDetailTimeout.Start();
            }
        }

        #region UPDATE MECHANISM
        private WeatherFactory _weatherFactory = new WeatherFactory();
        public TimeSpan UpdateTimeout { get; set; } = TimeSpan.FromHours(1);
        public DateTime LastUpdate { get; set; } = DateTime.MinValue;
        public SemaphoreSlim UILock { get; set; } = new SemaphoreSlim(1, 1);

        public async Task<object> ProcessData(Configuration.Configuration config)
        {
            return await _weatherFactory.GetWeather(config);
        }

        public void UpdateUI(Configuration.Configuration config, object data)
        {
            var weather = data as Weather;

            if (weather != null)
            {
                this.BeaufortWindScale = weather.BeaufortWindScale;
                this.SunState = weather.SunState;
                this.SunStateTime = weather.SunStateTime;
                this.Temperature = weather.Temperature;
                this.WeatherIcon = weather.WeatherIcon;

                if (this.Forecasts.Count == weather.Forecasts.Count)
                {
                    for (int i = 0; i < weather.Forecasts.Count; i++)
                    {
                        this.Forecasts[i].Update(weather.Forecasts[i]);
                    }
                }
                else
                {
                    double opacity = 1;
                    this.Forecasts.Clear();
                    foreach (var item in weather.Forecasts)
                    {
                        this.Forecasts.Add(item);
                        item.Opacity = opacity;
                        opacity -= 0.155;
                    }
                }

                if (this.ForecastDetails.Count == weather.ForecastDetails.Count)
                {
                    for (int i = 0; i < weather.ForecastDetails.Count; i++)
                    {
                        this.ForecastDetails[i].Update(weather.ForecastDetails[i]);
                    }
                }
                else
                {
                    double opacity = 1;
                    this.ForecastDetails.Clear();
                    foreach (var item in weather.ForecastDetails)
                    {
                        this.ForecastDetails.Add(item);
                        item.Opacity = opacity;
                        opacity -= 0.155;
                    }
                }
            }
        }
        #endregion

        public void SpeechRecognitionResultGenerated(SpeechRecognitionResult result)
        {
            if (result.IsCancel())
            {
                UI.EnsureOn(() => HideDetail());
            }
            else if (result.TextUpper == "WEATHER")
            {
                UI.EnsureOn(() => ViewDetail());
                result.IsHandled = true;
            }
        }
    }
}
