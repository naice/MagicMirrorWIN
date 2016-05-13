using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicMirror.Factory
{
    public class WeatherFactory
    {
        public static class WIWindBeaufort
        {
            public static readonly string[] Beaufort = new string[] {
                "\U0000f0b7", "\U0000f0b8", "\U0000f0b9", "\U0000f0ba", "\U0000f0bb", "\U0000f0bc",
                "\U0000f0bd", "\U0000f0be", "\U0000f0bf", "\U0000f0c0", "\U0000f0c1", "\U0000f0c2",
                "\U0000f0c3",
            };

            public static string Beaufort0  { get; private set; } = Beaufort[0 ];
            public static string Beaufort1  { get; private set; } = Beaufort[1 ];
            public static string Beaufort2  { get; private set; } = Beaufort[2 ];
            public static string Beaufort3  { get; private set; } = Beaufort[3 ];
            public static string Beaufort4  { get; private set; } = Beaufort[4 ];
            public static string Beaufort5  { get; private set; } = Beaufort[5 ];
            public static string Beaufort6  { get; private set; } = Beaufort[6 ];
            public static string Beaufort7  { get; private set; } = Beaufort[7 ];
            public static string Beaufort8  { get; private set; } = Beaufort[8 ];
            public static string Beaufort9  { get; private set; } = Beaufort[9 ];
            public static string Beaufort10 { get; private set; } = Beaufort[10];
            public static string Beaufort11 { get; private set; } = Beaufort[11];
            public static string Beaufort12 { get; private set; } = Beaufort[12];

            private static readonly double[] beaufortSpeeds = new double[] 
            {
                1, 5, 11, 19, 28, 38, 49, 61, 74, 88, 102, 117, 1000
            };
            public static int KMHToBeaufort(double kmh)
            {
	            for (int i = 0; i< beaufortSpeeds.Length; i++)
                {
                    var speed = beaufortSpeeds[i];
                    if (speed > kmh)
                    {
                        return i;
                    }
                }
                return 12;
            }
        }

        public static class WIWeather
        {
            public static string Sunrise { get; set; } = "\U0000f051";
            public static string Sunset { get; set; } = "\U0000f052";

            //public static string Sunrise { get; set; } = "\U0000";

            public static readonly Dictionary<string, string> IconTable = new Dictionary<string, string>() {
                {"01d","\U0000f00d"},
                {"02d","\U0000f002"},
                {"03d","\U0000f013"},
                {"04d","\U0000f012"},
                {"09d","\U0000f01a"},
                {"10d","\U0000f019"},
                {"11d","\U0000f01e"},
                {"13d","\U0000f01b"},
                {"50d","\U0000f014"},
                {"01n","\U0000f02e"},
                {"02n","\U0000f031"},
                {"03n","\U0000f031"},
                {"04n","\U0000f031"},
                {"09n","\U0000f037"},
                {"10n","\U0000f036"},
                {"11n","\U0000f03b"},
                {"13n","\U0000f038"},
                {"50n","\U0000f023"}
            };
        }


        public async Task<ViewModel.Weather> GetWeather(Configuration.Configuration config)
        {
            ViewModel.Weather weather = new ViewModel.Weather();
            WeatherNet.ClientSettings.ApiUrl = config.WeatherAPIUrl;
            WeatherNet.ClientSettings.ApiKey = config.WeatherAPIKey;

            // get current weather info
            var currentWeather = await WeatherNet.Clients.CurrentWeather.GetByCityNameAsync(
                config.WeatherCity, config.WeatherCountry, config.WeatherLanguage, config.WeatherUnits);
            
            if (!currentWeather.Success) return null;

            weather.BeaufortWindScale = WIWindBeaufort.Beaufort[WIWindBeaufort.KMHToBeaufort(currentWeather.Item.WindSpeed)];
            var now = DateTimeFactory.Instance.Now;

            weather.SunState = WIWeather.Sunrise;
            weather.SunStateTime = currentWeather.Item.Sunrise.ToString("HH:mm");
            if (currentWeather.Item.Sunrise < now && currentWeather.Item.Sunset > now)
            {
                weather.SunState = WIWeather.Sunset;
                weather.SunStateTime = currentWeather.Item.Sunset.ToString("HH:mm");
            }

            weather.Temperature = string.Format("{0:0.0}°", currentWeather.Item.Temp);
            weather.WeatherIcon = WIWeather.IconTable[currentWeather.Item.Icon];

            var forecastDaily = await WeatherNet.Clients.FiveDaysForecast.GetByCityNameDailyAsync(config.WeatherCity, config.WeatherCountry, config.WeatherLanguage, config.WeatherUnits);
            var forecastDetail = await WeatherNet.Clients.FiveDaysForecast.GetByCityNameAsync(config.WeatherCity, config.WeatherCountry, config.WeatherLanguage, config.WeatherUnits);


            if (forecastDaily.Success)
            {
                foreach (var item in forecastDaily.Items)
                {
                    var forecast = new ViewModel.WeatherForecast();
                    forecast.Icon = WIWeather.IconTable[item.Icon];
                    forecast.Day = item.Date.ToString("ddd.");
                    forecast.MaxTemp = string.Format("{0:0.0}°", item.TempMax);
                    forecast.MinTemp = string.Format("{0:0.0}°", item.TempMin);
                    forecast.Temp = string.Format("{0:0.0}°", item.Temp);
                    forecast.DMaxTemp = item.TempMax;
                    forecast.DMinTemp = item.TempMin;
                    forecast.DateTime = item.Date.Date;
                    weather.Forecasts.Add(forecast);

                    if (forecastDaily.Success)
                    {
                        var detailForecast = new ViewModel.WeatherForecast();
                        detailForecast.Update(forecast);

                        if (detailForecast.DateTime.Day == now.Day)
                            detailForecast.Day = "Heute";
                        else
                            detailForecast.Day = forecast.DateTime.ToString("dddd");

                        var dtBegin = detailForecast.DateTime.Date;
                        var dtEnd = dtBegin.AddDays(1);
                        foreach (var itemdetail in forecastDetail.Items.Where(A => A.Date >= dtBegin && A.Date < dtEnd))
                        {
                            ViewModel.WeatherForecastDetail detail = new ViewModel.WeatherForecastDetail();
                            detail.DateTime = itemdetail.Date;
                            detail.Time = itemdetail.Date.ToString("HH:mm");
                            detail.Icon = WIWeather.IconTable[itemdetail.Icon];
                            detail.Temp = string.Format("{0:0.0}°", itemdetail.Temp);
                            // TODO: MORE WEATHER INFO??

                            detailForecast.Details.Add(detail);
                        }

                        weather.ForecastDetails.Add(detailForecast);
                    }
                }

                if (weather.Forecasts.Count > 0)
                    weather.Forecasts.RemoveAt(0);
            }

            return weather;
        }
    }
}
