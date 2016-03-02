#region

using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using WeatherNet.Model;

#endregion

namespace WeatherNet.Util.Data
{
    internal class Deserializer
    {

        private static DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        private static DateTime unixEpochLocal = unixEpoch.ToLocalTime();

        public static DateTime TimestampToDateTime(long unixTimeStamp, bool useLocal = false)
        {
            return useLocal ? unixEpochLocal.AddSeconds(unixTimeStamp) : unixEpoch.AddSeconds(unixTimeStamp);
        }

        public static SingleResult<CurrentWeatherResult> GetWeatherCurrent(JObject response)
        {
            var error = GetServerErrorFromResponse(response);
            if (!String.IsNullOrEmpty(error))
                return new SingleResult<CurrentWeatherResult>(null, false, error);

            var weatherCurrent = new CurrentWeatherResult();

            if (response["sys"] != null)
            {
                weatherCurrent.Country = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(Convert.ToString(response["sys"]["country"])));
                weatherCurrent.Sunrise = TimestampToDateTime(Convert.ToInt64(Convert.ToString(response["sys"]["sunrise"])), true);
                weatherCurrent.Sunset = TimestampToDateTime(Convert.ToInt64(Convert.ToString(response["sys"]["sunset"])), true);
            }

            if (response["weather"] != null)
            {
                weatherCurrent.Title = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(Convert.ToString(response["weather"][0]["main"])));
                weatherCurrent.Description = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(Convert.ToString(response["weather"][0]["description"])));
                weatherCurrent.Icon = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(Convert.ToString(response["weather"][0]["icon"])));
            }

            if (response["main"] != null)
            {
                weatherCurrent.Temp = Convert.ToDouble(response["main"]["temp"].Value<double>());
                weatherCurrent.TempMax = Convert.ToDouble(response["main"]["temp_max"].Value<double>());
                try
                {
                    weatherCurrent.TempMin = Convert.ToDouble(response["main"]["temp_min"].Value<double>());
                }
                catch
                {
                    weatherCurrent.TempMin = 0;
                }
                weatherCurrent.Humidity = Convert.ToDouble(response["main"]["humidity"].Value<double>());
            }

            if (response["wind"] != null)
            {
                weatherCurrent.WindSpeed = Convert.ToDouble(response["wind"]["speed"].Value<double>());
            }

            weatherCurrent.Date = DateTime.UtcNow;
            weatherCurrent.City = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(Convert.ToString(response["name"])));
            weatherCurrent.CityId = Convert.ToInt32(response["id"].Value<Int32>());

            return new SingleResult<CurrentWeatherResult>(weatherCurrent, true, TimeHelper.MessageSuccess);
        }

        public static Result<FiveDaysForecastResult> GetWeatherForecast(JObject response)
        {
            var error = GetServerErrorFromResponse(response);
            if (!String.IsNullOrEmpty(error))
                return new Result<FiveDaysForecastResult>(null, false, error);


            var weatherForecasts = new List<FiveDaysForecastResult>();

            var responseItems = JArray.Parse(response["list"].ToString());
            foreach (var item in responseItems)
            {
                var weatherForecast = new FiveDaysForecastResult();
                if (response["city"] != null)
                {
                    weatherForecast.City = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(Convert.ToString(response["city"]["name"])));
                    weatherForecast.Country = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(Convert.ToString(response["city"]["country"])));
                    weatherForecast.CityId = Convert.ToInt32(response["city"]["id"].Value<Int32>());
                }

                if (item["weather"] != null)
                {
                    weatherForecast.Title = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(Convert.ToString(item["weather"][0]["main"])));
                    weatherForecast.Description = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(Convert.ToString(item["weather"][0]["description"])));
                    weatherForecast.Icon = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(Convert.ToString(item["weather"][0]["icon"])));
                }
                if (item["temp"] != null)
                {
                    weatherForecast.TempMax = Convert.ToDouble(item["temp"]["max"].Value<double>());
                    weatherForecast.TempMin = Convert.ToDouble(item["temp"]["min"].Value<double>());
                }

                if (item["main"] != null)
                {
                    weatherForecast.Temp = Convert.ToDouble(item["main"]["temp"].Value<double>());
                    weatherForecast.TempMax = Convert.ToDouble(item["main"]["temp_max"].Value<double>());
                    weatherForecast.TempMin = Convert.ToDouble(item["main"]["temp_min"].Value<double>());
                    weatherForecast.Humidity = Convert.ToDouble(item["main"]["humidity"].Value<double>());
                }

                if (item["wind"] != null)
                {
                    weatherForecast.WindSpeed = Convert.ToDouble(item["wind"]["speed"].Value<double>());
                }

                if (item["clouds"] != null)
                {
                    try { weatherForecast.Clouds = Convert.ToDouble(item["clouds"]["all"].Value<double>()); }
                    catch {
                        weatherForecast.Clouds = Convert.ToDouble(item["clouds"].Value<double>());
                    }
                }
                weatherForecast.DateUnixFormat = Convert.ToInt64(item["dt"].Value<Int64>());
                weatherForecast.Date = TimestampToDateTime(weatherForecast.DateUnixFormat, true);

                weatherForecasts.Add(weatherForecast);
            }

            return new Result<FiveDaysForecastResult>(weatherForecasts, true, TimeHelper.MessageSuccess);
        }

        public static Result<SixteenDaysForecastResult> GetWeatherDaily(JObject response)
        {
            var error = GetServerErrorFromResponse(response);
            if (!String.IsNullOrEmpty(error))
                return new Result<SixteenDaysForecastResult>(null, false, error);

            var weatherDailies = new List<SixteenDaysForecastResult>();

            var responseItems = JArray.Parse(response["list"].ToString());
            foreach (var item in responseItems)
            {
                var weatherDaily = new SixteenDaysForecastResult();
                if (response["city"] != null)
                {
                    weatherDaily.City = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(Convert.ToString(response["city"]["name"])));
                    weatherDaily.Country = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(Convert.ToString(response["city"]["country"])));
                    weatherDaily.CityId = Convert.ToInt32(response["city"]["id"].Value<Int32>());
                }
                if (item["weather"] != null)
                {
                    weatherDaily.Title = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(Convert.ToString(item["weather"][0]["main"])));
                    weatherDaily.Description = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(Convert.ToString(item["weather"][0]["description"])));
                    weatherDaily.Icon = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(Convert.ToString(item["weather"][0]["icon"])));
                }
                if (item["temp"] != null)
                {
                    weatherDaily.Temp = Convert.ToDouble(item["temp"]["day"].Value<double>());
                    weatherDaily.TempMax = Convert.ToDouble(item["temp"]["max"].Value<double>());
                    weatherDaily.TempMin = Convert.ToDouble(item["temp"]["min"].Value<double>());
                    weatherDaily.TempMorning = Convert.ToDouble(item["temp"]["morn"].Value<double>());
                    weatherDaily.TempEvening = Convert.ToDouble(item["temp"]["eve"].Value<double>());

                    weatherDaily.TempNight = Convert.ToDouble(item["temp"]["night"].Value<double>());
                }

                weatherDaily.Humidity = GetDoubleValue(item, "humidity");
                weatherDaily.WindSpeed = GetDoubleValue(item, "speed");
                weatherDaily.Clouds = GetDoubleValue(item, "clouds");
                weatherDaily.Pressure = GetDoubleValue(item, "pressure");
                weatherDaily.Rain = GetDoubleValue(item, "rain");
                weatherDaily.DateUnixFormat = Convert.ToInt32(item["dt"].Value<Int32>());
                weatherDaily.Date = TimeHelper.ToDateTime(weatherDaily.DateUnixFormat);

                weatherDailies.Add(weatherDaily);
            }

            return new Result<SixteenDaysForecastResult>(weatherDailies, true, TimeHelper.MessageSuccess);
        }

        private static double GetDoubleValue(JToken item,string name)
        {
            return item[name] != null ? item[name].Value<double>() : 0;
        }

        public static string GetServerErrorFromResponse(JObject response)
        {
            if (response["cod"].ToString() == "200")
                return null;

            var errorMessage = "Server error " + response["cod"];
            if (!String.IsNullOrEmpty(response["message"].ToString()))
                errorMessage += ". " + response["message"];
            return errorMessage;
        }
    }
}
