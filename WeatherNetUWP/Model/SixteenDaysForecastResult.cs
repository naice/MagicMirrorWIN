#region

using System;
using WeatherNet.Model;

#endregion

namespace WeatherNet.Model
{
    /// <summary>
    ///     SixteenDaysForecastResult forecast result type.
    /// </summary>
    public class SixteenDaysForecastResult : WeatherResult
    {
        /// <summary>
        ///     Temperature in the morning in Kelvin.
        /// </summary>
        public Double TempMorning { get; set; }

        /// <summary>
        ///     Temperature in the evening in Kelvin.
        /// </summary>
        public Double TempEvening { get; set; }

        /// <summary>
        ///     Temperature at night in Kelvin.
        /// </summary>
        public Double TempNight { get; set; }

        /// <summary>
        ///     Atmospheric pressure in hPa.
        /// </summary>
        public Double Pressure { get; set; }

        /// <summary>
        ///     Precipitation volume mm per 3 hours.
        /// </summary>
        public Double Rain { get; set; }

        /// <summary>
        ///     Time of data receiving in unixtime GMT.
        /// </summary>
        public int DateUnixFormat { get; set; }

        /// <summary>
        ///     Cloudiness in %
        /// </summary>
        public Double Clouds { get; set; }
    }
}