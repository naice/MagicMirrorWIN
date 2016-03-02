#region

using System;
using System.Diagnostics;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Threading.Tasks;

#endregion

namespace WeatherNet.Util.Api
{
    public static class ApiClient
    {
        
        
        public static async Task<JObject> GetResponse(String queryString)
        {
            using (var client = new HttpClient())
            {
                var apiKey = ClientSettings.ApiKey;
                var apiUrl = ClientSettings.ApiUrl;
                string url;
                if (!string.IsNullOrEmpty(apiKey))
                    url = apiUrl + queryString + "&APPID=" + apiKey;
                else
                    url = apiUrl + queryString;

                var response = await client.GetStringAsync(url);
                var parsedResponse = JObject.Parse(response);
                return parsedResponse;
            }
        }
    }
}