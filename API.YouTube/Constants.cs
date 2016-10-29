using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.YouTube
{
    public static class Constants
    {
        internal const string PROTOCOL = "https://";
        internal const string APIURL = PROTOCOL + "www.googleapis.com/youtube/v3/";

        /// <summary>
        /// The Api key, needs to be set before usage.
        /// </summary>
        public static string APIKey { get; set; } = string.Empty;
    }
}
