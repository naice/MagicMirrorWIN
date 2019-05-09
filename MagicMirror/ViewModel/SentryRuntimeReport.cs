using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MagicMirror.Configuration;
using Newtonsoft.Json;
using Sentry;
using Windows.System;

namespace MagicMirror.ViewModel
{
    internal class SentryRuntimeReport : IUpdateViewModel
    {
        public TimeSpan UpdateTimeout { get; set; } = TimeSpan.FromMinutes(1);
        public DateTime LastUpdate { get; set; } = DateTime.MinValue;
        public SemaphoreSlim UILock { get; set; } = new SemaphoreSlim(1, 1);

        public async Task<object> ProcessData(Configuration.Configuration config)
        {
            await PostToInfluxDebug(new Dictionary<string, decimal>() {
                { "AppMemoryUsage", (decimal)MemoryManager.AppMemoryUsage / 1024 / 1024 },
                { "AppMemoryUsageLimit", (decimal)MemoryManager.AppMemoryUsageLimit / 1024 / 1024 },
                { "ExpectedAppMemoryUsageLimit", (decimal)MemoryManager.ExpectedAppMemoryUsageLimit / 1024 / 1024 }
            });

            return null;
        }

        private async Task PostToInfluxDebug(Dictionary<string, decimal> measurements)
        {
            try
            {
                using (var client = new WebClient())
                {
                    client.Credentials = new NetworkCredential("debugUser", "jensm1985");
                    StringBuilder sb = new StringBuilder();
                    foreach (var item in measurements)
                    {
                        sb.Append($"{item.Key} value={Math.Round(item.Value, 2).ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)}");
                        sb.Append("\n");
                    }
                    var str = sb.ToString();
                    var reponse = await client.UploadStringTaskAsync("http://192.168.2.5:8086/write?db=Debug", str);                    
                }
            }
            catch (WebException ex)
            {
               Sentry.SentrySdk.CaptureException(ex);
            }
        }

        public void UpdateUI(Configuration.Configuration config, object data)
        {
            // NOOP
        }
    }
}