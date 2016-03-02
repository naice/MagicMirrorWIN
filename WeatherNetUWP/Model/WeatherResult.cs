#region

using System;

#endregion

namespace WeatherNet.Model
{
    /// <summary>
    ///     General weather result type
    /// </summary>
    public abstract class WeatherResult
    {
        /// <summary>
        ///     Time of data receiving in GMT.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        ///     City name.
        /// </summary>
        public String City { get; set; }

        /// <summary>
        ///     Country name.
        /// </summary>
        public String Country { get; set; }

        /// <summary>
        ///     City identifier.
        /// </summary>
        public int CityId { get; set; }

        /// <summary>
        ///     WeatherResult title.
        /// </summary>
        public String Title { get; set; }

        /// <summary>
        ///     WeatherResult description.
        /// </summary>
        public String Description { get; set; }

        /// <summary>
        ///     Temperature in Kelvin.
        /// </summary>
        public Double Temp { get; set; }

        /// <summary>
        ///     Humidity in %
        /// </summary>
        public Double Humidity { get; set; }

        /// <summary>
        ///     Maximum temperature in Kelvin.
        /// </summary>
        public Double TempMax { get; set; }

        /// <summary>
        ///     Minimum temperature in Kelvin.
        /// </summary>
        public Double TempMin { get; set; }

        /// <summary>
        ///     Wind speed in mps.
        /// </summary>
        public Double WindSpeed { get; set; }
        
        /// <summary>
        ///     Icon name.
        /// </summary>
        public String Icon { get; set; }

        public DateTime Sunrise { get; set; }
        public DateTime Sunset { get; set; }
    }
}
