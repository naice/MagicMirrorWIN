#region

using System;
using System.Threading.Tasks;
using WeatherNet.Model;
using WeatherNet.Util.Api;
using WeatherNet.Util.Data;

#endregion

namespace WeatherNet.Clients
{
    public static class SixteenDaysForecast
    {
       
 
        /// <summary>
        ///     Get the daily forecast for a specific city by indicating its 'OpenwWeatherMap' identifier.
        /// </summary>
        /// <param name="id">City 'OpenwWeatherMap' identifier.</param>
        /// <param name="days">Number of days we want the forecast for (from 0 to 14).</param>
        /// <returns>The forecast information.</returns>
        public static async Task<Result<SixteenDaysForecastResult>> GetByCityIdAsync(int id, int days)
        {
            try
            {
                if (0 > days || days > 16)
                    return new Result<SixteenDaysForecastResult>(null, false, "Days must be a value between 1 and 16.");
                if (0 > id)
                    return new Result<SixteenDaysForecastResult>(null, false, "City Id must be a positive number.");
                var response = ApiClient.GetResponse("/forecast/daily?id=" + id + "&cnt=" + days);
                return Deserializer.GetWeatherDaily(await response);
            }
            catch (Exception ex)
            {
                return new Result<SixteenDaysForecastResult> {Items = null, Success = false, Message = ex.Message};
            }
        }

        /// <summary>
        ///     Get the daily forecast for a specific city by indicating its 'OpenwWeatherMap' identifier, language and units
        ///     (metric or imperial)..
        /// </summary>
        /// <param name="id">City 'OpenwWeatherMap' identifier.</param>
        /// <param name="days">Number of days we want the forecast for (from 0 to 14).</param>
        /// <param name="language">
        ///     The language of the information returned (example: English - en, Russian - ru, Italian - it,
        ///     Spanish - sp, Ukrainian - ua, German - de, Portuguese - pt, Romanian - ro, Polish - pl, Finnish - fi, Dutch - nl,
        ///     French - fr, Bulgarian - bg, Swedish - se, Chinese Traditional - zh_tw, Chinese Simplified - zh_cn, Turkish - tr ,
        ///     Czech - cz, Galician - gl, Vietnamese - vi, Arabic - ar, Macedonian - mk, Slovak - sk).
        /// </param>
        /// <param name="units">The units of the date (metric or imperial).</param>
        /// <returns>The forecast information.</returns>
        public static async Task<Result<SixteenDaysForecastResult>> GetByCityIdAsync(int id, int days, String language, String units)
        {
            try
            {
                if (0 > days || days > 16)
                    return new Result<SixteenDaysForecastResult>(null, false, "Days must be a value between 1 and 16.");
                if (0 > id)
                    return new Result<SixteenDaysForecastResult>(null, false, "City Id must be a positive number.");
                var response =
                    ApiClient.GetResponse("/forecast/daily?id=" + id + "&cnt=" + days + "&lang=" + language + "&units=" +
                                          units);
                return Deserializer.GetWeatherDaily(await response);
            }
            catch (Exception ex)
            {
                return new Result<SixteenDaysForecastResult> {Items = null, Success = false, Message = ex.Message};
            }
        }

        /// <summary>
        ///     Get the daily forecast for a specific city by indicating the city and country names.
        /// </summary>
        /// <param name="city">Name of the city.</param>
        /// <param name="country">Country of the city.</param>
        /// <param name="days">Number of days we want the forecast for (from 0 to 14).</param>
        /// <returns>The forecast information.</returns>
        public static async Task<Result<SixteenDaysForecastResult>> GetByCityNameAsync(String city, String country, int days)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(city) || String.IsNullOrEmpty(country))
                    return new Result<SixteenDaysForecastResult>(null, false, "City and/or Country cannot be empty.");
                if (0 > days || days > 16)
                    return new Result<SixteenDaysForecastResult>(null, false, "Days must be a value between 1 and 16.");

