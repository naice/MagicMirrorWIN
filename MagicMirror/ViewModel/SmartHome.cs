using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MagicMirror.Configuration;
using Newtonsoft.Json;

namespace MagicMirror.ViewModel
{
    public class TemperatureItem
    {
        public string Name { get; set; }
        public string Temperature { get; set; }
    }

    public class SmartHome : BaseViewModel, IUpdateViewModel
    {
        private TemperatureItem[] _TemperatureItems;
        public TemperatureItem[] TemperatureItems
        {
            get { return _TemperatureItems; }
            set
            {
                if (value != _TemperatureItems)
                {
                    _TemperatureItems = value;
                    RaisePropertyChanged("TemperatureItems");
                }
            }
        }

        public TimeSpan UpdateTimeout { get; set; } = TimeSpan.FromMinutes(1);
        public DateTime LastUpdate { get; set; } = DateTime.MinValue;
        public SemaphoreSlim UILock { get; set; } = new SemaphoreSlim(1, 1);

        public async Task<object> Update(Configuration.Configuration config)
        {
            List<TemperatureItem> items = new List<TemperatureItem>();

            var item = await GetFromInflux("Aquarium", @"db=grafana_home&q=SELECT mean(""value"") FROM ""temperature2"" WHERE (""site_name"" = 'Aquarium.Temp') AND time >= now() - 30m GROUP BY time(5m) fill(null)");
            if (item != null) items.Add(item);
            item = await GetFromInflux("Lounge", @"db=grafana_home&q=SELECT mean(""value"") FROM ""temperature"" WHERE (""site_name"" = 'Lounge.Temp') AND time >= now() - 30m GROUP BY time(5m) fill(null)");
            if (item != null) items.Add(item);
            item = await GetFromInflux("Wohnzimmer", @"db=grafana_home&q=SELECT mean(""value"") FROM ""temperature"" WHERE (""site_name"" = 'Wohnzimmer.Temp') AND time >= now() - 30m GROUP BY time(5m) fill(null)");
            if (item != null) items.Add(item);


            return items.ToArray();
        }

        private async Task<TemperatureItem> GetFromInflux(string name, string query)
        {
            try
            {
                using (var client = new WebClient())
                {
                    //client.Credentials = new NetworkCredential("grafana_home", "jensm1985");
                    var queryString = Uri.EscapeUriString(query);
                    var queryUrl = "http://192.168.2.5:8086/query?" + queryString;
                    var responseStr = await client.DownloadStringTaskAsync(new Uri(queryUrl));
                    var response = JsonConvert.DeserializeObject<InfluxDbResponse>(responseStr);

                    return new TemperatureItem() { Name = name, Temperature = $"{response?.Results[0]?.Series[0]?.Values[0]?[1]:0.0}°" };
                }
            }
            catch (Exception ex)
            {
                Sentry.SentrySdk.CaptureException(ex);
                return null;
            }
        }

        public void UpdateUI(Configuration.Configuration config, object data)
        {
            if (!(data is TemperatureItem[] tempData))
                return;

            TemperatureItems = tempData;
        }
    }

    public class InfluxDbResponse
    {
        public Result[] Results { get; set; }
    }

    public class Result
    {
        public Series[] Series { get; set; }
    }

    public class Series
    {
        public string Name { get; set; }
        public string[] Columns { get; set; }
        public object[][] Values { get; set; }
    }
}