                var response =
                    ApiClient.GetResponse("/forecast/daily?q=" + city + "," + country + "&cnt=" + days);
                return Deserializer.GetWeatherDaily(await response);
            }
            catch (Exception ex)
            {
                return new Result<SixteenDaysForecastResult> {Items = null, Success = false, Message = ex.Message};
            }
        }

        /// <summary>
        ///     Get the daily forecast for a specific city by indicating the city, country, language and units (metric or
        ///     imperial).
        /// </summary>
        /// <param name="city">Name of the city.</param>
        /// <param name="country">Country of the city.</param>
        /// <param name="days">Number of days we want the forecast for (from 0 to 14).</param>
        /// <param name="language">
        ///     The language of the information returned (example: English - en, Russian - ru, Italian - it,
        ///     Spanish - sp, Ukrainian - ua, German - de, Portuguese - pt, Romanian - ro, Polish - pl, Finnish - fi, Dutch - nl,
        ///     French - fr, Bulgarian - bg, Swedish - se, Chinese Traditional - zh_tw, Chinese Simplified - zh_cn, Turkish - tr ,
        ///     Czech - cz, Galician - gl, Vietnamese - vi, Arabic - ar, Macedonian - mk, Slovak - sk).
        /// </param>
        /// <param name="units">The units of the date (metric or imperial).</param>
        /// <returns>The forecast information.</returns>
        public static async Task<Result<SixteenDaysForecastResult>> GetByCityNameAsync(String city, String country, int days, String language,
            String units)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(city) || String.IsNullOrEmpty(country))
                    return new Result<SixteenDaysForecastResult>(null, false, "City and/or Country cannot be empty.");
                if (0 > days || days > 16)
                    return new Result<SixteenDaysForecastResult>(null, false, "Days must be a value between 1 and 16.");

                var response =
                    ApiClient.GetResponse("/forecast/daily?q=" + city + "," + country + "&cnt=" + days + "&lang=" +
                                          language + "&units=" + units);
                return Deserializer.GetWeatherDaily(await response);
            }
            catch (Exception ex)
            {
                return new Result<SixteenDaysForecastResult> {Items = null, Success = false, Message = ex.Message};
            }
        }

        /// <summary>
        ///     Get the daily forecast for a specific city by indicating its coordinates.
        /// </summary>
        /// <param name="lat">Latitud of the city.</param>
        /// <param name="lon">Longitude of the city.</param>
        /// <param name="days">Number of days we want the forecast for (from 0 to 14).</param>
        /// <returns>The forecast information.</returns>
        public static async Task<Result<SixteenDaysForecastResult>> GetByCoordinatesAsync(double lat, double lon, int days)
        {
            try
            {
                if (0 > days || days > 16)
                    return new Result<SixteenDaysForecastResult>(null, false, "Days must be a value between 1 and 16.");
                var response =
                    ApiClient.GetResponse("/forecast/daily?lat=" + lat + "&lon=" + lon + "&cnt=" + days);
                return Deserializer.GetWeatherDaily(await response);
            }
            catch (Exception ex)
            {
                return new Result<SixteenDaysForecastResult> {Items = null, Success = false, Message = ex.Message};
            }
        }

        /// <summary>
        ///     Get the daily forecast for a specific city by indicating its coordinates, language and units (metric or imperial).
        /// </summary>
        /// <param name="lat">Latitud of the city.</param>
        /// <param name="lon">Longitude of the city.</param>
        /// <param name="days">Number of days we want the forecast for (from 0 to 14).</param>
        /// <param name="language">
        ///     The language of the information returned (example: English - en, Russian - ru, Italian - it,
        ///     Spanish - sp, Ukrainian - ua, German - de, Portuguese - pt, Romanian - ro, Polish - pl, Finnish - fi, Dutch - nl,
        ///     French - fr, Bulgarian - bg, Swedish - se, Chinese Traditional - zh_tw, Chinese Simplified - zh_cn, Turkish - tr ,
        ///     Czech - cz, Galician - gl, Vietnamese - vi, Arabic - ar, Macedonian - mk, Slovak - sk).
        /// </param>
        /// <param name="units">The units of the date (metric or imperial).</param>
        /// <returns>The forecast information.</returns>
        public static async Task<Result<SixteenDaysForecastResult>> GetByCoordinatesAsync(double lat, double lon, int days, String language,
            String units)
        {
            try
            {
                if (0 > days || days > 16)
                    return new Result<SixteenDaysForecastResult>(null, false, "Days must be a value between 1 and 16.");
                var response =
                    ApiClient.GetResponse("/forecast/daily?lat=" + lat + "&lon=" + lon + "&cnt=" + days + "&lang=" +
                                          language + "&units=" + units);
                return Deserializer.GetWeatherDaily(await response);
            }
            catch (Exception ex)
            {
                return new Result<SixteenDaysForecastResult> {Items = null, Success = false, Message = ex.Message};
            }
        }
    }
}